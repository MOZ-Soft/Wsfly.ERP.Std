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
using Wsfly.ERP.Std.AppCode.Base;
using System.Windows.Navigation;
using Wsfly.ERP.Std.Core.Base;
using Wsfly.ERP.Std.Core;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// 关闭时委托
    /// </summary>
    public delegate void PageWindowCloseDelegate();
    /// <summary>
    /// 回调委托
    /// </summary>
    /// <param name="param"></param>
    public delegate void PageWindowCallBackDelegate(PageWindow win, object param);

    /// <summary>
    /// PageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PageWindow : AppCode.Base.BaseWindow
    {
        /// <summary>
        /// 关闭 执行事件
        /// </summary>
        public event PageWindowCloseDelegate CloseWindow_Event;
        /// <summary>
        /// 选择 回调事件
        /// </summary>
        public event PageWindowCallBackDelegate CallBack_Event;

        /// <summary>
        /// 页面地址
        /// </summary>
        public string _Url { get; set; }
        /// <summary>
        /// 显示导航UI
        /// </summary>
        public bool _ShowNavUI { get; set; }
        /// <summary>
        /// 要显示的对象
        /// </summary>
        public UIElement _UIElement { get; set; }


        /// <summary>
        /// 是否显示最大化按钮
        /// </summary>
        public bool _ShowMaxBtn { get; set; }
        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public bool _ShowCloseBtn { get; set; }
        /// <summary>
        /// 是否显示最小化按钮
        /// </summary>
        public bool _ShowMinBtn { get; set; }

        /// <summary>
        /// 触发对象
        /// </summary>
        public object _TargetObj { get; set; }
        /// <summary>
        /// 返回行数
        /// </summary>
        public int CallBackRowCount { get; set; }

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WinWidth
        {
            get
            {
                double width = this.ActualWidth;
                if (double.IsNaN(width)) width = this.Width;
                return width;
            }
        }
        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WinHeight
        {
            get
            {
                double height = this.ActualHeight;
                if (double.IsNaN(height)) height = this.Height;
                return height;
            }
        }

        /// <summary>
        /// 得到滚动区域高度
        /// </summary>
        /// <returns></returns>
        public double GetScrollHeight()
        {
            return WinHeight - 50;
        }

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public PageWindow()
        {
            InitUI();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public PageWindow(UIElement element, string title = null)
        {
            _UIElement = element;

            if (!string.IsNullOrWhiteSpace(title))
            {
                this.Title = title;
            }

            InitUI();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public PageWindow(string url, string title = null, bool showsNavigationUI = false)
        {
            _Url = url;
            _ShowNavUI = showsNavigationUI;

            if (!string.IsNullOrWhiteSpace(title))
            {
                this.Title = title;
            }
            else
            {
                this.Title = AppGlobal.GetSysConfigReturnString("System_AppName");
            }

            InitUI();
            InitSize();
        }
        /// <summary>
        /// 初始界面UI
        /// </summary>
        private void InitUI()
        {
            //初始值
            _ShowMaxBtn = true;
            _ShowMinBtn = true;
            _ShowCloseBtn = true;

            //初始界面
            InitializeComponent();

            //拥有窗口对象
            this.Owner = AppData.MainWindow;

            //避免覆盖任务栏
            Core.Handler.WindowHandler.RepairWindowBehavior(this);

            //加载成功
            this.Loaded += new RoutedEventHandler(PageWindow_Loaded);
			this.Closed += new EventHandler(PageWindow_Closed);
            this.SizeChanged += PageWindow_SizeChanged;
            this.KeyUp += PageWindow_KeyUp;
            //移动窗体
            this.pTop.MouseLeftButtonDown += new MouseButtonEventHandler(pTop_MouseLeftButtonDown);
            this.pTop.MouseDown += new MouseButtonEventHandler(pTop_MouseDown);
            //窗体操作
            this.pMin.MouseLeftButtonDown += new MouseButtonEventHandler(pMin_MouseLeftButtonDown);
            this.pMax.MouseLeftButtonDown += new MouseButtonEventHandler(pMax_MouseLeftButtonDown);            
            this.pClose.MouseLeftButtonDown += new MouseButtonEventHandler(pClose_MouseLeftButtonDown);
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_ShowMaxBtn)
            {
                this.pMax.Visibility = System.Windows.Visibility.Collapsed;
                this.pTop.MouseDown -= new MouseButtonEventHandler(pTop_MouseDown);
            }

            if (!_ShowMinBtn)
            {
                this.pMin.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!_ShowCloseBtn)
            {
                this.pClose.Visibility = System.Windows.Visibility.Collapsed;
            }

            //加载数据
            LoadData();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            ///未设置标题
            if (string.IsNullOrWhiteSpace(this.Title))
            {
                this.Title = AppGlobal.GetSysConfigReturnString("System_AppName");
            }

            if (!string.IsNullOrWhiteSpace(_Url))
            {
                //设置是否显示导航工具栏
                this.frameMain.NavigationUIVisibility = _ShowNavUI ? NavigationUIVisibility.Visible : NavigationUIVisibility.Hidden;
                
                //生成Uri
                Uri uri = AppHandler.BuildUri(_Url);

                //导航到地址
                this.frameMain.Navigate(uri);
            }
            else
            {
                //显示内容
                this.frameMain.Content = _UIElement;
            }

            this.frameMain.Navigated -= new NavigatedEventHandler(frameMain_Navigated);
            this.frameMain.Navigated += new NavigatedEventHandler(frameMain_Navigated);
        }
        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frameMain_Navigated(object sender, NavigationEventArgs e)
        {
            if (this.frameMain.Content is BasePage)
            {
                (this.frameMain.Content as BasePage)._ParentWindow = this;
				(this.frameMain.Content as BasePage).SetSubPageParent();
            }
        }
        #endregion

        #region 窗体操作
        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }
        /// <summary>
        /// 初始大小
        /// </summary>
        private void InitSize()
        {
            this.scrollMainFrame.Width = this.Width - 10;
            this.scrollMainFrame.Height = this.Height - 50;
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PageWindow_Closed(object sender, EventArgs e)
        {
            //关闭浏览器 调用事件
            if (CloseWindow_Event != null)
            {
                CloseWindow_Event();
            }
        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 最大、常规窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pMax_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pMin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
        /// <summary>
        /// 鼠标移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pTop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }

        /// <summary>
        /// 双击标题栏事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pTop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //双击时执行
                if (this.WindowState != System.Windows.WindowState.Maximized)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }
        }
        /// <summary>
        /// 触发回调事件
        /// </summary>
        /// <param name="param"></param>
        public void CallBack(object param)
        {
            //返回数量加1
            CallBackRowCount++;

            //是否有绑定回调事件
            if (CallBack_Event != null)
            {
                //调用回调事件
                CallBack_Event(this, param);
            }
        }
        /// <summary>
        /// 按下键盘按键后弹起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
        #endregion
    }
}
