using System.ComponentModel.DataAnnotations;

namespace GestionPro.Models.Entities
{
    public class Cliente : EntidadBase
    {
        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(200)]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RUT { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        [Phone]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; } // Persona de contacto

        // Navegación
        public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
    }
}
