namespace Models
{
    public class TankTotal
    {
        public int Total { get; set; }
        public List<Tank> Tanks { get; set; } = new List<Tank>();

        public TankTotal(Dictionary<int, Tank> tanks)
        {
            foreach (KeyValuePair<int, Tank> pair in tanks)
            {
                if (pair.Value.Health >= 1)
                {
                    Tanks.Add(pair.Value);
                    Total++;
                }
            }
        }
    }

    public enum Color
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        White,
        Hotpink // Really cool and secret!
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Tanks
    {
        public static readonly int MAX_TANKS = 255;
        public static readonly int MAX_LEVEL = 3;

        public static Color? ParseColor(string color)
        {
            return color.ToLower() switch
            {
                "red" => Color.Red,
                "orange" => Color.Orange,
                "yellow" => Color.Yellow,
                "green" => Color.Green,
                "blue" => Color.Blue,
                "purple" => Color.Purple,
                "white" => Color.White,
                "hotpink" => Color.Hotpink,
                _ => null
            };
        }

        public const int MOVEMENT_RANGE = 4;

        public Dictionary<int, Tank> AllTanks = new();

        public int GetUniqueId()
        {
            int id = 1;
            while (AllTanks.ContainsKey(id))
            {
                id++;
            }

            return id;
        }

        public int New()
        {
            return Add(new Tank(GetUniqueId()));
        }

        public int Add(Tank tank)
        {
            AllTanks.Add(tank.Id, tank);
            return tank.Id;
        }

        public bool Move(int tankId, int x, int y)
        {
            Tank tank = AllTanks[tankId];

            if (!((MOVEMENT_RANGE * -1) <= x && x <= MOVEMENT_RANGE && (MOVEMENT_RANGE * -1) <= y && y <= MOVEMENT_RANGE))
            {
                return false;
            }

            tank.Position.X += x;
            tank.Position.Y += y;

            return true;
        }

        public bool Contains(int id)
        {
            return AllTanks.ContainsKey(id);
        }

        public bool HasActionPoints(int id, int amount)
        {
            return AllTanks[id].ActionPoints >= amount;
        }

        public void SpendActionPoints(int id, int amount)
        {
            AllTanks[id].ActionPoints -= amount;
        }

        internal void GiveActionPoints(int from, int to, int amount)
        {
            AllTanks[from].ActionPoints -= amount;
            AllTanks[to].ActionPoints += amount;
        }
    }

    public class Tank
    {
        public int Id { get; set; }
        public int Health { get; set; } = 3;
        public int Level { get; set; } = 1;
#if DEBUG
        public int ActionPoints { get; set; } = 10;
#else
        public int ActionPoints { get; set; } = 0;
#endif
        public Color Color { get; set; } = Color.Green;
        // Only use if MOVEMENT_RANGE should be ignored. Otherwise use Move()
        public Position Position { get; set; } = new(0, 0);

        public Tank(int id)
        {
            Id = id;
        }
    }
}
