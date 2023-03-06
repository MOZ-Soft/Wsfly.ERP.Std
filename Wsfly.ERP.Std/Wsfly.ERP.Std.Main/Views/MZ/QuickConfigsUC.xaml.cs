
using Wsfly.ERP.Std.AppCode.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// QuickConfigsUC.xaml 的交互逻辑
    /// </summary>
    public partial class QuickConfigsUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public QuickConfigsUC()
        {
            InitializeComponent();

            this.Loaded += QuickConfigsUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickConfigsUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSetAllTableIsSystem.Click += BtnSetAllTableIsSystem_Click;
            this.btnSetAllCellIsSystem.Click += BtnSetAllCellIsSystem_Click;
            
            this.btnClearTempFiles.Click += BtnClearTempFiles_Click;

            this.btnDownloadAllReports.Click += BtnDownloadAllReports_Click;
            this.btnDownloadAllFiles.Click += BtnDownloadAllFiles_Click;
        }

        #region 快捷文件
        /// <summary>
        /// 下载所有报表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownloadAllReports_Click(object sender, RoutedEventArgs e)
        {
            string reportPath = AppDomain.CurrentDomain.BaseDirectory + "AppData\\Reports\\";
            if (!System.IO.Directory.Exists(reportPath)) System.IO.Directory.CreateDirectory(reportPath);

            try
            {
                //查询所有报表文件
                SQLParam param = new SQLParam()
                {
                    
                    TableName = "Sys_PrintTemplates",
                };
                DataTable dt = SQLiteDao.GetTable(param);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    AppAlert.FormTips(gridMain, "还没有任何报表文件！");
                    return;
                }

                //将报表文件保存到本地
                foreach (DataRow row in dt.Rows)
                {
                    string path = row["TemplatePath"].ToString();
                }

                //打开目录
                System.Diagnostics.Process.Start(reportPath);
                AppAlert.FormTips(gridMain, "下载所有报表成功！");
            }
            catch (Exception ex)
            {
                AppAlert.FormTips(gridMain, "下载所有报表失败！");
            }
        }
        /// <summary>
        /// 下载所有文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownloadAllFiles_Click(object sender, RoutedEventArgs e)
        {
            string reportPath = AppDomain.CurrentDomain.BaseDirectory + "AppData\\Files\\";
            if (!System.IO.Directory.Exists(reportPath)) System.IO.Directory.CreateDirectory(reportPath);

            try
            {
                //查询所有文件
                SQLParam param = new SQLParam()
                {
                    
                    TableName = "Sys_Files",
                };
                DataTable dt = SQLiteDao.GetTable(param);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    AppAlert.FormTips(gridMain, "还没有任何文件！");
                    return;
                }

                //将报表文件保存到本地
                foreach (DataRow row in dt.Rows)
                {
                    //MZServer:\Uploads\1\20170512\力卡系统流程图.jpg
                    string path = row["FilePath"].ToString();
                }

                //打开目录
                System.Diagnostics.Process.Start(reportPath);
                AppAlert.FormTips(gridMain, "下载所有文件成功！");
            }
            catch (Exception ex)
            {
                AppAlert.FormTips(gridMain, "下载所有文件失败！");
            }
        }
        #endregion

        #region 快捷清除
        /// <summary>
        /// 清除所有临时文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearTempFiles_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                bool flag = false;

                try
                {
                    string tempFile = AppDomain.CurrentDomain.BaseDirectory + "AppData\\_Temp\\";
                    Core.Handler.FileHandler.DeleteFolder(tempFile);

                    tempFile = AppDomain.CurrentDomain.BaseDirectory + "AppData\\_Temp2\\";
                    Core.Handler.FileHandler.DeleteFolder(tempFile);

                    flag = true;
                }
                catch { }

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    if (flag)
                    {
                        AppAlert.FormTips(gridMain, "清除临时文件成功！");
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "清除临时文件失败！");
                    }
                    return null;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 快捷设置
        /// <summary>
        /// 所有列为系统列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetAllCellIsSystem_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"update [Sys_TableCells] set [IsSystem]=1";
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool flag = SQLiteDao.ExecuteSQL(sql);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            AppAlert.FormTips(gridMain, "所有列设置为系统列成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "所有列设置为系统列失败！");
                        }
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "所有列设置为系统列异常");
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "所有列设置为系统列失败！");
                        return null;
                    }));
                }
                finally
                {
                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 所有表为系统表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetAllTableIsSystem_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"update [Sys_Tables] set [IsSystem]=1";
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool flag = SQLiteDao.ExecuteSQL(sql);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            AppAlert.FormTips(gridMain, "所有表设置为系统表成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "所有表设置为系统表失败！");
                        }
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "所有表设置为系统表异常");
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "所有表设置为系统表失败！");
                        return null;
                    }));
                }
                finally
                {
                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion
    }
}
