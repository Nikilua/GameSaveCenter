using System.Windows;

namespace WpfAppleDesktopUi.Examples
{
    public enum WidthLayoutMode { Wide, Medium, Compact }
    public enum HeightDensityMode { Comfortable, Compact }

    public static class AdaptiveLayout
    {
        public static readonly DependencyProperty WidthModeProperty =
            DependencyProperty.RegisterAttached("WidthMode", typeof(WidthLayoutMode), typeof(AdaptiveLayout),
                new FrameworkPropertyMetadata(WidthLayoutMode.Wide, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty HeightModeProperty =
            DependencyProperty.RegisterAttached("HeightMode", typeof(HeightDensityMode), typeof(AdaptiveLayout),
                new FrameworkPropertyMetadata(HeightDensityMode.Comfortable, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(AdaptiveLayout),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetWidthMode(DependencyObject e, WidthLayoutMode value) => e.SetValue(WidthModeProperty, value);
        public static WidthLayoutMode GetWidthMode(DependencyObject e) => (WidthLayoutMode)e.GetValue(WidthModeProperty);
        public static void SetHeightMode(DependencyObject e, HeightDensityMode value) => e.SetValue(HeightModeProperty, value);
        public static HeightDensityMode GetHeightMode(DependencyObject e) => (HeightDensityMode)e.GetValue(HeightModeProperty);
        public static void SetIsEnabled(DependencyObject e, bool value) => e.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject e) => (bool)e.GetValue(IsEnabledProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is FrameworkElement element)) return;
            if ((bool)e.NewValue) { element.SizeChanged += OnSizeChanged; Update(element); }
            else element.SizeChanged -= OnSizeChanged;
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e) => Update((FrameworkElement)sender);
        private static void Update(FrameworkElement element)
        {
            SetWidthMode(element, element.ActualWidth >= 1280 ? WidthLayoutMode.Wide : element.ActualWidth >= 980 ? WidthLayoutMode.Medium : WidthLayoutMode.Compact);
            SetHeightMode(element, element.ActualHeight >= 760 ? HeightDensityMode.Comfortable : HeightDensityMode.Compact);
        }
    }
}
