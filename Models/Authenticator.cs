using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Models
{
    public class JwtResult
    {
        public string Token { get; set; }
        public JwtResult(string token)
        {
            Token = token;
        }
    }

    public class Authenticator
    {
        public string Secret;
        private SymmetricSecurityKey SecretKey;

        private readonly JwtSecurityTokenHandler Handler = new();
        private TokenValidationParameters validationParameters;

        public Authenticator()
        {
            Secret = "Conjuror1-Antivirus-Breeze-Gliding-Eats";
            SecretKey = new(Encoding.ASCII.GetBytes(Secret));
            validationParameters = new()
            {
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = SecretKey
            };
        }

        public void SetSecret(string secret)
        {
            Secret = secret;
            SecretKey = new(Encoding.ASCII.GetBytes(Secret));
            validationParameters = new()
            {
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = SecretKey
            };
        }

        public bool IsValid(string jwt)
        {
            // For some reason when the token is not valid it throws an exception
            try
            {
                Handler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public Account? GetUser(IHeaderDictionary headers)
        {
            string? token = headers["Authorization"];

            if (token == null)
            {
                return null;
            }

            if (!IsValid(token))
            {
                return null;
            }

            JwtSecurityToken jwt = new(token);

            string email = (string)jwt.Payload["email"];

            if (!Game.Accounts.ContainsKey(email))
            {
                return null;
            }

            return Game.Accounts[email];
        }

        public JwtResult CreateUserToken(string email)
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
            return new(Handler.WriteToken(token));
        }

        public string Encrypt(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: new byte[] { 6, 9, 4, 2, 0 }, // Very secure(Dont steal!)
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
        }
    }
}
