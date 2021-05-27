using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;
using AdskTemplateMepTools.RevitAPI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AdskTemplateMepTools.Commands.CopyADSK.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CopyAdsk : IExternalCommand
    {
        private static Document _doc;
        private static List<Element> _copiedElements;
        private static string _copiedData;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            _doc = uiDoc.Document;
            var settings = new CopyAdskSettings();
            settings.CreateInstance(commandData);
            var viewModel = new CopyAdskSettingsViewModel(settings);
            foreach (var schedule in viewModel.Schedules)
            {
                var view = new FilteredElementCollector(_doc)
                           .OfClass(typeof(ViewSchedule))
                           .Cast<ViewSchedule>()
                           .FirstOrDefault(vs => vs.Name.Equals(schedule.Name));
                if (view == null) continue;
                uiDoc.ActiveView = view;
                CopyToAdsk(view, schedule.Operations);
                CloseSchedules(uiDoc, view);
            }

            return Result.Succeeded;
        }

        private static void CloseSchedules(UIDocument uiDoc, Element view)
        {
            var openViews = uiDoc.GetOpenUIViews();
            var countOpenViews = uiDoc.GetOpenUIViews().Count;
            foreach (var uiView in openViews)
            {
                if (countOpenViews == 1) break;
                if (view.Id == uiView.ViewId) uiView.Close();
                countOpenViews -= 1;
            }
        }

        private static void CopyToAdsk(ViewSchedule vs, ObservableCollection<Operation> operations)
        {
            var tData = vs.GetTableData();
            var tsDada = tData.GetSectionData(SectionType.Body);
            using var tGroup = new TransactionGroup(_doc, "Заполнение ADSK_Наименование в спецификациях");
            tGroup.Start();
            for (var rInd = 0; rInd < tsDada.NumberOfRows; rInd++)
                foreach (var operation in operations)
                {
                    _copiedElements = RevitFunctions.GetElementsOnRow(_doc, vs, rInd);
                    if (_copiedElements == null) continue;
                    switch (operation.Command)
                    {
                        case Command.CopyName:
                            _copiedData = tsDada.GetCellText(rInd, operation.SourceColumn - 1);
                            RevitFunctions.CopyAdskName(_doc, _copiedElements, _copiedData);
                            break;
                        case Command.CopyLength:
                            RevitFunctions.CopyLengthValue(_doc, _copiedElements, operation.ReserveLength,
                                operation.ReserveParameter);
                            break;
                        case Command.CopyCount:
                            RevitFunctions.CopyCountValue(_doc, _copiedElements);
                            break;
                        case Command.CopyArea:
                            RevitFunctions.CopyAreaValue(_doc, _copiedElements, operation.ReserveLength,
                                operation.ReserveParameter);
                            break;
                        case Command.CopyVolume:
                            RevitFunctions.CopyVolumeValue(_doc, _copiedElements, operation.ReserveLength,
                                operation.ReserveParameter);
                            break;
                        case Command.CopyComment:
                            _copiedData = tsDada.GetCellText(rInd, operation.SourceColumn - 1);
                            RevitFunctions.CopyCommentValue(_doc, _copiedElements, _copiedData);
                            break;
                        case Command.CopyMass:
                            _copiedData = tsDada.GetCellText(rInd, operation.SourceColumn - 1);
                            RevitFunctions.CopyMass(_doc, _copiedElements, _copiedData);
                            break;
                        case Command.CopyTemperature:
                            RevitFunctions.CopyTemperature(_doc, _copiedElements, operation.Parameter);
                            break;
                    }
                }

            tGroup.Assimilate();
        }
    }
}