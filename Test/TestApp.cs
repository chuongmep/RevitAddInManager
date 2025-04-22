using Autodesk.Revit.UI;

namespace Test
{
    internal class TestApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            TaskDialog.Show("App", @"Startup");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            TaskDialog.Show("App", @"ShutDown");
            return Result.Cancelled;
        }
    }
}