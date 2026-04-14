namespace GestionPro.Models.Entities
{
    /// Registro de auditoría. Guarda cada cambio importante en el sistema:
    /// quién modificó qué, cuándo, y los valores anteriores/nuevos en JSON.
    public class AuditLog
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public int EntidadId { get; set; }
        public string Accion { get; set; } = string.Empty; // Crear / Editar / Eliminar
        public string? ValoresAnteriores { get; set; } // JSON
        public string? ValoresNuevos { get; set; } // JSON
        public string? Usuario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
