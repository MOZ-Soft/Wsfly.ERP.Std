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

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// WaitingUC.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingUC : UserControl
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(WaitingUC), new PropertyMetadata(Brushes.Black));
        /// <summary>
        /// 颜色
        /// </summary>
        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(WaitingUC), new PropertyMetadata(false));
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty ShowTimerProperty = DependencyProperty.Register("ShowTimer", typeof(bool), typeof(WaitingUC), new PropertyMetadata(true));
        /// <summary>
        /// 是否显示定时器
        /// </summary>
        public bool ShowTimer
        {
            get { return (bool)GetValue(ShowTimerProperty); }
            set { SetValue(ShowTimerProperty, value); }
        }

        /// <summary>
        /// 定时器
        /// </summary>
        private System.Timers.Timer _Timer = null;
        /// <summary>
        /// 秒数
        /// </summary>
        public int Seconds = 0;
        /// <summary>
        /// 刷新委托
        /// </summary>
        public delegate void FlushClientBaseDelegate();

        /// <summary>
        /// 构造
        /// </summary>
        public WaitingUC()
        {
            InitializeComponent();
            this.Loaded += WaitingUC_Loaded;
            this.IsVisibleChanged += WaitingUC_IsVisibleChanged;
        }
        /// <summary>
        /// 显示隐藏变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitingUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Collapsed)
            {
                _Timer.Close();
            }
            else if(this.Visibility == Visibility.Visible)
            {
                this.lblSeconds.Text = "";
            }
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitingUC_Loaded(object sender, RoutedEventArgs e)
        {
            //大小
            double width = this.Width;
            double height = this.Height;

            if (double.IsNaN(width))
            {
                width = this.ActualWidth;
                height = this.ActualHeight;
            }

            double scaleX = width / 120;
            double scaleY = height / 120;

            this.scaleMain.ScaleX = scaleX;
            this.scaleMain.ScaleY = scaleY;

            this.scaleMain.CenterX = -width / 2;
            this.scaleMain.CenterY = -height / 2;

            this.canvasMain.Width = width;
            this.canvasMain.Height = height;

            //颜色
            this.Color = AppGlobal.HtmlColorToBrush("#FF0092FF");
            this.ellipseFirst.Fill = Color;

            this.lblSeconds.Text = "";
            this.lblSeconds.FontSize = (24 * scaleX) < 8 ? 8 : 24 * scaleX;
            this.lblSeconds.Foreground = Color;

            //定时器
            if (ShowTimer)
            {
                Seconds = 0;

                if (_Timer != null)
                {
                    _Timer.Stop();
                    _Timer.Dispose();
                }

                _Timer = new System.Timers.Timer(1000);
                _Timer.Elapsed += Timer_Elapsed;
                _Timer.Start();
            }
        }
        /// <summary>
        /// 定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Seconds++;
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblSeconds.Text = Seconds.ToString();
            }));
        }
        /// <summary>
        /// 重新计时
        /// </summary>
        public void RestartTimer()
        {
            Seconds = 0;
            

            if (ShowTimer)
            {
                if (_Timer != null)
                {
                    _Timer.Stop();
                    _Timer.Dispose();
                }

                _Timer = new System.Timers.Timer(1000);
                _Timer.Elapsed += Timer_Elapsed;
                _Timer.Start();
            }
        }
    }
}
