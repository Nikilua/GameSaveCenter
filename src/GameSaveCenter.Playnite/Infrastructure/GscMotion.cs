using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Small, render-only motion primitives for the Playnite host.  Transforms are always
    /// made per visual before being animated, so a Freezable supplied by a Style can never
    /// be mutated or shared with another control instance.
    /// </summary>
    internal static class GscMotion
    {
        internal static readonly TimeSpan Fast = TimeSpan.FromMilliseconds(120);
        internal static readonly TimeSpan Normal = TimeSpan.FromMilliseconds(200);
        internal static readonly TimeSpan Slow = TimeSpan.FromMilliseconds(320);

        internal static bool IsEnabled(bool requested)
            => requested && !SystemParameters.HighContrast && SystemParameters.ClientAreaAnimation;

        internal static EasingFunctionBase CreateEaseOut()
            => new CubicEase { EasingMode = EasingMode.EaseOut };

        internal static TranslateTransform GetMutableTranslateTransform(FrameworkElement element)
        {
            if (element.RenderTransform is TranslateTransform translate)
            {
                if (!translate.IsFrozen) return translate;
                translate = (TranslateTransform)translate.CloneCurrentValue();
                element.RenderTransform = translate;
                return translate;
            }

            var group = element.RenderTransform as TransformGroup;
            if (group != null)
            {
                if (group.IsFrozen)
                {
                    group = (TransformGroup)group.CloneCurrentValue();
                    element.RenderTransform = group;
                }
                foreach (var child in group.Children)
                {
                    if (child is TranslateTransform childTranslate && !childTranslate.IsFrozen)
                        return childTranslate;
                }
                var appended = new TranslateTransform();
                group.Children.Add(appended);
                return appended;
            }

            var mutable = new TranslateTransform();
            if (element.RenderTransform != null && element.RenderTransform != Transform.Identity)
            {
                var replacement = new TransformGroup();
                replacement.Children.Add(element.RenderTransform.IsFrozen
                    ? element.RenderTransform.CloneCurrentValue()
                    : element.RenderTransform);
                replacement.Children.Add(mutable);
                element.RenderTransform = replacement;
            }
            else element.RenderTransform = mutable;
            return mutable;
        }

        internal static ScaleTransform GetMutableScaleTransform(FrameworkElement element)
        {
            if (element.RenderTransform is ScaleTransform scale)
            {
                if (!scale.IsFrozen) return scale;
                scale = (ScaleTransform)scale.CloneCurrentValue();
                element.RenderTransform = scale;
                return scale;
            }

            var mutable = new ScaleTransform(1, 1);
            if (element.RenderTransform != null && element.RenderTransform != Transform.Identity)
            {
                var group = new TransformGroup();
                group.Children.Add(element.RenderTransform.IsFrozen
                    ? element.RenderTransform.CloneCurrentValue()
                    : element.RenderTransform);
                group.Children.Add(mutable);
                element.RenderTransform = group;
            }
            else element.RenderTransform = mutable;
            return mutable;
        }

        internal static void AnimateTranslate(FrameworkElement element, double x, double y, TimeSpan duration)
        {
            var translate = GetMutableTranslateTransform(element);
            var easing = CreateEaseOut();
            translate.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(x, duration) { EasingFunction = easing });
            translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(y, duration) { EasingFunction = easing });
        }

        internal static void AnimateEntrance(FrameworkElement element, double offsetY)
        {
            var translate = GetMutableTranslateTransform(element);
            translate.Y = offsetY;
            element.Opacity = 0;
            var easing = CreateEaseOut();
            element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, Normal) { EasingFunction = easing });
            translate.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(offsetY, 0, Slow) { EasingFunction = easing });
        }
    }
}
