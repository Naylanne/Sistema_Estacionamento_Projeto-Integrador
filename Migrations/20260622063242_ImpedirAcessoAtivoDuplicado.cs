using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstacionamentoAPI.Migrations
{
    /// <inheritdoc />
    public partial class ImpedirAcessoAtivoDuplicado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_acesso_veiculo_id_veiculo",
                table: "acesso_veiculo");

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_ativo_unico",
                table: "acesso_veiculo",
                column: "id_veiculo",
                unique: true,
                filter: "hora_saida IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_acesso_veiculo_ativo_unico",
                table: "acesso_veiculo");

            migrationBuilder.CreateIndex(
                name: "ix_acesso_veiculo_id_veiculo",
                table: "acesso_veiculo",
                column: "id_veiculo");
        }
    }
}