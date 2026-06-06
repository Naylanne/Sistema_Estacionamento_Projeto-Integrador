using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EstacionamentoAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDevelopment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.CreateTable(
                name: "aviso",
                columns: table => new
                {
                    id_aviso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aviso", x => x.id_aviso);
                });

            migrationBuilder.CreateTable(
                name: "tarifa",
                columns: table => new
                {
                    id_tarifa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_tarifa = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor_primeira_hora = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    valor_hora_adicional = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    valor_diaria = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    desconto_parceiro = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    desconto_funcionario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tarifa", x => x.id_tarifa);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_usuario = table.Column<string>(type: "text", nullable: false),
                    cpf = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    data_nascimento = table.Column<DateTime>(type: "date", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "char(11)", maxLength: 11, nullable: false),
                    endereco = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    senha_acesso = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id_usuario);
                    table.CheckConstraint("CK_Usuario_Cpf", "cpf ~ '^[0-9]{11}$'");
                    table.CheckConstraint("CK_Usuario_DataNascimento", "data_nascimento <= CURRENT_DATE");
                });

            migrationBuilder.CreateTable(
                name: "vaga",
                columns: table => new
                {
                    id_vaga = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_vaga = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaga", x => x.id_vaga);
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    id_feedback = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    data_envio = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedback", x => x.id_feedback);
                    table.ForeignKey(
                        name: "fk_feedback_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "veiculo",
                columns: table => new
                {
                    id_veiculo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    placa = table.Column<string>(type: "char(7)", maxLength: 7, nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    tipo_veiculo = table.Column<string>(type: "text", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_veiculo", x => x.id_veiculo);
                    table.CheckConstraint("CK_Veiculo_Placa", "placa ~ '^[A-Za-z0-9]{7}$'");
                    table.ForeignKey(
                        name: "fk_veiculo_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "acesso_veiculo",
                columns: table => new
                {
                    id_acesso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticket = table.Column<string>(type: "text", nullable: false),
                    id_tarifa = table.Column<int>(type: "integer", nullable: false),
                    id_vaga = table.Column<int>(type: "integer", nullable: false),
                    id_veiculo = table.Column<int>(type: "integer", nullable: false),
                    hora_entrada = table.Column<DateTime>(type: "timestamp", nullable: false),
                    hora_saida = table.Column<DateTime>(type: "timestamp", nullable: true),
                    tempo_permanencia = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_acesso_veiculo", x => x.id_acesso);
                    table.CheckConstraint("CK_HoraSaida_Maior_Entrada", "hora_saida IS NULL OR hora_saida >= hora_entrada");
                    table.ForeignKey(
                        name: "fk_acesso_veiculo_tarifa_id_tarifa",
                        column: x => x.id_tarifa,
                        principalTable: "tarifa",
                        principalColumn: "id_tarifa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_acesso_veiculo_vaga_id_vaga",
                        column: x => x.id_vaga,
                        principalTable: "vaga",
                        principalColumn: "id_vaga",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_acesso_veiculo_veiculo_id_veiculo",
                        column: x => x.id_veiculo,
                        principalTable: "veiculo",
                        principalColumn: "id_veiculo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ocorrencia",
                columns: table => new
                {
                    id_ocorrencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_acesso = table.Column<int>(type: "integer", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ocorrencia", x => x.id_ocorrencia);
                    table.ForeignKey(
                        name: "fk_ocorrencia_acesso_veiculo_id_acesso",
                        column: x => x.id_acesso,
                        principalTable: "acesso_veiculo",
                        principalColumn: "id_acesso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagamento",
                columns: table => new
                {
                    id_pagamento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_acesso = table.Column<int>(type: "integer", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp", nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    forma_pagamento = table.Column<string>(type: "text", nullable: false),
                    status_pagamento = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamento", x => x.id_pagamento);
                    table.ForeignKey(
                        name: "fk_pagamento_acesso_veiculo_id_acesso",
                        column: x => x.id_acesso,
                        principalTable: "acesso_veiculo",
                        principalColumn: "id_acesso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "tarifa",
                columns: new[] { "id_tarifa", "desconto_funcionario", "desconto_parceiro", "tipo_tarifa", "valor_diaria", "valor_hora_adicional", "valor_primeira_hora" },
                values: new object[] { 1, 0.00m, 0.00m, "Padrao", 50.00m, 5.00m, 10.00m });

            migrationBuilder.InsertData(
                table: "vaga",
                columns: new[] { "id_vaga", "status", "tipo_vaga" },
                values: new object[,]
                {
                    { 1, "Disponivel", "Carro" },
                    { 2, "Disponivel", "Carro" },
                    { 3, "Disponivel", "Moto" },
                    { 4, "Disponivel", "Moto" },
                    { 5, "Disponivel", "Caminhonete" },
                    { 6, "Disponivel", "Caminhonete" },
                    { 7, "Disponivel", "Caminhao" },
                    { 8, "Disponivel", "Caminhao" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_id_tarifa",
                table: "acesso_veiculo",
                column: "id_tarifa");

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_id_vaga",
                table: "acesso_veiculo",
                column: "id_vaga");

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_id_veiculo",
                table: "acesso_veiculo",
                column: "id_veiculo");

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_ticket",
                table: "acesso_veiculo",
                column: "ticket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_id_usuario",
                table: "feedback",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_ocorrencia_id_acesso",
                table: "ocorrencia",
                column: "id_acesso");

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_id_acesso",
                table: "pagamento",
                column: "id_acesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_cpf",
                table: "usuario",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_veiculo_id_usuario",
                table: "veiculo",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_veiculo_placa",
                table: "veiculo",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aviso");

            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "ocorrencia");

            migrationBuilder.DropTable(
                name: "pagamento");

            migrationBuilder.DropTable(
                name: "acesso_veiculo");

            migrationBuilder.DropTable(
                name: "tarifa");

            migrationBuilder.DropTable(
                name: "vaga");

            migrationBuilder.DropTable(
                name: "veiculo");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
