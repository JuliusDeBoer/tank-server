using TankServer.Controllers;

namespace TankServer.Models
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
            Password = Game.Authenticator.Encrypt(password);
            TankId = tankId;
        }
    }
}
