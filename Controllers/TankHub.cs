using Microsoft.AspNetCore.SignalR;
using Models;

namespace Controllers
{
    public static class TankHubEndpoints
    {
        public static void MapTankHubEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/hub");

            group.MapHub<TankHub>("/a");

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapHub<TankHub>("/api/v1/hub");
            //});
        }
    }

    public class TankHub : Hub
    {
        public void MoveTank(int id, int x, int y)
        {
            Game.Tanks.Move(id, x, y);
        }

        public void Scream()
        {
            Log.Info("AAAAAHHH!");
        }
    }
}
