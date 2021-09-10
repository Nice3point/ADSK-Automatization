using System;
using System.Collections.Generic;
using System.Linq;
using AdskTemplateMepTools.Commands.CopyADSK.Operations;
using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;
using AdskTemplateMepTools.RevitUtils;
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

        private static void CopyToAdsk(ViewSchedule vs, IReadOnlyCollection<IOperation> operations)
        {
            var tData = vs.GetTableData();
            var tsDada = tData.GetSectionData(SectionType.Body);
            if (operations.Count == 0) return;
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
                            case Operation.CopyLength:
                                if (operation is CopyLengthOperation lengthOperation) CopyLengthValues(lengthOperation, elements);
                                break;
                            case Operation.CopyArea:
                                if (operation is CopyAreaOperation areaOperation) CopyAreaValues(areaOperation, elements);
                                break;
                            case Operation.CopyVolume:
                                if (operation is CopyVolumeOperation volumeOperation) CopyVolumeValues(volumeOperation, elements);
                                break;
                            case Operation.CopyMass:
                                if (operation is CopyMassOperation massOperation) CopyMassValues(massOperation, tsDada, rInd, elements);
                                break;
                            case Operation.CopyTemperature:
                                if (operation is CopyTemperatureOperation temperatureOperation) CopyTemperatureValues(temperatureOperation, elements);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
            });
        }

        private static void CopyStringValues(CopyStringOperation operation, TableSectionData tsDada, int row, List<Element> elements)
        {
            var value = string.Empty;
            var column = operation.SourceColumn - 1;
            if (column >= 0)
            {
                if (column < tsDada.NumberOfColumns) value = tsDada.GetCellText(row, column);
            }
            else
            {
                value = operation.Value;
            }

            RevitFunctions.CopyStringValue(_doc, operation.Parameter, value, elements);
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
                if (column < tsDada.NumberOfColumns)
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

        private static void CopyLengthValues(CopyLengthOperation operation, IEnumerable<Element> elements)
        {
            var reserve = 1d;
            if (!string.IsNullOrEmpty(operation.ReserveParameter))
            {
                if (RevitFunctions.TryGetGlobalReserveValue(_doc, operation.ReserveParameter, out double outValue)) reserve = outValue;
            }
            else
            {
                reserve = operation.Reserve;
            }

            RevitFunctions.CopyLengthValue(_doc, operation.Parameter, reserve, elements);
        }

        private static void CopyAreaValues(CopyAreaOperation operation, IEnumerable<Element> elements)
        {
            var reserve = 1d;
            if (!string.IsNullOrEmpty(operation.ReserveParameter))
            {
                if (RevitFunctions.TryGetGlobalReserveValue(_doc, operation.ReserveParameter, out double outValue)) reserve = outValue;
            }
            else
            {
                reserve = operation.Reserve;
            }

            RevitFunctions.CopyAreaValue(_doc, operation.Parameter, reserve, elements);
        }

        private static void CopyVolumeValues(CopyVolumeOperation operation, IEnumerable<Element> elements)
        {
            var reserve = 1d;
            if (!string.IsNullOrEmpty(operation.ReserveParameter))
            {
                if (RevitFunctions.TryGetGlobalReserveValue(_doc, operation.ReserveParameter, out double outValue)) reserve = outValue;
            }
            else
            {
                reserve = operation.Reserve;
            }

            RevitFunctions.CopyVolumeValue(_doc, operation.Parameter, reserve, elements);
        }

        private static void CopyMassValues(CopyMassOperation operation, TableSectionData tsDada, int row, IEnumerable<Element> elements)
        {
            var reserve = 1d;
            if (!string.IsNullOrEmpty(operation.ReserveParameter))
            {
                if (RevitFunctions.TryGetGlobalReserveValue(_doc, operation.ReserveParameter, out double outValue)) reserve = outValue;
            }
            else
            {
                reserve = operation.Reserve;
            }

            var value = 0d;
            var column = operation.SourceColumn - 1;
            if (column >= 0)
            {
                if (column < tsDada.NumberOfColumns)
                {
                    var data = tsDada.GetCellText(row, column);
                    if (double.TryParse(data.Replace(',', '.'), out var outValue)) value = outValue;
                }
            }

            RevitFunctions.CopyMassValue(_doc, operation.Parameter, value * reserve, elements);
        }

        private static void CopyTemperatureValues(CopyTemperatureOperation operation, IEnumerable<Element> elements)
        {
            RevitFunctions.CopyTemperatureValue(_doc, operation.Parameter, elements);
        }
    }
}