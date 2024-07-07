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
            //Console.WriteLine($"monitor Rect: Left = {lprcMonitor.Left}, Top = {lprcMonitor.Top}, Right = {lprcMonitor.Right}, Bottom = {lprcMonitor.Bottom}");
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


        public enum DwmWindowAttribute
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hWnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

        private bool EnumWindowsTest(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                StringBuilder windowTitle = new StringBuilder(256);
                GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

                WINDOWINFO info = new WINDOWINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
                GetWindowInfo(hWnd, out info);

                DwmWindowAttribute dwAttribute = DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS;
                RECT dimensions;
                DwmGetWindowAttribute(hWnd, dwAttribute, out dimensions, Marshal.SizeOf(typeof(RECT)));
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
                        Console.WriteLine($"Window Title: {windowTitle}");
                        Console.WriteLine($"Window Rect: Left = {dimensions.Left}, Top = {dimensions.Top}, Right = {dimensions.Right}, Bottom = {dimensions.Bottom}");
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

        private bool IsWindowOnMonitor(RECT windowCoords, RECT monitorCoords)
        {
            return ((windowCoords.Left < monitorCoords.Right)
                && (windowCoords.Top < monitorCoords.Bottom)
                && (windowCoords.Right > monitorCoords.Left)
                && (windowCoords.Bottom > monitorCoords.Top));
        }

        //Mathematical way to determine the border?
        private void SwapMonitors() 
        {
            //TODO: what if window is in both monitors? just leave it?
            foreach (WINDOW window in windows)
            {
                foreach (RECT monitor in monitors)
                {
                    IsWindowOnMonitor(window.rect, monitor);
                }
            }
            Console.WriteLine("printing");
            Console.WriteLine(monitors.Count);
            Console.WriteLine(windows.Count);
        }
    }
}
