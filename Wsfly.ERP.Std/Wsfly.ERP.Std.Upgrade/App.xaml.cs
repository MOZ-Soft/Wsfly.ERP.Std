using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Wsfly.ERP.Std.Upgrade
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 构造
        /// </summary>
        public App()
        {
            //异常处理
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            SignStart();
        }

        #region 未处理异常
        /// <summary>
        /// 非UI线程抛出的未处理异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //记录异常信息
            Exception ex = (Exception)e.ExceptionObject;

            //记录异常信息
            AppLog.WriteBugLog(ex, "非UI线程抛出的未处理异常");
        }
        /// <summary>
        /// UI线程的未处理异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //记录异常信息
            AppLog.WriteBugLog(e.Exception, "UI线程的未处理异常");

            //标记已经处理
            e.Handled = true;
        }
        #endregion

        #region 单进程实例
        /// <summary>
        /// 单进程实例
        /// </summary>
        private void SignStart()
        {
            try
            {
                Process process = Process.GetCurrentProcess();
                foreach (Process p in Process.GetProcessesByName(process.ProcessName))
                {
                    if (p.Id != process.Id)
                    {
                        //关闭上一个程序
                        p.Kill();
                        return;
                    }
                }
            }
            catch { }
        }
        #endregion
    }
}
