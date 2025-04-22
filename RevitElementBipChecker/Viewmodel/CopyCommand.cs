using System;
using System.Windows;
using System.Windows.Input;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class CopyCommand : ICommand
    {
        public BipCheckerViewmodel vm;
        public CopyCommand(BipCheckerViewmodel vm)
        {
            this.vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string name = parameter as string;
            switch (name)
            {
                case "BuildIn":
                    Copy_BuiltInParameter();
                    break;
                case "PraName":
                    Copy_ParameterName();
                    break;
                case "Type":
                    Copy_Type();
                    break;
                case "Value":
                    Copy_Value();
                    break;
                case "PraGroup":
                    Copy_ParameterGroup();
                    break;
                case "GName":
                    Copy_GroupName();
                    break;
                case "GUID":
                    Copy_Guid();
                    break;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        ParameterData GetSelectedItem()
        {
            int selected = vm.frmmain.lsBipChecker.SelectedIndex;
            return  vm.frmmain.lsBipChecker.Items[selected] as ParameterData;
        }
        private void Copy_BuiltInParameter()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.BuiltInParameter);
        }
        private void Copy_ParameterName()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.Name);
        }
        private void Copy_Type()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.Type);
        }
        private void Copy_Value()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.Value);
        }
        private void Copy_ParameterGroup()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.ParameterGroup);
        }
        private void Copy_GroupName()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.GroupName);
        }
        private void Copy_Guid()
        {
            ParameterData parameterData = GetSelectedItem();
            Clipboard.SetText(parameterData.GUID);
        }
    }
}
