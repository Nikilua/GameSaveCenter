using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class OverviewInteractionTests
{
    [Fact]
    public void OverviewActivityRowsKeepTheirVisualTreeAndCloudQueueCardExecutesOneClickCommand()
    {
        Exception? exception = null;
        var cloudQueueClicks = 0;

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewInteractionData(() => cloudQueueClicks++);
                data.Activities.Add(new ActivityEntryDto
                {
                    GameName = "全局",
                    Summary = "首页活动测试",
                    CreatedUtc = DateTime.UtcNow
                });

                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 820
                };
                var host = new Grid
                {
                    Width = 1280,
                    Height = 820
                };
                host.Children.Add(overview);

                host.Measure(new Size(1280, 820));
                host.Arrange(new Rect(0, 0, 1280, 820));
                overview.UpdateLayout();

                var activityButton = FindVisualDescendants<PlayniteButton>(overview).Find(button =>
                    AutomationProperties.GetName(button) == "打开活动对应工作区");
                Assert.NotNull(activityButton);
                Assert.IsType<Border>(activityButton!.Content);

                var cloudQueueCard = FindVisualDescendants<PlayniteButton>(overview).Find(button =>
                    AutomationProperties.GetName(button) == "打开云端队列");
                Assert.NotNull(cloudQueueCard);
                Assert.IsType<StackPanel>(cloudQueueCard!.Content);
                Assert.Null(cloudQueueCard.ContentTemplate);
                Assert.Same(data.OpenCloudQueueCommand, cloudQueueCard.Command);

                // Invoke the framework's protected click path so ButtonBase performs its
                // normal CanExecute/Execute handling rather than calling the command directly.
                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(cloudQueueCard, Array.Empty<object>());
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(1, cloudQueueClicks);
    }

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null)
        {
            return;
        }

        if (current is T match)
        {
            matches.Add(match);
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
        {
            Visit(VisualTreeHelper.GetChild(current, index), matches);
        }
    }

    private sealed class OverviewInteractionData
    {
        public OverviewInteractionData(Action onCloudQueueClick)
        {
            OpenCloudQueueCommand = new CountingCommand(onCloudQueueClick);
            OpenActivityCommand = new CountingCommand(() => { });
            RefreshCommand = new CountingCommand(() => { });
        }

        public DashboardSnapshotDto Snapshot { get; } = new();
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ICommand OpenCloudQueueCommand { get; }
        public ICommand OpenActivityCommand { get; }
        public ICommand RefreshCommand { get; }
    }

    private sealed class CountingCommand : ICommand
    {
        private readonly Action execute;

        public CountingCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
