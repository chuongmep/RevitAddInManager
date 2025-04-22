using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitElementBipChecker.Model;

public class BaseElementCompare : BaseDataCompare
{
    public UIDocument UiDoc { get; set; }
    public UIApplication UiApp { get; set; }
    public Element Element1 { get; set; }
    public Element Element2 { get; set; }
}