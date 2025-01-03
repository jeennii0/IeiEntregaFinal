using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Iei.Wrappers;
using Iei.Services;
using Iei.Extractors;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Agregar configuración de CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // URL del frontend
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Configuración de la base de datos PostgreSQL
        builder.Services.AddDbContext<IeiContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        builder.Services.AddControllers();

        // Configuración de Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Servicios inyectados
        builder.Services.AddScoped<MonumentoService>();
        builder.Services.AddScoped<CLEService>();
        builder.Services.AddScoped<CVService>();
        builder.Services.AddScoped<EUSService>();

        builder.Services.AddTransient<CLEExtractor>();
        builder.Services.AddTransient<CVExtractor>();
        builder.Services.AddTransient<EUSExtractor>();

        var app = builder.Build();

        // Si estamos en desarrollo, habilitamos Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Habilitar CORS
        app.UseCors("AllowLocalhost");

        // Configuración de redirección y autorización
        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Ejecutamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicación
        app.Run();
    }
}
