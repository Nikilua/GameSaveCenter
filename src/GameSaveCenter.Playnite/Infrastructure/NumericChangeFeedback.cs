using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Adds a deliberately small, render-only pulse to selected numeric summary values.
    /// The behavior is opt-in so progress text and technical values remain stable and
    /// copyable.  Motion is rate-limited to avoid turning frequent refreshes into a
    /// continuous bounce.
    /// </summary>
    public static class NumericChangeFeedback
    {
        private static readonly ConditionalWeakTable<TextBlock, State> States =
            new ConditionalWeakTable<TextBlock, State>();

        private static readonly DependencyPropertyDescriptor TextPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

        private static readonly TimeSpan MinimumFeedbackInterval =
            TimeSpan.FromMilliseconds(420);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(NumericChangeFeedback),
                new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// Inherited from the owning page so the existing application/system motion gate
        /// can make the counter update immediate without changing its binding value.
        /// </summary>
        public static readonly DependencyProperty MotionEnabledProperty =
            DependencyProperty.RegisterAttached(
                "MotionEnabled",
                typeof(bool),
                typeof(NumericChangeFeedback),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetIsEnabled(DependencyObject element, bool value)
            => element.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject element)
            => (bool)element.GetValue(IsEnabledProperty);

        public static void SetMotionEnabled(DependencyObject element, bool value)
            => element.SetValue(MotionEnabledProperty, value);

        public static bool GetMotionEnabled(DependencyObject element)
            => (bool)element.GetValue(MotionEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (!(dependencyObject is TextBlock textBlock))
                return;

            if ((bool)args.NewValue)
            {
                TextPropertyDescriptor.RemoveValueChanged(textBlock, OnTextChanged);
                TextPropertyDescriptor.AddValueChanged(textBlock, OnTextChanged);
                var state = States.GetOrCreateValue(textBlock);
                // A value already present when the behavior is attached is the initial
                // baseline. An empty binding still waits for its first real value.
                state.HasObservedText = !string.IsNullOrEmpty(textBlock.Text);
                state.LastText = textBlock.Text ?? string.Empty;
                state.LastFeedbackUtc = DateTime.MinValue;
                state.FeedbackCount = 0;
                return;
            }

            TextPropertyDescriptor.RemoveValueChanged(textBlock, OnTextChanged);
            States.Remove(textBlock);
        }

        private static void OnTextChanged(object sender, EventArgs args)
        {
            var textBlock = (TextBlock)sender;
            var state = States.GetOrCreateValue(textBlock);
            var currentText = textBlock.Text ?? string.Empty;

            if (!state.HasObservedText)
            {
                state.HasObservedText = true;
                state.LastText = currentText;
                return;
            }

            var previousText = state.LastText;
            state.LastText = currentText;
            if (!TryParseCounter(previousText, out var previousValue)
                || !TryParseCounter(currentText, out var currentValue)
                || previousValue == currentValue
                || !GetMotionEnabled(textBlock)
                || !GscMotion.IsEnabled(true))
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (now - state.LastFeedbackUtc < MinimumFeedbackInterval)
                return;

            state.LastFeedbackUtc = now;
            state.FeedbackCount++;
            GscMotion.AnimateScalePulse(textBlock, 1.04, GscMotion.MotionDurationKind.Fast);
        }

        private static bool TryParseCounter(string text, out long value)
        {
            return long.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
                || long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Test-only observation point. It does not participate in the production binding
        /// contract and lets a behavior test distinguish a real pulse from a source check.
        /// </summary>
        internal static int GetFeedbackCountForAudit(TextBlock textBlock)
            => States.TryGetValue(textBlock, out var state) ? state.FeedbackCount : 0;

        private sealed class State
        {
            internal bool HasObservedText;
            internal string LastText = string.Empty;
            internal DateTime LastFeedbackUtc;
            internal int FeedbackCount;
        }
    }
}
