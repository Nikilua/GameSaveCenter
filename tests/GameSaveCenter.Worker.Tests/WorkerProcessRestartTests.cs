using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

/// <summary>
/// Starts the real Worker executable in an isolated instance, hard-stops it, and verifies
/// that the next process reconciles an unfinished durable task before serving IPC.
/// </summary>
public sealed class WorkerProcessRestartTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [WorkerProcessFact]
    public async Task HardRestartReconcilesDurableIncompleteTask()
    {
        var root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.WorkerProcess", Guid.NewGuid().ToString("N"));
        var dataDirectory = Path.Combine(root, "Data");
        var savesDirectory = Path.Combine(root, "Saves");
        var mediaDirectory = Path.Combine(root, "Media");
        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(savesDirectory);
        Directory.CreateDirectory(mediaDirectory);

        var options = new WorkerOptions
        {
            DataDirectory = dataDirectory,
            LudusaviBackupDirectory = savesDirectory,
            MediaArchiveDirectory = mediaDirectory
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        Process? first = null;
        Process? second = null;
        try
        {
            await store.InitializeAsync(CancellationToken.None);

            var pipeName = "GameSaveCenter.E01." + Guid.NewGuid().ToString("N");
            var eventPipeName = pipeName + ".Events";
            var workerAssembly = typeof(GameSaveCenter.Worker.Program).Assembly.Location;
            Assert.True(File.Exists(workerAssembly), "测试输出中缺少 GameSaveCenter.Worker.dll。");

            first = StartWorker(workerAssembly, dataDirectory, savesDirectory, mediaDirectory, pipeName, eventPipeName);
            await WaitForPingAsync(first, pipeName, TimeSpan.FromSeconds(15));

            var taskId = "e01-crashed-task-" + Guid.NewGuid().ToString("N");
            await store.AddOrUpdateTaskAsync(new TaskStatusDto
            {
                TaskId = taskId,
                WorkerSessionId = "worker-before-hard-stop",
                TaskType = "Backup",
                GameId = "e01-game",
                GameName = "E01 isolated fixture",
                State = TaskState.Running,
                ProgressPercent = 47,
                Message = "正在执行",
                CreatedUtc = DateTime.UtcNow.AddSeconds(-2),
                StartedUtc = DateTime.UtcNow.AddSeconds(-1)
            }, CancellationToken.None);

            var seeded = (await store.GetRecentTasksAsync(20, CancellationToken.None))
                .Single(task => task.TaskId == taskId);
            Assert.Equal(TaskState.Running, seeded.State);

            Assert.True(HardStop(first), "第一次 Worker 未在硬停止后退出。");

            second = StartWorker(workerAssembly, dataDirectory, savesDirectory, mediaDirectory, pipeName, eventPipeName);
            await WaitForPingAsync(second, pipeName, TimeSpan.FromSeconds(15));

            var deadline = DateTime.UtcNow.AddSeconds(5);
            TaskStatusDto? reconciled = null;
            while (DateTime.UtcNow < deadline)
            {
                reconciled = (await store.GetRecentTasksAsync(20, CancellationToken.None))
                    .SingleOrDefault(task => task.TaskId == taskId);
                if (reconciled?.State == TaskState.Failed)
                    break;
                await Task.Delay(50).ConfigureAwait(false);
            }

            Assert.NotNull(reconciled);
            Assert.Equal(TaskState.Failed, reconciled!.State);
            Assert.Equal("WORKER_RESTARTED_RETRYABLE", reconciled.ErrorCode);
            Assert.Equal("worker-before-hard-stop", reconciled.WorkerSessionId);
        }
        finally
        {
            HardStop(first);
            HardStop(second);
            SqliteConnection.ClearAllPools();
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // A failed cleanup must not hide the process-level assertion. The fixture is
                // isolated under TEMP and can be removed by the next test run if locked.
            }
        }
    }

    private static Process StartWorker(
        string workerAssembly,
        string dataDirectory,
        string savesDirectory,
        string mediaDirectory,
        string pipeName,
        string eventPipeName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(workerAssembly) ?? AppContext.BaseDirectory
        };
        startInfo.ArgumentList.Add(workerAssembly);
        AddConfiguration(startInfo, "DataDirectory", dataDirectory);
        AddConfiguration(startInfo, "LudusaviBackupDirectory", savesDirectory);
        AddConfiguration(startInfo, "MediaArchiveDirectory", mediaDirectory);
        AddConfiguration(startInfo, "PipeName", pipeName);
        AddConfiguration(startInfo, "EventPipeName", eventPipeName);
        AddConfiguration(startInfo, "EnableProcessDetection", "false");
        AddConfiguration(startInfo, "EnableSessionSavePathDetection", "false");
        AddConfiguration(startInfo, "EnableMediaSync", "false");
        AddConfiguration(startInfo, "HealthInspectionEnabled", "false");
        AddConfiguration(startInfo, "EnableCloudUpload", "false");

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动隔离 GameSaveCenter Worker。");
    }

    private static void AddConfiguration(ProcessStartInfo startInfo, string key, string value)
        => startInfo.ArgumentList.Add("--GameSaveCenter:" + key + "=" + value);

    private static async Task WaitForPingAsync(Process process, string pipeName, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
                throw new InvalidOperationException($"隔离 Worker 提前退出，代码：{process.ExitCode}。", lastError);

            try
            {
                await using var client = new NamedPipeClientStream(
                    ".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                var remaining = Math.Max(100, (int)Math.Min(1000, (deadline - DateTime.UtcNow).TotalMilliseconds));
                await client.ConnectAsync(remaining).ConfigureAwait(false);
                using var reader = new StreamReader(client, new UTF8Encoding(false), false, 64 * 1024, leaveOpen: true);
                await using var writer = new StreamWriter(client, new UTF8Encoding(false), 64 * 1024, leaveOpen: true)
                {
                    AutoFlush = true
                };
                await writer.WriteLineAsync(JsonSerializer.Serialize(new IpcEnvelope
                {
                    Type = MessageTypes.Ping,
                    RequestId = Guid.NewGuid().ToString("N")
                })).ConfigureAwait(false);
                var line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromMilliseconds(remaining)).ConfigureAwait(false);
                var response = JsonSerializer.Deserialize<IpcEnvelope>(line ?? string.Empty, JsonOptions);
                if (response?.Success == true && string.Equals(response.Type, MessageTypes.Ping, StringComparison.Ordinal))
                    return;
                throw new InvalidOperationException("隔离 Worker 的 system.ping 响应无效。");
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or TimeoutException)
            {
                lastError = ex;
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        throw new TimeoutException("等待隔离 Worker 的真实 Named Pipe 就绪超时。", lastError);
    }

    internal static bool CanUseNamedPipes()
    {
        var name = "GameSaveCenter.E01.Probe." + Guid.NewGuid().ToString("N");
        try
        {
            using var server = new NamedPipeServerStream(
                name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var client = new NamedPipeClientStream(
                ".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
            client.Connect(500);
            server.WaitForConnection();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool HardStop(Process? process)
    {
        if (process == null)
            return true;
        var exited = true;
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            exited = process.HasExited;
        }
        catch (InvalidOperationException)
        {
            // The process may have exited between HasExited and Kill.
        }
        finally
        {
            process.Dispose();
        }
        return exited;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WorkerProcessFactAttribute : FactAttribute
{
    public WorkerProcessFactAttribute()
    {
        if (!WorkerProcessRestartTests.CanUseNamedPipes())
            Skip = "当前执行环境禁止创建本地 Named Pipe；在完整 Windows 环境执行独立 Worker 进程验收。";
    }
}
