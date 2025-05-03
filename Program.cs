using System.Text;
using leapcert_back.Helpers;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using leapcert_back.Repository;
using leapcert_back.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Configuração do banco de dados SQL server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuração do Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LeapCertService",
        Version = "v1",
        Description = "API do LeapCert muito froggers.",
    });

    // 🔐 Configuração do esquema JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Insira o token JWT desta forma: **Bearer {seu token}**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

//Configuracao do JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IModulesRepository, ModulesRepository>();
builder.Services.AddScoped<HelperService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserMapper>();
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

app.UseAuthentication();
app.UseAuthorization();

// Executa a aplicação
await app.RunAsync();