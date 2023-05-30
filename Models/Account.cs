namespace Models
{
    public class Account
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int TankId { get; set; }

        public Account(string email, string password, int tankId)
        {
            Email = email;
            Password = Game.Authenticator.Encrypt(password);
            TankId = tankId;
        }
    }
}
