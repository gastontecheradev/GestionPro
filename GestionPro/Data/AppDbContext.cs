using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GestionPro.Models.Entities;

namespace GestionPro.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ── DbSets ──
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Orden> Ordenes => Set<Orden>();
        public DbSet<OrdenDetalle> OrdenDetalles => Set<OrdenDetalle>();
        public DbSet<Factura> Facturas => Set<Factura>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Requerido por Identity

            // ── Categoria ──
            builder.Entity<Categoria>(e =>
            {
                e.HasIndex(c => c.Nombre).IsUnique();
            });

            // ── Producto ──
            builder.Entity<Producto>(e =>
            {
                e.HasIndex(p => p.Codigo).IsUnique();
                // Nota: SQLite no soporta HasFilter en índices,
                // pero el índice unique con nulls funciona bien en SQLite
                // (permite múltiples NULLs por defecto)

                e.HasOne(p => p.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(p => p.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Orden ──
            builder.Entity<Orden>(e =>
            {
                e.HasIndex(o => o.NumeroOrden).IsUnique();
                e.HasOne(o => o.Cliente)
                    .WithMany(c => c.Ordenes)
                    .HasForeignKey(o => o.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── OrdenDetalle ──
            builder.Entity<OrdenDetalle>(e =>
            {
                e.HasOne(d => d.Orden)
                    .WithMany(o => o.Detalles)
                    .HasForeignKey(d => d.OrdenId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(d => d.Producto)
                    .WithMany(p => p.OrdenDetalles)
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Factura (1:1 con Orden) ──
            builder.Entity<Factura>(e =>
            {
                e.HasIndex(f => f.NumeroFactura).IsUnique();
                e.HasOne(f => f.Orden)
                    .WithOne(o => o.Factura)
                    .HasForeignKey<Factura>(f => f.OrdenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Global Query Filters: Soft Delete ──
            // Cada query LINQ filtra automáticamente Activo == true
            builder.Entity<Categoria>().HasQueryFilter(c => c.Activo);
            builder.Entity<Producto>().HasQueryFilter(p => p.Activo);
            builder.Entity<Cliente>().HasQueryFilter(c => c.Activo);
            builder.Entity<Orden>().HasQueryFilter(o => o.Activo);
            builder.Entity<Factura>().HasQueryFilter(f => f.Activo);
        }

        // ── Override SaveChanges para auditoría automática ──
        private readonly IHttpContextAccessor? _httpContextAccessor;

        // Constructor adicional para inyectar HttpContextAccessor
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            var auditEntries = PrepararAuditoria();
            ActualizarCamposAuditoria();
            var result = base.SaveChanges();
            GuardarAuditoria(auditEntries);
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var auditEntries = PrepararAuditoria();
            ActualizarCamposAuditoria();
            var result = await base.SaveChangesAsync(ct);
            await GuardarAuditoriaAsync(auditEntries, ct);
            return result;
        }

        private void ActualizarCamposAuditoria()
        {
            var entries = ChangeTracker.Entries<EntidadBase>();
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.FechaCreacion = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.FechaModificacion = DateTime.UtcNow;
                        break;
                }
            }
        }

        /// Captura los cambios ANTES de guardar para registrarlos en AuditLog.
        /// Solo audita entidades que heredan de EntidadBase.
        private List<AuditLog> PrepararAuditoria()
        {
            var usuario = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Sistema";
            var auditEntries = new List<AuditLog>();

            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries<EntidadBase>())
            {
                if (entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var audit = new AuditLog
                {
                    Entidad = entry.Entity.GetType().Name,
                    Usuario = usuario,
                    Fecha = DateTime.UtcNow
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.Accion = "Crear";
                        // El ID aún no está asignado, se completa después
                        audit.ValoresNuevos = SerializarPropiedades(entry.CurrentValues);
                        break;

                    case EntityState.Modified:
                        audit.Accion = "Editar";
                        audit.EntidadId = entry.Entity.Id;
                        audit.ValoresAnteriores = SerializarCambios(entry, original: true);
                        audit.ValoresNuevos = SerializarCambios(entry, original: false);
                        break;

                    case EntityState.Deleted:
                        audit.Accion = "Eliminar";
                        audit.EntidadId = entry.Entity.Id;
                        audit.ValoresAnteriores = SerializarPropiedades(entry.OriginalValues);
                        break;
                }

                auditEntries.Add(audit);
            }

            return auditEntries;
        }

        private void GuardarAuditoria(List<AuditLog> auditEntries)
        {
            if (!auditEntries.Any()) return;

            // Asignar IDs de entidades recién creadas
            foreach (var audit in auditEntries.Where(a => a.Accion == "Crear"))
            {
                // Buscar la entidad correspondiente ya con ID asignado
                var entry = ChangeTracker.Entries<EntidadBase>()
                    .FirstOrDefault(e => e.Entity.GetType().Name == audit.Entidad
                                        && e.Entity.Id > 0
                                        && audit.EntidadId == 0);
                if (entry != null)
                    audit.EntidadId = entry.Entity.Id;
            }

            AuditLogs.AddRange(auditEntries);
            base.SaveChanges();
        }

        private async Task GuardarAuditoriaAsync(
            List<AuditLog> auditEntries, CancellationToken ct)
        {
            if (!auditEntries.Any()) return;

            foreach (var audit in auditEntries.Where(a => a.Accion == "Crear"))
            {
                var entry = ChangeTracker.Entries<EntidadBase>()
                    .FirstOrDefault(e => e.Entity.GetType().Name == audit.Entidad
                                        && e.Entity.Id > 0
                                        && audit.EntidadId == 0);
                if (entry != null)
                    audit.EntidadId = entry.Entity.Id;
            }

            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(ct);
        }

        /// Serializa solo las propiedades que cambiaron (para ediciones)
        private static string SerializarCambios(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, bool original)
        {
            var cambios = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsModified)
                {
                    cambios[prop.Metadata.Name] = original
                        ? prop.OriginalValue
                        : prop.CurrentValue;
                }
            }

            return System.Text.Json.JsonSerializer.Serialize(cambios);
        }

        /// Serializa todas las propiedades (para crear/eliminar).
        /// Excluye propiedades de navegación y campos internos.
        private static string SerializarPropiedades(
            Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values)
        {
            var props = new Dictionary<string, object?>();
            foreach (var prop in values.Properties)
            {
                // Excluir campos de auditoría internos para no llenar el log
                if (prop.Name is "CreadoPor" or "ModificadoPor"
                    or "FechaCreacion" or "FechaModificacion")
                    continue;

                props[prop.Name] = values[prop.Name];
            }
            return System.Text.Json.JsonSerializer.Serialize(props);
        }
    }
}
