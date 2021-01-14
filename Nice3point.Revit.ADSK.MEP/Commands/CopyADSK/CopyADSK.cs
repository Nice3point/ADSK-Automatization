using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP.Commands.CopyADSK
{
    [Transaction(TransactionMode.Manual)]
    public class CopyAdsk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            var settings = new CopyAdskSettings();
            settings.CreateInstance(commandData);
            var viewModel = new CopyAdskSettingsViewModel(settings);
            var schedules = viewModel.Schedules    
                .Select(vSchedule => new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .FirstOrDefault(vs => vs.Name.Equals(vSchedule))).OfType<ViewSchedule>();
            foreach (var view in schedules)
            {
                uiDoc.ActiveView = view;
                CopyToAdsk(doc, view);
                var openViews = uiDoc.GetOpenUIViews();
                var countOpenView = uiDoc.GetOpenUIViews().Count;
                foreach (var uiView in openViews)
                {
                    if (countOpenView==1) break;
                    if (view.Id == uiView.ViewId) uiView.Close();
                    countOpenView -= 1;
                }
            }

            return Result.Succeeded;
        }

        private static void CopyToAdsk(Document doc, ViewSchedule vs)
        {
            var copyLength = false;
            var copyVolume = false;
            var copyArea = false;
            var copyComment = false;
            var copyMass = false;
            var copyValue = true;
            var copyTemperature = false;
            if ((vs.Definition.CategoryId.IntegerValue == new ElementId(BuiltInCategory.OST_PipeCurves).IntegerValue) |
                (vs.Definition.CategoryId.IntegerValue == new ElementId(BuiltInCategory.OST_DuctCurves).IntegerValue) |
                (vs.Definition.CategoryId.IntegerValue ==
                 new ElementId(BuiltInCategory.OST_FlexDuctCurves).IntegerValue) |
                (vs.Definition.CategoryId.IntegerValue ==
                 new ElementId(BuiltInCategory.OST_FlexPipeCurves).IntegerValue))
            {
                copyLength = true;
                copyComment = true;
            }

            if (vs.Definition.CategoryId.IntegerValue == new ElementId(BuiltInCategory.OST_PipeCurves).IntegerValue)
                copyTemperature = true;

            if (vs.Definition.CategoryId.IntegerValue == new ElementId(BuiltInCategory.OST_PipeInsulations).IntegerValue
            )
                copyVolume = true;

            if (vs.Definition.CategoryId.IntegerValue ==
                new ElementId(BuiltInCategory.OST_DuctInsulations).IntegerValue)
                copyArea = true;

            if (vs.Name.Equals("В_ТМ_Технико-экономические показатели") |
                vs.Name.Equals("В_ГСВ_Технико-экономические показатели"))
            {
                copyMass = true;
                copyValue = false;
            }

            var tData = vs.GetTableData();
            var tsDada = tData.GetSectionData(SectionType.Body);
            using var tGroup =
                new TransactionGroup(doc, "Заполнение ADSK_Наименование в спецификациях");
            tGroup.Start();
            for (var rInd = 0; rInd < tsDada.NumberOfRows; rInd++)
            {
                var elementsOnRow = RevitFunctions.GetElementsOnRow(doc, vs, rInd);
                if (null == elementsOnRow) continue;
                string dataValue;
                string commentValue;
                try
                {
                    dataValue = tsDada.GetCellText(rInd, 1);
                    commentValue = tsDada.GetCellText(rInd, tsDada.LastColumnNumber);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка",
                        vs.Name + "\nInd: " + rInd + " of " + tsDada.NumberOfRows + "\n" + ex.Message);
                    return;
                }

                if (copyValue) SetValue(doc, dataValue, elementsOnRow);
                if (copyLength) CopyLengthValue(doc, copyComment, commentValue, elementsOnRow);
                if (copyMass) CopyMass(doc, dataValue, elementsOnRow);
                if (copyTemperature) CopyTemperature(doc, elementsOnRow);
                if (copyVolume) CopyVolumeValue(doc, elementsOnRow);
                if (copyArea) CopyAreaValue(doc, elementsOnRow);
            }

            tGroup.Assimilate();
        }

        private static double GetPercentGlobal(Document doc)
        {
            double percent = 1;
            var percentValue = new FilteredElementCollector(doc)
                .OfClass(typeof(GlobalParameter))
                .Cast<GlobalParameter>()
                .FirstOrDefault(gp => gp.Name.Equals("Спецификация_ЗапасДлины"));
            if (percentValue == null) return percent;
            if (percentValue.GetValue() is DoubleParameterValue dVal) percent = dVal.Value;
            return percent;
        }

        private static void SetValue(Document doc, string valueData, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Наименование");
            tr.Start();
            foreach (var curElement in elements)
                curElement.get_Parameter(AdskGuid.AdskName).Set(valueData); // Заполнение параметра ADSK_Наименование
            tr.Commit();
        }

        private static void CopyLengthValue(Document doc, bool copyComm, string commentValue, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество и ADSK_Примечание");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                var quantityParam = curElement.get_Parameter(AdskGuid.AdskQuantity);
                len = UnitUtils.ConvertFromInternalUnits(len, DisplayUnitType.DUT_METERS) * GetPercentGlobal(doc);
                if (curElement is FlexPipe fp)
                {
                    if (doc.GetElement(fp.GetTypeId()) is FlexPipeType fpt &&
                        Math.Abs(fpt.LookupParameter("Тип трубопровода").AsDouble() - 3) < 0.1)
                        quantityParam?.Set(1.0); // Заполнение параметра ADSK_Количество для гибкой трубы - Гибкая подводка.
                    else
                        quantityParam?.Set(len); // Заполнение параметра ADSK_Количество для любых других гибких труб.
                }
                else
                {
                    quantityParam?.Set(len); // Заполнение параметра ADSK_Количество для линейных элементов, трубы и воздуховоды
                }

                if (copyComm)
                {
                    var noteParam = curElement.get_Parameter(AdskGuid.AdskNote);
                    noteParam?.Set(commentValue); // Заполнение параметра ADSK_Примечание, площадь для воздуховодов
                }
            }

            tr.Commit();
        }

        private static void CopyVolumeValue(Document doc, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.RBS_INSULATION_LINING_VOLUME).AsDouble();
                len = UnitUtils.ConvertFromInternalUnits(len, DisplayUnitType.DUT_CUBIC_METERS);
                curElement.get_Parameter(AdskGuid.AdskQuantity).Set(len); // Заполнение параметра ADSK_Количество
            }

            tr.Commit();
        }

        private static void CopyAreaValue(Document doc, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA).AsDouble();
                len = UnitUtils.ConvertFromInternalUnits(len, DisplayUnitType.DUT_SQUARE_METERS);
                curElement.get_Parameter(AdskGuid.AdskQuantity).Set(len); // Заполнение параметра ADSK_Количество
            }

            tr.Commit();
        }

        private static void CopyMass(Document doc, string valueData, IEnumerable<Element> elements)
        {
            var doubleValueData = valueData.Contains(',') ? valueData.Replace(',', '.') : valueData;
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Масса элемента");
            tr.Start();
            foreach (var curElement in elements)
            {
                var massParam = curElement.get_Parameter(AdskGuid.AdskMassDimension);
                    massParam?.Set(double.Parse(doubleValueData)); // Заполнение параметра ADSK_Масса элемента
            }
            tr.Commit();
        }

        private static void CopyTemperature(Document doc, IEnumerable<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений Температура трубопровода");
            tr.Start();
            foreach (var curElement in elements)
            {
                var pipe = curElement as Pipe;
                if (null == pipe) continue;
                var systemType = doc.GetElement(pipe.MEPSystem.GetTypeId()) as PipingSystemType;
                if (systemType == null) continue;
                var temperature = systemType.FluidTemperature;
                var temperatureParam = curElement.LookupParameter("Температура трубопровода");
                temperatureParam?.Set(UnitUtils.ConvertFromInternalUnits(temperature, DisplayUnitType.DUT_KELVIN));
            }

            tr.Commit();
        }
    }
}