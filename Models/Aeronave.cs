using System.ComponentModel.DataAnnotations;

namespace despachoAeronave.Models
{
    public class Aeronave
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La matrícula es requerida")]
        [StringLength(20)]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        [StringLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "La capacidad debe ser un número positivo")]
        public int CapacidadPasajeros { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Activa"; // "Activa", "Mantenimiento", "Inactiva"
    }
}
