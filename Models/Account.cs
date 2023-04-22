using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tanks.Models
{
    public class Account
    {
        public string Username;
        public string Email;
        public string Password;
        public int TankId;

        public Account(string username, string email, string password, int tankId)
        {
            Username = username;
            Email = email;
            Password = password;
            TankId = tankId;
        }
    }

    static class JwtAuthenticator
    {
        private const string Secret = "Conjuror1-Antivirus-Breeze-Gliding-Eats"; // DEBUG! REMOVE IN PRODUCTION
        private static readonly SymmetricSecurityKey SecretKey = new(Encoding.ASCII.GetBytes(Secret));

        private static readonly JwtSecurityTokenHandler Handler = new();

        public static string CreateUserToken(string email)
        {
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("email", email)
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
            group.MapPost("/create", (string username, string email, string password) =>
            {
                if(Game.Accounts.ContainsKey(email))
                {
                    return Response.BadRequest(Response.ERR_ACCOUNT_EXISTS);
                }

                string encrypted = PasswordEnrypt(password);
                Game.CreateAccount(username, email, encrypted);

                string token = JwtAuthenticator.CreateUserToken(email);
                return (IResult)TypedResults.Created("/api/v1/account/login");
            })
            .WithName("CreateAccount");

            group.MapGet("/login", (string email, string password) =>
            {
                if (!Game.Accounts.ContainsKey(email))
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                Account account = Game.Accounts[email];

                if (account.Password == PasswordEnrypt(password))
                {
                    string token = JwtAuthenticator.CreateUserToken(email);
                    return (IResult)TypedResults.Ok(token);
                }

                return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
            })
            .WithName("Login");
        }
    }
}
