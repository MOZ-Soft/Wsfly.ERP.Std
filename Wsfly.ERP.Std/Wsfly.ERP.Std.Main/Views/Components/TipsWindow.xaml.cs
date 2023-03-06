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
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Forms;
using Wsfly.ERP.Std.Core.Base;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// TipsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TipsWindow : AppCode.Base.BaseWindow
    {
        private bool _autoClose = false;//是否自动关闭
        private bool _showClose = true;//显示关闭按钮

        private double _screenWidth;
        private DispatcherTimer _timer;
        private DispatcherTimer _timerClose;
        private Storyboard _storyboardShow;
        private Storyboard _storyboardHide;

        public delegate void ContentClick_Delegate(TipsWindow win);
        public event ContentClick_Delegate ContentClick_Event;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TipsWindow()
        {
            InitializeComponent();

            InitUI();
        }
        /// <summary>
        /// 初始界面 
        /// </summary>
        private void InitUI()
        {
            //显示
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += timer_Tick;
            _timer.Start();

            //关闭
            _timerClose = new DispatcherTimer();
            _timerClose.Interval = TimeSpan.FromSeconds(5);
            _timerClose.Tick += timerClose_Tick;

            this.Loaded += new RoutedEventHandler(TipsWindow_Loaded);

            this.MouseEnter += new System.Windows.Input.MouseEventHandler(TipsWindow_MouseEnter);
            this.MouseLeave += new System.Windows.Input.MouseEventHandler(TipsWindow_MouseLeave);
            this.txtContent.MouseLeftButtonDown += TxtContent_MouseLeftButtonDown;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TipsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        void ShowWindow()
        {
            _screenWidth = Screen.PrimaryScreen.WorkingArea.Right;
            this.Left = _screenWidth;
            this.Top = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;

            //显示动画
            _storyboardShow = new Storyboard();
            DoubleAnimationUsingKeyFrames doubleAnimation = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame easingDoubleKeyFrame = new EasingDoubleKeyFrame(_screenWidth - this.Width - 5, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2)));

            doubleAnimation.KeyFrames.Add(easingDoubleKeyFrame);
            _storyboardShow.Children.Add(doubleAnimation);
            Storyboard.SetTarget(doubleAnimation, this);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));

            //关闭动画
            _storyboardHide = new Storyboard();
            doubleAnimation = new DoubleAnimationUsingKeyFrames();
            easingDoubleKeyFrame = new EasingDoubleKeyFrame(_screenWidth, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2)));

            doubleAnimation.KeyFrames.Add(easingDoubleKeyFrame);
            _storyboardHide.Children.Add(doubleAnimation);
            _storyboardHide.Completed += new EventHandler(_storyboardHide_Completed);
            Storyboard.SetTarget(doubleAnimation, this);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
        }
        /// <summary>
        /// 鼠标离开窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TipsWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_autoClose) return;
            _timerClose.Start();
        }
        /// <summary>
        /// 鼠标进入窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TipsWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_autoClose) return;
            _timerClose.Stop();
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="title"></param>
        /// <param name="autoClose"></param>
        public void ShowMessage(string message, string title = "温馨提示", bool autoClose = false, bool showClose = true)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            this.txtTitle.Text = title;
            this.txtContent.Text = message;
            this.txtContent.FontSize = 14;
            this.txtContent.FontFamily = new FontFamily("微软雅黑");

            //得到文字大小
            System.Drawing.Size size = System.Windows.Forms.TextRenderer.MeasureText(message, new System.Drawing.Font(this.txtContent.FontFamily.ToString(), (float)16));
            if (size.Width / 2 <= this.Width)
            {
                this.txtContent.FontSize = 16;
            }

            _autoClose = autoClose;
            _showClose = showClose;

            if (!_showClose)
            {
                this.pClose.Visibility = System.Windows.Visibility.Hidden;
            }

            this.Show();
        }
        /// <summary>
        /// 显示UI对象
        /// </summary>
        /// <param name="element"></param>
        /// <param name="title"></param>
        /// <param name="autoClose"></param>
        /// <param name="showClose"></param>
        public void ShowContent(UIElement element, string title = "温馨提示", bool autoClose = false, bool showClose = true)
        {
            if (element == null) return;

            this.txtTitle.Text = title;

            this.pMain.Children.Clear();
            this.pMain.Children.Add(element);

            if (element is BasePage)
            {
                (element as BasePage)._ParentWindow = this;
            }
            else if (element is BaseUserControl)
            {
                (element as BaseUserControl)._ParentWindow = this;
            }

            _autoClose = autoClose;
            _showClose = showClose;

            if (!_showClose)
            {
                this.pClose.Visibility = System.Windows.Visibility.Hidden;
            }

            this.Show();
        }

        /// <summary>
        /// 动画显示窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (_autoClose) _timerClose.Start();
            _storyboardShow.Begin();
        }
        /// <summary>
        /// 动画关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerClose_Tick(object sender, EventArgs e)
        {
            _timerClose.Stop();
            _storyboardHide.Begin();
        }
        /// <summary>
        /// 动画完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _storyboardHide_Completed(object sender, EventArgs e)
        {
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
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 点击内容回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContentClick_Event != null)
            {
                ContentClick_Event(this);
            }
        }
    }
}
