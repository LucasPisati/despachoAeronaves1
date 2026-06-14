using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace despachoAeronave.Models
{
    public class Vuelo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de vuelo es requerido")]
        [StringLength(10)]
        public string NumeroVuelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El origen es requerido")]
        [StringLength(50)]
        public string Origen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El destino es requerido")]
        [StringLength(50)]
        public string Destino { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha y hora de salida es requerida")]
        public DateTime FechaHoraSalida { get; set; }

        [Required(ErrorMessage = "La fecha y hora de llegada es requerida")]
        public DateTime FechaHoraLlegada { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Programado"; // "Programado", "En Vuelo", "Aterrizado", "Cancelado"

        [Required(ErrorMessage = "Debe asignar una aeronave")]
        public int AeronaveId { get; set; }

        [ForeignKey("AeronaveId")]
        public Aeronave? Aeronave { get; set; }

        public int? PilotoId { get; set; }

        [ForeignKey("PilotoId")]
        public Usuario? Piloto { get; set; }
    }
}
