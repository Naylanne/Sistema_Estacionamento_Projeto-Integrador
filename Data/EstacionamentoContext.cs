using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Data
{
    public class EstacionamentoContext : DbContext
    {
        public EstacionamentoContext(DbContextOptions<EstacionamentoContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public DbSet<Acesso> Acessos { get; set; }
        
        // --- ADICIONADO RECENTEMENTE ---
        public DbSet<Ocorrencia> Ocorrencias { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Aviso> Avisos { get; set; }
        // ------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Extensão PostgreSQL
            modelBuilder.HasPostgresExtension("btree_gist");

            // ==================================================
            // USUARIO
            // ==================================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario");

                entity.HasKey(u => u.IdUsuario);

                entity.HasIndex(u => u.Cpf)
                    .IsUnique();

                // CORREÇÃO: Mapeamento do xmin para PostgreSQL
                entity.Property(u => u.RowVersion)
                      .HasColumnName("xmin")
                      .HasColumnType("xid")
                      .ValueGeneratedOnAddOrUpdate()
                      .IsConcurrencyToken();

                // CPF exatamente 11 números
                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Usuario_Cpf",
                        "cpf ~ '^[0-9]{11}$'"
                    ));

                // Data nascimento <= hoje
                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Usuario_DataNascimento",
                        "data_nascimento <= CURRENT_DATE"
                    ));
            });

            // ==================================================
            // TARIFA
            // ==================================================
            modelBuilder.Entity<Tarifa>(entity =>
            {
                entity.ToTable("tarifa");

                entity.HasKey(t => t.IdTarifa);

                // CORREÇÃO: Mapeamento do xmin para PostgreSQL
                entity.Property(t => t.RowVersion)
                      .HasColumnName("xmin")
                      .HasColumnType("xid")
                      .ValueGeneratedOnAddOrUpdate()
                      .IsConcurrencyToken();

                entity.HasData(
                    new Tarifa
                    {
                        IdTarifa = 1,
                        TipoTarifa = "Padrao",
                        ValorPrimeiraHora = 10.00m,
                        ValorHoraAdicional = 5.00m,
                        ValorDiaria = 50.00m,
                        DescontoFuncionario = 0.00m,
                        DescontoParceiro = 0.00m
                    }
                );
            });

            // ==================================================
            // VEICULO
            // ==================================================
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("veiculo");

                entity.HasKey(v => v.IdVeiculo);

                entity.Property(v => v.Placa)
                    .HasColumnName("placa")
                    .HasColumnType("char(7)")
                    .HasMaxLength(7)
                    .IsRequired();

                entity.HasIndex(v => v.Placa)
                    .IsUnique();

                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_Veiculo_Placa",
                        "placa ~ '^[A-Za-z0-9]{7}$'"
                    ));

                entity.HasOne(v => v.Usuario)
                    .WithMany(u => u.Veiculos)
                    .HasForeignKey(v => v.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================================================
            // VAGA
            // ==================================================
            modelBuilder.Entity<Vaga>(entity =>
            {
                entity.ToTable("vaga");

                entity.HasKey(v => v.IdVaga);

                entity.HasData(
                    new Vaga { IdVaga = 1, TipoVaga = "Carro", Status = "Disponivel" },
                    new Vaga { IdVaga = 2, TipoVaga = "Carro", Status = "Disponivel" },
                    new Vaga { IdVaga = 3, TipoVaga = "Moto", Status = "Disponivel" },
                    new Vaga { IdVaga = 4, TipoVaga = "Moto", Status = "Disponivel" },
                    new Vaga { IdVaga = 5, TipoVaga = "Caminhonete", Status = "Disponivel" },
                    new Vaga { IdVaga = 6, TipoVaga = "Caminhonete", Status = "Disponivel" },
                    new Vaga { IdVaga = 7, TipoVaga = "Caminhao", Status = "Disponivel" },
                    new Vaga { IdVaga = 8, TipoVaga = "Caminhao", Status = "Disponivel" }
                );
            });

            // ==================================================
            // ACESSO
            // ==================================================
            modelBuilder.Entity<Acesso>(entity =>
            {
                entity.ToTable("acesso_veiculo");

                entity.HasKey(a => a.IdAcesso);

                entity.HasIndex(a => a.Ticket)
                    .IsUnique();

                // Hora saída >= entrada
                entity.ToTable(t =>
                    t.HasCheckConstraint(
                        "CK_HoraSaida_Maior_Entrada",
                        "hora_saida IS NULL OR hora_saida >= hora_entrada"
                    ));

                entity.HasOne(a => a.Vaga)
                    .WithMany()
                    .HasForeignKey(a => a.IdVaga);

                entity.HasOne(a => a.Tarifa)
                    .WithMany()
                    .HasForeignKey(a => a.IdTarifa);

                entity.HasOne(a => a.Veiculo)
                    .WithMany()
                    .HasForeignKey(a => a.IdVeiculo);

                // CORREÇÃO: Mantido apenas este relacionamento 1:1 limpo (os outros duplicados foram removidos)
                entity.HasOne(a => a.Pagamento)
                    .WithOne(p => p.Acesso)
                    .HasForeignKey<Pagamento>(p => p.IdAcesso);
            });

            // ==================================================
            // PAGAMENTO
            // ==================================================
            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.ToTable("pagamento");

                entity.HasKey(p => p.IdPagamento);

                // CORREÇÃO: Mapeamento do xmin para PostgreSQL
                entity.Property(p => p.RowVersion)
                      .HasColumnName("xmin")
                      .HasColumnType("xid")
                      .ValueGeneratedOnAddOrUpdate()
                      .IsConcurrencyToken();

                entity.HasIndex(p => p.IdAcesso)
                      .IsUnique();
            });

            // ==================================================
            // FEEDBACK
            // ==================================================
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("feedback");

                entity.HasKey(f => f.IdFeedback);                

                entity.HasOne(f => f.Usuario)
                    .WithMany()
                    .HasForeignKey(f => f.IdUsuario);
            });

            // ==================================================
            // OCORRENCIA
            // ==================================================
            modelBuilder.Entity<Ocorrencia>(entity =>
            {
                entity.ToTable("ocorrencia");

                entity.HasKey(o => o.IdOcorrencia);

                entity.HasOne(o => o.Acesso)
                    .WithMany()
                    .HasForeignKey(o => o.IdAcesso);
            });

            // ==================================================
            // AVISO
            // ==================================================
            modelBuilder.Entity<Aviso>(entity =>
            {
                entity.ToTable("aviso");

                entity.HasKey(a => a.IdAviso);
            });
        }
    }
}