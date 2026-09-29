using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Adds vertical breathing room only when a responsive action WrapPanel is in its
    /// compact layout. Original margins are retained so returning to the wide layout
    /// restores each control's authored spacing exactly.
    /// </summary>
    public sealed class WrapPanelRowGapController
    {
        private readonly WrapPanel panel;
        private readonly Dictionary<FrameworkElement, Thickness> originalMargins = new Dictionary<FrameworkElement, Thickness>();

        public WrapPanelRowGapController(WrapPanel panel)
        {
            this.panel = panel ?? throw new System.ArgumentNullException(nameof(panel));
        }

        public void SetRowGap(double rowGap)
        {
            var enabled = rowGap > 0;
            foreach (UIElement child in panel.Children)
            {
                if (!(child is FrameworkElement element))
                    continue;

                if (!originalMargins.TryGetValue(element, out var original))
                {
                    original = element.Margin;
                    originalMargins.Add(element, original);
                }

                var next = enabled
                    ? new Thickness(original.Left, original.Top, original.Right, System.Math.Max(original.Bottom, rowGap))
                    : original;
                if (element.Margin != next)
                    element.Margin = next;
            }
        }
    }
}
