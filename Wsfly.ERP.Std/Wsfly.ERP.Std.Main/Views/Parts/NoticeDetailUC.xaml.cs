using KeQuan.Client.PC.AppCode.Base;
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

namespace KeQuan.Client.PC.Views.PopWin
{
    /// <summary>
    /// NoticeDetailUC.xaml 的交互逻辑
    /// </summary>
    public partial class NoticeDetailUC : BaseUserControl
    {
        System.Data.DataRow _row = null;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="row"></param>
        public NoticeDetailUC(System.Data.DataRow row)
        {
            _row = row;

            InitializeComponent();

            this.Loaded += NoticeDetailUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoticeDetailUC_Loaded(object sender, RoutedEventArgs e)
        {
            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            //隐藏滚动条
            (_ParentWindow as Ctls.PageWindow).scrollMainFrame.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            (_ParentWindow as Ctls.PageWindow).scrollMainFrame.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            //事件
            this.btnClose.Click += BtnClose_Click;

            //赋值
            this.lblTitle.Text = _row["Title"].ToString();
            this.lblAuthor.Text = _row["UserName"].ToString();
            this.lblCreateDate.Text = DataType.DateTime(_row["CreateDate"].ToString(), DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
            this.lblAttachment.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrWhiteSpace(_row["Attachment"].ToString()))
            {
                this.lblAttachment.Text = "附件：" + _row["Attachment"].ToString();
                this.lblAttachment.Visibility = Visibility.Visible;
                this.lblAttachment.Tag = _row["Attachment"].ToString();
                this.lblAttachment.MouseLeftButtonDown += LblAttachment_MouseLeftButtonDown;
            }

            //显示等待
            ShowLoading(gridMain);

            //下载内容
            AppGlobal.HTMLToLocal(_row["Content"].ToString(), new AppGlobal.HTMLToLocalSuccess_Delegate(delegate (string html)
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //内容
                    this.webBrowser.DocumentText = html;
                    HideLoading();
                    return null;
                }));
            }));
        }

        /// <summary>
        /// 点击附件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblAttachment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //下载文件
            string path = (sender as TextBlock).Tag.ToString();
            if (string.IsNullOrWhiteSpace(path)) return;

            //保存目录
            string saveDir = Client.Core.Handler.UploadFileHandler.ChooseFolderDialog();
            if (string.IsNullOrWhiteSpace(saveDir)) return;

            //设置状态
            this.lblAttachment.Text += "[正在下载...]";

            try
            {
                //调用下载
                AppGlobal.DownloadServerFile(path, saveDir, new AppGlobal.DownloadFileSuccess_Delegate(delegate (string savePath)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.lblAttachment.Text = this.lblAttachment.Text.Replace("正在下载...", "下载成功");
                        System.Diagnostics.Process.Start(savePath);
                        return null;
                    }));
                }));
            }
            catch { }
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始大小
        /// </summary>
        private void InitSize()
        {
            this.webBrowser.Width = Convert.ToInt32(_ParentWindow.Width - 30);
            this.webBrowser.Height = Convert.ToInt32(_ParentWindow.Height - 220);

            this.WindowsFormsHost.Width = Convert.ToInt32(_ParentWindow.Width - 30);
            this.WindowsFormsHost.Height = Convert.ToInt32(_ParentWindow.Height - 220);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            (this._ParentWindow as Ctls.PageWindow).Close();
        }
    }
}
