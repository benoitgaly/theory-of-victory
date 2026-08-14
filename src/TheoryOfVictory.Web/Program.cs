using System.Globalization;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// V1.0 is deterministic: both runs are played once at startup and served from memory.
builder.Services.AddSingleton<PlayedGameLibrary>();
builder.Services.AddSingleton<ProvenanceLibraryCache>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();

// La langue est le premier segment du chemin, et rien d'autre : le site est figé en HTML et
// servi sans serveur, donc il n'y a ni cookie ni en-tête à interroger au moment où la page est
// rendue. /fr/ et /en/ sont deux sites complets, et l'adresse dit lequel on lit.
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    Language language = LanguageFromPath(context.Request.Path);
    Localizer.Current = language;

    CultureInfo culture = Languages.Culture(language);
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;

    await next(context);
});

app.UseRouting();

// Les pages de provenance portent un nom de fichier plat, et la même forme en développement
// qu'une fois le site figé : le plateau est publié en statique, ses liens sont écrits par le
// JavaScript au moment de l'affichage, et ils ne peuvent donc pas être réécrits à la
// publication. Une seule forme d'URL des deux côtés est la seule qui ne casse jamais.
//
// Chaque page existe deux fois, sous /fr/ et sous /en/, avec le MÊME nom de fichier : les codes
// du moteur sont déjà en anglais, donc provenance-civilian-ru.html vaut dans les deux langues et
// un lien déjà partagé ne casse pas. Les routes sans préfixe restent le français, pour que
// l'adresse d'hier continue de répondre en développement comme en ligne.
app.MapControllerRoute(
    name: "provenance-detail-localised",
    pattern: "{lang:regex(^(fr|en)$)}/provenance-{id}.html",
    defaults: new { controller = "Provenance", action = "Detail" });

app.MapControllerRoute(
    name: "provenance-index-localised",
    pattern: "{lang:regex(^(fr|en)$)}/provenance.html",
    defaults: new { controller = "Provenance", action = "Index" });

app.MapControllerRoute(
    name: "board-localised",
    pattern: "{lang:regex(^(fr|en)$)}/{index:regex(^index\\.html$)?}",
    defaults: new { controller = "Game", action = "Index" });

app.MapControllerRoute(
    name: "provenance-detail",
    pattern: "provenance-{id}.html",
    defaults: new { controller = "Provenance", action = "Detail" });

app.MapControllerRoute(
    name: "provenance-index",
    pattern: "provenance.html",
    defaults: new { controller = "Provenance", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}");

app.Run();

static Language LanguageFromPath(PathString path)
{
    string value = path.Value ?? string.Empty;
    string[] segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
    return segments.Length == 0 ? Language.French : Languages.Parse(segments[0]);
}
