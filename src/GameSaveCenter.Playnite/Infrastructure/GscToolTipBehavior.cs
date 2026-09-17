using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps a production tooltip dismissible with Escape without moving keyboard focus
    /// into the transient popup. The scope is attached to the plugin surface so other
    /// Playnite extensions do not receive this behavior.
    /// </summary>
    public static class GscToolTipBehavior
    {
        public static readonly DependencyProperty EnableEscapeCloseProperty =
            DependencyProperty.RegisterAttached(
                "EnableEscapeClose",
                typeof(bool),
                typeof(GscToolTipBehavior),
                new PropertyMetadata(false, OnEnableEscapeCloseChanged));

        public static bool GetEnableEscapeClose(DependencyObject element)
            => (bool)element.GetValue(EnableEscapeCloseProperty);

        public static void SetEnableEscapeClose(DependencyObject element, bool value)
            => element.SetValue(EnableEscapeCloseProperty, value);

        private static void OnEnableEscapeCloseChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            if (!(dependencyObject is UIElement element))
                return;

            if ((bool)args.NewValue)
                element.PreviewKeyDown += OnPreviewKeyDown;
            else
                element.PreviewKeyDown -= OnPreviewKeyDown;
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key != Key.Escape || args.Handled || !(sender is DependencyObject scope))
                return;

            if (!CloseOpenTooltips(scope))
                return;

            args.Handled = true;
        }

        private static bool CloseOpenTooltips(DependencyObject scope)
        {
            var openTooltips = new List<ToolTip>();
            foreach (PresentationSource source in PresentationSource.CurrentSources)
            {
                if (source.RootVisual is DependencyObject root)
                    CollectOpenTooltips(root, openTooltips);
            }

            var closed = false;
            foreach (var tooltip in openTooltips)
            {
                if (tooltip.PlacementTarget is UIElement target && IsWithinScope(target, scope))
                {
                    tooltip.IsOpen = false;
                    closed = true;
                }
            }

            return closed;
        }

        private static void CollectOpenTooltips(DependencyObject root, ICollection<ToolTip> results)
        {
            if (root is ToolTip tooltip && tooltip.IsOpen)
                results.Add(tooltip);

            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                CollectOpenTooltips(VisualTreeHelper.GetChild(root, index), results);
        }

        private static bool IsWithinScope(DependencyObject element, DependencyObject scope)
        {
            var visited = new HashSet<DependencyObject>();
            var current = element;
            while (current != null && visited.Add(current))
            {
                if (ReferenceEquals(current, scope))
                    return true;

                var visualParent = current is Visual || current is Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : null;
                current = visualParent ?? LogicalTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
