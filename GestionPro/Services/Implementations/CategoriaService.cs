using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models;
using GestionPro.Models.Entities;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Services.Implementations
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoriaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedList<CategoriaListViewModel>> ObtenerTodosAsync(
            string? busqueda, int pagina, int porPagina = 10)
        {
            var query = _context.Categorias
                .Include(c => c.Productos)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(c =>
                    c.Nombre.ToLower().Contains(busqueda) ||
                    (c.Descripcion != null && c.Descripcion.ToLower().Contains(busqueda)));
            }

            var projected = query
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaListViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    TotalProductos = c.Productos.Count,
                    FechaCreacion = c.FechaCreacion
                });

            return await PagedList<CategoriaListViewModel>.CreateAsync(projected, pagina, porPagina);
        }

        public async Task<CategoriaFormViewModel?> ObtenerParaEditarAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            return categoria == null ? null : _mapper.Map<CategoriaFormViewModel>(categoria);
        }

        public async Task<int> CrearAsync(CategoriaFormViewModel model, string usuario)
        {
            var categoria = _mapper.Map<Categoria>(model);
            categoria.CreadoPor = usuario;
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return categoria.Id;
        }

        public async Task<bool> ActualizarAsync(CategoriaFormViewModel model, string usuario)
        {
            var categoria = await _context.Categorias.FindAsync(model.Id);
            if (categoria == null) return false;
            _mapper.Map(model, categoria);
            categoria.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id, string usuario)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (categoria == null) return false;
            if (categoria.Productos.Any()) return false;
            categoria.Activo = false;
            categoria.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SelectListItem>> ObtenerSelectListAsync()
        {
            return await _context.Categorias
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                })
                .ToListAsync();
        }
    }
}
