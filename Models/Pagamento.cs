using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using EstacionamentoAPI.Enums;

namespace EstacionamentoAPI.Models
{
    public class Pagamento
    {
        [Key]
        public int IdPagamento { get; set; }

        // FK única para acesso
        public int IdAcesso { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime DataHora { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal ValorPago { get; set; }

        [Required]
        public string FormaPagamento { get; set; } = "Dinheiro";

        [Required]
        public StatusPagamentoEnum StatusPagamento { get; set; } = StatusPagamentoEnum.Pendente;

        [JsonIgnore] // Evita ciclo infinito no JSON
        public Acesso? Acesso { get; set; }

        [ConcurrencyCheck]
        public uint RowVersion { get; set; }
    }
}