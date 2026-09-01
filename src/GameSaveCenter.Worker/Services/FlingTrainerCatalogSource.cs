using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

public interface ITrainerCatalogSource
{
    Task<TrainerCatalogSyncResultDto> SyncCatalogAsync(CancellationToken token);
    Task<List<TrainerCatalogItemDto>> SearchAsync(string query, int limit, CancellationToken token);
    Task<List<TrainerReleaseDto>> GetReleasesAsync(string catalogId, CancellationToken token);
    Task DownloadAsync(string releaseId, string targetPath, IProgress<(long Received,long? Total)>? progress, CancellationToken token);
}

/// <summary>Isolates FLiNG's public HTML so page changes cannot affect local tool management.</summary>
public sealed class FlingTrainerCatalogSource : ITrainerCatalogSource
{
    private const long MaxDownloadBytes=2L*1024*1024*1024;
    private const int MaxArchiveDirectories=2048;
    private const int MaxArchiveEntries=10000;
    private static readonly string[] ArchiveExtensions={".zip",".rar",".7z"};
    private static readonly Uri CatalogUri = new("https://flingtrainer.com/all-trainers/");
    private static readonly Uri ArchiveCatalogUri = new("https://archive.flingtrainer.com/");
    private static readonly Regex TrainerLink = new(
        "<a\\b[^>]*\\bhref\\s*=\\s*[\"'](?<url>[^\"']*)[\"'][^>]*>(?<title>.*?)</a\\s*>",
        RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
    private static readonly Regex DownloadLink = new(
        "<a\\b[^>]*\\bhref\\s*=\\s*[\"'](?<url>[^\"']*)[\"'][^>]*>(?<name>.*?)</a\\s*>",
        RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
    private static readonly Regex ArchiveFileLink = new(
        "<a[^>]+href=[\"'](?<url>[^\"'#?]+)[\"'][^>]*>(?<name>.*?)</a>",
        RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
    private static readonly Regex Tags = new("<[^>]+>",RegexOptions.Compiled);
    private readonly HttpClient _http;
    private readonly SqliteStateStore _store;
    private readonly ILogger<FlingTrainerCatalogSource> _logger;

    public FlingTrainerCatalogSource(SqliteStateStore store,ILogger<FlingTrainerCatalogSource> logger)
    {
        _store=store;_logger=logger;
        _http=new HttpClient(new HttpClientHandler{AutomaticDecompression=DecompressionMethods.All})
        {Timeout=TimeSpan.FromSeconds(45)};
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("GameSaveCenter/0.5 (+https://github.com/)");
    }

    public async Task<TrainerCatalogSyncResultDto> SyncCatalogAsync(CancellationToken token)
    {
        var html=await GetHtmlAsync(CatalogUri.ToString(),token).ConfigureAwait(false);
        var now=DateTime.UtcNow;
        var onlineItems=ParseOnlineCatalog(html,now);
        if(onlineItems.Count<100)throw new WorkerOperationException("FLING_CATALOG_PARSE_FAILED","FLiNG 目录结构可能已经变化，未覆盖本地缓存。",$"Parsed only {onlineItems.Count} trainer links.");

        var archiveItems=new List<TrainerCatalogItemDto>();
        try
        {
            archiveItems=await GetArchiveCatalogAsync(now,token).ConfigureAwait(false);
        }
        catch(OperationCanceledException){throw;}
        catch(Exception ex)
        {
            // The historical archive is optional. A temporary outage must not discard
            // the current catalog or invalidate an otherwise successful refresh.
            _logger.LogWarning(ex,"Historical FLiNG archive could not be synchronized; keeping current catalog");
        }
        var items=onlineItems.Concat(archiveItems)
            .GroupBy(x=>x.PageUrl,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).ToList();
        await _store.ReplaceTrainerCatalogAsync(items,token).ConfigureAwait(false);
        _logger.LogInformation("Synchronized {Count} FLiNG catalog entries ({OnlineCount} online, {ArchiveCount} archive)",items.Count,onlineItems.Count,archiveItems.Count);
        return new TrainerCatalogSyncResultDto{ItemCount=items.Count,SyncedUtc=now,Message=$"已同步 {items.Count} 个 FLiNG 修改器"};
    }

    public Task<List<TrainerCatalogItemDto>> SearchAsync(string query,int limit,CancellationToken token)
        =>_store.SearchTrainerCatalogAsync(query,limit,token);

    public async Task<List<TrainerReleaseDto>> GetReleasesAsync(string catalogId,CancellationToken token)
    {
        var item=await _store.GetTrainerCatalogItemAsync(catalogId,token).ConfigureAwait(false)
                 ?? throw new KeyNotFoundException("FLiNG 目录项不存在，请刷新目录。");
        EnsureFlingUri(item.PageUrl);
        if(IsArchiveFileUrl(item.PageUrl))
        {
            var archiveRelease=new TrainerReleaseDto
            {
                ReleaseId=StableId(item.PageUrl),CatalogId=catalogId,DisplayName=item.Title,
                DownloadUrl=item.PageUrl
            };
            await _store.ReplaceTrainerReleasesAsync(catalogId,new[]{archiveRelease},token).ConfigureAwait(false);
            return new List<TrainerReleaseDto>{archiveRelease};
        }
        var html=await GetHtmlAsync(item.PageUrl,token).ConfigureAwait(false);
        var releases=ParseReleases(html,catalogId,new Uri(item.PageUrl,UriKind.Absolute));
        if(releases.Count==0)throw new WorkerOperationException("FLING_RELEASE_PARSE_FAILED","没有从详情页识别到可下载版本；FLiNG 页面可能已变化。",item.PageUrl);
        await _store.ReplaceTrainerReleasesAsync(catalogId,releases,token).ConfigureAwait(false);
        return releases;
    }

    public async Task DownloadAsync(string releaseId,string targetPath,IProgress<(long Received,long? Total)>? progress,CancellationToken token)
    {
        var release=await _store.GetTrainerReleaseAsync(releaseId,token).ConfigureAwait(false)
                    ?? throw new KeyNotFoundException("下载版本不存在，请重新展开版本列表。");
        EnsureFlingUri(release.DownloadUrl);
        using var response=await _http.GetAsync(release.DownloadUrl,HttpCompletionOption.ResponseHeadersRead,token).ConfigureAwait(false);
        EnsureFlingUri(response.RequestMessage?.RequestUri?.ToString()??string.Empty);
        response.EnsureSuccessStatusCode();
        if(response.Content.Headers.ContentLength is long declaredLength&&declaredLength>MaxDownloadBytes)
            throw new WorkerOperationException("FLING_DOWNLOAD_TOO_LARGE","修改器下载文件超过安全大小上限，已拒绝下载。",$"{declaredLength} bytes");
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        await using var source=await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        await using var target=new FileStream(targetPath,FileMode.Create,FileAccess.Write,FileShare.None,81920,true);
        var buffer=new byte[81920];long received=0;var total=response.Content.Headers.ContentLength;
        while(true)
        {
            var count=await source.ReadAsync(buffer,token).ConfigureAwait(false);if(count==0)break;
            await target.WriteAsync(buffer.AsMemory(0,count),token).ConfigureAwait(false);received+=count;
            if(received>MaxDownloadBytes)
                throw new WorkerOperationException("FLING_DOWNLOAD_TOO_LARGE","修改器下载文件超过安全大小上限，已中止下载。",$"{received} bytes");
            progress?.Report((received,total));
        }
        if(received==0)throw new WorkerOperationException("FLING_DOWNLOAD_EMPTY","下载内容为空。",release.DownloadUrl);
    }

    private async Task<string> GetHtmlAsync(string url,CancellationToken token)
    {
        EnsureFlingUri(url);
        using var response=await _http.GetAsync(url,HttpCompletionOption.ResponseContentRead,token).ConfigureAwait(false);
        EnsureFlingUri(response.RequestMessage?.RequestUri?.ToString()??string.Empty);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }

    private async Task<List<TrainerCatalogItemDto>> GetArchiveCatalogAsync(DateTime syncedUtc,CancellationToken token)
    {
        var pending=new Queue<Uri>();
        var visited=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items=new List<TrainerCatalogItemDto>();
        pending.Enqueue(ArchiveCatalogUri);

        while(pending.Count>0&&visited.Count<MaxArchiveDirectories&&items.Count<MaxArchiveEntries)
        {
            token.ThrowIfCancellationRequested();
            var directory=pending.Dequeue();
            if(!visited.Add(directory.AbsoluteUri))continue;

            var html=await GetHtmlAsync(directory.AbsoluteUri,token).ConfigureAwait(false);
            var listing=ParseArchiveDirectoryListing(html,directory,syncedUtc);
            items.AddRange(listing.Files);
            foreach(var child in listing.Directories)
            {
                if(visited.Count+pending.Count>=MaxArchiveDirectories)break;
                if(!visited.Contains(child.AbsoluteUri))pending.Enqueue(child);
            }
        }

        if(pending.Count>0)
            _logger.LogWarning("Historical FLiNG archive crawl reached its safety limit (directories={Directories}, entries={Entries}); remaining directories were skipped",visited.Count,items.Count);

        return items.GroupBy(x=>x.PageUrl,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).Take(MaxArchiveEntries).ToList();
    }

    private static void EnsureFlingUri(string value)
    {
        if(!Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme!=Uri.UriSchemeHttps||
           !IsFlingHost(uri.Host))
            throw new WorkerOperationException("FLING_URL_REJECTED","拒绝访问非 FLiNG HTTPS 地址。",value);
    }

    private static bool TryNormalizeFlingUri(string value,Uri baseUri,string requiredPathPrefix,bool stripQuery,out Uri uri)
    {
        uri=null!;
        var decoded=WebUtility.HtmlDecode(value).Trim();
        if(string.IsNullOrWhiteSpace(decoded)||!Uri.TryCreate(baseUri,decoded,out var candidate)
           ||candidate.Scheme!=Uri.UriSchemeHttps||!IsFlingHost(candidate.Host)
           ||!candidate.AbsolutePath.StartsWith(requiredPathPrefix,StringComparison.OrdinalIgnoreCase))return false;

        var builder=new UriBuilder(candidate){Fragment=string.Empty};
        if(stripQuery)builder.Query=string.Empty;
        uri=builder.Uri;
        return true;
    }

    private static bool IsFlingHost(string host)
        =>host.Equals("flingtrainer.com",StringComparison.OrdinalIgnoreCase)
          ||host.EndsWith(".flingtrainer.com",StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Parses the online /all-trainers/ page into catalog items. Pure and deterministic,
    /// so a FLiNG markup change can be caught by canary tests before it reaches the local cache.
    /// </summary>
    internal static List<TrainerCatalogItemDto> ParseOnlineCatalog(string html,DateTime syncedUtc)
        =>ParseOnlineCatalog(html,syncedUtc,CatalogUri);

    internal static List<TrainerCatalogItemDto> ParseOnlineCatalog(string html,DateTime syncedUtc,Uri baseUri)
    {
        var items=new List<TrainerCatalogItemDto>();
        var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(Match match in TrainerLink.Matches(html??string.Empty))
        {
            if(!TryNormalizeFlingUri(match.Groups["url"].Value,baseUri,"/trainer/",stripQuery:true,out var uri))continue;
            var title=Clean(match.Groups["title"].Value);
            if(string.IsNullOrWhiteSpace(title)||!seen.Add(uri.AbsoluteUri))continue;
            var url=uri.AbsoluteUri;
            items.Add(new TrainerCatalogItemDto
            {
                CatalogId=StableId(url),Title=title,NormalizedTitle=Normalize(title),
                PageUrl=url,LastSyncedUtc=syncedUtc
            });
        }
        return items;
    }

    /// <summary>
    /// Parses a trainer detail page into downloadable release versions. Pure and deterministic;
    /// empty names fall back to the generic "FLiNG Trainer" display label.
    /// </summary>
    internal static List<TrainerReleaseDto> ParseReleases(string html,string catalogId)
        =>ParseReleases(html,catalogId,CatalogUri);

    internal static List<TrainerReleaseDto> ParseReleases(string html,string catalogId,Uri baseUri)
    {
        var releases=new List<TrainerReleaseDto>();
        var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(Match match in DownloadLink.Matches(html??string.Empty))
        {
            if(!TryNormalizeFlingUri(match.Groups["url"].Value,baseUri,"/downloads/",stripQuery:false,out var uri))continue;
            var url=uri.AbsoluteUri;
            if(!seen.Add(url))continue;
            var name=Clean(match.Groups["name"].Value);
            releases.Add(new TrainerReleaseDto
            {
                ReleaseId=StableId(url),CatalogId=catalogId,DisplayName=string.IsNullOrWhiteSpace(name)?"FLiNG Trainer":name,
                DownloadUrl=url
            });
        }
        return releases;
    }

    internal static List<TrainerCatalogItemDto> ParseArchiveCatalog(string html,DateTime syncedUtc)
        =>ParseArchiveDirectoryListing(html,ArchiveCatalogUri,syncedUtc).Files;

    internal static ArchiveDirectoryListing ParseArchiveDirectoryListing(string html,Uri baseUri,DateTime syncedUtc)
    {
        var result=new ArchiveDirectoryListing();
        foreach(Match match in ArchiveFileLink.Matches(html??string.Empty))
        {
            var href=WebUtility.HtmlDecode(match.Groups["url"].Value.Trim());
            if(string.IsNullOrWhiteSpace(href)||href.StartsWith("#",StringComparison.Ordinal))continue;
            if(!Uri.TryCreate(baseUri,href,out var uri)||!IsArchiveUri(uri))continue;
            var name=Clean(match.Groups["name"].Value);
            if(IsArchiveFileUrl(uri.ToString()))
            {
                if(string.IsNullOrWhiteSpace(name))name=Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath));
                var title=Path.GetFileNameWithoutExtension(name).Replace('_',' ').Trim();
                if(string.IsNullOrWhiteSpace(title))continue;
                result.Files.Add(new TrainerCatalogItemDto
                {
                    CatalogId=StableId(uri.ToString()),Title=title,NormalizedTitle=Normalize(title),
                    PageUrl=uri.ToString(),LastSyncedUtc=syncedUtc
                });
            }
            else if(IsArchiveDirectoryLink(href,name,uri,baseUri))
            {
                result.Directories.Add(uri);
            }
        }
        result.Files=result.Files.GroupBy(x=>x.PageUrl,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).ToList();
        result.Directories=result.Directories.GroupBy(x=>x.AbsoluteUri,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).ToList();
        return result;
    }

    private static bool IsArchiveDirectoryLink(string href,string name,Uri uri,Uri baseUri)
        =>!uri.Equals(baseUri)
          &&(href.EndsWith("/",StringComparison.Ordinal)||name.EndsWith("/",StringComparison.Ordinal))
          &&uri.AbsolutePath.EndsWith("/",StringComparison.Ordinal);

    private static bool IsArchiveUri(Uri uri)
        =>uri.Scheme==Uri.UriSchemeHttps&&uri.Host.Equals("archive.flingtrainer.com",StringComparison.OrdinalIgnoreCase);

    private static bool IsArchiveFileUrl(string value)
        =>Uri.TryCreate(value,UriKind.Absolute,out var uri)
          &&IsArchiveUri(uri)
          &&(uri.AbsolutePath.EndsWith(".exe",StringComparison.OrdinalIgnoreCase)
             ||ArchiveExtensions.Any(extension=>uri.AbsolutePath.EndsWith(extension,StringComparison.OrdinalIgnoreCase)));

    private static string Clean(string value)=>WebUtility.HtmlDecode(Tags.Replace(value,string.Empty)).Trim();
    private static string Normalize(string value)=>new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    private static string StableId(string value)
    {
        var bytes=SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..24];
    }
}

internal sealed class ArchiveDirectoryListing
{
    public List<TrainerCatalogItemDto> Files { get; set; }=new();
    public List<Uri> Directories { get; set; }=new();
}
