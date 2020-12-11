using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using Nice3point.Revit.ADSK.MEP.ViewModel;

namespace Nice3point.Revit.ADSK.MEP.Model
{
    public class SettingsCopyAdskModel
    {
        private readonly FileIniDataParser _iniParser;
        private readonly IniData _iniData;
        public SettingsCopyAdskModel(SettingsCopyAdskViewModel viewModel)
        {
            ViewModel = viewModel;
            Application.CheckConfiguration();
            _iniParser = new FileIniDataParser();
            _iniData = _iniParser.ReadFile($@"{Application.ConfigurationPath}");
            LoadConfiguration();
            LoadSpecificationNames();
        }

        private SettingsCopyAdskViewModel ViewModel { get; }

        private void LoadConfiguration()
        {
            
            if (!_iniData["CopyAdsk"].ContainsKey("Profile file"))
            {
                IniWriteKey();
            }
            ViewModel.CopyAdskSettingsPath = _iniData["CopyAdsk"]["Profile file"];
        }

        private void IniWriteKey()
        {
            _iniData["CopyAdsk"]["Profile file"] = $@"{ViewModel.CopyAdskSettingsPath}";
            _iniParser.WriteFile($@"{Application.ConfigurationPath}", _iniData);
        }

        public void UpdateConfiguration()
        {
            IniWriteKey();
        }

        public void LoadSpecificationNames()
        {
            if (!File.Exists($@"{ViewModel.CopyAdskSettingsPath}"))
            {
                CreateDefaultSpecificationNames();
            }
            else
            {
                var json = File.ReadAllText($@"{ViewModel.CopyAdskSettingsPath}");
                if (ViewModel.SpecificationNames.Count > 0) ViewModel.SpecificationNames.Clear();
                foreach (var s in JsonConvert.DeserializeObject<ObservableCollection<string>>(json))
                {
                    ViewModel.SpecificationNames.Add(s);
                }
            }
        }

        public void SaveSpecificationNames()
        {
            using var file = File.CreateText($@"{ViewModel.CopyAdskSettingsPath}");
            var serializer = new JsonSerializer();
            serializer.Serialize(file, ViewModel.SpecificationNames);
        }

        private void CreateDefaultSpecificationNames()
        {
            ViewModel.SpecificationNames = new ObservableCollection<string>
            {
                "В_ОВ_Гибкие воздуховоды",
                "В_ОВ_Изоляция воздуховодов",
                "В_ОВ_Круглые воздуховоды",
                "В_ОВ_Прямоугольные воздуховоды",
                "В_ОВ_Фасонные детали воздуховодов",
                "В_ОВ_Гибкие трубы",
                "В_ОВ_Изоляция труб",
                "В_ОВ_Трубопроводы",
                "В_ВК_Гибкие трубы",
                "В_ВК_Изоляция труб",
                "В_ВК_Трубопроводы"
            };
            var json = JsonConvert.SerializeObject(ViewModel.SpecificationNames);
            File.WriteAllText($@"{ViewModel.CopyAdskSettingsPath}", json);
        }

        public void DeleteSpecifications(IEnumerable<string> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                ViewModel.SpecificationNames.Remove(item);
            }
        }

        public List<string> GetProjectSchedules(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(s => !s.IsTemplate)
                .Where(s => !s.IsTitleblockRevisionSchedule)
                .Select(s => s.Name)
                .Where(s => !ViewModel.SpecificationNames.Contains(s))
                .OrderBy(s => s)
                .ToList();
        }
    }
}