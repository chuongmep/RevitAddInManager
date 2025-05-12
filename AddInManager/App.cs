using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.UI;
using RevitAddinManager.Command;
using RevitAddinManager.View;
using System.Reflection;
using Autodesk.Windows;
using RevitAddinManager.Model;
using RevitAddinManager.View.Control;
using RevitAddinManager.ViewModel;
using RevitElementBipChecker.Command;
using static RevitAddinManager.Model.BitmapSourceConverter;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace RevitAddinManager;

public class App : IExternalApplication
{
    public static FrmAddInManager FrmAddInManager { get; set; }
    public static LogControl FrmLogControl { get; set; }
    public static FrmDockablePanel DockPanelProvider;
    public static int ThemId { get; set; } = -1;
    public static DockablePaneId PaneId => new DockablePaneId(new Guid("942D8578-7F25-4DC3-8BD8-585C1DBD3614"));
    public static string PaneName => "Debug/Trace Output";
    
    private static readonly Dictionary<string, string> KnownAssemblies = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        { "System.Resources.Extensions", "System.Resources.Extensions.dll" },
        { "System.Runtime.CompilerServices.Unsafe", "System.Runtime.CompilerServices.Unsafe.dll" }
    };
    private static readonly ConcurrentDictionary<string, Assembly> AssemblyCache = new ConcurrentDictionary<string, Assembly>(StringComparer.Ordinal);
    private static readonly string BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public Result OnStartup(UIControlledApplication application)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        CreateRibbonPanel(application);
        application.ControlledApplication.DocumentClosed += DocumentClosed;
        DefaultSetting.Version += VersionChecker.CurrentVersion;
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
        pulldownButton.Image = ToImageSource(Resource.dev1, ImageType.Small);
        pulldownButton.LargeImage = ToImageSource(Resource.dev1, ImageType.Large);
        AddPushButton(pulldownButton, typeof(AddInManagerManual), "Add-In Manager(Manual Mode)");
        AddPushButton(pulldownButton, typeof(AddInManagerFaceless), "Add-In Manager(Manual Mode,Faceless)");
        AddPushButton(pulldownButton, typeof(AddInManagerReadOnly), "Add-In Manager(Read Only Mode)");
        AddPushButton(pulldownButton, typeof(DockableCommand), "Show/Hide Panel(Debug-Trace-Events)");
        AddPushButtonBipChecker(pulldownButton, typeof(BipCheckerCommand), "Bip Checker");
        AddPushButtonBipChecker(pulldownButton, typeof(CompareTwoEleCommand), "Compare\nTwo Element(Compact)");
        var tab = ComponentManager.Ribbon.FindTab("Modify");
        if (tab != null)
        {
            var adwPanel = new Autodesk.Windows.RibbonPanel();
            adwPanel.CopyFrom(GetRibbonPanel(ribbonPanel));
            tab.Panels.Add(adwPanel);
        }

    }
    private static readonly FieldInfo RibbonPanelField = typeof(Autodesk.Revit.UI.RibbonPanel).GetField("m_RibbonPanel", BindingFlags.Instance | BindingFlags.NonPublic);
       
    public static Autodesk.Windows.RibbonPanel GetRibbonPanel(Autodesk.Revit.UI.RibbonPanel panel)
    {
        return RibbonPanelField.GetValue(panel) as Autodesk.Windows.RibbonPanel;
    }

    private static void AddPushButton(PulldownButton pullDownButton, Type command, string buttonText)
    {
        var buttonData = new PushButtonData(command.FullName, buttonText, Assembly.GetAssembly(command).Location, command.FullName);
        buttonData.AvailabilityClassName = typeof(AddinManagerCommandAvail).FullName;
        pullDownButton.AddPushButton(buttonData);
    }
    private static void AddPushButtonBipChecker(PulldownButton pullDownButton, Type command, string buttonText)
    {
        var buttonData = new PushButtonData(command.FullName, buttonText, Assembly.GetAssembly(command).Location, command.FullName);
        buttonData.AvailabilityClassName = typeof(BipCheckerCommandAvail).FullName;
        buttonData.ToolTip = "Check Built-in Parameter of the Element";
        pullDownButton.AddPushButton(buttonData);
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var requestedAssemblyName = new AssemblyName(args.Name).Name;
        if (AssemblyCache.TryGetValue(requestedAssemblyName, out var cachedAssembly))
        {
            return cachedAssembly;
        }
        if (KnownAssemblies.TryGetValue(requestedAssemblyName, out var assemblyFile))
        {
            var assemblyPath = Path.Combine(BaseDirectory, assemblyFile);
        
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                AssemblyCache.TryAdd(requestedAssemblyName, assembly);
                return assembly;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    
        return null;
    }
    
    private void DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
    {
        FrmAddInManager?.Close();
    }
}
