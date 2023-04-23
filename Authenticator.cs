using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;

namespace Tanks
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
        public string Secret = "Conjuror1-Antivirus-Breeze-Gliding-Eats"; // DEBUG! REMOVE IN PRODUCTION
        private SymmetricSecurityKey SecretKey = new(new byte[] { 6, 9, 4, 2, 0 }); // VERY SECURE! DO NOT USE PLS!!!!

        private readonly JwtSecurityTokenHandler Handler = new();

        public void SetSecret(string secret)
        {
            Secret = secret;
            SecretKey = new(Encoding.ASCII.GetBytes(Secret));
        }

        public Authenticator()
        {
            SecretKey = new(Encoding.ASCII.GetBytes(Secret));
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
