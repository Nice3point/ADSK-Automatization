using System;
using System.Collections.Generic;
using System.Linq;
using AdskTemplateMepTools.Commands;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AdskTemplateMepTools.RevitAPI
{
    public static class RevitFunctions
    {
        public static Parameter GetParameter(Element element, string parameterName)
        {
            var parameter = element.LookupParameter(parameterName);
            if (parameter != null) return parameter;
            var elementType = element.Document.GetElement(element.GetTypeId());
            var typeParameter = elementType?.LookupParameter(parameterName);
            if (typeParameter == null) return null;
            parameter = typeParameter;
            return parameter;
        }
        public static string GetParameterValue(Element element, string parameterName)
        {
            var parameter = GetParameter(element, parameterName);
            if (parameter == null) return null;
            var value = parameter.AsString();
            if (value != null) return value;
            var elementType = element.Document.GetElement(element.GetTypeId());
            return elementType?.LookupParameter(parameterName)?.AsString();
        }
        public static void CopySystemNameValue(Document doc, IEnumerable<Element> elements)
        {
            TransactionManager.CreateTransaction(doc, "Копирование имени систем", () =>
            {
                foreach (var curElement in elements)
                {
                    var rbsName = curElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)?.AsString();
                    if (curElement is FamilyInstance fInstance)
                    {
                        if (null != fInstance.SuperComponent)
                        {
                            rbsName = fInstance.SuperComponent.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)?.AsString();
                            fInstance.LookupParameter("ИмяСистемы")?.Set(rbsName);
                        }
                        else
                        {
                            fInstance.LookupParameter("ИмяСистемы")?.Set(rbsName);
                        }
                    }
                    else
                    {
                        curElement.LookupParameter("ИмяСистемы")?.Set(rbsName);
                    }
                }

            });
        }

        public static void CopyIntegerValue(Document doc, string parameterName, int value, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Копирование целых чисел", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    GetParameter(curElement, parameterName)?.Set(value);
                }
            });
        }

        public static void CopyTemperature(Document doc, IEnumerable<Element> copiedElements, string parameterName)
        {
            using var tr = new Transaction(doc, $"Заполнение значений {parameterName}");
            tr.Start();
            foreach (var curElement in copiedElements)
            {
                var pipe = curElement as Pipe;
                if (null == pipe) continue;
                var systemType = doc.GetElement(pipe.MEPSystem.GetTypeId()) as PipingSystemType;
                if (systemType == null) continue;
                var temperature = systemType.FluidTemperature;
                var temperatureParam = curElement.LookupParameter("Температура трубопровода");
                temperatureParam?.Set(UnitUtils.ConvertFromInternalUnits(temperature, UnitTypeId.Kelvin));
            }

            tr.Commit();
        }

        public static void CopyAdskName(Document doc, IEnumerable<Element> copiedElements, string copiedData)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Наименование");
            tr.Start();
            foreach (var curElement in copiedElements)
            {
                var parameter = curElement.get_Parameter(SpfGuids.AdskName);
                if (parameter == null) continue;
                if (parameter.IsReadOnly) continue;
                parameter.Set(copiedData);
            }

            tr.Commit();
        }

        public static void CopyCommentValue(Document doc, IEnumerable<Element> copiedElements, string copiedData)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Примечание");
            tr.Start();
            foreach (var curElement in copiedElements) curElement.get_Parameter(SpfGuids.AdskNote)?.Set(copiedData);
            tr.Commit();
        }

        public static void CopyMass(Document doc, IEnumerable<Element> copiedElements, string copiedData)
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Масса элемента");
            tr.Start();
            foreach (var curElement in copiedElements)
            {
                var massParam = curElement.get_Parameter(SpfGuids.AdskMassDimension);
                if (double.TryParse(copiedData.Replace(',', '.'), out var value)) massParam?.Set(UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Kilograms));
            }

            tr.Commit();
        }

        public static void CopyLengthValue(Document doc, IEnumerable<Element> copiedElements, double reserveLength = 1, string reserveParameter = "")
        {
            using var tr = new Transaction(doc, "Заполнение значения ADSK_Количество");
            tr.Start();
            foreach (var curElement in copiedElements) SetBuiltinParameterValue(doc, curElement, BuiltInParameter.CURVE_ELEM_LENGTH, UnitTypeId.Meters, reserveLength, reserveParameter);
            tr.Commit();
        }

        public static void CopyVolumeValue(Document doc, IEnumerable<Element> copiedElements, double reserveLength = 1, string reserveParameter = "")
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in copiedElements)
                SetBuiltinParameterValue(doc, curElement, BuiltInParameter.RBS_INSULATION_LINING_VOLUME, UnitTypeId.CubicMeters, reserveLength, reserveParameter);
            tr.Commit();
        }

        public static void CopyAreaValue(Document doc, IEnumerable<Element> copiedElements, double reserveLength = 1, string reserveParameter = "")
        {
            using var tr = new Transaction(doc, "Заполнение значений ADSK_Количество");
            tr.Start();
            foreach (var curElement in copiedElements) SetBuiltinParameterValue(doc, curElement, BuiltInParameter.RBS_CURVE_SURFACE_AREA, UnitTypeId.SquareMeters, reserveLength, reserveParameter);
            tr.Commit();
        }
        public static GlobalParameter GetGlobalParameter(Document doc, string name)
        {
            return new FilteredElementCollector(doc)
                        .OfClass(typeof(GlobalParameter))
                        .Cast<GlobalParameter>()
                        .FirstOrDefault(gp => gp.Name.Equals(name));
            
        }
        public static bool TryGetGlobalReserveValue(Document doc, string name, out double value)
        {
            value = default;
            var reserveValue = GetGlobalParameter(doc, name);
            if (reserveValue?.GetValue() is not DoubleParameterValue doubleParameterValue) return false;
            value = doubleParameterValue.Value;
            return true;
        }
        public static bool TryGetGlobalReserveValue(Document doc, string name, out int value)
        {
            value = default;
            var reserveValue = GetGlobalParameter(doc, name);
            if (reserveValue?.GetValue() is not IntegerParameterValue integerParameterValue) return false;
            value = integerParameterValue.Value;
            return true;
        }


        public static void SetBuiltinParameterValue(Document doc, Element curElement, BuiltInParameter parameter, ForgeTypeId unitType, double reserveLength, string reserveParameter)
        {
            var len = curElement.get_Parameter(parameter).AsDouble();
            if (!TryGetGlobalReserveValue(doc, reserveParameter, out double reserve)) reserve = reserveLength;
            len = UnitUtils.ConvertFromInternalUnits(len, unitType) * reserve;
            curElement.get_Parameter(SpfGuids.AdskQuantity)?.Set(len);
        }

        public static List<Element> GetElementsOnRow(Document doc, ViewSchedule vs, int rowNumber)
        {
            var tableData = vs.GetTableData();
            var tableSectionData = tableData.GetSectionData(SectionType.Body);
            var elemIds = new FilteredElementCollector(doc, vs.Id)
                          .ToElementIds()
                          .ToList();

            using var t = new Transaction(doc, "Получение элементов таблицы");
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

            var remainingElementsIds = new FilteredElementCollector(doc, vs.Id)
                                       .ToElementIds()
                                       .ToList();
            t.RollBack();

            return elemIds
                   .Where(id => !remainingElementsIds.Contains(id))
                   .Select(doc.GetElement)
                   .ToList();
        }
    }
}