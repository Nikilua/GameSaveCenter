using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Ipc;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Starts the packaged Worker, recovers stale instances and waits long enough
    /// for first-run SQLite/Defender initialization. Worker output is persisted for diagnostics.
    /// </summary>
    public sealed class WorkerLauncher
    {
        private static readonly object logGate = new object();
        private readonly WorkerIpcClient client;
        private readonly SemaphoreSlim startupGate = new SemaphoreSlim(1, 1);
        private Process? runningWorker;
        private string? workerLogPath;
        private volatile bool shutdownRequested;
        public WorkerLauncher(WorkerIpcClient client) { this.client = client; }

        public async Task EnsureStartedAsync(string executable, bool terminateUnhealthyProcess = true, string? expectedVersion = null, string? expectedBuildIdentity = null)
        {
            if (shutdownRequested) return;
            if (await IsHealthyAsync(expectedVersion: expectedVersion, expectedBuildIdentity: expectedBuildIdentity).ConfigureAwait(false)) return;
            await startupGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (shutdownRequested) return;
                if (await IsHealthyAsync(expectedVersion: expectedVersion, expectedBuildIdentity: expectedBuildIdentity).ConfigureAwait(false)) return;
                if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
                    throw new FileNotFoundException("未找到 GameSaveCenter Worker。", executable);
                if (!GameSaveCenterSettings.IsWorkerExecutable(executable))
                    throw new InvalidOperationException(
                        $"Worker 路径配置错误：{executable}。必须选择 GameSaveCenter.Worker.exe；Ludusavi 请填写在单独的 Ludusavi 路径中。");

                var fullExecutable = Path.GetFullPath(executable);
                var processName = Path.GetFileNameWithoutExtension(fullExecutable);
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "GameSaveCenter", "Logs", "worker-launch.log");
                workerLogPath = logPath;
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                var expectedVersionLabel = string.IsNullOrWhiteSpace(expectedVersion) ? "unknown" : expectedVersion;
                var expectedBuildLabel = string.IsNullOrWhiteSpace(expectedBuildIdentity) ? "unknown" : expectedBuildIdentity;
                // A Worker can be temporarily unable to answer a two-second Ping while it is
                // opening SQLite or yielding a large background Ludusavi batch.  Do not kill a
                // live instance merely because that first probe timed out: doing so loses the
                // durable request queue and creates the restart/pipe-timeout loop seen in large
                // Playnite libraries. Give the existing process a bounded grace period first.
                var existingBusyProcess = false;
                HealthProbeResult? existingIncompatibleProbe = null;
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        var runningPath = process.MainModule?.FileName;
                        if (!string.IsNullOrWhiteSpace(runningPath) &&
                            string.Equals(Path.GetFullPath(runningPath), fullExecutable, StringComparison.OrdinalIgnoreCase))
                        {
                            // Large SQLite stores and a first-run process scan can legitimately
                            // take longer than the old 12-second grace period.  Killing that
                            // instance created the exact restart/pipe-timeout loop reported by
                            // 900+ game libraries.  Give a live, same-path Worker enough time
                            // to finish initialization before treating it as wedged.
                            // A stable named pipe can belong to an older installed Worker.
                            // Treat a responding version mismatch differently from a busy
                            // current Worker: the former must be replaced, otherwise the new
                            // plugin silently reuses old startup/catalog behavior forever.
                            var probe = await ProbeHealthAsync(TimeSpan.FromSeconds(2), expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
                            if (IsTransient(probe.Status))
                                probe = await WaitForHealthAsync(TimeSpan.FromSeconds(45), expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
                            if (probe.IsHealthy) return;
                            AppendLog(logPath, DescribeHealth("Existing Worker health probe", probe, expectedVersion, expectedBuildIdentity));

                            // A large-library Worker may be healthy at the process level while
                            // temporarily unable to answer a short Ping during SQLite/Ludusavi
                            // work.  Never kill that live process from an interactive request;
                            // the caller can surface a bounded unavailable state and retry later.
                            // For small libraries retain the old last-resort recovery path for a
                            // genuinely stale process.
                            if (IsIncompatible(probe.Status) && !terminateUnhealthyProcess)
                            {
                                existingIncompatibleProbe = probe;
                                continue;
                            }

                            if (IsTransient(probe.Status) && !terminateUnhealthyProcess)
                            {
                                existingBusyProcess = !process.HasExited;
                                continue;
                            }

                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                            }
                        }
                    }
                    catch
                    {
                        // A process owned by another security context is left untouched.
                    }
                    finally { process.Dispose(); }
                }

                if (existingIncompatibleProbe != null)
                    throw new InvalidOperationException(
                        $"Worker 现有实例不兼容，已停止重复拉起。{DescribeHealth("Existing Worker", existingIncompatibleProbe, expectedVersion, expectedBuildIdentity)}");
                if (existingBusyProcess)
                    throw new TimeoutException("Worker 正在执行后台工作，暂时无法响应健康探测；已保留现有进程，稍后可重试。");

                AppendLog(logPath, $"Starting Worker: {fullExecutable} (expected GameSaveCenter Worker version {expectedVersionLabel}, build {expectedBuildLabel})");

                runningWorker?.Dispose();
                var worker = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fullExecutable,
                        WorkingDirectory = Path.GetDirectoryName(fullExecutable),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                worker.OutputDataReceived += (_, args) => AppendLog(logPath, args.Data);
                worker.ErrorDataReceived += (_, args) => AppendLog(logPath, args.Data);
                worker.Exited += (_, __) =>
                {
                    try { AppendLog(logPath, $"Worker exited with code {worker.ExitCode}."); }
                    catch { AppendLog(logPath, "Worker exited before its exit code could be read."); }
                };
                if (!worker.Start()) throw new InvalidOperationException("Worker 进程启动失败。");
                Interlocked.Exchange(ref runningWorker, worker);
                if (shutdownRequested)
                {
                    StopProcess(worker, "Playnite 正在退出");
                    return;
                }
                worker.BeginOutputReadLine();
                worker.BeginErrorReadLine();

                var startupDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                var lastProbe = new HealthProbeResult(HealthProbe.ConnectionFailed, null, null, "尚未完成健康探测。");
                while (DateTime.UtcNow < startupDeadline)
                {
                    await Task.Delay(250).ConfigureAwait(false);
                    if (worker.HasExited)
                    {
                        var exitCode = worker.ExitCode;
                        // The Worker uses a named mutex and deliberately exits with code 0
                        // when another current instance already owns the pipe.  A concurrent
                        // Playnite startup used to misreport that normal hand-off as a failed
                        // cold start. Give the existing instance a short bounded chance to
                        // answer before surfacing a genuine launch failure.
                        var existingProbe = await WaitForHealthAsync(
                            TimeSpan.FromSeconds(5), expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
                        if (exitCode == 0 && existingProbe.IsHealthy)
                        {
                            AppendLog(logPath, "Worker 启动进程退出码 0，但已有健康实例，复用现有 Worker。");
                            Interlocked.CompareExchange(ref runningWorker, null, worker);
                            worker.Dispose();
                            return;
                        }

                        AppendLog(logPath, DescribeHealth("Worker 启动子进程退出", existingProbe, expectedVersion, expectedBuildIdentity));
                        throw new InvalidOperationException(
                            $"Worker 启动进程已退出，退出码 {exitCode}。{DescribeHealth("Worker", existingProbe, expectedVersion, expectedBuildIdentity)}日志：{logPath}");
                    }
                    // A failed pipe connect is expected during cold start. Keep each probe
                    // short and enforce one real wall-clock deadline; the previous fixed
                    // 120-iteration loop multiplied a 2-second probe timeout into several
                    // minutes when the Worker never created its pipe.
                    lastProbe = await ProbeHealthAsync(TimeSpan.FromMilliseconds(650), expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
                    if (lastProbe.IsHealthy) return;
                }
                AppendLog(logPath, DescribeHealth("Worker 启动超时", lastProbe, expectedVersion, expectedBuildIdentity));
                throw new TimeoutException(
                    $"Worker 已启动，但 30 秒内未就绪。{DescribeHealth("Worker", lastProbe, expectedVersion, expectedBuildIdentity)}请查看日志：{logPath}");
            }
            finally
            {
                startupGate.Release();
            }
        }

        /// <summary>
        /// Stops only the Worker process created by this plugin instance. Playnite does not
        /// provide a console/window for the self-contained Worker, so normal host shutdown
        /// must release the child explicitly or the next development install can inherit an
        /// orphaned file lock.
        /// </summary>
        public void StopOwnedWorker()
        {
            shutdownRequested = true;
            var worker = Interlocked.Exchange(ref runningWorker, null);
            if (worker == null) return;
            StopProcess(worker, "Playnite 正在退出");
        }

        private void StopProcess(Process worker, string reason)
        {
            try
            {
                if (!worker.HasExited)
                {
                    AppendLog(workerLogPath, $"Stopping owned Worker: {reason}.");
                    worker.Kill();
                    worker.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                AppendLog(workerLogPath, $"Unable to stop owned Worker: {ex.Message}");
            }
            finally
            {
                worker.Dispose();
            }
        }

        public async Task<bool> IsHealthyAsync(TimeSpan? timeout = null, string? expectedVersion = null, string? expectedBuildIdentity = null)
        {
            return (await ProbeHealthAsync(timeout ?? TimeSpan.FromSeconds(2), expectedVersion, expectedBuildIdentity).ConfigureAwait(false)).IsHealthy;
        }

        private async Task<HealthProbeResult> WaitForHealthAsync(TimeSpan gracePeriod, string? expectedVersion = null, string? expectedBuildIdentity = null)
        {
            var deadline = DateTime.UtcNow + gracePeriod;
            var lastProbe = new HealthProbeResult(HealthProbe.ConnectionFailed, null, null, "尚未完成健康探测。");
            do
            {
                lastProbe = await ProbeHealthAsync(TimeSpan.FromSeconds(2), expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
                if (lastProbe.IsHealthy || IsIncompatible(lastProbe.Status)) return lastProbe;
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) break;
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(500, remaining.TotalMilliseconds))).ConfigureAwait(false);
            }
            while (DateTime.UtcNow < deadline);

            return lastProbe;
        }

        private enum HealthProbe
        {
            Healthy,
            ConnectionFailed,
            TimedOut,
            PipeDisconnected,
            ProtocolIncompatible,
            VersionIncompatible,
            BuildIdentityIncompatible,
            UnknownBuildIdentity,
            ServerRejected
        }

        private sealed class HealthProbeResult
        {
            public HealthProbeResult(HealthProbe status, string? actualVersion, string? actualBuildIdentity, string detail)
            {
                Status = status;
                ActualVersion = actualVersion;
                ActualBuildIdentity = actualBuildIdentity;
                Detail = detail ?? string.Empty;
            }

            public HealthProbe Status { get; }
            public string? ActualVersion { get; }
            public string? ActualBuildIdentity { get; }
            public string Detail { get; }
            public bool IsHealthy => Status == HealthProbe.Healthy;
        }

        private async Task<HealthProbeResult> ProbeHealthAsync(TimeSpan timeout, string? expectedVersion, string? expectedBuildIdentity)
        {
            try
            {
                var handshake = await client.HandshakeAsync(timeout).ConfigureAwait(false);
                return EvaluateIdentity(handshake.WorkerVersion, handshake.BuildIdentity, expectedVersion, expectedBuildIdentity);
            }
            catch (WorkerRequestException ex) when (string.Equals(ex.Code, "PROTOCOL_MISMATCH", StringComparison.Ordinal))
            {
                return new HealthProbeResult(HealthProbe.ProtocolIncompatible, null, null, ex.Message);
            }
            catch (WorkerRequestException ex) when (ex.FailureKind == WorkerIpcFailureKind.ServerRejected)
            {
                // Older Worker builds without the explicit handshake still answer Ping.
                return await ProbeLegacyPingAsync(timeout, expectedVersion, expectedBuildIdentity).ConfigureAwait(false);
            }
            catch (WorkerRequestException ex)
            {
                return ClassifyTransportFailure(ex);
            }
            catch (TimeoutException ex)
            {
                return new HealthProbeResult(HealthProbe.TimedOut, null, null, ex.Message);
            }
            catch (Exception ex)
            {
                return new HealthProbeResult(HealthProbe.ConnectionFailed, null, null, ex.Message);
            }
        }

        private async Task<HealthProbeResult> ProbeLegacyPingAsync(TimeSpan timeout, string? expectedVersion, string? expectedBuildIdentity)
        {
            try
            {
                var ping = await client.RequestAsync<WorkerPingDto>(MessageTypes.Ping, new { }, timeout).ConfigureAwait(false);
                return EvaluateIdentity(ping.Version, ping.BuildIdentity, expectedVersion, expectedBuildIdentity);
            }
            catch (WorkerRequestException ex)
            {
                return ClassifyTransportFailure(ex);
            }
            catch (TimeoutException ex)
            {
                return new HealthProbeResult(HealthProbe.TimedOut, null, null, ex.Message);
            }
            catch (Exception ex)
            {
                return new HealthProbeResult(HealthProbe.ConnectionFailed, null, null, ex.Message);
            }
        }

        private static HealthProbeResult EvaluateIdentity(
            string? actualVersion,
            string? actualBuildIdentity,
            string? expectedVersion,
            string? expectedBuildIdentity)
        {
            if (!string.IsNullOrWhiteSpace(expectedVersion) &&
                !string.Equals(actualVersion, expectedVersion, StringComparison.OrdinalIgnoreCase))
                return new HealthProbeResult(HealthProbe.VersionIncompatible, actualVersion, actualBuildIdentity, "Worker 版本不一致。");

            if (!string.IsNullOrWhiteSpace(expectedBuildIdentity) &&
                (BuildIdentity.IsUnknown(expectedBuildIdentity) || BuildIdentity.IsUnknown(actualBuildIdentity)))
                return new HealthProbeResult(HealthProbe.UnknownBuildIdentity, actualVersion, actualBuildIdentity, "Worker 构建身份为空或 unknown，无法证明与插件同源。");

            if (!IsBuildIdentityCompatible(actualBuildIdentity, expectedBuildIdentity))
                return new HealthProbeResult(HealthProbe.BuildIdentityIncompatible, actualVersion, actualBuildIdentity, "Worker 构建身份不一致。");

            return new HealthProbeResult(HealthProbe.Healthy, actualVersion, actualBuildIdentity, "握手成功。");
        }

        private static HealthProbeResult ClassifyTransportFailure(WorkerRequestException exception)
        {
            var status = exception.FailureKind switch
            {
                WorkerIpcFailureKind.Timeout => HealthProbe.TimedOut,
                WorkerIpcFailureKind.PipeDisconnected => HealthProbe.PipeDisconnected,
                WorkerIpcFailureKind.ConnectionFailed => HealthProbe.ConnectionFailed,
                _ => HealthProbe.ServerRejected
            };
            return new HealthProbeResult(status, null, null, exception.Message);
        }

        private static bool IsTransient(HealthProbe status)
            => status == HealthProbe.ConnectionFailed
               || status == HealthProbe.TimedOut
               || status == HealthProbe.PipeDisconnected
               || status == HealthProbe.ServerRejected;

        private static bool IsIncompatible(HealthProbe status)
            => status == HealthProbe.ProtocolIncompatible
               || status == HealthProbe.VersionIncompatible
               || status == HealthProbe.BuildIdentityIncompatible
               || status == HealthProbe.UnknownBuildIdentity;

        private static string DescribeHealth(
            string prefix,
            HealthProbeResult probe,
            string? expectedVersion,
            string? expectedBuildIdentity)
        {
            var status = probe.Status switch
            {
                HealthProbe.Healthy => "健康",
                HealthProbe.ConnectionFailed => "连接失败",
                HealthProbe.TimedOut => "超时",
                HealthProbe.PipeDisconnected => "管道断开",
                HealthProbe.ProtocolIncompatible => "协议不兼容",
                HealthProbe.VersionIncompatible => "版本不兼容",
                HealthProbe.BuildIdentityIncompatible => "构建身份不兼容",
                HealthProbe.UnknownBuildIdentity => "构建身份未知",
                _ => "Worker 拒绝请求"
            };
            return $"{prefix}健康探测：{status}；详情={probe.Detail}；实际版本={Display(probe.ActualVersion)}，期望版本={Display(expectedVersion)}；实际构建身份={Display(probe.ActualBuildIdentity)}，期望构建身份={Display(expectedBuildIdentity)}。";
        }

        private static string Display(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrWhiteSpace(text) ? "<空>" : text!;
        }

        internal static bool IsBuildIdentityCompatible(string? actual, string? expected)
            => string.IsNullOrWhiteSpace(expected)
               || (!string.IsNullOrWhiteSpace(actual) &&
                   string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase));

        private static void AppendLog(string? path, string? message)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(message)) return;
            lock (logGate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
        }
    }
}
