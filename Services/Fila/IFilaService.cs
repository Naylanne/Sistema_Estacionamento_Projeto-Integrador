namespace EstacionamentoAPI.Services.Fila
{
    public interface IFilaService
    {
        Task PublicarEntradaRegistradaAsync(object mensagem);
    }
}