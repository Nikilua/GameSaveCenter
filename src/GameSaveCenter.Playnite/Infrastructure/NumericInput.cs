using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Validates integer editor text before a settings binding commits it.</summary>
    public sealed class IntegerRangeValidationRule : ValidationRule
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; } = int.MaxValue;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => ValidateValue(value, cultureInfo, Minimum, Maximum);

        public static ValidationResult ValidateValue(object value, CultureInfo cultureInfo, int minimum, int maximum)
        {
            var formatProvider = cultureInfo ?? CultureInfo.CurrentCulture;
            var text = Convert.ToString(value, formatProvider)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return new ValidationResult(false, "请输入数值。");
            if (!int.TryParse(text, NumberStyles.Integer, formatProvider, out var parsed))
                return new ValidationResult(false, "请输入整数，且不能超出整数范围。");
            if (parsed < minimum || parsed > maximum)
                return new ValidationResult(false, $"请输入 {minimum}–{maximum} 之间的数值。");
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// Gives every numeric TextBox the same bounded wheel behavior as its binding rule.
    /// Invalid text and an out-of-range next value are left untouched so the page can keep
    /// receiving the wheel event; in particular, this never silently clamps user input.
    /// </summary>
    public static class NumericInput
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(NumericInput), new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(DependencyObject element) => (bool)element.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject element, bool value) => element.SetValue(EnabledProperty, value);

        private static void OnEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is not TextBox textBox) return;
            if ((bool)args.NewValue) textBox.PreviewMouseWheel += OnPreviewMouseWheel;
            else textBox.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (sender is not TextBox textBox
                || !textBox.IsEnabled
                || textBox.IsReadOnly
                || args.Delta == 0)
                return;

            var range = FindRangeRule(textBox);
            if (range == null) return;

            var culture = CultureInfo.CurrentCulture;
            var validation = range.Validate(textBox.Text, culture);
            if (!validation.IsValid
                || !int.TryParse(textBox.Text?.Trim() ?? string.Empty, NumberStyles.Integer, culture, out var current))
                return;

            var next = (long)current + Math.Sign(args.Delta);
            if (next < range.Minimum || next > range.Maximum)
                return;

            var originalText = textBox.Text;
            var originalCaret = textBox.CaretIndex;
            textBox.Text = next.ToString(culture);
            textBox.CaretIndex = textBox.Text.Length;

            var expression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            expression?.UpdateSource();
            if (Validation.GetHasError(textBox))
            {
                textBox.Text = originalText;
                textBox.CaretIndex = Math.Min(originalCaret, textBox.Text?.Length ?? 0);
                return;
            }

            args.Handled = true;
        }

        private static IntegerRangeValidationRule? FindRangeRule(TextBox textBox)
        {
            var expression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            return expression?.ParentBinding?.ValidationRules
                .OfType<IntegerRangeValidationRule>()
                .FirstOrDefault();
        }
    }

    /// <summary>Lets numeric fields replace their complete value on keyboard focus without changing mouse caret behavior.</summary>
    public static class SelectAllOnKeyboardFocus
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(SelectAllOnKeyboardFocus), new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(DependencyObject element) => (bool)element.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject element, bool value) => element.SetValue(EnabledProperty, value);

        private static void OnEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is not TextBox textBox) return;
            if ((bool)args.NewValue) textBox.GotKeyboardFocus += OnGotKeyboardFocus;
            else textBox.GotKeyboardFocus -= OnGotKeyboardFocus;
        }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
        {
            if (sender is not TextBox textBox) return;

            // Keyboard focus can be delivered while a hosted page is being torn down.  Selecting
            // the text is only a convenience; never let a dispatcher shutdown race turn it into
            // an unhandled exception in Playnite's UI dispatcher.
            var dispatcher = textBox.Dispatcher;
            if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished) return;

            try
            {
                dispatcher.BeginInvoke(new Action(textBox.SelectAll), DispatcherPriority.Input);
            }
            catch (InvalidOperationException)
            {
                // The dispatcher may begin shutdown immediately after the guard above.  There is
                // no safe UI surface left to select, so leaving the existing caret is intentional.
            }
        }
    }
}
