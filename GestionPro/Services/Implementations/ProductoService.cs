using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GestionPro.Data;
using GestionPro.Models;
using GestionPro.Models.Entities;
using GestionPro.Models.ViewModels;
using GestionPro.Services.Interfaces;

namespace GestionPro.Services.Implementations
{
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductoService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedList<ProductoListViewModel>> ObtenerTodosAsync(
            string? busqueda, int? categoriaId, bool? soloStockBajo,
            int pagina, int porPagina = 10)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    (p.Codigo != null && p.Codigo.ToLower().Contains(busqueda)) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(busqueda)));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (soloStockBajo == true)
                query = query.Where(p => p.Stock <= p.StockMinimo);

            var projected = query
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoListViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Codigo = p.Codigo,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo,
                    StockBajo = p.Stock <= p.StockMinimo,
                    CategoriaNombre = p.Categoria.Nombre,
                    FechaCreacion = p.FechaCreacion
                });

            return await PagedList<ProductoListViewModel>.CreateAsync(projected, pagina, porPagina);
        }

        public async Task<ProductoDetalleViewModel?> ObtenerPorIdAsync(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            return producto == null ? null : _mapper.Map<ProductoDetalleViewModel>(producto);
        }

        public async Task<ProductoFormViewModel?> ObtenerParaEditarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            return producto == null ? null : _mapper.Map<ProductoFormViewModel>(producto);
        }

        public async Task<int> CrearAsync(ProductoFormViewModel model, string usuario)
        {
            var producto = _mapper.Map<Producto>(model);
            producto.CreadoPor = usuario;
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto.Id;
        }

        public async Task<bool> ActualizarAsync(ProductoFormViewModel model, string usuario)
        {
            var producto = await _context.Productos.FindAsync(model.Id);
            if (producto == null) return false;
            _mapper.Map(model, producto);
            producto.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id, string usuario)
        {
            var producto = await _context.Productos
                .Include(p => p.OrdenDetalles)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return false;
            if (producto.OrdenDetalles.Any()) return false;
            producto.Activo = false;
            producto.ModificadoPor = usuario;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
