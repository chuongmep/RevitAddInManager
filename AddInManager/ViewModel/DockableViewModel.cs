using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class DockableViewModel : ViewModelBase
{
    public DockableViewModel(UIControlledApplication a)
    {
        a.ViewActivated += OnViewActivated;
    }

    private void OnViewActivated(object sender, ViewActivatedEventArgs e)
    {
        this.Document = e.Document;
    }

    private Document document;
    public Document Document
    {
        get => document;
        set
        {
            document = value;
            OnPropertyChanged(nameof(Document));
        }
    }
}