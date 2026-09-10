using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Theme-aware line icon host.  The geometry is supplied by the shared icon pack and
    /// the stroke/fill follows the control foreground, so the same icon works in every
    /// Playnite theme without a bitmap background or a third-party SVG renderer.
    /// </summary>
    public sealed class ThemeAwareIcon : Control
    {
        static ThemeAwareIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ThemeAwareIcon),
                new FrameworkPropertyMetadata(typeof(ThemeAwareIcon)));
        }

        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register(
                nameof(IconData),
                typeof(Geometry),
                typeof(ThemeAwareIcon),
                new FrameworkPropertyMetadata(null));

        public Geometry IconData
        {
            get => (Geometry)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }

        public static readonly DependencyProperty FillDataProperty =
            DependencyProperty.Register(
                nameof(FillData),
                typeof(Geometry),
                typeof(ThemeAwareIcon),
                new FrameworkPropertyMetadata(null));

        public Geometry FillData
        {
            get => (Geometry)GetValue(FillDataProperty);
            set => SetValue(FillDataProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(ThemeAwareIcon),
                new FrameworkPropertyMetadata(1.85d));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
    }
}
