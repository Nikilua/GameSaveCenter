using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Pauses the shared indeterminate progress animation while its visual is not
    /// presented, its page is hidden, or its host window is minimized. The control's
    /// IsIndeterminate value remains owned by the real busy state.
    /// </summary>
    public static class IndeterminateProgressBehavior
    {
        public static readonly DependencyProperty PauseWhenUnavailableProperty =
            DependencyProperty.RegisterAttached(
                "PauseWhenUnavailable",
                typeof(bool),
                typeof(IndeterminateProgressBehavior),
                new PropertyMetadata(false, OnPauseWhenUnavailableChanged));

        public static readonly DependencyProperty IsAnimationPausedProperty =
            DependencyProperty.RegisterAttached(
                "IsAnimationPaused",
                typeof(bool),
                typeof(IndeterminateProgressBehavior),
                new FrameworkPropertyMetadata(false));

        private static readonly ConditionalWeakTable<ProgressBar, Subscription> Subscriptions =
            new ConditionalWeakTable<ProgressBar, Subscription>();

        public static void SetPauseWhenUnavailable(DependencyObject element, bool value)
            => element.SetValue(PauseWhenUnavailableProperty, value);

        public static bool GetPauseWhenUnavailable(DependencyObject element)
            => (bool)element.GetValue(PauseWhenUnavailableProperty);

        public static void SetIsAnimationPaused(DependencyObject element, bool value)
            => element.SetValue(IsAnimationPausedProperty, value);

        public static bool GetIsAnimationPaused(DependencyObject element)
            => (bool)element.GetValue(IsAnimationPausedProperty);

        private static void OnPauseWhenUnavailableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressBar progressBar))
                return;

            if ((bool)e.NewValue)
            {
                var subscription = Subscriptions.GetOrCreateValue(progressBar);
                subscription.Attach(progressBar);
                subscription.Refresh(progressBar);
                return;
            }

            if (Subscriptions.TryGetValue(progressBar, out var existing))
            {
                existing.Detach(progressBar);
                Subscriptions.Remove(progressBar);
            }

            SetIsAnimationPaused(progressBar, false);
        }

        private sealed class Subscription
        {
            private ProgressBar? progressBar;
            private Window? hostWindow;

            public void Attach(ProgressBar progressBar)
            {
                this.progressBar = progressBar;
                progressBar.Loaded -= OnLoaded;
                progressBar.Loaded += OnLoaded;
                progressBar.Unloaded -= OnUnloaded;
                progressBar.Unloaded += OnUnloaded;
                progressBar.IsVisibleChanged -= OnIsVisibleChanged;
                progressBar.IsVisibleChanged += OnIsVisibleChanged;
            }

            public void Detach(ProgressBar progressBar)
            {
                progressBar.Loaded -= OnLoaded;
                progressBar.Unloaded -= OnUnloaded;
                progressBar.IsVisibleChanged -= OnIsVisibleChanged;
                SetHostWindow(progressBar, null);
                SetIsAnimationPaused(progressBar, false);
                this.progressBar = null;
            }

            public void Refresh(ProgressBar progressBar)
            {
                if (!progressBar.IsLoaded)
                {
                    SetHostWindow(progressBar, null);
                    SetIsAnimationPaused(progressBar, false);
                    return;
                }

                var window = Window.GetWindow(progressBar);
                SetHostWindow(progressBar, window);
                SetIsAnimationPaused(
                    progressBar,
                    !progressBar.IsVisible
                        || window == null
                        || !window.IsVisible
                        || window.WindowState == WindowState.Minimized);
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
                => Refresh((ProgressBar)sender);

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                var progressBar = (ProgressBar)sender;
                SetHostWindow(progressBar, null);
                SetIsAnimationPaused(progressBar, true);
            }

            private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
                => Refresh((ProgressBar)sender);

            private void SetHostWindow(ProgressBar progressBar, Window? nextWindow)
            {
                if (ReferenceEquals(hostWindow, nextWindow))
                    return;

                if (hostWindow != null)
                {
                    hostWindow.StateChanged -= OnHostWindowStateChanged;
                    hostWindow.IsVisibleChanged -= OnHostWindowIsVisibleChanged;
                }

                hostWindow = nextWindow;
                if (hostWindow != null)
                {
                    hostWindow.StateChanged += OnHostWindowStateChanged;
                    hostWindow.IsVisibleChanged += OnHostWindowIsVisibleChanged;
                }
            }

            private void OnHostWindowStateChanged(object? sender, EventArgs e)
            {
                if (progressBar != null)
                    Refresh(progressBar);
            }

            private void OnHostWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (progressBar != null)
                    Refresh(progressBar);
            }
        }
    }
}
