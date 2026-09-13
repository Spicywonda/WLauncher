namespace WLauncher.Core.Services;

/// <summary>Replaces a file only after its complete contents have been written.</summary>
public static class AtomicFile
{
    public static async Task WriteAllTextAsync(string path, string content)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, content).ConfigureAwait(false);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
