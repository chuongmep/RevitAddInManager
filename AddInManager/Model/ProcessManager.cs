using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Size = System.Drawing.Size;

namespace RevitAddinManager.Model
{
    public static class ProcessManager
    {
        private static FormControl FrmControl => FormControl.Instance;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void SetRevitAsWindowOwner(this Window window)
        {
            if (null == window) { return; }
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = GetActivateWindow();
            window.Loaded += delegate { FrmControl.IsOpened = true; };
            window.Closed += delegate { FrmControl.IsOpened = false; };
            window.Closing += SetActivateWindow;
           
        }
        
        /// <summary>
        /// Set correct position for window when monitor is changed
        /// </summary>
        /// <param name="window"></param>
        public static void SetMonitorSize(this Window window)
        {
            ScreenInformation screenInformation = new ScreenInformation();
            int monitorCount = screenInformation.GetMonitorCount();
            double appLeft = Properties.App.Default.AppLeft;
            int width = MonitorControl.GetMonitorSize().Width;
            int height = MonitorControl.GetMonitorSize().Height;
            if (monitorCount==1 && appLeft>width)
            {
                Properties.App.Default.AppLeft = width *0.5;
                Properties.App.Default.AppTop = height * 0.5;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Properties.App.Default.Save();
            }
        }

        private static void SetActivateWindow(object sender, CancelEventArgs e)
        {
            SetActivateWindow();
        }

        /// <summary>
        /// Set process revert use revit
        /// </summary>
        /// <returns></returns>
        private static void SetActivateWindow()
        {
            IntPtr ptr = GetActivateWindow();
            if (ptr != IntPtr.Zero)
            {
                SetForegroundWindow(ptr);
            }
        }

        /// <summary>
        /// return active windows is active
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetActivateWindow()
        {
            return Process.GetCurrentProcess().MainWindowHandle;
        }
    }
}