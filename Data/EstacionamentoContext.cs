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
        
        // --- ADICIONADO RECENTEMENTE ---
        public DbSet<Ocorrencia> Ocorrencias { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Aviso> Avisos { get; set; }
        // ------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario");

                entity.HasKey(u => u.IdUsuario);

                // UNIQUE CPF
                entity.HasIndex(u => u.Cpf)
                      .IsUnique();

                // CHECK CPF = exatamente 11 números
                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Usuario_Cpf",
                        "cpf ~ '^[0-9]{11}$'"
                    ));

                // CHECK data nascimento <= data atual
                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Usuario_DataNascimento",
                        "data_nascimento <= CURRENT_DATE"
                    ));
            });
           
            modelBuilder.Entity<Tarifa>().HasData(new Tarifa 
            { 
                IdTarifa = 1, 
                TipoTarifa = "Padrao", 
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