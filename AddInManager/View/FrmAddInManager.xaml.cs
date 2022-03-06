using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RevitAddinManager.Model;
using RevitAddinManager.ViewModel;

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
        if (viewModel.MAddinManagerBase.ActiveCmdItem != null && TabControl.SelectedIndex==0)
        {
            viewModel.MAddinManagerBase.ActiveCmdItem.Description = TbxDescription.Text;
        }
        if (viewModel.MAddinManagerBase.ActiveAppItem != null && TabControl.SelectedIndex==1)
        {
            viewModel.MAddinManagerBase.ActiveAppItem.Description = TbxDescription.Text;
        }
        viewModel.MAddinManagerBase.AddinManager.SaveToAimIni();
    }

    
}