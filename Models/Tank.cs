using Microsoft.AspNetCore.Http.HttpResults;
namespace Tanks.Models
{
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public class Tank
    {
        public int Health { get; set; } = 3;
        public int Range { get; set; } = 1;
        public Direction Direction { get; set; } = Direction.North;
    }


public static class TankEndpoints
{
	public static void MapTankEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/Tank");

        group.MapGet("/", () =>
        {
            return new Tank();
        })
        .WithName("GetAllTanks");

        group.MapGet("/{id}", (int id) =>
        {
            //return new Tank { ID = id };
        })
        .WithName("GetTankById");

        group.MapPut("/{id}", (int id, Tank input) =>
        {
            return TypedResults.NoContent();
        })
        .WithName("UpdateTank");

        group.MapPost("/", (Tank model) =>
        {
            //return TypedResults.Created($"/Tanks/{model.ID}", model);
        })
        .WithName("CreateTank");

        group.MapDelete("/{id}", (int id) =>
        {
            //return TypedResults.Ok(new Tank { ID = id });
        })
        .WithName("DeleteTank");
    }
}}
    