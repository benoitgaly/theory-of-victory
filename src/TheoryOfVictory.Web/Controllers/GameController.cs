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
        Converters = { new JsonStringEnumConverter() },
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
        ViewBag.GamesJson = JsonSerializer.Serialize(games, JsonOptions);
        ViewBag.Games = games;
        return View();
    }
}
