namespace Tanks.Models
{
    public class Account
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int TankId { get; set; }

        public Account(string username, string email, string password, int tankId)
        {
            Username = username;
            Email = email;
            Password = password;
            TankId = tankId;
        }
    }

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

                string encrypted = Game.Authenticator.Encrypt(password);
                Game.CreateAccount(username, email, encrypted);

                JwtResult token = Game.Authenticator.CreateUserToken(email);
                return (IResult)TypedResults.Created($"/api/v1/account/login?email={email}&password={password}");
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
                    return (IResult)TypedResults.Ok(token);
                }

                return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
            })
            .WithName("Login");
        }
    }
}
