using System.ComponentModel.DataAnnotations;

namespace despachoAeronave.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Rol { get; set; } = "Despachador"; // "Despachador" o "Piloto"
    }
}
