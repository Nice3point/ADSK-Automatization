using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using AdskTemplateMepTools.Commands.CopyADSK.View;
using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace AdskTemplateMepTools.Commands.CopyADSK.Commands
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

        public static ObservableCollection<Schedule> LoadSchedules(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(json);
        }

        public static void SaveSchedules(string path, ObservableCollection<Schedule> schedules)
        {
            using var file = File.CreateText(path);
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Serialize(file, schedules);
        }

        public static ObservableCollection<Schedule> CreateDefaultSchedules(string path)
        {
            const int defaultColumn = 2;
            var schedules = new ObservableCollection<Schedule>
            {
                new("В_ОВ_Гибкие воздуховоды",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyLengthOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ОВ_Изоляция воздуховодов",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyAreaOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ОВ_Круглые воздуховоды",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyLengthOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve),
                    new CopyStringOperation(SpfParameter.AdskNote, 8)),
                new("В_ОВ_Прямоугольные воздуховоды",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyLengthOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve),
                    new CopyStringOperation(SpfParameter.AdskNote, 8)),
                new("В_ОВ_Фасонные детали воздуховодов",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn)),
                new("В_ОВ_Гибкие трубы",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyIntegerOperation(SpfParameter.AdskQuantity, 1)),
                new("В_ОВ_Изоляция труб",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyVolumeOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ОВ_Трубопроводы",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyLengthOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ВК_Гибкие трубы",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyIntegerOperation(SpfParameter.AdskQuantity, 1)),
                new("В_ВК_Изоляция труб",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyVolumeOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ВК_Трубопроводы",
                    new CopyStringOperation(SpfParameter.AdskName,defaultColumn),
                    new CopyLengthOperation(SpfParameter.AdskQuantity, ProjectGlobalParameter.Reserve)),
                new("В_ТМ_Технико-экономические показатели",
                    new CopyMassOperation(SpfParameter.AdskMassDimension, defaultColumn)),
                new("В_ТМ_Трубопроводы",
                    new CopyTemperatureOperation("Температура трубопровода"))
            };
            SaveSchedules(path, schedules);
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