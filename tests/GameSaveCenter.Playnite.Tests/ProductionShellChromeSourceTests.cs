using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class ProductionShellChromeSourceTests
{
    [Fact]
    public void ProductionFooterOwnsWorkerStatusAndSpansTheShell()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");

        Assert.Contains("x:Name=\"FooterSurface\" Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\"", shell);
        Assert.Contains("Grid.Column=\"1\" x:Name=\"FooterStatusPanel\"", shell);
        Assert.Contains("{Binding Snapshot.WorkerHealthy}", shell);
        Assert.Contains("{Binding Snapshot.LudusaviAvailable}", shell);
        Assert.DoesNotContain("Text=\"生产版 · 真实数据由 Worker 提供\"", shell);
        Assert.DoesNotContain("Grid.Column=\"2\" Text=\"GameSaveCenter\"", shell);
        Assert.DoesNotContain("GscRedesignStatusCard", shell);
    }

    [Fact]
    public void GameBackgroundCoversTheWholeShellWithoutAColumnGutter()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");

        Assert.Contains("x:Name=\"DemoShell\" Margin=\"0\"", shell);
        Assert.Contains("Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\"", shell);
        Assert.Contains("x:Name=\"ShellAmbientMaterialLayer\"", shell);
        Assert.Contains("Grid.Row=\"0\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\"", shell);
        Assert.Contains("Grid.RowSpan=\"2\"", shell);
        Assert.Contains("UseSelectedGameBackground=\"False\"", shell);
        Assert.Contains("IsShellLayer=\"True\"", shell);
        Assert.Contains("x:Name=\"SidebarSurface\"", shell);
        Assert.Contains("Margin=\"0\"", shell);
        Assert.DoesNotContain("Margin=\"0,0,6,0\"", shell);
        Assert.Contains("BorderBrush=\"Transparent\"", shell);
        Assert.Contains("x:Name=\"FooterSurface\" Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\"", shell);
    }

    [Fact]
    public void ProductionHeaderUsesRoundedSharedChrome()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");
        var redesign = ReadSource("src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml");

        Assert.Contains("x:Name=\"HeaderSurface\" Grid.Row=\"0\"", shell);
        Assert.Contains("Style=\"{StaticResource GscRedesignHeaderSurface}\"", shell);
        Assert.Contains("x:Key=\"GscRedesignHeaderSurface\"", redesign);
        Assert.Contains("x:Key=\"GscRedesignHeaderCorner\">18", redesign);
        Assert.Contains("CornerRadius\" Value=\"{StaticResource GscRedesignHeaderCorner}\"", redesign);
        Assert.Contains("BorderThickness\" Value=\"1\"", redesign);
        Assert.Contains("ClipToBounds\" Value=\"True\"", redesign);
    }

    [Fact]
    public void ProductionSidebarCollapseIsAnIntegratedBoundaryAffordance()
    {
        var shell = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");
        var shellCode = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs");
        var settingsCode = ReadSource("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs");
        var resources = ReadSource("src", "GameSaveCenter.Playnite", "Themes", "AcrylicProductionResources.xaml");

        Assert.Contains("x:Name=\"SidebarColumn\" Width=\"270\"", shell);
        Assert.Contains("CacheMode=\"BitmapCache\"", shell);
        Assert.Contains("x:Name=\"SidebarContentLayer\"", shell);
        Assert.Contains("AcrylicSidebarBoundaryButton", shell);
        Assert.Contains("x:Name=\"SidebarCollapseButton\"", shell);
        Assert.Contains("x:Name=\"SidebarCollapseArea\" Grid.Row=\"1\"", shell);
        Assert.Contains("Width=\"32\"", shell);
        Assert.Contains("Height=\"32\"", shell);
        Assert.Contains("Text=\"‹\"", shell);
        Assert.DoesNotContain("x:Name=\"SidebarCollapseButtonContent\"", shell);
        Assert.DoesNotContain("Text=\"收起侧栏\"", shell);
        Assert.Contains("Property=\"Width\" Value=\"32\"", resources);
        Assert.Contains("Property=\"Height\" Value=\"32\"", resources);
        Assert.Contains("CornerRadius=\"16\"", resources);
        Assert.Contains("Background\" Value=\"Transparent\"", resources);
        Assert.Contains("Click=\"OnSidebarCollapseClick\"", shell);
        Assert.Contains("AutomationProperties.Name=\"收起导航栏\"", shell);
        Assert.Contains("x:Name=\"NavOverviewContent\"", shell);
        Assert.Contains("IconData=\"{StaticResource GscIconNavHome}\"", shell);
        Assert.Contains("x:Name=\"SidebarProductionVersionText\"", shell);
        Assert.Contains("sidebarCollapsed = !sidebarCollapsed", shellCode);
        Assert.Contains("sidebarTransitionRunning", shellCode);
        Assert.Contains("GridLengthAnimation", shellCode);
        Assert.Contains("Duration = new Duration(GscMotion.Normal)", shellCode);
        Assert.Contains("GscMotion.CreateEaseOut()", shellCode);
        Assert.Contains("MotionEnabledProvider", shellCode);
        Assert.Contains("new GridLength(sidebarCollapsed ? 72 : 270, GridUnitType.Pixel)", shellCode);
        Assert.Contains("ApplySidebarLayout(updateColumnWidth: false)", shellCode);
        Assert.Contains("SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty", shellCode);
        Assert.Contains("new DoubleAnimation(0, 1, GscMotion.Normal)", shellCode);
        Assert.Contains("translate.X = sidebarCollapsed ? -4 : 4", shellCode);
        Assert.Contains("SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null)", shellCode);
        Assert.Contains("SidebarCollapsedProvider", shellCode);
        Assert.Contains("SidebarCollapsedChanged", shellCode);
        Assert.Contains("SidebarHeaderLayout.Margin", shellCode);
        Assert.Contains("content.Width = expanded ? double.NaN : 26", shellCode);
        Assert.Contains("public bool SidebarCollapsed", settingsCode);
        Assert.Contains("SidebarCollapsed = other.SidebarCollapsed", settingsCode);
        Assert.Contains("public bool FollowSelectedGameBackground", settingsCode);
        Assert.Contains("FollowSelectedGameBackground = other.FollowSelectedGameBackground", settingsCode);
        Assert.Contains("HorizontalAlignment.Center", shellCode);
        Assert.Contains("typeof(AcrylicProductionShellView).Assembly.GetName().Version", shellCode);
        Assert.Contains("展开导航栏", shellCode);
        Assert.Contains("ApplyResponsiveLayout(ActualWidth, ActualHeight);", shellCode);
        Assert.Contains("NormalizeMotionIfDisabled();", shellCode);
    }

    [Fact]
    public void SidebarTransitionUsesTheCurrentWidthAndKeepsTheLatestTarget()
    {
        var shellCode = ReadSource("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs");

        Assert.DoesNotContain("if (sidebarTransitionRunning)", shellCode);
        Assert.Contains("Capture the currently rendered width before cancelling the old", shellCode);
        Assert.Contains("SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);", shellCode);
        Assert.Contains("var transitionGeneration = ++sidebarTransitionGeneration;", shellCode);
        Assert.Contains("if (transitionGeneration != sidebarTransitionGeneration)", shellCode);
        Assert.Contains("SidebarColumn.Width = new GridLength(currentWidth, GridUnitType.Pixel);", shellCode);
        Assert.Contains("internal bool SidebarTransitionRunningForAudit", shellCode);
        Assert.Contains("SidebarContentLayer.BeginAnimation(UIElement.OpacityProperty, null);", shellCode);
        Assert.Contains("sidebarTransitionRunning = false;", shellCode);
    }

    [Fact]
    public void ReducedMotionNormalizesSidebarToItsFinalStateInAnActualWpfWindow()
    {
        Exception? exception = null;
        var transitionRunning = true;
        var sidebarWidth = 0d;
        var sidebarCollapsed = false;
        var motionEnabled = true;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new GameSaveCenter.Playnite.Views.AcrylicProductionShellView
                {
                    MotionEnabledProvider = () => false,
                    SidebarCollapsedProvider = () => false
                };
                window = new Window
                {
                    Content = shell,
                    Width = 900,
                    Height = 640,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();
                shell.UpdateLayout();
                shell.SidebarCollapseButtonForAudit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                shell.UpdateLayout();

                motionEnabled = shell.SidebarMotionEnabledForAudit;
                transitionRunning = shell.SidebarTransitionRunningForAudit;
                sidebarWidth = shell.SidebarWidthForAudit;
                sidebarCollapsed = shell.SidebarCollapsedForAudit;
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
        Assert.False(motionEnabled);
        Assert.False(transitionRunning);
        Assert.True(sidebarCollapsed);
        Assert.Equal(72d, sidebarWidth);
    }

    [Fact]
    public void SettingsViewRequestsAUsableDefaultWindowSize()
    {
        var settings = ReadSource("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml");
        var settingsCode = ReadSource("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs");

        Assert.DoesNotContain("MinWidth=\"1180\" MinHeight=\"760\"", settings);
        Assert.Contains("EnsureHostWindowSize();", settingsCode);
        Assert.Contains("preferredWidth = 1280", settingsCode);
        Assert.Contains("preferredHeight = 840", settingsCode);
        Assert.Contains("hostWindow.SizeToContent = SizeToContent.Manual", settingsCode);
    }

    [Fact]
    public void GameBackgroundPreferenceControlsDecodeAndMaterialFallback()
    {
        var settings = ReadSource("src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml");
        var dashboard = ReadSource("src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs");
        var viewModel = ReadSource("src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs");
        var ambient = ReadSource("src", "GameSaveCenter.Playnite", "Controls", "AmbientMaterialLayer.xaml.cs");

        Assert.Contains("IsChecked=\"{Binding FollowSelectedGameBackground}\"", settings);
        Assert.Contains("不再解码封面", settings);
        Assert.Contains("ApplySelectedGameBackgroundPreference();", dashboard);
        Assert.Contains("plugin.Settings.FollowSelectedGameBackground", dashboard);
        Assert.Contains("CancelSelectedGameBackgroundLoad();", viewModel);
        Assert.Contains("var useGameMaterial = UseSelectedGameBackground && hasGameMaterial;", ambient);
        Assert.Contains("ThemeAmbientWash.Opacity = useGameMaterial ? 0 : 1;", ambient);
    }

    private static string ReadSource(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        if (directory == null)
            throw new InvalidOperationException("无法定位仓库根目录");
        return File.ReadAllText(Path.Combine(new[] { directory.FullName }.Concat(segments).ToArray()));
    }
}
