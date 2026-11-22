using GAC.Application.Contracts;
using GAC.Infrastructure.Data;
using GAC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
namespace GAC.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var connection = builder.Configuration.GetConnectionString("GacDb")
    ?? throw new InvalidOperationException("Configura ConnectionStrings:GacDb.");

            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddScoped<IConsultaInformesTecnicos, ConsultaInformesTecnicos>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
