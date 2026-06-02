using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Tarifa
    {
        [Key]
        public int IdTarifa { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoTarifa { get; set; } = "Padrao";
        
        [Column(TypeName = "numeric(10,2)")]
        public decimal ValorPrimeiraHora { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal ValorHoraAdicional { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal ValorDiaria { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal DescontoParceiro { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal DescontoFuncionario { get; set; }
        
        [Timestamp]
        public uint RowVersion { get; set; }
    }
}