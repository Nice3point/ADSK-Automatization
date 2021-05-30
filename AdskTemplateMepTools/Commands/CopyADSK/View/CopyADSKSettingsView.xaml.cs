using System.Diagnostics;
using System.Windows.Navigation;
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

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}