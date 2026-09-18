using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Transfers a vertical wheel step from a nested finite scroll surface to its nearest
    /// outer ScrollViewer only when the nested surface has reached that direction's edge.
    /// The attached behavior never changes scrollbar visibility or content scrolling mode.
    /// </summary>
    public static class ScrollBoundaryRoutingBehavior
    {
        private static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(ScrollBoundaryRoutingBehavior),
            new PropertyMetadata(false));

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(ScrollBoundaryRoutingBehavior),
            new PropertyMetadata(false, OnEnabledChanged));

        public static void SetEnabled(DependencyObject element, bool value)
            => element.SetValue(EnabledProperty, value);

        public static bool GetEnabled(DependencyObject element)
            => (bool)element.GetValue(EnabledProperty);

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is UIElement element)) return;

            if ((bool)args.NewValue)
            {
                if ((bool)element.GetValue(IsAttachedProperty)) return;
                element.SetValue(IsAttachedProperty, true);
                element.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel), true);
                return;
            }

            if (!(bool)element.GetValue(IsAttachedProperty)) return;
            element.RemoveHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(OnPreviewMouseWheel));
            element.ClearValue(IsAttachedProperty);
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (args.Handled || args.Delta == 0) return;

            var nested = ResolveNestedScroller(sender, args.OriginalSource as DependencyObject);
            if (nested == null || CanScroll(nested, args.Delta)) return;

            var outer = FindParentScrollViewer(nested);
            if (outer == null || !CanScroll(outer, args.Delta))
            {
                // At the final reachable boundary there is no scroll target left. Consume
                // the no-op so WPF does not schedule a layout pass for every repeated wheel
                // event, while still leaving events with a movable outer surface untouched.
                args.Handled = true;
                return;
            }

            ScrollByWheelDelta(outer, args.Delta);
            args.Handled = true;
        }

        private static ScrollViewer? ResolveNestedScroller(object sender, DependencyObject? originalSource)
        {
            var nearest = FindNearestScrollViewer(originalSource);
            if (sender is ScrollViewer scrollViewer)
                return ReferenceEquals(nearest, scrollViewer) ? scrollViewer : null;

            if (!(sender is DataGrid grid)) return null;
            if (nearest != null && IsWithin(nearest, grid)) return nearest;
            return FindDescendantScrollViewer(grid);
        }

        private static bool CanScroll(ScrollViewer viewer, int delta)
        {
            if (viewer == null || viewer.ScrollableHeight <= 0.5 || delta == 0) return false;
            return delta > 0
                ? viewer.VerticalOffset > 0.5
                : viewer.VerticalOffset < viewer.ScrollableHeight - 0.5;
        }

        private static void ScrollByWheelDelta(ScrollViewer viewer, int delta)
        {
            var lines = Math.Max(1, Math.Abs(delta) / 120);
            for (var index = 0; index < lines; index++)
            {
                if (delta > 0) viewer.LineUp();
                else viewer.LineDown();
            }
        }

        private static ScrollViewer? FindParentScrollViewer(DependencyObject source)
        {
            var current = GetParent(source);
            while (current != null)
            {
                if (current is ScrollViewer viewer) return viewer;
                current = GetParent(current);
            }

            return null;
        }

        private static ScrollViewer? FindNearestScrollViewer(DependencyObject? source)
        {
            var current = source;
            while (current != null)
            {
                if (current is ScrollViewer viewer) return viewer;
                current = GetParent(current);
            }

            return null;
        }

        private static ScrollViewer? FindDescendantScrollViewer(DependencyObject source)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(source); index++)
            {
                var child = VisualTreeHelper.GetChild(source, index);
                if (child is ScrollViewer viewer) return viewer;
                var nested = FindDescendantScrollViewer(child);
                if (nested != null) return nested;
            }

            return null;
        }

        private static bool IsWithin(DependencyObject source, DependencyObject ancestor)
        {
            var current = source;
            while (current != null)
            {
                if (ReferenceEquals(current, ancestor)) return true;
                current = GetParent(current);
            }

            return false;
        }

        private static DependencyObject? GetParent(DependencyObject source)
        {
            if (source == null) return null;
            var visualParent = source is Visual || source is Visual3D
                ? VisualTreeHelper.GetParent(source)
                : null;
            return visualParent ?? LogicalTreeHelper.GetParent(source);
        }
    }
}
