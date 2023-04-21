using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Tanks.Models
{
    public class Account
    {
        public string Username;
        public string Password;

        public Account(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public static class AccountEndpoints
    {
        private static string PasswordEnrypt(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: new byte[] {6, 9, 4 ,2, 0}, // Very secure(Dont steal!)
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        }
        
        public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/account");

            group.MapPost("/create", (string username, string password) =>
            {
                string encrypted = PasswordEnrypt(password);
                Log.Info($"{username} with password {encrypted} wants to make an account");
                return (IResult)TypedResults.Ok(Response.OK);
            })
            .WithName("CreateAccount");

            group.MapGet("/login", (string username, string password) =>
            {
                return (IResult)TypedResults.Ok(Response.OK);
            })
            .WithName("Login");
        }
    }
}
