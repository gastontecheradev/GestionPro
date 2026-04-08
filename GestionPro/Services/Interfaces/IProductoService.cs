using GestionPro.Models;
using GestionPro.Models.ViewModels;

namespace GestionPro.Services.Interfaces
{
    public interface IProductoService
    {
        Task<PagedList<ProductoListViewModel>> ObtenerTodosAsync(
            string? busqueda, int? categoriaId, bool? soloStockBajo,
            int pagina, int porPagina = 10);
        Task<ProductoDetalleViewModel?> ObtenerPorIdAsync(int id);
        Task<ProductoFormViewModel?> ObtenerParaEditarAsync(int id);
        Task<int> CrearAsync(ProductoFormViewModel model, string usuario);
        Task<bool> ActualizarAsync(ProductoFormViewModel model, string usuario);
        Task<bool> EliminarAsync(int id, string usuario);
    }
}
