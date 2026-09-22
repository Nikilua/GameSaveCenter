using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

using GameSaveCenter.Playnite.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Shows short, non-modal clipboard feedback beside the initiating control.
    /// The attached state keeps one reusable popup per target and the active state
    /// is shared so repeated copy actions never build a second toast stack.
    /// </summary>
    internal static class ClipboardFeedback
    {
        private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State",
            typeof(ClipboardFeedbackState),
            typeof(ClipboardFeedback),
            new PropertyMetadata(null));

        private static ClipboardFeedbackState? activeState;

        internal static void Show(FrameworkElement target, string message, bool isError)
        {
            if (target == null || target.Dispatcher.HasShutdownStarted || target.Dispatcher.HasShutdownFinished)
                return;

            var state = target.GetValue(StateProperty) as ClipboardFeedbackState;
            if (state == null)
            {
                state = new ClipboardFeedbackState(target);
                target.SetValue(StateProperty, state);
            }

            if (activeState != null && !ReferenceEquals(activeState, state))
            {
                if (activeState.Dispatcher.CheckAccess())
                    activeState.Hide();
                activeState = null;
            }

            state.Show(message, isError);
            activeState = state;
        }

        internal static bool IsVisibleForVerification(FrameworkElement target)
            => (target?.GetValue(StateProperty) as ClipboardFeedbackState)?.IsVisible == true;

        internal static Popup? GetPopupForVerification(FrameworkElement target)
            => (target?.GetValue(StateProperty) as ClipboardFeedbackState)?.Popup;

        internal static string GetMessageForVerification(FrameworkElement target)
            => (target?.GetValue(StateProperty) as ClipboardFeedbackState)?.Message ?? string.Empty;

        internal static void CloseForVerification(FrameworkElement target)
        {
            var state = target?.GetValue(StateProperty) as ClipboardFeedbackState;
            if (state == null || !state.Dispatcher.CheckAccess()) return;
            state.Hide();
            if (ReferenceEquals(activeState, state)) activeState = null;
        }

        private sealed class ClipboardFeedbackState
        {
            private readonly FrameworkElement target;
            private FeedbackToast? card;
            private Border? indicator;
            private TextBlock? messageText;
            private Popup? popup;
            private DispatcherTimer? timer;

            internal ClipboardFeedbackState(FrameworkElement target)
            {
                this.target = target;
            }

            internal Dispatcher Dispatcher => target.Dispatcher;
            internal Popup? Popup => popup;
            internal string Message { get; private set; } = string.Empty;
            internal bool IsVisible => popup?.IsOpen == true;

            internal void Show(string message, bool isError)
            {
                EnsurePopup();
                Message = message ?? string.Empty;

                if (indicator != null)
                {
                    indicator.SetResourceReference(
                        Border.BackgroundProperty,
                        isError ? "GscErrorBrush" : "GscSuccessBrush");
                }

                if (messageText != null)
                {
                    messageText.Text = Message;
                    messageText.ToolTip = Message;
                }

                if (card != null)
                {
                    var summary = "复制结果：" + Message;
                    AutomationProperties.SetName(card, summary);
                    AutomationProperties.SetHelpText(card, isError
                        ? "复制失败；剪贴板可能被其他程序占用，请稍后重试。"
                        : "复制成功；反馈会自动消失。" + summary);
                    card.RaiseFeedbackChanged();
                }

                if (popup == null || timer == null) return;
                timer.Stop();
                popup.IsOpen = true;
                timer.Interval = TimeSpan.FromSeconds(isError ? 5 : 3.2);
                timer.Start();
            }

            internal void Hide()
            {
                timer?.Stop();
                if (popup != null) popup.IsOpen = false;
            }

            private void EnsurePopup()
            {
                if (popup != null) return;

                card = new FeedbackToast
                {
                    Focusable = false,
                    IsHitTestVisible = false,
                    Width = 320,
                    MaxWidth = 320,
                    Margin = new Thickness(0),
                    Padding = new Thickness(12, 9, 12, 9)
                };
                var cardStyle = target.TryFindResource("GscRedesignFeedbackToastCard") as Style;
                if (cardStyle != null) card.Style = cardStyle;
                else
                {
                    card.SetResourceReference(Border.BackgroundProperty, "GscGlassStrongBrush");
                    card.SetResourceReference(Border.BorderBrushProperty, "GscGlassStrokeBrush");
                    card.BorderThickness = new Thickness(1);
                    card.CornerRadius = new CornerRadius(12);
                }

                var layout = new Grid();
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(9) });
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                indicator = new Border
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 4, 7, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                indicator.SetResourceReference(Border.BackgroundProperty, "GscSuccessBrush");
                layout.Children.Add(indicator);

                messageText = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxHeight = 60
                };
                var messageStyle = target.TryFindResource("GscRedesignFeedbackToastMessage") as Style;
                if (messageStyle != null) messageText.Style = messageStyle;
                else messageText.SetResourceReference(TextBlock.ForegroundProperty, "GscSecondaryTextBrush");
                Grid.SetColumn(messageText, 1);
                layout.Children.Add(messageText);
                card.Child = layout;

                popup = new Popup
                {
                    PlacementTarget = target,
                    Placement = PlacementMode.Bottom,
                    VerticalOffset = 6,
                    AllowsTransparency = true,
                    StaysOpen = true,
                    Focusable = false,
                    Child = card
                };
                timer = new DispatcherTimer(DispatcherPriority.Background, target.Dispatcher)
                {
                    Interval = TimeSpan.FromSeconds(3.2)
                };
                timer.Tick += (_, __) => Hide();
            }
        }
    }
}
