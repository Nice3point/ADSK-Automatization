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
        #region ADSK guids

        private static readonly Guid AdskPosition = new Guid("ae8ff999-1f22-4ed7-ad33-61503d85f0f4"); //Позиция
        private static readonly Guid AdskName = new Guid("e6e0f5cd-3e26-485b-9342-23882b20eb43"); //Наименование
        private static readonly Guid AdskType = new Guid("2204049c-d557-4dfc-8d70-13f19715e46d"); //Тип,Марка
        private static readonly Guid AdskCode = new Guid("2fd9e8cb-84f3-4297-b8b8-75f444e124ed"); //Код оборудования
        private static readonly Guid AdskManufacturer = new Guid("a8cdbf7b-d60a-485e-a520-447d2055f351"); //Завод изготовитель
        private static readonly Guid AdskUnit = new Guid("4289cb19-9517-45de-9c02-5a74ebf5c86d"); //Единица измерения
        private static readonly Guid AdskQuantity = new Guid("8d057bb3-6ccd-4655-9165-55526691fe3a"); //Кол-во
        private static readonly Guid AdskMass = new Guid("32989501-0d17-4916-8777-da950841c6d7"); //Масса единицы
        private static readonly Guid AdskMassDimension = new Guid("5913a1f9-0b38-4364-96fe-a6f3cb7fcc68"); //Масса  с размерностью
        private static readonly Guid AdskNote = new Guid("a85b7661-26b0-412f-979c-66af80b4b2c3"); //Примечание

        #endregion

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            var viewSchedules = new List<string>
            {
                "В_ТМ_Гибкие воздуховоды",
                "В_ТМ_Изоляция воздуховодов",
                "В_ТМ_Круглые воздуховоды",
                "В_ТМ_Прямоугольные воздуховоды",
                "В_ТМ_Фасонные детали воздуховодов",
                "В_ТМ_Гибкие трубы",
                "В_ТМ_Изоляция труб",
                "В_ТМ_Трубопроводы",
                "В_ТМ_Технико-экономические показатели"
            };
            foreach (var curViewSchedule in viewSchedules.Select(vSchedule => new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .FirstOrDefault(vs => vs.Name.Equals(vSchedule))).OfType<ViewSchedule>())
            {
                uiDoc.ActiveView = curViewSchedule;
                CopyToAdsk(doc, curViewSchedule);
                foreach (var uiView in uiDoc.GetOpenUIViews())
                {
                    if (curViewSchedule.Id == uiView.ViewId)
                    {
                        uiView.Close();
                    }
                }
            }

            return Result.Succeeded;
        }

        private void CopyToAdsk(Document doc, ViewSchedule vs)
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

            if (vs.Name.Equals("В_ТМ_Технико-экономические показатели"))
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
                var elementsOnRow = GetElementsOnRow(doc, vs, rInd);
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
            var percentValue = new FilteredElementCollector(doc).OfClass(typeof(GlobalParameter))
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
                curElement.get_Parameter(AdskName).Set(valueData); // Заполнение параметра ADSK_Наименование
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
                        curElement.get_Parameter(AdskQuantity)
                            .Set(1.0); // Заполнение параметра ADSK_Количество для гибкой трубы - Гибкая подводка.
                    else
                        curElement.get_Parameter(AdskQuantity)
                            .Set(len); // Заполнение параметра ADSK_Количество для любых других гибких труб.
                }
                else
                {
                    curElement.get_Parameter(AdskQuantity)
                        .Set(len); // Заполнение параметра ADSK_Количество для линейных элементов, трубы и воздуховоды
                }

                if (copyComm)
                    curElement.get_Parameter(AdskNote)
                        .Set(commentValue); // Заполнение параметра ADSK_Примечание, площадь для воздуховодов
            }

            tr.Commit();
        }

        private static void CopyVolumeValue(Document doc, List<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.RBS_INSULATION_LINING_VOLUME).AsDouble();
                len = UnitUtils.ConvertFromInternalUnits(len, UnitTypeId.CubicMeters);
                curElement.get_Parameter(AdskQuantity).Set(len); // Заполнение параметра ADSK_Количество
            }

            tr.Commit();
        }

        private static void CopyAreaValue(Document doc, List<Element> elements)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in elements)
            {
                var len = curElement.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA).AsDouble();
                len = UnitUtils.ConvertFromInternalUnits(len, UnitTypeId.SquareMeters);
                curElement.get_Parameter(AdskQuantity).Set(len); // Заполнение параметра ADSK_Количество
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
                    curElement.get_Parameter(AdskMassDimension)
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

        private static List<Element> GetElementsOnRow(Document doc, ViewSchedule vs, int rowNumber)
        {
            var tableData = vs.GetTableData();
            var tableSectionData = tableData.GetSectionData(SectionType.Body);
            var elemIds = new FilteredElementCollector(doc, vs.Id).ToElementIds().ToList();
            List<ElementId> remainingElementsIds;

            using (var t = new Transaction(doc, "Empty"))
            {
                t.Start();
                using (var st = new SubTransaction(doc))
                {
                    st.Start();
                    try
                    {
                        tableSectionData.RemoveRow(rowNumber);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    st.Commit();
                }

                remainingElementsIds = new FilteredElementCollector(doc, vs.Id).ToElementIds().ToList();
                t.RollBack();
            }

            return elemIds
                .Where(id => !remainingElementsIds.Contains(id))
                .Select(doc.GetElement)
                .ToList();
        }
    }
}