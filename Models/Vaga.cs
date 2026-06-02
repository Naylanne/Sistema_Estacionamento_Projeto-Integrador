using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstacionamentoAPI.Models
{
    public class Vaga
    {
        [Key]
        public int IdVaga { get; set; }

        [Required]
        [StringLength(15)]
        [Column(TypeName = "varchar(15)")]
        public string TipoVaga { get; set; } = "Carro"; // Carro, Moto

        [Required]
        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string Status { get; set; } = "Disponivel"; // Disponivel, Ocupada
        
    }
}