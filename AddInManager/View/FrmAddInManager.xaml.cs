using System.Windows;
using AddinManager.ViewModel;

namespace AddinManager.View
{
    /// <summary>
    /// Interaction logic for FrmAddInManager.xaml
    /// </summary>
    public partial class FrmAddInManager : Window
    {
        public FrmAddInManager(AddInManagerViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            vm.FrmAddInManager = this;
        }
        
       
    }
}