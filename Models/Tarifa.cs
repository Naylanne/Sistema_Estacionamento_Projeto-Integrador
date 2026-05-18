using System.ComponentModel.DataAnnotations;

namespace EstacionamentoAPI.Models
{
    public class Tarifa
    {
        [Key]
        public int IdTarifa { get; set; }
        public string TipoTarifa { get; set; } = "Padrao";
        public decimal ValorPrimeiraHora { get; set; }
        public decimal ValorHoraAdicional { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal DescontoParceiro { get; set; }
        public decimal DescontoFuncionario { get; set; }
    }
}