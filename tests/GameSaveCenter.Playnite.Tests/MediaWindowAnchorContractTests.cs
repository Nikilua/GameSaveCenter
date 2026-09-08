using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MediaWindowAnchorContractTests
{
    [Fact]
    public void LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var codeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");

        Assert.Contains("Click=\"OnLoadMoreMediaClick\"", view);
        Assert.Contains("Click=\"OnLoadMoreMediaInboxClick\"", view);
        Assert.Contains("CaptureAnchor(MediaGrid)", codeBehind);
        Assert.Contains("CaptureAnchor(MediaInboxGrid)", codeBehind);
        Assert.Contains("ScrollToVerticalOffset", codeBehind);
        Assert.Contains("ScrollIntoView", codeBehind);
        Assert.Contains("anchorRestoreGeneration", codeBehind);
        Assert.Contains("PropertyChanged += OnViewModelPropertyChanged", codeBehind);
        Assert.Contains("generation == anchorRestoreGeneration", codeBehind);
        Assert.Contains("selectionRestoreQueued = false;", codeBehind);
    }

    [Fact]
    public void EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics()
    {
        var view = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var codeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");
        var viewModel = Read("src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs");

        Assert.Contains("ReloadMediaWindowCommand", view);
        Assert.Contains("ReloadMediaInboxCommand", view);
        Assert.Contains("仅当前保留项参与操作", codeBehind);
        Assert.Contains("ReloadMediaWindowAsync", viewModel);
        Assert.Contains("ReloadMediaInboxWindowAsync", viewModel);
    }

    [Fact]
    public void PurposeNavigationUsesDedicatedMediaAndSaveTabState()
    {
        var viewModel = Read("src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs");
        var media = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var saves = Read("src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml");

        Assert.Contains("MediaTabIndex = 0", viewModel);
        Assert.Contains("SaveTabIndex = 1", viewModel);
        Assert.Contains("SelectedIndex=\"{Binding MediaTabIndex, Mode=TwoWay}\"", media);
        Assert.Contains("SelectedIndex=\"{Binding SaveTabIndex, Mode=TwoWay}\"", saves);
        Assert.DoesNotContain("SelectedIndex=\"1\"", media);
    }

    [Fact]
    public void GridScrollTemplateReservesTheRealContentViewport()
    {
        var redesign = Read("src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml");
        var task = Read("src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml");
        var mediaCodeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");

        Assert.Contains("GscRedesignDataGridTemplate", redesign);
        Assert.Contains("PART_ColumnHeadersPresenter", redesign);
        Assert.Contains("PART_ScrollContentPresenter", redesign);
        Assert.Contains("PART_VerticalScrollBar", redesign);
        Assert.Contains("PART_HorizontalScrollBar", redesign);
        Assert.Contains("<RowDefinition Height=\"*\"/>", redesign);
        Assert.Contains("Grid.Row=\"2\"", redesign);
        Assert.DoesNotContain("Padding\" Value=\"0,0,0,12\"", task);
        Assert.Contains("GetContentViewport", mediaCodeBehind);
        Assert.Contains("ScrollUnit.Item", mediaCodeBehind);
        Assert.Contains("VirtualizingStackPanel", mediaCodeBehind);
    }

    [Fact]
    public void CurrentMediaCardsUseTheBoundedVirtualizingPanel()
    {
        var media = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");

        Assert.Contains("<ui:VirtualizingWrapPanel", media);
        Assert.Contains("ItemWidth=\"164\"", media);
        Assert.Contains("ItemHeight=\"154\"", media);
        Assert.Contains("HorizontalSpacing=\"0\"", media);
        Assert.Contains("VerticalSpacing=\"0\"", media);
    }

    [Fact]
    public void StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation()
    {
        Exception? exception = null;
        var viewWasLoaded = false;
        var noticeVisibility = Visibility.Visible;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var view = new GameSaveCenter.Playnite.Views.MediaCenterView();
                var viewType = typeof(GameSaveCenter.Playnite.Views.MediaCenterView);
                var list = (ListBox)viewType.GetField("MediaGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var anchorType = viewType.GetNestedType("ScrollAnchor", BindingFlags.NonPublic)!;
                var anchor = Activator.CreateInstance(
                    anchorType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    args: new object[] { "removed-after-window-trim", 0, 0d, 0d },
                    culture: null)!;
                var items = new ObservableCollection<MediaItemDto>
                {
                    Media("anchor"),
                    Media("new-selection")
                };
                list.ItemsSource = items;
                window = new Window
                {
                    Content = view,
                    Width = 900,
                    Height = 640,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();
                view.UpdateLayout();
                viewWasLoaded = view.IsLoaded;

                var generation = (long)viewType.GetField("anchorRestoreGeneration", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                viewType.GetMethod("QueueRestore", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(
                    view,
                    new object?[] { list, anchor, new HashSet<string>(StringComparer.OrdinalIgnoreCase), false, null, generation });

                viewType.GetMethod("InvalidatePendingAnchorRestore", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(view, null);
                view.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                noticeVisibility = ((TextBlock)viewType.GetField("MediaWindowAnchorNotice", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!).Visibility;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.True(viewWasLoaded);
        Assert.Equal(Visibility.Collapsed, noticeVisibility);
    }

    [Fact]
    public void EvictedAnchorNoticeReleasesSelectionRestoreGuard()
    {
        Exception? exception = null;
        var noticeVisibility = Visibility.Collapsed;
        var selectionGuard = true;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var view = new GameSaveCenter.Playnite.Views.MediaCenterView();
                var viewType = typeof(GameSaveCenter.Playnite.Views.MediaCenterView);
                var list = (ListBox)viewType.GetField("MediaGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var anchorType = viewType.GetNestedType("ScrollAnchor", BindingFlags.NonPublic)!;
                var anchor = Activator.CreateInstance(
                    anchorType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    args: new object[] { "removed-after-window-trim", 0, 0d, 0d },
                    culture: null)!;
                list.ItemsSource = new ObservableCollection<MediaItemDto> { Media("kept") };
                window = new Window
                {
                    Content = view,
                    Width = 900,
                    Height = 640,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();
                view.UpdateLayout();

                var generation = (long)viewType.GetField("anchorRestoreGeneration", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                viewType.GetField("selectionRestoreQueued", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);
                viewType.GetMethod("RestoreAnchor", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(
                    view,
                    new object?[]
                    {
                        list,
                        anchor,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                        false,
                        null,
                        generation,
                        0
                    });
                noticeVisibility = ((TextBlock)viewType.GetField("MediaWindowAnchorNotice", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!).Visibility;
                selectionGuard = (bool)viewType.GetField("selectionRestoreQueued", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(Visibility.Visible, noticeVisibility);
        Assert.False(selectionGuard);
    }

    private static MediaItemDto Media(string id)
        => new MediaItemDto
        {
            MediaId = id,
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.WindowsScreenshot,
            ArchivePath = "C:\\archive\\" + id + ".png",
            OriginalPath = "C:\\source\\" + id + ".png",
            Sha256 = id,
            CapturedUtc = DateTime.UtcNow,
            ClassificationState = "Assigned"
        };

    private static string Read(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null) throw new DirectoryNotFoundException("无法定位仓库根目录。");
        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray()));
    }
}
