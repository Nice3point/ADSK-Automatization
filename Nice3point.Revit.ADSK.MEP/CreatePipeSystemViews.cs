using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP
{
    [Transaction(TransactionMode.Manual)]
    public class CreatePipeSystemViews : IExternalCommand
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
                    foreach (var cat in GetPipeCategories())
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
                        "Данные из параметра Имя системы для всех элементов трубопроводных систем скопированы",
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
                foreach (var systemName in GetPipeSystemNames(doc))
                    CreateFilterForPipeSystem(doc, sysNameParam, systemName);
            }
            else
            {
                TaskDialog.Show("Предупреждение", "Не активирован вид для создания копий с применением фильтра");
            }

            return Result.Succeeded;
        }

        private void CreateFilterForPipeSystem(Document doc, Element sysNameParam, string systemName)
        {
            using var tr = new Transaction(doc, "Создание фильтра для: " + systemName);
            tr.Start();
            var view = _uiDoc.ActiveView;
            var categories = new List<ElementId>
            {
                new ElementId(BuiltInCategory.OST_PipeAccessory),
                new ElementId(BuiltInCategory.OST_PipeCurves),
                new ElementId(BuiltInCategory.OST_PipeFitting),
                new ElementId(BuiltInCategory.OST_PipeInsulations),
                new ElementId(BuiltInCategory.OST_PlumbingFixtures),
                new ElementId(BuiltInCategory.OST_FlexPipeCurves),
                new ElementId(BuiltInCategory.OST_PlaceHolderPipes),
                new ElementId(BuiltInCategory.OST_GenericModel),
                new ElementId(BuiltInCategory.OST_MechanicalEquipment),
                new ElementId(BuiltInCategory.OST_Sprinklers)
            };
            var rule = ParameterFilterRuleFactory.CreateNotContainsRule(sysNameParam.Id, systemName, true);
            var epf = new ElementParameterFilter(rule);
            var ef = (ElementFilter) epf;
            ParameterFilterElement filter;
            try
            {
                filter = ParameterFilterElement.Create(doc, "ADSK_Трубопровод_" + systemName, categories, ef);
            }
            catch (ArgumentException)
            {
                var filter1 = new FilteredElementCollector(doc)
                    .OfClass(typeof(ParameterFilterElement))
                    .FirstOrDefault(f => f.Name == "ADSK_Трубопровод_" + systemName);
                filter = filter1 as ParameterFilterElement;
                filter?.SetElementFilter(ef);
            }

            var eView = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .FirstOrDefault(v => v.Name == view.Name + systemName);
            if (null == eView)
            {
                var copyViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
                var copiedView = doc.GetElement(copyViewId) as View;
                if (copiedView != null)
                {
                    copiedView.Name = view.Name + systemName;
                    if (filter != null)
                    {
                        copiedView.AddFilter(filter.Id);
                        copiedView.SetFilterVisibility(filter.Id, false);
                    }
                }
            }

            tr.Commit();
        }

        private static IEnumerable<string> GetPipeSystemNames(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_PipingSystem)
                .WhereElementIsNotElementType()
                .Select(s => s.Name)
                .ToList();
        }

        private static IEnumerable<BuiltInCategory> GetPipeCategories()
        {
            var cats = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_PlaceHolderPipes,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_Sprinklers
            };
            return cats;
        }
    }
}