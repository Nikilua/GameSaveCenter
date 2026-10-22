using System;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>Arranges a command, its pin action and its independent readable reason.</summary>
    public sealed class CommandDirectoryResultPanel : Panel
    {
        private bool stackPin;
        private double pinWidth;
        private double actionHeight;
        private double pinHeight;
        private double reasonHeight;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (InternalChildren.Count != 3)
                throw new InvalidOperationException("Command results require an action, a pin action and a reason.");

            var action = InternalChildren[0];
            var pin = InternalChildren[1];
            var reason = InternalChildren[2];
            var width = availableSize.Width;
            pin.Measure(new Size(width, double.PositiveInfinity));
            pinWidth = pin.DesiredSize.Width;
            var control = action as Control;
            // Reserve a short readable label plus its icon/padding. Scale the
            // threshold with typography instead of assuming a fixed window width.
            var minimumActionWidth = (control?.FontSize ?? 14d) * 8d
                + (control?.Padding.Left ?? 0d) + (control?.Padding.Right ?? 0d);
            stackPin = !double.IsInfinity(width) && width < minimumActionWidth + pinWidth;
            var actionWidth = stackPin ? width : Math.Max(0, width - pinWidth);
            action.Measure(new Size(actionWidth, double.PositiveInfinity));
            actionHeight = action.DesiredSize.Height;
            pinHeight = pin.DesiredSize.Height;
            var desiredWidth = double.IsInfinity(width)
                ? action.DesiredSize.Width + pinWidth
                : width;
            reason.Measure(new Size(desiredWidth, double.PositiveInfinity));
            reasonHeight = reason.DesiredSize.Height;
            return new Size(desiredWidth,
                (stackPin ? actionHeight + pinHeight : Math.Max(actionHeight, pinHeight)) + reasonHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var actionWidth = stackPin ? finalSize.Width : Math.Max(0, finalSize.Width - pinWidth);
            InternalChildren[0].Arrange(new Rect(0, 0, actionWidth, actionHeight));
            InternalChildren[1].Arrange(new Rect(Math.Max(0, finalSize.Width - pinWidth),
                stackPin ? actionHeight : 0, Math.Min(pinWidth, finalSize.Width), pinHeight));
            var reasonTop = stackPin ? actionHeight + pinHeight : Math.Max(actionHeight, pinHeight);
            InternalChildren[2].Arrange(new Rect(0, reasonTop, finalSize.Width, reasonHeight));
            return finalSize;
        }
    }
}
