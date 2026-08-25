using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Diagnostics;
using GameSaveCenter.Playnite.Infrastructure;
using Microsoft.Win32;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Settings
{
    public partial class GameSaveCenterSettingsView : UserControl
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private bool entrancePlayed;
        private bool settingsTransferInProgress;
        private bool responsiveLayoutPending;
        private bool adaptiveThemePending;
        private bool systemParametersSubscribed;
        private bool scrollSelectionPending;
        private Size pendingResponsiveSize;

        public GameSaveCenterSettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
            SizeChanged += OnSizeChanged;
            AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnSettingsFieldChanged));
            AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnSettingsFieldChanged));
            AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(OnSettingsFieldChanged));
        }

        private GameSaveCenterSettings? CurrentSettings => DataContext as GameSaveCenterSettings;

        private bool MotionEnabled => (CurrentSettings?.EnableUiAnimations ?? true) && !SystemParameters.HighContrast && SystemParameters.ClientAreaAnimation;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!systemParametersSubscribed)
            {
                SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
                systemParametersSubscribed = true;
            }
            ApplyAdaptiveTheme();
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
            RealHostUiAuditService.TryCaptureSettings(this);
            RefreshValidationSummary();
            if (entrancePlayed)
            {
                SettingsShell.Opacity = 1;
                return;
            }

            entrancePlayed = true;
            BeginUiSafely(PlayEntranceAnimation, DispatcherPriority.Loaded);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible) return;
            ApplyAdaptiveTheme();
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            => QueueResponsiveLayout(e.NewSize);

        private void OnSettingsFieldChanged(object sender, RoutedEventArgs e) => RefreshValidationSummary();

        private void RefreshValidationSummary()
        {
            var settings = CurrentSettings;
            if (settings == null || SettingsValidationSummary == null) return;
            var errors = new List<string>();
            settings.VerifySettings(out errors);
            if (errors.Count == 0)
            {
                SettingsValidationSummary.Visibility = Visibility.Collapsed;
                return;
            }
            SettingsValidationSummary.Text = "设置需要修正：" + string.Join("；", errors.Take(4));
            SettingsValidationSummary.Visibility = Visibility.Visible;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // A detached Playnite settings page must not keep an entrance animation clock alive.
            // Import/export can still finish in the background; only its visual feedback is gated.
            SettingsShell.BeginAnimation(UIElement.OpacityProperty, null);
            if (SettingsShell.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.YProperty, null);
            }
            responsiveLayoutPending = false;
            adaptiveThemePending = false;
            if (systemParametersSubscribed)
            {
                SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
                systemParametersSubscribed = false;
            }
        }

        private void OnSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            QueueAdaptiveThemeUpdate();
        }

        private void QueueResponsiveLayout(Size size)
        {
            pendingResponsiveSize = size;
            if (responsiveLayoutPending) return;
            responsiveLayoutPending = true;
            BeginUiSafely(() =>
            {
                responsiveLayoutPending = false;
                if (!IsLoaded) return;
                ApplyResponsiveLayout(pendingResponsiveSize.Width, pendingResponsiveSize.Height);
            }, DispatcherPriority.Render);
        }

        private void OnThemeModeChanged(object sender, SelectionChangedEventArgs e)
        {
            QueueAdaptiveThemeUpdate();
        }

        private void OnSettingsTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = SettingsSectionTabs?.SelectedIndex ?? 0;
            SetCategoryVisibility(SettingsGeneralPanel, selectedIndex == 0);
            SetCategoryVisibility(SettingsBackupPanel, selectedIndex == 1);
            SetCategoryVisibility(SettingsAppearancePanel, selectedIndex == 2);
            SetCategoryVisibility(SettingsAutomationPanel, selectedIndex == 3);
            SetCategoryVisibility(SettingsMigrationPanel, selectedIndex == 4);
            ScrollSelectedCategoryIntoView();
            ScheduleScrollSelectedCategoryIntoView();
        }

        private static void SetCategoryVisibility(UIElement? panel, bool isVisible)
        {
            if (panel != null)
                panel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ScheduleScrollSelectedCategoryIntoView()
        {
            if (scrollSelectionPending) return;
            scrollSelectionPending = true;
            BeginUiSafely(() =>
            {
                scrollSelectionPending = false;
                ScrollSelectedCategoryIntoView();
            }, DispatcherPriority.Loaded);
        }

        private void ScrollSelectedCategoryIntoView()
        {
            if (SettingsSectionTabs?.SelectedItem is not ListBoxItem selected)
                return;
            try
            {
                selected.BringIntoView();
            }
            catch (InvalidOperationException)
            {
                // Selected ListBoxItem can be disconnected from the template during startup.
            }
        }

        private void OnVisualSettingChanged(object sender, RoutedEventArgs e)
        {
            QueueAdaptiveThemeUpdate();
        }

        private void OnGlassStrengthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            QueueAdaptiveThemeUpdate();
        }

        // Slider and toggle events can arrive faster than the Dispatcher can recreate the
        // local palette and WPF-UI resources. Keep only the latest settings state pending.
        private void QueueAdaptiveThemeUpdate()
        {
            if (!IsLoaded || adaptiveThemePending) return;
            adaptiveThemePending = true;
            if (BeginUiSafely(() =>
            {
                adaptiveThemePending = false;
                if (!IsLoaded) return;
                ApplyAdaptiveTheme();
            }, DispatcherPriority.Background)) return;

            adaptiveThemePending = false;
        }

        private bool BeginUiSafely(Action action, DispatcherPriority priority)
        {
            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return false;
            try
            {
                Dispatcher.BeginInvoke(action, priority);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "GameSaveCenter skipped a deferred settings UI callback because the dispatcher is unavailable.");
                return false;
            }
        }

        private void OnExportSettingsClick(object sender, RoutedEventArgs e)
            => _ = ObserveUiOperationAsync(ExportSettingsAsync, "GameSaveCenter settings export failed.");

        private async Task ExportSettingsAsync()
        {
            var settings = CurrentSettings;
            if (settingsTransferInProgress || settings == null) return;
            var dialog = new SaveFileDialog
            {
                Title = "导出 GameSaveCenter 设置",
                Filter = "GameSaveCenter 设置 (*.json)|*.json",
                FileName = $"GameSaveCenter-settings-{DateTime.Now:yyyyMMdd}.json",
                AddExtension = true,
                DefaultExt = ".json"
            };
            if (dialog.ShowDialog() != true) return;

            settingsTransferInProgress = true;
            try
            {
                var json = settings.ExportPortableJson();
                var fileName = dialog.FileName;
                await Task.Run(() => File.WriteAllText(fileName, json, new System.Text.UTF8Encoding(false)));
                if (!CanPresentUiFeedback) return;
                ShowSettingsSnackbar("设置已导出", "文件不包含 Rclone 密码，但会包含本地路径和云端目标名称。");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GameSaveCenter settings export failed.");
                ShowSettingsError("无法导出设置：" + ex.Message);
            }
            finally
            {
                settingsTransferInProgress = false;
            }
        }

        private void OnImportSettingsClick(object sender, RoutedEventArgs e)
            => _ = ObserveUiOperationAsync(ImportSettingsAsync, "GameSaveCenter settings import failed.");

        private async Task ImportSettingsAsync()
        {
            var settings = CurrentSettings;
            if (settingsTransferInProgress || settings == null) return;
            var dialog = new OpenFileDialog
            {
                Title = "导入 GameSaveCenter 设置",
                Filter = "GameSaveCenter 设置 (*.json)|*.json|所有文件 (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() != true) return;

            settingsTransferInProgress = true;
            try
            {
                var fileName = dialog.FileName;
                var json = await Task.Run(() =>
                {
                    var info = new FileInfo(fileName);
                    if (info.Length > 1024 * 1024) throw new InvalidDataException("设置文件超过 1 MiB 安全上限。");
                    return File.ReadAllText(fileName);
                });
                var report = settings.ImportPortableJson(json);
                if (!CanPresentUiFeedback) return;
                DataContext = null;
                DataContext = settings;
                ApplyAdaptiveTheme();
                ApplyResponsiveLayout(ActualWidth, ActualHeight);
                await ShowImportReportAsync(report.Summary, report.MissingPaths.Count != 0);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GameSaveCenter settings import failed.");
                ShowSettingsError("无法导入设置：" + ex.Message);
            }
            finally
            {
                settingsTransferInProgress = false;
            }
        }

        private Task ShowImportReportAsync(string summary, bool hasMissingPaths)
        {
            // WPF-UI ContentDialogHost is Window-wide and cannot be placed in Playnite pages.
            // The import has already completed; MessageBox gives the report a reliable modal path.
            ShowSettingsMessage(summary, "GameSaveCenter 设置迁移报告",
                hasMissingPaths ? MessageBoxImage.Warning : MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        private void ShowSettingsSnackbar(string title, string message)
        {
            // Keep settings feedback on the native modal path. WPF-UI SnackbarPresenter can
            // resolve deferred CornerRadius resources outside a stable Playnite window scope.
            ShowSettingsMessage(message, title, MessageBoxImage.Information);
        }

        private async Task ObserveUiOperationAsync(Func<Task> operation, string errorMessage)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, errorMessage);
                ShowSettingsError("设置操作失败：" + ex.Message);
            }
        }

        private bool CanPresentUiFeedback => IsLoaded && !Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished;

        private void ShowSettingsError(string message)
            => ShowSettingsMessage(message, "GameSaveCenter", MessageBoxImage.Error);

        private void ShowSettingsMessage(string message, string title, MessageBoxImage image)
        {
            if (!CanPresentUiFeedback)
            {
                Logger.Debug("GameSaveCenter skipped settings feedback because the page is no longer loaded.");
                return;
            }

            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, image);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GameSaveCenter could not present settings feedback.");
            }
        }

        private void PlayEntranceAnimation()
        {
            if (!MotionEnabled)
            {
                SettingsShell.Opacity = 1;
                SettingsShell.RenderTransform = Transform.Identity;
                return;
            }

            var translate = new TranslateTransform(0, 14);
            SettingsShell.RenderTransform = translate;
            SettingsShell.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(270))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
            translate.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(310))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                });
        }

        private void ApplyAdaptiveTheme()
        {
            // High contrast is an accessibility mode, not merely a visual preference: all
            // translucent material and its backing blur must take the opaque fallback path.
            var glassEnabled = (CurrentSettings?.EnableGlassEffects ?? true) && !SystemParameters.HighContrast;
            var strength = CurrentSettings?.GlassEffectStrength ?? 78;
            var palette = AdaptiveThemePaletteFactory.Create(this, glassEnabled, strength, CurrentSettings?.ThemeMode ?? GameSaveCenterThemeMode.FollowPlaynite);

            AdaptiveThemePaletteFactory.ApplyAccentResources(Resources, palette);
            AdaptiveThemePaletteFactory.ApplyMaterialResources(Resources, palette, glassEnabled, MotionEnabled);
            AdaptiveThemePaletteFactory.ApplyWpfUiResources(Resources, palette);
            Resources["GscPrimaryTextBrush"] = AdaptiveThemePaletteFactory.Brush(palette.PrimaryText);
            Resources["GscSecondaryTextBrush"] = AdaptiveThemePaletteFactory.Brush(palette.SecondaryText);
            Resources["GscMutedTextBrush"] = AdaptiveThemePaletteFactory.Brush(palette.MutedText);
            Resources["GscControlFillBrush"] = AdaptiveThemePaletteFactory.Brush(palette.ControlFill);
            Resources["GscControlStrokeBrush"] = AdaptiveThemePaletteFactory.Brush(palette.ControlStroke);
            Resources["GscDividerBrush"] = AdaptiveThemePaletteFactory.Brush(palette.Divider);
            Resources["GscGlassFillBrush"] = AdaptiveThemePaletteFactory.Gradient(palette.SurfaceTop, palette.SurfaceBottom);
            Resources["GscGlassStrokeBrush"] = AdaptiveThemePaletteFactory.Brush(palette.ControlStroke);
            Resources["GscBackdropBrush"] = AdaptiveThemePaletteFactory.Brush(palette.Backdrop);
            WpfUiThemeScope.Apply(Resources, palette.IsDark);
            AdaptiveThemePaletteFactory.ApplyDemoCoreResources(Resources, palette.IsDark);
            AdaptiveThemePaletteFactory.ApplySettingsMaterialResources(Resources, palette, glassEnabled);

            // Keep the fixed background ambient layer out of the render tree when glass is
            // disabled. Visibility is preferable to retaining a decorative visual at opacity 0.
            SettingsAmbientLayer.Visibility = glassEnabled ? Visibility.Visible : Visibility.Collapsed;
            SettingsAmbientLayer.Opacity = glassEnabled
                ? (palette.IsDark ? 0.82 : 0.64) * Math.Max(0.2, Math.Min(1, strength / 100.0))
                : 0;
        }

        internal void ApplyThemeForAudit(GameSaveCenterThemeMode mode)
        {
            if (CurrentSettings != null)
                CurrentSettings.ThemeMode = mode;
            ApplyAdaptiveTheme();
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
            UpdateLayout();
        }

        private void ApplyResponsiveLayout(double width, double height)
        {
            if (SettingsShell == null || SettingsHeaderGrid == null || SettingsHeaderHintRow == null
                || SettingsHeaderSubtitle == null || SettingsSaveHint == null || SettingsSectionTabs == null
                || SettingsWorkspace == null || SettingsCategoryRail == null || SettingsScroller == null
                || SettingsCompactContentRow == null || SettingsIntroDescription == null
                || SettingsHeaderIcon == null || SettingsHeader == null) return;

            // SettingsShell is the real layout surface.  The Playnite settings host can be
            // wider than this shell because the shell is capped at 1360 DIP and inset by the
            // product frame.  Using the outer UserControl width here made the form keep two
            // or three columns after its actual content had already crossed the readable
            // threshold, squeezing ComboBox/TextBox rows instead of following the Demo's
            // content-width breakpoints.
            var layoutWidth = SettingsShell.ActualWidth > 0
                ? SettingsShell.ActualWidth
                : Math.Max(320, width - 2 * 18 - 2 * 20);

            // Keep the Demo's left category rail at the common Playnite content widths.
            // Only genuinely narrow hosts move the rail above the form; this prevents a
            // 1040px window from spending the entire first viewport on navigation.
            var expanded = layoutWidth >= 560;
            var compact = layoutWidth < 560;
            var narrow = layoutWidth < 520;
            var shortHeight = height > 0 && height < 760;
            var horizontalMargin = narrow ? 10 : 18;
            var contentWidth = Math.Max(320, layoutWidth - horizontalMargin * 2 - 40);
            var formWidth = compact ? contentWidth : Math.Max(320, contentWidth - 248);

            // Compact/short headers stop competing with the settings body: the long
            // description and secondary hero text reduce to the essential title, and
            // the category rail keeps its own readable strip below the header.
            SettingsIntroDescription.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            var compactHeaderHeight = compact && shortHeight;
            SettingsHeader.Padding = compactHeaderHeight
                ? new Thickness(12, 8, 12, 8)
                : compact ? new Thickness(14, 10, 14, 10) : new Thickness(16);
            SettingsHeader.MinHeight = compactHeaderHeight ? 56 : compact ? 68 : 76;
            SettingsHeader.Margin = compact ? new Thickness(0, 0, 0, 8) : new Thickness(0, 0, 0, 12);
            var headerIconSize = narrow || shortHeight ? 34d : compact ? 40d : 46d;
            SettingsHeaderIcon.Width = headerIconSize;
            SettingsHeaderIcon.Height = headerIconSize;
            SettingsHeaderIcon.Margin = compact ? new Thickness(0, 0, 10, 0) : new Thickness(0, 0, 12, 0);

            // The outer SettingsDemoShell owns the product-level 18-DIP breathing room.
            // Keep the inner content stretch-only so the demo shell does not regress into
            // a narrow, left-aligned island when the Playnite settings host is wide.
            SettingsDemoShell.Margin = new Thickness(horizontalMargin);
            SettingsShell.Margin = new Thickness(0);
            SettingsShell.HorizontalAlignment = HorizontalAlignment.Stretch;
            SettingsShell.Width = double.NaN;
            SettingsShell.MaxWidth = 1360;
            // SettingsScroller is the overflow channel. Keep context and save semantics
            // visible at every height; only constrain their width so compact headers wrap
            // instead of silently removing information.
            SettingsHeaderSubtitle.Visibility = narrow || shortHeight ? Visibility.Collapsed : Visibility.Visible;
            SettingsHeaderSubtitle.MaxWidth = narrow ? 300 : double.PositiveInfinity;
            SettingsSaveHint.Visibility = narrow || shortHeight ? Visibility.Collapsed : Visibility.Visible;
            SettingsSaveHint.MaxWidth = layoutWidth >= 1040 ? 320 : narrow ? 180 : 230;
            var stackHeaderHint = compact;
            SettingsHeaderHintRow.Height = stackHeaderHint ? GridLength.Auto : new GridLength(0);
            Grid.SetRow(SettingsSaveHint, stackHeaderHint ? 1 : 0);
            Grid.SetColumn(SettingsSaveHint, stackHeaderHint ? 1 : 2);
            Grid.SetColumnSpan(SettingsSaveHint, stackHeaderHint ? 2 : 1);
            SettingsSaveHint.HorizontalAlignment = stackHeaderHint
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Stretch;
            SettingsSaveHint.VerticalAlignment = stackHeaderHint
                ? VerticalAlignment.Top
                : VerticalAlignment.Center;
            SettingsSaveHint.Margin = stackHeaderHint
                ? new Thickness(0, 12, 0, 0)
                : new Thickness(14, 0, 0, 0);
            if (compact)
            {
                Grid.SetRow(SettingsCategoryRail, 0);
                Grid.SetColumn(SettingsCategoryRail, 0);
                Grid.SetColumnSpan(SettingsCategoryRail, 3);
                Grid.SetRow(SettingsScroller, 1);
                Grid.SetColumn(SettingsScroller, 0);
                Grid.SetColumnSpan(SettingsScroller, 3);
                SettingsWorkspace.RowDefinitions[0].Height = GridLength.Auto;
                SettingsCompactContentRow.Height = new GridLength(1, GridUnitType.Star);
                SettingsCategoryRail.Margin = new Thickness(0, 0, 0, 8);
                SettingsSectionTabs.MinHeight = 0;
                SettingsSectionTabs.MaxHeight = narrow ? 180 : 200;
            }
            else
            {
                Grid.SetRow(SettingsCategoryRail, 0);
                Grid.SetColumn(SettingsCategoryRail, 0);
                Grid.SetColumnSpan(SettingsCategoryRail, 1);
                Grid.SetRow(SettingsScroller, 0);
                Grid.SetColumn(SettingsScroller, 2);
                Grid.SetColumnSpan(SettingsScroller, 1);
                SettingsWorkspace.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                SettingsCompactContentRow.Height = new GridLength(0);
                SettingsCategoryRail.Margin = new Thickness(0);
                SettingsSectionTabs.MinHeight = 0;
                SettingsSectionTabs.MaxHeight = double.PositiveInfinity;
            }

            var twoColumns = formWidth >= 720;
            if (StorageFormatFields != null)
            {
                StorageFormatFields.Columns = twoColumns ? 2 : 1;
            }
            if (StorageNumericFields != null)
            {
                StorageNumericFields.Columns = formWidth >= 720 ? 3 : formWidth >= 480 ? 2 : 1;
            }
            if (AppearanceFields != null)
            {
                AppearanceFields.Columns = twoColumns ? 2 : 1;
            }
            if (AutomationIntervalFields != null)
            {
                AutomationIntervalFields.Columns = expanded && formWidth >= 930 ? 3 : formWidth >= 650 ? 2 : 1;
            }

            ScrollSelectedCategoryIntoView();
        }
    }
}
