using Wsfly.ERP.Std.Core.Base;
using Wsfly.ERP.Std.AppCode.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// ExitTipsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExitTipsWindow : WsflyWindow
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ExitTipsWindow()
        {
            InitUI();
        }
        /// <summary>
        /// 初始界面 UI
        /// </summary>
        private void InitUI()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ExitTipsWindow_Loaded);
            this.btnOK.Click += new RoutedEventHandler(btnOK_Click);
            this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ExitTipsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbNotExit.Checked += CbNotExit_Checked;
            this.cbNotExit.Unchecked += CbNotExit_Unchecked;
        }

        /// <summary>
        /// 退出系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbNotExit_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lblTips.Text = "是否确定退出程序？";
        }
        /// <summary>
        /// 选择 最小化到托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbNotExit_Checked(object sender, RoutedEventArgs e)
        {
            this.lblTips.Text = "切换到后台运行，且不再提示！";
        }

        /// <summary>
        /// 取消事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //隐藏主窗口
            _ParentWindow.Hide();

            //显示到托盘
            if (_ParentWindow is MainWindow)
            {
                (_ParentWindow as MainWindow).ShowNotifyTips("最小化到这里了，双击显示界面！");
            }

            //关闭窗口
            this.Close();
        }
        /// <summary>
        /// 确定按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.cbNotExit.IsChecked.HasValue && this.cbNotExit.IsChecked.Value)
                {
                    if (AppGlobal.UserInfo != null)
                    {
                        //用户配置
                        if (AppGlobal.UserConfig == null)
                        {
                            AppGlobal.UserConfig = new UserConfig()
                            {
                                UserId = AppGlobal.UserInfo.UserId,
                                IsDirctExit = false,
                                IsFullScreen = true,
                                ThemeName = "Default"
                            };
                        }

                        //保存配置
                        if (AppGlobal.UserConfig != null) AppGlobal.UserConfig.IsDirctExit = false;
                        if (AppData.MainWindow != null) AppData.MainWindow.SaveUserConfig();
                    }

                    //隐藏弹窗
                    this.Hide();
                    //隐藏主窗口
                    if (_ParentWindow != null) _ParentWindow.Hide();

                    //关闭窗口
                    this.Close();
                }
                else
                {
                    //关闭窗口
                    this.Close();
                    //直接退出
                    if (AppData.MainWindow != null) AppData.MainWindow.ExitApp();
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "确定退出程序异常！");
            }
        }
    }
}
