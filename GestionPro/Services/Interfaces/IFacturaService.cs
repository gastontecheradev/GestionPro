using GestionPro.Models;
using GestionPro.Models.ViewModels;

namespace GestionPro.Services.Interfaces
{
    public interface IFacturaService
    {
        Task<PagedList<FacturaListViewModel>> ObtenerTodosAsync(
            string? busqueda, bool? soloPendientes, int pagina, int porPagina = 10);
        Task<FacturaDetalleViewModel?> ObtenerPorIdAsync(int id);
        Task<int> GenerarDesdeOrdenAsync(int ordenId, string usuario);
        Task<bool> MarcarPagadaAsync(int id, string usuario);
    }
}
