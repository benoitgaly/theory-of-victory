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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}");

app.Run();
