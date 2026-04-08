using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPro.Models.Entities
{
    public class Factura : EntidadBase
    {
        [StringLength(20)]
        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        public bool Pagada { get; set; } = false;

        public DateTime? FechaPago { get; set; }

        // FK — Relación 1:1 con Orden
        public int OrdenId { get; set; }
        public Orden Orden { get; set; } = null!;
    }
}
