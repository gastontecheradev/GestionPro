# GestionPro — Business Management System

Management system for wholesale distribution built with **ASP.NET Core 8 MVC**, **Entity Framework Core** and **SQLite**.

🔗 **Live site:** [GestionPro](https://gestionpro.somee.com/)

## Features

* **Customer Management** — Full CRUD with search and server-side pagination
* **Products & Stock** — Inventory control with low-stock alerts
* **Purchase Orders** — Multiple line items, status tracking and automatic VAT calculation
* **Invoicing** — Generated from approved orders with sequential numbering
* **Dashboard** — Real-time metrics: sales, stock, pending orders
* **Authentication & Roles** — Admin, Salesperson and Viewer with ASP.NET Identity
* **Audit Log** — Automatic change tracking (who, what, when)
* **Soft Delete** — Records are deactivated instead of being permanently removed

## Tech Stack

| Layer | Technology |
| --- | --- |
| Framework | ASP.NET Core 8 MVC |
| Language | C# 12 |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQLite |
| Authentication | ASP.NET Identity with Roles |
| Frontend | Razor Views + Bootstrap 5 |
| Validation | Data Annotations + FluentValidation |
| Pagination | X.PagedList |
| Mapping | AutoMapper |

## Architecture

```
GestionPro/
├── Controllers/          ← Receive requests, delegate to services
├── Models/
│   ├── Entities/         ← Classes mapped to database tables
│   ├── ViewModels/       ← DTOs for views
│   └── Enums/
├── Services/
│   ├── Interfaces/       ← Service contracts
│   └── Implementations/  ← Business logic
├── Data/
│   ├── AppDbContext.cs   ← DbContext with soft delete and audit
│   └── SeedData.cs       ← Initial seed data
├── Views/                ← Razor Views
└── Program.cs            ← Application configuration
```

## Patterns & Practices

* **Service Layer Pattern** — Business logic decoupled from controllers
* **Soft Delete with Global Query Filters** — Automatic filtering on every query
* **Automatic auditing** — SaveChanges override for change tracking
* **Code First Migrations** — Database schema versioned alongside the code

## Author

**Gastón Techera** — .NET Developer
[Portfolio](https://gastontecheradev.github.io/portfolio) · [LinkedIn](https://www.linkedin.com/in/gaston-techera-dev/) · [GitHub](https://github.com/gastontecheradev)
