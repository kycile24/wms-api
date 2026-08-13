<div align="center">

# Warehouse Management System

### Enterprise Warehouse Management Platform built with ASP.NET Core

A backend system for managing warehouse operations, inventory, suppliers, customers, and order processing.  
The project demonstrates how a modern warehouse can automate inventory movement, purchasing, sales, and logistics using a scalable architecture.

---

.NET 9 • ASP.NET Core • PostgreSQL • Entity Framework Core • Docker • JWT • Clean Architecture

</div>

---

# 📖 Project Overview

Warehouse Management System (WMS) is a backend application designed to simplify warehouse operations and inventory management.

Instead of maintaining stock using spreadsheets or disconnected systems, WMS provides a centralized platform where employees can manage products, warehouses, purchases, sales, suppliers, customers, and inventory movements.

The application models the complete lifecycle of goods inside a warehouse:

- products arrive from suppliers;
- inventory is stored in warehouses;
- stock levels are updated automatically;
- sales orders reserve inventory;
- shipments move goods to customers;
- every movement is tracked and auditable.

The project focuses not only on CRUD operations but also on business processes that exist in real warehouse management systems.

---

#  Business Goals

The main objective of the project is to digitize warehouse workflows and provide a reliable API for inventory management.

The system allows companies to:

- maintain an accurate inventory
- reduce manual stock errors
- monitor product availability
- manage suppliers and customers
- process purchase and sales orders
- track warehouse movements
- generate dashboard statistics
- keep a complete audit history

---

# 🏢 Business Workflow

The typical business process inside the system looks like this:

```text
Supplier
      │
      ▼
Purchase Order
      │
      ▼
Warehouse
      │
      ▼
Inventory
      │
      ▼
Sales Order
      │
      ▼
Shipment
      │
      ▼
Customer
```

Every operation automatically updates inventory and creates stock movement records, making warehouse activity fully traceable.

---

#  Architecture

The project follows **Clean Architecture**, where each layer has a single responsibility and depends only on the layers below it.

```text
                Client Applications
                        │
                        ▼
                 ASP.NET Core API
                        │
──────────────────────────────────────────
              Application Layer
──────────────────────────────────────────
Business Services
DTOs
Interfaces
Validation
Business Rules
──────────────────────────────────────────
                Domain Layer
──────────────────────────────────────────
Entities
Enums
Value Objects
Domain Logic
──────────────────────────────────────────
            Infrastructure Layer
──────────────────────────────────────────
Entity Framework Core
Repositories
Authentication
Database
Logging
```

This architecture makes the project easy to extend, maintain, and test.

---

#  Solution Structure

```text
src/

 Wms.Api
 ├── Controllers
 ├── Middlewares
 ├── Configuration
 ├── Program.cs

 Wms.Application
 ├── Services
 ├── Interfaces
 ├── DTOs
 ├── Validators
 ├── Common

 Wms.Domain
 ├── Entities
 ├── Enums
 ├── ValueObjects
 ├── Common

 Wms.Infrastructure
 ├── Persistence
 ├── Repositories
 ├── Identity
 ├── Configurations
 ├── DatabaseSeeder
 ├── Migrations

tests/

 Wms.UnitTests
```

---

#  Core Modules

## Authentication

Responsible for user registration, login, JWT generation and refresh tokens.

---

## Product Management

Maintains product catalog, categories and warehouse availability.

---

## Warehouse Management

Supports multiple warehouses with independent inventory.

---

## Inventory

Tracks current stock quantities and records every inventory change.

---

## Purchase Orders

Handles incoming goods from suppliers and updates warehouse inventory.

---

## Sales Orders

Processes customer orders and reserves available stock.

---

## Shipments

Represents outgoing deliveries and shipment statuses.

---

## Dashboard

Provides warehouse statistics and aggregated business metrics.

---

## Audit Logs

Stores important system events for traceability.

---

#  Security

The application implements modern authentication mechanisms.

- JWT Authentication
- Refresh Tokens
- Password Hashing
- Role-based Authorization
- Global Exception Handling

---

#  Deployment

The project can be started either locally or using Docker.

### Docker

```bash
docker compose up -d --build
```

### Local

```bash
dotnet restore
dotnet ef database update
dotnet run --project src/Wms.Api
```

Swagger documentation becomes available after startup.

---

