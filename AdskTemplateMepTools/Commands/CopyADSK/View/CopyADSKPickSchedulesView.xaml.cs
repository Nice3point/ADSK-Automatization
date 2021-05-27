using System.Collections.Generic;
using System.Windows;
using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;

namespace AdskTemplateMepTools.Commands.CopyADSK.View
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