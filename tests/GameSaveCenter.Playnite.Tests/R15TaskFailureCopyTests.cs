using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskFailureCopyTests
{
    [Fact]
    public void FailureSummaryIsShortAndCopyPayloadIsRedactedButComplete()
    {
        var task = new TaskStatusDto
        {
            TaskId = "task-copy-1",
            TaskType = "Backup",
            GameName = "测试游戏",
            State = TaskState.Failed,
            ErrorCode = "BACKUP_FAILED",
            ErrorMessage = "首行摘要\r\nSystem.Exception: password=should-not-leak\r\n" + new string('x', 260)
        };

        Assert.Equal("首行摘要", task.FailureSummary);

        var payload = TaskFailureClipboardFormatter.Format(task);

        Assert.Contains("任务摘要：首行摘要", payload, StringComparison.Ordinal);
        Assert.Contains("失败原因：首行摘要\r\nSystem.Exception: password=[已隐藏]", payload, StringComparison.Ordinal);
        Assert.Contains("错误码：BACKUP_FAILED", payload, StringComparison.Ordinal);
        Assert.Contains("技术详情：错误码：BACKUP_FAILED；首行摘要\r\nSystem.Exception: password=[已隐藏]", payload, StringComparison.Ordinal);
        Assert.Contains("任务 ID：task-copy-1", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("should-not-leak", payload, StringComparison.Ordinal);
    }

    [Fact]
    public void LongSingleLineFailureSummaryIsBoundedWithoutChangingTechnicalDetails()
    {
        var longMessage = new string('x', 300);
        var task = new TaskStatusDto { State = TaskState.Failed, ErrorMessage = longMessage };

        Assert.Equal(240, task.FailureSummary.Length);
        Assert.EndsWith("…", task.FailureSummary, StringComparison.Ordinal);
        Assert.Equal(longMessage, task.DetailMessage);
        Assert.Equal(longMessage, task.SafeDetailMessage);
    }

    [Fact]
    public async Task ClipboardRetryPreservesPayloadAndRetriesTransientFailures()
    {
        var attempts = 0;
        var copied = string.Empty;
        var payload = "任务摘要：失败\r\n错误码：TEST_FAILURE";

        var succeeded = await ClipboardRetry.TrySetTextAsync(
            payload,
            text =>
            {
                attempts++;
                if (attempts < 3) throw new InvalidOperationException("clipboard busy");
                copied = text;
            },
            _ => Task.CompletedTask);

        Assert.True(succeeded);
        Assert.Equal(3, attempts);
        Assert.Equal(payload, copied);
    }

    [Fact]
    public async Task ClipboardRetryStopsAfterFourAttemptsAndKeepsFailureNegative()
    {
        var attempts = 0;
        var succeeded = await ClipboardRetry.TrySetTextAsync(
            "payload",
            _ =>
            {
                attempts++;
                throw new InvalidOperationException("clipboard busy");
            },
            _ => Task.CompletedTask);

        Assert.False(succeeded);
        Assert.Equal(4, attempts);
    }

    [Fact]
    public void TechnicalDetailsUseSelectableReadOnlyProductionControlAndSummaryBinding()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        Assert.Contains("SelectedTask.FailureSummary", source, StringComparison.Ordinal);
        Assert.Contains("SelectedTask.SafeDetailMessage", source, StringComparison.Ordinal);
        Assert.Contains("IsReadOnly=\"True\"", source, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"任务可选择技术详情\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock Text=\"{Binding SelectedTask.DetailMessage, Mode=OneWay}\"", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionReadOnlyDetailsKeepTheCompleteSanitizedTextSelectable()
    {
        Exception? failure = null;
        string selected = string.Empty;
        var text = "错误码：TEST\r\npassword=[已隐藏]\r\n技术堆栈：line 2";
        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var style = Assert.IsType<Style>(resources["GscWpfUiTextBox"]);
                var box = new TextBox
                {
                    Style = style,
                    Text = text,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    MaxHeight = 180
                };
                var host = new Border { Width = 420, Height = 120, Child = box };
                host.Measure(new Size(420, 120));
                host.Arrange(new Rect(0, 0, 420, 120));
                host.UpdateLayout();
                box.SelectAll();
                selected = box.SelectedText;
                Assert.True(box.IsReadOnly);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(text, selected);
    }

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
}
