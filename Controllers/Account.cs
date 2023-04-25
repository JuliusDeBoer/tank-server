using Models;

namespace Controllers
{
    public static class AccountEndpoints
    {
        public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/account");

            // TODO: Create a algorith to get unique id
            group.MapPost("/create", (string username, string email, string password) =>
            {
                if (username == null || email == null || password == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                if (Game.Accounts.ContainsKey(email))
                {
                    return Response.BadRequest(Response.ERR_ACCOUNT_EXISTS);
                }

                Game.CreateAccount(username, email, password);

                return TypedResults.Created($"/api/v1/account/login?email={email}&password={password}");
            })
            .WithName("CreateAccount");

            group.MapGet("/login", (string email, string password) =>
            {
                if (!Game.Accounts.ContainsKey(email))
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                Account account = Game.Accounts[email];

                if (account.Password == Game.Authenticator.Encrypt(password))
                {
                    JwtResult token = Game.Authenticator.CreateUserToken(email);
                    return TypedResults.Ok(token);
                }

                return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
            })
            .WithName("Login");
        }
    }
}
