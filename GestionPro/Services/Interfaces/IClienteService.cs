using GestionPro.Models;
using GestionPro.Models.ViewModels;

namespace GestionPro.Services.Interfaces
{
    public interface IClienteService
    {
        Task<PagedList<ClienteListViewModel>> ObtenerTodosAsync(
            string? busqueda, int pagina, int porPagina = 10);
        Task<ClienteDetalleViewModel?> ObtenerPorIdAsync(int id);
        Task<ClienteFormViewModel?> ObtenerParaEditarAsync(int id);
        Task<int> CrearAsync(ClienteFormViewModel model, string usuario);
        Task<bool> ActualizarAsync(ClienteFormViewModel model, string usuario);
        Task<bool> EliminarAsync(int id, string usuario);
    }
}
