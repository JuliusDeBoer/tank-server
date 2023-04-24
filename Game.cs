using TankServer.Models;

namespace TankServer
{
    public static class Game
    {
        public static readonly Authenticator Authenticator = new();
        public static readonly Tanks Tanks = new();
        public static Dictionary<string, Account> Accounts { get; set; } = new Dictionary<string, Account>();

        public static readonly Jury Jury = new();

        public static void Schedule()
        {
            DateTime startTime = DateTime.Today.AddHours(12);
            if (startTime < DateTime.Now)
            {
                startTime = startTime.AddDays(1);
            }
            var timer = new Timer(Update, null, startTime - DateTime.Now, TimeSpan.FromDays(1));
        }

        public static void Update(object? state)
        {
            foreach (KeyValuePair<int, Tank> pair in Tanks.AllTanks)
            {
                if (pair.Value.Health >= 1)
                {
                    pair.Value.ActionPoints++;
                }
            }

            int? result = Jury.GetWinner();

            if(result != null)
            {
                Tanks.AllTanks[(int)result].ActionPoints++;
            }

            Schedule();
        }

        // Password must be hashed beforehand
        public static void CreateAccount(string username, string email, string password)
        {
            int id = Tanks.New();
            Accounts.Add(email, new Account(username, email, password, id));
        }

        public static int GetTankByPosition(Models.Position position)
        {
            foreach (KeyValuePair<int, Tank> pair in Tanks.AllTanks)
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
