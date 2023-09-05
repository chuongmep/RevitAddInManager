using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Viewmodel;
using System.Collections.Generic;

namespace RevitElementBipChecker.Model
{
    public class BaseElement : ViewmodeBase
    {
        private List<Element> _elements;
        private UIDocument _uiDoc;
        public BipCheckerViewmodel Viewmodel { get; set; }

        public UIDocument UIDoc
        {
            get => _uiDoc;
            set => _uiDoc = value;
        }

        public List<Element> Elements
        {
            get => _elements;
            set => _elements = value;
        }

        //
        // public string category;
        // public string Category
        // {
        //     get => Element.Category.Name;
        //     set => category = value;
        // }
        //
        // public string Name
        // {
        //     get => Element.Name;
        //     set => Element.Name = value;
        // }
        public string State { get; set; }
    }
}