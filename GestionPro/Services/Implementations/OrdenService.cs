using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models;
using GestionPro.Models.Entities;
using GestionPro.Models.Enums;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Services.Implementations
{
    public class OrdenService : IOrdenService
    {
        private readonly AppDbContext _context;

        public OrdenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<OrdenListViewModel>> ObtenerTodosAsync(
            string? busqueda, EstadoOrden? estado, int pagina, int porPagina = 10)
        {
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                .Include(o => o.Factura)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(o =>
                    o.NumeroOrden.ToLower().Contains(busqueda) ||
                    o.Cliente.RazonSocial.ToLower().Contains(busqueda));
            }

            if (estado.HasValue)
            {
                query = query.Where(o => o.Estado == estado.Value);
            }

            var projected = query
                .OrderByDescending(o => o.FechaOrden)
                .Select(o => new OrdenListViewModel
                {
                    Id = o.Id,
                    NumeroOrden = o.NumeroOrden,
                    FechaOrden = o.FechaOrden,
                    ClienteNombre = o.Cliente.RazonSocial,
                    Estado = o.Estado,
                    Total = o.Total,
                    TotalLineas = o.Detalles.Count,
                    TieneFactura = o.Factura != null
                });

            return await PagedList<OrdenListViewModel>.CreateAsync(projected, pagina, porPagina);
        }

        public async Task<OrdenDetallePageViewModel?> ObtenerPorIdAsync(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(o => o.Factura)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null) return null;

            return new OrdenDetallePageViewModel
            {
                Id = orden.Id,
                NumeroOrden = orden.NumeroOrden,
                FechaOrden = orden.FechaOrden,
                Estado = orden.Estado,
                ClienteNombre = orden.Cliente.RazonSocial,
                ClienteId = orden.ClienteId,
                Subtotal = orden.Subtotal,
                PorcentajeIVA = orden.PorcentajeIVA,
                MontoIVA = orden.MontoIVA,
                Total = orden.Total,
                Observaciones = orden.Observaciones,
                CreadoPor = orden.CreadoPor,
                FechaCreacion = orden.FechaCreacion,
                TieneFactura = orden.Factura != null,
                Lineas = orden.Detalles.Select(d => new OrdenDetalleLineaViewModel
                {
                    ProductoNombre = d.Producto.Nombre,
                    ProductoCodigo = d.Producto.Codigo,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList(),
                EstadosPermitidos = ObtenerEstadosPermitidos(orden.Estado)
            };
        }

        public async Task<int> CrearAsync(OrdenFormViewModel model, string usuario)
        {
            // Usar transacción para garantizar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Generar número de orden correlativo
                var numeroOrden = await GenerarNumeroOrdenAsync();

                // 2. Crear la orden
                var orden = new Orden
                {
                    NumeroOrden = numeroOrden,
                    ClienteId = model.ClienteId,
                    Observaciones = model.Observaciones,
                    PorcentajeIVA = 22, // IVA Uruguay
                    CreadoPor = usuario
                };

                decimal subtotalOrden = 0;

                // 3. Procesar cada línea de detalle
                foreach (var linea in model.Detalles)
                {
                    // Obtener producto y validar stock
                    var producto = await _context.Productos.FindAsync(linea.ProductoId);
                    if (producto == null)
                        throw new InvalidOperationException(
                            $"Producto con ID {linea.ProductoId} no encontrado.");

                    if (producto.Stock < linea.Cantidad)
                        throw new InvalidOperationException(
                            $"Stock insuficiente para \"{producto.Nombre}\". " +
                            $"Disponible: {producto.Stock}, Solicitado: {linea.Cantidad}.");

                    // Descontar stock
                    producto.Stock -= linea.Cantidad;

                    // Crear detalle
                    var subtotalLinea = producto.Precio * linea.Cantidad;
                    orden.Detalles.Add(new OrdenDetalle
                    {
                        ProductoId = linea.ProductoId,
                        Cantidad = linea.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalLinea
                    });

                    subtotalOrden += subtotalLinea;
                }

                // 4. Calcular totales
                orden.Subtotal = subtotalOrden;
                orden.MontoIVA = Math.Round(subtotalOrden * (orden.PorcentajeIVA / 100), 2);
                orden.Total = orden.Subtotal + orden.MontoIVA;

                // 5. Guardar
                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return orden.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CambiarEstadoAsync(
            int id, EstadoOrden nuevoEstado, string usuario)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null) return false;

            var permitidos = ObtenerEstadosPermitidos(orden.Estado);
            if (!permitidos.Contains(nuevoEstado)) return false;

            orden.Estado = nuevoEstado;
            orden.ModificadoPor = usuario;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelarAsync(int id, string usuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Detalles)
                    .Include(o => o.Factura)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (orden == null) return false;

                // No se puede cancelar si ya tiene factura
                if (orden.Factura != null) return false;

                // No se puede cancelar si ya fue entregada
                if (orden.Estado == EstadoOrden.Entregada) return false;

                // Devolver stock
                foreach (var detalle in orden.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock += detalle.Cantidad;
                    }
                }

                orden.Estado = EstadoOrden.Cancelada;
                orden.ModificadoPor = usuario;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ── Helpers ──

        public async Task<List<SelectListItem>> ObtenerClientesSelectAsync()
        {
            return await _context.Clientes
                .OrderBy(c => c.RazonSocial)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.RazonSocial
                })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> ObtenerProductosSelectAsync()
        {
            return await _context.Productos
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Nombre} — ${p.Precio:N2} (Stock: {p.Stock})"
                })
                .ToListAsync();
        }

        public async Task<ProductoInfoDto?> ObtenerInfoProductoAsync(int productoId)
        {
            return await _context.Productos
                .Where(p => p.Id == productoId)
                .Select(p => new ProductoInfoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Stock = p.Stock
                })
                .FirstOrDefaultAsync();
        }

        private async Task<string> GenerarNumeroOrdenAsync()
        {
            var anio = DateTime.UtcNow.Year;
            var prefijo = $"ORD-{anio}-";

            // Buscar el último número de orden del año actual
            // IgnoreQueryFilters para incluir órdenes canceladas/inactivas
            var ultimaOrden = await _context.Ordenes
                .IgnoreQueryFilters()
                .Where(o => o.NumeroOrden.StartsWith(prefijo))
                .OrderByDescending(o => o.NumeroOrden)
                .Select(o => o.NumeroOrden)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimaOrden != null)
            {
                var parteNumerica = ultimaOrden.Replace(prefijo, "");
                if (int.TryParse(parteNumerica, out var ultimo))
                {
                    siguienteNumero = ultimo + 1;
                }
            }

            return $"{prefijo}{siguienteNumero:D4}";
        }

        /// Define las transiciones de estado válidas.
        /// Pendiente → Aprobada → EnPreparacion → Enviada → Entregada
        /// Cualquiera (excepto Entregada) → Cancelada
        private static List<EstadoOrden> ObtenerEstadosPermitidos(EstadoOrden estadoActual)
        {
            return estadoActual switch
            {
                EstadoOrden.Pendiente => new List<EstadoOrden>
                    { EstadoOrden.Aprobada, EstadoOrden.Cancelada },
                EstadoOrden.Aprobada => new List<EstadoOrden>
                    { EstadoOrden.EnPreparacion, EstadoOrden.Cancelada },
                EstadoOrden.EnPreparacion => new List<EstadoOrden>
                    { EstadoOrden.Enviada, EstadoOrden.Cancelada },
                EstadoOrden.Enviada => new List<EstadoOrden>
                    { EstadoOrden.Entregada },
                _ => new List<EstadoOrden>()
            };
        }
    }
}
