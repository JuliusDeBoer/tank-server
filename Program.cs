using Models;
using Controllers;
using System.Configuration;

namespace TankServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            //builder.UseUrls("http://localhost:6666");

            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.

            app.MapControllers();

            app.MapServerEndpoints();
            app.MapAccountEndpoints();
            app.MapTankEndpoints();
            app.MapJuryEndpoints();

            app.MapHub<MasterHub>("/api/v1/hub");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            Game.Schedule();

            app.Run();
        }
    }
}