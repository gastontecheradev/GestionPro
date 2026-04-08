using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionPro.Filters;
using GestionPro.Services.Interfaces;

namespace GestionPro.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private readonly IFacturaService _facturaService;

        public FacturasController(IFacturaService facturaService)
        {
            _facturaService = facturaService;
        }

        // GET: Facturas
        public async Task<IActionResult> Index(
            string? busqueda, bool? pendientes, int pagina = 1)
        {
            var facturas = await _facturaService.ObtenerTodosAsync(
                busqueda, pendientes, pagina);

            ViewBag.BusquedaActual = busqueda;
            ViewBag.PendientesActual = pendientes;
            return View(facturas);
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var factura = await _facturaService.ObtenerPorIdAsync(id);
            if (factura == null) return NotFound();
            return View(factura);
        }

        // POST: Facturas/GenerarDesdeOrden
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> GenerarDesdeOrden(int ordenId)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";
                var facturaId = await _facturaService.GenerarDesdeOrdenAsync(ordenId, usuario);

                TempData["Success"] = "Factura generada correctamente.";
                return RedirectToAction(nameof(Details), new { id = facturaId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Ordenes", new { id = ordenId });
            }
        }

        // POST: Facturas/MarcarPagada
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> MarcarPagada(int id)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _facturaService.MarcarPagadaAsync(id, usuario);

            if (resultado)
                TempData["Success"] = "Factura marcada como pagada.";
            else
                TempData["Error"] = "No se pudo actualizar la factura.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
