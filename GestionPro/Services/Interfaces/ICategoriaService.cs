using GestionPro.Models;
using GestionPro.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionPro.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<PagedList<CategoriaListViewModel>> ObtenerTodosAsync(
            string? busqueda, int pagina, int porPagina = 10);
        Task<CategoriaFormViewModel?> ObtenerParaEditarAsync(int id);
        Task<int> CrearAsync(CategoriaFormViewModel model, string usuario);
        Task<bool> ActualizarAsync(CategoriaFormViewModel model, string usuario);
        Task<bool> EliminarAsync(int id, string usuario);
        Task<List<SelectListItem>> ObtenerSelectListAsync();
    }
}
