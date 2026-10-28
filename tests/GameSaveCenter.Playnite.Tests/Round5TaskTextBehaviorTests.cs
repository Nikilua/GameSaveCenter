using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Round5TaskTextBehaviorTests
{
    [Fact]
    public void TrimmedTaskTextKeepsItsFullValueAvailableToAutomation()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var view = new TaskCenterView();
                var fullText = new string('长', 200);
                var text = new TextBlock { Style = (Style)view.FindResource("TaskLongTextCell"), Text = fullText, Width = 72 };
                text.Measure(new Size(72, 40));
                text.Arrange(new Rect(0, 0, 72, 40));
                BindingOperations.GetBindingExpression(text, AutomationProperties.HelpTextProperty)?.UpdateTarget();
                Assert.Equal(TextTrimming.CharacterEllipsis, text.TextTrimming);
                Assert.Equal(TextWrapping.NoWrap, text.TextWrapping);
                Assert.Equal(fullText, AutomationProperties.GetHelpText(text));
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
