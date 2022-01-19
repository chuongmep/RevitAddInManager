using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace RevitTest
{
    internal class TestApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            MessageBox.Show("Startup");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            MessageBox.Show("ShutDown");
            return Result.Cancelled;
        }
    }
}
