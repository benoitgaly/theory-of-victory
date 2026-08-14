using System.Globalization;
using TheoryOfVictory.Web.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// V1.0 is deterministic: both runs are played once at startup and served from memory.
builder.Services.AddSingleton<PlayedGameLibrary>();

WebApplication app = builder.Build();

CultureInfo french = new("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = french;
CultureInfo.DefaultThreadCurrentUICulture = french;

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Les pages de provenance portent un nom de fichier plat, et la même forme en développement
// qu'une fois le site figé : le plateau est publié en statique, ses liens sont écrits par le
// JavaScript au moment de l'affichage, et ils ne peuvent donc pas être réécrits à la
// publication. Une seule forme d'URL des deux côtés est la seule qui ne casse jamais.
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
