using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using WLauncher.Core.Services;
using WLauncher.Services;
using WLauncher.Models;
using WLauncher.Core.Models;

int failures = 0;
void Check(string name, Func<bool> test) { try { if (!test()) throw new Exception("Expected behavior missing"); Console.WriteLine("PASS " + name); } catch (Exception e) { Console.WriteLine("FAIL " + name + ": " + e.Message); failures++; } }
Check("Xbox aliases form one category", () => CatalogService.Parse("""{"Rexglue - Xbox 360":[{"name":"A","repository":"o/a","folderName":"A"}],"Xbox 360 (Rexglue)":[{"name":"B","repository":"o/b","folderName":"B"}]}""").Count == 1);
Check("Malformed JSON is rejected before caching", () => { try { CatalogService.Parse("{broken"); return false; } catch (System.Text.Json.JsonException) { return true; } });
Check("Duplicate repositories are shown once", () => CatalogService.Parse("""{"N64 Ports":[{"name":"A","repository":"o/a","folderName":"A"},{"name":"A duplicate","repository":"O/A","folderName":"A"}]}""").Single().Entries.Count == 1);
Check("Entries are alphabetical", () => CatalogService.Parse("""{"N64 Ports":[{"name":"Zelda","repository":"o/z","folderName":"Z"},{"name":"Banjo","repository":"o/b","folderName":"B"}]}""").Single().Entries[0].Name == "Banjo");


Check("Unsafe installation folder is rejected", () => {
    try { CatalogService.Parse("""{"N64 Ports":[{"name":"A","repository":"o/a","folderName":"../escape"}]}"""); return false; }
    catch (JsonException) { return true; }
});
var temporary = Path.Combine(Path.GetTempPath(), "wlauncher-tests-" + Guid.NewGuid());
Directory.CreateDirectory(temporary);
try
{
    var invalidPath = Path.Combine(temporary, "apps.json");
    File.WriteAllText(invalidPath, "{broken");
    Check("Unreadable app list is reported without discarding it", () => {
        var manager = (GameManager)RuntimeHelpers.GetUninitializedObject(typeof(GameManager));
        var method = typeof(GameManager).GetMethod("LoadAppsFromFileAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try { ((Task<List<GameInfo>>)method.Invoke(manager, [invalidPath])!).GetAwaiter().GetResult(); return false; }
        catch (IOException ex) when (ex.InnerException is JsonException) { return File.ReadAllText(invalidPath) == "{broken"; }
    });
}
finally { Directory.Delete(temporary, true); }
Check("Unsupported packages are not offered as installable downloads", () =>
{
    var release = new GitHubRelease { assets = [
        new GitHubAsset { name = "game-windows.zip" },
        new GitHubAsset { name = "game-macos.dmg" },
        new GitHubAsset { name = "sha256sums.txt" },
        new GitHubAsset { name = "game-linux.tar.xz" }
    ] };
    return GitHubReleaseService.GetDownloadableAssets(release).Select(asset => asset.name)
        .SequenceEqual(new[] { "game-windows.zip" });
});
Check("Unsupported installation cannot write a success version", () =>
{
    var path = Path.Combine(Path.GetTempPath(), "wlauncher-install-" + Guid.NewGuid());
    Directory.CreateDirectory(path);
    try
    {
        File.WriteAllText(Path.Combine(path, "version.txt"), "old");
        try { GameInstallationService.InstallOrUpdateGameAsync("missing.dmg", path, "game.dmg", "new").GetAwaiter().GetResult(); return false; }
        catch (NotSupportedException) { return File.ReadAllText(Path.Combine(path, "version.txt")) == "old"; }
    }
    finally { Directory.Delete(path, true); }
});
Check("Bundled entries load without a remote catalog", () =>
{
    var entries = WLauncherCatalog.Build([]).SelectMany(group => group.Entries).ToList();
    return entries.Any(entry => entry.Name == "Extreme-G Recompiled" && entry.RequiresManualDownload)
        && entries.Any(entry => entry.Repository == "RSDKModding/Sonic-Mania-Decompilation");
});
Check("Moved repositories do not create duplicate catalog cards", () =>
{
    var entries = WLauncherCatalog.Build([
        new CatalogEntry("Old Star Fox", "sonicdcer/Starfox64Recomp", "OldFolder", "", "N64 Ports"),
        new CatalogEntry("Remote Nocturne", "birabittoh/NocturneRecomp", "Nocturne", "", "Rexglue - Xbox 360")
    ]).SelectMany(group => group.Entries).ToList();
    return entries.Count(entry => entry.Repository.Contains("Starfox64Recomp")) == 1
        && entries.Count(entry => entry.Repository == "birabittoh/NocturneRecomp") == 1;
});

const string validCatalog = """{"N64 Ports":[{"name":"Banjo","repository":"o/b","folderName":"Banjo"}]}""";
var cacheDirectory = Path.Combine(Path.GetTempPath(), "wlauncher-cache-tests-" + Guid.NewGuid());
Directory.CreateDirectory(cacheDirectory);
try
{
    var cachePath = Path.Combine(cacheDirectory, "snapshot.json");
    CatalogCache.LoadAsync(cachePath, "owner/catalog", false, () => Task.FromResult("v1"),
        _ => Task.FromResult(validCatalog)).GetAwaiter().GetResult();
    var original = File.ReadAllText(cachePath);
    Check("Invalid refresh preserves the last valid catalog and version", () =>
    {
        var result = CatalogCache.LoadAsync(cachePath, "owner/catalog", true, () => Task.FromResult("v2"),
            _ => Task.FromResult("{broken")).GetAwaiter().GetResult();
        return result.IsCached && result.Version == "v1" && result.Json == validCatalog
            && File.ReadAllText(cachePath) == original;
    });
    Check("Offline requests return the cached catalog", () =>
    {
        var result = CatalogCache.LoadAsync(cachePath, "owner/catalog", false,
            () => Task.FromException<string>(new HttpRequestException("offline")),
            _ => throw new Exception("Download should not run")).GetAwaiter().GetResult();
        return result.IsCached && result.Json == validCatalog;
    });
    Check("An unchanged release avoids downloading its catalog again", () =>
    {
        var result = CatalogCache.LoadAsync(cachePath, "owner/catalog", false, () => Task.FromResult("v1"),
            _ => throw new Exception("Unnecessary download")).GetAwaiter().GetResult();
        return result.Version == "v1";
    });
    Check("A different repository never receives the previous repository cache", () =>
    {
        try
        {
            CatalogCache.LoadAsync(cachePath, "different/catalog", false,
                () => Task.FromException<string>(new HttpRequestException("offline")),
                _ => throw new Exception("Download should not run")).GetAwaiter().GetResult();
            return false;
        }
        catch (HttpRequestException) { return File.ReadAllText(cachePath) == original; }
    });
    Check("Successful refresh replaces data and version together", () =>
    {
        var result = CatalogCache.LoadAsync(cachePath, "owner/catalog", true, () => Task.FromResult("v2"),
            _ => Task.FromResult(validCatalog)).GetAwaiter().GetResult();
        var saved = JsonSerializer.Deserialize<CatalogSnapshot>(File.ReadAllText(cachePath))!;
        return !result.IsCached && saved.Version == "v2" && saved.Json == validCatalog
            && Directory.GetFiles(cacheDirectory, "*.tmp").Length == 0;
    });
}
finally { Directory.Delete(cacheDirectory, true); }
if (args.Length > 0)
{
    Check("Current upstream catalog remains compatible", () =>
    {
        var groups = WLauncherCatalog.Build(CatalogService.Parse(File.ReadAllText(args[0])).SelectMany(group => group.Entries));
        var entries = groups.SelectMany(group => group.Entries).ToList();
        return entries.Count > 100 && entries.Select(entry => entry.Repository).Distinct(StringComparer.OrdinalIgnoreCase).Count() == entries.Count;
    });
}
Check("A single foreign package still requires an explicit selection", () =>
{
    var files = new[] { new GitHubAsset { name = "game-windows.zip" } };
    return !GitHubReleaseService.CanAutomaticallySelectAsset(files, "macOS")
        && GitHubReleaseService.CanAutomaticallySelectAsset(files, "Windows");
});
Check("Offline upgrade preserves a validated legacy catalog without claiming its source", () =>
{
    var directory = Path.Combine(Path.GetTempPath(), "wlauncher-legacy-" + Guid.NewGuid());
    Directory.CreateDirectory(directory);
    try
    {
        string legacy = Path.Combine(directory, "app_catalog_cache.json");
        string snapshot = Path.Combine(directory, "snapshot.json");
        File.WriteAllText(legacy, validCatalog);
        var result = CatalogCache.LoadAsync(snapshot, "SirDiabo/GHLAppList", false,
            () => Task.FromException<string>(new HttpRequestException("offline")),
            _ => throw new Exception("Should not download"), legacy).GetAwaiter().GetResult();
        return result.Json == validCatalog && result.IsCached && result.Source == string.Empty
            && !File.Exists(snapshot) && File.ReadAllText(legacy) == validCatalog;
    }
    finally { Directory.Delete(directory, true); }
});
Console.WriteLine($"Result: {failures} failure(s)");
Environment.ExitCode = failures == 0 ? 0 : 1;
