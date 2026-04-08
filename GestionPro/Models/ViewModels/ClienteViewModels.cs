using System.ComponentModel.DataAnnotations;

namespace GestionPro.Models.ViewModels
{
    /// <summary>
    /// ViewModel para crear y editar clientes.
    /// Nunca exponemos la entidad directamente a la vista.
    /// </summary>
    public class ClienteFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres")]
        [Display(Name = "RUT")]
        public string? RUT { get; set; }

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Persona de Contacto")]
        public string? Contacto { get; set; }
    }

    /// <summary>
    /// ViewModel para mostrar clientes en listados.
    /// Incluye datos calculados que no están en la entidad.
    /// </summary>
    public class ClienteListViewModel
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? RUT { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Contacto { get; set; }
        public int TotalOrdenes { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    /// <summary>
    /// ViewModel para la vista de detalle de un cliente.
    /// </summary>
    public class ClienteDetalleViewModel
    {
        public int Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string? RUT { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Contacto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? CreadoPor { get; set; }
    }
}
