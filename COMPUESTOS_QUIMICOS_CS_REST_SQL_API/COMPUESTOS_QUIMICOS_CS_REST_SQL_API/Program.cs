using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Repositories;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DBContext
builder.Services.AddSingleton<PgsqlDbContext>();

//Los repositorios
//builder.Services.AddScoped<IResumenRepository, ResumenRepository>();
//builder.Services.AddScoped<IElementoRepository, ElementoRepository>();
builder.Services.AddScoped<ICompuestoRepository, CompuestoRepository>();

//servicios asociados para cada ruta
//builder.Services.AddScoped<ResumenService>();
//builder.Services.AddScoped<ElementoService>();
builder.Services.AddScoped<CompuestoService>();


// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Compuestos Quimicos - Versión en PostgreSQL",
        Description = "API para la gestión de Información sobre compuestos quimicos"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Modificamos el encabezado de las peticiones para ocultar el web server utilizado
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Server", "QuimicoServer");
    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
