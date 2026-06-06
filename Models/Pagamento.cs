using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public string StatusPagamento { get; set; } = "Pendente";

        [JsonIgnore]
        public Acesso? Acesso { get; set; }

        [Timestamp]
        public uint RowVersion { get; set; }
    }
}