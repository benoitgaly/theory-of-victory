using TheoryOfVictory.Core;
using TheoryOfVictory.Core.Localization;
using TheoryOfVictory.Engine.Provenance;

namespace TheoryOfVictory.Web.Services;

/// <summary>
/// The registry, once per language, read at startup. The figures are the same in both — they
/// are read from the same file — and only the prose laid over them differs, but the merge is
/// done at load time and there is no reason to redo it on every page.
/// </summary>
public sealed class ProvenanceLibraryCache
{
    private readonly Dictionary<Language, ProvenanceRegistry> _registries = [];

    public ProvenanceLibraryCache()
    {
        foreach (Language language in Languages.All)
        {
            _registries[language] = ProvenanceLibrary.Load(language);
        }
    }

    public ProvenanceRegistry For(Language language)
    {
        return _registries[language];
    }
}
