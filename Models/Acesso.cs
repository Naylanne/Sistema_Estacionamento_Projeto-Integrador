using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Acesso
    {
        [Key]
        public int IdAcesso { get; set; }

        [Required]
        public string Ticket { get; set; } = string.Empty;

        // FK Tarifa
        public int IdTarifa { get; set; }

        [JsonIgnore] // Evita ciclo infinito no JSON
        public Tarifa? Tarifa { get; set; }

        // FK Vaga
        public int IdVaga { get; set; }

        [JsonIgnore] // Evita ciclo infinito no JSON
        public Vaga? Vaga { get; set; }

        // FK Veículo
        public int IdVeiculo { get; set; }

        [JsonIgnore] // Evita ciclo infinito no JSON
        public Veiculo? Veiculo { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime HoraEntrada { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? HoraSaida { get; set; }

        public TimeSpan? TempoPermanencia { get; set; }

        // Relação 1:1 com pagamento
        [JsonIgnore] // Evita ciclo infinito no JSON
        public Pagamento? Pagamento { get; set; }
    }
}