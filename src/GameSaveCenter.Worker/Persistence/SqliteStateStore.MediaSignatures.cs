using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Cached SHA-256 of an already-scanned media file, bound to file metadata and a
/// small content sample. The sample is a cheap change detector; the stored SHA-256 remains
/// authoritative for deduplication and old rows without a sample are rehashed once.</summary>
internal sealed record MediaFileSignature(string Path, long Length, DateTime LastWriteTimeUtc, string Sha256, string SampleHash);

public sealed partial class SqliteStateStore
{
    internal async Task<MediaFileSignature?> TryGetMediaFileSignatureAsync(string path, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT length,last_write_utc,sha256,sample_hash FROM media_file_signatures WHERE path=$path LIMIT 1;";
        command.Parameters.AddWithValue("$path", path);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return null;
        return new MediaFileSignature(path, reader.GetInt64(0), DateTime.Parse(reader.GetString(1)).ToUniversalTime(), reader.GetString(2), reader.IsDBNull(3) ? string.Empty : reader.GetString(3));
    }

    internal Task UpsertMediaFileSignatureAsync(string path, long length, DateTime lastWriteTimeUtc, string sha256, CancellationToken token)
        => UpsertMediaFileSignatureAsync(path, length, lastWriteTimeUtc, sha256, string.Empty, token);

    internal Task UpsertMediaFileSignatureAsync(string path, long length, DateTime lastWriteTimeUtc, string sha256, string sampleHash, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO media_file_signatures(path,length,last_write_utc,sha256,sample_hash,updated_utc)
VALUES($path,$length,$write,$hash,$sample,$utc)
ON CONFLICT(path) DO UPDATE SET length=excluded.length,last_write_utc=excluded.last_write_utc,sha256=excluded.sha256,sample_hash=excluded.sample_hash,updated_utc=excluded.updated_utc;",
            new Dictionary<string, object?>
            {
                ["$path"] = path,
                ["$length"] = length,
                ["$write"] = lastWriteTimeUtc.ToString("O"),
                ["$hash"] = sha256,
                ["$sample"] = sampleHash,
                ["$utc"] = DateTime.UtcNow.ToString("O")
            }, token);

    internal Task PruneMediaFileSignaturesAsync(DateTime olderThanUtc, int maximumRows, CancellationToken token)
    {
        maximumRows = Math.Max(1, maximumRows);
        return ExecuteAsync(@"
DELETE FROM media_file_signatures WHERE updated_utc < $cutoff;
DELETE FROM media_file_signatures
WHERE path IN (
    SELECT path FROM media_file_signatures
    ORDER BY updated_utc DESC
    LIMIT -1 OFFSET $maximumRows
);",
            new Dictionary<string, object?>
            {
                ["$cutoff"] = olderThanUtc.ToUniversalTime().ToString("O"),
                ["$maximumRows"] = maximumRows
            }, token);
    }
}
