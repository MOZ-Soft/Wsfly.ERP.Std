using System;
using System.Collections.Generic;
using System.Data;
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
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// XMLEditorUC.xaml 的交互逻辑
    /// </summary>
    public partial class XMLEditorUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public XMLEditorUC(string xml)
        {
            InitializeComponent();

            this.txtXML.Text = xml;

            this.Loaded += XMLEditorUC_Loaded;
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XMLEditorUC_Loaded(object sender, RoutedEventArgs e)
        {
            InitSize();

            this.btnOk.Click += BtnOk_Click;
            this.btnCancel.Click += BtnCancel_Click;
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var pageWindow = this._ParentWindow as Components.PageWindow;
            pageWindow.Close();
        }
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string xml = this.txtXML.Text.Trim();

            var pageWindow = this._ParentWindow as Components.PageWindow;
            pageWindow.CallBack(xml);
        }


        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitSize()
        {
            this.Height = AppData.MainWindow.WinHeight - 100;
            this.txtXML.Height = this.Height - 120;
        }
        
    }
}
