using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstacionamentoAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        public string TipoUsuario { get; set; } = string.Empty; // Visitante, Funcionario, Parceiro

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 números.")]
        [Column(TypeName = "char(11)")]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime DataNascimento { get; set; }

        [Required]
        public string Cargo { get; set; } = string.Empty; // Atendente, Gerente

        [Required]
        [StringLength(11, MinimumLength = 11)]
        [Column(TypeName = "char(11)")]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Endereco { get; set; } = string.Empty;

        [Required]
        public string SenhaAcesso { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Veiculo>? Veiculos { get; set; }

        [Timestamp]
        public uint RowVersion { get; set; }
    }
}