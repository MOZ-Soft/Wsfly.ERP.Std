using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// PC信息
    /// </summary>
    public class PCHandler
    {
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        [DllImport("user32")]
        public static extern void LockWorkStation();
        [DllImport("user32")]
        public static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        #region 获取PC信息
        /// <summary>
        /// 获取主机信息
        /// CPU ：Win32_Processor
        /// 主板：Win32_BaseBoard
        /// 硬盘：Win32_DiskDrive
        /// </summary>
        /// <returns></returns>
        public static string GetPCID(string idName)
        {
            try
            {
                ManagementClass mc = new ManagementClass(idName);
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (idName.Equals("Win32_Processor"))
                    {
                        //CPU
                        return mo.Properties["ProcessorId"].Value.ToString();
                    }
                    else if (idName.Equals("Win32_BaseBoard"))
                    {
                        //主板
                        return mo.Properties["SerialNumber"].Value.ToString();
                    }
                    else if (idName.Equals("Win32_DiskDrive"))
                    {
                        //硬盘
                        return mo.Properties["SerialNumber"].Value.ToString();
                    }
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region PC操作
        /// <summary>
        /// 显示器状态
        /// </summary>
        public enum MonitorState
        {
            MonitorStateOn = -1,
            MonitorStateOff = 2,
            MonitorStateStandBy = 1
        }

        /// <summary>
        /// 关机
        /// </summary>
        public static void ShutDown()
        {
            try
            {
                AppLog.WriteDebugLog("关机");

                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-s -t 00");
                System.Diagnostics.Process.Start(startinfo);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用关机异常！");
            }
        }
        /// <summary>
        /// 重启
        /// </summary>
        public static void Restart()
        {
            try
            {
                AppLog.WriteDebugLog("重启");

                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -t 00");
                System.Diagnostics.Process.Start(startinfo);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用重启异常！");
            }
        }
        /// <summary>
        /// 注销
        /// </summary>
        public static void LogOff()
        {
            try
            {
                AppLog.WriteDebugLog("注销");

                ExitWindowsEx(0, 0);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用注销异常！");
            }
        }
        /// <summary>
        /// 锁定
        /// </summary>
        public static void LockPC()
        {
            try
            {
                AppLog.WriteDebugLog("锁定");

                LockWorkStation();
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用锁定异常！");
            }
        }
        /// <summary>
        /// 关闭显示器
        /// </summary>
        public static void TurnOffMonitor()
        {
            AppLog.WriteDebugLog("关闭显示器");

            SetMonitorInState(MonitorState.MonitorStateOff);
        }
        /// <summary>
        /// 设置显示器状态
        /// </summary>
        /// <param name="state"></param>
        private static void SetMonitorInState(MonitorState state)
        {
            try
            {
                SendMessage(0xFFFF, 0x112, 0xF170, (int)state);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "设置显示器状态异常！");
            }
        }
        #endregion

        #region 操作任务栏
        /// <summary>
        /// 获取屏幕坐标 
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);
        /// <summary>
        /// 查找窗口
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpszClass"></param>
        /// <param name="lpszWindow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        /// <summary>
        /// 获取屏幕坐标
        /// </summary>
        /// <returns></returns>
        public static System.Drawing.Point GetCursorPos()
        {
            System.Drawing.Point point = new System.Drawing.Point();
            GetCursorPos(ref point);
            return point;
        }
        /// <summary>
        /// 隐藏、显示任务栏
        /// </summary>
        /// <param name="isHide">是否隐藏</param>
        public static void HideTask(bool isHide)
        {
            try
            {
                //任务栏
                IntPtr trayHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
                //开始菜单
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
        #endregion
    }
}
