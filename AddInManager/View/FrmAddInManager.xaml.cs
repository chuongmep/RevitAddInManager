using RevitAddinManager.ViewModel;
using System.Windows;
using System.Windows.Input;
using RevitAddinManager.Model;
using System.Windows.Threading;

namespace RevitAddinManager.View;

/// <summary>
/// Interaction logic for FrmAddInManager.xaml
/// </summary>
public partial class FrmAddInManager : Window
{
    private readonly AddInManagerViewModel viewModel;

    public FrmAddInManager(AddInManagerViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        viewModel = vm;
        App.FrmAddInManager = this;
        ThemManager.ChangeThem(true);
        Title += DefaultSetting.Version;
    }

    private void TbxDescription_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (viewModel.MAddinManagerBase.ActiveCmdItem != null && TabControl.SelectedIndex == 0)
        {
            viewModel.MAddinManagerBase.ActiveCmdItem.Description = TbxDescription.Text;
        }

        if (viewModel.MAddinManagerBase.ActiveAppItem != null && TabControl.SelectedIndex == 1)
        {
            viewModel.MAddinManagerBase.ActiveAppItem.Description = TbxDescription.Text;
        }

        viewModel.MAddinManagerBase.AddinManager.SaveToAimIni();
    }

    private void TreeViewCommand_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
            return;

        if (e.Delta > 0)
            ZoomIn();

        else if (e.Delta < 0)
            ZoomOut();
    }

    void ZoomIn()
    {
        if (TreeViewCommand.FontSize <= 30)
        {
            TreeViewCommand.FontSize += 2f;
        }
    }

    void ZoomOut()
    {
        if (TreeViewCommand.FontSize >= 10)
        {
            TreeViewCommand.FontSize -= 2f;
        }
    }

    private void HandleTreeViewCommandKeyPress(object sender, KeyEventArgs e)
    {
        int indexCmd = TreeViewCommand.Items.IndexOf(TreeViewCommand.SelectedItem);
        if (e.Key == Key.Up && TabCommand.IsFocused)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && Keyboard.Modifiers == ModifierKeys.Control && TabCommand.IsSelected)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && indexCmd == 0 && TabCommand.IsSelected)
        {
            TabCommand.Focus();
        }
        if (e.Key == Key.Down && TabCommand.IsSelected)
        {
            TreeViewCommand.Focus();
        }
        if (e.Key == Key.Enter)
        {
            viewModel.ExecuteAddinCommandClick();
        }
    }

    private void HandleTreeViewAppKeyPress(object sender, KeyEventArgs e)
    {
        int indexCmd = TreeViewApp.Items.IndexOf(TreeViewApp.SelectedItem);
        if (e.Key == Key.Up && TabApp.IsFocused)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && Keyboard.Modifiers == ModifierKeys.Control && TabApp.IsSelected)
        {
            tbxSearch.Focus();
        }
        else if (e.Key == Key.Up && indexCmd == 0 && TabApp.IsSelected)
        {
            TabApp.Focus();
        }
        if (e.Key == Key.Down && TabApp.IsSelected)
        {
            TreeViewApp.Focus();
        }
        if (e.Key == Key.Enter)
        {
            viewModel.ExecuteAddinCommandClick();
        }
    }

    private void HandleTextboxKeyPress(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (viewModel.IsTabCmdSelected)
            {
                TreeViewCommand.Focus();
            }
            else if (viewModel.IsTabAppSelected)
            {
                TreeViewApp.Focus();
            }
            else if (viewModel.IsTabStartSelected)
            {
                DataGridStartup.Focus();
            }
            else
            {
                LogControl.Focus();
            }
        }
    }

    private void CloseFormEvent(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private DispatcherTimer SizeChangedDebouncer;

    private void FrmSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SizeChangedDebouncer = Debounce(SizeChangedDebouncer, TimeSpan.FromSeconds(1), SaveUserSettings);
    }

    private void SaveUserSettings()
    {
        Properties.App.Default.AppHeight = Height;
        Properties.App.Default.AppWidth = Width;
        Properties.App.Default.Save();
    }

    private DispatcherTimer Debounce(DispatcherTimer dispatcher, TimeSpan interval, Action action)
    {
        dispatcher?.Stop();
        dispatcher = null;
        dispatcher = new DispatcherTimer(interval, DispatcherPriority.ApplicationIdle, (s, e) =>
        {
            dispatcher?.Stop();
            action.Invoke();
        }, Dispatcher.CurrentDispatcher);
        dispatcher?.Start();

        return dispatcher;
    }

    private void OnCloseApp(object sender, EventArgs e)
    {
        Properties.App.Default.AppLeft = Left;
        Properties.App.Default.AppTop = Top;
        Properties.App.Default.Save();
    }
}