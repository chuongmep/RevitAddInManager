using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using RevitElementBipChecker.Model;
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
