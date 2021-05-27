using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AdskTemplateMepTools.Commands.AutoNumerate
{
    [Transaction(TransactionMode.Manual)]
    public class AutoNumerate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            if (doc.ActiveView.ViewType == ViewType.Schedule)
            {
                var td = new TaskDialog("Auto-numbering")
                {
                    Id = "ID_TaskDialog_Auto-numbering",
                    MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                    Title = "Автонумерация позиции",
                    TitleAutoPrefix = false,
                    AllowCancellation = true,
                    MainInstruction =
                        "Для автонумерации позиции могут использоваться номер строки или индекс вложенных семейств",
                    MainContent = "Выберите способ автонумерации:"
                };

                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "По строке элемента в спецификации");
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "По вложенным семействам");
                var tdRes = td.Show();
                var sub = tdRes switch
                {
                    TaskDialogResult.CommandLink1 => false,
                    TaskDialogResult.CommandLink2 => true,
                    _ => false
                };

                if (!(uiDoc.ActiveView is ViewSchedule locVs)) return Result.Succeeded;
                var tableData = locVs.GetTableData();
                var tableSectionData = tableData.GetSectionData(SectionType.Body);
                if (tableSectionData.NumberOfRows <= 0) return Result.Failed;
                var startIndex = 1; //Стартовый значение для номера

                using var tGroup = new TransactionGroup(doc, "Автонумерация спецификации: " + locVs.Name);
                tGroup.Start();
                for (var rInd = 0; rInd < tableSectionData.NumberOfRows; rInd++)
                {
                    var elementsOnRow = RevitFunctions.GetElementsOnRow(doc, locVs, rInd);
                    if (null == elementsOnRow) continue;
                    if (sub)
                        startIndex = SetNum(doc, startIndex, elementsOnRow, true);
                    else
                        SetNum(doc, startIndex++, elementsOnRow, false);
                }

                tGroup.Assimilate();
            }
            else
            {
                TaskDialog.Show("Предупреждение", "Для автонумерации требуется открыть спецификацию!");
            }

            return Result.Succeeded;
        }

        private static int SetNum(Document doc, int num, IReadOnlyCollection<Element> elements, bool sub)
        {
            var hostFrames = false;
            var system = false;
            var upped = false;
            var pos = num;
            using var tr = new Transaction(doc, "Задание номера позиции элементам");
            tr.Start();
            if (!sub)
            {
                foreach (var curElement in elements)
                    if (doc.GetElement(curElement.Id) is FamilyInstance curFi)
                    {
                        if (curFi.GetSubComponentIds().Count > 0 && null == curFi.SuperComponent)
                        {
                            curElement.get_Parameter(SpfGuids.AdskPosition)
                                .Set(num.ToString()); // Заполнение параметра ADSK_Позиция
                            hostFrames = true;
                        }
                        else if (!hostFrames)
                        {
                            curElement.get_Parameter(SpfGuids.AdskPosition)
                                .Set(num.ToString()); // Заполнение параметра ADSK_Позиция
                        }
                    }
                    else
                    {
                        curElement.get_Parameter(SpfGuids.AdskPosition)
                            .Set(num.ToString()); // Заполнение параметра ADSK_Позиция
                    }
            }
            else
            {
                foreach (var curElement in elements)
                    if (doc.GetElement(curElement.Id) is FamilyInstance curFi)
                    {
                        if (curFi.GetSubComponentIds().Count > 0 && null == curFi.SuperComponent)
                        {
                            curElement.get_Parameter(SpfGuids.AdskPosition)
                                .Set(pos.ToString()); // Заполнение параметра ADSK_Позиция
                            var subNum = 1;
                            foreach (var subElementId in curFi.GetSubComponentIds())
                            {
                                Element subElement = doc.GetElement(subElementId) as FamilyInstance;
                                subElement?.get_Parameter(SpfGuids.AdskPosition)
                                    .Set(pos + "." + subNum); // Заполнение параметра ADSK_Позиция
                                subNum++;
                            }

                            hostFrames = true;
                        }
                        else if (null == curFi.SuperComponent)
                        {
                            curElement.get_Parameter(SpfGuids.AdskPosition)
                                .Set(num.ToString()); // Заполнение параметра ADSK_Позиция
                            var super = elements.Cast<FamilyInstance>()
                                .Count(e => e.GetSubComponentIds().Count > 0);
                            if (super != 0 || upped) continue;
                            pos++;
                            upped = true;
                        }
                    }
                    else
                    {
                        curElement.get_Parameter(SpfGuids.AdskPosition)
                            .Set(num.ToString()); // Заполнение параметра ADSK_Позиция
                        system = true;
                        //pos++;
                    }

                if (hostFrames | system) pos++;
            }

            tr.Commit();

            return pos;
        }
    }
}