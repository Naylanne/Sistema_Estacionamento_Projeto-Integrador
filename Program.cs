using EstacionamentoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services;
using EstacionamentoAPI.Services.Interfaces;
using EstacionamentoAPI.Enums;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao container.
// Configura também para aceitar enums como texto no JSON.
// Exemplo: "statusPagamento": "Pago" em vez de "statusPagamento": 1
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
         // Converte enums para string
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Garante suporte a acentos e caracteres Unicode
        options.JsonSerializerOptions.Encoder = 
            JavaScriptEncoder.Create(UnicodeRanges.All);
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Configuração do PostgreSQL
// Pega a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EstacionamentoContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<StatusPagamentoEnum>("status_pagamento_enum");
        })
    .UseSnakeCaseNamingConvention());

// 2. Injeção de dependência - Repositories
builder.Services.AddScoped<IAcessoRepository, AcessoRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IVagaRepository, VagaRepository>();

// 3. Injeção de dependência - Services
builder.Services.AddScoped<IAcessoService, AcessoService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<ITarifaService, TarifaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IVagaService, VagaService>();

// 4. Configuração de CORS
// Permite que o frontend acesse a API sem bloqueios
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// 5. Escopo para testar conexão com o banco
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<EstacionamentoContext>();

        // Se quiser garantir que o banco existe ao rodar, pode usar migrations:
        // context.Database.Migrate();

        // Evite usar EnsureCreated() junto com migrations.
        // context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao conectar no banco: " + ex.Message);
    }
}

// Configura o pipeline de requisição HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6. Configuração para servir o frontend da pasta wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Comentado para evitar erro "Failed to determine the https port"
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();