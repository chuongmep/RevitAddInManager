using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using QuickMsiBuilder.CLI;

namespace QuickMsiBuilder.UI
{
    /// <summary>
    /// Opens the NLog build log, shared by the main window and the result dialog.
    /// </summary>
    public static class LogFile
    {
        public static void Open(Window owner)
        {
            var path = File.Exists(BuildLog.LogFilePath) ? BuildLog.LogFilePath : BuildLog.LogDirectory;

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                MessageBox.Show(owner, "No log has been written yet.", "Quick MSI Builder");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "Could not open the log: " + ex.Message, "Quick MSI Builder");
            }
        }
    }
}
