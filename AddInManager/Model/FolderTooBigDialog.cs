using System.Text;
using System.Windows;

namespace RevitAddinManager.Model;

internal static class FolderTooBigDialog
{
    /// <summary>
    /// Show a waring if file dll too large
    /// </summary>
    /// <param name="folderPath">folder contains file resource</param>
    /// <param name="sizeInMB">limit of dll size</param>
    /// <returns></returns>
    public static MessageBoxResult Show(string folderPath, long sizeInMB)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Folder [" + folderPath + "]");
        stringBuilder.AppendLine("is " + sizeInMB + "MB large.");
        stringBuilder.AppendLine("AddinManager will attempt to copy all the files to temp folder");
        stringBuilder.AppendLine("Select [Yes] to copy all the files to temp folder");
        stringBuilder.AppendLine("Select [No] to only copy test script DLL");
        var text = stringBuilder.ToString();
        return MessageBox.Show(text, Resource.AppName, MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
    }
}