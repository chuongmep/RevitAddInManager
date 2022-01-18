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

		public static string commandFullName = typeof(IExternalCommand).FullName;

		
		public static string appFullName = typeof(IExternalApplication).FullName;

		public static RegenerationOption m_regenOption;

		public static TransactionMode m_tsactMode;
	}
}
