using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Ocorrencia
    {
        [Key]
        public int IdOcorrencia { get; set; }
        public int IdAcesso { get; set; } // FK para o Ticket
        public DateTime DataHora { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}