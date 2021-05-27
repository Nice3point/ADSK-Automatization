using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;

namespace AdskTemplateMepTools.Commands.CopyADSK.View
{
    public partial class CopyAdskPickSchedulesView
    {
        private readonly CopyAdskPickScheduleViewModel _viewModel;
        public CopyAdskPickSchedulesView(CopyAdskPickScheduleViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel; 
            InitializeComponent();
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SpecificationPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            _viewModel.SelectedUnusedSpecification =listBox.SelectedItems.Cast<string>().ToList();
        }
    }
}