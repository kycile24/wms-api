using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Wms.Api.Middlewares;
using Wms.Application.Interfaces;
using Wms.Application.Services;
using Wms.Application.Validators.Auth;
using Wms.Infrastructure.Identity;
using Wms.Infrastructure.Persistence;

// Serilog Config
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Запуск веб-хоста WMS API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add Database Context
    var publicDemo = builder.Configuration.GetValue<bool>("PublicDemo");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (publicDemo)
            options.UseSqlite("Data Source=/tmp/warehouse-demo.db");
        else
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    if (publicDemo)
        builder.Configuration["JwtSettings:Secret"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));

    // Add JwtSettings
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

    // Services Registration
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // Категорія та продукти 
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IProductService, ProductService>();

    // Склади та Залишки
    builder.Services.AddScoped<IWarehouseService, WarehouseService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();

    // Закупівлі
    builder.Services.AddScoped<ISupplierService, SupplierService>();
    builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

    // Продажі та Відвантаження
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();

    // Аналітика та Дашборд
    builder.Services.AddScoped<IDashboardService, DashboardService>();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();

    // JWT Authentication Configuration
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("Секретний ключ 'JwtSettings:Secret' не вказано в appsettings.json.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger Configuration
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "WMS API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    app.UseDefaultFiles();
    app.UseStaticFiles();
    if (!publicDemo)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/workspace-mode", () => new { publicDemo });
    var catalogEndpoint = app.MapGet(publicDemo ? "/demo/catalog" : "/api/dashboard/catalog",
        async (ApplicationDbContext db, CancellationToken ct) => new { data = await db.InventoryItems.AsNoTracking()
            .OrderBy(i => i.Product.Name).ThenBy(i => i.Warehouse.Code)
            .Select(i => new { sku = i.Product.Sku, name = i.Product.Name, category = i.Product.Category.Name,
                warehouse = i.Warehouse.Name, code = i.Warehouse.Code, price = i.Product.UnitPrice,
                available = i.QuantityOnHand - i.QuantityAllocated, threshold = i.Product.MinimumStockThreshold,
                bin = i.Zone + "/" + i.Aisle + "/" + i.Rack + "/" + i.Shelf })
            .ToListAsync(ct) });
    if (!publicDemo) catalogEndpoint.RequireAuthorization();
    if (publicDemo)
    {
        app.MapGet("/demo/summary", async (IDashboardService dashboard, CancellationToken ct) =>
            new { data = (await dashboard.GetSummaryAsync(ct)).Value });
        app.MapGet("/demo/low-stock-alerts", async (IDashboardService dashboard, CancellationToken ct) =>
            new { data = (await dashboard.GetLowStockAlertsAsync(ct)).Value });
    }
    else
    {
        app.MapControllers();
    }

    // Автоматично застосовуємо міграції/створюємо базу та сідимо тестові дані
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            // Автоматично створює структуру БД (якщо ще немає міграцій в коді)
            await dbContext.Database.EnsureCreatedAsync();

            // Якщо ви вже створили файли міграцій через 'dotnet ef migrations add', розкоментуйте рядок нижче:
            // await dbContext.Database.MigrateAsync();

            await DatabaseSeeder.SeedAsync(dbContext);
            Log.Information("Базу даних успішно ініціалізовано та заповнено початковими даними.");
        }
        catch (Exception ex)
        {
            if (publicDemo) throw;
            Log.Error(ex, "Помилка під час ініціалізації бази даних або сідингу.");
        }
    }

  
    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Додаток завершився з критичною помилкою на старті!");
}
finally
{
    Log.CloseAndFlush();
}