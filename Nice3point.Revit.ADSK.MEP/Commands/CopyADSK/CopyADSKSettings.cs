using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace Nice3point.Revit.ADSK.MEP.Commands.CopyADSK
{
    [Transaction(TransactionMode.ReadOnly)] 
    public class CopyAdskSettings : IExternalCommand
    {
        private Document _doc;
        private UIDocument _uiDoc;
        private CopyAdskSettingsViewModel _viewModel;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _viewModel = new CopyAdskSettingsViewModel(this);
            var view = new CopyAdskSettingsView(_viewModel);
            view.ShowDialog();
            return Result.Succeeded;
        }

        public void CreateInstance(ExternalCommandData commandData)
        {
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _viewModel = new CopyAdskSettingsViewModel(this);
        }

        public ObservableCollection<string> LoadSchedules(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObservableCollection<string>>(json);
        }
        public void SaveSchedules(string path, ObservableCollection<string> schedules)
        {
            using var file = File.CreateText(path);
            var serializer = new JsonSerializer();
            serializer.Serialize(file, schedules);
        }
        public ObservableCollection<string> CreateDefaultSchedules(string path)
        {
            var schedules = new ObservableCollection<string>
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

            File.WriteAllText(path, JsonConvert.SerializeObject(schedules));
            return schedules;
        }

        public IEnumerable<string> GetProjectSchedules()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(s => !s.IsTemplate)
                .Where(s => !s.IsTitleblockRevisionSchedule)
                .Select(s => s.Name);
        }
    }
}