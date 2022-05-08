using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows;

namespace RevitAddinManager.Model;

public static class StaticUtil
{
    public static void ShowWarning(string msg)
    {
        MessageBox.Show(msg, Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    public static string CommandFullName = typeof(IExternalCommand).FullName;

    public static string AppFullName = typeof(IExternalApplication).FullName;

    public static RegenerationOption RegenOption;

    public static TransactionMode TransactMode;
    /// <summary>
    /// CaseInsensitiveContains
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="stringComparison"></param>
    /// <returns></returns>
    public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        return text.IndexOf(value, stringComparison) >= 0;
    }
}