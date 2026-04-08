using System.ComponentModel.DataAnnotations;

namespace GestionPro.Models.ViewModels
{
    public class FacturaListViewModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public int OrdenId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public bool Pagada { get; set; }
        public DateTime? FechaPago { get; set; }
    }

    public class FacturaDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public bool Pagada { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? CreadoPor { get; set; }

        // Datos de la orden asociada
        public int OrdenId { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string? ClienteRUT { get; set; }
        public string? ClienteDireccion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public decimal MontoIVA { get; set; }

        public List<OrdenDetalleLineaViewModel> Lineas { get; set; } = new();
    }
}
