using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace AddInManager.Model
{
    public static class StaticUtil
    {
        public static void ShowWarning(string msg)
        {
            MessageBox.Show(msg, Resource.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static string CommandFullName = typeof(IExternalCommand).FullName;


        public static string AppFullName = typeof(IExternalApplication).FullName;

        public static RegenerationOption RegenOption;

        public static TransactionMode TransactMode;
    }
}
