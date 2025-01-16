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
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API de la Comunidad Valenciana",
                Version = "v1",
                Description = "Esta es la documentación de la API de la Comunidad Valenciana"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

        });

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
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API de Euskadi v1");
            });
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Habilitamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicación
        app.Run();
    }
}
