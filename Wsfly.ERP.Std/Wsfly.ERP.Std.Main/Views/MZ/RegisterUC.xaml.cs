
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
using Wsfly.ERP.Std.AppCode.Handler;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// RegisterUC.xaml 的交互逻辑
    /// </summary>
    public partial class RegisterUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public RegisterUC()
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
            InitUI();
        }

        /// <summary>
        /// 初始界面
        /// </summary>
        private void InitUI()
        {
            this.btnOk.Click += BtnOk_Click;
        }
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = this.txtName.Text.Trim();
                string telphone = this.txtTelphone.Text.Trim();

                if (string.IsNullOrWhiteSpace(name))
                {
                    this.lblTips.Text = "请输入企业名称！";
                    return;
                }
                if (string.IsNullOrWhiteSpace(telphone))
                {
                    this.lblTips.Text = "请输入联系电话！";
                    return;
                }

                this.lblTips.Text = "请稍候，正在提交认证资料...";
                this.txtName.IsEnabled = false;
                this.txtTelphone.IsEnabled = false;
                this.btnOk.IsEnabled = false;

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        //提交注册
                        var result = AppCode.API.XTAPI.Regist(name, telphone);
                        if (result.Success)
                        {
                            //重新加载系统配置
                            AppGlobal.LoadSysConfigs();
                            //重新加载引导
                            AppData.MainWindow.ReloadGuide();

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.lblTips.Text = "认证成功！";
                                AppData.MainWindow.Registed();
                            }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.lblTips.Text = result.Message;

                                this.txtName.IsEnabled = true;
                                this.txtTelphone.IsEnabled = true;
                                this.btnOk.IsEnabled = true;
                            }));
                        }
                    }
                    catch (Exception ex) { }
                });
                thread.Start();
            }
            catch (Exception ex) { }
        }
    }
}

