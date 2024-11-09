using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DbContexts;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Repositories;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DBContext
builder.Services.AddSingleton<PgsqlDbContext>();

//Los repositorios
builder.Services.AddScoped<IResumenRepository, ResumenRepository>();
builder.Services.AddScoped<IElementoRepository, ElementoRepository>();
builder.Services.AddScoped<ICompuestoRepository, CompuestoRepository>();

//servicios asociados para cada ruta
builder.Services.AddScoped<ResumenService>();
builder.Services.AddScoped<ElementoService>();
builder.Services.AddScoped<CompuestoService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
