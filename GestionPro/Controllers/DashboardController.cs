using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models.Enums;

namespace GestionPro.Controllers
{
    [Authorize] // Solo usuarios autenticados
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Métricas generales
            ViewBag.TotalClientes = await _context.Clientes.CountAsync();
            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.ProductosStockBajo = await _context.Productos
                .CountAsync(p => p.Stock <= p.StockMinimo);
            ViewBag.OrdenesPendientes = await _context.Ordenes
                .CountAsync(o => o.Estado == EstadoOrden.Pendiente);

            // Ventas del mes actual
            // SQLite no soporta Sum sobre decimal, se traen los totales a memoria
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var totalesMes = await _context.Ordenes
                .Where(o => o.FechaOrden >= inicioMes
                    && o.Estado != EstadoOrden.Cancelada)
                .Select(o => o.Total)
                .ToListAsync();
            ViewBag.VentasMes = totalesMes.Sum();

            // Últimas 5 órdenes
            ViewBag.UltimasOrdenes = await _context.Ordenes
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.FechaOrden)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
