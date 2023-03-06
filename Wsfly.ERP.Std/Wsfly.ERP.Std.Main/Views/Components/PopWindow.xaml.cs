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
using System.Windows.Shapes;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// 关闭委托
    /// </summary>
    public delegate void ClosePopWindowDelegate(PopWindow win);
    /// <summary>
    /// 回调委托
    /// </summary>
    /// <param name="param"></param>
    public delegate void CallBackDelegate(PopWindow win, object param);

    /// <summary>
    /// PopWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopWindow : BaseWindow
    {
        /// <summary>
        /// 关闭 回调事件
        /// </summary>
        public event ClosePopWindowDelegate CloseWindow_Event;
        /// <summary>
        /// 选择 回调事件
        /// </summary>
        public event CallBackDelegate CallBack_Event;

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
        /// 要显示的对象
        /// </summary>
        public UIElement _UIElement { get; set; }
        /// <summary>
        /// 列信息
        /// </summary>
        public CellInfo _CellInfo { get; set; }
        /// <summary>
        /// 引用后关闭
        /// </summary>
        public bool _YYHGB { get; set; }
        /// <summary>
        /// 返回行数
        /// </summary>
        public int CallBackRowCount { get; set; }

        /// <summary>
        /// 默认规格大小
        /// </summary>
        private bool DefaultSize = true;

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
        public PopWindow()
        {
            InitUI();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public PopWindow(UIElement element, string title = null)
        {
            _UIElement = element;

            //上级对象
            if(_UIElement is BaseUserControl)
            {
                (_UIElement as BaseUserControl)._ParentWindow = this;
            }

            InitUI();
            InitSize();

            //标题
            if (!string.IsNullOrWhiteSpace(title))
            {
                this.Title = title;
            }
            else
            {
                this.Title = "MOZ-ERP";
            }

            this.lblTitle.Text = this.Title;
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

            //背景
            //this.borderMain.Background = AppGlobal._AppBackground;

            //拥有弹出窗口对象
            this.Owner = AppData.MainWindow;

            //避免覆盖任务栏
            Core.Handler.WindowHandler.RepairWindowBehavior(this);

            //加载成功
            this.Loaded += PopWindow_Loaded;
            this.Closed += PopWindow_Closed;
            this.SizeChanged += PopWindow_SizeChanged;
            this.KeyUp += PopWindow_KeyUp;

            //移动窗体
            this.pTop.MouseLeftButtonDown += new MouseButtonEventHandler(pTop_MouseLeftButtonDown);
            this.pTop.MouseDown += new MouseButtonEventHandler(pTop_MouseDown);

            //窗体操作
            this.pMin.MouseLeftButtonDown += new MouseButtonEventHandler(pMin_MouseLeftButtonDown);
            this.pMax.MouseLeftButtonDown += new MouseButtonEventHandler(pMax_MouseLeftButtonDown);
            this.pClose.MouseLeftButtonDown += new MouseButtonEventHandler(pClose_MouseLeftButtonDown);
        }

        /// <summary>
        /// 键盘按键按下弹起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DefaultSize = false;

            InitSize();
        }

        /// <summary>
        /// 初始大小
        /// </summary>
        private void InitSize()
        {
            if (DefaultSize)
            {
                if (AppData.MainWindow.WinWidth > 0)
                {
                    this.Width = AppData.MainWindow.WinWidth * 0.7;
                    this.Height = AppData.MainWindow.WinHeight * 0.7;

                    if (this.Width < 1000) this.Width = 1000;
                    if (this.Height < 600) this.Height = 600;
                }
            }

            this.scrollMainFrame.Width = this.Width - 10;
            this.scrollMainFrame.Height = this.Height - 50;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PopWindow_Loaded(object sender, RoutedEventArgs e)
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

            LoadData();
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            //没有内容
            if (_UIElement == null) return;
            //加载内容
            this.panelMain.Children.Add(_UIElement);
        }
        #endregion

        #region 窗体操作
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PopWindow_Closed(object sender, EventArgs e)
        {
            //关闭 调用事件
            if (CloseWindow_Event != null)
            {
                CloseWindow_Event(this);
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
        #endregion

        #region 拖动窗体大小
        private const int WM_NCHITTEST = 0x0084;//
        private readonly int agWidth = 12;      //拐角宽度
        private readonly int bThickness = 5;    // 边框宽度
        private Point mousePoint = new Point(); //鼠标坐标
        private const int WM_GETMINMAXINFO = 0x0024;//大小变化

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            System.Windows.Interop.HwndSource hwndSource = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }
        }
        public enum HitTest : int
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
        }
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    {
                        this.mousePoint.X = (lParam.ToInt32() & 0xFFFF);
                        this.mousePoint.Y = (lParam.ToInt32() >> 16);

                        //标记消息已经处理
                        //handled = true;

                        if (AppData.MainWindow._SystemDPI == 96)
                        {
                            #region 测试鼠标位置
                            // 窗口右下角
                            if (this.ActualWidth + this.Left - this.mousePoint.X <= this.agWidth && this.ActualHeight + this.Top - this.mousePoint.Y <= this.agWidth)
                            {
                                handled = true;
                                return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                            }
                            // 窗口右侧
                            else if (this.ActualWidth + this.Left - this.mousePoint.X <= this.bThickness)
                            {
                                handled = true;
                                return new IntPtr((int)HitTest.HTRIGHT);
                            }
                            // 窗口下方
                            else if (this.ActualHeight + this.Top - this.mousePoint.Y <= this.bThickness)
                            {
                                handled = true;
                                return new IntPtr((int)HitTest.HTBOTTOM);
                            }
                            else
                            {
                                handled = false;
                                break;
                            }
                            #endregion
                        }
                        else
                        {
                            handled = false;
                            break;
                        }
                    }

            }

            return IntPtr.Zero;
        }
        #endregion

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
        /// 设置标题
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblTitle.Text = title;
                return;
            }));
        }
    }
}
