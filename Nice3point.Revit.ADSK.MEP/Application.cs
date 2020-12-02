using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Nice3point.Revit.ADSK.MEP
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var panel = application.CreateRibbonPanel("Шаблон ADSK");

            if (panel.AddItem(new PushButtonData("AutoNumerate", "Автонумерация\nв спецификациях",
                Assembly.GetExecutingAssembly().Location, typeof(AutoNumerate).FullName)) is PushButton buttonAutoNumerate)
            {
                buttonAutoNumerate.ToolTip = "Автонумерация спецификаций в общий параметр ADSK_Позиция";
                buttonAutoNumerate.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
                buttonAutoNumerate.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            if (panel.AddItem(new PushButtonData("CheckADSK", "Проверить\nсемейства",
                Assembly.GetExecutingAssembly().Location, typeof(CheckAdsk).FullName)) is PushButton buttonCheckAdsk)
            {
                buttonCheckAdsk.ToolTip =
                    "Проверка файлов семейств на соответствие требованиям по общим параметрам ADSK";
                buttonCheckAdsk.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
                buttonCheckAdsk.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            if (panel.AddItem(new PushButtonData("CopyADSK", "Копировать\nзначения",
                Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName)) is PushButton buttonCopyAdsk)
            {
                buttonCopyAdsk.ToolTip = "Копирование параметров ADSK в спецификациях";
                buttonCopyAdsk.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
                buttonCopyAdsk.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            if (panel.AddItem(new PushButtonData("CreateDuctSystemView",
                "Создать\nсистемы воздуховодов",
                Assembly.GetExecutingAssembly().Location, typeof(CreateDuctSystemViews).FullName)) is PushButton buttonCreateDuctSystemView)
            {
                buttonCreateDuctSystemView.ToolTip =
                    "Дублирования значений Имя системы для воздушных систем в проекте. Создание копий видов с фильтрами для данных систем";
                buttonCreateDuctSystemView.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
                buttonCreateDuctSystemView.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            if (panel.AddItem(new PushButtonData("CreatePipeSystemView",
                "Создать\nсистемы трубопроводов",
                Assembly.GetExecutingAssembly().Location, typeof(CreatePipeSystemViews).FullName)) is PushButton buttonCreatePipeSystemView)
            {
                buttonCreatePipeSystemView.ToolTip =
                    "Дублирования значений Имя системы для трубопроводных систем в проекте. Создание копий видов с фильтрами для данных систем";
                buttonCreatePipeSystemView.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK16.png"));
                buttonCreatePipeSystemView.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            if (panel.AddItem(new PushButtonData("CreateSpaces", "Создать\nпространства",
                Assembly.GetExecutingAssembly().Location, typeof(CreateSpaces).FullName)) is PushButton buttonCreateSpaces)
            {
                buttonCreateSpaces.ToolTip =
                    "Создание и автоматическое копирования значений параметров из архитектурных помещений в инженерные пространства. Используется экземпляр размещенной связи АР.";
                buttonCreateSpaces.Image = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CreateSpaces16.png"));
                buttonCreateSpaces.LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/Nice3point.Revit.ADSK.MEP;component/Resources/CopyADSK32.png"));
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}