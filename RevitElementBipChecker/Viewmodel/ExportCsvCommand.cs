using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class ExportCsvCommand : ICommand
    {
        public BipCheckerViewmodel Viewmodel;
        public ExportCsvCommand(BipCheckerViewmodel vm)
        {
            this.Viewmodel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            try
            {
                List<ParameterData> parameterDatas = Viewmodel.frmmain.lsBipChecker.Items.Cast<ParameterData>().ToList();
                DataTable dataTable = parameterDatas.ToDataTable();
                dataTable.Columns.Remove("Parameter");
                string path = dataTable.ExportCsv();
                Process.Start(path);
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Please Close File Data Exported");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }


        }
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
