namespace GestionPro.Models.Entities
{
    /// Clase base que agrega campos de auditoría y soft delete a todas las entidades.
    /// Todas las entidades principales heredan de esta clase.
    public abstract class EntidadBase
    {
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaModificacion { get; set; }

        public string? CreadoPor { get; set; }

        public string? ModificadoPor { get; set; }

        /// Soft delete: en vez de borrar, marcamos Activo = false.
        /// Los Global Query Filters en el DbContext filtran automáticamente.
        public bool Activo { get; set; } = true;
    }
}
