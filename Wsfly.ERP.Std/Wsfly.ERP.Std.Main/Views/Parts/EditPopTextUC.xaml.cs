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
    /// EditPopTextUC.xaml 的交互逻辑
    /// </summary>
    public partial class EditPopTextUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public EditPopTextUC(string text)
        {
            InitializeComponent();

            this.txtContent.Text = text;

            this.Loaded += EditPopTextUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditPopTextUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnCancel.Click += BtnCancel_Click;
            this.btnOK.Click += BtnOK_Click;

            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            this.txtContent.Focus();
        }
        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }
        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitSize()
        {
            this.Width = _ParentWindow.Width - 48;
            this.Height = _ParentWindow.Height - 70;
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            string text = this.txtContent.Text;

            if(_ParentWindow is Components.PageWindow) (_ParentWindow as Views.Components.PageWindow).CallBack(text);
            else if (_ParentWindow is Components.PopWindow) (_ParentWindow as Views.Components.PopWindow).CallBack(text);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_ParentWindow is Components.PageWindow) (_ParentWindow as Views.Components.PageWindow).Close();
            else if (_ParentWindow is Components.PopWindow) (_ParentWindow as Views.Components.PopWindow).Close();
        }
    }
}
