using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Adds horizontal movement to production DataGrids without replacing their ScrollViewer,
    /// scrollbar template, or virtualization settings. Shift+vertical-wheel is the WPF
    /// fallback; native horizontal-wheel messages cover mice and touchpads that emit them.
    /// </summary>
    public static class DataGridHorizontalWheelBehavior
    {
        private const int WmMouseHorizontalWheel = 0x020E;
        private const double WheelDetentDip = 48d;

        private static readonly DependencyProperty HookRegistrationProperty = DependencyProperty.RegisterAttached(
            "HookRegistration",
            typeof(HookRegistration),
            typeof(DataGridHorizontalWheelBehavior),
            new PropertyMetadata(null));

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(DataGridHorizontalWheelBehavior),
            new PropertyMetadata(false, OnEnabledChanged));

        public static void SetEnabled(DependencyObject element, bool value)
            => element.SetValue(EnabledProperty, value);

        public static bool GetEnabled(DependencyObject element)
            => (bool)element.GetValue(EnabledProperty);

        internal static bool HasHookRegistration(DataGrid grid)
            => grid.GetValue(HookRegistrationProperty) is HookRegistration;

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is DataGrid grid)) return;

            if ((bool)args.NewValue)
            {
                grid.Loaded += OnGridLoaded;
                grid.Unloaded += OnGridUnloaded;
                if (grid.IsLoaded) Attach(grid);
                return;
            }

            grid.Loaded -= OnGridLoaded;
            grid.Unloaded -= OnGridUnloaded;
            grid.PreviewMouseWheel -= OnPreviewMouseWheel;
            Detach(grid);
        }

        private static void OnGridLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is DataGrid grid) Attach(grid);
        }

        private static void OnGridUnloaded(object sender, RoutedEventArgs args)
        {
            if (!(sender is DataGrid grid)) return;
            grid.PreviewMouseWheel -= OnPreviewMouseWheel;
            Detach(grid);
        }

        private static void Attach(DataGrid grid)
        {
            grid.PreviewMouseWheel -= OnPreviewMouseWheel;
            grid.PreviewMouseWheel += OnPreviewMouseWheel;

            var source = PresentationSource.FromVisual(grid) as HwndSource;
            if (source == null) return;

            var current = (HookRegistration)grid.GetValue(HookRegistrationProperty);
            if (current != null && ReferenceEquals(current.Source, source)) return;

            Detach(grid);
            HwndSourceHook hook = (IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
                => OnWindowMessage(grid, hwnd, message, wParam, lParam, ref handled);
            source.AddHook(hook);
            grid.SetValue(HookRegistrationProperty, new HookRegistration(source, hook));
        }

        private static void Detach(DataGrid grid)
        {
            var registration = (HookRegistration)grid.GetValue(HookRegistrationProperty);
            if (registration == null) return;
            registration.Source.RemoveHook(registration.Hook);
            grid.ClearValue(HookRegistrationProperty);
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (!(sender is DataGrid grid)) return;
            if (TryHandleShiftWheel(grid, args.Delta, Keyboard.Modifiers))
                args.Handled = true;
        }

        // Shared with the STA behavior tests so modifier-specific input is exercised against
        // the actual DataGrid template and ScrollViewer rather than inferred from XAML text.
        internal static bool TryHandleShiftWheel(DataGrid grid, int delta, ModifierKeys modifiers)
        {
            if (delta == 0
                || (modifiers & ModifierKeys.Shift) == 0
                || (modifiers & ModifierKeys.Control) != 0)
                return false;

            var viewer = FindGridScrollViewer(grid);
            if (viewer == null || viewer.ScrollableWidth <= 0.5) return false;

            // A positive vertical wheel delta means content moves toward the top. For the
            // Shift fallback, keep that convention on the horizontal axis and expose columns
            // to the left for a positive delta.
            ApplyHorizontalDelta(viewer, -delta);
            return true;
        }

        internal static IntPtr OnWindowMessage(DataGrid grid, IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message != WmMouseHorizontalWheel || !IsPointerInsideGrid(grid, lParam))
                return IntPtr.Zero;

            var packed = unchecked((long)wParam.ToInt64());
            var delta = unchecked((short)((packed >> 16) & 0xFFFF));
            if (delta == 0) return IntPtr.Zero;

            var viewer = FindGridScrollViewer(grid);
            if (viewer == null || viewer.ScrollableWidth <= 0.5) return IntPtr.Zero;

            // WM_MOUSEHWHEEL's positive delta indicates motion to the right, so advance the
            // horizontal offset. Small high-resolution deltas remain fractional DIPs.
            ApplyHorizontalDelta(viewer, delta);
            handled = true;
            return new IntPtr(1);
        }

        private static bool IsPointerInsideGrid(DataGrid grid, IntPtr lParam)
        {
            if (!grid.IsLoaded || !grid.IsVisible || !grid.IsEnabled || grid.ActualWidth <= 0 || grid.ActualHeight <= 0)
                return false;

            var packed = unchecked((long)lParam.ToInt64());
            var screenX = unchecked((short)(packed & 0xFFFF));
            var screenY = unchecked((short)((packed >> 16) & 0xFFFF));
            try
            {
                var point = grid.PointFromScreen(new Point(screenX, screenY));
                return point.X >= 0 && point.Y >= 0 && point.X < grid.ActualWidth && point.Y < grid.ActualHeight;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static ScrollViewer? FindGridScrollViewer(DataGrid grid)
        {
            grid.ApplyTemplate();
            var named = grid.Template?.FindName("DG_ScrollViewer", grid) as ScrollViewer;
            if (named != null) return named;

            ScrollViewer? best = null;
            foreach (var candidate in FindVisualChildren<ScrollViewer>(grid))
            {
                if (best == null || candidate.ScrollableWidth > best.ScrollableWidth)
                    best = candidate;
            }
            return best;
        }

        private static void ApplyHorizontalDelta(ScrollViewer viewer, int delta)
        {
            var target = viewer.HorizontalOffset + (delta / 120d * WheelDetentDip);
            target = Math.Max(0, Math.Min(viewer.ScrollableWidth, target));
            if (Math.Abs(target - viewer.HorizontalOffset) > 0.001)
                viewer.ScrollToHorizontalOffset(target);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                var child = VisualTreeHelper.GetChild(root, index);
                if (child is T match) yield return match;
                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
        }

        private sealed class HookRegistration
        {
            public HookRegistration(HwndSource source, HwndSourceHook hook)
            {
                Source = source;
                Hook = hook;
            }

            public HwndSource Source { get; }
            public HwndSourceHook Hook { get; }
        }
    }
}
