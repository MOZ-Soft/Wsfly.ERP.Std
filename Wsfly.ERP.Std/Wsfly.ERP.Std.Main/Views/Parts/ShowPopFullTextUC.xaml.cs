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
    /// ShowPopFullTextUC.xaml 的交互逻辑
    /// </summary>
    public partial class ShowPopFullTextUC : BaseUserControl
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ShowPopFullTextUC(string text)
        {
            Text = text;

            InitializeComponent();

            this.Loaded += ShowPopFullTextUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPopFullTextUC_Loaded(object sender, RoutedEventArgs e)
        {
            //隐藏浏览器
            this.webBrowser.Visible = false;
            this.WindowsFormsHost.Visibility = Visibility.Collapsed;

            //显示等待
            ShowLoading(gridMain);

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //浏览器
                this.WindowsFormsHost.Visibility = Visibility.Visible;
                this.webBrowser.Visible = true;
                this.webBrowser.DocumentText = BuildHtmlPage("", Text, false);

                //隐藏等待
                HideLoading();
                return null;
            }));

            this.btnClose.Click += BtnClose_Click;

            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;
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
            this.webBrowser.Width = Convert.ToInt32(_ParentWindow.Width - 40);
            this.webBrowser.Height = Convert.ToInt32(_ParentWindow.Height - 140);

            this.WindowsFormsHost.Width = Convert.ToInt32(_ParentWindow.Width - 40);
            this.WindowsFormsHost.Height = Convert.ToInt32(_ParentWindow.Height - 140);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _ParentWindow.Close();
        }
    }
}
