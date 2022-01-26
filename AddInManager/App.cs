using System.Reflection;
using Autodesk.Revit.UI;
using RevitAddinManager.Command;
using static RevitAddinManager.Model.BitmapSourceConverter;

namespace RevitAddinManager
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbonPanel(application);
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
        }

        private static PushButton AddPushButton(PulldownButton pullDownButton, Type command, string buttonText)
        {
            var buttonData = new PushButtonData(command.FullName, buttonText, Assembly.GetAssembly(command).Location, command.FullName);
            return pullDownButton.AddPushButton(buttonData);
        }

      
    }
}
