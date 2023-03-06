using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wsfly.ERP.Std.Core.Base
{
    public class WsflyWindow : BaseWindow
    {
        /// <summary>
        /// 是否支持最大化
        /// </summary>
        public bool ShowMaxButton { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WsflyWindow()
        {
            //默认显示最大化
            ShowMaxButton = true;

            //无边框
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            //修复全屏覆盖任务栏
            Handler.WindowHandler.RepairWindowBehavior(this);

            this.Loaded += new System.Windows.RoutedEventHandler(WsflyWindow_Loaded);
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WsflyWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ///生成界面
            BuildWsflyUI();
        }
        /// <summary>
        /// 生成界面
        /// </summary>
        private void BuildWsflyUI()
        {
            //主笔刷
            Brush mainBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00aeff"));

            //边框
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00aeff"));
            border.BorderThickness = new System.Windows.Thickness(5);

            int widthActions = 70;

            if (!ShowMaxButton) widthActions -= 20;

            ///动态创建网格
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(widthActions) });

            ///标题栏
            StackPanel pTop = new StackPanel();
            pTop.Height = 40;
            pTop.Background = mainBrush;
            pTop.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(pTop_MouseLeftButtonDown);

            grid.Children.Add(pTop);
            Grid.SetRow(pTop, 0);
            Grid.SetColumn(pTop, 0);
            Grid.SetColumnSpan(pTop, 2);


            //标题
            TextBlock lblTitle = new TextBlock();
            lblTitle.Text = this.Title;
            lblTitle.FontSize = 14;
            lblTitle.Margin = new System.Windows.Thickness(10);
            lblTitle.Foreground = Brushes.White;
            lblTitle.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            grid.Children.Add(lblTitle);
            Grid.SetRow(lblTitle, 0);
            Grid.SetColumn(lblTitle, 0);

            //操作栏
            WrapPanel pActions = new WrapPanel();
            pActions.Height = 40;
            pActions.Margin = new System.Windows.Thickness(5);
            
            //最小化
            StackPanel pMin = new StackPanel();
            pMin.Width = 20;
            pMin.Height = 20;
            pMin.Background = mainBrush;
            pMin.Cursor = System.Windows.Input.Cursors.Hand;
            pMin.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(pMin_MouseLeftButtonDown);

            Line line3 = new Line();
            line3.Stroke = Brushes.White;
            line3.StrokeThickness = 2;
            line3.X1 = 4;
            line3.Y1 = 14;
            line3.X2 = 16;
            line3.Y2 = 14;

            pMin.Children.Add(line3);
            pActions.Children.Add(pMin);

            if (ShowMaxButton)
            {
                //最大化
                StackPanel pMax = new StackPanel();
                pMax.Width = 20;
                pMax.Height = 20;
                pMax.Background = mainBrush;
                pMax.Cursor = System.Windows.Input.Cursors.Hand;
                pMax.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(pMax_MouseLeftButtonDown);

                Rectangle rectangle = new Rectangle();
                rectangle.Height = 12;
                rectangle.Width = 12;
                rectangle.Stroke = Brushes.White;
                rectangle.StrokeThickness = 2;
                rectangle.Margin = new System.Windows.Thickness(4);

                pMax.Children.Add(rectangle);
                pActions.Children.Add(pMax);
            }

            //关闭窗口
            StackPanel pClose = new StackPanel();
            pClose.Width = 20;
            pClose.Height = 20;
            pClose.Background = mainBrush;
            pClose.Cursor = System.Windows.Input.Cursors.Hand;
            pClose.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(pClose_MouseLeftButtonDown);

            Canvas cClose = new Canvas();
            Line line1 = new Line();
            line1.Stroke = Brushes.White;
            line1.StrokeThickness = 2;
            line1.X1 = 5;
            line1.Y1 = 5;
            line1.X2 = 15;
            line1.Y2 = 15;

            Line line2 = new Line();
            line2.Stroke = Brushes.White;
            line2.StrokeThickness = 2;
            line2.X1 = 15;
            line2.Y1 = 5;
            line2.X2 = 5;
            line2.Y2 = 15;

            cClose.Children.Add(line1);
            cClose.Children.Add(line2);
            pClose.Children.Add(cClose);
            pActions.Children.Add(pClose);


            //添加操作栏
            grid.Children.Add(pActions);
            Grid.SetRow(pActions, 0);
            Grid.SetColumn(pActions, 1);

            //内容栏
            StackPanel pContent = new StackPanel();
            System.Windows.UIElement uiContent = this.Content as System.Windows.UIElement;
            this.Content = null;
            pContent.Children.Add(uiContent);

            //滚动条
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			scrollViewer.ClipToBounds = true;
            scrollViewer.Content = pContent;
            //scrollViewer.Template = App.Current.Resources["ScrollViewer"] as ControlTemplate;

            grid.Children.Add(scrollViewer);
            Grid.SetRow(scrollViewer, 1);
            Grid.SetColumn(scrollViewer, 0);
            Grid.SetColumnSpan(scrollViewer, 2);

            border.Child = grid;

            this.Content = border;
        }
        
        #region 窗口操作
        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pMin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
        /// <summary>
        /// 最大化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pMax_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pClose_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 鼠标点击标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pTop_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && ShowMaxButton)
            {
                if (this.WindowState != System.Windows.WindowState.Maximized)
                {
                    //双击最大化窗口
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    //正常窗口大小
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }
            else
            {
                //移动窗体
                this.DragMove();
            }
        }
        #endregion
    }
}
