using System;
using System.IO;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    internal sealed class SettingsPathEditorProbe
    {
        internal SettingsPathEditorProbe(
            bool isValid,
            string message,
            string expandedPath,
            bool exists)
        {
            IsValid = isValid;
            Message = message ?? string.Empty;
            ExpandedPath = expandedPath ?? string.Empty;
            Exists = exists;
        }

        internal bool IsValid { get; }
        internal string Message { get; }
        internal string ExpandedPath { get; }
        internal bool Exists { get; }
    }

    /// <summary>
    /// Performs one-field, read-only path checks for the settings path editor.
    /// It deliberately treats a missing directory as invalid for Open, while the
    /// existing full settings validator may still allow a missing leaf to be created
    /// by Worker when the settings are saved.
    /// </summary>
    internal static class SettingsPathEditorService
    {
        internal static SettingsPathEditorProbe Probe(
            SettingsPathEditorOption option,
            string value)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            var rawValue = value?.Trim() ?? string.Empty;
            if (rawValue.Length == 0)
                return Invalid("当前字段尚未填写。", string.Empty, false);

            var expandedPath = Environment.ExpandEnvironmentVariables(rawValue);
            try
            {
                var fullPath = Path.GetFullPath(expandedPath);
                if (option.Kind == SettingsPathEditorKind.Executable)
                    return ProbeExecutable(fullPath);
                return ProbeDirectory(fullPath);
            }
            catch (Exception exception) when (IsPathAccessFailure(exception))
            {
                return Invalid($"当前路径无效或不可访问：{exception.Message}", expandedPath, false);
            }
        }

        private static SettingsPathEditorProbe ProbeExecutable(string path)
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(path);
            }
            catch (FileNotFoundException)
            {
                return Missing(path, "可执行文件不存在。");
            }
            catch (DirectoryNotFoundException)
            {
                return Missing(path, "可执行文件所在磁盘或网络共享不可访问。");
            }
            catch (UnauthorizedAccessException)
            {
                return Invalid("可执行文件存在，但当前用户无权访问。", path, true);
            }
            catch (IOException exception)
            {
                return Invalid($"可执行文件存在，但当前不可访问：{exception.Message}", path, true);
            }

            if ((attributes & FileAttributes.Directory) != 0)
                return Invalid("当前路径指向目录，不能作为可执行文件。", path, true);
            return Valid(path, "当前可执行文件有效，可打开所在目录或复制完整路径。", true);
        }

        private static SettingsPathEditorProbe ProbeDirectory(string path)
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(path);
            }
            catch (FileNotFoundException)
            {
                return Missing(path, "目录不存在；保存时的全量校验仍会按现有规则判断是否可创建。");
            }
            catch (DirectoryNotFoundException)
            {
                return Missing(path, "目录所在磁盘或网络共享不可访问。");
            }
            catch (UnauthorizedAccessException)
            {
                return Invalid("目录存在，但当前用户无权访问。", path, true);
            }
            catch (IOException exception)
            {
                return Invalid($"目录存在，但当前不可访问：{exception.Message}", path, true);
            }

            if ((attributes & FileAttributes.Directory) == 0)
                return Invalid("当前路径指向文件，不能作为目录。", path, true);

            try
            {
                // A read-only enumeration makes an existing but inaccessible share
                // observable without creating, deleting, or modifying user data.
                Directory.GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
                return Valid(path, "当前目录有效，可打开目录或复制完整路径。", true);
            }
            catch (UnauthorizedAccessException)
            {
                return Invalid("目录存在，但当前用户无权访问。", path, true);
            }
            catch (IOException exception)
            {
                return Invalid($"目录存在，但当前不可访问：{exception.Message}", path, true);
            }
        }

        private static SettingsPathEditorProbe Valid(string path, string message, bool exists)
            => new(true, message, path, exists);

        private static SettingsPathEditorProbe Missing(string path, string message)
            => new(false, message, path, false);

        private static SettingsPathEditorProbe Invalid(string message, string path, bool exists)
            => new(false, message, path, exists);

        private static bool IsPathAccessFailure(Exception exception)
            => exception is ArgumentException
                || exception is NotSupportedException
                || exception is PathTooLongException
                || exception is IOException
                || exception is UnauthorizedAccessException;
    }
}
