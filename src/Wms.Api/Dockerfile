
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Wms.Api/Wms.Api.csproj", "src/Wms.Api/"]
COPY ["src/Wms.Infrastructure/Wms.Infrastructure.csproj", "src/Wms.Infrastructure/"]
COPY ["src/Wms.Application/Wms.Application.csproj", "src/Wms.Application/"]
COPY ["src/Wms.Domain/Wms.Domain.csproj", "src/Wms.Domain/"]
RUN dotnet restore "./src/Wms.Api/Wms.Api.csproj"
COPY . .
WORKDIR "/src/src/Wms.Api"
RUN dotnet build "./Wms.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Wms.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wms.Api.dll"]