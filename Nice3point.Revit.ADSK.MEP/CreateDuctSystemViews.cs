using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP
{
    [Transaction(TransactionMode.Manual)]
    public class CreateDuctSystemViews : IExternalCommand
    {
        private UIDocument _uiDoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uiDoc = commandData.Application.ActiveUIDocument;
            var doc = _uiDoc.Document;
            if (doc.ActiveView.ViewType != ViewType.ProjectBrowser)
            {
                using (var trg = new TransactionGroup(doc, "Копирование значений имя системы"))
                {
                    trg.Start();
                    foreach (var cat in GetDuctCategories())
                    {
                        var elementsByCat = new FilteredElementCollector(doc)
                            .OfCategory(cat)
                            .WhereElementIsNotElementType()
                            .ToList();
                        if (elementsByCat.Count > 0) CommonFunctions.CopySystemNameValue(doc, elementsByCat);
                    }

                    trg.Assimilate();
                }

                var td = new TaskDialog("Copy views")
                {
                    Id = "ID_TaskDialog_Copy_Views",
                    MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                    Title = "Создание копий видов с применением фильтра",
                    TitleAutoPrefix = false,
                    AllowCancellation = true,
                    MainInstruction =
                        "Данные из параметра Имя системы для всех элементов систем воздуховодов скопированы",
                    MainContent = "Хотите создать копии текущего вида с применением фильтров по системам?"
                };

                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Да, создать фильтры и виды");
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Нет");
                var tdRes = td.Show();
                if (tdRes != TaskDialogResult.CommandLink1) return Result.Cancelled;
                var sysNameParamElement = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterElement))
                    .FirstOrDefault(p => p.Name == "ИмяСистемы");
                var sysNameParam = sysNameParamElement as ParameterElement;
                foreach (var systemName in GetDuctSystemNames(doc))
                    CreateFilterForDuctSystem(doc, sysNameParam, systemName);
            }
            else
            {
                TaskDialog.Show("Предупреждение", "Не активирован вид для создания копий с применением фильтра");
            }

            return Result.Succeeded;
        }

        private static IEnumerable<string> GetDuctSystemNames(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DuctSystem)
                .WhereElementIsNotElementType()
                .Select(s => s.Name)
                .ToList();
        }

        private void CreateFilterForDuctSystem(Document doc, Element sysNameParam, string systemName)
        {
            using var tr = new Transaction(doc, "Создание фильтра для: " + systemName);
            tr.Start();
            var view = _uiDoc.ActiveView;
            var categories = new List<ElementId>
            {
                new ElementId(BuiltInCategory.OST_DuctAccessory),
                new ElementId(BuiltInCategory.OST_DuctCurves),
                new ElementId(BuiltInCategory.OST_DuctFitting),
                new ElementId(BuiltInCategory.OST_DuctInsulations),
                new ElementId(BuiltInCategory.OST_DuctTerminal),
                new ElementId(BuiltInCategory.OST_FlexDuctCurves),
                new ElementId(BuiltInCategory.OST_PlaceHolderDucts),
                new ElementId(BuiltInCategory.OST_GenericModel),
                new ElementId(BuiltInCategory.OST_MechanicalEquipment)
            };
            var rule = ParameterFilterRuleFactory.CreateNotContainsRule(sysNameParam.Id, systemName, true);
            var epf = new ElementParameterFilter(rule);
            var ef = (ElementFilter) epf;
            ParameterFilterElement filter;
            try
            {
                filter = ParameterFilterElement.Create(doc, "ADSK_Воздуховод_" + systemName, categories, ef);
            }
            catch (ArgumentException)
            {
                var filter1 = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterFilterElement))
                    .First(f => f.Name == "ADSK_Воздуховод_" + systemName);
                filter = filter1 as ParameterFilterElement;
                filter?.SetElementFilter(ef);
            }

            var eView = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .FirstOrDefault(v => v.Name == $"{view.Name}_{systemName}");
            if (null == eView)
            {
                var copyViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
                if (doc.GetElement(copyViewId) is View copiedView)
                {
                    copiedView.Name = $"{view.Name}_{systemName}";
                    if (filter != null)
                    {
                        copiedView.AddFilter(filter.Id);
                        copiedView.SetFilterVisibility(filter.Id, false);
                    }
                }
            }

            tr.Commit();
        }

        private static IEnumerable<BuiltInCategory> GetDuctCategories()
        {
            var cats = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_PlaceHolderDucts,
                BuiltInCategory.OST_MechanicalEquipment
            };
            return cats;
        }
    }
}