using System.Text.RegularExpressions;
using System.Text.Json;
using TheoryOfVictory.Core.Localization;
using Xunit;

namespace TheoryOfVictory.Engine.UnitTests;

/// <summary>
/// Le filet de la localisation. Les deux façons dont ce site redeviendrait français sans que
/// personne s'en aperçoive, tenues au build plutôt qu'à la relecture.
///
/// La PREMIÈRE est une chaîne écrite en dur au prochain correctif. Elle ne casse rien : la page
/// française l'affiche parfaitement, et c'est exactement le problème — seul un lecteur anglais
/// la verrait, et il ne nous écrira pas. La seule façon de la voir est de refuser toute chaîne
/// accentuée qui ne passe pas par le catalogue.
///
/// La SECONDE est une clé qui n'existe que d'un côté. En production elle ne produit pas une
/// erreur : elle produit un blanc, ou un mot français au milieu d'une phrase anglaise. Elle
/// casse donc ici, où quelqu'un la lit.
/// </summary>
public sealed class TranslationTests
{
    /// <summary>
    /// Les trois façons d'entrer dans le catalogue : Localizer.Loc en C# et dans les vues,
    /// T(...) dans le JavaScript. Une chaîne affichée qui n'entre pas par là n'est pas
    /// traduisible, et c'est le premier test qui la rattrape.
    /// </summary>
    private static readonly Regex[] Calls =
    [
        new(@"Localizer\.Loc\(\s*" + Joined, RegexOptions.Compiled),
        new(@"(?<![\w.])T\(\s*" + Joined, RegexOptions.Compiled),
        new(@"tov\.t\(\s*" + Joined, RegexOptions.Compiled),
    ];

    /// <summary>
    /// Une clé peut s'écrire sur plusieurs lignes, collée par des + : le compilateur n'en fait
    /// qu'une chaîne, et l'inventaire doit la recomposer de la même façon. Une expression qui
    /// s'arrêterait au premier littéral enregistrerait le premier tiers d'une phrase, que le
    /// code ne demandera jamais.
    /// </summary>
    private const string Joined = "(?<parts>" + Quoted + @"(?:\s*\+\s*" + Quoted + ")*)";

    private static readonly Regex Accented = new(@"[à-öø-ÿÀ-ÖØ-Þ«»œŒ]", RegexOptions.Compiled);

    /// <summary>
    /// La seule exception qui ne soit pas une valeur de donnée : le POURQUOI d'un ordre du
    /// calendrier, écrit à côté de lui dans le scénario. Rien ne le lit et rien ne l'affiche —
    /// c'est une annotation d'auteur, au même titre qu'un commentaire, et elle se lit là où
    /// elle explique quelque chose : contre la ligne qu'elle justifie.
    /// </summary>
    private static readonly Regex Annotation =
        new("Reason = " + Quoted, RegexOptions.Compiled);

    /// <summary>Un littéral entre guillemets, échappements compris.</summary>
    private const string Quoted = @"""((?:[^""\\]|\\.)*)""";

    /// <summary>
    /// Les littéraux accentués qui ne sont PAS du texte affiché, un par un, avec la raison.
    ///
    /// Ils ont tous la même forme : une valeur ÉCRITE DANS UN FICHIER DE DONNÉES, sur laquelle
    /// le code s'indexe. La famille d'une carte choisit son ciel, sa couleur de coque et sa
    /// scène ; l'indice de confiance d'une observation choisit sa pastille. Les traduire
    /// casserait la correspondance ; les sortir du code demanderait de recoder les données en
    /// identifiants anglais, ce qui est un autre chantier. Ils sont donc listés ici, et le sont
    /// un par un pour qu'aucun ne rentre par la bande.
    ///
    /// Les COMMENTAIRES ne sont pas concernés : ils sont en français dans ce projet et le
    /// restent. Le test ne regarde que les chaînes littérales.
    /// </summary>
    private static readonly HashSet<string> DataValues =
    [
        // Les six familles de cartes, telles que cards.fr.json les écrit. board.js s'en sert
        // pour choisir le ciel, la teinte de coque et la scène gravée d'une carte.
        "Économique",
        "Politique occidentale",
        "Politique interne",
        "Énergie",
        "Militaire et technologique",
        // Les cinq natures de source et les trois indices de confiance de
        // historical-figures.json, sur lesquels la page de provenance branche sa pastille.
        "Mesure publiée",
        "Lecture graphique",
        "Estimation d'un tiers",
        "Document budgétaire",
    ];

    /// <summary>
    /// Ce que le test surveille : tout ce qui peut finir sur la page. Le moteur en fait partie
    /// depuis qu'il n'énonce plus que des faits — c'est le livre de phrases, et lui seul, qui a
    /// le droit d'écrire du français.
    ///
    /// Le simulateur n'y entre pas : c'est un rapport de console pour développeur, servi à
    /// aucun lecteur, et le traduire ne rendrait service à personne.
    /// </summary>
    private static readonly string[] WatchedFolders =
    [
        Path.Combine("src", "TheoryOfVictory.Web", "wwwroot", "js"),
        Path.Combine("src", "TheoryOfVictory.Web", "Views"),
        Path.Combine("src", "TheoryOfVictory.Web", "Controllers"),
        Path.Combine("src", "TheoryOfVictory.Web", "Services"),
        Path.Combine("src", "TheoryOfVictory.Engine"),
        Path.Combine("src", "TheoryOfVictory.Core"),
    ];

    [Fact]
    public void NoDisplayedStringIsWrittenInFrenchOutsideTheCatalogue()
    {
        List<string> offenders = [];

        foreach (string file in WatchedFiles(includeCatalogues: false))
        {
            string content = WithoutComments(File.ReadAllText(file), file);
            HashSet<string> keys = KeysIn(content);
            HashSet<string> annotations =
                [.. Annotation.Matches(content).Select(match => Unescape(match.Groups[1].Value))];

            foreach (Match match in Literals(content))
            {
                string literal = Unescape(match.Groups[1].Value);
                if (!Accented.IsMatch(literal))
                {
                    continue;
                }

                if (keys.Contains(literal) || DataValues.Contains(literal) || annotations.Contains(literal))
                {
                    continue;
                }

                offenders.Add($"{Path.GetFileName(file)} : \"{literal}\"");
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Ces chaînes françaises n'entrent pas par le catalogue et ne seront jamais traduites :\n  "
                + string.Join("\n  ", offenders));
    }

    [Fact]
    public void EveryFrenchKeyHasAnEnglishOne_AndTheOtherWayRound()
    {
        IReadOnlyDictionary<string, string> french = Localizer.Catalogue(Language.French);
        IReadOnlyDictionary<string, string> english = Localizer.Catalogue(Language.English);

        string[] untranslated = [.. french.Keys.Where(key => !english.ContainsKey(key)).Order()];
        string[] orphaned = [.. english.Keys.Where(key => !french.ContainsKey(key)).Order()];

        Assert.True(
            untranslated.Length == 0,
            "Ces clés n'ont pas de traduction anglaise, et afficheraient du français :\n  "
                + string.Join("\n  ", untranslated));

        Assert.True(
            orphaned.Length == 0,
            "Ces traductions anglaises ne correspondent à aucune clé du code :\n  "
                + string.Join("\n  ", orphaned));
    }

    /// <summary>
    /// Une traduction vide est pire qu'une traduction absente : le repli n'a pas lieu, et la
    /// page imprime un trou là où le français aurait au moins dit quelque chose.
    /// </summary>
    [Fact]
    public void NoTranslationIsEmpty()
    {
        foreach (Language language in Languages.All)
        {
            foreach (KeyValuePair<string, string> entry in Localizer.Catalogue(language))
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(entry.Value),
                    $"'{entry.Key}' n'a pas de valeur en {Languages.Code(language)}.");
            }
        }
    }

    /// <summary>
    /// Le catalogue français est un INVENTAIRE, pas une traduction : sa valeur est sa clé. Une
    /// valeur qui s'en écarterait ferait dire au code autre chose que ce qu'on y lit — et
    /// <c>Loc</c> ne consulte même pas ce fichier en français, si bien que la divergence ne se
    /// verrait nulle part.
    /// </summary>
    [Fact]
    public void TheFrenchCatalogueSaysExactlyWhatTheCodeSays()
    {
        foreach (KeyValuePair<string, string> entry in Localizer.Catalogue(Language.French))
        {
            Assert.Equal(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Une clé appelée par le code mais absente du catalogue s'affiche en français dans toutes
    /// les langues sans que rien ne le dise. C'est le seul test qui relie les deux moitiés :
    /// le catalogue liste ce que le code demande, et rien d'autre.
    /// </summary>
    [Fact]
    public void EveryKeyTheCodeAsksForIsInTheCatalogue()
    {
        IReadOnlyDictionary<string, string> catalogue = Localizer.Catalogue(Language.French);
        List<string> missing = [];

        foreach (string file in WatchedFiles(includeCatalogues: true))
        {
            string content = WithoutComments(File.ReadAllText(file), file);
            foreach (string key in KeysIn(content))
            {
                if (!catalogue.ContainsKey(key))
                {
                    missing.Add($"{Path.GetFileName(file)} : \"{key}\"");
                }
            }
        }

        Assert.True(
            missing.Count == 0,
            "Ces clés sont appelées par le code et absentes du catalogue — lancer scripts\\Sync-Translations.ps1 :\n  "
                + string.Join("\n  ", missing));
    }

    private static HashSet<string> KeysIn(string content)
    {
        HashSet<string> keys = [];
        foreach (Regex call in Calls)
        {
            foreach (Match match in call.Matches(content))
            {
                keys.Add(Rejoin(match.Groups["parts"].Value));
            }
        }

        return keys;
    }

    /// <summary>Les morceaux d'une clé écrite sur plusieurs lignes, recollés.</summary>
    private static string Rejoin(string parts)
    {
        return string.Concat(Regex.Matches(parts, Quoted).Select(piece => Unescape(piece.Groups[1].Value)));
    }

    private static MatchCollection Literals(string content)
    {
        return Regex.Matches(content, @"""((?:[^""\\\n]|\\.)*)""");
    }

    private static string Unescape(string literal)
    {
        return literal.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n");
    }

    /// <summary>
    /// Les commentaires sont en français et le restent : ils expliquent le pourquoi à qui écrit
    /// ce code, pas au lecteur du site. Seules les chaînes littérales sont surveillées.
    /// </summary>
    private static string WithoutComments(string content, string file)
    {
        string stripped = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        stripped = Regex.Replace(stripped, @"^\s*//.*$", string.Empty, RegexOptions.Multiline);

        if (file.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
        {
            stripped = Regex.Replace(stripped, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline);
        }

        return stripped;
    }

    private static IEnumerable<string> WatchedFiles(bool includeCatalogues)
    {
        string root = RepositoryRoot();
        foreach (string folder in WatchedFolders)
        {
            string path = Path.Combine(root, folder);
            if (!Directory.Exists(path))
            {
                continue;
            }

            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(file);
                string extension = Path.GetExtension(file);
                bool watched = extension is ".cs" or ".cshtml" or ".js";

                // i18n.js et Phrasebook.cs SONT les catalogues — l'un pour la page, l'autre pour
                // le serveur. Ce sont les deux seuls fichiers qui ont le droit d'écrire une
                // phrase française, puisque c'est très exactement leur travail.
                bool catalogue = name is "i18n.js" or "Phrasebook.cs";
                if (watched && (includeCatalogues || !catalogue)
                    && !file.Contains($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}"))
                {
                    yield return file;
                }
            }
        }
    }

    /// <summary>
    /// La racine du dépôt, trouvée en remontant depuis l'exécutable : le test lit les SOURCES,
    /// et non ce qui a été copié à côté de la DLL.
    /// </summary>
    private static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TheoryOfVictory.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("La racine du dépôt est introuvable depuis " + AppContext.BaseDirectory);
    }
}
