using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RevitAddinManager.Model;

public static class MonitorControl
{
    public static System.Drawing.Size GetMonitorSize()
    {
        var hwnd = Process.GetCurrentProcess().MainWindowHandle;
        var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
        NativeMethods.MONITORINFO info = new NativeMethods.MONITORINFO();
        NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
        return info.rcMonitor.Size;
    }
    
}
internal static class NativeMethods
{
    public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(HandleRef hmonitor, MONITORINFO info);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MONITORINFO
    {
        internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        internal RECT rcMonitor = new RECT();
        internal RECT rcWork = new RECT();
        internal int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(System.Drawing.Rectangle r)
        {
            left = r.Left;
            top = r.Top;
            right = r.Right;
            bottom = r.Bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height) => new RECT(x, y, x + width, y + height);

        public System.Drawing.Size Size => new System.Drawing.Size(right - left, bottom - top);
    }
}