using Microsoft.EntityFrameworkCore;
using Iei.Wrappers;
using Iei.Services;
using Iei.Extractors;
using Iei.Repositories;
using Microsoft.OpenApi.Models;

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
                Title = "API de Castilla y León",
                Version = "v1",
                Description = "Esta es la documentación de la API de Castilla y Léón"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

        });

        builder.Services.AddScoped<ICLERepository, CLERepository>();

        builder.Services.AddScoped<ICLEService, CLEService>();
        builder.Services.AddScoped<GeocodingService>();

        builder.Services.AddScoped<CLEWrapper>();
        builder.Services.AddScoped<CLEExtractor>();

        



        var app = builder.Build();

        // Si estamos en desarrollo, habilitamos Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API de Castilla y León v1");
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
