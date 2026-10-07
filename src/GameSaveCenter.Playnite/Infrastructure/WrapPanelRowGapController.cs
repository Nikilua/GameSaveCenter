using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Adds vertical breathing room when a horizontal action WrapPanel actually wraps.
    /// Original margins are restored when the arranged children fit on one row.
    /// </summary>
    public sealed class WrapPanelRowGapController
    {
        private readonly WrapPanel panel;
        private readonly Dictionary<FrameworkElement, Thickness> originalMargins = new Dictionary<FrameworkElement, Thickness>();
        private double requestedRowGap;
        private int childCount = -1;
        private bool listeningForLayout;

        public WrapPanelRowGapController(WrapPanel panel)
        {
            this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
            panel.Loaded += OnLoaded;
            panel.Unloaded += OnUnloaded;
            // SizeChanged is instance-owned and also supports manually arranged panels.
            panel.SizeChanged += OnSizeChanged;
            if (panel.IsLoaded)
                ListenForLayout();
        }

        public void SetRowGap(double rowGap)
        {
            requestedRowGap = rowGap > 0 ? rowGap : 0;
            RefreshMargins();
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            ListenForLayout();
            RefreshMargins();
        }

        private void ListenForLayout()
        {
            if (listeningForLayout)
                return;
            panel.LayoutUpdated += OnLayoutUpdated;
            listeningForLayout = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            // WPF LayoutUpdated observes dispatcher-wide layout. Do not retain an
            // unloaded workspace through that subscription; reload attaches once.
            panel.LayoutUpdated -= OnLayoutUpdated;
            listeningForLayout = false;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args) => RefreshMargins();

        private void OnLayoutUpdated(object? sender, EventArgs args) => RefreshMargins();

        private void RefreshMargins()
        {
            var membershipChanged = childCount != panel.Children.Count;
            foreach (UIElement child in panel.Children)
            {
                if (child is FrameworkElement element && !originalMargins.ContainsKey(element))
                {
                    originalMargins.Add(element, element.Margin);
                    membershipChanged = true;
                }
            }
            if (membershipChanged)
            {
                foreach (var removed in originalMargins.Keys.Where(element => !panel.Children.Contains(element)).ToArray())
                    originalMargins.Remove(removed);
                childCount = panel.Children.Count;
            }

            // Read slots only after Arrange. Updating a margin invalidates measurement;
            // the following layout pass settles it without recursive UpdateLayout calls.
            if (requestedRowGap > 0 && !panel.IsArrangeValid)
                return;
            var enabled = requestedRowGap > 0 && HasMultipleRows();
            foreach (UIElement child in panel.Children)
            {
                if (!(child is FrameworkElement element))
                    continue;

                var original = originalMargins[element];
                var next = enabled
                    ? new Thickness(original.Left, original.Top, original.Right, Math.Max(original.Bottom, requestedRowGap))
                    : original;
                if (element.Margin != next)
                    element.Margin = next;
            }
        }

        private bool HasMultipleRows()
        {
            if (panel.Orientation != Orientation.Horizontal)
                return false;
            double? firstRowTop = null;
            foreach (UIElement child in panel.Children)
            {
                if (!(child is FrameworkElement element) || element.Visibility == Visibility.Collapsed)
                    continue;
                // Slots share a row top even when short labels are vertically centered
                // next to taller buttons. Render bounds would misclassify that row.
                var slot = LayoutInformation.GetLayoutSlot(element);
                if (slot.IsEmpty || slot.Width <= 0 || slot.Height <= 0)
                    continue;
                if (firstRowTop.HasValue && Math.Abs(slot.Top - firstRowTop.Value) > 0.5)
                    return true;
                firstRowTop = slot.Top;
            }
            return false;
        }
    }
}
