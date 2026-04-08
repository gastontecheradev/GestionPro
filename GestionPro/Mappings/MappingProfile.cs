using AutoMapper;
using GestionPro.Models.Entities;
using GestionPro.Models.ViewModels;

namespace GestionPro.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── Cliente ──
            CreateMap<Cliente, ClienteFormViewModel>().ReverseMap();
            CreateMap<Cliente, ClienteListViewModel>()
                .ForMember(dest => dest.TotalOrdenes,
                    opt => opt.MapFrom(src => src.Ordenes.Count));
            CreateMap<Cliente, ClienteDetalleViewModel>();

            // ── Categoria ──
            CreateMap<Categoria, CategoriaFormViewModel>().ReverseMap();
            CreateMap<Categoria, CategoriaListViewModel>()
                .ForMember(dest => dest.TotalProductos,
                    opt => opt.MapFrom(src => src.Productos.Count));

            // ── Producto ──
            CreateMap<Producto, ProductoFormViewModel>().ReverseMap();
            CreateMap<Producto, ProductoListViewModel>()
                .ForMember(dest => dest.CategoriaNombre,
                    opt => opt.MapFrom(src => src.Categoria.Nombre))
                .ForMember(dest => dest.StockBajo,
                    opt => opt.MapFrom(src => src.Stock <= src.StockMinimo));
            CreateMap<Producto, ProductoDetalleViewModel>()
                .ForMember(dest => dest.CategoriaNombre,
                    opt => opt.MapFrom(src => src.Categoria.Nombre))
                .ForMember(dest => dest.StockBajo,
                    opt => opt.MapFrom(src => src.Stock <= src.StockMinimo));
        }
    }
}
