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
        public struct LPMONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
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
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfoA(IntPtr hMonitor, out LPMONITORINFO lpmi);

        private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            //Console.WriteLine($"monitor Rect: Left = {lprcMonitor.Left}, Top = {lprcMonitor.Top}, Right = {lprcMonitor.Right}, Bottom = {lprcMonitor.Bottom}");
            LPMONITORINFO monitorInfo;
            monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(LPMONITORINFO));
            GetMonitorInfoA(hMonitor, out monitorInfo);
            //monitors.Add(lprcMonitor);
            monitors.Add(monitorInfo.rcWork);
            //Console.WriteLine($"monitor work area: Left = {workArea.Left}, Top = {workArea.Top}, Right = {workArea.Right}, Bottom = {workArea.Bottom}");
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

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
                        Console.WriteLine($"Window Title: {windowTitle}");
                        Console.WriteLine($"Window Rect: Left = {rect.Left}, Top = {rect.Top}, Right = {rect.Right}, Bottom = {rect.Bottom}");
                        Console.WriteLine();
                        /*
                        Console.WriteLine($"Window Handle: {hWnd}");
                        Console.WriteLine($"Window Title: {windowTitle}");
                        Console.WriteLine($"Window Info: rcWindow = {info.rcWindow}, rcClient = {info.rcClient}, dwStyle = {info.dwStyle.ToString("X")}, dwExStyle = {info.dwExStyle.ToString("X")}, dwWindowStatus = {info.dwWindowStatus}");
                        Console.WriteLine();
                        */
                    }
                }
            }
            return true;
        }

        private bool IsWindowOnMonitor(RECT windowCoords, RECT monitorCoords)
        {
            //TODO: THis only gets fullscreens
            return ((windowCoords.Left + windowCoords.Right == monitorCoords.Left + monitorCoords.Right)
                && (windowCoords.Top + windowCoords.Bottom == monitorCoords.Top + monitorCoords.Bottom));
        }

        private void SwapMonitors() 
        {
            //TODO: what if window is in both monitors? just leave it?
            List<WINDOW> windowsToSwap = new List<WINDOW>();
            //TODO: Maybe iterate backwards
            foreach (WINDOW window in windows)
            {
                foreach (RECT monitor in monitors)
                {
                    if (IsWindowOnMonitor(window.rect, monitor))
                    {
                        //call swapWindow instead here
                        windowsToSwap.Add(window);

                        SwapWindow(window);
                        break;
                    }
                }
            }
            Console.WriteLine("printing");
            Console.WriteLine(monitors.Count);
            Console.WriteLine(windows.Count);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref LPPOINT lpPoint);

        public struct LPPOINT
        {
            public long x;
            public long y;
        }


        private void SwapWindow(WINDOW window)
        {
            //+/- 1080
            RECT rect = window.rect;
            int Y  = rect.Top;
            //TODO: Temp
            if (rect.Bottom > 1080)
            {
                Y -= 1080; 
            } else
            {
                Y += 1080;
            }
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            StringBuilder windowTitle = new StringBuilder(256);
            GetWindowText(window.hWnd, windowTitle, windowTitle.Capacity);
            Console.WriteLine($"Moving: {windowTitle} to {Y}");

            LPPOINT clientCoords = new LPPOINT();

            ScreenToClient(window.hWnd, ref clientCoords);

            Console.WriteLine($"X: {clientCoords.x}, Y: {clientCoords.y}");
            if (windowTitle.ToString().Contains("otepad"))
            {
                //Need to consider the taskbar
                SetWindowPos(window.hWnd, IntPtr.Zero, 0, 0, 1920, 1030, 0x0004 | 0x0040);
            }
            //SetWindowPos(window.hWnd, IntPtr.Zero, 0, 0, 1920, 1080, 0x0004);
            //SetWindowPos(window.hWnd, IntPtr.Zero, rect.Left, Y, width, height, 0x0004);
        }
    }
}
