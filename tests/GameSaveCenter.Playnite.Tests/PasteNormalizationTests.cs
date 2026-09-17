using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class PasteNormalizationTests
{
    [Fact]
    public void DirectoryPasteTrimsOuterWhitespaceQuotesAndTrailingShellNewline()
    {
        var result = PasteNormalization.Normalize(
            "  \"C:\\Game Saves\\Slot 1\"  \r\n",
            PasteNormalizationKind.Path);

        Assert.True(result.Accepted);
        Assert.True(result.Changed);
        Assert.Equal("C:\\Game Saves\\Slot 1", result.Value);
        Assert.Equal("  \"C:\\Game Saves\\Slot 1\"  \r\n", result.RawValue);
        Assert.Contains("外层空白和引号", result.Message);
        Assert.Contains("Ctrl+Z", result.Message);
    }

    [Fact]
    public void MultiLinePasteIsRejectedWithoutChangingTheValue()
    {
        const string raw = "C:\\Game Saves\\A\r\nC:\\Game Saves\\B";
        var result = PasteNormalization.Normalize(raw, PasteNormalizationKind.Path);

        Assert.False(result.Accepted);
        Assert.False(result.Changed);
        Assert.Equal(raw, result.RawValue);
        Assert.Equal(raw, result.Value);
        Assert.Contains("多行粘贴", result.Message);
        Assert.Contains("原字段未改变", result.Message);
        Assert.Contains("剪贴板内容未改写", result.Message);
    }

    [Theory]
    [InlineData("  '8080'  ", PasteNormalizationKind.Port, "8080")]
    [InlineData("  '*.png'  ", PasteNormalizationKind.ExcludePattern, "*.png")]
    [InlineData("  \"myremote:GameSaveCenter\"  ", PasteNormalizationKind.RemoteTarget, "myremote:GameSaveCenter")]
    public void EachInputKindUsesTheSameWrapperRulesWithoutCollapsingItsSyntax(
        string raw,
        PasteNormalizationKind kind,
        string expected)
    {
        var result = PasteNormalization.Normalize(raw, kind);

        Assert.True(result.Accepted);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void WpfPasteKeepsUndoForNormalizedTextAndLeavesRejectedTextUntouched()
    {
        RunSta(() =>
        {
            var application = new Application();
            var textBox = new TextBox
            {
                Text = "old-value",
                ToolTip = "原始提示"
            };
            PasteNormalization.SetKind(textBox, PasteNormalizationKind.Path);
            var window = new Window
            {
                Content = textBox,
                Width = 240,
                Height = 80,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                Assert.True(textBox.Focus());
                textBox.SelectAll();
                RaisePaste(textBox, "  \"new-value\"  ");

                Assert.Equal("new-value", textBox.Text);
                Assert.Equal("  \"new-value\"  ", PasteNormalization.GetLastRawText(textBox));
                Assert.Contains("标准化粘贴内容", PasteNormalization.GetLastMessage(textBox));
                Assert.Contains("标准化粘贴内容", textBox.ToolTip as string);
                Assert.True(textBox.CanUndo);

                textBox.Undo();
                Assert.Equal("old-value", textBox.Text);

                textBox.SelectAll();
                RaisePaste(textBox, "C:\\A\r\nC:\\B");

                Assert.Equal("old-value", textBox.Text);
                Assert.Contains("多行粘贴", PasteNormalization.GetLastMessage(textBox));
            }
            finally
            {
                if (window.IsVisible) window.Close();
                application.Shutdown();
            }
        });
    }

    private static DataObjectPastingEventArgs RaisePaste(TextBox textBox, string value)
    {
        var data = new DataObject();
        data.SetData(DataFormats.UnicodeText, value);
        var args = new DataObjectPastingEventArgs(data, false, DataFormats.UnicodeText)
        {
            RoutedEvent = DataObject.PastingEvent
        };
        textBox.RaiseEvent(args);
        return args;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
