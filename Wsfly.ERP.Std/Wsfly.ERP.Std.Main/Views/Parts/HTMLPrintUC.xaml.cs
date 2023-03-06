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

using System.Runtime.InteropServices;
using SHDocVw;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// HTMLPrintUC.xaml 的交互逻辑
    /// </summary>
    public partial class HTMLPrintUC : BaseUserControl
    {
        /// <summary>
        /// 要打印的HTML
        /// </summary>
        string _Html = string.Empty;

        /// <summary>
        /// 打印容器
        /// </summary>
        SHDocVw.IWebBrowser2 _printBrowser = null;

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }
        static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");

        /// <summary>
        /// 构造
        /// </summary>
        public HTMLPrintUC(string html)
        {
            _Html = html;

            InitializeComponent();

            this.Loaded += HTMLPrintUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HTMLPrintUC_Loaded(object sender, RoutedEventArgs e)
        {
            //显示内容
            this.browser.NavigateToString(ConvertExtendedASCII(_Html));

            //事件
            this.btnPrint.Click += BtnPrint_Click;
            this.btnPreview.Click += BtnPreview_Click;
            this.btnSetting.Click += BtnSetting_Click;
            this.btnCancel.Click += BtnCancel_Click;
        }

        /// <summary>
        /// 转换编码
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        private static string ConvertExtendedASCII(string HTML)
        {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127) retVal += "&#" + Convert.ToInt32(c) + ";";
                else retVal += c;
            }

            return retVal;
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            //获取打印容器
            GetPrinter();
            //打印：
            _printBrowser.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINT, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER);
        }

        /// <summary>
        /// 预览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            //获取打印容器
            GetPrinter();
            //打印预览：
            _printBrowser.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINTPREVIEW, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            //获取打印容器
            GetPrinter();
            //页面设置：
            _printBrowser.ExecWB(SHDocVw.OLECMDID.OLECMDID_PAGESETUP, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_PROMPTUSER);
        }

        /// <summary>
        /// 获取打印容器
        /// </summary>
        private void GetPrinter()
        {
            //是否有已经有容器
            if (_printBrowser != null) return;

            //服务接口
            IServiceProvider serviceProvider = null;

            //得到文档
            if (this.browser.Document != null)
            {
                serviceProvider = (IServiceProvider)this.browser.Document;
            }
            Guid serviceGuid = SID_SWebBrowserApp;
            Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;

            //打印窗口
            _printBrowser = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._ParentWindow.Close();
        }
    }
}
