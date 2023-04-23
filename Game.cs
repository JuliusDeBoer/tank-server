using Tanks.Models;

namespace Tanks
{
    public static class Game
    {
        public static readonly Authenticator Authenticator = new();
        public static Dictionary<int, Models.Tank> Tanks { get; set; } = new Dictionary<int, Models.Tank>();
        public static Dictionary<string, Account> Accounts { get; set; } = new Dictionary<string, Account>();

        public static readonly Jury Jury = new();

        // Password must be hashed beforehand
        public static void CreateAccount(string username, string email, string password)
        {
            int id = Tanks.Count + 1;

            Tanks.Add(id, new Tank(id));
            Accounts.Add(email, new Account(username, email, password, id));
        }

        public static int GetTankByPosition(Models.Position position)
        {
            foreach (KeyValuePair<int, Tank> pair in Tanks)
            {
                if (pair.Value.Position == position)
                {
                    return pair.Key;
                }
            }

            return 0;
        }
    }
}
