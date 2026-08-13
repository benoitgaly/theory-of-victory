using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TheoryOfVictory.Core;
using TheoryOfVictory.Engine;
using TheoryOfVictory.Web.Services;

namespace TheoryOfVictory.Web.Controllers;

public sealed class GameController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(), new FiniteDoubleConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly PlayedGameLibrary _library;

    public GameController(PlayedGameLibrary library)
    {
        _library = library;
    }

    public IActionResult Index()
    {
        List<PlayedGame> games = [.. _library.All];

        var board = _library.BoardSectors.Select(sector => new
        {
            code = sector.Code,
            name = sector.Name,
            lon = sector.Longitude,
            lat = sector.Latitude,
            pushLon = sector.PushLongitude,
            pushLat = sector.PushLatitude,
            // Fixed properties of the ground, unchanged for the whole game: the board reads
            // them once, here, rather than per turn on every resolution.
            width = sector.Width,
            terrain = sector.TerrainMultiplier,
            urbanisation = sector.Urbanisation,
            strategicValue = sector.StrategicValue,
        });

        // The page opens on the quarter we are actually living in, and the timeline draws
        // everything beyond it as a projection. The turn is found by matching the calendar
        // rather than counted from a hard-coded start: the scenario has already moved its
        // opening once, from the invasion back to the autumn 2021 build-up.
        DateTime now = DateTime.Now;
        int currentTurn = 1;
        foreach (TurnSnapshot snapshot in games[0].Turns)
        {
            int firstMonth = snapshot.Season switch
            {
                Season.Winter => 1,
                Season.Spring => 4,
                Season.Summer => 7,
                _ => 10,
            };

            if (new DateTime(snapshot.Year, firstMonth, 1) <= now)
            {
                currentTurn = snapshot.Turn;
            }
        }

        ViewBag.GamesJson = JsonSerializer.Serialize(games, JsonOptions);
        ViewBag.FrontHistoryJson = JsonSerializer.Serialize(_library.FrontHistory, JsonOptions);
        ViewBag.BoardJson = JsonSerializer.Serialize(board, JsonOptions);
        ViewBag.DeckJson = JsonSerializer.Serialize(_library.Deck, JsonOptions);
        ViewBag.CurrentTurn = currentTurn;
        return View();
    }
}
