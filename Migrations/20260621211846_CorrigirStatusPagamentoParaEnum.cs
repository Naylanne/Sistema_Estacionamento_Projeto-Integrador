using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstacionamentoAPI.Migrations
{
    public partial class CorrigirStatusPagamentoParaEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:status_pagamento_enum", "cancelado,concluido,pago,pendente")
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");

            migrationBuilder.Sql("""
                ALTER TABLE pagamento
                ALTER COLUMN status_pagamento TYPE status_pagamento_enum
                USING (
                    CASE status_pagamento
                        WHEN 'Pendente' THEN 'pendente'
                        WHEN 'Pago' THEN 'pago'
                        WHEN 'Concluido' THEN 'concluido'
                        WHEN 'Concluído' THEN 'concluido'
                        WHEN 'Cancelado' THEN 'cancelado'
                        WHEN 'pendente' THEN 'pendente'
                        WHEN 'pago' THEN 'pago'
                        WHEN 'concluido' THEN 'concluido'
                        WHEN 'cancelado' THEN 'cancelado'
                        ELSE 'pendente'
                    END
                )::status_pagamento_enum;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE pagamento
                ALTER COLUMN status_pagamento TYPE text
                USING status_pagamento::text;
            """);

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .OldAnnotation("Npgsql:Enum:status_pagamento_enum", "cancelado,concluido,pago,pendente")
                .OldAnnotation("Npgsql:PostgresExtension:btree_gist", ",,");
        }
    }
}