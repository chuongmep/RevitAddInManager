using RevitAddinManager.ViewModel;
using System.Windows;
using System.Windows.Input;

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
        vm.FrmAddInManager = this;
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
        if(TreeViewCommand.FontSize >= 10)
        {
            TreeViewCommand.FontSize -= 2f;
        }
    }

    private void HandleTreeViewCommandKeyPress(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            viewModel.ExecuteAddinCommandClick();
        }

    }

    private void HandleTextboxKeyPress(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.Down)
        {
            if (viewModel.IsTabCmdSelected)
            {
                TreeViewCommand.Focus();
            }
            else if (viewModel.IsTabAppSelected)
            {
                TreeViewApp.Focus();
            }
            else if(viewModel.IsTabStartSelected)
            {
                DataGridStartup.Focus();
            }
            else
            {
                LogControl.Focus();
            }
        }
        
    }
}