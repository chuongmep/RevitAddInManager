using System.Linq;
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
                Element =vm.UIDoc.GetSelection().First();
                vm.Element = Element;
                vm.State = "Element"; 
                vm.GetDatabase();
            }
        }
    }
}
