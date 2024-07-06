using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

namespace Assist
{
    internal class HandleWindows
    {
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
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
        public struct WINDOW
        {
            public RECT rect;
            public IntPtr hWnd;

            public WINDOW(RECT rect, IntPtr hWnd)
            {
                this.rect = rect;
                this.hWnd = hWnd;
            }
        }
        public const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        public const long WS_EX_TOOLWINDOW = 0x00000080L;

        public List<RECT> monitors;
        public List<WINDOW> windows;

        public HandleWindows() {
            monitors = new List<RECT>();
            windows = new List<WINDOW>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
            EnumWindows(EnumWindowsTest, IntPtr.Zero);
            SwapMonitors();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumDisplayMonitorsProc lpfnEnum, IntPtr dwData);
        public delegate bool EnumDisplayMonitorsProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            monitors.Add(lprcMonitor);
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

        private bool EnumWindowsTest(IntPtr hWnd, IntPtr lParam)
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
                        WINDOW window = new WINDOW(rect, hWnd);
                        windows.Add(window);
                        /*
                        Console.WriteLine($"Window Handle: {hWnd}");
                        Console.WriteLine($"Window Title: {windowTitle}");
                        Console.WriteLine($"Window Info: rcWindow = {info.rcWindow}, rcClient = {info.rcClient}, dwStyle = {info.dwStyle.ToString("X")}, dwExStyle = {info.dwExStyle.ToString("X")}, dwWindowStatus = {info.dwWindowStatus}");
                        Console.WriteLine($"Window Rect: Left = {rect.Left}, Top = {rect.Top}, Right = {rect.Right}, Bottom = {rect.Bottom}");
                        Console.WriteLine();
                        */
                    }
                }
            }
            return true;
        }

        private void WindowOnMonitor(RECT windowCoords)
        {
            //windowCoords.Top < bot
            //windowCoords.Bottom > top

            //windowCoords.Right > left
            //windowCoords.Left < right
        }
        private void SwapMonitors() 
        {
            //TODO: what if window is in both monitors? just leave it?
            Console.WriteLine("printing");
            Console.WriteLine(monitors.Count);
            Console.WriteLine(windows.Count);
        }
    }
}
