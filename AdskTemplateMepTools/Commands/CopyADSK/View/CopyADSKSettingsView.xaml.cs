using AdskTemplateMepTools.Commands.CopyADSK.ViewModel;

namespace AdskTemplateMepTools.Commands.CopyADSK.View
{
    public partial class CopyAdskSettingsView
    {
        public CopyAdskSettingsView(CopyAdskSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}