using System.Windows;
using Autodesk.Revit.UI;

namespace RevitAddinManager.View.Control;

public partial class FrmDockablePanel : IDockablePaneProvider
{
    public FrmDockablePanel()
    {
        InitializeComponent();
    }

    public void SetupDockablePane(DockablePaneProviderData data)
    {
        data.FrameworkElement = this;
    }
}