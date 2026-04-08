using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionPro.Filters;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        // GET: Categorias
        public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
        {
            var categorias = await _categoriaService.ObtenerTodosAsync(busqueda, pagina);
            ViewBag.BusquedaActual = busqueda;
            return View(categorias);
        }

        // GET: Categorias/Create
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public IActionResult Create()
        {
            return View(new CategoriaFormViewModel());
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Create(CategoriaFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = User.Identity?.Name ?? "Sistema";
            await _categoriaService.CrearAsync(model, usuario);

            TempData["Success"] = $"Categoría \"{model.Nombre}\" creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Edit/5
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoriaService.ObtenerParaEditarAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id, CategoriaFormViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _categoriaService.ActualizarAsync(model, usuario);
            if (!resultado) return NotFound();

            TempData["Success"] = $"Categoría \"{model.Nombre}\" actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Categorias/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _categoriaService.EliminarAsync(id, usuario);

            if (!resultado)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene productos asociados.";
            }
            else
            {
                TempData["Success"] = "Categoría eliminada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
