using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Clips a Border and its child to the Border's live rounded outline. Border's own
    /// background is rounded, but ClipToBounds alone only clips children to a rectangle.
    /// </summary>
    public static class RoundedClipBehavior
    {
        public static readonly DependencyProperty ClipToRoundedCornersProperty = DependencyProperty.RegisterAttached(
            "ClipToRoundedCorners",
            typeof(bool),
            typeof(RoundedClipBehavior),
            new FrameworkPropertyMetadata(false, OnClipToRoundedCornersChanged));

        private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State",
            typeof(ClipState),
            typeof(RoundedClipBehavior),
            new PropertyMetadata(null));

        private static readonly DependencyPropertyDescriptor? CornerRadiusDescriptor =
            DependencyPropertyDescriptor.FromProperty(Border.CornerRadiusProperty, typeof(Border));

        public static bool GetClipToRoundedCorners(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (bool)element.GetValue(ClipToRoundedCornersProperty);
        }

        public static void SetClipToRoundedCorners(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(ClipToRoundedCornersProperty, value);
        }

        private static void OnClipToRoundedCornersChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (!(target is Border border))
                throw new InvalidOperationException("Rounded clipping can only be attached to a Border.");

            if ((bool)args.NewValue)
            {
                if (border.GetValue(StateProperty) is ClipState)
                    return;

                var state = new ClipState(border.Clip);
                border.SetValue(StateProperty, state);
                border.SizeChanged += OnSizeChanged;
                border.Loaded += OnLoaded;
                if (CornerRadiusDescriptor != null)
                    CornerRadiusDescriptor.AddValueChanged(border, OnCornerRadiusChanged);
                UpdateClip(border, state);
                return;
            }

            if (!(border.GetValue(StateProperty) is ClipState existing))
                return;

            border.SizeChanged -= OnSizeChanged;
            border.Loaded -= OnLoaded;
            if (CornerRadiusDescriptor != null)
                CornerRadiusDescriptor.RemoveValueChanged(border, OnCornerRadiusChanged);
            if (ReferenceEquals(border.Clip, existing.LastClip))
                border.Clip = existing.OriginalClip;
            border.ClearValue(StateProperty);
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is Border border && border.GetValue(StateProperty) is ClipState state)
                UpdateClip(border, state);
        }

        private static void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is Border border && border.GetValue(StateProperty) is ClipState state)
                UpdateClip(border, state);
        }

        private static void OnCornerRadiusChanged(object? sender, EventArgs args)
        {
            if (sender is Border border && border.GetValue(StateProperty) is ClipState state)
                UpdateClip(border, state);
        }

        private static void UpdateClip(Border border, ClipState state)
        {
            // Keep a caller-provided clip by intersecting it with the rounded surface. If a
            // consumer replaces Clip while the behavior is active, preserve that new clip too.
            if (state.LastClip != null && !ReferenceEquals(border.Clip, state.LastClip))
                state.OriginalClip = border.Clip;

            var rounded = CreateRoundedGeometry(border.ActualWidth, border.ActualHeight, border.CornerRadius);
            var clip = state.OriginalClip == null
                ? rounded
                : new CombinedGeometry(GeometryCombineMode.Intersect, state.OriginalClip, rounded);
            border.Clip = clip;
            state.LastClip = clip;
        }

        private static Geometry CreateRoundedGeometry(double width, double height, CornerRadius corners)
        {
            var geometry = new StreamGeometry();
            if (width <= 0 || height <= 0)
                return geometry;

            var topLeft = Math.Max(0, corners.TopLeft);
            var topRight = Math.Max(0, corners.TopRight);
            var bottomRight = Math.Max(0, corners.BottomRight);
            var bottomLeft = Math.Max(0, corners.BottomLeft);
            var scale = 1d;
            scale = LimitScale(scale, width, topLeft + topRight);
            scale = LimitScale(scale, width, bottomLeft + bottomRight);
            scale = LimitScale(scale, height, topLeft + bottomLeft);
            scale = LimitScale(scale, height, topRight + bottomRight);
            topLeft *= scale;
            topRight *= scale;
            bottomRight *= scale;
            bottomLeft *= scale;

            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(topLeft, 0), true, true);
                context.LineTo(new Point(width - topRight, 0), true, false);
                AddArc(context, new Point(width, topRight), topRight);
                context.LineTo(new Point(width, height - bottomRight), true, false);
                AddArc(context, new Point(width - bottomRight, height), bottomRight);
                context.LineTo(new Point(bottomLeft, height), true, false);
                AddArc(context, new Point(0, height - bottomLeft), bottomLeft);
                context.LineTo(new Point(0, topLeft), true, false);
                AddArc(context, new Point(topLeft, 0), topLeft);
            }

            if (geometry.CanFreeze)
                geometry.Freeze();
            return geometry;
        }

        private static double LimitScale(double current, double extent, double sum)
        {
            return sum > extent && sum > 0 ? Math.Min(current, extent / sum) : current;
        }

        private static void AddArc(StreamGeometryContext context, Point end, double radius)
        {
            if (radius <= 0)
            {
                context.LineTo(end, true, false);
                return;
            }

            context.ArcTo(end, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
        }

        private sealed class ClipState
        {
            public ClipState(Geometry? originalClip)
            {
                OriginalClip = originalClip;
            }

            public Geometry? OriginalClip { get; set; }
            public Geometry? LastClip { get; set; }
        }
    }
}
