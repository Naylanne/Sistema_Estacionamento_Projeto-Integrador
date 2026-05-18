using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class AcessoVeiculo
    {
        [Key]
        public int IdAcesso { get; set; }
        public int IdVaga { get; set; }
        public string Placa { get; set; } = string.Empty;
        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSaida { get; set; }
        public decimal ValorPago { get; set; }
        public string StatusPagamento { get; set; } = "Pendente";
        public string FormaPagamento { get; set; } = "Dinheiro"; 
    }
}