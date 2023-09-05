using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using RevitElementBipChecker.Viewmodel;

namespace RevitElementBipChecker.View
{
    /// <summary>
    /// Interaction logic for FrmCompareBip.xaml
    /// </summary>
    public partial class FrmCompareBip
    {
        private ParameterCompareViewModel _viewModel { get; set; }

        public FrmCompareBip(ParameterCompareViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = viewModel;
            viewModel.FrmCompareBip = this;
        }

        private void SortByColor(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button) sender;
                // sort another remain order by Name 
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