using System.Text.Json;

namespace WLauncher.Core.Services;

public sealed record CatalogEntry(string Name, string Repository, string FolderName, string AppIconUrl, string Category)
{
    public bool RequiresManualDownload => Uri.TryCreate(Repository, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps;
}

/// <summary>Validates and organizes catalog data independently of the desktop UI.</summary>
public static class CatalogService
{
    public static List<(string Category, List<CatalogEntry> Entries)> Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("The catalog must contain category arrays.");

        var entries = new List<CatalogEntry>();
        foreach (var section in document.RootElement.EnumerateObject())
        {
            if (section.Value.ValueKind != JsonValueKind.Array)
                throw new JsonException($"Category '{section.Name}' must be an array.");
            foreach (var item in section.Value.EnumerateArray())
            {
                var name = ReadString(item, "name");
                var repository = ReadString(item, "repository");
                var folder = ReadString(item, "folderName");
                if (string.IsNullOrWhiteSpace(name) || !IsRepository(repository) || !IsSafeFolder(folder))
                    throw new JsonException($"Invalid catalog entry in '{section.Name}'. Check name, repository and folderName.");
                var icon = ReadString(item, "appIconUrl");
                if (string.IsNullOrEmpty(icon)) icon = ReadString(item, "gameIconUrl");
                entries.Add(new(name.Trim(), repository.Trim(), folder, icon, section.Name));
            }
        }
        return Organize(entries);
    }

    public static List<(string Category, List<CatalogEntry> Entries)> Organize(IEnumerable<CatalogEntry> entries) => entries
        .Where(entry => !entry.Category.Trim().Equals("System Emulation", StringComparison.OrdinalIgnoreCase))
        .Select(entry => entry with { Category = NormalizeCategory(entry.Category) })
        .GroupBy(entry => entry.Repository.Trim(), StringComparer.OrdinalIgnoreCase)
        .Select(group => group.First())
        .GroupBy(entry => entry.Category, StringComparer.OrdinalIgnoreCase)
        .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
        .Select(group => (group.Key, group.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToList()))
        .ToList();

    public static string NormalizeCategory(string category) => category.Trim().ToLowerInvariant() switch
    {
        "rexglue - xbox 360" or "xbox 360 (rexglue)" => "Xbox 360 (Rexglue)",
        "ai rexglue - xbox 360" or "ai - xbox 360 (rexglue)" => "AI - Xbox 360 (Rexglue)",
        "other ports" => "Other Ports",
        _ => category.Trim()
    };

    private static string ReadString(JsonElement item, string property)
    {
        if (item.ValueKind != JsonValueKind.Object) throw new JsonException("Catalog entries must be objects.");
        if (!item.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null) return string.Empty;
        if (value.ValueKind != JsonValueKind.String) throw new JsonException($"'{property}' must be a string.");
        return value.GetString() ?? string.Empty;
    }

    private static bool IsRepository(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return uri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrEmpty(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);
        var parts = value.Split('/');
        return parts.Length == 2 && parts.All(part => part.Length > 0 && part != "." && part != ".."
            && part.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'));
    }

    private static bool IsSafeFolder(string folder) => !string.IsNullOrWhiteSpace(folder)
        && folder is not "." and not ".." && !folder.EndsWith('.') && !folder.EndsWith(' ')
        && !folder.Any(c => char.IsControl(c) || "<>:\"/\\|?*".Contains(c));
}
