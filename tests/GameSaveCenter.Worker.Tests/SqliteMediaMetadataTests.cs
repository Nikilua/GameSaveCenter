using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SqliteMediaMetadataTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"GameSaveCenter.Tests",Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public SqliteMediaMetadataTests()
    {
        var options=new WorkerOptions
        {
            DataDirectory=root,
            LudusaviBackupDirectory=Path.Combine(root,"Saves"),
            MediaArchiveDirectory=Path.Combine(root,"Media")
        };
        store=new SqliteStateStore(options,NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task BatchMetadataUpdate_IsAtomicAndPreservesUnchangedFields()
    {
        await AddAsync("one",false,"first");
        await AddAsync("two",false,"second");

        await store.UpdateMediaMetadataBatchAsync(new MediaMetadataBatchUpdateDto
        {
            MediaIds=new List<string>{"one","two"},
            IsFavorite=true,
            UpdateComment=false,
            Comment="ignored"
        },CancellationToken.None);

        var one=await store.GetMediaByIdAsync("one",CancellationToken.None);
        var two=await store.GetMediaByIdAsync("two",CancellationToken.None);
        Assert.True(one!.IsFavorite);
        Assert.True(two!.IsFavorite);
        Assert.Equal("first",one.Comment);
        Assert.Equal("second",two.Comment);

        await Assert.ThrowsAsync<InvalidOperationException>(()=>store.UpdateMediaMetadataBatchAsync(new MediaMetadataBatchUpdateDto
        {
            MediaIds=new List<string>{"one","missing"},
            IsFavorite=false
        },CancellationToken.None));

        one=await store.GetMediaByIdAsync("one",CancellationToken.None);
        Assert.True(one!.IsFavorite);
    }

    [Fact]
    public async Task IgnoredMediaCanBeListedAndRestoredToInbox()
    {
        await AddAsync("ignored",false,string.Empty,"Ignored");
        await AddAsync("assigned",false,string.Empty,"Assigned");

        var ignored=await store.GetIgnoredMediaAsync(50,CancellationToken.None);
        Assert.Single(ignored);
        Assert.Equal("ignored",ignored[0].MediaId);

        var restoredPath=Path.Combine(root,"Media","_Inbox","Pending","ignored.png");
        await store.RestoreMediaToInboxAsync("ignored",restoredPath,CancellationToken.None);

        var restored=await store.GetMediaByIdAsync("ignored",CancellationToken.None);
        Assert.NotNull(restored);
        Assert.Equal("Inbox",restored!.ClassificationState);
        Assert.Equal(string.Empty,restored.PlayniteId);
        Assert.Equal(restoredPath,restored.ArchivePath);
        Assert.Equal("用户撤销忽略，待重新归类",restored.ClassificationReason);
        Assert.Empty(await store.GetIgnoredMediaAsync(50,CancellationToken.None));
        Assert.Single(await store.GetUnassignedMediaAsync(50,CancellationToken.None));
    }

    [Fact]
    public async Task InboxMediaCanBeReadInBoundedPagesWithoutRepeatingRows()
    {
        await AddAsync("page-one",false,string.Empty,"Inbox",DateTime.UtcNow.AddMinutes(-1));
        await AddAsync("page-two",false,string.Empty,"Inbox",DateTime.UtcNow.AddMinutes(-2));
        await AddAsync("page-three",false,string.Empty,"Inbox",DateTime.UtcNow.AddMinutes(-3));

        var firstPage = await store.GetUnassignedMediaAsync(2,CancellationToken.None,0);
        var secondPage = await store.GetUnassignedMediaAsync(2,CancellationToken.None,2);

        Assert.Equal(new[] { "page-one", "page-two" }, firstPage.Select(x => x.MediaId));
        Assert.Equal(new[] { "page-three" }, secondPage.Select(x => x.MediaId));
        Assert.Empty(firstPage.Select(x => x.MediaId).Intersect(secondPage.Select(x => x.MediaId)));
    }

    [Fact]
    public async Task DeviceConflictDecision_IsPersistedWithoutFileOperations()
    {
        var decision=new DeviceConflictDecisionDto
        {
            PlayniteId="game",RemoteDevice="OTHER-PC",LocalBackupId="local",
            RemoteBackupId="remote",Decision="KeepBoth",Comment="manual review",DecidedUtc=DateTime.UtcNow
        };
        await store.SaveDeviceConflictDecisionAsync(decision,CancellationToken.None);
        var loaded=await store.GetDeviceConflictDecisionAsync("game","OTHER-PC",CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal("KeepBoth",loaded!.Decision);
        Assert.Equal("manual review",loaded.Comment);
    }

    [Theory]
    [InlineData("PreferLocal")]
    [InlineData("PreferRemote")]
    [InlineData("KeepBoth")]
    public async Task EveryExplicitConflictChoicePersistsWithoutTouchingEitherBranch(string choice)
    {
        var remoteDeviceId = Guid.NewGuid().ToString("N");
        await store.SaveDeviceConflictDecisionAsync(new DeviceConflictDecisionDto
        {
            PlayniteId = "branch-game", RemoteDevice = remoteDeviceId,
            LocalBackupId = "A3", RemoteBackupId = "B3", Decision = choice,
            Comment = "explicit user decision", DecidedUtc = DateTime.UtcNow
        }, CancellationToken.None);

        var loaded = await store.GetDeviceConflictDecisionAsync("branch-game", remoteDeviceId, CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal(choice, loaded!.Decision);
        Assert.Equal("A3", loaded.LocalBackupId);
        Assert.Equal("B3", loaded.RemoteBackupId);
    }

    [Fact]
    public async Task MediaSourceRule_CanBePausedAndDeletedWithoutTouchingArchivedMedia()
    {
        var source=new MediaSourceRuleDto
        {
            SourceId="custom-source",
            PlayniteId="game",
            SourceKind=MediaSourceKind.Custom,
            RootPath=root,
            IncludePattern="*.png",
            Enabled=true
        };
        await store.AddMediaSourceAsync(source,CancellationToken.None);
        source.Enabled=false;
        await store.AddMediaSourceAsync(source,CancellationToken.None);

        var paused=await store.GetMediaSourcesAsync("game",CancellationToken.None);
        Assert.Single(paused);
        Assert.False(paused[0].Enabled);

        await store.DeleteMediaSourceAsync(source.SourceId,CancellationToken.None);
        Assert.Empty(await store.GetMediaSourcesAsync("game",CancellationToken.None));
        Assert.True(Directory.Exists(root));
    }

    [Fact]
    public async Task UpsertedGameKeepsMatchAttemptUnsetUntilMatchingCompletes()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto
            {
                PlayniteId="pending-game",
                Name="Pending Game",
                Platform=GamePlatformKind.Steam,
                PlatformGameId="123",
                IsInstalled=true
            }
        }, CancellationToken.None);

        var cache = await store.GetGameMatchCacheAsync(CancellationToken.None);
        Assert.True(cache.ContainsKey("pending-game"));
        Assert.Null(cache["pending-game"].LastMatchAttemptUtc);
        Assert.Empty(cache["pending-game"].LudusaviName);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if(Directory.Exists(root))Directory.Delete(root,true);
    }

    private Task AddAsync(string id,bool favorite,string comment,string classificationState="Assigned",DateTime? capturedUtc=null)=>store.AddMediaAsync(new MediaItemDto
    {
        MediaId=id,
        PlayniteId="game",
        Kind=MediaKind.Screenshot,
        Source=MediaSourceKind.Custom,
        ArchivePath=Path.Combine(root,id+".png"),
        OriginalPath=Path.Combine(root,id+".png"),
        CapturedUtc=capturedUtc??DateTime.UtcNow,
        SizeBytes=10,
        Sha256=id.PadRight(64,'0'),
        IsFavorite=favorite,
        Comment=comment,
        ClassificationState=classificationState
    },CancellationToken.None);
}
