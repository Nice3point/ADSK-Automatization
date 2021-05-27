using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using AdskTemplateMepTools.Commands.AutoNumerate;
using AdskTemplateMepTools.Commands.CheckADSK;
using AdskTemplateMepTools.Commands.CopyADSK.Commands;
using AdskTemplateMepTools.Commands.CreateDuctSystemViews;
using AdskTemplateMepTools.Commands.CreatePipeSystemViews;
using AdskTemplateMepTools.Commands.CreateSpaces;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace AdskTemplateMepTools
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            Configuration.Init();
            var panel = CreateRibbonTab(application);

            Configuration.TryReadKey(nameof(AutoNumerate), Resources.Localization.Configuration.ShowButtonKey, out var tabVisible);
            if (bool.Parse(tabVisible))
                if (panel.AddItem(new PushButtonData(nameof(AutoNumerate), Resources.Localization.Application.AutonumerateButtonName, Assembly.GetExecutingAssembly().Location,
                    typeof(AutoNumerate).FullName)) is PushButton buttonAutoNumerate)
                {
                    buttonAutoNumerate.ToolTip = Resources.Localization.Application.AutonumerateTootip;
                    buttonAutoNumerate.Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/AutoNumerate16.png"));
                    buttonAutoNumerate.LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/AutoNumerate32.png"));
                }

            Configuration.TryReadKey(nameof(CheckAdsk), Resources.Localization.Configuration.ShowButtonKey, out tabVisible);
            if (bool.Parse(tabVisible))
                if (panel.AddItem(new PushButtonData(nameof(CheckAdsk), Resources.Localization.Application.CheckAdskButtonName, Assembly.GetExecutingAssembly().Location, typeof(CheckAdsk).FullName))
                    is PushButton buttonCheckAdsk)
                {
                    buttonCheckAdsk.ToolTip = Resources.Localization.Application.CheckAdskTooltip;
                    buttonCheckAdsk.Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CheckADSK16.png"));
                    buttonCheckAdsk.LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CheckADSK32.png"));
                }

            Configuration.TryReadKey(nameof(CopyAdsk), Resources.Localization.Configuration.ShowButtonKey, out tabVisible);
            if (bool.Parse(tabVisible))
            {
                var buttonCopyAdsk = new PushButtonData(nameof(CopyAdsk), Resources.Localization.Application.CopyAdskButtonName, Assembly.GetExecutingAssembly().Location, typeof(CopyAdsk).FullName)
                {
                    ToolTip = Resources.Localization.Application.CopyAdskTooltip,
                    Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CopyADSK16.png")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CopyADSK32.png"))
                };
                var buttonSettingsCopyAdsk = new PushButtonData(nameof(CopyAdskSettings), Resources.Localization.Application.CopyAdskSettingsButtonName, Assembly.GetExecutingAssembly().Location,
                    typeof(CopyAdskSettings).FullName)
                {
                    ToolTip = Resources.Localization.Application.CopyAdskSettingsTooltip,
                    Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/SettingsCopyADSK16.png")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/SettingsCopyADSK32.png"))
                };
                var splitCopyAdsk = new SplitButtonData("SplitCopyADSK", "Меню");
                if (panel.AddItem(splitCopyAdsk) is SplitButton splitBtn)
                {
                    splitBtn.AddPushButton(buttonCopyAdsk);
                    splitBtn.AddPushButton(buttonSettingsCopyAdsk);
                }
            }

            Configuration.TryReadKey(nameof(CreateDuctSystemViews), Resources.Localization.Configuration.ShowButtonKey, out tabVisible);
            if (bool.Parse(tabVisible))
                if (panel.AddItem(new PushButtonData(nameof(CreateDuctSystemViews), Resources.Localization.Application.DuctSystemButtonName, Assembly.GetExecutingAssembly().Location,
                    typeof(CreateDuctSystemViews).FullName)) is PushButton buttonCreateDuctSystemView)
                {
                    buttonCreateDuctSystemView.ToolTip = Resources.Localization.Application.DuctSystemTooltip;
                    buttonCreateDuctSystemView.Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateDuctSystemViews16.png"));
                    buttonCreateDuctSystemView.LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateDuctSystemViews32.png"));
                }

            Configuration.TryReadKey(nameof(CreatePipeSystemViews), Resources.Localization.Configuration.ShowButtonKey, out tabVisible);
            if (bool.Parse(tabVisible))
                if (panel.AddItem(new PushButtonData(nameof(CreatePipeSystemViews), Resources.Localization.Application.PipeSystemButtonName, Assembly.GetExecutingAssembly().Location,
                    typeof(CreatePipeSystemViews).FullName)) is PushButton buttonCreatePipeSystemView)
                {
                    buttonCreatePipeSystemView.ToolTip = Resources.Localization.Application.PipeSystemTooltip;
                    buttonCreatePipeSystemView.Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreatePipeSystemViews16.png"));
                    buttonCreatePipeSystemView.LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreatePipeSystemViews32.png"));
                }

            Configuration.TryReadKey(nameof(CreateSpaces), Resources.Localization.Configuration.ShowButtonKey, out tabVisible);
            if (bool.Parse(tabVisible))
                if (panel.AddItem(new PushButtonData(nameof(CreateSpaces), Resources.Localization.Application.CreateSpacesButtonName, Assembly.GetExecutingAssembly().Location,
                    typeof(CreateSpaces).FullName)) is PushButton buttonCreateSpaces)
                {
                    buttonCreateSpaces.ToolTip = Resources.Localization.Application.CreateSpacesTooltip;
                    buttonCreateSpaces.Image = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateSpaces16.png"));
                    buttonCreateSpaces.LargeImage = new BitmapImage(new Uri("pack://application:,,,/AdskTemplateMepTools;component/Resources/Icons/Panel/CreateSpaces32.png"));
                }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static RibbonPanel CreateRibbonTab(UIControlledApplication application)
        {
            Configuration.TryReadKey("Application", Resources.Localization.Configuration.TabNameKey, out var tabName);
            if (string.IsNullOrEmpty(tabName)) return application.CreateRibbonPanel(Resources.Localization.Application.PanelName);
            var ribbonTab = ComponentManager.Ribbon.Tabs.FirstOrDefault(tab => tab.Id.Equals(tabName));
            if (ribbonTab == null) application.CreateRibbonTab(tabName);
            return application.CreateRibbonPanel(tabName, Resources.Localization.Application.PanelName);
        }
    }
}