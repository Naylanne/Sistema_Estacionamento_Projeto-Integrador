using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class AcessoVeiculo
    {
        [Key]
        public int IdAcesso { get; set; }

        // FK Tarifa
        public int IdTarifa { get; set; }

        [JsonIgnore]
        public Tarifa? Tarifa { get; set; }

        // FK Vaga
        public int IdVaga { get; set; }

        [JsonIgnore]
        public Vaga? Vaga { get; set; }

        // FK Veículo
        public int IdVeiculo { get; set; }

        [JsonIgnore]
        public Veiculo? Veiculo { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime HoraEntrada { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? HoraSaida { get; set; }

        public TimeSpan? TempoPermanencia { get; set; }

        // Relação 1:1 com pagamento
        [JsonIgnore]
        public Pagamento? Pagamento { get; set; }
    }
}