using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models;
using GestionPro.Models.Entities;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Services.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClienteService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedList<ClienteListViewModel>> ObtenerTodosAsync(
            string? busqueda, int pagina, int porPagina = 10)
        {
            var query = _context.Clientes
                .Include(c => c.Ordenes)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(c =>
                    c.RazonSocial.ToLower().Contains(busqueda) ||
                    (c.RUT != null && c.RUT.Contains(busqueda)) ||
                    (c.Email != null && c.Email.ToLower().Contains(busqueda)) ||
                    (c.Contacto != null && c.Contacto.ToLower().Contains(busqueda)));
            }

            var projected = query
                .OrderBy(c => c.RazonSocial)
                .Select(c => new ClienteListViewModel
                {
                    Id = c.Id,
                    RazonSocial = c.RazonSocial,
                    RUT = c.RUT,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Contacto = c.Contacto,
                    TotalOrdenes = c.Ordenes.Count,
                    FechaCreacion = c.FechaCreacion
                });

            return await PagedList<ClienteListViewModel>.CreateAsync(projected, pagina, porPagina);
        }

        public async Task<ClienteDetalleViewModel?> ObtenerPorIdAsync(int id)
        {
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            return cliente == null ? null : _mapper.Map<ClienteDetalleViewModel>(cliente);
        }

        public async Task<ClienteFormViewModel?> ObtenerParaEditarAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            return cliente == null ? null : _mapper.Map<ClienteFormViewModel>(cliente);
        }

        public async Task<int> CrearAsync(ClienteFormViewModel model, string usuario)
        {
            var cliente = _mapper.Map<Cliente>(model);
            cliente.CreadoPor = usuario;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente.Id;
        }

        public async Task<bool> ActualizarAsync(ClienteFormViewModel model, string usuario)
        {
            var cliente = await _context.Clientes.FindAsync(model.Id);
            if (cliente == null) return false;
            _mapper.Map(model, cliente);
            cliente.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id, string usuario)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Ordenes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return false;
            if (cliente.Ordenes.Any()) return false;
            cliente.Activo = false;
            cliente.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
