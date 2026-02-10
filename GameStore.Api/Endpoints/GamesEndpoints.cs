using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameByIdEndPointName = "GetGameById";

    public static readonly List<GameSummaryDto> games = [
        new(
            1,
            "Street Fighter II",
            "Fighting",
            19.99M,
            new DateOnly(1991, 1, 1)
        ),
        new(
            2,
            "Mega Man",
            "Action",
            14.99M,
            new DateOnly(1987, 1, 1)
        ),
        new(
            3,
            "Final Fantasy",
            "RPG",
            69.99M,
            new DateOnly(2004, 2, 29)
        )
    ];

    public static void MapGamesEndPoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext)
        => await dbContext.Games
                          .Include(game => game.Genre)
                          .Select(game => new GameSummaryDto(
                            game.Id,
                            game.Name,
                            game.Genre!.Name,
                            game.Price,
                            game.ReleaseDate
                          ))
                          .AsNoTracking()
                          .ToListAsync());

        // GET /games/1
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(
                new GameDetailsDto(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                )
            );
        })
        .WithName(GetGameByIdEndPointName);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            // GameDto game = new(
            //     games.Count + 1,
            //     newGame.Name,
            //     newGame.Genre,
            //     newGame.Price,
            //     newGame.ReleaseDate
            // );

            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameByIdEndPointName, new { id = gameDto.Id }, gameDto);
        });

        // PUT /games/1
        group.MapPut("/{id}", async (
            int id,
            UpdateGameDto updatedGame,
            GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games
                                       .FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                           .Where(game => game.Id == id)
                           .ExecuteDeleteAsync();

            return Results.NoContent();
        });
    }
}