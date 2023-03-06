using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// PdfiumViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PdfiumViewerWindow : BaseWindow
    {
        string _pdfPath = string.Empty;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="path"></param>
        public PdfiumViewerWindow(string path)
        {
            _pdfPath = path;

            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowState = WindowState.Maximized;

            this.Loaded += PdfiumViewerWindow_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PdfiumViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_pdfPath)) return;

            try
            {
                pdfViewer.Document = PdfDocument.Load(_pdfPath);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "加载PDF文件失败");
            }
        }
    }
}
