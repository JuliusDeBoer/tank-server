using Tanks.Models;

namespace Tanks
{
    public static class Game
    {
        public static readonly Authenticator Authenticator = new();
        public static Dictionary<int, Tank> Tanks { get; set; } = new Dictionary<int, Tank>();
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
            foreach (KeyValuePair<int, Tank> pair in Tanks)
            {
                if (pair.Value.Health >= 1)
                {
                    pair.Value.ActionPoints++;
                }
            }

            int? result = Jury.GetWinner();

            if(result != null)
            {
                Tanks[(int)result].ActionPoints++;
            }

            Schedule();
        }

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
