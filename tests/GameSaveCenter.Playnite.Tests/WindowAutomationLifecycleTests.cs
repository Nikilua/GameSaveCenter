using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WindowAutomationLifecycleTests
{
    [Fact]
    public void StandardWindowPeerKeepsItsNameAfterNativeWindowCloses()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                for (var index = 0; index < 9; index++)
                {
                    var window = new Window
                    {
                        Content = new Border(), Width = 120, Height = 80,
                        ShowActivated = false, ShowInTaskbar = false,
                        Title = index % 3 == 1 ? "Named test title" : string.Empty
                    };
                    var expected = index % 3 == 2 ? "Explicit automation name"
                        : index % 3 == 1 ? "Named test title" : "GameSaveCenter WPF test window";
                    if (index % 3 == 2)
                        AutomationProperties.SetName(window, expected);
                    try
                    {
                        window.Show();
                        window.UpdateLayout();
                        var peer = Assert.IsType<WindowAutomationPeer>(UIElementAutomationPeer.CreatePeerForElement(window));
                        Assert.Equal(expected, peer.GetName());
                        window.Close();
                        Assert.Equal(expected, peer.GetName());
                    }
                    finally
                    {
                        window.Close();
                        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                    }
                }
            }
            catch (Exception caught) { failure = caught; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }
}
