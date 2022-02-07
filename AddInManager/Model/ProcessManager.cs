using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RevitAddinManager.Model
{
    public static class ProcessManager
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Set process revert use revit
        /// </summary>
        /// <returns></returns>
        public static bool SetActivateWindow()
        {
            IntPtr ptr = GetActivateWindow();
            if (ptr != IntPtr.Zero)
            {
                return SetForegroundWindow(ptr);
            }
            return false;
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
