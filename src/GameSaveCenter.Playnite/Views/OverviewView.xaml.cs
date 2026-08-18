using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    public partial class OverviewView : UserControl
    {
        public bool UiAnimationsEnabled { get; set; } = true;

        public OverviewView() => InitializeComponent();

        // DashboardView still drives the legacy responsive coordinator while the
        // production shell owns the actual page host. Keep this compatibility
        // surface harmless for the migrated Demo layout.
        public GridLength OverviewCompactSecondaryRowHeight
        {
            get => GridLength.Auto;
            set { }
        }

        public void ApplyResponsiveColumns(bool stack) { }
        public void ApplyResponsiveWidth(double width) { }
        public void ApplyResponsiveHeight(double height, bool stack) { }

        private void Execute(ICommand command)
        {
            if (command.CanExecute(null)) command.Execute(null);
        }

        private void OnBackupClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.BackupSelectedCommand);
        }

        private void OnLoadDetailsClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.LoadDetailsCommand);
        }

        private void OnAttentionClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.OpenAttentionCenterCommand);
        }

        private void OnProtectionGamesClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.OpenProtectionGamesCommand);
        }

        private void OnApplyProtectionClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.ApplyRecommendedProtectionCommand);
        }

        private void OnOpenMaintenanceClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is DashboardViewModel vm) Execute(vm.OpenMaintenanceCommand);
        }
    }
}
