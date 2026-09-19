using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Diagnostics;
using GameSaveCenter.Playnite.Infrastructure;
using Microsoft.Win32;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Settings
{
    public partial class GameSaveCenterSettingsView : UserControl
    {
        public static readonly DependencyProperty SearchTermsProperty = DependencyProperty.RegisterAttached(
            "SearchTerms",
            typeof(string),
            typeof(GameSaveCenterSettingsView),
            new PropertyMetadata(string.Empty));

        public static void SetSearchTerms(DependencyObject element, string value)
            => element.SetValue(SearchTermsProperty, value ?? string.Empty);

        public static string GetSearchTerms(DependencyObject element)
            => (string)element.GetValue(SearchTermsProperty);

        private static readonly ILogger Logger = LogManager.GetLogger();
        private bool entrancePlayed;
        private bool settingsTransferInProgress;
        private bool responsiveLayoutPending;
        private bool adaptiveThemePending;
        private bool validationPending;
        private bool pathValidationPending;
        private bool systemParametersSubscribed;
        private bool scrollSelectionPending;
        private bool settingsBaselineInitialized;
        private bool settingsClosePromptOpen;
        private bool restoreDraftFocusOnLoad;
        private int firstValidationCategoryIndex;
        private Size pendingResponsiveSize;
        private GameSaveCenterSettings? observedSettings;
        private Window? observedHostWindow;
        private FrameworkElement? draftFocusTarget;
        private int draftFocusCategoryIndex;
        private string savedSettingsFingerprint = string.Empty;
        private long pathValidationGeneration;
        private IReadOnlyList<string> pathValidationErrors = Array.Empty<string>();
        private readonly SettingsSaveFeedbackState saveFeedback = new();
        private readonly LatestAsyncValidationCoordinator<SettingsPathValidationSnapshot, IReadOnlyList<string>> pathValidationCoordinator = new();
        private readonly List<ValidationFieldTarget> validationFieldTargets = new();
        private readonly List<SettingsSearchTarget> settingsSearchTargets = new();
        private ValidationFieldTarget? firstValidationTarget;
        private bool settingsSearchApplying;
        private int settingsSearchOriginCategory = -1;
        private string appliedSettingsSearchQuery = string.Empty;

        private sealed class ValidationFieldTarget
        {
            private readonly Func<FrameworkElement?> resolveElement;

            public ValidationFieldTarget(int categoryIndex, string displayName, Func<FrameworkElement?> resolveElement)
            {
                CategoryIndex = categoryIndex;
                DisplayName = displayName;
                this.resolveElement = resolveElement;
            }

            public int CategoryIndex { get; }
            public string DisplayName { get; }
            public FrameworkElement? Element => resolveElement();
        }

        private sealed class ValidationSummaryEntry
        {
            public ValidationSummaryEntry(string message, ValidationFieldTarget? target)
            {
                Message = message;
                Target = target;
            }

            public string Message { get; }
            public ValidationFieldTarget? Target { get; }
        }

        private sealed class SettingsSearchTarget
        {
            public SettingsSearchTarget(int categoryIndex, FrameworkElement element, string terms)
            {
                CategoryIndex = categoryIndex;
                Element = element;
                Terms = terms;
            }

            public int CategoryIndex { get; }
            public FrameworkElement Element { get; }
            public string Terms { get; }
        }

        public GameSaveCenterSettingsView()
        {
            InitializeComponent();
            RegisterSettingsSearchTargets();
            RegisterValidationFieldTargets();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
            SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;
            AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnSettingsFieldChanged));
            AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnSettingsFieldChanged));
            AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(OnSettingsFieldChanged));
            AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnSettingsFieldChanged));
            AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnSettingsFieldChanged));
            AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnSettingsValidationErrorChanged));
        }

        private void RegisterSettingsSearchTargets()
        {
            settingsSearchTargets.Clear();
            var panels = new FrameworkElement[]
            {
                SettingsGeneralPanel,
                SettingsBackupPanel,
                SettingsAppearancePanel,
                SettingsAutomationPanel,
                SettingsMigrationPanel
            };
            for (var categoryIndex = 0; categoryIndex < panels.Length; categoryIndex++)
            {
                foreach (var node in EnumerateLogicalTree(panels[categoryIndex]))
                {
                    if (node is not FrameworkElement element) continue;
                    var terms = GetSearchTerms(element);
                    if (!string.IsNullOrWhiteSpace(terms))
                        settingsSearchTargets.Add(new SettingsSearchTarget(categoryIndex, element, terms));
                }
            }
        }

        private static IEnumerable<DependencyObject> EnumerateLogicalTree(DependencyObject root)
        {
            yield return root;
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                foreach (var descendant in EnumerateLogicalTree(child))
                    yield return descendant;
            }
        }

        private void OnSettingsSearchTextChanged(object sender, TextChangedEventArgs e)
            => ApplySettingsSearch();

        private bool HasSettingsSearch
            => !string.IsNullOrWhiteSpace(SettingsSearchTextBox?.Text);

        private void ApplySettingsSearch()
        {
            if (settingsSearchApplying) return;
            settingsSearchApplying = true;
            try
            {
                var tabs = SettingsSectionTabs;
                var summary = SettingsSearchSummary;
                if (tabs == null || summary == null) return;
                var query = SettingsSearchTextBox?.Text?.Trim() ?? string.Empty;
                var wasSearching = appliedSettingsSearchQuery.Length > 0;
                var panels = new FrameworkElement[]
                {
                    SettingsGeneralPanel,
                    SettingsBackupPanel,
                    SettingsAppearancePanel,
                    SettingsAutomationPanel,
                    SettingsMigrationPanel
                };
                if (query.Length > 0 && !wasSearching)
                    settingsSearchOriginCategory = Math.Max(0, Math.Min(panels.Length - 1, SettingsSectionTabs?.SelectedIndex ?? 0));
                if (query.Length == 0)
                {
                    foreach (var target in settingsSearchTargets)
                        target.Element.Visibility = Visibility.Visible;
                    var selectedIndex = wasSearching && settingsSearchOriginCategory >= 0
                        ? settingsSearchOriginCategory
                        : tabs.SelectedIndex;
                    for (var index = 0; index < panels.Length; index++)
                        SetCategoryVisibility(panels[index], index == selectedIndex);
                    summary.Visibility = Visibility.Collapsed;
                    appliedSettingsSearchQuery = string.Empty;
                    settingsSearchOriginCategory = -1;
                    if (wasSearching && tabs.SelectedIndex != selectedIndex)
                        tabs.SelectedIndex = selectedIndex;
                    return;
                }

                var matchingCategories = new HashSet<int>();
                var matchCount = 0;
                foreach (var target in settingsSearchTargets)
                {
                    var matches = ContainsSearchTerm(target.Terms, query);
                    target.Element.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
                    if (matches)
                    {
                        matchCount++;
                        matchingCategories.Add(target.CategoryIndex);
                    }
                }

                for (var index = 0; index < panels.Length; index++)
                    SetCategoryVisibility(panels[index], matchingCategories.Contains(index));

                summary.Text = matchCount == 0
                    ? $"没有找到“{query}”匹配的设置；清空搜索恢复原分类。"
                    : $"找到 {matchCount} 个匹配设置，涉及 {matchingCategories.Count} 个分类；搜索只改变可见字段，不会修改配置。";
                summary.Visibility = Visibility.Visible;
                var firstCategory = matchingCategories.OrderBy(index => index).DefaultIfEmpty(-1).First();
                if (firstCategory >= 0 && tabs.SelectedIndex != firstCategory)
                    tabs.SelectedIndex = firstCategory;
                appliedSettingsSearchQuery = query;
            }
            finally
            {
                settingsSearchApplying = false;
            }
        }

        private static bool ContainsSearchTerm(string value, string query)
        {
            var compactValue = value.Replace(" ", string.Empty);
            var compactQuery = query.Replace(" ", string.Empty);
            if (compactValue.IndexOf(compactQuery, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            return query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .All(token => value.IndexOf(token, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        private void RegisterValidationFieldTargets()
        {
            validationFieldTargets.Add(new ValidationFieldTarget(0, "Worker 可执行文件", () => WorkerExecutableTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(0, "Ludusavi 可执行文件", () => LudusaviExecutableTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(0, "存档目录", () => LudusaviBackupDirectoryTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(0, "Rclone 可执行文件", () => RcloneExecutableTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(0, "媒体目录", () => MediaArchiveDirectoryTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(0, "本地镜像目录", () => LocalMirrorPathTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(1, "完整备份保留数量", () => FullBackupLimitTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(1, "差异备份保留数量", () => DifferentialBackupLimitTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(1, "压缩等级", () => CompressionLevelTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(2, "毛玻璃强度", () => GlassStrengthSlider));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "默认游玩中备份间隔", () => DefaultBackupIntervalMinutesTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "进程检测间隔", () => ProcessPollingSecondsTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "管理面板刷新间隔", () => DashboardRefreshSecondsTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "最近保护统计窗口", () => RecentProtectionWindowComboBox));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "恢复巡检间隔", () => HealthInspectionIntervalMinutesTextBox));
            validationFieldTargets.Add(new ValidationFieldTarget(3, "重新验证有效期", () => HealthInspectionStaleAfterDaysTextBox));
        }

        private GameSaveCenterSettings? CurrentSettings => DataContext as GameSaveCenterSettings;

        private bool MotionEnabled => GscMotion.IsEnabled(CurrentSettings?.EnableUiAnimations ?? true);

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!systemParametersSubscribed)
            {
                SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
                systemParametersSubscribed = true;
            }
            ApplyAdaptiveTheme();
            ApplyResponsiveLayout(ActualWidth, ActualHeight);
            EnsureHostWindowSize();
            AttachHostWindowClosingHandler();
            BeginUiSafely(EnsureHostWindowSize, DispatcherPriority.ContextIdle);
            RealHostUiAuditService.TryCaptureSettings(this);
            StartPathValidation(++pathValidationGeneration);
            RefreshValidationSummary();
            RefreshSaveState();
            if (restoreDraftFocusOnLoad && HasUnsavedSettings)
            {
                restoreDraftFocusOnLoad = false;
                BeginUiSafely(RestoreDraftFocus, DispatcherPriority.Loaded);
            }
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

        private void EnsureHostWindowSize()
        {
            var hostWindow = Window.GetWindow(this);
            if (hostWindow == null || hostWindow.WindowState == WindowState.Maximized)
                return;

            // The UserControl deliberately has no minimum size: compact layout must be
            // allowed to take over when the user resizes the host smaller.  The first
            // settings visit, however, should open with enough room for the category rail
            // and form instead of inheriting Playnite's narrow fallback dialog size.
            const double preferredWidth = 1280;
            const double preferredHeight = 840;
            var workArea = SystemParameters.WorkArea;
            var targetWidth = Math.Min(preferredWidth, Math.Max(1024, workArea.Width - 80));
            var targetHeight = Math.Min(preferredHeight, Math.Max(720, workArea.Height - 80));

            hostWindow.SizeToContent = SizeToContent.Manual;
            hostWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            hostWindow.VerticalContentAlignment = VerticalAlignment.Stretch;
            var currentWidth = hostWindow.ActualWidth > 0 ? hostWindow.ActualWidth : hostWindow.Width;
            var currentHeight = hostWindow.ActualHeight > 0 ? hostWindow.ActualHeight : hostWindow.Height;
            if (double.IsNaN(currentWidth) || currentWidth < targetWidth)
                hostWindow.Width = targetWidth;
            if (double.IsNaN(currentHeight) || currentHeight < targetHeight)
                hostWindow.Height = targetHeight;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidatePathValidation();
            saveFeedback.Reset();
            if (observedSettings != null)
            {
                observedSettings.SettingsCommitted -= OnSettingsCommitted;
                observedSettings.SettingsReverted -= OnSettingsReverted;
                observedSettings.SettingsSaveStarted -= OnSettingsSaveStarted;
                observedSettings.SettingsApplyStarted -= OnSettingsApplyStarted;
                observedSettings.SettingsApplyCompleted -= OnSettingsApplyCompleted;
                observedSettings.SettingsSaveFailed -= OnSettingsSaveFailed;
            }

            observedSettings = e.NewValue as GameSaveCenterSettings;
            if (observedSettings == null)
            {
                RefreshSaveState();
                return;
            }

            observedSettings.SettingsCommitted += OnSettingsCommitted;
            observedSettings.SettingsReverted += OnSettingsReverted;
            observedSettings.SettingsSaveStarted += OnSettingsSaveStarted;
            observedSettings.SettingsApplyStarted += OnSettingsApplyStarted;
            observedSettings.SettingsApplyCompleted += OnSettingsApplyCompleted;
            observedSettings.SettingsSaveFailed += OnSettingsSaveFailed;
            if (!settingsTransferInProgress || !settingsBaselineInitialized)
            {
                savedSettingsFingerprint = observedSettings.GetEditBaselineFingerprint();
                settingsBaselineInitialized = true;
            }

            if (IsLoaded)
                StartPathValidation(pathValidationGeneration);
            RefreshValidationSummary();
            RefreshSaveState();
        }

        private void OnSettingsCommitted(object? sender, EventArgs e)
        {
            if (sender is not GameSaveCenterSettings settings) return;
            savedSettingsFingerprint = settings.CreateSettingsFingerprint();
            settingsBaselineInitialized = true;
            InvalidatePathValidation();
            if (IsLoaded)
                StartPathValidation(pathValidationGeneration);
            RefreshValidationSummary();
            RefreshSaveState();
        }

        private void OnSettingsSaveStarted(object? sender, EventArgs e)
        {
            saveFeedback.BeginSave();
            if (IsLoaded) RefreshSaveState();
        }

        private void OnSettingsApplyStarted(object? sender, EventArgs e)
        {
            saveFeedback.BeginApply();
            if (IsLoaded) RefreshSaveState();
        }

        private void OnSettingsApplyCompleted(object? sender, EventArgs e)
        {
            BeginUiSafely(() =>
            {
                if (!IsLoaded) return;
                saveFeedback.CompleteApply();
                RefreshSaveState();
            }, DispatcherPriority.Background);
        }

        private void OnSettingsSaveFailed(object? sender, SettingsSaveFailedEventArgs e)
        {
            BeginUiSafely(() =>
            {
                if (!IsLoaded) return;
                saveFeedback.Fail(e);
                RefreshSaveState();
            }, DispatcherPriority.Background);
        }

        private void OnSettingsReverted(object? sender, EventArgs e)
        {
            if (sender is not GameSaveCenterSettings settings) return;
            savedSettingsFingerprint = settings.CreateSettingsFingerprint();
            settingsBaselineInitialized = true;
            saveFeedback.Reset();
            InvalidatePathValidation();
            if (IsLoaded)
                StartPathValidation(pathValidationGeneration);
            RefreshValidationSummary();
            RefreshSaveState();
        }

        private void OnSettingsFieldChanged(object sender, RoutedEventArgs e) => QueueValidationSummaryUpdate();

        private void OnSettingsValidationErrorChanged(object sender, RoutedEventArgs e)
            => QueueValidationSummaryUpdate();

        // Coalesce per-keystroke notifications so typing a long path does not synchronously
        // hit the filesystem on every character or compete with the input caret/layout pass.
        private void QueueValidationSummaryUpdate()
        {
            if (!IsLoaded) return;
            saveFeedback.ResetFailureAfterEdit();
            pathValidationGeneration++;
            if (validationPending) return;
            validationPending = true;
            if (BeginUiSafely(() =>
            {
                validationPending = false;
                if (!IsLoaded) return;
                StartPathValidation(pathValidationGeneration);
                RefreshValidationSummary();
            }, DispatcherPriority.Background)) return;

            validationPending = false;
        }

        private void RefreshValidationSummary()
            => RefreshValidationSummaryCore(!IsLoaded);

        private void RefreshValidationSummaryCore(bool includeSynchronousPathValidation)
        {
            var settings = CurrentSettings;
            if (settings == null || SettingsValidationSummary == null || SettingsValidationDetails == null
                || SettingsValidationDetailsText == null || SettingsGeneralValidationHint == null) return;
            var errors = new List<string>();
            if (includeSynchronousPathValidation)
                settings.VerifySettings(out errors);
            else
            {
                settings.VerifySettingsWithoutPathAvailability(out errors);
                errors.AddRange(pathValidationErrors);
            }
            var entries = errors
                .Select(error => new ValidationSummaryEntry(error, ResolveValidationTarget(error)))
                .ToList();
            foreach (var target in validationFieldTargets)
            {
                var element = target.Element;
                if (element == null || !element.IsEnabled) continue;
                foreach (var validationError in Validation.GetErrors(element))
                {
                    var message = validationError.ErrorContent?.ToString();
                    if (string.IsNullOrWhiteSpace(message)
                        || entries.Any(entry => ReferenceEquals(entry.Target, target)
                            && string.Equals(entry.Message, message, StringComparison.Ordinal)))
                        continue;
                    entries.Add(new ValidationSummaryEntry(message!, target));
                }
            }

            UpdateValidationFieldHelp(entries);
            firstValidationTarget = entries.Select(entry => entry.Target).FirstOrDefault(target => target != null);
            if (entries.Count == 0)
            {
                firstValidationTarget = null;
                SettingsValidationSummary.Visibility = Visibility.Collapsed;
                SettingsValidationLocateButton.Visibility = Visibility.Collapsed;
                SettingsValidationDetails.Visibility = Visibility.Collapsed;
                SettingsGeneralValidationHint.Visibility = Visibility.Collapsed;
                RefreshSaveState(true);
                return;
            }
            firstValidationCategoryIndex = firstValidationTarget?.CategoryIndex
                ?? FindValidationCategoryIndex(entries.Select(entry => entry.Message));
            // Keep the header as a compact status and leave the full messages either beside
            // their fields or behind an explicit disclosure.  Joining path-heavy errors here
            // used most of a small settings host before the user reached a single field.
            SettingsValidationSummary.Text = $"有 {entries.Count} 项设置需要修正";
            SettingsValidationSummary.ToolTip = string.Join(Environment.NewLine, entries.Select(entry => entry.Message));
            SettingsValidationSummary.Visibility = Visibility.Visible;
            SettingsValidationLocateButton.Visibility = Visibility.Visible;
            RebuildValidationDetails(entries);
            SettingsValidationDetails.Visibility = Visibility.Visible;
            var generalErrors = entries
                .Where(entry => (entry.Target?.CategoryIndex ?? FindValidationCategoryIndex(new[] { entry.Message })) == 0)
                .Select(entry => entry.Message)
                .ToArray();
            SettingsGeneralValidationHint.Text = generalErrors.Length == 0
                ? string.Empty
                : string.Join(Environment.NewLine, generalErrors.Select(error => "需要修正：" + error));
            SettingsGeneralValidationHint.Visibility = generalErrors.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            SettingsValidationLocateButton.ToolTip = firstValidationTarget == null
                ? $"切换到“{GetSettingsCategoryName(firstValidationCategoryIndex)}”并查看首个校验错误。"
                : $"切换到“{GetSettingsCategoryName(firstValidationCategoryIndex)}”并聚焦{firstValidationTarget.DisplayName}。";
            RefreshSaveState(false);
        }

        private void StartPathValidation(long generation)
        {
            if (!IsLoaded || generation != pathValidationGeneration) return;
            var settings = CurrentSettings;
            if (settings == null) return;

            pathValidationPending = true;
            pathValidationErrors = Array.Empty<string>();
            var snapshot = settings.CreatePathValidationSnapshot();
            pathValidationCoordinator.Start(
                snapshot,
                SettingsPathValidationService.ValidateAsync,
                (_, errors) => PresentPathValidationResult(generation, errors),
                (_, exception) => PresentPathValidationFailure(generation, exception));
        }

        private void PresentPathValidationResult(long generation, IReadOnlyList<string> errors)
        {
            BeginUiSafely(() =>
            {
                if (!IsLoaded || generation != pathValidationGeneration) return;
                pathValidationPending = false;
                pathValidationErrors = errors ?? Array.Empty<string>();
                RefreshValidationSummary();
            }, DispatcherPriority.Background);
        }

        private void PresentPathValidationFailure(long generation, Exception exception)
        {
            Logger.Error(exception, "GameSaveCenter settings path validation failed.");
            BeginUiSafely(() =>
            {
                if (!IsLoaded || generation != pathValidationGeneration) return;
                pathValidationPending = false;
                pathValidationErrors = new[] { "设置路径校验失败，请稍后重试。" };
                RefreshValidationSummary();
            }, DispatcherPriority.Background);
        }

        private void InvalidatePathValidation()
        {
            pathValidationGeneration++;
            pathValidationCoordinator.Cancel();
            pathValidationPending = false;
            pathValidationErrors = Array.Empty<string>();
        }

        private void OnSettingsValidationLocateClick(object sender, RoutedEventArgs e)
        {
            FocusValidationTarget(firstValidationTarget);
            e.Handled = true;
        }

        private ValidationFieldTarget? ResolveValidationTarget(string error)
        {
            if (ContainsOrdinal(error, "Worker")) return FindValidationTarget("Worker 可执行文件");
            if (ContainsOrdinal(error, "存档目录")) return FindValidationTarget("存档目录");
            if (ContainsOrdinal(error, "媒体目录")) return FindValidationTarget("媒体目录");
            if (ContainsOrdinal(error, "镜像")) return FindValidationTarget("本地镜像目录");
            if (ContainsOrdinal(error, "Ludusavi")) return FindValidationTarget("Ludusavi 可执行文件");
            if (ContainsOrdinal(error, "Rclone")) return FindValidationTarget("Rclone 可执行文件");
            if (ContainsOrdinal(error, "定时备份")) return FindValidationTarget("默认游玩中备份间隔");
            if (ContainsOrdinal(error, "进程检测")) return FindValidationTarget("进程检测间隔");
            if (ContainsOrdinal(error, "管理面板") || ContainsOrdinal(error, "面板刷新")) return FindValidationTarget("管理面板刷新间隔");
            if (ContainsOrdinal(error, "统计窗口") || ContainsOrdinal(error, "保护窗口")) return FindValidationTarget("最近保护统计窗口");
            if (ContainsOrdinal(error, "毛玻璃")) return FindValidationTarget("毛玻璃强度");
            if (ContainsOrdinal(error, "完整备份") || ContainsOrdinal(error, "完整版本")) return FindValidationTarget("完整备份保留数量");
            if (ContainsOrdinal(error, "差异备份") || ContainsOrdinal(error, "差异版本")) return FindValidationTarget("差异备份保留数量");
            if (ContainsOrdinal(error, "巡检间隔")) return FindValidationTarget("恢复巡检间隔");
            if (ContainsOrdinal(error, "验证有效期")) return FindValidationTarget("重新验证有效期");
            if (ContainsOrdinal(error, "压缩")) return FindValidationTarget("压缩等级");
            return null;
        }

        private ValidationFieldTarget? FindValidationTarget(string displayName)
            => validationFieldTargets.FirstOrDefault(target => string.Equals(target.DisplayName, displayName, StringComparison.Ordinal));

        private void UpdateValidationFieldHelp(IEnumerable<ValidationSummaryEntry> entries)
        {
            foreach (var target in validationFieldTargets)
            {
                var element = target.Element;
                if (element == null) continue;
                var messages = entries
                    .Where(entry => ReferenceEquals(entry.Target, target))
                    .Select(entry => entry.Message)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (messages.Length == 0)
                    AutomationProperties.SetHelpText(element, string.Empty);
                else
                    AutomationProperties.SetHelpText(element, string.Join("；", messages));
            }
        }

        private void RebuildValidationDetails(IReadOnlyList<ValidationSummaryEntry> entries)
        {
            SettingsValidationDetailsText.Inlines.Clear();
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                SettingsValidationDetailsText.Inlines.Add(new Run("• "));
                if (entry.Target == null)
                {
                    SettingsValidationDetailsText.Inlines.Add(new Run(entry.Message));
                }
                else
                {
                    var link = new Hyperlink(new Run(entry.Message))
                    {
                        Tag = entry.Target,
                        ToolTip = $"切换到“{GetSettingsCategoryName(entry.Target.CategoryIndex)}”并聚焦{entry.Target.DisplayName}"
                    };
                    AutomationProperties.SetName(link, $"定位错误：{entry.Message}");
                    AutomationProperties.SetHelpText(link, link.ToolTip?.ToString() ?? string.Empty);
                    link.Click += OnSettingsValidationErrorLinkClick;
                    SettingsValidationDetailsText.Inlines.Add(link);
                }

                if (index < entries.Count - 1)
                    SettingsValidationDetailsText.Inlines.Add(new LineBreak());
            }
        }

        private void OnSettingsValidationErrorLinkClick(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link && link.Tag is ValidationFieldTarget target)
            {
                FocusValidationTarget(target);
                e.Handled = true;
            }
        }

        private void FocusValidationTarget(ValidationFieldTarget? target)
        {
            if (HasSettingsSearch && SettingsSearchTextBox != null)
                SettingsSearchTextBox.Clear();
            if (SettingsSectionTabs == null) return;
            if (target == null)
            {
                SettingsSectionTabs.SelectedIndex = Math.Max(0, Math.Min(4, firstValidationCategoryIndex));
                SettingsSectionTabs.Focus();
                ScrollSelectedCategoryIntoView();
                return;
            }

            SettingsSectionTabs.SelectedIndex = Math.Max(0, Math.Min(4, target.CategoryIndex));
            ScrollSelectedCategoryIntoView();
            void FocusField()
            {
                var field = target.Element;
                if (field == null || !field.IsEnabled || !field.Focusable) return;
                try
                {
                    field.BringIntoView();
                }
                catch (InvalidOperationException)
                {
                    // A category can still be completing its visibility/layout pass.
                }
                field.Focus();
                Keyboard.Focus(field);
            }

            FocusField();
            BeginUiSafely(FocusField, DispatcherPriority.Loaded);
        }

        private static int FindValidationCategoryIndex(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                if (ContainsOrdinal(error, "压缩")
                    || ContainsOrdinal(error, "完整备份")
                    || ContainsOrdinal(error, "差异备份"))
                    return 1;
                if (ContainsOrdinal(error, "毛玻璃"))
                    return 2;
                if (ContainsOrdinal(error, "进程")
                    || ContainsOrdinal(error, "刷新")
                    || ContainsOrdinal(error, "巡检")
                    || ContainsOrdinal(error, "定时备份")
                    || ContainsOrdinal(error, "恢复可用性")
                    || ContainsOrdinal(error, "统计窗口")
                    || ContainsOrdinal(error, "通知"))
                    return 3;
                if (ContainsOrdinal(error, "Worker")
                    || ContainsOrdinal(error, "Ludusavi")
                    || ContainsOrdinal(error, "Rclone")
                    || ContainsOrdinal(error, "镜像"))
                    return 0;
            }
            return 0;
        }

        private static string GetSettingsCategoryName(int index)
            => index switch
            {
                1 => "备份与恢复",
                2 => "外观与可访问性",
                3 => "自动化与媒体",
                4 => "设置迁移",
                _ => "常规与目录"
            };

        private static bool ContainsOrdinal(string value, string token)
            => value.IndexOf(token, StringComparison.Ordinal) >= 0;

        private void RefreshSaveState()
        {
            var settings = CurrentSettings;
            if (settings == null || SettingsSaveHintText == null) return;
            RefreshSaveState(settings.VerifySettings(out var errors));
        }

        private void RefreshSaveState(bool settingsValid)
        {
            var settings = CurrentSettings;
            if (settings == null || SettingsSaveHintText == null) return;
            if (!settingsBaselineInitialized)
            {
                savedSettingsFingerprint = settings.GetEditBaselineFingerprint();
                settingsBaselineInitialized = true;
            }

            var isDirty = !string.Equals(savedSettingsFingerprint, settings.CreateSettingsFingerprint(), StringComparison.Ordinal);
            SettingsSaveHintText.Text = saveFeedback.IsSaving
                ? "正在保存设置 · 请稍候"
                : saveFeedback.IsApplying
                    ? "已写入 Playnite · 正在应用到 Worker"
                    : saveFeedback.HasFailure
                        ? "已保存 · Worker 应用失败"
                        : pathValidationPending
                            ? "正在校验路径 · 保存前请稍候"
                            : !settingsValid
                                ? "存在校验错误 · 保存前请修正"
                                : isDirty
                                    ? "有未保存更改 · 使用 Playnite 保存"
                                    : "已保存 · 由 Playnite 保存按钮提交";
            SettingsSaveHintText.Foreground = FindResource(saveFeedback.IsSaving || saveFeedback.IsApplying
                ? "GscWarningBrush"
                : saveFeedback.HasFailure || !settingsValid
                    ? "GscErrorBrush"
                    : isDirty
                        ? "GscWarningBrush"
                        : "GscSecondaryTextBrush") as Brush;
            SettingsSaveHintText.ToolTip = saveFeedback.IsSaving
                ? "Playnite 正在写入设置；请等待本次保存完成。重复保存请求会被忽略。"
                : saveFeedback.IsApplying
                    ? "设置已写入 Playnite，正在异步应用到 Worker；当前页面不会重复发起应用请求。"
                    : saveFeedback.HasFailure
                        ? saveFeedback.FailureMessage + " 请确认 Worker 和路径后重新打开设置并保存。"
                        : pathValidationPending
                            ? "路径正在后台校验；校验完成前不要提交设置。"
                            : !settingsValid
                                ? "存在设置校验错误，Playnite 保存前请先修正。"
                                : isDirty
                                    ? "设置已修改但尚未提交；请使用 Playnite 设置窗口的保存按钮，或使用取消按钮放弃修改。"
                                    : "当前设置已保存；继续修改后请使用 Playnite 的保存或取消按钮。";
        }

        private bool HasUnsavedSettings
        {
            get
            {
                var settings = CurrentSettings;
                return settings?.HasPendingEdit == true
                    && settingsBaselineInitialized
                    && !string.Equals(savedSettingsFingerprint, settings.CreateSettingsFingerprint(), StringComparison.Ordinal);
            }
        }

        private void AttachHostWindowClosingHandler()
        {
            var hostWindow = Window.GetWindow(this);
            if (ReferenceEquals(hostWindow, observedHostWindow)) return;
            DetachHostWindowClosingHandler();
            observedHostWindow = hostWindow;
            if (observedHostWindow != null)
                observedHostWindow.Closing += OnHostWindowClosing;
        }

        private void DetachHostWindowClosingHandler()
        {
            if (observedHostWindow == null) return;
            observedHostWindow.Closing -= OnHostWindowClosing;
            observedHostWindow = null;
        }

        private void OnHostWindowClosing(object? sender, CancelEventArgs e)
        {
            if (settingsClosePromptOpen || !HasUnsavedSettings) return;

            RememberDraftFocus();
            settingsClosePromptOpen = true;
            try
            {
                // Reuse the existing native settings feedback path. Yes is the destructive
                // choice; No keeps the window open and returns the user to the edited field.
                var result = MessageBox.Show(
                    observedHostWindow,
                    "设置有未保存更改。选择“否”继续编辑并返回当前字段；选择“是”放弃更改并关闭设置窗口。",
                    "GameSaveCenter 设置",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    CurrentSettings?.CancelEdit();
                    return;
                }

                e.Cancel = true;
                RestoreDraftFocus();
            }
            catch (Exception ex)
            {
                // A disappearing host must not turn an unavailable dialog into silent draft
                // loss. Keep the window open and preserve the edit buffer for a later retry.
                Logger.Error(ex, "GameSaveCenter could not confirm closing settings with unsaved changes.");
                e.Cancel = true;
                RestoreDraftFocus();
            }
            finally
            {
                settingsClosePromptOpen = false;
            }
        }

        private void RememberDraftFocus()
        {
            if (Keyboard.FocusedElement is not FrameworkElement focused
                || !focused.Focusable
                || !focused.IsEnabled
                || !IsDescendantOf(focused, this))
                return;

            draftFocusTarget = focused;
            draftFocusCategoryIndex = Math.Max(0, Math.Min(4, SettingsSectionTabs?.SelectedIndex ?? 0));
        }

        private void RestoreDraftFocus()
        {
            var target = draftFocusTarget;
            if (target == null || !target.Focusable || !target.IsEnabled) return;

            SettingsSectionTabs.SelectedIndex = draftFocusCategoryIndex;
            ScrollSelectedCategoryIntoView();
            try
            {
                target.BringIntoView();
            }
            catch (InvalidOperationException)
            {
                // The selected category may still be completing its layout pass.
            }

            if (target.Focus())
                Keyboard.Focus(target);
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject ancestor)
        {
            var current = child;
            while (current != null)
            {
                if (ReferenceEquals(current, ancestor)) return true;
                current = current is Visual || current is Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }
            return false;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // A detached Playnite settings page must not keep an entrance animation clock alive.
            // Import/export can still finish in the background; only its visual feedback is gated.
            RememberDraftFocus();
            restoreDraftFocusOnLoad = HasUnsavedSettings && draftFocusTarget != null;
            DetachHostWindowClosingHandler();
            InvalidatePathValidation();
            SettingsShell.BeginAnimation(UIElement.OpacityProperty, null);
            if (SettingsShell.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.YProperty, null);
            }
            responsiveLayoutPending = false;
            adaptiveThemePending = false;
            validationPending = false;
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
            // SelectionChanged can arrive before the TwoWay binding has pushed the enum back
            // into the settings object. Write the selected value explicitly so switching from
            // Dark to Follow Playnite immediately re-evaluates the host theme in this window.
            if (CurrentSettings != null && sender is ComboBox selector
                && selector.SelectedValue is GameSaveCenterThemeMode mode)
            {
                CurrentSettings.ThemeMode = mode;
            }
            QueueAdaptiveThemeUpdate();
        }

        private void OnSettingsTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HasSettingsSearch)
            {
                ApplySettingsSearch();
                return;
            }
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
            QueueValidationSummaryUpdate();
            QueueAdaptiveThemeUpdate();
        }

        private void OnGlassStrengthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            QueueValidationSummaryUpdate();
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

            GscMotion.AnimateEntrance(SettingsShell, 12);
        }

        private void ApplyAdaptiveTheme()
        {
            // High contrast is an accessibility mode, not merely a visual preference: all
            // translucent material and its backing blur must take the opaque fallback path.
            var glassEnabled = (CurrentSettings?.EnableGlassEffects ?? true) && !SystemParameters.HighContrast;
            var strength = CurrentSettings?.GlassEffectStrength ?? 78;
            var palette = AdaptiveThemePaletteFactory.Create(this, glassEnabled, strength, CurrentSettings?.ThemeMode ?? GameSaveCenterThemeMode.FollowPlaynite);

            // Apply exactly the same complete semantic palette as the production shell.
            // Settings then overrides only its structural surfaces below, because it has no
            // selected-game artwork behind it. This prevents a newly added shell token from
            // silently remaining on the static fallback in the settings window.
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(Resources, palette, glassEnabled, MotionEnabled);
            // Keep the same host-neutral baseline used by DashboardView. The settings
            // material below only changes structural surfaces; it must not leave core
            // semantic brushes inherited from a Playnite host dictionary.
            AdaptiveThemePaletteFactory.ApplyDemoCoreResources(Resources, palette.IsDark, palette.IsHighContrast);
            AdaptiveThemePaletteFactory.ApplySettingsMaterialResources(Resources, palette, glassEnabled);

            // Keep the fixed background ambient layer out of the render tree when glass is
            // disabled. Visibility is preferable to retaining a decorative visual at opacity 0.
            SettingsAmbientLayer.Visibility = glassEnabled ? Visibility.Visible : Visibility.Collapsed;
            SettingsAmbientLayer.Opacity = glassEnabled
                ? (palette.IsDark ? 0.82 : 0.64) * Math.Max(0.2, Math.Min(1, strength / 100.0))
                : 0;

            // ToolTip is hosted in a detached WPF window and cannot reliably resolve the page's
            // DynamicResource chain after a theme switch. Keep already-open Popup/ToolTip
            // instances on the same scoped palette without mutating Application resources.
            GscToolTipBehavior.RefreshOpenTransientSurfaces(this);
            NormalizeMotionIfDisabled();
        }

        private void NormalizeMotionIfDisabled()
        {
            if (MotionEnabled)
                return;

            GscMotion.NormalizeAll();
            SettingsShell.BeginAnimation(UIElement.OpacityProperty, null);
            if (SettingsShell.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                translate.Y = 0;
            }
            SettingsShell.Opacity = 1;
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
                || SettingsHeaderIcon == null || SettingsHeader == null || SettingsHeaderEyebrow == null) return;

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
            // The hero subtitle and the category card descriptions already explain the
            // settings scope. Keep the old full-width intro out of the first viewport so
            // the first editable fields and the one-line Playnite save state arrive sooner.
            SettingsIntroDescription.Visibility = Visibility.Collapsed;
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
            SettingsHeaderEyebrow.Visibility = shortHeight ? Visibility.Collapsed : Visibility.Visible;
            SettingsHeaderSubtitle.MaxWidth = narrow ? 300 : double.PositiveInfinity;
            SettingsSaveHint.Visibility = Visibility.Visible;
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
