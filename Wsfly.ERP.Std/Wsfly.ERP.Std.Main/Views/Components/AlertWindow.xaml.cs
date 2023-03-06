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
using System.Windows.Shapes;
using Wsfly.ERP.Std.AppCode.Enums;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// AlertWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AlertWindow : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AlertWindow()
        {
            InitializeComponent();

            InitUI();
        }
        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitUI()
        {
            this.btnOk.Click += new RoutedEventHandler(btnOk_Click);
            this.btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            this.pClose.MouseLeftButtonDown += new MouseButtonEventHandler(pClose_MouseLeftButtonDown);
            this.KeyDown += AlertWindow_KeyDown;
        }

        /// <summary>
        /// 热键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlertWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
                this.Close();
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="showClose"></param>
        /// <param name="btns"></param>
        public bool? Alert(string message, string title = "温馨提示", AlertWindowButton btns = AlertWindowButton.Ok, bool showClose = true)
        {
            this.txtTitle.Text = title;
            this.txtContent.Text = message;
            this.txtContent.FontSize = 14;
            this.txtContent.FontFamily = new FontFamily("微软雅黑");

            //得到文字大小
            //System.Drawing.Size size = System.Windows.Forms.TextRenderer.MeasureText(message, new System.Drawing.Font(this.txtContent.FontFamily.ToString(), (float)16));
            //if (size.Width / 2 <= this.Width)
            //{
            //    this.txtContent.FontSize = 16;
            //}

            try
            {
                FontFamily family = new FontFamily("微软雅黑");
                FontStyle style = new FontStyle();
                FontWeight weight = new FontWeight();
                FontStretch stretch = FontStretches.Normal;
                double fontSize = 16;
                //计算显示大小
                AppGlobal.MeasureSize size = AppGlobal.MeasureText(message, family, style, weight, stretch, fontSize);
                if (size.Width <= (this.Width - 30))
                {
                    this.txtContent.FontSize = 16;
                }
            }
            catch { }

            //不显示关闭按钮
            if (!showClose) this.pClose.Visibility = System.Windows.Visibility.Collapsed;

            if (btns == AlertWindowButton.None)
            {
                //不显示按钮
                this.pBtns.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (btns == AlertWindowButton.OkCancel)
            {
                //显示确定取消
                this.btnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                //移除Cancel按钮
                this.pBtns.Children.Remove(this.btnCancel);
            }

            //初始窗体大小
            InitWindowSize();

            //返回结果
            return this.ShowDialog();
        }
        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="element"></param>
        /// <param name="title"></param>
        /// <param name="showClose"></param>
        /// <param name="btns"></param>
        public bool? Alert(System.Windows.UIElement element, string title = "温馨提示", AlertWindowButton btns = AlertWindowButton.Ok, bool showClose = true)
        {
            if (element == null) return false;

            this.txtTitle.Text = title;
            this.pMain.Children.Clear();
            this.pMain.Children.Add(element);

            //不显示关闭按钮
            if (!showClose) this.pClose.Visibility = System.Windows.Visibility.Hidden;

            if (btns == AlertWindowButton.None)
            {
                //不显示按钮
                this.pBtns.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (btns == AlertWindowButton.OkCancel)
            {
                //显示确定取消
                this.btnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                //移除Cancel按钮
                this.pBtns.Children.Remove(this.btnCancel);
            }

            //初始窗体大小
            InitWindowSize();

            //返回结果
            return this.ShowDialog();
        }

        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitWindowSize()
        {
            double height = this.ActualHeight;
            if (double.IsNaN(height) || height <= 0) height = this.Height;
            if (double.IsNaN(height) || height <= 0) height = 200;

            this.scrollMainFrame.Height = height - 85;
        }

        /// <summary>
        /// 水平对齐
        /// </summary>
        /// <param name="align"></param>
        public void TextHorizontalAlignment(HorizontalAlignment align)
        {
            this.pMain.HorizontalAlignment = align;
        }
        /// <summary>
        /// 垂直对齐
        /// </summary>
        /// <param name="align"></param>
        public void TextVerticalAlignment(VerticalAlignment align)
        {
            this.pMain.VerticalAlignment = align;
        }
    }
}
