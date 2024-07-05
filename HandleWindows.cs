using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Assist
{
    internal class HandleWindows
    {
        public HandleWindows() {
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
            EnumWindows(EnumWindowsTest, IntPtr.Zero);
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumDisplayMonitorsProc lpfnEnum, IntPtr dwData);
        public delegate bool EnumDisplayMonitorsProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        private static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            Console.WriteLine($"Monitor Handle: {hMonitor}");
            Console.WriteLine($"Monitor Coordinates: Left = {lprcMonitor.Left}, Top = {lprcMonitor.Top}, Right = {lprcMonitor.Right}, Bottom = {lprcMonitor.Bottom}");
            Console.WriteLine();
            return true;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowInfo(IntPtr hWnd, out WINDOWINFO pwi);

        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        public const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        public const long WS_EX_TOOLWINDOW = 0x00000080L;

        private static bool EnumWindowsTest(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

                WINDOWINFO info = new WINDOWINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
                GetWindowInfo(hWnd, out info);

                //TODO: check for if the actual program itself is being filtered out
                if (windowTitle.Length > 0 && ((info.dwExStyle & WS_EX_NOREDIRECTIONBITMAP) == 0) && ((info.dwExStyle & WS_EX_TOOLWINDOW) == 0))
                {
                    RECT rect;
                    if (GetWindowRect(hWnd, out rect))
                    {
                        //If 2 windows have same coord, the one returned first is on top
                        //Design choice: How to handle minimize windows?
                        Console.WriteLine($"Window Handle: {hWnd}");
                        Console.WriteLine($"Window Title: {windowTitle}");
                        Console.WriteLine($"Window Info: rcWindow = {info.rcWindow}, rcClient = {info.rcClient}, dwStyle = {info.dwStyle.ToString("X")}, dwExStyle = {info.dwExStyle.ToString("X")}, dwWindowStatus = {info.dwWindowStatus}");
                        Console.WriteLine($"Window Rect: Left = {rect.Left}, Top = {rect.Top}, Right = {rect.Right}, Bottom = {rect.Bottom}");
                        Console.WriteLine();
                    }
                }

            }
            return true;
        }
    }
}
