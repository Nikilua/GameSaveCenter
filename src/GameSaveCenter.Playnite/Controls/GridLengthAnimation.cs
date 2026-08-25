using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Animates an absolute GridLength so a Grid column can move with the shell instead of
    /// jumping between the expanded and compact sidebar widths.
    /// </summary>
    public sealed class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            nameof(From), typeof(GridLength), typeof(GridLengthAnimation),
            new PropertyMetadata(new GridLength(0, GridUnitType.Pixel)));

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            nameof(To), typeof(GridLength), typeof(GridLengthAnimation),
            new PropertyMetadata(new GridLength(0, GridUnitType.Pixel)));

        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
            nameof(EasingFunction), typeof(EasingFunctionBase), typeof(GridLengthAnimation),
            new PropertyMetadata(null));

        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public EasingFunctionBase? EasingFunction
        {
            get => (EasingFunctionBase?)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        public override object GetCurrentValue(
            object defaultOriginValue,
            object defaultDestinationValue,
            AnimationClock animationClock)
        {
            var from = From.IsAbsolute
                ? From.Value
                : defaultOriginValue is GridLength origin && origin.IsAbsolute ? origin.Value : 0;
            var to = To.IsAbsolute
                ? To.Value
                : defaultDestinationValue is GridLength destination && destination.IsAbsolute ? destination.Value : from;
            var progress = animationClock.CurrentProgress ?? 1;
            if (EasingFunction != null)
                progress = EasingFunction.Ease(progress);
            return new GridLength(from + ((to - from) * progress), GridUnitType.Pixel);
        }
    }
}
