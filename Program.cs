using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using wsapi.Context;

var builder = WebApplication.CreateBuilder(args);

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", corsBuilder =>
    {
        corsBuilder.WithOrigins(builder.Configuration["CORS:AllowedOrigins"]);
        corsBuilder.AllowAnyMethod();
        corsBuilder.AllowAnyHeader();
        corsBuilder.AllowCredentials();
    });
});

// Adiciona controllers
builder.Services.AddControllers();

// Obtém a string de conexão
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("A string de conexão não foi encontrada no appsettings.json.");
}

// Configuração do banco de dados MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Configuração do Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LeapCertService",
        Version = "v1",
        Description = "API do LeapCert muito froggers.",
    });
});

var app = builder.Build();

// Middleware do Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LeapCert v1");
    c.RoutePrefix = string.Empty;
});

// Middleware do CORS
app.UseCors("CorsPolicy");

// Habilita WebSockets
app.UseWebSockets();

// Mapeia os controllers
app.MapControllers();

// Executa a aplicação
await app.RunAsync();