using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using AdskTemplateMepTools.Commands.AutoNumerate;
using AdskTemplateMepTools.Commands.CheckADSK;
using AdskTemplateMepTools.Commands.CopyADSK.Commands;
using AdskTemplateMepTools.Commands.CreateDuctSystemViews;
using AdskTemplateMepTools.Commands.CreatePipeSystemViews;
using AdskTemplateMepTools.Commands.CreateSpaces;
using Autodesk.Revit.UI;
using IniParser;
using IniParser.Model;
using AdskTemplateMepTools.Commands;
using AdskTemplateMepTools.Commands.CopyADSK;

namespace AdskTemplateMepTools
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            Configuration.Init();
            
            var panel = CreateRibbonTab(application);
            
            Configuration.TryReadKey(nameof(AutoNumerate), "Tab visibility",out var tabVisible);
            if (bool.Parse(tabVisible))
            {
                if (panel.AddItem(new PushButtonData(nameof(AutoNumerate), "Автонумерация",
                        Assembly.GetExecutingAssembly().Location, typeof(AutoNumerate).FullName)) is PushButton
                    buttonAutoNumerate)
                {
                    buttonAutoNumerate.ToolTip = "Автонумерация спецификаций в общий параметр ADSK_Позиция";
                    buttonAutoNumerate.Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/AutoNumerate16.png"));
                    buttonAutoNumerate.LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/AutoNumerate32.png"));
                }
            }
            
            Configuration.TryReadKey(nameof(CheckAdsk), "Tab visibility", out tabVisible);
            if (bool.Parse(tabVisible))
            {
                if (panel.AddItem(new PushButtonData(nameof(CheckAdsk), "Проверить\nсемейства",
                    Assembly.GetExecutingAssembly().Location, typeof(CheckAdsk).FullName)) is PushButton buttonCheckAdsk)
                {
                    buttonCheckAdsk.ToolTip =
                        "Проверка файлов семейств на соответствие требованиям по общим параметрам ADSK";
                    buttonCheckAdsk.Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CheckADSK16.png"));
                    buttonCheckAdsk.LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CheckADSK32.png"));
                }
            }
            
            Configuration.TryReadKey(nameof(CopyAdsk), "Tab visibility", out tabVisible);
            if (bool.Parse(tabVisible))
            {
                var buttonCopyAdsk = new PushButtonData(nameof(CopyAdsk), "Копировать\nзначения",
                    Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName)
                {
                    ToolTip = "Копирование параметров ADSK в спецификациях",
                    Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CopyADSK16.png")),
                    LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CopyADSK32.png"))
                };
                var buttonSettingsCopyAdsk = new PushButtonData(nameof(CopyAdskSettings), "Настройка\nкопирования",
                    Assembly.GetExecutingAssembly().Location, typeof(CopyAdskSettings).FullName)
                {
                    ToolTip = "Настройка параметров копирования",
                    Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/SettingsCopyADSK16.png")),
                    LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/SettingsCopyADSK32.png"))
                };
                var splitCopyAdsk = new SplitButtonData("SplitCopyADSK", "Меню");
                if (panel.AddItem(splitCopyAdsk) is SplitButton splitBtn)
                {
                    splitBtn.AddPushButton(buttonCopyAdsk);
                    splitBtn.AddPushButton(buttonSettingsCopyAdsk);
                }
            }
            
            Configuration.TryReadKey(nameof(CreateDuctSystemViews), "Tab visibility", out tabVisible);
            if (bool.Parse(tabVisible))
            {
                if (panel.AddItem(new PushButtonData(nameof(CreateDuctSystemViews),
                        "Создать системы\nвоздуховодов",
                        Assembly.GetExecutingAssembly().Location, typeof(CreateDuctSystemViews).FullName)) is PushButton
                    buttonCreateDuctSystemView)
                {
                    buttonCreateDuctSystemView.ToolTip =
                        "Дублирования значений Имя системы для воздушных систем в проекте. Создание копий видов с фильтрами для данных систем";
                    buttonCreateDuctSystemView.Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateDuctSystemViews16.png"));
                    buttonCreateDuctSystemView.LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateDuctSystemViews32.png"));
                }
            }
            
            Configuration.TryReadKey(nameof(CreatePipeSystemViews), "Tab visibility", out tabVisible);
            if (bool.Parse(tabVisible))
            {
                if (panel.AddItem(new PushButtonData(nameof(CreatePipeSystemViews),
                        "Создать системы\nтрубопроводов",
                        Assembly.GetExecutingAssembly().Location, typeof(CreatePipeSystemViews).FullName)) is PushButton
                    buttonCreatePipeSystemView)
                {
                    buttonCreatePipeSystemView.ToolTip =
                        "Дублирования значений Имя системы для трубопроводных систем в проекте. Создание копий видов с фильтрами для данных систем";
                    buttonCreatePipeSystemView.Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreatePipeSystemViews16.png"));
                    buttonCreatePipeSystemView.LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreatePipeSystemViews32.png"));
                }
            }
            
            Configuration.TryReadKey(nameof(CreateSpaces), "Tab visibility", out tabVisible);
            if (bool.Parse(tabVisible))
            {
                if (panel.AddItem(new PushButtonData(nameof(CreateSpaces), "Создать\nпространства",
                        Assembly.GetExecutingAssembly().Location, typeof(CreateSpaces).FullName)) is PushButton
                    buttonCreateSpaces)
                {
                    buttonCreateSpaces.ToolTip =
                        "Создание и автоматическое копирования значений параметров из архитектурных помещений в инженерные пространства. Используется экземпляр размещенной связи АР.";
                    buttonCreateSpaces.Image = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateSpaces16.png"));
                    buttonCreateSpaces.LargeImage = new BitmapImage(new Uri(
                        "pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateSpaces32.png"));
                }
            }

            return Result.Succeeded;
        }

        private static RibbonPanel CreateRibbonTab(UIControlledApplication application)
        {
            Configuration.TryReadKey("Application", "Ribbon tab name", out var tabName);
            if (string.IsNullOrEmpty(tabName)) return application.CreateRibbonPanel("Шаблон ADSK");
            application.CreateRibbonTab(tabName);
            return application.CreateRibbonPanel(tabName, "Шаблон ADSK");
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}