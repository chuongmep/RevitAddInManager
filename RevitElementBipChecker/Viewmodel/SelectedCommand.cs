using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class SelectedCommand : BaseElement
    {
        
        public SelectedCommand(BipCheckerViewmodel vm)
        {
            int count = vm.UIDoc.GetSelection().Count;
            if (count>0)
            {
                Elements =vm.UIDoc.GetSelection();
                vm.Elements = Elements;
                vm.State = "Element"; 
                vm.GetDatabase();
            }
        }
    }
}
