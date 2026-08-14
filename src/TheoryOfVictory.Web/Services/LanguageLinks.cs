using TheoryOfVictory.Core.Localization;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// The address of the page being read, in the other language.
///
/// The link is RELATIVE — <c>../en/provenance-oil-ru.html</c> — for the same reason the assets
/// are: GitHub Pages serves the site from a sub-directory, so an absolute path would point
/// outside it once published. And the file name never changes from one language to the other,
/// because the engine codes it is built from are already English: the same page, under two
/// prefixes, and a link shared yesterday still opens.
/// </summary>
public static class LanguageLinks
{
    public static string For(Language language, PathString path)
    {
        return $"../{Languages.Code(language)}/{FileName(path)}";
    }

    /// <summary>
    /// The last segment of the path, or index.html when the address ends on a directory — the
    /// board answers on both /fr/ and /fr/index.html, and the frozen site only has the file.
    /// </summary>
    private static string FileName(PathString path)
    {
        string[] segments = (path.Value ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        string last = segments.Length == 0 ? string.Empty : segments[^1];

        if (!last.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            return "index.html";
        }

        return last;
    }
}
