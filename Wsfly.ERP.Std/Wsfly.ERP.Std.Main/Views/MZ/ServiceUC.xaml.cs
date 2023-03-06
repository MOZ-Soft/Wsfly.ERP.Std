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
using ThoughtWorks.QRCode.Codec;
using Wsfly.ERP.Std.AppCode.Base;


namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// ServiceUC.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ServiceUC()
        {
            InitializeComponent();

            this.Loaded += ServiceUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceUC_Loaded(object sender, RoutedEventArgs e)
        {
            //二维码
            //LYP：https://u.wechat.com/ME04YoUjrMRGbOzFN4CR7AY
            //慕之：https://u.wechat.com/MJv8jyz6UWZFQZhs0f_4yb0

            string qrcodeData = "https://u.wechat.com/MJv8jyz6UWZFQZhs0f_4yb0";
            QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//编码方式(注意：BYTE能支持中文，ALPHA_NUMERIC扫描出来的都是数字)
            encoder.QRCodeScale = 2;
            encoder.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
            System.Drawing.Bitmap bitmap = encoder.Encode(qrcodeData);//生成二维码
            this.imgWXQrCode.Source = Core.Handler.ImageBrushHandler.GetImageSource(bitmap);
            bitmap.Dispose();

            this.lblSoftwareName.Text = AppGlobal.GetSysConfigReturnString("System_AppName");
            this.lblAppVersion.Text = AppGlobal.GetSysConfigReturnString("System_AppVersion") + "-" + AppGlobal.LocalConfig.Version;
            this.lblServerName.Text = "erp.wsfly.com";
            this.lblServerName.Foreground = Brushes.Blue;
            this.lblServerName.MouseDown += LblServerName_MouseDown;

            InitWinSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }
        /// <summary>
        /// 打开官网
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblServerName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://erp.wsfly.com/Moz/Index");
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
            
            this.borderService.Height = this.Height;
        }
    }
}
