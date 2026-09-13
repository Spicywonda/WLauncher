using System.Text.Json;

namespace WLauncher.Core.Services;

public sealed record CatalogSnapshot(string Source, string Version, string Json, bool IsCached = false);

public static class CatalogCache
{
    public static async Task<CatalogSnapshot> LoadAsync(string path, string source, bool forceRefresh,
        Func<Task<string>> fetchVersion, Func<string, Task<string>> fetchCatalog, string? legacyPath = null)
    {
        CatalogSnapshot? cached = null;
        if (File.Exists(path))
        {
            try
            {
                cached = JsonSerializer.Deserialize<CatalogSnapshot>(await File.ReadAllTextAsync(path).ConfigureAwait(false));
                if (cached == null || !string.Equals(cached.Source, source, StringComparison.OrdinalIgnoreCase)) cached = null;
                else CatalogService.Parse(cached.Json);
            }
            catch (Exception ex) when (ex is JsonException or IOException or ArgumentException)
            {
                cached = null;
            }
        }

        // Old catalogs have no source metadata. Use them only as an explicitly
        // unverified offline fallback; never persist or treat them as current.
        if (cached == null && !File.Exists(path) && source.Equals("SirDiabo/GHLAppList", StringComparison.OrdinalIgnoreCase)
            && legacyPath != null && File.Exists(legacyPath))
        {
            try
            {
                string json = await File.ReadAllTextAsync(legacyPath).ConfigureAwait(false);
                if (CatalogService.Parse(json).Count > 0)
                    cached = new CatalogSnapshot(string.Empty, "legacy", json, IsCached: true);
            }
            catch (Exception ex) when (ex is JsonException or IOException) { }
        }

        try
        {
            var version = await fetchVersion().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(version)) throw new IOException("No catalog release could be retrieved.");
            if (!forceRefresh && cached?.Source == source && cached.Version == version) return cached;
            var json = await fetchCatalog(version).ConfigureAwait(false);
            var categories = CatalogService.Parse(json);
            if (categories.Count == 0) throw new JsonException("The downloaded catalog contains no games.");
            var snapshot = new CatalogSnapshot(source, version, json);
            await AtomicFile.WriteAllTextAsync(path, JsonSerializer.Serialize(snapshot)).ConfigureAwait(false);
            return snapshot;
        }
        catch (Exception ex) when (cached != null && (ex is HttpRequestException or IOException or JsonException or TaskCanceledException))
        {
            return cached with { IsCached = true };
        }
    }
}
