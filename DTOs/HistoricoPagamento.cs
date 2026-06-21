public class HistoricoComPagamentoDto
{
    public int IdAcesso { get; set; }
    public required string Ticket { get; set; }
    public DateTime HoraEntrada { get; set; }
    public DateTime? HoraSaida { get; set; }
    public TimeSpan? TempoPermanencia { get; set; }

    // Dados de pagamento
    public decimal? ValorPago { get; set; }
    public string? FormaPagamento { get; set; }
    public string? StatusPagamento { get; set; }
    public DateTime? DataPagamento { get; set; }
}