using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
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

        private static void CopyToAdsk(ViewSchedule vs, ObservableCollection<IOperation> operations)
        {
            var tData = vs.GetTableData();
            var tsDada = tData.GetSectionData(SectionType.Body);
            TransactionManager.CreateTransactionGroup(_doc, "Копирование параметров", () =>
            {
                for (var rInd = 0; rInd < tsDada.NumberOfRows; rInd++)
                    foreach (var operation in operations)
                    {
                        var elements = RevitFunctions.GetElementsOnRow(_doc, vs, rInd);
                        if (elements == null) continue;
                        switch (operation.Name)
                        {
                            case Operation.CopyString:
                                if (operation is CopyStringOperation stringOperation) CopyStringValues(stringOperation, tsDada, rInd, elements);
                                break;
                            case Operation.CopyInteger:
                                if (operation is CopyIntegerOperation integerOperation) CopyIntegerValues(integerOperation, tsDada, rInd, elements);
                                break;
                            case Operation.CopyDouble:
                                break;
                            case Operation.CopyArea:
                                break;
                            case Operation.CopyVolume:
                                break;
                            case Operation.CopyTemperature:
                                break;
                            case Operation.CopyMass:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
            });
        }

        private static void CopyStringValues(CopyStringOperation stringOperation, TableSectionData tsDada, int rInd, List<Element> elements)
        {
            throw new NotImplementedException();
        }

        private static void CopyIntegerValues(CopyIntegerOperation operation, TableSectionData tsDada, int row, IEnumerable<Element> elements)
        {
            var reserve = 1;
            if (!string.IsNullOrEmpty(operation.ReserveParameter))
            {
                if (RevitFunctions.TryGetGlobalReserveValue(_doc, operation.ReserveParameter, out int outValue)) reserve = outValue;
            }
            else
            {
                reserve = operation.Reserve;
            }

            var value = 1;
            var column = operation.SourceColumn - 1;
            if (column >= 0)
            {
                if (tsDada.NumberOfColumns < column)
                {
                    var data = tsDada.GetCellText(row, column);
                    if (int.TryParse(data, out var outValue)) value = outValue;
                }
            }
            else
            {
                value = operation.IntegerValue;
            }

            RevitFunctions.CopyIntegerValue(_doc, operation.Parameter, value * reserve, elements);
        }
    }
}