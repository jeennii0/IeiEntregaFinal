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

        // Agregar configuraci�n de CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // URL del frontend
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Configuraci�n de la base de datos PostgreSQL
        builder.Services.AddDbContext<IeiContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        builder.Services.AddControllers();

        // Configuraci�n de Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API de B�squeda",
                Version = "v1",
                Description = "Esta es la documentaci�n de la API de B�squeda"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

        });

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
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API de B�squeda v1");
            });
        }

        // Habilitar CORS
        app.UseCors("AllowLocalhost");

        // Configuraci�n de redirecci�n y autorizaci�n
        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Ejecutamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicaci�n
        app.Run();
    }
}
