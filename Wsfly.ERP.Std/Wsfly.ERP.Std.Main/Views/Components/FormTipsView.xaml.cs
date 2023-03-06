using Wsfly.ERP.Std.Core;
using Wsfly.ERP.Std.AppCode.Enums;
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

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// FormTipsView.xaml 的交互逻辑
    /// </summary>
    public partial class FormTipsView : UserControl
    {
        string _Message;
        int _Seconds;
        FormTipsType _TipsType;
        int _timerAutoHideIndex = 1;
        System.Windows.Threading.DispatcherTimer _timerAutoHide;
        bool _IsShowFullBG = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="type">提示类型</param>
        /// <param name="seconds">自动关闭秒数，0-不自动关闭</param>
        public FormTipsView(string msg, FormTipsType type = FormTipsType.Info, int seconds = 5, bool isShowFullBG = false)
        {
            _Message = msg;
            _Seconds = seconds;
            _TipsType = type;
            _IsShowFullBG = isShowFullBG;

            if (string.IsNullOrWhiteSpace(msg))
            {
                this.Visibility = Visibility.Collapsed;
            }

            if (_Seconds <= 0) _Seconds = 0;

            InitUI();
        }
        /// <summary>
        /// 初始化界面 
        /// </summary>
        private void InitUI()
        {
            InitializeComponent();

            this.lblTips.Text = _Message;

            if (!_IsShowFullBG)
            {
                //得到文字大小
                System.Drawing.Size size = System.Windows.Forms.TextRenderer.MeasureText(_Message, new System.Drawing.Font(this.lblTips.FontFamily.ToString(), (float)this.lblTips.FontSize));
                this.Width = size.Width + 60;
                this.Height = 60;

                int dpi = Core.Handler.PrimaryScreen.DpiX;
                if (dpi == 120)
                {
                    //125%
                    this.Width = this.Width * 0.9;
                }
                else if (dpi == 144)
                {
                    //150%
                    this.Width = this.Width * 0.75;
                }
                else if (dpi == 192)
                {
                    //200%
                    this.Width = this.Width * 0.5;
                }
            }

            string tagPath = string.Empty;
            //SolidColorBrush brushBackground;
            //SolidColorBrush brushBorder;

            //提示图片
            ImageSource imgTips = AppGlobal.FormTips_Info;

            switch (_TipsType)
            {
                case FormTipsType.Error:
                    //提示错误
                    //tagPath = "Tips.Error.png";
                    imgTips = AppGlobal.FormTips_Error;
                    break;
                case FormTipsType.Right:
                    //提示正确
                    //tagPath = "Tips.Right.png";
                    imgTips = AppGlobal.FormTips_Right;
                    break;
                case FormTipsType.Waiting:
                    //提示等待
                    tagPath = "Tips.Waiting.gif";
                    //生成操作选择：Resource
                    //tagPath = "pack://application:,,,/Wsfly.Resources;component/Default/Images/Tips.Waiting.gif";
                    //tagPath = "/Wsfly.Resources;component/Default/Images/Tips.Waiting.gif";
                    break;
                case FormTipsType.Warning:
                    //提示警告
                    //tagPath = "Tips.Warning.png";
                    imgTips = AppGlobal.FormTips_Warning;
                    break;
                case FormTipsType.Info:
                default:
                    //提示消息
                    //tagPath = "Tips.Info.png";
                    break;
            }

            if (_TipsType == FormTipsType.Waiting)
            {
                this.imgIcon.Visibility = System.Windows.Visibility.Collapsed;
                this.imgWaiting.Visibility = System.Windows.Visibility.Visible;
                this.imgWaiting.Source = tagPath;
            }
            else
            {
                this.imgWaiting.Visibility = System.Windows.Visibility.Collapsed;
                //this.imgIcon.Source = AppHandler.GetResourceImageSource(tagPath);
                this.imgIcon.Source = imgTips;
            }

            //this.Background = brushBackground;
            //this.borderMain.BorderBrush = brushBorder;

            //定时自动关闭
            _timerAutoHide = new System.Windows.Threading.DispatcherTimer();
            _timerAutoHide.Interval = new TimeSpan(0, 0, 1);
            _timerAutoHide.Tick += new EventHandler(timerAutoHide_Tick);
            _timerAutoHide.Start();
        }
        /// <summary>
        /// 定时执行事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timerAutoHide_Tick(object sender, EventArgs e)
        {
            _timerAutoHideIndex++;

            if (_timerAutoHideIndex == _Seconds)
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
                _timerAutoHide.Stop();
            }
        }
    }
}
