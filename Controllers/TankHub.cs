using Microsoft.AspNetCore.SignalR;
using Models;

namespace Controllers
{
    public class MasterHub : Hub
    {
        // === SERVER START ===

        public ServerInfo GetServerInfo()
        {
            return ServerInfo.GetInfo();
        }

        // === SERVER END ===

        // === TANK START ===

        public TankTotal GetTanks()
        {
            Log.Info("Requested all tanks");
            return new TankTotal(Game.Tanks.AllTanks);
        }

        public Tank? GetTank(int id)
        {
            if (!Game.Tanks.Contains(id))
            {
                Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                return null;
            }
            Log.Info($"Got tank {id}");
            return Game.Tanks.AllTanks[id];
        }

        // TODO: Return a value based on if the action succeeded
        public void MoveTank(string auth, int x, int y)
        {
            Account? account = Game.Authenticator.GetUser(auth);
            if(account == null)
            {
                return;
            }

            if (!Game.Tanks.Contains(account.TankId))
            {
                return;
            }

            Game.Tanks.Move(account.TankId, x, y);
        }

        public void SetColor(string auth, string color)
        {
            Account? account = Game.Authenticator.GetUser(auth);
            if (account == null)
            {
                return;
            }

            int id = account.TankId;

            if (!Game.Tanks.Contains(id))
            {
                return;
            }

            Color? parsed = TankCollection.ParseColor(color);

            if (parsed == null)
            {
                return;
            }

            Game.Tanks.AllTanks[id].Color = (Color)parsed;
        }

        public void Shoot(string auth, int target)
        {
            Account? account = Game.Authenticator.GetUser(auth);
            if (account == null)
            {
                return;
            }

            int id = account.TankId;

            if(!Game.Tanks.Contains(id) || !Game.Tanks.Contains(target) || id == target)
            {
                return;
            }

            // TODO: Move this to seperate function
            Tank origin = Game.Tanks.AllTanks[id];
            Tank dest = Game.Tanks.AllTanks[target];

            if (origin.ActionPoints <= 0)
            {
                return;
            }

            if (dest.Health <= 0)
            {
                return;
            }

            // TODO: Add funcion for this
            origin.ActionPoints--;
            dest.Health--;
        }

        public void Upgrade(string auth)
        {
            Account? account = Game.Authenticator.GetUser(auth);
            if (account == null)
            {
                return;
            }

            int id = account.TankId;

            if (!Game.Tanks.Contains(id))
            {
                return;
            }

            Tank tank = Game.Tanks.AllTanks[id];

            // TODO: Move this to seperate function
            if (tank.ActionPoints <= 0)
            {
                return;
            }

            if (tank.Level >= TankCollection.MAX_LEVEL)
            {
                return;
            }

            if (!Game.Tanks.HasActionPoints(id, 1))
            {
                return;
            }

            Game.Tanks.SpendActionPoints(id, 1);

            tank.Level++;
        }

        public void Give(string auth, int amount, int target)
        {
            Account? account = Game.Authenticator.GetUser(auth);
            if (account == null)
            {
                return;
            }

            int id = account.TankId;

            if (amount <= 0){ return; }
            if (!Game.Tanks.Contains(id)) { return; }
            if (!Game.Tanks.Contains(target)) { return; }

            Tank origin = Game.Tanks.AllTanks[id];
            Tank destination = Game.Tanks.AllTanks[target];

            if (origin.Health <= 0) { return; }
            if (destination.Health <= 0) { return; }
            if (!Game.Tanks.HasActionPoints(id, amount)) { return; }

            Game.Tanks.GiveActionPoints(id, target, amount);
        }

        public int GetMyTankId(string auth)
        {

            Account? account = Game.Authenticator.GetUser(auth);
            if (account == null)
            {
                return -1;
            }

            return account.TankId;
        }

        // === TANK END ===

        // === ACCOUNT START ===

        public void CreateAccount(string username, string email, string password)
        {
            if (Game.Accounts.ContainsKey(email))
            {
                return;
            }

            Game.CreateAccount(username, email, password);
        }

        public string? Login(string email, string password)
        {
            if(email == null)
            {
                return null;
            }

            if(password == null)
            {
                return null;
            }

            if (!Game.Accounts.ContainsKey(email))
            {
                return null;
            }

            Account account = Game.Accounts[email];

            if (account.Password == Game.Authenticator.Encrypt(password))
            {
                JwtResult token = Game.Authenticator.CreateUserToken(email);
                return token.Token;
            }

            return null;
        }

        // === ACCOUNT END ===

        // === JURY START ===

        public void Vote(string auth, int candidate)
        {
            Account? account = Game.Authenticator.GetUser(auth);

            if (account == null)
            {
                return;
            }

            Tank tank = Game.Tanks.AllTanks[account.TankId];

            if(tank.Health >= 1)
            {
                return;
            }

            Game.Jury.Vote(account.TankId, candidate);
        }

        // == JURY END ===
    }
}
