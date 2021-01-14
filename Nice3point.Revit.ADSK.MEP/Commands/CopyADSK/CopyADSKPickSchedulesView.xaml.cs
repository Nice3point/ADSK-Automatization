using System.Collections.Generic;
using System.Windows;

namespace Nice3point.Revit.ADSK.MEP.Commands.CopyADSK
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