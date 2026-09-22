using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Owns the embedded dialog mask and card transition as one visual surface. The mask
    /// stays hit-testable during exit and is collapsed only after the card has settled.
    /// </summary>
    internal sealed class DialogOverlayMotion
    {
        private readonly Grid overlay;
        private readonly Border card;
        private int generation;
        private Action? completion;
        private bool closing;

        internal DialogOverlayMotion(Grid overlay, Border card)
        {
            this.overlay = overlay;
            this.card = card;
        }

        internal void BeginOpen(bool animated, Action completed)
        {
            var currentGeneration = ++generation;
            completion = completed;
            closing = false;
            ClearMotion();
            overlay.Visibility = Visibility.Visible;
            card.Opacity = animated ? 0 : 1;
            var translate = GscMotion.GetMutableTranslateTransform(card);
            translate.Y = animated ? 14 : 0;

            if (!animated)
            {
                CompleteOpen(currentGeneration);
                return;
            }

            var duration = GscMotion.GetDuration(card, GscMotion.MotionDurationKind.Normal);
            var easing = GscMotion.CreateEaseOut();
            var fade = new DoubleAnimation(0, 1, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var slide = new DoubleAnimation(14, 0, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            slide.Completed += (_, __) => CompleteOpen(currentGeneration);
            card.BeginAnimation(UIElement.OpacityProperty, fade);
            translate.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        internal void BeginClose(bool animated, Action completed)
        {
            var currentGeneration = ++generation;
            completion = completed;
            closing = true;

            if (overlay.Visibility != Visibility.Visible)
            {
                CompleteClose(currentGeneration);
                return;
            }

            var translate = GscMotion.GetMutableTranslateTransform(card);
            var currentOpacity = card.Opacity;
            var currentY = translate.Y;
            card.BeginAnimation(UIElement.OpacityProperty, null);
            translate.BeginAnimation(TranslateTransform.YProperty, null);
            card.Opacity = currentOpacity;
            translate.Y = currentY;

            if (!animated)
            {
                CompleteClose(currentGeneration);
                return;
            }

            var duration = GscMotion.GetDuration(card, GscMotion.MotionDurationKind.Normal);
            var easing = GscMotion.CreateEaseOut();
            var fade = new DoubleAnimation(currentOpacity, 0, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            var slide = new DoubleAnimation(currentY, 14, duration)
            {
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };
            // Opacity is the shared completion clock for the mask/card transition. The
            // translate clock remains visual-only; using one authority prevents a stale
            // transform callback from leaving the mask visible after close.
            fade.Completed += (_, __) => CompleteClose(currentGeneration);
            card.BeginAnimation(UIElement.OpacityProperty, fade);
            translate.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        /// <summary>
        /// Applies the reduced-motion terminal state without leaving a stale callback or
        /// an overlay visible behind the next page.
        /// </summary>
        internal void Normalize()
        {
            var wasClosing = closing;
            var callback = completion;
            completion = null;
            closing = false;
            generation++;
            ClearMotion();

            if (wasClosing)
            {
                card.Opacity = 0;
                overlay.Visibility = Visibility.Collapsed;
            }
            else if (overlay.Visibility == Visibility.Visible)
            {
                card.Opacity = 1;
            }

            callback?.Invoke();
        }

        internal void ForceClosed()
        {
            completion = null;
            closing = false;
            generation++;
            ClearMotion();
            card.Opacity = 0;
            overlay.Visibility = Visibility.Collapsed;
        }

        private void CompleteOpen(int currentGeneration)
        {
            if (currentGeneration != generation || closing || overlay.Visibility != Visibility.Visible)
                return;

            var callback = completion;
            completion = null;
            ClearMotion();
            card.Opacity = 1;
            callback?.Invoke();
        }

        private void CompleteClose(int currentGeneration)
        {
            if (currentGeneration != generation || !closing)
                return;

            var callback = completion;
            completion = null;
            closing = false;
            ClearMotion();
            card.Opacity = 0;
            overlay.Visibility = Visibility.Collapsed;
            callback?.Invoke();
        }

        private void ClearMotion()
        {
            card.BeginAnimation(UIElement.OpacityProperty, null);
            if (card.RenderTransform is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                translate.Y = 0;
            }
        }
    }
}
