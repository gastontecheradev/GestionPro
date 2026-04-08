using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionPro.Filters;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
        {
            var clientes = await _clienteService.ObtenerTodosAsync(busqueda, pagina);

            ViewBag.BusquedaActual = busqueda;
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public IActionResult Create()
        {
            return View(new ClienteFormViewModel());
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Create(ClienteFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = User.Identity?.Name ?? "Sistema";
            await _clienteService.CrearAsync(model, usuario);

            TempData["Success"] = $"Cliente \"{model.RazonSocial}\" creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _clienteService.ObtenerParaEditarAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Vendedor")]
        [DemoReadOnly]
        public async Task<IActionResult> Edit(int id, ClienteFormViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _clienteService.ActualizarAsync(model, usuario);

            if (!resultado) return NotFound();

            TempData["Success"] = $"Cliente \"{model.RazonSocial}\" actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Clientes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [DemoReadOnly]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = User.Identity?.Name ?? "Sistema";
            var resultado = await _clienteService.EliminarAsync(id, usuario);

            if (!resultado)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene órdenes asociadas.";
            }
            else
            {
                TempData["Success"] = "Cliente eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
