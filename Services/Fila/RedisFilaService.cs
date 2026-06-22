using System.Text.Json;
using StackExchange.Redis;

namespace EstacionamentoAPI.Services.Fila
{
    public class RedisFilaService : IFilaService
    {
        private readonly IConnectionMultiplexer _redis;
        private const string NomeFila = "fila:entradas";

        public RedisFilaService(IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(redisConnection);
        }

        public async Task PublicarEntradaRegistradaAsync(object mensagem)
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(mensagem);

            await db.ListLeftPushAsync(NomeFila, json);

            Console.WriteLine($"[FILA] Mensagem publicada na fila {NomeFila}: {json}");
        }
    }
}