using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// Windows系统 任务栏隐藏
    /// </summary>
    public class SystemTaskBarHandler
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        public static Point GetCursorPos()
        {
            Point point = new Point();
            GetCursorPos(ref point);
            return point;
        }

        /// <summary>
        /// 隐藏任务栏
        /// </summary>
        /// <param name="isHide"></param>
        public static void HideTask(bool isHide)
        {
            try
            {
                IntPtr trayHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
                IntPtr hStar = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Button", null);

                if (isHide)
                {
                    ShowWindow(trayHwnd, 0);
                    ShowWindow(hStar, 0);
                }
                else
                {
                    ShowWindow(trayHwnd, 1);
                    ShowWindow(hStar, 1);
                }
            }
            catch { }
        }
    }
}
