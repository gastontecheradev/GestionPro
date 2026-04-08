using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models;
using GestionPro.Models.Entities;
using GestionPro.Models.Enums;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Services.Implementations
{
    public class FacturaService : IFacturaService
    {
        private readonly AppDbContext _context;

        public FacturaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<FacturaListViewModel>> ObtenerTodosAsync(
            string? busqueda, bool? soloPendientes, int pagina, int porPagina = 10)
        {
            var query = _context.Facturas
                .Include(f => f.Orden)
                    .ThenInclude(o => o.Cliente)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(f =>
                    f.NumeroFactura.ToLower().Contains(busqueda) ||
                    f.Orden.NumeroOrden.ToLower().Contains(busqueda) ||
                    f.Orden.Cliente.RazonSocial.ToLower().Contains(busqueda));
            }

            if (soloPendientes == true)
            {
                query = query.Where(f => !f.Pagada);
            }

            var projected = query
                .OrderByDescending(f => f.FechaEmision)
                .Select(f => new FacturaListViewModel
                {
                    Id = f.Id,
                    NumeroFactura = f.NumeroFactura,
                    NumeroOrden = f.Orden.NumeroOrden,
                    OrdenId = f.OrdenId,
                    ClienteNombre = f.Orden.Cliente.RazonSocial,
                    FechaEmision = f.FechaEmision,
                    MontoTotal = f.MontoTotal,
                    Pagada = f.Pagada,
                    FechaPago = f.FechaPago
                });

            return await PagedList<FacturaListViewModel>.CreateAsync(projected, pagina, porPagina);
        }

        public async Task<FacturaDetalleViewModel?> ObtenerPorIdAsync(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.Orden)
                    .ThenInclude(o => o.Cliente)
                .Include(f => f.Orden)
                    .ThenInclude(o => o.Detalles)
                        .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null) return null;

            return new FacturaDetalleViewModel
            {
                Id = factura.Id,
                NumeroFactura = factura.NumeroFactura,
                FechaEmision = factura.FechaEmision,
                MontoTotal = factura.MontoTotal,
                Pagada = factura.Pagada,
                FechaPago = factura.FechaPago,
                CreadoPor = factura.CreadoPor,
                OrdenId = factura.OrdenId,
                NumeroOrden = factura.Orden.NumeroOrden,
                ClienteNombre = factura.Orden.Cliente.RazonSocial,
                ClienteRUT = factura.Orden.Cliente.RUT,
                ClienteDireccion = factura.Orden.Cliente.Direccion,
                Subtotal = factura.Orden.Subtotal,
                PorcentajeIVA = factura.Orden.PorcentajeIVA,
                MontoIVA = factura.Orden.MontoIVA,
                Lineas = factura.Orden.Detalles.Select(d => new OrdenDetalleLineaViewModel
                {
                    ProductoNombre = d.Producto.Nombre,
                    ProductoCodigo = d.Producto.Codigo,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }

        public async Task<int> GenerarDesdeOrdenAsync(int ordenId, string usuario)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Factura)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null)
                throw new InvalidOperationException("Orden no encontrada.");

            if (orden.Factura != null)
                throw new InvalidOperationException("Esta orden ya tiene una factura generada.");

            if (orden.Estado == EstadoOrden.Pendiente || orden.Estado == EstadoOrden.Cancelada)
                throw new InvalidOperationException(
                    "Solo se puede facturar órdenes aprobadas o posteriores.");

            var numeroFactura = await GenerarNumeroFacturaAsync();

            var factura = new Factura
            {
                NumeroFactura = numeroFactura,
                MontoTotal = orden.Total,
                OrdenId = ordenId,
                CreadoPor = usuario
            };

            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return factura.Id;
        }

        public async Task<bool> MarcarPagadaAsync(int id, string usuario)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null) return false;

            factura.Pagada = true;
            factura.FechaPago = DateTime.UtcNow;
            factura.ModificadoPor = usuario;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerarNumeroFacturaAsync()
        {
            var anio = DateTime.UtcNow.Year;
            var prefijo = $"FAC-{anio}-";

            var ultimaFactura = await _context.Facturas
                .IgnoreQueryFilters()
                .Where(f => f.NumeroFactura.StartsWith(prefijo))
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimaFactura != null)
            {
                var parteNumerica = ultimaFactura.Replace(prefijo, "");
                if (int.TryParse(parteNumerica, out var ultimo))
                {
                    siguienteNumero = ultimo + 1;
                }
            }

            return $"{prefijo}{siguienteNumero:D4}";
        }
    }
}
