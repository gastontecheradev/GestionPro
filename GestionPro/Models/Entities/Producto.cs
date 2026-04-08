using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPro.Models.Entities
{
    public class Producto : EntidadBase
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        public int StockMinimo { get; set; } = 5;

        [StringLength(50)]
        public string? Codigo { get; set; } // SKU

        public string? ImagenUrl { get; set; }

        // FK
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        // Navegación
        public ICollection<OrdenDetalle> OrdenDetalles { get; set; }
            = new List<OrdenDetalle>();

        // Propiedad calculada (no se guarda en la DB)
        [NotMapped]
        public bool StockBajo => Stock <= StockMinimo;
    }
}
