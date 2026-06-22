using StackExchange.Redis;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const string redisConnection = "localhost:6379";
const string nomeFila = "fila:entradas";

Console.WriteLine("[WORKER] Worker de processamento iniciado.");
Console.WriteLine($"[WORKER] Conectando ao Redis em {redisConnection}...");
Console.WriteLine($"[WORKER] Aguardando mensagens na fila {nomeFila}.");
Console.WriteLine();

var redis = await ConnectionMultiplexer.ConnectAsync(redisConnection);
var db = redis.GetDatabase();

while (true)
{
    var mensagem = await db.ListRightPopAsync(nomeFila);

    if (mensagem.HasValue)
    {
        Console.WriteLine("[WORKER] Mensagem recebida da fila Redis:");
        Console.WriteLine(mensagem.ToString());

        Console.WriteLine("[WORKER] Processando tarefa em background...");
        await Task.Delay(2000);

        Console.WriteLine("[WORKER] Tarefa processada com sucesso.");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("[WORKER] Nenhuma mensagem na fila. Aguardando...");
        await Task.Delay(3000);
    }
}