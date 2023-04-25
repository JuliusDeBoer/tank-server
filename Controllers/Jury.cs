using Models;

namespace Controllers
{

    public static class JuryEndPoints
    {
        public static void MapJuryEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/jury");

            group.MapPost("/vote", (HttpContext context) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                return (IResult)TypedResults.Ok(account);
            })
            .WithName("GetJury");
        }
    }
}
