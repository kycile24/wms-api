# Warehouse Operations

A bilingual inventory dashboard and ASP.NET Core API for warehouse staff. The dashboard shows current stock alerts, pending purchases, active sales orders and recent movements from the connected SQL Server database. The API also manages products, warehouses, suppliers and customers.

[Open the live warehouse dashboard](https://warehouse-kycile24.onrender.com). Explore seeded inventory and filter shortages by product, SKU or warehouse. The public deployment uses a dedicated read-only SQLite demo. The full authenticated application below uses SQL Server.

## Run locally

Install Docker with Linux containers. Set **WMS_DB_PASSWORD** to a strong SQL Server password and **WMS_JWT_SECRET** to a randomly generated secret of at least 32 bytes. In the repository root, run:

```sh
docker compose up --build
```

Open [the dashboard](http://localhost:8080) and select **English** or **Українська** in the header. For a new local database, create an account through `POST /api/auth/register` in [the API reference](http://localhost:8080/swagger). Sign in to view the inventory. Product and warehouse names appear as entered in the database.

## Architecture

| Component | Responsibility |
| --- | --- |
| Wms.Api | HTTP endpoints, JWT authentication, OpenAPI and the browser dashboard |
| Wms.Application | Inventory workflows, DTOs, validation and reporting |
| Wms.Domain | Products, stock movements, warehouses and order entities |
| Wms.Infrastructure | SQL Server persistence, Entity Framework Core, repositories and authentication services |

## Development

Use the .NET 10 SDK and SQL Server. Supply `ConnectionStrings__DefaultConnection` and `JwtSettings__Secret` through your development environment.

```sh
dotnet build src/Wms.Api/Wms.Api.csproj
dotnet test tests/Wms.UnitTests/Wms.UnitTests.csproj
dotnet run --project src/Wms.Api
```

The web interface calls the authenticated dashboard endpoints. The language selection is saved in the browser. The access token is held in memory for the current page session.
