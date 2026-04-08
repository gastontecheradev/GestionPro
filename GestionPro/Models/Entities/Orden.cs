using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionPro.Models.Enums;

namespace GestionPro.Models.Entities
{
    public class Orden : EntidadBase
    {
        [StringLength(20)]
        public string NumeroOrden { get; set; } = string.Empty;

        public DateTime FechaOrden { get; set; } = DateTime.UtcNow;

        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeIVA { get; set; } = 22; // IVA Uruguay

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoIVA { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // FK
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        // Navegación
        public ICollection<OrdenDetalle> Detalles { get; set; }
            = new List<OrdenDetalle>();
        public Factura? Factura { get; set; }
    }
}
