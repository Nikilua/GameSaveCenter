using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Describes the existing responsive decisions without touching WPF controls.
    /// Keeping the thresholds here makes Dashboard and the extracted production shell
    /// consume the same width/height state while preserving the established values.
    /// </summary>
    public readonly struct ResponsiveLayoutState
    {
        public ResponsiveLayoutState(double width, double height)
        {
            Width = width;
            Height = height;
            Mode = width >= 1280 ? LayoutMode.Expanded
                : width >= 1040 ? LayoutMode.Standard
                : width >= 960 ? LayoutMode.Compact
                : LayoutMode.Narrow;
            IsIconSidebar = Mode == LayoutMode.Compact || Mode == LayoutMode.Narrow;
            SidebarWidth = Mode == LayoutMode.Expanded || Mode == LayoutMode.Standard
                ? 228
                : Mode == LayoutMode.Compact ? 78 : 72;
            SidebarGutterWidth = IsIconSidebar ? 10 : 0;
            ToastTopMargin = height < 760 ? 66 : 78;
            ToastRightMargin = width < 1080 ? 12 : 22;
            IsToolbarLabelsVisible = Mode == LayoutMode.Expanded;
            SupportsTopBarPicker = Mode == LayoutMode.Expanded || Mode == LayoutMode.Standard;
            PickerWidth = Mode == LayoutMode.Expanded ? 380 : 330;
            IsCompactGameBrowser = Mode == LayoutMode.Narrow;
            TableMinHeight = height < 650 ? 0 : height < 760 ? 96 : 140;
            WorkspaceTableMinHeight = height < 650 ? 0 : height < 760 ? 112 : 160;
            TableViewportHeight = System.Math.Max(520d, System.Math.Min(820d, height * (height < 700 ? 0.94 : 0.95)));
            WorkspaceTopGap = Mode == LayoutMode.Expanded ? 12 : Mode == LayoutMode.Standard ? 10 : 8;
            IsComfortableHeight = height >= 760;
            IsShortFooter = height < 700;
            IsFooterHintVisible = width >= 900;
            IsCompactShellHeader = width < 1280;
            IsVeryCompactShellHeader = width < 720;
            ShellPickerWidth = IsCompactShellHeader
                ? (IsVeryCompactShellHeader ? 190 : 220)
                : 300;
            OverviewUsesStackedColumns = width < 1200;
        }

        public double Width { get; }
        public double Height { get; }
        public LayoutMode Mode { get; }
        public bool IsIconSidebar { get; }
        public double SidebarWidth { get; }
        public double SidebarGutterWidth { get; }
        public double ToastTopMargin { get; }
        public double ToastRightMargin { get; }
        public bool IsToolbarLabelsVisible { get; }
        public bool SupportsTopBarPicker { get; }
        public double PickerWidth { get; }
        public bool IsCompactGameBrowser { get; }
        public double TableMinHeight { get; }
        public double WorkspaceTableMinHeight { get; }
        public double TableViewportHeight { get; }
        public double WorkspaceTopGap { get; }
        public bool IsComfortableHeight { get; }
        public bool IsShortFooter { get; }
        public bool IsFooterHintVisible { get; }
        public bool IsCompactShellHeader { get; }
        public bool IsVeryCompactShellHeader { get; }
        public double ShellPickerWidth { get; }
        public bool OverviewUsesStackedColumns { get; }

        public bool ShouldStackGameHeader(double workspaceContentWidth)
            => workspaceContentWidth < 1180;
    }

    public static class ResponsiveLayoutCoordinator
    {
        // The detail inspector needs a small content-budget hysteresis band. Without it,
        // a host that reports 979/980 DIP while the user drags its edge repeatedly
        // moves the same inspector between a column and a drawer on every render pass.
        public const double DetailCompactThreshold = 980d;
        public const double DetailCompactEnterThreshold = 972d;
        public const double DetailWideExitThreshold = 988d;

        public static ResponsiveLayoutState Calculate(double width, double height)
            => new ResponsiveLayoutState(width, height);
    }

    /// <summary>
    /// Holds a detail layout decision while a host is inside the small measurement
    /// band around the content-budget breakpoint. The visual tree and its ScrollViewer
    /// remain the same object when the decision changes.
    /// </summary>
    public sealed class ResponsiveDetailBreakpointLatch
    {
        private bool initialized;
        private bool isCompact;

        public bool IsInitialized => initialized;
        public bool IsCompact => isCompact;
        public int TransitionCount { get; private set; }

        public bool Evaluate(double width)
        {
            if (!initialized)
            {
                initialized = true;
                isCompact = width < ResponsiveLayoutCoordinator.DetailCompactThreshold;
                return isCompact;
            }

            var next = isCompact
                ? width < ResponsiveLayoutCoordinator.DetailWideExitThreshold
                : width < ResponsiveLayoutCoordinator.DetailCompactEnterThreshold;
            if (next != isCompact)
            {
                isCompact = next;
                TransitionCount++;
            }

            return isCompact;
        }
    }
}
