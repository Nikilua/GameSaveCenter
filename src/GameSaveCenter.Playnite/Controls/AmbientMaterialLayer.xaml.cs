using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Controls
{
    public partial class AmbientMaterialLayer : UserControl
    {
        public static readonly DependencyProperty ShowLeftGlowProperty = DependencyProperty.Register(
            nameof(ShowLeftGlow),
            typeof(bool),
            typeof(AmbientMaterialLayer),
            new PropertyMetadata(true));

        public AmbientMaterialLayer()
        {
            InitializeComponent();
        }

        public bool ShowLeftGlow
        {
            get => (bool)GetValue(ShowLeftGlowProperty);
            set => SetValue(ShowLeftGlowProperty, value);
        }
    }
}
