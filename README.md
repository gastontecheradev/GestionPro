# GestionPro — Business Management System

Management system for wholesale distribution built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **SQLite**.

🔗 **Live site:** [GestionPro](https://gestionpro.somee.com/)

## Features

- **Customer Management** — Full CRUD with server-side search and pagination
- **Products & Stock** — Inventory control with low stock alerts
- **Purchase Orders** — Multiple detail lines, statuses, and automatic VAT calculation
- **Invoicing** — Generated from approved orders with sequential numbering
- **Dashboard** — Real-time metrics: sales, stock, pending orders
- **Authentication & Roles** — Admin, Seller, and Viewer with ASP.NET Identity
- **Audit Log** — Automatic change tracking (who, what, when)
- **Soft Delete** — Records are deactivated instead of deleted

## Tech Stack

| Layer | Technology |
|-------|-----------|
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
├── Controllers/          ← Receive requests, delegate to service layer
├── Models/
│   ├── Entities/         ← Classes mapped to database tables
│   ├── ViewModels/       ← DTOs for views
│   └── Enums/
├── Services/
│   ├── Interfaces/       ← Contracts
│   └── Implementations/  ← Business logic
├── Data/
│   ├── AppDbContext.cs   ← DbContext with soft delete and auditing
│   └── SeedData.cs       ← Initial seed data
├── Views/                ← Razor Views
└── Program.cs            ← App configuration
```

## Patterns & Practices

- **Service Layer Pattern** — Business logic separated from controllers
- **Soft Delete with Global Query Filters** — Automatic filtering on every query
- **Automatic Auditing** — SaveChanges override for change tracking
- **Code First Migrations** — Database versioned alongside the codebase

## Author

**Gastón Techera** — .NET Developer  
[Portfolio](https://gastontecheradev.github.io/portfolio-react) · [LinkedIn](https://www.linkedin.com/in/gaston-techera-dev/) · [GitHub](https://github.com/gastontecheradev)
