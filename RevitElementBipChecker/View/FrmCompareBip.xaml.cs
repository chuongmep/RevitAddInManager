using RevitElementBipChecker.Viewmodel;

namespace RevitElementBipChecker.View
{
    /// <summary>
    /// Interaction logic for FrmCompareBip.xaml
    /// </summary>
    public partial class FrmCompareBip
    {
        public FrmCompareBip(CompareBipViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.FrmCompareBip = this;
        }
    }
}
