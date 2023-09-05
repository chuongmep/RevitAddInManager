using RevitElementBipChecker.Viewmodel;
using System.Windows;

namespace RevitElementBipChecker.View
{
    /// <summary>
    /// Interaction logic for FrmPropertyChecker.xaml
    /// </summary>
    public partial class FrmPropertyChecker : Window
    {
        private PropertyObjectCompareViewModel _viewModel;
        public FrmPropertyChecker(PropertyObjectCompareViewModel viewModel)
        {
            InitializeComponent();
            this._viewModel = viewModel;
            this.DataContext = viewModel;

        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
