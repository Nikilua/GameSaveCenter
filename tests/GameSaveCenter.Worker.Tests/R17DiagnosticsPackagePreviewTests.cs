using System;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class R17DiagnosticsPackagePreviewTests
{
    [Fact]
    public void PreviewIsReadOnlyAndStatesIncludedAndExcludedBoundaries()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-diagnostics-preview-" + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new WorkerOptions { DataDirectory = root };
            var service = new DiagnosticsPackageService(options, new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance), NullLogger<DiagnosticsPackageService>.Instance);

            var preview = service.Preview(new CreateDiagnosticsPackageRequestDto { AuditLimit = 999, TaskLimit = 0 });

            Assert.Equal(300, preview.AuditLimit);
            Assert.Equal(1, preview.TaskLimit);
            Assert.Contains(preview.IncludedItems, item => item.EntryName == "health.json" && item.Redaction.Contains("脱敏", StringComparison.Ordinal));
            Assert.Contains(preview.IncludedItems, item => item.Optional && item.EntryName.StartsWith("logs/", StringComparison.Ordinal));
            Assert.Contains("真实存档和备份归档文件", preview.ExcludedItems);
            Assert.Contains("SQLite 数据库文件", string.Join("\n", preview.ExcludedItems), StringComparison.Ordinal);
            Assert.Contains("不会上传", preview.Summary, StringComparison.Ordinal);
            Assert.Contains("明确不包含", preview.ConfirmationText, StringComparison.Ordinal);
            Assert.False(Directory.Exists(root));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
