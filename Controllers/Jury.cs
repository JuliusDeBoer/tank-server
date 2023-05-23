using Models;

namespace Controllers
{

    public static class JuryEndPoints
    {
        public static void MapJuryEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/jury");

            group.MapPost("/vote", (HttpContext context, int candidate) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                Tank tank = Game.Tanks.AllTanks[account.TankId];

                if (tank.Health >= 1)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Game.Jury.Vote(account.TankId, candidate);

                return Response.Ok(Response.OK);
            })
            .WithName("GetJury");
        }
    }
}
