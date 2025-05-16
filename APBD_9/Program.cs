using APBD_9.Middlewares;
using APBD_9.Repositories;
using APBD_9.Services;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace APBD_9;

public class Program
{
    public static void Main(string[] args)
    {
        //http://localhost:5031/swagger/index.html
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        
        //Rejestrowanie zależności
        builder.Services.AddScoped<IWarehouseService, WarehouseService>();
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",new OpenApiInfo
            {
                Title = "APBD_9",
                Version = "v1",
                Description = "APBD_9",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com",
                    Url = new Uri("https://example.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
        });
        
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseGlobalExceptionHandlingMiddleware();
        
        app.UseSwagger();
        
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "APBD_9");
            c.DocExpansion(DocExpansion.List);
            c.DefaultModelsExpandDepth(0);
            c.DisplayRequestDuration();
            c.EnableFilter();
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        
        app.Run();
    }
}