using System.ComponentModel.DataAnnotations;
using GestionPro.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionPro.Models.ViewModels
{
    /// ViewModel para crear/editar una orden con sus líneas de detalle.
    public class OrdenFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Líneas de detalle
        public List<OrdenDetalleFormViewModel> Detalles { get; set; } = new();

        // Listas para dropdowns
        public List<SelectListItem>? Clientes { get; set; }
        public List<SelectListItem>? Productos { get; set; }
    }

    /// ViewModel para cada línea de detalle en el formulario.
    public class OrdenDetalleFormViewModel
    {
        [Required(ErrorMessage = "Seleccione un producto")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        // Estos se completan desde el servidor/JS
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Para mostrar en la UI
        public string? ProductoNombre { get; set; }
        public int StockDisponible { get; set; }
    }

    /// ViewModel para el listado de órdenes.
    public class OrdenListViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public EstadoOrden Estado { get; set; }
        public decimal Total { get; set; }
        public int TotalLineas { get; set; }
        public bool TieneFactura { get; set; }
    }

    /// ViewModel para la vista de detalle de una orden.
    public class OrdenDetallePageViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public EstadoOrden Estado { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal MontoIVA { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public string? CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool TieneFactura { get; set; }

        public List<OrdenDetalleLineaViewModel> Lineas { get; set; } = new();

        // Estados a los que puede transicionar
        public List<EstadoOrden> EstadosPermitidos { get; set; } = new();
    }

    /// Cada línea de detalle en la vista de detalle.
    public class OrdenDetalleLineaViewModel
    {
        public string ProductoNombre { get; set; } = string.Empty;
        public string? ProductoCodigo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// DTO para la info de producto que devuelve el endpoint JSON (para el JS dinámico).
    public class ProductoInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}
