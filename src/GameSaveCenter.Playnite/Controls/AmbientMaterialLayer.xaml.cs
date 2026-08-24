using System.ComponentModel;
using System.Windows.Controls;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Controls
{
    public partial class AmbientMaterialLayer : UserControl
    {
        private DashboardViewModel? dashboard;

        public AmbientMaterialLayer()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Subscribe(DataContext);
        }

        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Unsubscribe();
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded) Subscribe(e.NewValue);
        }

        private void Subscribe(object? dataContext)
        {
            Unsubscribe();
            dashboard = dataContext as DashboardViewModel;
            if (dashboard != null)
                dashboard.PropertyChanged += OnDashboardPropertyChanged;
            ApplyDashboardMaterial();
        }

        private void Unsubscribe()
        {
            if (dashboard != null)
                dashboard.PropertyChanged -= OnDashboardPropertyChanged;
            dashboard = null;
            ApplyDashboardMaterial();
        }

        private void OnDashboardPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(DashboardViewModel.SelectedGameBackgroundAmbientBrush)
                && e.PropertyName != nameof(DashboardViewModel.HasSelectedGameBackgroundAmbientMaterial)) return;
            if (Dispatcher.CheckAccess())
                ApplyDashboardMaterial();
            else
                Dispatcher.BeginInvoke(new System.Action(ApplyDashboardMaterial));
        }

        private void ApplyDashboardMaterial()
        {
            var hasGameMaterial = dashboard?.HasSelectedGameBackgroundAmbientMaterial == true;
            ThemeAmbientWash.Opacity = hasGameMaterial ? 0 : 1;
            GameBackgroundAmbientWash.Fill = dashboard?.SelectedGameBackgroundAmbientBrush;
        }
    }
}
