using System.Text;

namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>
/// Shared temp-file + atomic move helpers. A reader only ever sees the complete new file,
/// and failed writes clean up their temporary partial file before rethrowing.
/// </summary>
public static class AtomicFileWriter
{
    public static async Task WriteAllTextAsync(string path, string content, CancellationToken token)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullPath), ".tmp");
        try
        {
            await File.WriteAllTextAsync(temporary, content ?? string.Empty, new UTF8Encoding(false), token).ConfigureAwait(false);
            File.Move(temporary, fullPath, true);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    public static void WriteAllText(string path, string content)
        => WriteAllTextAsync(path, content, CancellationToken.None).GetAwaiter().GetResult();

    public static async Task CopyAtomicallyAsync(string source, string destination, CancellationToken token)
    {
        var fullSource = Path.GetFullPath(source);
        var fullDestination = Path.GetFullPath(destination);
        var directory = Path.GetDirectoryName(fullDestination) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullDestination), ".partial");
        try
        {
            await using (var input = new FileStream(fullSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 128, true))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true))
                await input.CopyToAsync(output, token).ConfigureAwait(false);
            File.Move(temporary, fullDestination, false);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    public static async Task ReplaceFileAsync(string source, string destination, CancellationToken token)
    {
        var fullSource = Path.GetFullPath(source);
        var fullDestination = Path.GetFullPath(destination);
        var directory = Path.GetDirectoryName(fullDestination) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullDestination), ".replace");
        try
        {
            await using (var input = new FileStream(fullSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 128, true))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true))
                await input.CopyToAsync(output, token).ConfigureAwait(false);
            File.Move(temporary, fullDestination, true);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    private static string TemporaryPath(string directory, string fileName, string suffix)
        => Path.Combine(directory, "." + fileName + "." + Guid.NewGuid().ToString("N") + suffix);

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
