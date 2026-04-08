namespace GestionPro.Models.Entities
{
    /// <summary>
    /// Clase base que agrega campos de auditoría y soft delete a todas las entidades.
    /// Todas las entidades principales heredan de esta clase.
    /// </summary>
    public abstract class EntidadBase
    {
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaModificacion { get; set; }

        public string? CreadoPor { get; set; }

        public string? ModificadoPor { get; set; }

        /// <summary>
        /// Soft delete: en vez de borrar, marcamos Activo = false.
        /// Los Global Query Filters en el DbContext filtran automáticamente.
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}
