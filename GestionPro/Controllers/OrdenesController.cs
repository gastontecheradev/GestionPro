using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionPro.Filters;
using GestionPro.Models.Enums;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Controllers
{
    [Authorize]
    public class OrdenesController : Controller
    {
        private readonly IOrdenService _ordenService;

        public OrdenesController(IOrdenService ordenService)
        {
            _ordenService = ordenService;
        }

        // GET: Ordenes
        public async Task<IActionResult> Index(
            string? busqueda, EstadoOrden? estado, int pagina = 1)
        {
            var ordenes = await _ordenService.ObtenerTodosAsync(
                busqueda, estado, pagina);

            ViewBag.BusquedaActual = busqueda;
            ViewBag.EstadoActual = estado;
            return View(ordenes);
        }

        // GET: Ordenes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var orden = await _ordenService.ObtenerPorIdAsync(id);
            if (orden == null) return NotFound();
            return View(orden);
        }

        // GET: Ordenes/Create
        public async Task<IActionResult> Create()
        {
            var model = new OrdenFormViewModel
            {
                Clientes = await _ordenService.ObtenerClientesSelectAsync(),
                Productos = await _ordenService.ObtenerProductosSelectAsync()
            };
            return View(model);
        }

        // POST: Ordenes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DemoReadOnly]
        public async Task<IActionResult> Create(OrdenFormViewModel model)
        {
            // Remover validaciones de campos que se llenan por JS/servidor
            ModelState.Remove("Detalles");
            for (int i = 0; i < model.Detalles.Count; i++)
            {
                ModelState.Remove($"Detalles[{i}].PrecioUnitario");
                ModelState.Remove($"Detalles[{i}].Subtotal");
                ModelState.Remove($"Detalles[{i}].ProductoNombre");
                ModelState.Remove($"Detalles[{i}].StockDisponible");
            }

            if (model.Detalles == null || !model.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos una línea de detalle.");
            }

            if (!ModelState.IsValid)
            {
                model.Clientes = await _ordenService.ObtenerClientesSelectAsync();
                model.Productos = await _ordenService.ObtenerProductosSelectAsync();
                return View(model);
            }

            try
            {
                var usuario = User.Identity?.Name ?? "Sistema";
                var ordenId = await _ordenService.CrearAsync(model, usuario);

                TempData["Success"] = "Orden creada correctamente.";
                return RedirectToAction(nameof(Details), new { id = ordenId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                model.Clientes = await _ordenService.ObtenerClientesSelectAsync();
                model.Productos = await _ordenService.ObtenerProductosSelectAsync();
                return View(model);
            }
        }

        // POST: Ordenes/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DemoReadOnly]
        public async Task<IActionResult> CambiarEstado(int id, EstadoOrden nuevoEstado)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _ordenService.CambiarEstadoAsync(id, nuevoEstado, usuario);

            if (resultado)
            {
                TempData["Success"] = $"Estado cambiado a {nuevoEstado}.";
            }
            else
            {
                TempData["Error"] = "No se pudo cambiar el estado de la orden.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Ordenes/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DemoReadOnly]
        public async Task<IActionResult> Cancelar(int id)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _ordenService.CancelarAsync(id, usuario);

            if (resultado)
            {
                TempData["Success"] = "Orden cancelada. El stock fue devuelto.";
            }
            else
            {
                TempData["Error"] = "No se puede cancelar esta orden (ya entregada o facturada).";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── API JSON para el JavaScript dinámico ──

        /// Devuelve info del producto (precio, stock) para las líneas dinámicas.
        /// Llamado via AJAX desde el formulario de crear orden.
        [HttpGet]
        public async Task<IActionResult> GetProductoInfo(int id)
        {
            var info = await _ordenService.ObtenerInfoProductoAsync(id);
            if (info == null) return NotFound();
            return Json(info);
        }
    }
}
