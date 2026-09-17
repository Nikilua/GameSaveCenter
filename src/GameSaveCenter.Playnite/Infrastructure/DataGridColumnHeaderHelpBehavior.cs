using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Adds a short semantic explanation to generated DataGrid column headers without
    /// replacing the string Header. The header remains the WPF sorting and resize target.
    /// </summary>
    public static class DataGridColumnHeaderHelpBehavior
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
            "Description",
            typeof(string),
            typeof(DataGridColumnHeaderHelpBehavior),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(DataGridColumnHeaderHelpBehavior),
            new PropertyMetadata(false, OnEnabledChanged));

        public static void SetDescription(DependencyObject element, string value)
            => element.SetValue(DescriptionProperty, value ?? string.Empty);

        public static string GetDescription(DependencyObject element)
            => (string)element.GetValue(DescriptionProperty);

        public static void SetEnabled(DependencyObject element, bool value)
            => element.SetValue(EnabledProperty, value);

        public static bool GetEnabled(DependencyObject element)
            => (bool)element.GetValue(EnabledProperty);

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is DataGridColumnHeader header)) return;

            if ((bool)args.NewValue)
            {
                header.Loaded += OnHeaderLoaded;
                ApplyDescription(header);
            }
            else
            {
                header.Loaded -= OnHeaderLoaded;
            }
        }

        private static void OnHeaderLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is DataGridColumnHeader header)
                ApplyDescription(header);
        }

        private static void ApplyDescription(DataGridColumnHeader header)
        {
            var description = header.Column == null ? string.Empty : GetDescription(header.Column);
            if (string.IsNullOrWhiteSpace(description)) return;

            // Keep the native header as the only interactive element. A tooltip and
            // Automation HelpText do not intercept Sorting, keyboard navigation or resize.
            header.ToolTip = description;
            AutomationProperties.SetHelpText(header, description);
        }
    }
}
