using TankServer.Models;

namespace TankServer.Controllers
{
    public static class ServerEndPoints
    {
        public static void MapServerEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1");

            group.MapGet("/", () =>
            {
                Log.Info("Requested server info");
                return (IResult)TypedResults.Ok(ServerInfo.GetInfo());
            })
            .WithName("GetServerInfo");
        }
    }
}
