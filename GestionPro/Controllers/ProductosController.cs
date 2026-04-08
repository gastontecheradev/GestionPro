using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionPro.Filters;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;

        public ProductosController(
            IProductoService productoService,
            ICategoriaService categoriaService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        // GET: Productos
        public async Task<IActionResult> Index(
            string? busqueda, int? categoriaId, bool? stockBajo, int pagina = 1)
        {
            var productos = await _productoService.ObtenerTodosAsync(
                busqueda, categoriaId, stockBajo, pagina);

            ViewBag.BusquedaActual = busqueda;
            ViewBag.CategoriaActual = categoriaId;
            ViewBag.StockBajoActual = stockBajo;
            ViewBag.Categorias = await _categoriaService.ObtenerSelectListAsync();

            return View(productos);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // GET: Productos/Create
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Create()
        {
            var model = new ProductoFormViewModel
            {
                Categorias = await _categoriaService.ObtenerSelectListAsync()
            };
            return View(model);
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Create(ProductoFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categorias = await _categoriaService.ObtenerSelectListAsync();
                return View(model);
            }

            var usuario = User.Identity?.Name ?? "Sistema";
            await _productoService.CrearAsync(model, usuario);

            TempData["Success"] = $"Producto \"{model.Nombre}\" creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Edit/5
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productoService.ObtenerParaEditarAsync(id);
            if (model == null) return NotFound();

            model.Categorias = await _categoriaService.ObtenerSelectListAsync();
            return View(model);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id, ProductoFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                model.Categorias = await _categoriaService.ObtenerSelectListAsync();
                return View(model);
            }

            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _productoService.ActualizarAsync(model, usuario);
            if (!resultado) return NotFound();

            TempData["Success"] = $"Producto \"{model.Nombre}\" actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Productos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _productoService.EliminarAsync(id, usuario);

            if (!resultado)
            {
                TempData["Error"] = "No se puede eliminar el producto porque tiene órdenes asociadas.";
            }
            else
            {
                TempData["Success"] = "Producto eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
