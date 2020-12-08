using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI;
using Nice3point.Revit.ADSK.MEP.ViewModel;

namespace Nice3point.Revit.ADSK.MEP.UI
{
    public partial class CopyAdskPickSchedulesView
    {
        private CopyAdskPickScheduleViewModel ViewModel { get; }
        public CopyAdskPickSchedulesView(List<string> schedules)
        {
            ViewModel = new CopyAdskPickScheduleViewModel(schedules);
            DataContext = ViewModel; 
            InitializeComponent();
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}