using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Lightweight shared presenter for Loading, Empty, Error, Degraded, Offline and
    /// Disabled workspace states. Business commands stay in the ViewModel; this control
    /// only renders the state and an optional retry action.
    /// </summary>
    public sealed class WorkspaceStatePresenter : ContentControl
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State), typeof(string), typeof(WorkspaceStatePresenter), new PropertyMetadata("Empty"));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(WorkspaceStatePresenter), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(WorkspaceStatePresenter), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DetailProperty = DependencyProperty.Register(
            nameof(Detail), typeof(string), typeof(WorkspaceStatePresenter), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
            nameof(RetryCommand), typeof(ICommand), typeof(WorkspaceStatePresenter), new PropertyMetadata(null));

        public static readonly DependencyProperty RetryTextProperty = DependencyProperty.Register(
            nameof(RetryText), typeof(string), typeof(WorkspaceStatePresenter), new PropertyMetadata("重试"));

        static WorkspaceStatePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(WorkspaceStatePresenter),
                new FrameworkPropertyMetadata(typeof(WorkspaceStatePresenter)));
        }

        public string State
        {
            get => (string)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string Detail
        {
            get => (string)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        public ICommand? RetryCommand
        {
            get => (ICommand?)GetValue(RetryCommandProperty);
            set => SetValue(RetryCommandProperty, value);
        }

        public string RetryText
        {
            get => (string)GetValue(RetryTextProperty);
            set => SetValue(RetryTextProperty, value);
        }

        public string StateGlyph => State switch
        {
            "Loading" => "\uE823",
            "Error" => "\uE711",
            "Degraded" => "\uE7BA",
            "Offline" => "\uE774",
            "Disabled" => "\uE7B3",
            _ => "\uE7BA"
        };

        public string StateDisplay => State switch
        {
            "Loading" => "加载中",
            "Error" => "错误",
            "Degraded" => "降级",
            "Offline" => "离线",
            "Disabled" => "已禁用",
            _ => "空"
        };
    }
}
