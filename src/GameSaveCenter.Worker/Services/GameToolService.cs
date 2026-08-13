using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Imports, launches and tracks local trainers and Cheat Engine tables.</summary>
public sealed class GameToolService
{
    private const int MaxArchiveEntryCount=5000;
    private const long MaxArchiveEntryBytes=1024L*1024*1024;
    private const long MaxArchiveExpandedBytes=4L*1024*1024*1024;
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ITrainerCatalogSource _catalog;
    private readonly TaskCoordinator _tasks;
    private readonly ILogger<GameToolService> _logger;
    private readonly GameToolSessionTracker _sessionTracker=new();
    private readonly IShortcutResolver _shortcutResolver;

    public GameToolService(WorkerOptions options,SqliteStateStore store,ITrainerCatalogSource catalog,TaskCoordinator tasks,ILogger<GameToolService> logger,IShortcutResolver? shortcutResolver=null)
    {_options=options;_store=store;_catalog=catalog;_tasks=tasks;_logger=logger;_shortcutResolver=shortcutResolver??new WindowsShortcutResolver();}

    public Task<List<GameToolDto>> ListAsync(string gameId,CancellationToken token)=>_store.GetGameToolsAsync(gameId,token);

    public Task<GameToolImportInspectionDto> InspectImportAsync(InspectGameToolImportRequestDto request,CancellationToken token)
    {
        var source=Path.GetFullPath(Environment.ExpandEnvironmentVariables(request.SourcePath??string.Empty));
        if(!File.Exists(source)&&!Directory.Exists(source))throw new FileNotFoundException("导入源不存在。",source);
        var extension=request.ToolType switch
        {
            GameToolType.CheatTable=>".ct",
            GameToolType.CustomExecutable=>null,
            _=>".exe"
        };
        var candidates=new List<GameToolEntryCandidateDto>();
        if(Directory.Exists(source))
        {
            foreach(var path in Directory.EnumerateFiles(source,"*",SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                if(!IsEntryCandidate(path,extension))continue;
                candidates.Add(new GameToolEntryCandidateDto{RelativePath=Path.GetRelativePath(source,path),SizeBytes=new FileInfo(path).Length});
            }
        }
        else if(source.EndsWith(".zip",StringComparison.OrdinalIgnoreCase)&&request.ToolType!=GameToolType.CustomExecutable)
        {
            using var zip=ZipFile.OpenRead(source);
            ValidateArchiveShape(zip);
            foreach(var entry in zip.Entries)
            {
                token.ThrowIfCancellationRequested();
                if(string.IsNullOrEmpty(entry.Name)||!IsEntryCandidate(entry.FullName,extension))continue;
                // Validate every selectable entry before showing it to the user.
                ArchivePathGuard.ResolveEntryPath(Path.Combine(Path.GetTempPath(),"GameSaveCenterImportInspection"),entry.FullName);
                candidates.Add(new GameToolEntryCandidateDto{RelativePath=entry.FullName.Replace('/',Path.DirectorySeparatorChar),SizeBytes=entry.Length});
            }
        }
        else if(IsEntryCandidate(source,extension))
            candidates.Add(new GameToolEntryCandidateDto{RelativePath=Path.GetFileName(source),SizeBytes=new FileInfo(source).Length});

        candidates=candidates.OrderByDescending(x=>IsLikelyPrimaryEntry(x.RelativePath))
            .ThenByDescending(x=>x.SizeBytes).ThenBy(x=>x.RelativePath,StringComparer.OrdinalIgnoreCase).ToList();
        if(candidates.Count==0)throw new InvalidDataException($"未在导入内容中找到 {(extension??"可启动文件")}。");
        return Task.FromResult(new GameToolImportInspectionDto{SourcePath=source,ToolType=request.ToolType,Candidates=candidates});
    }

    public async Task<GameToolDto> ImportAsync(ImportGameToolRequestDto request,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(request.PlayniteId))throw new ArgumentException("必须选择目标游戏。");
        var source=Path.GetFullPath(Environment.ExpandEnvironmentVariables(request.SourcePath??string.Empty));
        if(!File.Exists(source)&&!Directory.Exists(source))throw new FileNotFoundException("导入源不存在。",source);
        if(request.ToolType==GameToolType.CheatTable&&!source.EndsWith(".ct",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cheat Table 必须是 .ct 文件。");

        var toolId=Guid.NewGuid().ToString("N");var versionId=Guid.NewGuid().ToString("N");
        var requiresStorage=(File.Exists(source)&&source.EndsWith(".zip",StringComparison.OrdinalIgnoreCase)&&request.ToolType!=GameToolType.CustomExecutable)
                            ||Directory.Exists(source)
                            ||request.CopyIntoLibrary;
        var root=requiresStorage
            ? Path.Combine(_options.GameToolsDirectory,SafeSegment(request.PlayniteId),toolId,versionId)
            : string.Empty;
        if(requiresStorage)Directory.CreateDirectory(root);
        string entry;
        if(File.Exists(source)&&source.EndsWith(".zip",StringComparison.OrdinalIgnoreCase)&&request.ToolType!=GameToolType.CustomExecutable)
        {
            ExtractZipSafely(source,root);
            entry=SelectEntry(root,request.EntryFileName,request.ToolType);
        }
        else if(Directory.Exists(source))
        {
            CopyDirectory(source,root,token);
            entry=SelectEntry(root,request.EntryFileName,request.ToolType);
        }
        else if(request.CopyIntoLibrary)
        {
            entry=Path.Combine(root,Path.GetFileName(source));File.Copy(source,entry,false);
        }
        else entry=source;

        var now=DateTime.UtcNow;
        var tool=new GameToolDto
        {
            ToolId=toolId,PlayniteId=request.PlayniteId,ToolType=request.ToolType,SourceType=GameToolSourceType.Manual,
            DisplayName=string.IsNullOrWhiteSpace(request.DisplayName)?Path.GetFileNameWithoutExtension(entry):request.DisplayName.Trim(),
            Enabled=true,AutoStart=false,LaunchDelaySeconds=8,ActiveVersionId=versionId,CreatedUtc=now,UpdatedUtc=now
        };
        var version=new GameToolVersionDto
        {
            VersionId=versionId,ToolId=toolId,VersionName=string.IsNullOrWhiteSpace(request.VersionName)?"本地版本":request.VersionName.Trim(),
            EntryPath=entry,WorkingDirectory=Path.GetDirectoryName(entry)??root,
            ResolvedTargetPath=request.ToolType==GameToolType.CustomExecutable?ResolveTrackTargetPath(entry):string.Empty,
            FileSha256=await HashAsync(entry,token).ConfigureAwait(false),
            CreatedUtc=now,IsAvailable=true
        };
        await _store.UpsertGameToolAsync(tool,version,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("GameTool","已导入游戏工具",System.Text.Json.JsonSerializer.Serialize(new{tool.ToolId,tool.PlayniteId,tool.DisplayName,tool.ToolType}),token).ConfigureAwait(false);
        return (await _store.GetGameToolAsync(toolId,token).ConfigureAwait(false))!;
    }

    public async Task<object> UpdateAsync(UpdateGameToolRequestDto request,CancellationToken token)
    {
        var tool=await _store.GetGameToolAsync(request.ToolId,token).ConfigureAwait(false)??throw new KeyNotFoundException("游戏工具不存在。");
        if(tool.ToolType!=GameToolType.CustomExecutable)
        {
            request.IfAlreadyRunning=GameToolIfAlreadyRunning.Skip;
            request.RiskCategory=GameToolRiskCategory.Unknown;
        }
        await _store.UpdateGameToolAsync(request,token).ConfigureAwait(false);return new{updated=true};
    }

    public async Task<GameToolDto> RelocateAsync(RelocateGameToolRequestDto request,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(request.SourcePath))throw new ArgumentException("必须选择新的文件路径。");
        var tool=await _store.GetGameToolAsync(request.ToolId,token).ConfigureAwait(false)??throw new KeyNotFoundException("游戏工具不存在。");
        var source=Path.GetFullPath(Environment.ExpandEnvironmentVariables(request.SourcePath));
        if(!File.Exists(source))throw new FileNotFoundException("重定位文件不存在。",source);
        var currentWorking=tool.ActiveVersion.WorkingDirectory;
        var oldEntryDirectory=Path.GetDirectoryName(tool.ActiveVersion.EntryPath)??string.Empty;
        var workingIsAutoDerived=string.Equals(
            NormalizeDirectory(currentWorking),
            NormalizeDirectory(oldEntryDirectory),
            StringComparison.OrdinalIgnoreCase);
        var workingDirectory=workingIsAutoDerived||!Directory.Exists(currentWorking)
            ? Path.GetDirectoryName(source)??string.Empty
            : currentWorking;
        var resolved=ResolveTrackTargetPath(source);
        var hash=await HashAsync(source,token).ConfigureAwait(false);
        await _store.RelocateGameToolAsync(tool.ToolId,source,workingDirectory,resolved,hash,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("GameTool","已重新定位游戏工具",
            System.Text.Json.JsonSerializer.Serialize(new{toolId=tool.ToolId,displayName=tool.DisplayName,path=source}),token).ConfigureAwait(false);
        return (await _store.GetGameToolAsync(tool.ToolId,token).ConfigureAwait(false))!;
    }

    public async Task<object> DeleteAsync(string toolId,CancellationToken token)
    {
        if(await _store.GetGameToolAsync(toolId,token).ConfigureAwait(false)==null)throw new KeyNotFoundException("游戏工具不存在。");
        await _store.DeleteGameToolAsync(toolId,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("GameTool","已解除游戏工具绑定",System.Text.Json.JsonSerializer.Serialize(new{toolId}),token).ConfigureAwait(false);
        return new{deleted=true};
    }

    public async Task<object> LaunchAsync(string toolId,CancellationToken token)
    {
        var tool=await _store.GetGameToolAsync(toolId,token).ConfigureAwait(false)??throw new KeyNotFoundException("游戏工具不存在。");
        var decision=PrepareLaunch(tool);
        if(decision.Skipped)
            return new{started=false,skipped=true,processId=0,existingProcessIds=decision.ExistingProcessIds};
        var (process,_)=LaunchWithPlan(tool);return new{started=true,processId=process.Id};
    }

    public async Task<object> OpenDirectoryAsync(string toolId,CancellationToken token)
    {
        var tool=await _store.GetGameToolAsync(toolId,token).ConfigureAwait(false)??throw new KeyNotFoundException("游戏工具不存在。");
        var directory=Path.GetDirectoryName(tool.ActiveVersion.EntryPath);
        if(string.IsNullOrWhiteSpace(directory)||!Directory.Exists(directory))throw new DirectoryNotFoundException(directory);
        Process.Start(new ProcessStartInfo{FileName=directory,UseShellExecute=true});return new{opened=true};
    }

    public async Task StartAutomaticAsync(GameSessionEventDto session,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(session.SessionId))return;
        var descriptor=await _store.GetGameAsync(session.PlayniteId,token).ConfigureAwait(false);
        var tools=(await _store.GetGameToolsAsync(session.PlayniteId,token).ConfigureAwait(false)).Where(x=>x.Enabled&&x.AutoStart).ToList();
        var antiCheat=HasAntiCheat(descriptor);
        var allowed=new List<GameToolDto>();
        foreach(var tool in tools)
        {
            if(!GameToolAutoStartPolicy.IsAllowed(tool,antiCheat,out var blockedReason))
            {
                await RecordAutoStartBlockedAsync(tool,session,blockedReason,token).ConfigureAwait(false);
                continue;
            }
            allowed.Add(tool);
        }
        tools=allowed;
        if(tools.Count==0)return;
        var linked=CancellationTokenSource.CreateLinkedTokenSource(token);_sessionTracker.RegisterDelay(session.SessionId,linked);
        foreach(var tool in tools)_=LaunchAfterDelayAsync(session,tool,linked.Token);
    }

    public async Task StopAutomaticAsync(string sessionId,CancellationToken token)
    {
        await _sessionTracker.CloseSessionAsync(sessionId,TimeSpan.FromSeconds(2.5),_logger,token).ConfigureAwait(false);
    }

    public async Task<TaskStatusDto> DownloadAsync(DownloadTrainerRequestDto request,CancellationToken token)
    {
        var game=await _store.GetGameAsync(request.PlayniteId,token).ConfigureAwait(false);
        return await _tasks.RunAsync("TrainerDownload",request.PlayniteId,game?.Name??"游戏",async(progress,taskToken)=>
        {
            var catalog=await _store.GetTrainerCatalogItemAsync(request.CatalogId,taskToken).ConfigureAwait(false)
                        ??throw new KeyNotFoundException("FLiNG 目录项不存在。");
            var release=await _store.GetTrainerReleaseAsync(request.ReleaseId,taskToken).ConfigureAwait(false)
                        ??throw new KeyNotFoundException("FLiNG 版本不存在。");
            var installed=await _store.GetGameToolsAsync(request.PlayniteId,taskToken).ConfigureAwait(false);
            var existingTool=installed.FirstOrDefault(x=>x.SourceType==GameToolSourceType.Fling&&
                string.Equals(x.DisplayName,catalog.Title,StringComparison.OrdinalIgnoreCase));
            if(existingTool?.Versions.Any(x=>string.Equals(x.SourceUrl,release.DownloadUrl,StringComparison.OrdinalIgnoreCase))==true)
            {
                await progress.ReportAsync(100,"该 FLiNG 版本已经绑定，无需重复下载").ConfigureAwait(false);
                return;
            }
            var temporary=Path.Combine(_options.DownloadDirectory,request.ReleaseId+"."+Guid.NewGuid().ToString("N")+".download");
            await progress.ReportAsync(5,"正在下载 FLiNG 修改器").ConfigureAwait(false);
            var sink=new Progress<(long Received,long? Total)>(value=>
            {
                var percent=value.Total>0?(int)Math.Min(80,5+value.Received*75/value.Total.Value):35;
                _=progress.ReportAsync(percent,"正在下载 FLiNG 修改器");
            });
            await _catalog.DownloadAsync(request.ReleaseId,temporary,sink,taskToken).ConfigureAwait(false);
            await progress.ReportAsync(82,"正在安全解压").ConfigureAwait(false);
            var toolId=existingTool?.ToolId??Guid.NewGuid().ToString("N");var versionId=Guid.NewGuid().ToString("N");
            var root=Path.Combine(_options.GameToolsDirectory,SafeSegment(request.PlayniteId),toolId,versionId);Directory.CreateDirectory(root);
            var installedSuccessfully=false;
            try
            {
                string entry;
                if(HasSignature(temporary,0x50,0x4B))
                {
                    ExtractZipSafely(temporary,root);
                    entry=SelectEntry(root,string.Empty,GameToolType.Trainer);
                }
                else if(HasSignature(temporary,0x4D,0x5A))
                {
                    entry=Path.Combine(root,SafeSegment(release.DisplayName)+".exe");
                    File.Move(temporary,entry,true);
                }
                else
                {
                    throw new WorkerOperationException("FLING_DOWNLOAD_INVALID","下载内容既不是 ZIP 也不是 Windows 可执行文件，已拒绝绑定。",release.DownloadUrl);
                }

                var now=DateTime.UtcNow;
                var tool=existingTool??new GameToolDto{ToolId=toolId,PlayniteId=request.PlayniteId,ToolType=GameToolType.Trainer,SourceType=GameToolSourceType.Fling,
                    DisplayName=catalog.Title,Enabled=true,AutoStart=false,LaunchDelaySeconds=8,CreatedUtc=now};
                tool.ActiveVersionId=versionId;tool.UpdatedUtc=now;
                var version=new GameToolVersionDto{VersionId=versionId,ToolId=toolId,VersionName=release.DisplayName,EntryPath=entry,
                    WorkingDirectory=Path.GetDirectoryName(entry)??root,SourceUrl=release.DownloadUrl,FileSha256=await HashAsync(entry,taskToken).ConfigureAwait(false),
                    DownloadUtc=now,CreatedUtc=now,IsAvailable=true};
                await _store.UpsertGameToolAsync(tool,version,taskToken).ConfigureAwait(false);
                installedSuccessfully=true;
                await progress.ReportAsync(96,"已下载并绑定到当前游戏").ConfigureAwait(false);
            }
            catch
            {
                if(!installedSuccessfully&&Directory.Exists(root))Directory.Delete(root,true);
                throw;
            }
            finally
            {
                if(File.Exists(temporary))File.Delete(temporary);
            }
        },token).ConfigureAwait(false);
    }

    private async Task LaunchAfterDelayAsync(GameSessionEventDto session,GameToolDto tool,CancellationToken token)
    {
        try
        {
            var delay=tool.LaunchTiming==GameToolLaunchTiming.Delayed?Math.Clamp(tool.LaunchDelaySeconds,0,300):0;
            if(delay>0)await Task.Delay(TimeSpan.FromSeconds(delay),token).ConfigureAwait(false);
            var decision=PrepareLaunch(tool);
            if(decision.Skipped)
            {
                await _store.AppendAuditAsync("GameTool","已有同一路径实例，已跳过自动启动",
                    System.Text.Json.JsonSerializer.Serialize(new{session.SessionId,tool.ToolId,tool.DisplayName,existingProcessIds=decision.ExistingProcessIds}),CancellationToken.None).ConfigureAwait(false);
                return;
            }
            var (process,trackable)=LaunchWithPlan(tool);
            var closeOnExit=tool.CloseOnGameExit&&trackable;
            _sessionTracker.Track(session.SessionId,process.Id,process.StartTime.ToUniversalTime(),closeOnExit);
            await _store.AppendAuditAsync("GameTool","已随游戏启动工具",
                System.Text.Json.JsonSerializer.Serialize(new{session.SessionId,tool.ToolId,tool.DisplayName,processId=process.Id}),CancellationToken.None).ConfigureAwait(false);
        }
        catch(OperationCanceledException){}
        catch(Exception ex)
        {
            _logger.LogError(ex,"Automatic game tool launch failed for {Tool}",tool.DisplayName);
            await _store.AppendAuditAsync("GameTool","随游戏启动工具失败",
                System.Text.Json.JsonSerializer.Serialize(new{tool.ToolId,tool.DisplayName,error=ex.Message}),CancellationToken.None).ConfigureAwait(false);
        }
    }

    private (Process Process,bool Trackable) LaunchWithPlan(GameToolDto tool)
    {
        var plan=GameToolLauncher.Build(tool,_shortcutResolver);
        var process=Process.Start(plan.StartInfo)??throw new InvalidOperationException("Windows 未返回工具进程。");
        return (process,plan.Trackable);
    }

    private LaunchDecision PrepareLaunch(GameToolDto tool)
    {
        if(tool.ToolType!=GameToolType.CustomExecutable||tool.IfAlreadyRunning==GameToolIfAlreadyRunning.AllowAnotherInstance)
            return LaunchDecision.Start;
        var target=GameToolProcessGuard.ResolveExecutableTarget(tool,_shortcutResolver);
        if(string.IsNullOrWhiteSpace(target))
            throw new WorkerOperationException("GAME_TOOL_TARGET_UNKNOWN","无法安全判断自定义启动项的目标进程；请选择“允许多开”或改用可解析的 EXE。",tool.ActiveVersion.EntryPath);
        var scan=GameToolProcessGuard.Scan(target);
        var action=GameToolProcessGuard.Decide(tool.IfAlreadyRunning,scan);
        if(action==GameToolProcessGuard.ExistingProcessAction.BlockUnreadable)
            throw new WorkerOperationException("GAME_TOOL_PROCESS_UNREADABLE","发现同名进程但当前会话无法读取其程序路径；为避免误操作，已停止启动。请使用管理员权限检查进程，或选择“允许多开”。",target);
        if(action==GameToolProcessGuard.ExistingProcessAction.Start)
            return LaunchDecision.Start;
        if(action==GameToolProcessGuard.ExistingProcessAction.Skip)
            return new LaunchDecision(true,scan.MatchingProcessIds);
        GameToolProcessGuard.RestartExact(target,scan,TimeSpan.FromSeconds(2.5));
        return LaunchDecision.Start;
    }

    private async Task RecordAutoStartBlockedAsync(GameToolDto tool,GameSessionEventDto session,string reason,CancellationToken token)
    {
        await _store.AppendAuditAsync("GameTool",reason,
            System.Text.Json.JsonSerializer.Serialize(new{session.PlayniteId,session.GameName,tool.ToolId,tool.DisplayName,tool.RiskCategory}),token).ConfigureAwait(false);
    }

    private sealed record LaunchDecision(bool Skipped,IReadOnlyList<int> ExistingProcessIds)
    {
        public static LaunchDecision Start { get; }=new(false,Array.Empty<int>());
    }

    private static void ExtractZipSafely(string archive,string destination)
    {
        using var zip=ZipFile.OpenRead(archive);
        ValidateArchiveShape(zip);
        foreach(var entry in zip.Entries)
        {
            string target;
            try{target=ArchivePathGuard.ResolveEntryPath(destination,entry.FullName);}
            catch(InvalidDataException ex){throw new InvalidDataException("ZIP 包含越界路径，已拒绝解压。",ex);}
            if(string.IsNullOrEmpty(entry.Name)){Directory.CreateDirectory(target);continue;}
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);entry.ExtractToFile(target,false);
        }
    }

    private static void ValidateArchiveShape(ZipArchive zip)
    {
        if(zip.Entries.Count>MaxArchiveEntryCount)
            throw new InvalidDataException($"ZIP 包含过多文件（{zip.Entries.Count}），已拒绝解压。");
        long expandedBytes=0;
        foreach(var entry in zip.Entries)
        {
            if(entry.Length>MaxArchiveEntryBytes)
                throw new InvalidDataException($"ZIP 包含超过安全大小上限的文件：{entry.FullName}");
            expandedBytes=checked(expandedBytes+entry.Length);
            if(expandedBytes>MaxArchiveExpandedBytes)
                throw new InvalidDataException("ZIP 解压后的总大小超过安全上限，已拒绝解压。");
        }
    }

    private static string SelectEntry(string root,string requested,GameToolType type)
    {
        if(!string.IsNullOrWhiteSpace(requested))
        {
            var selected=Path.GetFullPath(Path.Combine(root,requested));
            if(selected.StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)&&File.Exists(selected))return selected;
        }
        if(type==GameToolType.CustomExecutable)
        {
            var files=Directory.GetFiles(root,"*",SearchOption.AllDirectories)
                .OrderByDescending(x=>new FileInfo(x).Length).ToList();
            if(files.Count==0)throw new InvalidDataException("未在导入内容中找到可启动文件。");
            return files[0];
        }
        var extension=type==GameToolType.CheatTable?"*.ct":"*.exe";
        var candidates=Directory.GetFiles(root,extension,SearchOption.AllDirectories)
            .Where(x=>!Path.GetFileName(x).Contains("unins",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileName(x).Contains("update",StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x=>new FileInfo(x).Length).ToList();
        if(candidates.Count==0)throw new InvalidDataException($"未在导入内容中找到 {extension}。");
        return candidates[0];
    }

    private static bool IsEntryCandidate(string path,string? extension)
    {
        if(extension!=null&&!path.EndsWith(extension,StringComparison.OrdinalIgnoreCase))return false;
        return extension==null
            ||(!Path.GetFileName(path).Contains("unins",StringComparison.OrdinalIgnoreCase)
               &&!Path.GetFileName(path).Contains("update",StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsLikelyPrimaryEntry(string path)
    {
        var name=Path.GetFileNameWithoutExtension(path);
        return name.Contains("trainer",StringComparison.OrdinalIgnoreCase)
               ||name.Contains("fling",StringComparison.OrdinalIgnoreCase)
               ||name.Contains("launcher",StringComparison.OrdinalIgnoreCase);
    }

    private static void CopyDirectory(string source,string target,CancellationToken token)
    {
        foreach(var directory in Directory.GetDirectories(source,"*",SearchOption.AllDirectories))
        {token.ThrowIfCancellationRequested();Directory.CreateDirectory(Path.Combine(target,Path.GetRelativePath(source,directory)));}
        foreach(var file in Directory.GetFiles(source,"*",SearchOption.AllDirectories))
        {token.ThrowIfCancellationRequested();var destination=Path.Combine(target,Path.GetRelativePath(source,file));Directory.CreateDirectory(Path.GetDirectoryName(destination)!);File.Copy(file,destination,false);}
    }

    private static async Task<string> HashAsync(string path,CancellationToken token)
    {
        await using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,81920,true);
        using var hash=SHA256.Create();var value=await hash.ComputeHashAsync(stream,token).ConfigureAwait(false);
        return Convert.ToHexString(value).ToLowerInvariant();
    }

    private static bool HasAntiCheat(GameDescriptorDto? game)
    {
        if(game==null)return false;
        var values=game.Actions.SelectMany(x=>new[]{x.Name,x.Path,x.Arguments}).Concat(game.KnownProcessNames).Concat(game.Tags);
        return values.Any(x=>!string.IsNullOrWhiteSpace(x)&&(x.Contains("easyanticheat",StringComparison.OrdinalIgnoreCase)||
            x.Contains("easy anti-cheat",StringComparison.OrdinalIgnoreCase)||x.Contains("battleye",StringComparison.OrdinalIgnoreCase)||
            x.Contains("ricochet",StringComparison.OrdinalIgnoreCase)||x.Contains("vanguard",StringComparison.OrdinalIgnoreCase)));
    }

    private string ResolveTrackTargetPath(string entryPath)
    {
        if(GameToolLaunchKinds.FromPath(entryPath)!=GameToolLaunchKind.Shortcut)return entryPath;
        var target=_shortcutResolver.Resolve(entryPath);
        if(string.IsNullOrWhiteSpace(target.TargetPath))
            throw new InvalidOperationException("快捷方式目标为空，无法解析，请重新定位或删除。");
        return target.TargetPath;
    }

    private static string NormalizeDirectory(string path)
    {
        if(string.IsNullOrWhiteSpace(path))return string.Empty;
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
    }


    private static bool HasSignature(string path,byte first,byte second)
    {
        using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        return stream.ReadByte()==first&&stream.ReadByte()==second;
    }

    private static string SafeSegment(string value)
    {
        var invalid=Path.GetInvalidFileNameChars();var clean=new string((value??string.Empty).Where(x=>!invalid.Contains(x)).ToArray());
        return string.IsNullOrWhiteSpace(clean)?"unnamed":clean.Length>80?clean[..80]:clean;
    }

}
