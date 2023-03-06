using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Wsfly.ERP.Std
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public App()
        {
            try
            {
                //提升目录权限
                Core.Handler.FileHandler.AddDirectorySecurity(AppDomain.CurrentDomain.BaseDirectory, "Everyone", System.Security.AccessControl.FileSystemRights.FullControl);
            }
            catch (Exception ex) { }

            //启动执行
            AppGlobal.FirstRun();

            //启动事件
            this.Startup += new StartupEventHandler(App_Startup);

            //异常处理
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        /// <summary>
        /// 程序启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void App_Startup(object sender, StartupEventArgs e)
        {
            //.net 4.5+ bing 小数点问题
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            //删除临时文件
            AppCode.Handler.PrintHandler.DeletePrintTempFile();

            //定时器启动
            AppTimer.Start();

            //定时事件
            Service.Exts.TimeEventService.Start();

            //单进程实例
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
                //foreach (Process p in Process.GetProcessesByName("Main.vshost"))
                foreach (Process p in Process.GetProcessesByName(process.ProcessName))
                {
                    if (p.Id != process.Id && process.MainModule.FileName.Equals(p.MainModule.FileName))
                    {
                        //通知上一实例 显示界面
                        //CallInstanceDisplay();
                        //关闭上一进程
                        p.Kill();
                        return;
                    }
                }
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 通知上一实例 显示界面
        /// </summary>
        private void CallInstanceDisplay()
        {
            Thread pipeThread = new Thread(delegate ()
            {
                try
                {
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream("127.0.0.1", "MZERPPIPE", PipeDirection.InOut, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Impersonation))
                    {
                        pipeClient.Connect();
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(pipeClient);
                        sw.WriteLine("SHOWMZERPDISPLY");
                        sw.Flush();
                        pipeClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "通知上一实例显示界面异常");
                }
                finally
                {
                    //退出程序
                    Environment.Exit(0);
                }
            });
            pipeThread.IsBackground = true;
            pipeThread.Start();
        }
        #endregion
    }
}
