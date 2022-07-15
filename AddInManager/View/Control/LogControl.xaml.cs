using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RevitAddinManager.Model;
using RevitAddinManager.ViewModel;

namespace RevitAddinManager.View.Control
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        public LogControl()
        {
            InitializeComponent();
            LogControlViewModel viewModel = new LogControlViewModel();
            DataContext = viewModel;
            App.FrmLogControl = this;
            this.Loaded += viewModel.LogFileWatcher;
            this.Unloaded += viewModel.UserControl_Unloaded;

        }
        private void RightClickCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (LogMessageString dd in listBox_LogMessages.SelectedItems)
            {
                sb.AppendLine(dd.Message);
            }
            Clipboard.SetText(sb.ToString());
        }
        private void RightClickCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = listBox_LogMessages.SelectedItem != null;
        }
    }
}
