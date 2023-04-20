using Tanks.Models;

namespace Tanks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.MapServerEndpoints();

            app.MapTankEndpoints();

            app.Run();
        }
    }
}