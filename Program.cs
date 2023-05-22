using Models;
using Controllers;

namespace TankServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.MapControllers();

            app.MapServerEndpoints();
            app.MapAccountEndpoints();
            app.MapTankEndpoints();
            app.MapJuryEndpoints();

            app.MapHub<TankHub>("/hub");

            Game.Schedule();

            app.Run();
        }
    }
}