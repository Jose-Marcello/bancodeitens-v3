using BancoDeItens.Application.Services;
using BancoDeItens.Domain.Interfaces;
using BancoDeItensWebApi.Extensions;
using BancoItens.Application.Interface;
using BancoItens.Infrastructure.Data;
using BancoItens.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DE SERVIÇOS INICIAIS ===

// 🛑 REGISTRO DO MVC E FLUENTVALIDATION
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    options.Filters.Add(new ProducesAttribute("application/json"));
});

// 🟢 REGISTRO DA INJEÇÃO DE DEPENDÊNCIA
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<IQuestaoRepository, QuestaoRepository>();
builder.Services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
builder.Services.AddScoped<IQuestaoService, QuestaoService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy.WithOrigins(
            "https://polite-dune-053c7490f.3.azurestaticapps.net",
            "app.palpitesbolao.com.br", // Para testes locais
            "http://localhost:4200" // Para testes locais
        )
        .AllowAnyMethod()
        .AllowAnyHeader());
});


// 🛑 CORS: Totalmente permissivo para comunicação entre Azure SWA (Frontend) e Azure ACA (Backend)
/*
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
*/

// === CONFIGURAÇÃO DO DBCONTEXT (POSTGRESQL) ===

// 💡 CÓDIGO DO RAILWAY COMENTADO, MAS MANTIDO PARA REFERÊNCIA:
// var railwayConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1. Tenta ler a Connection String do Azure (formato tradicional) ou 'DefaultConnection'
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Tenta ler a Connection String do Container Apps (ACA) / Variável de Ambiente (ex: DATABASE_URL)
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration["DATABASE_URL"];
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("A Connection String 'DefaultConnection' ou 'DATABASE_URL' não foi encontrada.");
}

// 🛑 TRATAMENTO DA CONNECTION STRING (MANTIDO PARA COMPATIBILIDADE COM FORMATO URL)
// Converte a URL PostgreSQL (ex: railway/heroku) para o formato chave/valor
if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var match = Regex.Match(connectionString,
        @"postgresql://(?<user>[^:]+):(?<password>[^@]+)@(?<host>[^:]+):(?<port>\d+)/(?<database>.+)");

    if (match.Success)
    {
        connectionString = $"Host={match.Groups["host"].Value};" +
                           $"Port={match.Groups["port"].Value};" +
                           $"Username={match.Groups["user"].Value};" +
                           $"Password={match.Groups["password"].Value};" +
                           $"Database={match.Groups["database"].Value}";
    }
}
// FIM DO TRATAMENTO


builder.Services.AddDbContext<BancoDeItensContext>(options =>
{
    options.UseNpgsql(connectionString,
        npgsqlOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
            );
        })
        .LogTo(Console.WriteLine, LogLevel.Information);
});


var app = builder.Build();

// 🛑 BLOCo DE MIGRATIONS: Executa a aplicação da Migration
app.ApplyMigrations();

// === CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÃO HTTP ===

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();