using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Nice3point.Revit.ADSK.MEP.Commands;

namespace Nice3point.Revit.ADSK.MEP
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var panel = application.CreateRibbonPanel("Шаблон ADSK");

            if (panel.AddItem(new PushButtonData("AutoNumerate", "Автонумерация\nв спецификациях",
                    Assembly.GetExecutingAssembly().Location, typeof(AutoNumerate).FullName)) is PushButton
                buttonAutoNumerate)
            {
                buttonAutoNumerate.ToolTip = "Автонумерация спецификаций в общий параметр ADSK_Позиция";
                buttonAutoNumerate.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/AutoNumerate16.png"));
                buttonAutoNumerate.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/AutoNumerate32.png"));
            }

            if (panel.AddItem(new PushButtonData("CheckADSK", "Проверить\nсемейства",
                Assembly.GetExecutingAssembly().Location, typeof(CheckAdsk).FullName)) is PushButton buttonCheckAdsk)
            {
                buttonCheckAdsk.ToolTip =
                    "Проверка файлов семейств на соответствие требованиям по общим параметрам ADSK";
                buttonCheckAdsk.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CheckADSK16.png"));
                buttonCheckAdsk.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CheckADSK32.png"));
            }

            var buttonCopyAdsk = new PushButtonData("CopyADSK", "Копировать\nзначения",
                Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName)
            {
                ToolTip = "Копирование параметров ADSK в спецификациях",
                Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png")),
                LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"))
            };
            var buttonSettingsCopyAdsk = new PushButtonData("SettingsCopyADSK", "Настройка\nкопирования",
                Assembly.GetExecutingAssembly().Location, typeof(CopyAdskSettings).FullName)
            {
                ToolTip = "Настройка параметров копирования",
                Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/SettingsCopyADSK16.png")),
                LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/SettingsCopyADSK32.png"))
            };
            var splitCopyAdsk = new SplitButtonData("SplitCopyADSK", "Меню");
            if (panel.AddItem(splitCopyAdsk) is SplitButton splitBtn)
            {
                splitBtn.AddPushButton(buttonCopyAdsk);
                splitBtn.AddPushButton(buttonSettingsCopyAdsk);
            }

            if (panel.AddItem(new PushButtonData("CreateDuctSystemView",
                    "Создать системы\nвоздуховодов",
                    Assembly.GetExecutingAssembly().Location, typeof(CreateDuctSystemViews).FullName)) is PushButton
                buttonCreateDuctSystemView)
            {
                buttonCreateDuctSystemView.ToolTip =
                    "Дублирования значений Имя системы для воздушных систем в проекте. Создание копий видов с фильтрами для данных систем";
                buttonCreateDuctSystemView.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreateDuctSystemViews16.png"));
                buttonCreateDuctSystemView.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreateDuctSystemViews32.png"));
            }

            if (panel.AddItem(new PushButtonData("CreatePipeSystemView",
                    "Создать системы\nтрубопроводов",
                    Assembly.GetExecutingAssembly().Location, typeof(CreatePipeSystemViews).FullName)) is PushButton
                buttonCreatePipeSystemView)
            {
                buttonCreatePipeSystemView.ToolTip =
                    "Дублирования значений Имя системы для трубопроводных систем в проекте. Создание копий видов с фильтрами для данных систем";
                buttonCreatePipeSystemView.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreatePipeSystemViews16.png"));
                buttonCreatePipeSystemView.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreatePipeSystemViews32.png"));
            }

            if (panel.AddItem(new PushButtonData("CreateSpaces", "Создать\nпространства",
                    Assembly.GetExecutingAssembly().Location, typeof(CreateSpaces).FullName)) is PushButton
                buttonCreateSpaces)
            {
                buttonCreateSpaces.ToolTip =
                    "Создание и автоматическое копирования значений параметров из архитектурных помещений в инженерные пространства. Используется экземпляр размещенной связи АР.";
                buttonCreateSpaces.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreateSpaces16.png"));
                buttonCreateSpaces.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreateSpaces32.png"));
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}