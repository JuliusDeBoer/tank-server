namespace Tanks.Models
{
    public class ServerInfo
    {
        private static ServerInfo? _serverInfo;
        public string Version { get; } = "1.0";

        public static ServerInfo GetInfo()
        {
            _serverInfo = new ServerInfo();
            return (ServerInfo)_serverInfo;
        }
    }

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
