using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

    static class JwtAuthenticator
    {
        private const string Secret = "Conjuror1-Antivirus-Breeze-Gliding-Eats"; // DEBUG! REMOVE IN PRODUCTION
        private static readonly SymmetricSecurityKey SecretKey = new(Encoding.ASCII.GetBytes(Secret));

        private static readonly JwtSecurityTokenHandler Handler = new();

        public static string CreateUserToken(int id)
        {
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", id.ToString())
                }),
                Expires = DateTime.MaxValue,
                SigningCredentials = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha512Signature)
            };

            SecurityToken token = Handler.CreateToken(descriptor);
            return Handler.WriteToken(token);
        }
    }

    public static class AccountEndpoints
    {
        private static string PasswordEnrypt(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: new byte[] { 6, 9, 4, 2, 0 }, // Very secure(Dont steal!)
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        }

        public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/account");

            // TODO: Create a algorith to get unique id
            group.MapPost("/create", (string username, string password) =>
            {
                string encrypted = PasswordEnrypt(password);
                int id = Game.Accounts.Count + 1;

                Game.Accounts.Add(id, new Account(username, password));

                string token = JwtAuthenticator.CreateUserToken(id);

                return (IResult)TypedResults.Created(token);
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
