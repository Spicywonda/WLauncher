using WLauncher.Core.Services;

namespace WLauncher.Services;

/// <summary>WLauncher additions and known repository moves. Installed folder names stay unchanged.</summary>
public static class WLauncherCatalog
{
    private static readonly Dictionary<string, string> RepositoryMoves = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sonicdcer/Starfox64Recomp"] = "https://gitlab.com/sonicdcer/Starfox64Recomp/-/releases",
        ["sonicdcer/MarioKart64Recomp"] = "https://gitlab.com/sonicdcer/MarioKart64Recomp/-/releases",
        ["sonicdcer/DNZHRecomp"] = "https://gitlab.com/sonicdcer/DNZHRecomp/-/releases",
        ["bryanthaboi/pokemon-gen1-recomp-project"] = "bryanthaboi/gen1recomp"
    };

    public static string ResolveRepository(string repository) =>
        RepositoryMoves.TryGetValue(repository.Trim(), out var current) ? current : repository.Trim();

    public static List<(string Category, List<CatalogEntry> Entries)> Build(IEnumerable<CatalogEntry> remote)
    {
        using var stream = typeof(WLauncherCatalog).Assembly.GetManifestResourceStream("WLauncher.Catalog.additions.json")
            ?? throw new InvalidOperationException("The bundled WLauncher catalog is missing.");
        using var reader = new StreamReader(stream);
        var additions = CatalogService.Parse(reader.ReadToEnd()).SelectMany(group => group.Entries);
        return CatalogService.Organize(additions.Concat(remote).Select(entry => entry with
        {
            Repository = ResolveRepository(entry.Repository),
            AppIconUrl = entry.Repository.Equals("HarvestMoon64Recomp/HarvestMoon64Recomp", StringComparison.OrdinalIgnoreCase)
                ? "/Assets/Icons/harvestmoon64.jpg" : entry.AppIconUrl
        }));
    }
}
