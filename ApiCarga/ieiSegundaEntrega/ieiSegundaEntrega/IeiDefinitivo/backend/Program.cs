
using Iei;
using Iei.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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

        builder.Services.AddDbContext<IeiContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API de Carga",
                Version = "v1",
                Description = "Esta es la documentación de la API de Carga"
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);





        });


        builder.Services.AddHttpClient();
        builder.Services.AddScoped<ICargaService, CargaService>();

        builder.Services.Configure<MicroservicesOptions>(
            builder.Configuration.GetSection("Microservices"));

        var app = builder.Build();

        // Si estamos en desarrollo, habilitamos Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API IEI v1");
            });
        }

        // Aplicar CORS antes de UseAuthorization y UseEndpoints
        app.UseCors("AllowLocalhost");

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Habilitamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicación
        app.Run();
    }
}
