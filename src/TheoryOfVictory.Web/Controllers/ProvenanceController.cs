using Microsoft.AspNetCore.Mvc;
using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Web.Services;

namespace TheoryOfVictory.Web.Controllers;

/// <summary>
/// Where a figure comes from: its sources first, then every dated observation behind it, with
/// how much each one can be trusted and why it is — or is not — the value the engine carries.
///
/// It carries nothing about the three runs. Those are projections of the model, not historical
/// data: there is no source to cite for them and therefore nothing to justify. Printing them
/// here made a bibliography page look bigger without making it worth more.
/// </summary>
public sealed class ProvenanceController : Controller
{
    private readonly ProvenanceLibraryCache _registries;

    public ProvenanceController(ProvenanceLibraryCache registries)
    {
        _registries = registries;
    }

    private ProvenanceRegistry Registry
    {
        get { return _registries.For(Localizer.Current); }
    }

    public IActionResult Index()
    {
        return View(Registry);
    }

    public IActionResult Detail(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        ProvenanceRegistry registry = Registry;
        HistoricalFigure? figure = registry.Find(id);
        if (figure is null)
        {
            // A post the database says nothing about. The page exists and says exactly that,
            // rather than 404-ing: "nothing is documented here" is itself the answer.
            ViewBag.MissingCode = id;
            return View("Missing");
        }

        ViewBag.Sources = registry.SourcesOf(figure);
        return View(figure);
    }
}
