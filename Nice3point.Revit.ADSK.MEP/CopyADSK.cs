using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP
{
    [Transaction(TransactionMode.Manual)]
    public class CopyAdsk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            var viewSchedules = new List<string>
            {
                "В_ОВ_Гибкие воздуховоды",
                "В_ОВ_Изоляция воздуховодов",
                "В_ОВ_Круглые воздуховоды",
                "В_ОВ_Прямоугольные воздуховоды",
                "В_ОВ_Фасонные детали воздуховодов",
                "В_ОВ_Гибкие трубы",
                "В_ОВ_Изоляция труб",
                "В_ОВ_Трубопроводы",
                "В_ТМ_Гибкие воздуховоды",
                "В_ТМ_Изоляция воздуховодов",
                "В_ТМ_Круглые воздуховоды",
                "В_ТМ_Прямоугольные воздуховоды",
                "В_ТМ_Фасонные детали воздуховодов",
                "В_ТМ_Гибкие трубы",
                "В_ТМ_Изоляция труб",
                "В_ТМ_Трубопроводы",
                "В_ТМ_Технико-экономические показатели",
                "В_ГСВ_Гибкие трубы",
                "В_ГСВ_Изоляция труб",
                "В_ГСВ_Трубопроводы",
                "В_ГСВ_Технико-экономические показатели"
            };
            foreach (var curViewSchedule in viewSchedules.Select(vSchedule => new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .FirstOrDefault(vs => vs.Name.Equals(vSchedule))).OfType<ViewSchedule>())
            {
                uiDoc.ActiveView = curViewSchedule;
                CopyToAdsk(doc, curViewSchedule);
                foreach (var uiView in uiDoc.GetOpenUIViews())
                    if (curViewSchedule.Id == uiView.ViewId)
                        uiView.Close();
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
                new TransactionGroup(doc, "Заполнение данных ADSK_Наименование для спецификации: " + vs.Name);
            tGroup.Start();
            for (var rInd = 0; rInd < tsDada.NumberOfRows; rInd++)
            {
                var elementsOnRow = CommonFunctions.GetElementsOnRow(doc, vs, rInd);
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

        private static void CopyLengthValue(Document doc, bool copyComm, string commentValue, List<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество и ADSK_Примечание");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                len = UnitUtils.ConvertFromInternalUnits(len, UnitTypeId.Meters) * GetPercentGlobal(doc);
                if (curElement is FlexPipe fp)
                {
                    if (doc.GetElement(fp.GetTypeId()) is FlexPipeType fpt &&
                        Math.Abs(fpt.LookupParameter("Тип трубопровода").AsDouble() - 3) < 0.1)
                        curElement.get_Parameter(AdskGuid.AdskQuantity)
                            .Set(1.0); // Заполнение параметра ADSK_Количество для гибкой трубы - Гибкая подводка.
                    else
                        curElement.get_Parameter(AdskGuid.AdskQuantity)
                            .Set(len); // Заполнение параметра ADSK_Количество для любых других гибких труб.
                }
                else
                {
                    curElement.get_Parameter(AdskGuid.AdskQuantity)
                        .Set(len); // Заполнение параметра ADSK_Количество для линейных элементов, трубы и воздуховоды
                }

                if (copyComm)
                    curElement.get_Parameter(AdskGuid.AdskNote)
                        .Set(commentValue); // Заполнение параметра ADSK_Примечание, площадь для воздуховодов
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
                len = UnitUtils.ConvertFromInternalUnits(len, UnitTypeId.CubicMeters);
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
                len = UnitUtils.ConvertFromInternalUnits(len, UnitTypeId.SquareMeters);
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
                try
                {
                    curElement.get_Parameter(AdskGuid.AdskMassDimension)
                        .Set(double.Parse(doubleValueData)); // Заполнение параметра ADSK_Масса элемента
                }
                catch (Exception)
                {
                    // ignored
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
                curElement.LookupParameter("Температура трубопровода")
                    .Set(UnitUtils.ConvertFromInternalUnits(temperature, UnitTypeId.Kelvin));
            }

            tr.Commit();
        }
    }
}