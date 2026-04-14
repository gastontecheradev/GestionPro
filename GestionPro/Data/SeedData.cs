using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestionPro.Models.Entities;
using GestionPro.Models.Enums;

namespace GestionPro.Data
{
    /// Inicializa la base de datos con roles, usuario administrador,
    /// y datos de ejemplo para poder probar la aplicación desde el primer momento.
    public static class SeedData
    {
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Asegurar que la DB existe y aplicar migraciones pendientes
            await context.Database.MigrateAsync();

            // ── 1. Crear Roles ──
            string[] roles = { "Admin", "Vendedor", "Viewer" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // ── 2. Crear Usuario Admin ──
            // Las credenciales se leen de User Secrets (desarrollo) o variables de entorno (producción)
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@gestionpro.com";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ── 3. Crear Usuario Vendedor de ejemplo ──
            var vendedorEmail = "vendedor@gestionpro.com";
            if (await userManager.FindByEmailAsync(vendedorEmail) == null)
            {
                var vendedor = new IdentityUser
                {
                    UserName = vendedorEmail,
                    Email = vendedorEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(vendedor, "Vendedor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(vendedor, "Vendedor");
                }
            }

            // ── 4. Datos de ejemplo (solo si no existen) ──
            if (await context.Categorias.AnyAsync()) return;

            // Categorías
            var categorias = new List<Categoria>
            {
                new() { Nombre = "Bebidas", Descripcion = "Agua, refrescos, jugos y bebidas alcohólicas", CreadoPor = adminEmail },
                new() { Nombre = "Alimentos Secos", Descripcion = "Arroz, pastas, harinas y cereales", CreadoPor = adminEmail },
                new() { Nombre = "Lácteos", Descripcion = "Leche, quesos, yogures y manteca", CreadoPor = adminEmail },
                new() { Nombre = "Limpieza", Descripcion = "Productos de limpieza e higiene", CreadoPor = adminEmail },
                new() { Nombre = "Congelados", Descripcion = "Productos congelados y helados", CreadoPor = adminEmail },
            };
            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();

            // Productos
            var bebidas = categorias[0];
            var secos = categorias[1];
            var lacteos = categorias[2];
            var limpieza = categorias[3];
            var congelados = categorias[4];

            var productos = new List<Producto>
            {
                // Bebidas
                new() { Nombre = "Agua Mineral Salus 1.5L", Codigo = "BEB-001", Precio = 45.00m, Stock = 200, StockMinimo = 20, CategoriaId = bebidas.Id, CreadoPor = adminEmail },
                new() { Nombre = "Coca-Cola 2.25L", Codigo = "BEB-002", Precio = 89.00m, Stock = 150, StockMinimo = 15, CategoriaId = bebidas.Id, CreadoPor = adminEmail },
                new() { Nombre = "Jugo Citric Naranja 1L", Codigo = "BEB-003", Precio = 72.00m, Stock = 80, StockMinimo = 10, CategoriaId = bebidas.Id, CreadoPor = adminEmail },
                new() { Nombre = "Cerveza Patricia 1L", Codigo = "BEB-004", Precio = 95.00m, Stock = 120, StockMinimo = 15, CategoriaId = bebidas.Id, CreadoPor = adminEmail },

                // Alimentos Secos
                new() { Nombre = "Arroz Blue Patna 1kg", Codigo = "SEC-001", Precio = 62.00m, Stock = 300, StockMinimo = 30, CategoriaId = secos.Id, CreadoPor = adminEmail },
                new() { Nombre = "Fideos Adria Tallarin 500g", Codigo = "SEC-002", Precio = 48.00m, Stock = 250, StockMinimo = 25, CategoriaId = secos.Id, CreadoPor = adminEmail },
                new() { Nombre = "Harina Cañuelas 0000 1kg", Codigo = "SEC-003", Precio = 38.00m, Stock = 180, StockMinimo = 20, CategoriaId = secos.Id, CreadoPor = adminEmail },

                // Lácteos
                new() { Nombre = "Leche Conaprole Entera 1L", Codigo = "LAC-001", Precio = 42.00m, Stock = 100, StockMinimo = 15, CategoriaId = lacteos.Id, CreadoPor = adminEmail },
                new() { Nombre = "Queso Colonia Conaprole 1kg", Codigo = "LAC-002", Precio = 320.00m, Stock = 40, StockMinimo = 5, CategoriaId = lacteos.Id, CreadoPor = adminEmail },

                // Limpieza
                new() { Nombre = "Lavandina Agua Jane 1L", Codigo = "LIM-001", Precio = 55.00m, Stock = 90, StockMinimo = 10, CategoriaId = limpieza.Id, CreadoPor = adminEmail },
                new() { Nombre = "Detergente Nevex 1.2L", Codigo = "LIM-002", Precio = 110.00m, Stock = 70, StockMinimo = 8, CategoriaId = limpieza.Id, CreadoPor = adminEmail },

                // Congelados
                new() { Nombre = "Milanesas de Pollo Schneck 8u", Codigo = "CON-001", Precio = 185.00m, Stock = 3, StockMinimo = 5, CategoriaId = congelados.Id, CreadoPor = adminEmail }, // Stock bajo a propósito
                new() { Nombre = "Helado Crufi Dulce de Leche 1L", Codigo = "CON-002", Precio = 210.00m, Stock = 2, StockMinimo = 5, CategoriaId = congelados.Id, CreadoPor = adminEmail }, // Stock bajo a propósito
            };
            context.Productos.AddRange(productos);
            await context.SaveChangesAsync();

            // Clientes
            var clientes = new List<Cliente>
            {
                new()
                {
                    RazonSocial = "Supermercado El Dorado S.A.",
                    RUT = "210123456789",
                    Direccion = "Av. 18 de Julio 1234, Montevideo",
                    Telefono = "2908-1234",
                    Email = "compras@eldorado.com.uy",
                    Contacto = "María González",
                    CreadoPor = adminEmail
                },
                new()
                {
                    RazonSocial = "Almacén Don Pepe",
                    RUT = "211234567890",
                    Direccion = "José Batlle y Ordóñez 567, Canelones",
                    Telefono = "2699-5678",
                    Email = "donpepe@gmail.com",
                    Contacto = "José Pérez",
                    CreadoPor = adminEmail
                },
                new()
                {
                    RazonSocial = "Distribuidora Costa SRL",
                    RUT = "212345678901",
                    Direccion = "Ruta 1 km 23.5, Ciudad de la Costa",
                    Telefono = "2682-9012",
                    Email = "info@discosta.com.uy",
                    Contacto = "Andrés Costa",
                    CreadoPor = adminEmail
                },
                new()
                {
                    RazonSocial = "Minimarket Las Piedras",
                    RUT = "213456789012",
                    Direccion = "Artigas 890, Las Piedras",
                    Telefono = "2364-3456",
                    Email = "minimarketlp@hotmail.com",
                    Contacto = "Laura Martínez",
                    CreadoPor = adminEmail
                },
            };
            context.Clientes.AddRange(clientes);
            await context.SaveChangesAsync();

            // Órdenes de ejemplo
            var orden1 = new Orden
            {
                NumeroOrden = "ORD-2025-0001",
                FechaOrden = DateTime.UtcNow.AddDays(-10),
                Estado = EstadoOrden.Entregada,
                ClienteId = clientes[0].Id,
                PorcentajeIVA = 22,
                CreadoPor = adminEmail,
                Detalles = new List<OrdenDetalle>
                {
                    new() { ProductoId = productos[0].Id, Cantidad = 50, PrecioUnitario = 45.00m, Subtotal = 2250.00m },
                    new() { ProductoId = productos[4].Id, Cantidad = 30, PrecioUnitario = 62.00m, Subtotal = 1860.00m },
                    new() { ProductoId = productos[7].Id, Cantidad = 20, PrecioUnitario = 42.00m, Subtotal = 840.00m },
                }
            };
            orden1.Subtotal = 2250.00m + 1860.00m + 840.00m; // 4950
            orden1.MontoIVA = orden1.Subtotal * 0.22m;         // 1089
            orden1.Total = orden1.Subtotal + orden1.MontoIVA;   // 6039

            var orden2 = new Orden
            {
                NumeroOrden = "ORD-2025-0002",
                FechaOrden = DateTime.UtcNow.AddDays(-3),
                Estado = EstadoOrden.Aprobada,
                ClienteId = clientes[1].Id,
                PorcentajeIVA = 22,
                CreadoPor = adminEmail,
                Detalles = new List<OrdenDetalle>
                {
                    new() { ProductoId = productos[1].Id, Cantidad = 24, PrecioUnitario = 89.00m, Subtotal = 2136.00m },
                    new() { ProductoId = productos[5].Id, Cantidad = 50, PrecioUnitario = 48.00m, Subtotal = 2400.00m },
                }
            };
            orden2.Subtotal = 2136.00m + 2400.00m;             // 4536
            orden2.MontoIVA = orden2.Subtotal * 0.22m;          // 997.92
            orden2.Total = orden2.Subtotal + orden2.MontoIVA;    // 5533.92

            var orden3 = new Orden
            {
                NumeroOrden = "ORD-2025-0003",
                FechaOrden = DateTime.UtcNow,
                Estado = EstadoOrden.Pendiente,
                ClienteId = clientes[2].Id,
                PorcentajeIVA = 22,
                CreadoPor = adminEmail,
                Detalles = new List<OrdenDetalle>
                {
                    new() { ProductoId = productos[8].Id, Cantidad = 10, PrecioUnitario = 320.00m, Subtotal = 3200.00m },
                    new() { ProductoId = productos[9].Id, Cantidad = 15, PrecioUnitario = 55.00m, Subtotal = 825.00m },
                    new() { ProductoId = productos[10].Id, Cantidad = 12, PrecioUnitario = 110.00m, Subtotal = 1320.00m },
                }
            };
            orden3.Subtotal = 3200.00m + 825.00m + 1320.00m;   // 5345
            orden3.MontoIVA = orden3.Subtotal * 0.22m;           // 1175.90
            orden3.Total = orden3.Subtotal + orden3.MontoIVA;     // 6520.90

            context.Ordenes.AddRange(orden1, orden2, orden3);
            await context.SaveChangesAsync();

            // Factura para la orden entregada
            var factura = new Factura
            {
                NumeroFactura = "FAC-2025-0001",
                FechaEmision = orden1.FechaOrden.AddDays(1),
                MontoTotal = orden1.Total,
                Pagada = true,
                FechaPago = orden1.FechaOrden.AddDays(15),
                OrdenId = orden1.Id,
                CreadoPor = adminEmail
            };
            context.Facturas.Add(factura);
            await context.SaveChangesAsync();
        }
    }
}
