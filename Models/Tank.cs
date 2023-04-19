using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Numerics;

namespace Tanks.Models
{
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

    public class Response
    {
        public string Message { get; set; }

        public Response(string message)
        {
            Message = message;
        }
    }

    public class Tank
    {
        public const int MOVEMENT_RANGE = 4;

        public int Health { get; set; } = 3;
        public int Range { get; set; } = 1;
        public int ActionPoints { get; set; } = 0;
        public Color Color { get; set; } = Color.Green;
        // Only use if MOVEMENT_RANGE should be ignored. Otherwise use Move()
        public Position Position { get; set; } = new Position(0, 0);

        public bool SpendActionPoint()
        {
            if(ActionPoints <= 0)
            {
                return false;
            }

            ActionPoints--;
            return true;
        }

        // Returns false if move was invalid
        public bool Move(int x, int y)
        {
            // Dont have enough action points
            if(!SpendActionPoint())
            {
                return false;
            }

            // Check if specified move is within range
            if(!((MOVEMENT_RANGE * -1) <= x && x <= MOVEMENT_RANGE && (MOVEMENT_RANGE * -1) <= y && y <= MOVEMENT_RANGE))
            {
                return false;
            }

            Position.X += x;
            Position.Y += y;

            return true;
        }
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
                _ => null
            };
        }

        public static void MapTankEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/tank");

            group.MapGet("/", () =>
            {
                Log.Info("Requested all tanks");
                return Board.Tanks;
            })
            .WithName("GetAllTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound(new Response("Tank does not exist"));
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Board.Tanks[id]);
            })
            .WithName("GetTankById");

            group.MapGet("/total", () => {
                return new TotalTanks(Board.Tanks.Count);
            })
            .WithName("GetTotalTanks");

            group.MapPost("/create", () =>
            {
                // This causes a race condition.
                // TODO: Make map a mutex
                int id = Board.Tanks.Count + 1;
                Board.Tanks.Add(id, new Tank());
                Log.Info($"Created new tank with id: {id}");
                return TypedResults.Created($"/api/v1/tanks/{id}", Board.Tanks[id]);
            })
            .WithName("CreateTank");

            group.MapPost("/{id}/move", (int id, int? x, int? y) =>
            {
                if(x == null || y == null)
                {
                    Log.Error("Move was sent. But the position was not correctly specified.");
                    return (IResult)TypedResults.BadRequest(new Response("Direction was not correctly specified"));
                }

                try
                {
                    if (!Board.Tanks[id].Move((int)x, (int)y))
                    {
                        Log.Error("Unable to move tank");
                        return (IResult)TypedResults.BadRequest(new Response("Unable to move tank"));
                    }
                    Log.Info($"Moved tank {id} {{x: {x}; y: {y}}}");
                }
                catch (KeyNotFoundException)
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.BadRequest(new Response($"Attempted to get tank with id: {id}. Which doesn't exist"));
                }

                return (IResult)TypedResults.Ok(new Response("Tank moved"));
            })
            .WithName("MoveTank");

            group.MapPost("/{id}/color", (int id, string color) =>
            {
                if (!Board.Tanks.ContainsKey(id))
                {
                    Log.Error($"Attempted to shoot tank with id {id}. Which does not exist");
                    return (IResult)TypedResults.NotFound(new Response($"Attempted to shoot tank with id {id}. Which does not exist"));
                }

                Color? parsed = ParseColor(color);

                if(parsed == null)
                {
                    Log.Error($"\"{color}\" is not a recognized color");
                    return (IResult)TypedResults.BadRequest(new Response($"\"{color}\" is not a recognized color"));
                }

                Board.Tanks[id].Color = (Color)parsed;

                Log.Info("Successfully changed color");
                return (IResult)TypedResults.Ok(new Response("Successfully changed color"));
            });

            group.MapPost("/{id}/shoot", (int id, int target) =>
            {
                if(!Board.Tanks.ContainsKey(id))
                {
                    Log.Error($"Attempted to shoot tank with id {id}. Which does not exist");
                    return (IResult)TypedResults.NotFound(new Response($"Attempted to shoot tank with id {id}. Which does not exist"));
                }

                if (!Board.Tanks.ContainsKey(target))
                {
                    Log.Error($"Attempted to shoot tank id {target}. Which does not exist");
                    return (IResult)TypedResults.NotFound(new Response($"Attempted to shoot tank id {target}. Which does not exist"));
                }

                Tank origin = Board.Tanks[id];
                Tank dest = Board.Tanks[target];

                if(origin.ActionPoints <= 0)
                {
                    Log.Error("Not enough action points");
                    return (IResult)TypedResults.BadRequest(new Response("Not enough action points"));
                }

                if(dest.Health <= 0)
                {
                    Log.Error("Target already dead");
                    return (IResult)TypedResults.BadRequest(new Response("Target already dead"));
                }

                // TODO: Add funcion for this
                origin.ActionPoints--;
                dest.Health--;

                return TypedResults.Ok(new Response("Hit!"));
            })
            .WithName("ShootTank");
        }
    }
}
