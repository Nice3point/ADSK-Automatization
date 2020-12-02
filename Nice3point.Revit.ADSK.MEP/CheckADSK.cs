using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;

namespace Nice3point.Revit.ADSK.MEP
{
    [Transaction(TransactionMode.ReadOnly)]
    public class CheckAdsk : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var log = new StringBuilder(
                "Имя семейства;ADSK_Наименование;ADSK_Марка;ADSK_Код изделия;ADSK_Завод-изготовитель;ADSK_Единица измерения;ADSK_Количество;ADSK_Масса\n");
            if (CheckOpenedFamilies(doc))
            {
                var fPath = SelectFolder();
                var logFile = Path.Combine(fPath, "Log_ADSK_Checker.csv");
                if (fPath == "") return Result.Cancelled;
                var lFiles = new List<string>();
                var lFilesBck = new List<string>();
                var files = Directory.GetFiles(fPath, "*.rfa", SearchOption.AllDirectories);
                lFiles.AddRange(files);
                var filesBck = Directory.GetFiles(fPath, "*.????.rfa", SearchOption.AllDirectories);
                lFilesBck.AddRange(filesBck);
                if (files.Length > 0)
                    foreach (var fl in lFiles)
                    {
                        if (lFilesBck.Contains(fl)) continue;
                        var mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(fl);
                        var oOps = new OpenOptions {Audit = true};
                        var famDoc = doc.Application.OpenDocumentFile(mPath, oOps);
                        var fam = famDoc.OwnerFamily;
                        if (fam.FamilyCategory.CategoryType == CategoryType.Model)
                        {
                            var rfaName = Path.GetFileName(fl);
                            log.AppendLine(rfaName + CheckFamily(famDoc));
                        }

                        famDoc.Close(false);
                    }

                var text = log.ToString().Replace(';', '\t');
                using (var sWriter = new StreamWriter(logFile))
                {
                    sWriter.Write(text, Encoding.Unicode);
                }

                var td = new TaskDialog("Check_Family")
                {
                    Id = "ID_TaskDialog_Checked_Families",
                    MainIcon = TaskDialogIcon.TaskDialogIconWarning,
                    Title = "Проверка файлов семейств",
                    TitleAutoPrefix = false,
                    AllowCancellation = true,
                    MainInstruction = "Семейства проверены, файл отчет находится в корневой папке поиска"
                };

                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "ОК");
                td.Show();
            }
            else
            {
                var td = new TaskDialog("Warn_Families")
                {
                    Id = "ID_TaskDialog_Warn_Families",
                    MainIcon = TaskDialogIcon.TaskDialogIconWarning,
                    Title = "Проверка файлов семейств",
                    TitleAutoPrefix = false,
                    AllowCancellation = true,
                    MainInstruction =
                        "Перед пакетной проверкой семейств необходимо закрыть все текущие открытые семейства!"
                };

                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "ОК");
                td.Show();
            }

            return Result.Succeeded;
        }

        private static string SelectFolder()
        {
            var fBrowser = new FolderBrowserDialog {RootFolder = Environment.SpecialFolder.MyComputer};
            var resultDg = fBrowser.ShowDialog();
            return resultDg == DialogResult.OK ? fBrowser.SelectedPath : "";
        }

        private static string CheckFamily(Document doc)
        {
            var famManager = doc.FamilyManager;
            var res = "";
            var listFamParams = famManager.GetParameters()
                .Where(fp => fp.IsShared)
                .ToList();
            var listGuids = listFamParams.Select(p => p.GUID).ToList();
            if (listGuids.Count <= 0) return res;
            res += listGuids.Contains(AdskGuid.AdskName) ? ";+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskType) ? "+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskCode) ? "+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskManufacturer) ? "+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskUnit) ? "+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskQuantity) ? "+;" : "-;";
            res += listGuids.Contains(AdskGuid.AdskMassDimension) ? "+" : "-";

            return res;
        }

        private static bool CheckOpenedFamilies(Document doc)
        {
            var docSet = doc.Application.Documents;
            return docSet.Cast<Document>()
                .All(curDoc => !curDoc.IsFamilyDocument);
        }
    }
}