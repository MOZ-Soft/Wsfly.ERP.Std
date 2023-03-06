using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// 任务栏图标背景闪烁
    /// </summary>
    public static class WindowExtensions
    {
        #region Window Flashing API Stuff  

        private const UInt32 FLASHW_STOP = 0; //Stop flashing. The system restores the window to its original state.  
        private const UInt32 FLASHW_CAPTION = 1; //Flash the window caption.  
        private const UInt32 FLASHW_TRAY = 2; //Flash the taskbar button.  
        private const UInt32 FLASHW_ALL = 3; //Flash both the window caption and taskbar button.  
        private const UInt32 FLASHW_TIMER = 4; //Flash continuously, until the FLASHW_STOP flag is set.  
        private const UInt32 FLASHW_TIMERNOFG = 12; //Flash continuously until the window comes to the foreground.  

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize; //The size of the structure in bytes.  
            public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.  
            public UInt32 dwFlags; //The Flash Status.  
            public UInt32 uCount; // number of times to flash the window  
            public UInt32 dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.  
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        #endregion

        /// <summary>
        /// 开始闪烁：FlashWindow(this Window win, UInt32 count = UInt32.MaxValue, UInt32 interval = 100)  
        /// win 主窗体，count 闪动次数微信是4次，interval 闪烁间隔毫秒
        /// WindowExtensions.FlashWindow(this, 5, 500);
        /// </summary>
        /// <param name="win"></param>
        /// <param name="count"></param>
        /// <param name="interval"></param>
        public static void FlashWindow(this Window win, UInt32 count = UInt32.MaxValue, UInt32 interval = 100)
        {
            //Don't flash if the window is active  
            //if (win.IsActive) return;

            WindowInteropHelper h = new WindowInteropHelper(win);
            FLASHWINFO info = new FLASHWINFO
            {
                hwnd = h.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMER,
                uCount = count,
                dwTimeout = interval
            };

            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            FlashWindowEx(ref info);
        }
        /// <summary>
        /// 停止闪烁
        /// </summary>
        /// <param name="win"></param>
        public static void StopFlashingWindow(this Window win)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);

            FLASHWINFO info = new FLASHWINFO();
            info.hwnd = h.Handle;
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = FLASHW_STOP;
            info.uCount = UInt32.MaxValue;
            info.dwTimeout = 0;

            FlashWindowEx(ref info);
        }
    }
}
