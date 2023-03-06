
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
using System.Xml;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// SysSettingsUC.xaml 的交互逻辑
    /// </summary>
    public partial class SysSettingsUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public SysSettingsUC()
        {
            InitializeComponent();

            this.Loaded += SysSettingsUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysSettingsUC_Loaded(object sender, RoutedEventArgs e)
        {
            InitWinSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;

            InitUI();
        }
        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitWinSize();
        }
        /// <summary>
        /// 初始窗口大小
        /// </summary>
        private void InitWinSize()
        {
            this.Width = AppData.MainWindow.WinWidth - 82;
            this.Height = AppData.MainWindow.WinHeight - 82;

            this.scrollMainFrame.Width = this.Width - 18;
            this.scrollMainFrame.Height = this.Height - 20;
        }

        /// <summary>
        /// 初始界面
        /// </summary>
        private void InitUI()
        {
            this.cbAutoRun.IsChecked = AppGlobal.LocalConfig.AutoRun;
            this.cbStatyRun.IsChecked = AppGlobal.UserConfig != null ? !AppGlobal.UserConfig.IsDirctExit : false;
            
            this.lblSoftwareName.Text = AppGlobal.GetSysConfigReturnString("System_AppName");
            this.lblAppVersion.Text = AppGlobal.GetSysConfigReturnString("System_AppVersion") + "-" + AppGlobal.LocalConfig.Version;
            this.lblServerName.Text = "erp.wsfly.com";

            this.txtIconZoom.Text = AppGlobal.LocalConfig.NavIconZoom.ToString();

            string databaseBackTimes = AppGlobal.GetSysConfigReturnString("System_DatabaseBackupTimes");
            int deleteCycle = AppGlobal.GetSysConfigReturnInt("System_DatabaseBackupExpire", 1);

            //事件
            this.btnSaveCustomerSetting.Click += BtnSaveCustomerSetting_Click;
            this.btnSaveSysConfig.Click += BtnSaveSysConfig_Click;
            this.btnSavePwd.Click += BtnSavePwd_Click;

            //点击升级更新
            this.btnUpgrade.Click += BtnUpgrade_Click;
        }

        /// <summary>
        /// 保存个性化设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveCustomerSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //放大倍数
                double zoom = DataType.Double(this.txtIconZoom.Text.Trim(), 1);
                zoom = zoom < 1 ? 1 : zoom > 3 ? 3 : zoom;
                AppGlobal.LocalConfig.NavIconZoom = zoom;
                AppGlobal.LocalConfig.Save();

                AppAlert.FormTips(gridMain, "保存配置成功！", AppCode.Enums.FormTipsType.Right);
                return;
            }
            catch (Exception ex) { }

            AppAlert.FormTips(gridMain, "保存配置失败！", AppCode.Enums.FormTipsType.Info);
        }



        /// <summary>
        /// 升级更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //启动升级，不提示
                string path = AppDomain.CurrentDomain.BaseDirectory + "Upgrade.exe";
                System.Diagnostics.Process.Start(path);
                //当前程序退出
                App.Current.Shutdown(-1);
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSavePwd_Click(object sender, RoutedEventArgs e)
        {
            string oldPwd = this.txtOldPwd.Text.Trim();
            string newPwd = this.txtNewPwd.Text.Trim();

            if (string.IsNullOrWhiteSpace(oldPwd))
            {
                AppAlert.FormTips(gridMain, "请输入旧密码！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            if (string.IsNullOrWhiteSpace(newPwd))
            {
                AppAlert.FormTips(gridMain, "请输入新密码！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            if (newPwd.Length < 6 || newPwd.Length > 20)
            {
                AppAlert.FormTips(gridMain, "新密码长度在6-20位！", AppCode.Enums.FormTipsType.Info);
                return;
            }
                        
            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //调用修改密码
                    bool flag = Service.Exts.UserService.UpdatePassword(AppGlobal.UserInfo, oldPwd, newPwd);
                    if (flag)
                    {
                        //已经修改了密码
                        AppGlobal.SetSysConfig("System_Guide_FirstUpdatePassword", "true");
                        AppData.MainWindow.ReloadGuide();
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            //成功
                            this.txtOldPwd.Text = "";
                            this.txtNewPwd.Text = "";
                            AppAlert.FormTips(gridMain, "修改密码成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            //失败
                            this.txtOldPwd.Text = "";
                            this.txtNewPwd.Text = "";
                            AppAlert.FormTips(gridMain, "修改密码失败，请重新输入！", AppCode.Enums.FormTipsType.Info);
                        }

                        HideLoading();

                        return null;
                    }));

                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 保存系统配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveSysConfig_Click(object sender, RoutedEventArgs e)
        {
            bool autoRun = this.cbAutoRun.IsChecked.Value;
            bool isStatyRun = this.cbStatyRun.IsChecked.Value;

            AppGlobal.LocalConfig.AutoRun = autoRun;
            AppGlobal.LocalConfig.Save();

            try
            {
                //设置自动启动
                Core.AppHandler.SetAutoRun(System.Reflection.Assembly.GetExecutingAssembly().Location, autoRun);
            }
            catch { }

            if (AppGlobal.UserConfig == null)
            {
                AppGlobal.UserConfig = new UserConfig()
                {
                    UserId = AppGlobal.UserInfo.UserId,
                    ThemeName = "Default",
                    IsDirctExit = isStatyRun,
                    IsFullScreen = true
                };

                AppData.MainWindow.SaveUserConfig();
                AppAlert.FormTips(gridMain, "保存系统配置成功！", AppCode.Enums.FormTipsType.Right);
                HideLoading();
            }
            else
            {
                if (AppGlobal.UserConfig.IsDirctExit == isStatyRun)
                {
                    AppGlobal.UserConfig.IsDirctExit = !isStatyRun;

                    ShowLoading(gridMain);

                    System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            SQLParam param = new SQLParam()
                            {
                                TableName = "Sys_UserConfigs",
                                OpreateCells = new List<KeyValue>()
                                {
                                 new KeyValue("IsDirctExit", !isStatyRun)
                                },
                                Wheres = new List<Where>()
                                {
                                new Where() { CellName="UserId", CellValue = AppGlobal.UserInfo.UserId }
                                }
                            };

                            //保存到用户配置
                            bool flag = SQLiteDao.Update(param);

                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                AppAlert.FormTips(gridMain, "保存系统配置成功！", AppCode.Enums.FormTipsType.Right);
                                HideLoading();
                                return null;
                            }));
                        }
                        catch
                        {
                            HideLoading();
                        }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
                else
                {
                    AppAlert.FormTips(gridMain, "保存系统配置成功！", AppCode.Enums.FormTipsType.Right);
                }
            }
        }
    }
}

