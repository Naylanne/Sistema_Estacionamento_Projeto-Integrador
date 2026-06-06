using EstacionamentoAPI.Data;
using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Models;
using EstacionamentoAPI.Repositories;
using EstacionamentoAPI.Repositories.Interfaces;
using EstacionamentoAPI.Services;
using EstacionamentoAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao container.
builder.Services.AddControllers();
// Aprenda mais sobre configurar Swagger/OpenAPI em https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Configuração do PostgreSQL
// Pega a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EstacionamentoContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// 2. Configuração de CORS (Permite que o Frontend acesse a API sem bloqueios)
builder.Services.AddScoped<IAcessoRepository, AcessoRepository>();
builder.Services.AddScoped<IAcessoService, AcessoService>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();
builder.Services.AddScoped<ITarifaService, TarifaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IVagaRepository, VagaRepository>();
builder.Services.AddScoped<IVagaService, VagaService>();

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

// 3. Escopo para garantir criação do Banco (Opcional, mas bom para garantir)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<EstacionamentoContext>();
        // Se quiser garantir que o banco existe ao rodar:
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

// 4. Configuração para servir o Frontend (Pasta wwwroot)
app.UseDefaultFiles(); // Faz o sistema procurar por index.html, default.html, etc.
app.UseStaticFiles();  // Permite servir arquivos estáticos (CSS, JS, Imagens)

// 5. ATENÇÃO: Comentado para evitar o erro "Failed to determine the https port"
// app.UseHttpsRedirection(); 

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();