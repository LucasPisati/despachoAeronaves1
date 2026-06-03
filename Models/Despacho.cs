using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace despachoAeronave.Models
{
    public class Despacho
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un vuelo")]
        public int VueloId { get; set; }

        [ForeignKey("VueloId")]
        public Vuelo? Vuelo { get; set; }

        [Required(ErrorMessage = "El nombre del despachador es requerido")]
        [StringLength(100)]
        public string DespachadorNombre { get; set; } = string.Empty;

        [Range(0, 500000, ErrorMessage = "El combustible requerido debe ser mayor o igual a 0")]
        public double CombustibleRequerido { get; set; }

        [Range(0, 500000, ErrorMessage = "La carga de pago debe ser mayor o igual a 0")]
        public double CargaPago { get; set; }

        [Required(ErrorMessage = "La ruta de vuelo es requerida")]
        public string Ruta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El reporte de clima es requerido")]
        public string ClimaReporte { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string? Observaciones { get; set; }
    }
}
