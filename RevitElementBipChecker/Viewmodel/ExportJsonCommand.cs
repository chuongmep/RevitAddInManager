using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class ExportJsonCommand : ICommand
    {
        public BipCheckerViewmodel Viewmodel;
        public ExportJsonCommand(BipCheckerViewmodel vm)
        {
            this.Viewmodel = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {

            List<ParameterData> parameterDatas = Viewmodel.frmmain.lsBipChecker.Items.Cast<ParameterData>().ToList();
            DataTable dataTable = parameterDatas.ToDataTable();
            dataTable.Columns.Remove("Parameter");
            dataTable.WriteJson(out string path);
            Process.Start("explorer.exe", path);

        }
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
