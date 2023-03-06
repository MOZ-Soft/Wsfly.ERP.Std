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
using System.Windows.Media.Effects;

using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// PrintWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PrintWindow : AppCode.Base.BaseWindow
    {
        /// <summary>
        /// 显示文件头
        /// </summary>
        bool ShowHeader = true;
        /// <summary>
        /// 打印对象
        /// </summary>
        UIElement _printElement;
        /// <summary>
        /// 打印区域
        /// </summary>
        StackPanel printArea;

        /// <summary>
        /// 打印窗口
        /// </summary>
        public PrintWindow(UIElement element,bool ShowHeader =true)
        {
            _printElement = element;
            ShowHeader = true;

            InitializeComponent();

            InitUI();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void InitUI()
        {
            //动态创建打印区域
            printArea = new StackPanel();

            //打印象区域属性
            printArea.Background = Brushes.White;
            //printArea.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            printArea.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            printArea.MinWidth = 800;
            printArea.MinHeight = 1132;
            printArea.Margin = new Thickness(0,0,0,0);

            //添加阴影效果
            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = Colors.Black;
            effect.Direction = 5;
            effect.ShadowDepth = 5;
            effect.Opacity = 1;

            printArea.Effect = effect;


            //页头重叠效果
            Grid grid = new Grid();
            grid.Children.Add(_printElement);

            if (ShowHeader)
            {
                //页头
                Image img = new Image();
                img.Width = 150;
                img.Height = 50;
                img.Margin = new Thickness(10);
                img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                //img.Source = Wsfly.Resources.ResourceHandler.GetImageSource("Images.PrintHeader.png");

                grid.Children.Add(img);
            }

            //添加到打印工作区
            printArea.Children.Add(grid);


            //添加到打印工作区
            this.panelPrinter.Children.Clear();
            this.panelPrinter.Children.Add(printArea);

            //事件
            this.btnPrint.Click += new RoutedEventHandler(btnPrint_Click);
            this.btnClose.Click += new RoutedEventHandler(btnClose_Click);
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //printArea.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            {
                dialog.PrintVisual(printArea, "Wsfly Printer");
                //printArea.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            }
        }
        /// <summary>
        /// 打印关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
