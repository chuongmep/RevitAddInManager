using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Viewmodel;

namespace RevitElementBipChecker.Model
{
    public class BaseElement  : ViewmodeBase
    {
        private Element _element;
        private UIDocument _uiDoc;
        public BipCheckerViewmodel Viewmodel { get; set; }
        public UIDocument UIDoc
        {
            get => _uiDoc ;
            set => _uiDoc = value;
        }

       public Element Element
        {
            get => _element;
            set => _element = value;
        }

       private Element elementType;
        public Element ElementType
        {
            get
            {
                if(Element.CanHaveTypeAssigned()) return UIDoc.Document.GetElement(Element.GetTypeId());
                return elementType;
            }
            set => elementType = value;
        }

        public string category;
        public string Category
        {
            get => Element.Category.Name;
            set => category = value;
        }

        public string Name
        {
            get => Element.Name;
            set => Element.Name = value;
        }
        public string State { get; set; }
       
    }

    
}
