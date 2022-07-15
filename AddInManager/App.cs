using Autodesk.Revit.UI;
using RevitAddinManager.Command;
using RevitAddinManager.View;
using System.Reflection;
using RevitAddinManager.Model;
using RevitAddinManager.View.Control;
using RevitAddinManager.ViewModel;
using static RevitAddinManager.Model.BitmapSourceConverter;

namespace RevitAddinManager;

public class App : IExternalApplication
{
    public static FrmAddInManager FrmAddInManager { get; set; }
    public static LogControl FrmLogControl { get; set; }
    public static FrmDockablePanel DockPanelProvider;
    public static int ThemId { get; set; } = -1;
    public static DockablePaneId PaneId => new DockablePaneId(new Guid("942D8578-7F25-4DC3-8BD8-585C1DBD3614"));
    public static string PaneName => "Debug/Trace Output";
    public Result OnStartup(UIControlledApplication application)
    {
        CreateRibbonPanel(application);
        application.ControlledApplication.DocumentClosed += DocumentClosed;
        //EventWatcher eventWatcher = new EventWatcher(application);

        DockPanelProvider = new FrmDockablePanel() { DataContext = new DockableViewModel(application) };
        if (!DockablePane.PaneIsRegistered(PaneId))
        {
            application.RegisterDockablePane(PaneId, PaneName, DockPanelProvider);
        }
        return Result.Succeeded;
    }
    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Cancelled;
    }
    private static void CreateRibbonPanel(UIControlledApplication application)
    {
        var ribbonPanel = application.CreateRibbonPanel("External Tools");
        var pulldownButtonData = new PulldownButtonData("Options", "Add-in Manager");
        var pulldownButton = (PulldownButton)ribbonPanel.AddItem(pulldownButtonData);
        pulldownButton.Image = ToImageSource(Resource.dev1, BitmapSourceConverter.ImageType.Small);
        pulldownButton.LargeImage = ToImageSource(Resource.dev1, BitmapSourceConverter.ImageType.Large);
        AddPushButton(pulldownButton, typeof(AddInManagerManual), "Add-In Manager(Manual Mode)");
        AddPushButton(pulldownButton, typeof(AddInManagerFaceless), "Add-In Manager(Manual Mode,Faceless)");
        AddPushButton(pulldownButton, typeof(AddInManagerReadOnly), "Add-In Manager(Read Only Mode)");
        AddPushButton(pulldownButton, typeof(DockableCommand), "Show/Hide Panel(Debug-Trace-Events)");
    }

    private static void AddPushButton(PulldownButton pullDownButton, Type command, string buttonText)
    {
        var buttonData = new PushButtonData(command.FullName, buttonText, Assembly.GetAssembly(command).Location, command.FullName);
        buttonData.AvailabilityClassName = typeof(AddinManagerCommandAvail).FullName;
        pullDownButton.AddPushButton(buttonData);
    }

    private void DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
    {
        FrmAddInManager?.Close();
    }
}