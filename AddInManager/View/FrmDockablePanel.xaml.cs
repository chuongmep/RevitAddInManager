using Autodesk.Revit.UI;

namespace RevitAddinManager.View;

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
#if R19
#else
            MinimumWidth = 250,
            MinimumHeight = 250,
#endif
        };

#if R14 || R15 || R16
#else
        data.VisibleByDefault = false;
#endif
    }
}