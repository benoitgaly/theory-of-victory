using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
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
        });

        // February 2022 is turn 1; the page opens on the quarter we are actually living in.
        DateTime now = DateTime.Now;
        int quartersSinceStart = ((now.Year - 2022) * 4) + (now.Month - 1) / 3;

        ViewBag.GamesJson = JsonSerializer.Serialize(games, JsonOptions);
        ViewBag.BoardJson = JsonSerializer.Serialize(board, JsonOptions);
        ViewBag.DeckJson = JsonSerializer.Serialize(_library.Deck, JsonOptions);
        ViewBag.CurrentTurn = Math.Max(1, quartersSinceStart + 1);
        return View();
    }
}
