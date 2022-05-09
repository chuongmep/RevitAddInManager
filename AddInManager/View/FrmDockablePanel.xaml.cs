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
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Right,
            MinimumWidth = 250,
            MinimumHeight = 250,
        };
        data.VisibleByDefault = false;
    }
}