using System.Windows;
using Microsoft.Win32;
using Nice3point.Revit.ADSK.MEP.ViewModel;

namespace Nice3point.Revit.ADSK.MEP.UI
{
    public partial class CopyAdskSettings
    {
        private SettingsCopyAdskViewModel ViewModel { get; }

        public CopyAdskSettings()
        {
            ViewModel = new SettingsCopyAdskViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }

        private void ButtonLoadingConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            var loadingConfigurationDialog = new OpenFileDialog
            {
                InitialDirectory = $@"{ViewModel.CopyAdskSettingsPath}",
                Filter = "Json files (*.json)|*.json"
            };
            if (loadingConfigurationDialog.ShowDialog() != true) return;
            ViewModel.ChangeCopyAdskSettingsPath(loadingConfigurationDialog.FileName, false);
        }

        private void ButtonSaveConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            var saveConfigurationDialog = new SaveFileDialog()
            {
                InitialDirectory = $@"{ViewModel.CopyAdskSettingsPath}",
                Filter = "Json files (*.json)|*.json"
            };
            if (saveConfigurationDialog.ShowDialog() != true) return;
            ViewModel.ChangeCopyAdskSettingsPath(saveConfigurationDialog.FileName, true);
        }
    }
}