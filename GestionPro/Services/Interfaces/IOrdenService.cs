using GestionPro.Models;
using GestionPro.Models.Enums;
using GestionPro.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionPro.Services.Interfaces
{
    public interface IOrdenService
    {
        Task<PagedList<OrdenListViewModel>> ObtenerTodosAsync(
            string? busqueda, EstadoOrden? estado, int pagina, int porPagina = 10);
        Task<OrdenDetallePageViewModel?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(OrdenFormViewModel model, string usuario);
        Task<bool> CambiarEstadoAsync(int id, EstadoOrden nuevoEstado, string usuario);
        Task<bool> CancelarAsync(int id, string usuario);
        Task<List<SelectListItem>> ObtenerClientesSelectAsync();
        Task<List<SelectListItem>> ObtenerProductosSelectAsync();
        Task<ProductoInfoDto?> ObtenerInfoProductoAsync(int productoId);
    }
}
