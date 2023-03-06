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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// GetPwdUC.xaml 的交互逻辑
    /// </summary>
    public partial class GetPwdUC : BaseUserControl
    {
        public string _Mobile { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public GetPwdUC()
        {
            InitializeComponent();

            this.Loaded += GetPwdUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetPwdUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSave.IsEnabled = false;
            this.txtCode.IsEnabled = false;
            this.txtNewPwd.IsEnabled = false;

            this.btnSend.Click += BtnSend_Click;
            this.btnSave.Click += BtnSave_Click;

            this.txtMobile.Focus();
        }
        /// <summary>
        /// 保存新密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string code = this.txtCode.Text.Trim();
            string newPwd = this.txtNewPwd.Text.Trim();
            if (string.IsNullOrWhiteSpace(code) || code.Length != 5)
            {
                AppAlert.FormTips(gridMain, "请输入正确的验证码！", AppCode.Enums.FormTipsType.Info);
                return;
            }
            if (string.IsNullOrWhiteSpace(newPwd) || newPwd.Length < 6)
            {
                AppAlert.FormTips(gridMain, "请输入最少6位密码！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            this.txtCode.IsEnabled = false;
            this.txtNewPwd.IsEnabled = false;
            this.btnSend.IsEnabled = false;
            this.btnSave.IsEnabled = false;

            //等待
            Components.FormTipsView viewTips = AppAlert.FormTips(gridMain, "请稍候，正在保存...", AppCode.Enums.FormTipsType.Waiting, false);

            //取消定时
            AppTimer.SecondsRun_Event -= AppTimer_SecondsRun_Event;

            //线程
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //保存到服务器
                bool flag = false;

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    if (flag)
                    {
                        //设置成功
                        this.gridMain.Children.Remove(viewTips);
                        AppAlert.FormTips(gridMain, "新密码设置成功！", AppCode.Enums.FormTipsType.Right);
                        AppData.MainWindow.RemovePage(_CurrentPageIndex);
                    }
                    else
                    {
                        //设置失败
                        this.gridMain.Children.Remove(viewTips);
                        AppAlert.FormTips(gridMain, "新密码设置失败！", AppCode.Enums.FormTipsType.Error);

                        this.txtCode.IsEnabled = true;
                        this.txtNewPwd.IsEnabled = true;
                        this.btnSend.IsEnabled = true;
                        this.btnSave.IsEnabled = true;

                        this.txtCode.Text = "";
                        this.btnSend.Content = "重发";
                    }

                    return null;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string mobile = this.txtMobile.Text.Trim();

            bool flag = Core.Handler.RegexHandler.IsMobile(mobile);

            if (!flag)
            {
                AppAlert.FormTips(gridMain, "请输入正确的手机号码！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            this.btnSend.IsEnabled = false;
            this.txtCode.IsEnabled = false;
            this.txtMobile.IsEnabled = false;
            this.btnSave.IsEnabled = false;

            //手机号
            _Mobile = mobile;

            //线程
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //调用发送短信接口


                //发送成功
                SendSMSSuccess();
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 发送验证码成功
        /// </summary>
        private void SendSMSSuccess()
        {
            AppTimer.SecondsRun_Event += AppTimer_SecondsRun_Event;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.txtCode.IsEnabled = true;
                this.txtNewPwd.IsEnabled = true;
                this.btnSave.IsEnabled = true;
                this.txtCode.Focus();
                return null;
            }));
        }
        int sendSeconds = 0;
        int maxWaitSeconds = 10;
        /// <summary>
        /// 每秒事件
        /// </summary>
        /// <param name="runIndex"></param>
        private void AppTimer_SecondsRun_Event(long runIndex)
        {
            sendSeconds++;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                if (sendSeconds == maxWaitSeconds)
                {
                    //移除事件
                    AppTimer.SecondsRun_Event -= AppTimer_SecondsRun_Event;

                    sendSeconds = 0;
                    this.btnSend.IsEnabled = true;
                    this.btnSend.Content = "重发";
                    return null;
                }

                this.btnSend.Content = (maxWaitSeconds - sendSeconds).ToString();

                return null;
            }));
        }
    }
}
