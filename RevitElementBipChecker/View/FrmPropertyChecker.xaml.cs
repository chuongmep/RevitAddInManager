using System;
using System.Linq;
using RevitElementBipChecker.Viewmodel;
using System.Windows;
using System.Windows.Controls;

namespace RevitElementBipChecker.View
{
    /// <summary>
    /// Interaction logic for FrmPropertyChecker.xaml
    /// </summary>
    public partial class FrmPropertyChecker : Window
    {
        private ComparePropertyViewModel _viewModel;
        public FrmPropertyChecker(ComparePropertyViewModel viewModel)
        {
            InitializeComponent();
            this._viewModel = viewModel;
            this.DataContext = viewModel;

        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void SortByColor(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button) sender;
                dataGrid.ItemsSource = _viewModel.Differences
                    .OrderByDescending(x => x?.RowColor == button?.Background)
                    .ThenBy(x => x.Name);
                dataGrid.ScrollIntoView(dataGrid.Items[0]);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
