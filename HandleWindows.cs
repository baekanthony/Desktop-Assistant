using System;
using System.Runtime.InteropServices;
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
            public bool isMaximized;

            public WINDOW(RECT rect, IntPtr hWnd, bool isMaximized)
            {
                this.rect = rect;
                this.hWnd = hWnd;
                this.isMaximized = isMaximized;
            }
        }

        //Caching?
        public List<RECT> monitors;
        public List<WINDOW> windows;

        public HandleWindows() {
            monitors = new List<RECT>();
            windows = new List<WINDOW>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumDisplayMonitorsProc lpfnEnum, IntPtr dwData);
        public delegate bool EnumDisplayMonitorsProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfoA(IntPtr hMonitor, out LPMONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            LPMONITORINFO monitorInfo;
            monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(LPMONITORINFO));
            GetMonitorInfoA(hMonitor, out monitorInfo);
            monitors.Add(monitorInfo.rcWork);
            return true;
        }

        private bool WindowEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                WINDOWINFO info = new WINDOWINFO();
                info.cbSize = (uint)Marshal.SizeOf(typeof(WINDOWINFO));
                GetWindowInfo(hWnd, out info);

                const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
                const long WS_EX_TOOLWINDOW = 0x00000080L;
                const long WS_MAXIMIZE = 0x01000000L;

                if (((info.dwExStyle & WS_EX_NOREDIRECTIONBITMAP) == 0) && ((info.dwExStyle & WS_EX_TOOLWINDOW) == 0))
                {
                    WINDOW window = new WINDOW(info.rcClient, hWnd, (info.dwStyle & WS_MAXIMIZE) > 0);
                    windows.Add(window);
                }
            }
            return true;
        }

        private bool IsWindowOnMonitor(RECT windowCoords, RECT monitorCoords)
        {
            //Currently only swaps if window is perfectly inside a monitor's coordinates
            return (windowCoords.Left >= monitorCoords.Left
                && windowCoords.Top >= monitorCoords.Top
                && windowCoords.Right <= monitorCoords.Right
                && windowCoords.Bottom <= monitorCoords.Bottom);
        }

        //TODO: What if called with only 1 monitor?
        public void SwapMonitors(int monitorNum1, int monitorNum2)
        {
            monitorNum1 -= 1;
            monitorNum2 -= 1;
            int[] monitorNums = Properties.Settings.Default.MonitorNums;
            if (monitorNums != null)
            {
                monitorNum1 = monitorNums[monitorNum1];
                monitorNum2 = monitorNums[monitorNum2];
            }
            EnumWindows(WindowEnumProc, IntPtr.Zero);
            RECT original;
            RECT target;
            foreach (WINDOW window in windows)
            {
                if (IsWindowOnMonitor(window.rect, monitors[monitorNum1]))
                {
                    original = monitors[monitorNum1];
                    target = monitors[monitorNum2];
                }
                else if (IsWindowOnMonitor(window.rect, monitors[monitorNum2]))
                {
                    original = monitors[monitorNum2];
                    target = monitors[monitorNum1];
                }
                else
                {
                    break;
                }
                SwapWindow(window, original, target);
            }
        }

        //where input is 2 RECTs of 2 monitors
        private (double wRatio, double hRatio) ScalingResolutions(RECT original, RECT target)
        {
            int originalWidth = original.Right - original.Left;
            int originalHeight = original.Bottom - original.Top;

            int targetWidth = target.Right - target.Left;
            int targetHeight = target.Bottom - target.Top;

            double widthRatio = (double) targetWidth / originalWidth;
            double heightRatio = (double) targetHeight / originalHeight;
            return (widthRatio, heightRatio);
        }

        private void SwapWindow(WINDOW window, RECT original, RECT target)
        {
            RECT windowRect = window.rect;

            var (wRatio, hRatio) = ScalingResolutions(original, target);

            int x = (int) (windowRect.Left * wRatio) + (target.Left - original.Left);
            int y = (int) (windowRect.Top * hRatio) + (target.Top - original.Top);

            int width = (int) ((windowRect.Right - windowRect.Left) * wRatio);
            int height = (int)((windowRect.Bottom - windowRect.Top) * hRatio);

            const int SW_NORMAL = 1;
            const int SW_MAXIMIZE = 3;
            const uint SWP_SHOWWINDOW = 0x0040;

            if (window.isMaximized)
            {
                ShowWindow(window.hWnd, SW_NORMAL);
                SetWindowPos(window.hWnd, IntPtr.Zero, x, y, width, height - 1, SWP_SHOWWINDOW);
                ShowWindow(window.hWnd, SW_MAXIMIZE);
            } 
            else
            {
                SetWindowPos(window.hWnd, IntPtr.Zero, x, y, width, height, SWP_SHOWWINDOW);
            }
        }
    }
}
