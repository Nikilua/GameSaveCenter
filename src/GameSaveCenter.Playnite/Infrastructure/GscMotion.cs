using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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
        private static readonly ConditionalWeakTable<FrameworkElement, MotionState> MotionStates =
            new ConditionalWeakTable<FrameworkElement, MotionState>();
        private static readonly ConditionalWeakTable<Dispatcher, MotionRegistry> MotionRegistries =
            new ConditionalWeakTable<Dispatcher, MotionRegistry>();

        private sealed class MotionState
        {
            public int TranslateGeneration;
            public int EntranceGeneration;
            public int ScaleGeneration;
            public bool HasScaleBase;
            public double ScaleBaseX;
            public double ScaleBaseY;
        }

        private sealed class MotionRegistry
        {
            public readonly object Gate = new object();
            public readonly List<WeakReference<FrameworkElement>> Elements =
                new List<WeakReference<FrameworkElement>>();
        }

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

        private static void Track(FrameworkElement element)
        {
            var registry = MotionRegistries.GetOrCreateValue(element.Dispatcher);
            lock (registry.Gate)
            {
                for (var index = registry.Elements.Count - 1; index >= 0; index--)
                {
                    if (!registry.Elements[index].TryGetTarget(out var tracked))
                    {
                        registry.Elements.RemoveAt(index);
                        continue;
                    }

                    if (ReferenceEquals(tracked, element))
                        return;
                }

                registry.Elements.Add(new WeakReference<FrameworkElement>(element));
            }
        }

        /// <summary>
        /// Ends tracked render-only motion when an app or system preference disables
        /// animations. The caller owns the semantic state; this method only releases
        /// animation clocks and restores neutral visual values on the owning dispatcher.
        /// </summary>
        internal static void NormalizeAll()
        {
            FrameworkElement[] snapshot;
            var dispatcher = Dispatcher.CurrentDispatcher;
            if (!MotionRegistries.TryGetValue(dispatcher, out var registry))
                return;

            lock (registry.Gate)
            {
                var live = new List<FrameworkElement>();
                for (var index = registry.Elements.Count - 1; index >= 0; index--)
                {
                    if (registry.Elements[index].TryGetTarget(out var tracked))
                        live.Add(tracked);
                    else
                        registry.Elements.RemoveAt(index);
                }

                snapshot = live.ToArray();
            }

            foreach (var element in snapshot)
            {
                if (!element.Dispatcher.CheckAccess())
                    continue;

                var state = MotionStates.GetOrCreateValue(element);
                state.TranslateGeneration++;
                state.EntranceGeneration++;
                state.ScaleGeneration++;
                element.BeginAnimation(UIElement.OpacityProperty, null);
                if (element.RenderTransform is TranslateTransform translate)
                {
                    translate.BeginAnimation(TranslateTransform.XProperty, null);
                    translate.BeginAnimation(TranslateTransform.YProperty, null);
                    translate.X = 0;
                    translate.Y = 0;
                }
                if (state.HasScaleBase)
                {
                    var scale = GetMutableScaleTransform(element);
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                    scale.ScaleX = state.ScaleBaseX;
                    scale.ScaleY = state.ScaleBaseY;
                    state.HasScaleBase = false;
                }
                element.Opacity = 1;
            }
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
            Track(element);
            var motionState = MotionStates.GetOrCreateValue(element);
            var generation = ++motionState.TranslateGeneration;
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
                if (generation != motionState.TranslateGeneration)
                    return;

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

        /// <summary>
        /// Gives a local numeric value a short render-only pulse. The base scale is kept
        /// per element so an existing transform is restored exactly after completion or
        /// when reduced motion normalizes the active clocks.
        /// </summary>
        internal static void AnimateScalePulse(FrameworkElement element, double peak, MotionDurationKind kind)
        {
            if (peak <= 0 || double.IsNaN(peak) || double.IsInfinity(peak))
                return;

            Track(element);
            var motionState = MotionStates.GetOrCreateValue(element);
            var generation = ++motionState.ScaleGeneration;
            var scale = GetMutableScaleTransform(element);
            if (!motionState.HasScaleBase)
            {
                motionState.ScaleBaseX = scale.ScaleX;
                motionState.ScaleBaseY = scale.ScaleY;
                motionState.HasScaleBase = true;
            }

            var baseX = motionState.ScaleBaseX;
            var baseY = motionState.ScaleBaseY;
            var currentX = scale.ScaleX;
            var currentY = scale.ScaleY;
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            scale.ScaleX = currentX;
            scale.ScaleY = currentY;

            var duration = GetDuration(element, kind);
            var halfDuration = TimeSpan.FromTicks(Math.Max(1, duration.Ticks / 2));
            var easing = CreateEaseOut();
            var peakX = baseX * peak;
            var peakY = baseY * peak;
            var upX = new DoubleAnimation(currentX, peakX, halfDuration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var upY = new DoubleAnimation(currentY, peakY, halfDuration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var downX = new DoubleAnimation(peakX, baseX, halfDuration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var downY = new DoubleAnimation(peakY, baseY, halfDuration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };

            upX.Completed += (_, __) =>
            {
                if (generation != motionState.ScaleGeneration)
                    return;

                scale.BeginAnimation(ScaleTransform.ScaleXProperty, downX);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, downY);
            };
            downX.Completed += (_, __) =>
            {
                if (generation != motionState.ScaleGeneration)
                    return;

                scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                scale.ScaleX = baseX;
                scale.ScaleY = baseY;
                motionState.HasScaleBase = false;
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, upX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, upY);
        }

        internal static void AnimateEntrance(FrameworkElement element, double offsetY)
        {
            Track(element);
            var motionState = MotionStates.GetOrCreateValue(element);
            var generation = ++motionState.EntranceGeneration;
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
                if (generation != motionState.EntranceGeneration)
                    return;

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
