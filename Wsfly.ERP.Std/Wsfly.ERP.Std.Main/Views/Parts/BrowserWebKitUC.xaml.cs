
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
    /// BrowserWebKitUC.xaml 的交互逻辑
    /// </summary>
    public partial class BrowserWebKitUC : BaseUserControl
    {
        private string _url = null;

        /// <summary>
        /// 构造
        /// </summary>
        public BrowserWebKitUC(string url)
        {
            _url = url;

            InitializeComponent();

            this.Loaded += BrowserWebKitUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserWebKitUC_Loaded(object sender, RoutedEventArgs e)
        {
            ////浏览器核
            //WebKit.WebKitBrowser browser = new WebKitBrowser();
            //browser.Dock = DockStyle.Fill;

            ////Host
            //System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            //host.Child = browser;
            //this.gridMain.Children.Add(host);

            ////加载页面
            //browser.Navigate(_url);
        }
    }
}
