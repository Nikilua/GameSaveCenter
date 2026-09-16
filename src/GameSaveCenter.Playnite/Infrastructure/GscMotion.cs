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
        private static readonly Lazy<ResourceDictionary> CanonicalTokens =
            new Lazy<ResourceDictionary>(LoadCanonicalTokens);

        // XAML owns the timing values. The explicit fallbacks only cover a host that cannot
        // load a pack URI (for example an isolated unit test or an early shutdown path); they
        // intentionally match the same semantic values instead of introducing a second table.
        internal static TimeSpan Fast => GetDuration(null, MotionDurationKind.Fast);
        internal static TimeSpan Press => GetDuration(null, MotionDurationKind.Press);
        internal static TimeSpan Normal => GetDuration(null, MotionDurationKind.Normal);
        internal static TimeSpan Slow => GetDuration(null, MotionDurationKind.Slow);

        internal enum MotionDurationKind
        {
            Fast,
            Press,
            Normal,
            Slow
        }

        internal static TimeSpan GetDuration(FrameworkElement? resourceHost, MotionDurationKind kind)
        {
            var key = kind switch
            {
                MotionDurationKind.Fast => "GscMotionFast",
                MotionDurationKind.Press => "GscMotionPress",
                MotionDurationKind.Normal => "GscMotionNormal",
                MotionDurationKind.Slow => "GscMotionSlow",
                _ => "GscMotionNormal"
            };
            var fallback = kind switch
            {
                MotionDurationKind.Fast => TimeSpan.FromMilliseconds(120),
                MotionDurationKind.Press => TimeSpan.FromMilliseconds(100),
                MotionDurationKind.Normal => TimeSpan.FromMilliseconds(220),
                MotionDurationKind.Slow => TimeSpan.FromMilliseconds(300),
                _ => TimeSpan.FromMilliseconds(220)
            };

            try
            {
                var value = resourceHost?.TryFindResource(key);
                if (value is Duration hostDuration && hostDuration.HasTimeSpan)
                    return hostDuration.TimeSpan;

                var canonicalValue = CanonicalTokens.Value[key];
                if (canonicalValue is Duration canonicalDuration && canonicalDuration.HasTimeSpan)
                    return canonicalDuration.TimeSpan;
            }
            catch
            {
                // A missing resource host must not make a notification or shutdown path fail.
            }

            return fallback;
        }

        private static ResourceDictionary LoadCanonicalTokens()
        {
            var dictionary = new ResourceDictionary
            {
                Source = new Uri("/GameSaveCenter.Playnite;component/Themes/MotionTokens.xaml", UriKind.Relative)
            };
            return dictionary;
        }

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

            var group = element.RenderTransform as TransformGroup;
            if (group != null)
            {
                if (group.IsFrozen)
                {
                    group = (TransformGroup)group.CloneCurrentValue();
                    element.RenderTransform = group;
                }

                var existing = FindMutableScaleTransform(group);
                if (existing != null)
                    return existing;

                var appended = new ScaleTransform(1, 1);
                group.Children.Add(appended);
                return appended;
            }

            var mutable = new ScaleTransform(1, 1);
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

        private static ScaleTransform? FindMutableScaleTransform(TransformGroup group)
        {
            for (var index = 0; index < group.Children.Count; index++)
            {
                var child = group.Children[index];
                if (child is ScaleTransform scale)
                {
                    if (scale.IsFrozen)
                    {
                        scale = (ScaleTransform)scale.CloneCurrentValue();
                        group.Children[index] = scale;
                    }
                    return scale;
                }

                if (child is TransformGroup nested)
                {
                    if (nested.IsFrozen)
                    {
                        nested = (TransformGroup)nested.CloneCurrentValue();
                        group.Children[index] = nested;
                    }
                    var found = FindMutableScaleTransform(nested);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        internal static void AnimateTranslate(FrameworkElement element, double x, double y, TimeSpan duration)
        {
            var translate = GetMutableTranslateTransform(element);
            var currentX = translate.X;
            var currentY = translate.Y;
            translate.BeginAnimation(TranslateTransform.XProperty, null);
            translate.BeginAnimation(TranslateTransform.YProperty, null);
            translate.X = currentX;
            translate.Y = currentY;
            var easing = CreateEaseOut();
            var xAnimation = new DoubleAnimation(currentX, x, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var yAnimation = new DoubleAnimation(currentY, y, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            xAnimation.Completed += (_, __) =>
            {
                translate.BeginAnimation(TranslateTransform.XProperty, null);
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                translate.X = x;
                translate.Y = y;
            };
            translate.BeginAnimation(TranslateTransform.XProperty, xAnimation);
            translate.BeginAnimation(TranslateTransform.YProperty, yAnimation);
        }

        internal static void AnimateTranslate(FrameworkElement element, double x, double y, MotionDurationKind kind)
            => AnimateTranslate(element, x, y, GetDuration(element, kind));

        internal static void AnimateEntrance(FrameworkElement element, double offsetY)
        {
            var translate = GetMutableTranslateTransform(element);
            var translateSource = DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty);
            var opacitySource = DependencyPropertyHelper.GetValueSource(element, UIElement.OpacityProperty);
            var hasActiveAnimation = translateSource.IsAnimated || opacitySource.IsAnimated;
            var currentY = translate.Y;
            var currentOpacity = element.Opacity;

            if (hasActiveAnimation)
            {
                // Capture the effective values before removing the old clocks. This prevents
                // a rapid re-entry from flashing back to the original entrance offset.
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                element.BeginAnimation(UIElement.OpacityProperty, null);
                translate.Y = currentY;
                element.Opacity = currentOpacity;
            }
            else
            {
                currentY = offsetY;
                currentOpacity = 0;
                translate.Y = currentY;
                element.Opacity = currentOpacity;
            }

            var easing = CreateEaseOut();
            var opacityAnimation = new DoubleAnimation(currentOpacity, 1, GetDuration(element, MotionDurationKind.Normal))
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var translateAnimation = new DoubleAnimation(currentY, 0, GetDuration(element, MotionDurationKind.Slow))
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            translateAnimation.Completed += (_, __) =>
            {
                element.BeginAnimation(UIElement.OpacityProperty, null);
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                element.Opacity = 1;
                translate.Y = 0;
            };
            element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            translate.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        }
    }
}
