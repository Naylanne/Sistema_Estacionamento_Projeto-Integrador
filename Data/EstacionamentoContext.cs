using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Data
{
    public class EstacionamentoContext : DbContext
    {
        public EstacionamentoContext(DbContextOptions<EstacionamentoContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<AcessoVeiculo> Acessos { get; set; }
        
        // --- ADICIONADO AGORA ---
        public DbSet<Ocorrencia> Ocorrencias { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Aviso> Avisos { get; set; }
        // ------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed inicial para testes
            modelBuilder.Entity<Tarifa>().HasData(new Tarifa 
            { 
                IdTarifa = 1, 
                TipoTarifa = "Comum", 
                ValorPrimeiraHora = 10.00m, 
                ValorHoraAdicional = 5.00m, 
                ValorDiaria = 50.00m,
                DescontoFuncionario = 0.0m,
                DescontoParceiro = 0.0m
            });

            modelBuilder.Entity<Vaga>().HasData(
                new Vaga { IdVaga = 1, TipoVaga = "Carro", Status = "Disponivel" },
                new Vaga { IdVaga = 2, TipoVaga = "Carro", Status = "Disponivel" }
            );
        }
    }
}