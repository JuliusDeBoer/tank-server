using Models;

namespace Controllers
{
    public static class TankEndpoints
    {
        public static void MapTankEndpoints(this IEndpointRouteBuilder routes)
        {
            RouteGroupBuilder group = routes.MapGroup("/api/v1/tank");

            group.MapGet("/", () =>
            {
                Log.Info("Requested all tanks");
                return new TankTotal(Game.Tanks.AllTanks);
            })
            .WithName("GetTanks");

            group.MapGet("/{id}", (int id) =>
            {
                if (!Game.Tanks.Contains(id))
                {
                    Log.Error($"Attempted to get tank with id: {id}. Which doesn't exist");
                    return (IResult)TypedResults.NotFound(new Response("ERR_NO_TANK_FOUND", "Tank does not exist"));
                }
                Log.Info($"Got tank {id}");
                return (IResult)TypedResults.Ok(Game.Tanks.AllTanks[id]);
            })
            .WithName("GetTank");

            group.MapPost("/move", (HttpContext context, int? x, int? y) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                if (x == null || y == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                int id = account.TankId;

                // TODO: Refactor this
                try
                {
                    if (!Game.Tanks.Move(id, (int)x, (int)y))
                    {
                        Log.Error(new Response("ERR_CODE_NOT_IMPLEMENTED", "Unable to move tank"));
                        return (IResult)TypedResults.BadRequest(new Response("ERR_CODE_NOT_IMPLEMENTED", "Unable to move tank"));
                    }
                    Log.Info($"Moved tank {id} {{x: {x}; y: {y}}}");
                }
                catch (KeyNotFoundException)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                return Response.Ok(Response.OK);
            })
            .WithName("MoveTank");

            group.MapPost("/color", (HttpContext context, string color) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Color? parsed = Tanks.ParseColor(color);

                if (parsed == null)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                Game.Tanks.AllTanks[id].Color = (Color)parsed;

                return Response.Ok(Response.OK);
            })
            .WithName("SetColor");

            group.MapPost("/shoot", (HttpContext context, int target) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.Contains(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks.AllTanks[id];
                Tank dest = Game.Tanks.AllTanks[target];

                if (origin.ActionPoints <= 0)
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (dest.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                // TODO: Add funcion for this
                origin.ActionPoints--;
                dest.Health--;

                return TypedResults.Ok(Response.OK);
            })
            .WithName("Shoot");

            group.MapPost("/upgrade", (HttpContext context) =>
            {
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank tank = Game.Tanks.AllTanks[id];

                if (tank.ActionPoints <= 0)
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                if (tank.Level >= Tanks.MAX_LEVEL)
                {
                    return Response.BadRequest(Response.ERR_MAX_LEVEL_REACHED);
                }

                if (!Game.Tanks.HasActionPoints(id, 1))
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                Game.Tanks.SpendActionPoints(id, 1);

                tank.Level++;

                return Response.Ok(Response.OK);
            })
            .WithName("Upgrade");

            group.MapPost("/give", (HttpContext context, int amount, int target) =>
            {
                // TODO: Header possibly null here. Catch this exception!
                Account? account = Game.Authenticator.GetUser(context.Request.Headers["Authorization"]);

                if (account == null)
                {
                    return Response.BadRequest(Response.ERR_INVALID_CREDENTIALS);
                }

                int id = account.TankId;

                if (amount <= 0)
                {
                    return Response.BadRequest(Response.ERR_BAD_ARGUMENTS);
                }

                if (!Game.Tanks.Contains(id))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (!Game.Tanks.Contains(target))
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                Tank origin = Game.Tanks.AllTanks[id];
                Tank destination = Game.Tanks.AllTanks[target];

                if (origin.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                if (destination.Health <= 0)
                {
                    return Response.BadRequest(Response.ERR_NO_SUCH_TANK);
                }

                // Actually spend points
                if (!Game.Tanks.HasActionPoints(id, amount))
                {
                    return Response.BadRequest(Response.ERR_NOT_ENOUGH_ACTION_POINTS);
                }

                Game.Tanks.GiveActionPoints(id, target, amount);

                return Response.Ok(Response.OK);
            })
            .WithName("Give");
        }
    }
}
