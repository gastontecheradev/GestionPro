# GestionPro — Sistema de Gestión Empresarial

Sistema de gestión para distribución mayorista construido con **ASP.NET Core 8 MVC**, **Entity Framework Core** y **SQLite**.

## Funcionalidades

- **Gestión de Clientes** — CRUD completo con búsqueda y paginación server-side
- **Productos y Stock** — Control de inventario con alertas de stock bajo
- **Órdenes de Compra** — Múltiples líneas de detalle, estados y cálculo automático de IVA
- **Facturación** — Generación desde órdenes aprobadas con numeración correlativa
- **Dashboard** — Métricas en tiempo real: ventas, stock, órdenes pendientes
- **Autenticación y Roles** — Admin, Vendedor y Viewer con ASP.NET Identity
- **Auditoría** — Registro automático de cambios (quién, qué, cuándo)
- **Soft Delete** — Los registros se desactivan en vez de borrarse

## Stack Técnico

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Core 8 MVC |
| Lenguaje | C# 12 |
| ORM | Entity Framework Core 8 (Code First) |
| Base de Datos | SQLite |
| Autenticación | ASP.NET Identity con Roles |
| Frontend | Razor Views + Bootstrap 5 |
| Validación | Data Annotations + FluentValidation |
| Paginación | X.PagedList |
| Mapeo | AutoMapper |

## Arquitectura

```
GestionPro/
├── Controllers/          ← Reciben requests, delegan al servicio
├── Models/
│   ├── Entities/         ← Clases que mapean a tablas
│   ├── ViewModels/       ← DTOs para las vistas
│   └── Enums/
├── Services/
│   ├── Interfaces/       ← Contratos
│   └── Implementations/  ← Lógica de negocio
├── Data/
│   ├── AppDbContext.cs   ← DbContext con soft delete y auditoría
│   └── SeedData.cs       ← Datos iniciales
├── Views/                ← Razor Views
└── Program.cs            ← Configuración
```


## Patrones y Prácticas

- **Service Layer Pattern** — Lógica de negocio separada de controllers
- **Soft Delete con Global Query Filters** — Filtrado automático en cada consulta
- **Auditoría automática** — Override de SaveChanges para tracking de cambios
- **Code First Migrations** — La DB se versiona junto con el código

## Autor

**Gastón Techera** — Desarrollador .NET  
[Portfolio](https://gastontecheradev.github.io/portfolio-react) · [LinkedIn](https://www.linkedin.com/in/gaston-techera-dev/) · [GitHub](https://github.com/gastontecheradev)
