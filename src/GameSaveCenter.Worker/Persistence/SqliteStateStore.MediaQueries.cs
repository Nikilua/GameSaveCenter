using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private const int MaximumMediaQueryPageSize = 200;

    public Task<MediaPageDto> GetMediaPageAsync(MediaQueryDto? query, CancellationToken token)
        => GetMediaPageAsync(query, "Assigned", requirePlayniteId: true, token);

    public Task<MediaPageDto> GetUnassignedMediaPageAsync(MediaQueryDto? query, CancellationToken token)
        => GetMediaPageAsync(query, "Inbox", requirePlayniteId: false, token);

    public Task<MediaPageDto> GetIgnoredMediaPageAsync(MediaQueryDto? query, CancellationToken token)
        => GetMediaPageAsync(query, "Ignored", requirePlayniteId: false, token);

    private async Task<MediaPageDto> GetMediaPageAsync(
        MediaQueryDto? query,
        string classificationState,
        bool requirePlayniteId,
        CancellationToken token)
    {
        query ??= new MediaQueryDto();
        if (requirePlayniteId && string.IsNullOrWhiteSpace(query.PlayniteId))
            return new MediaPageDto();

        var limit = Math.Clamp(query.Limit, 1, MaximumMediaQueryPageSize);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = $"SELECT COUNT(*) FROM media WHERE {BuildMediaWhere(countCommand, query, classificationState, includeCursor: false)};";
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(token).ConfigureAwait(false));

        var pageCommand = connection.CreateCommand();
        var where = BuildMediaWhere(pageCommand, query, classificationState, includeCursor: true);
        pageCommand.CommandText = $@"
SELECT media_id,playnite_id,kind,source,archive_path,original_path,captured_utc,size_bytes,sha256,is_favorite,comment,cloud_state,classification_state,classification_reason
FROM media
WHERE {where}
ORDER BY captured_utc DESC, media_id DESC
LIMIT $limit;";
        pageCommand.Parameters.AddWithValue("$limit", limit + 1);

        var items = new List<MediaItemDto>(limit);
        await using var reader = await pageCommand.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (items.Count <= limit && await reader.ReadAsync(token).ConfigureAwait(false))
            items.Add(ReadMedia(reader));

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new MediaPageDto
        {
            Items = items,
            TotalCount = totalCount,
            HasMore = hasMore,
            NextCursor = hasMore && items.Count > 0 ? EncodeMediaCursor(items[items.Count - 1]) : string.Empty
        };
    }

    private static string BuildMediaWhere(SqliteCommand command, MediaQueryDto query, string classificationState, bool includeCursor)
    {
        var predicates = new List<string> { "classification_state=$classification" };
        command.Parameters.AddWithValue("$classification", classificationState);

        if (!string.IsNullOrWhiteSpace(query.PlayniteId))
        {
            predicates.Add("playnite_id=$game");
            command.Parameters.AddWithValue("$game", query.PlayniteId.Trim());
        }

        if (query.Kind.HasValue)
        {
            predicates.Add("kind=$kind");
            command.Parameters.AddWithValue("$kind", (int)query.Kind.Value);
        }

        if (query.FavoriteOnly)
            predicates.Add("is_favorite=1");

        var search = query.Search?.Trim() ?? string.Empty;
        if (search.Length > 0)
        {
            command.Parameters.AddWithValue("$search", "%" + EscapeLike(search) + "%");
            var sourcePredicates = new List<string>();
            foreach (var source in MediaSourceSearchLabels)
            {
                if (!source.Label.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;

                var parameter = "$source" + (int)source.Kind;
                command.Parameters.AddWithValue(parameter, (int)source.Kind);
                sourcePredicates.Add("source=" + parameter);
            }

            var textPredicates = new List<string>
            {
                "media_id LIKE $search ESCAPE '\\' COLLATE NOCASE",
                "archive_path LIKE $search ESCAPE '\\' COLLATE NOCASE",
                "original_path LIKE $search ESCAPE '\\' COLLATE NOCASE",
                "COALESCE(comment,'') LIKE $search ESCAPE '\\' COLLATE NOCASE"
            };
            textPredicates.AddRange(sourcePredicates);
            predicates.Add("(" + string.Join(" OR ", textPredicates) + ")");
        }

        var cursor = includeCursor ? DecodeMediaCursor(query.Cursor) : null;
        if (cursor != null)
        {
            predicates.Add("(captured_utc < $cursor_capture OR (captured_utc = $cursor_capture AND media_id < $cursor_id))");
            command.Parameters.AddWithValue("$cursor_capture", cursor.Value.CapturedUtc.ToString("O", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("$cursor_id", cursor.Value.MediaId);
        }

        return string.Join(" AND ", predicates);
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private static string EncodeMediaCursor(MediaItemDto item)
    {
        var payload = item.CapturedUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) + "\n" + item.MediaId;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    private static MediaCursor? DecodeMediaCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var payload = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var separator = payload.IndexOf('\n');
            if (separator <= 0 || separator == payload.Length - 1)
                return null;
            if (!DateTime.TryParseExact(
                    payload.Substring(0, separator),
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var capturedUtc))
                return null;
            return new MediaCursor(capturedUtc.ToUniversalTime(), payload.Substring(separator + 1));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private readonly record struct MediaCursor(DateTime CapturedUtc, string MediaId);

    private static readonly (MediaSourceKind Kind, string Label)[] MediaSourceSearchLabels =
    {
        (MediaSourceKind.Steam, "Steam"),
        (MediaSourceKind.XboxGameBar, "Xbox Game Bar"),
        (MediaSourceKind.WindowsScreenshot, "Windows 截图"),
        (MediaSourceKind.Epic, "Epic"),
        (MediaSourceKind.Ubisoft, "Ubisoft"),
        (MediaSourceKind.Ea, "EA"),
        (MediaSourceKind.Gog, "GOG"),
        (MediaSourceKind.ReShade, "ReShade"),
        (MediaSourceKind.Nvidia, "NVIDIA"),
        (MediaSourceKind.Amd, "AMD"),
        (MediaSourceKind.GameNative, "游戏内截图"),
        (MediaSourceKind.Custom, "自定义来源"),
        (MediaSourceKind.Unknown, "其他来源")
    };
}
