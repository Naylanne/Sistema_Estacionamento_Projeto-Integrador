using NpgsqlTypes;

namespace EstacionamentoAPI.Enums
{
    public enum StatusPagamentoEnum
    {
        [PgName("pendente")]
        Pendente,

        [PgName("pago")]
        Pago,

        [PgName("concluido")]
        Concluido,

        [PgName("cancelado")]
        Cancelado
    }
}