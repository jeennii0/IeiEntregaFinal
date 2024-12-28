using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Iei.Wrappers;
using Iei.Services;
using Iei.Extractors;
using Iei.Repositories;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<IeiContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<CVWrapper>();
        builder.Services.AddScoped<CVExtractor>();
        builder.Services.AddScoped<GeocodingService>();
        builder.Services.AddScoped<ICVService, CVService>();
        builder.Services.AddScoped<ICVRepository, CVRepository>();



        var app = builder.Build();

        // Si estamos en desarrollo, habilitamos Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Habilitamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicaci�n
        app.Run();
    }
}
