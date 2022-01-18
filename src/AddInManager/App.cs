using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AddInManager.Command;
using AddInManager.Model;
using Autodesk.Revit.UI;

namespace AddInManager
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
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("External Tools");
            PulldownButtonData pulldownButtonData = new PulldownButtonData("Options", "Add-in Manager");
            PulldownButton PulldownButton = (PulldownButton)ribbonPanel.AddItem(pulldownButtonData);
            PulldownButton.Image = SetImage(Resource.dev1);
            PulldownButton.LargeImage = SetLargeImage(Resource.dev1);
            AddPushButton(PulldownButton, typeof(AddInManagerManual), "Add-In Manager(Manual Mode)");
            AddPushButton(PulldownButton, typeof(AddInManagerFaceless), "Add-In Manager(Manual Mode,Faceless)");
            AddPushButton(PulldownButton, typeof(AddInManagerReadOnly), "Add-In Manager(Read Only Mode)");
        }

        public static PushButton AddPushButton(PulldownButton pullDownButton, Type command, string buttonText)
        {
            PushButtonData buttonData = new PushButtonData(command.FullName, buttonText, Assembly.GetAssembly(command).Location, command.FullName);
            return pullDownButton.AddPushButton(buttonData);
        }

        public static ImageSource SetImage(Bitmap bitmap)
        {
           return BitmapSourceConverter.ConvertFromImage(Resource.dev1).Resize(16);
        }
        public static ImageSource SetLargeImage(Bitmap bitmap)
        {
            return BitmapSourceConverter.ConvertFromImage(Resource.dev1).Resize(32);
        }
    }
}
