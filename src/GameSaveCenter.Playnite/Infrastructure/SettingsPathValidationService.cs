using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Performs the existing settings path checks against an immutable snapshot.
    /// </summary>
    internal static class SettingsPathValidationService
    {
        internal static Task<IReadOnlyList<string>> ValidateAsync(
            SettingsPathValidationSnapshot snapshot,
            CancellationToken cancellationToken)
            => Task.Run<IReadOnlyList<string>>(
                () => Validate(snapshot, cancellationToken),
                cancellationToken);

        internal static IReadOnlyList<string> Validate(
            SettingsPathValidationSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            cancellationToken.ThrowIfCancellationRequested();

            var errors = new List<string>();
            var workerPath = Expand(snapshot.WorkerExecutable);
            if (string.IsNullOrWhiteSpace(snapshot.WorkerExecutable) || !File.Exists(workerPath))
                errors.Add("未找到 GameSaveCenter Worker。请先运行打包脚本，或选择正确的 Worker 可执行文件。");
            else if (!GameSaveCenterSettings.IsWorkerExecutable(snapshot.WorkerExecutable))
                errors.Add("Worker 路径必须指向 GameSaveCenter.Worker.exe，不能选择 Ludusavi 或其他程序。");

            cancellationToken.ThrowIfCancellationRequested();
            AddDirectoryPathError(errors, "存档目录", snapshot.LudusaviBackupDirectory, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            AddDirectoryPathError(errors, "媒体目录", snapshot.MediaArchiveDirectory, cancellationToken);
            if (snapshot.EnableLocalMirror && string.IsNullOrWhiteSpace(snapshot.LocalMirrorPath))
                errors.Add("启用本地镜像时必须填写镜像目录。");
            else if (snapshot.EnableLocalMirror)
                AddDirectoryPathError(errors, "本地镜像", snapshot.LocalMirrorPath, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(snapshot.LudusaviExecutable) && !File.Exists(Expand(snapshot.LudusaviExecutable)))
                errors.Add("Ludusavi 路径不存在。");
            if (!string.IsNullOrWhiteSpace(snapshot.RcloneExecutable) && !File.Exists(Expand(snapshot.RcloneExecutable)))
                errors.Add("Rclone 路径不存在。");
            return errors;
        }

        private static void AddDirectoryPathError(
            List<string> errors,
            string label,
            string path,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(path))
            {
                errors.Add($"{label}路径不能为空。");
                return;
            }

            try
            {
                var fullPath = Path.GetFullPath(Expand(path));
                if (File.Exists(fullPath))
                {
                    errors.Add($"{label}路径指向文件，不能作为目录：{path}");
                    return;
                }

                if (Directory.Exists(fullPath)) return;

                // Missing leaf directories are valid: Worker creates them on demand. A
                // missing/unreachable volume or share is not, and must not be presented as
                // a healthy configured path.
                var root = Path.GetPathRoot(fullPath);
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                    errors.Add($"{label}所在磁盘或网络共享不可访问：{path}");
                else
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var attributes = File.GetAttributes(fullPath);
                        if ((attributes & FileAttributes.Directory) == 0)
                            errors.Add($"{label}路径不是目录：{path}");
                    }
                    catch (FileNotFoundException) { }
                    catch (DirectoryNotFoundException) { }
                    catch (UnauthorizedAccessException)
                    {
                        errors.Add($"{label}目录不可访问：{path}");
                    }
                    catch (IOException ex)
                    {
                        errors.Add($"{label}目录不可访问：{path}（{ex.Message}）");
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException || ex is IOException || ex is UnauthorizedAccessException)
            {
                errors.Add($"{label}路径无效或不可访问：{path}（{ex.Message}）");
            }
        }

        private static string Expand(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : Environment.ExpandEnvironmentVariables(value);
    }
}
