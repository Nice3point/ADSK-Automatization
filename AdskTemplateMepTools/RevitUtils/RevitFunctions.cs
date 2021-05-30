using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AdskTemplateMepTools.RevitUtils
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

        public static void CopyStringValue(Document doc, string parameterName, string value, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Копирование текста", () =>
            {
                foreach (var curElement in copiedElements) GetParameter(curElement, parameterName)?.Set(value);
            });
        }

        public static void CopyIntegerValue(Document doc, string parameterName, int value, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Копирование целых чисел", () =>
            {
                foreach (var curElement in copiedElements) GetParameter(curElement, parameterName)?.Set(value);
            });
        }

        public static void CopyLengthValue(Document doc, string parameterName, double reserve, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Заполнение длины в метрах", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    var parameter = curElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                    if (parameter is not {StorageType: StorageType.Double}) continue;
                    var value = parameter.AsDouble();
                    var length = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Meters) * reserve;
                    GetParameter(curElement, parameterName)?.Set(length);
                }
            });
        }

        public static void CopyAreaValue(Document doc, string parameterName, double reserve, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Заполнение площади в квадратных метрах", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    var parameter = curElement.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                    if (parameter is not {StorageType: StorageType.Double}) continue;
                    var value = parameter.AsDouble();
                    var length = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.SquareMeters) * reserve;
                    GetParameter(curElement, parameterName)?.Set(length);
                }
            });
        }

        public static void CopyVolumeValue(Document doc, string parameterName, double reserve, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Заполнение объема в кубических метрах", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    var parameter = curElement.get_Parameter(BuiltInParameter.RBS_INSULATION_LINING_VOLUME);
                    if (parameter is not {StorageType: StorageType.Double}) continue;
                    var value = parameter.AsDouble();
                    var length = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.CubicMeters) * reserve;
                    GetParameter(curElement, parameterName)?.Set(length);
                }
            });
        }

        public static void CopyMassValue(Document doc, string parameterName, double value, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Заполнение массы в килограммах", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    var length = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Kilograms);
                    GetParameter(curElement, parameterName)?.Set(length);
                }
            });
        }

        public static void CopyTemperatureValue(Document doc, string parameterName, IEnumerable<Element> copiedElements)
        {
            TransactionManager.CreateTransaction(doc, "Заполнение температуры трубы в Кельвинах", () =>
            {
                foreach (var curElement in copiedElements)
                {
                    var pipe = curElement as Pipe;
                    if (null == pipe) continue;
                    var systemType = doc.GetElement(pipe.MEPSystem.GetTypeId()) as PipingSystemType;
                    if (systemType == null) continue;
                    var temperature = UnitUtils.ConvertFromInternalUnits(systemType.FluidTemperature, UnitTypeId.Kelvin);
                    GetParameter(curElement, parameterName)?.Set(temperature);
                }
            });
        }

        private static GlobalParameter GetGlobalParameter(Document doc, string name)
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