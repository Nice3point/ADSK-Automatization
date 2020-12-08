using System.Linq;
using System.Windows;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Nice3point.Revit.ADSK.MEP.ViewModel;

namespace Nice3point.Revit.ADSK.MEP.UI
{
    public partial class CopyAdskSettingsView
    {
        private UIDocument UiDoc { get; }
        private SettingsCopyAdskViewModel ViewModel { get; }

        public CopyAdskSettingsView(UIDocument uiDoc)
        {
            UiDoc = uiDoc;
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
            var saveConfigurationDialog = new SaveFileDialog
            {
                InitialDirectory = $@"{ViewModel.CopyAdskSettingsPath}",
                Filter = "Json files (*.json)|*.json"
            };
            if (saveConfigurationDialog.ShowDialog() != true) return;
            ViewModel.ChangeCopyAdskSettingsPath(saveConfigurationDialog.FileName, true);
        }

        private void ButtonAddSpecification_OnClick(object sender, RoutedEventArgs e)
        {
            var projectSchedules = ViewModel.GetProjectSchedules(UiDoc.Document);
            var schedulePicker = new CopyAdskPickSchedulesView(projectSchedules)
            {
                Owner = this,
            };
            schedulePicker.ShowDialog();
            var selectedItems = schedulePicker.ListBoxSpecificationPicker
                .SelectedItems
                .Cast<string>()
                .ToList();
            ViewModel.AddSchedules(selectedItems);
        }

        private void ButtonDeleteSpecification_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = ListBoxSpecificationNames
                .SelectedItems
                .Cast<string>()
                .ToList();
            ViewModel.DeleteSpecifications(selectedItems);
        }
    }
}