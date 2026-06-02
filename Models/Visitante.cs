using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Visitante
    {
        [Key]
        public int IdTicket { get; set; }

        [Required]
        [StringLength(7, MinimumLength = 7)]
        [RegularExpression(
            @"^[A-Za-z0-9]{7}$", ErrorMessage = "A placa deve conter exatamente 7 caracteres alfanuméricos.")]
        [Column(TypeName = "char(7)")]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime HorarioEntrada { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? HorarioSaida { get; set; }
  
    }
}