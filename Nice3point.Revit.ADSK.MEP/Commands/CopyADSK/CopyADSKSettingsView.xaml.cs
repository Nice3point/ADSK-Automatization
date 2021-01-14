namespace Nice3point.Revit.ADSK.MEP.Commands.CopyADSK
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