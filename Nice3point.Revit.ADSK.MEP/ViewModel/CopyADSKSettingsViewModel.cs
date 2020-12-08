using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.ADSK.MEP.Annotations;
using Nice3point.Revit.ADSK.MEP.Model;

namespace Nice3point.Revit.ADSK.MEP.ViewModel
{
    public class SettingsCopyAdskViewModel : INotifyPropertyChanged
    {
        public static readonly string ConfigurationDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".RevitAddins");

        public readonly string ConfigurationPath = Path.Combine(ConfigurationDirectory, "config.ini");
        private string _copyAdskSettingsPath = Path.Combine(ConfigurationDirectory, "CopyADSKSettings.json");

        public SettingsCopyAdskViewModel()
        {
            SpecificationNames = new ObservableCollection<string>();
            Model = new SettingsCopyAdskModel(this);
        }

        public ObservableCollection<string> SpecificationNames { get; set; }

        private SettingsCopyAdskModel Model { get; }

        public string CopyAdskSettingsPath
        {
            get => _copyAdskSettingsPath;
            set
            {
                _copyAdskSettingsPath = value;
                OnPropertyChanged(nameof(CopyAdskSettingsPath));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeCopyAdskSettingsPath(string path, bool saving)
        {
            CopyAdskSettingsPath = path;
            if (saving)
            {
                Model.UpdateConfiguration();
                Model.SaveSpecificationNames();
            }
            else
            {
                Model.UpdateConfiguration();
                Model.LoadSpecificationNames();
            }
        }

        public void DeleteSpecifications(IEnumerable<string> selectedItems)
        {
            Model.DeleteSpecifications(selectedItems);
        }

        public List<string> GetProjectSchedules(Document uiDocDocument)
        {
            return Model.GetProjectSchedules(uiDocDocument);
        }

        public void AddSchedules(List<string> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                SpecificationNames.Add(item);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}