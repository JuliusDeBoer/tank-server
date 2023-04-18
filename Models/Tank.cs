using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Numerics;

namespace Tanks.Models
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public enum Color
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        White
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

    public class Tank
    {
        public int Health { get; set; } = 3;
        public int Range { get; set; } = 1;
        public Direction Direction { get; set; } = Direction.North;
        public Color Color { get; set; } = Color.Green;
        public Position Position { get; set; } = new Position(1, 0);
    }

    public static class TankEndpoints
    {
        public class TotalTanks
        {
            public int Total { get; set; } = 0;

            public TotalTanks(int total)
            {
                Total = total;
            }
        }
        public static void MapTankEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/v1/tank");

            group.MapGet("/", () =>
            {
                Log.Info("Requested a new tank");
                return new Tank();
            })
            .WithName("GetAllTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Board.Map.ContainsKey(id))
                {
                    Log.Warn($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound();
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Board.Map[id]);
            })
            .WithName("GetTankById");

            group.MapGet("/total", () => {
                return new TotalTanks(Board.Map.Count);
            })
            .WithName("GetTotalTanks");

            group.MapPut("/{id}", (int id, Tank input) =>
            {
                return TypedResults.NoContent();
            })
            .WithName("UpdateTank");

            group.MapPost("/create", () =>
            {
                // This causes a race condition.
                // TODO: Make map a mutex
                int id = Board.Map.Count + 1;
                Board.Map.Add(id, new Tank());
                Log.Info($"Created new tank with id: {id}");
                return TypedResults.Created($"/api/v1/tanks/{id}", Board.Map[id]);
            })
            .WithName("CreateTank");

            group.MapDelete("/{id}", (int id) =>
            {
                if(!Board.Map.ContainsKey(id))
                {
                    return (IResult)TypedResults.BadRequest();
                }

                Board.Map.Remove(id);
                return (IResult)TypedResults.Ok();
                // return TypedResults.Ok(new Tank { ID = id });
            })
            .WithName("DeleteTank");
        }
    }
}
