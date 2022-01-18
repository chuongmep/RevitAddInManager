using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AddInManager.Command;
using AddInManager.Model;
using AddInManager.ViewModel;

namespace AddInManager.View
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