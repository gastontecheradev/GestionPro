using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPro.Models.Entities
{
    /// Línea de detalle de una Orden. NO hereda de EntidadBase porque
    /// su ciclo de vida depende de la Orden padre (Aggregate Root pattern).
    public class OrdenDetalle
    {
        public int Id { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // FK
        public int OrdenId { get; set; }
        public Orden Orden { get; set; } = null!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
    }
}
