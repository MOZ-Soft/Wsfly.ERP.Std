using Wsfly.ERP.Std.Core.Base;
using Wsfly.ERP.Std.Core.Handler;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wsfly.ERP.Std
{
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : AppCode.Base.BaseWindow
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public LoadingWindow()
        {
            InitializeComponent();
            
            this.ShowInTaskbar = false;

            this.Loaded += LoadingWindow_Loaded;
            this.MouseDown += LoadingWindow_MouseDown;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //AppTimer.SecondsRun_Event += Loading_SecondsRun_Event;
        }
        /// <summary>
        /// 呈现结束
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            //透明度动画
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.3, 1, new Duration(TimeSpan.FromSeconds(1)));
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            opacityAnimation.AutoReverse = true;
            this.Canvas.BeginAnimation(Canvas.OpacityProperty, opacityAnimation, HandoffBehavior.Compose);

            //投影动画
            DoubleAnimation effectAnimation = new DoubleAnimation(0, 15, new Duration(TimeSpan.FromSeconds(2)));
            effectAnimation.RepeatBehavior = RepeatBehavior.Forever;
            effectAnimation.AutoReverse = true;
            this.Canvas.Effect = new System.Windows.Media.Effects.DropShadowEffect()
            {
                Color = Colors.Cyan,
                BlurRadius = 0,
                ShadowDepth = 0
            };
            //this.Canvas.Effect.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.BlurRadiusProperty, effectAnimation, HandoffBehavior.Compose); 

            //变大动画
            DoubleAnimation widthAnimation = new DoubleAnimation(0, 400, new Duration(TimeSpan.FromSeconds(0.5)));
            this.borderMain.BeginAnimation(Border.WidthProperty, widthAnimation, HandoffBehavior.Compose);
            DoubleAnimation heightAnimation = new DoubleAnimation(0, 300, new Duration(TimeSpan.FromSeconds(0.5)));
            this.borderMain.BeginAnimation(Border.HeightProperty, heightAnimation, HandoffBehavior.Compose);
        }
        /// <summary>
        /// 移动窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadingWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //移动窗体
                this.DragMove();
            }
            catch { }
        }

        /// <summary>
        /// 每秒执行一次
        /// </summary>
        /// <param name="runIndex"></param>
        public void Loading_SecondsRun_Event(long runIndex)
        {
            this.Dispatcher.Invoke(new BasePage.FlushClientBaseDelegate(delegate ()
            {
                if (runIndex % 6 == 0)
                {
                    this.lblLoadingWaiting.Text = ".";
                }
                else
                {
                    this.lblLoadingWaiting.Text += ".";
                }
            }));
        }
        /// <summary>
        /// 显示版本号
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void ShowVersion(string version)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblAppVersion.Text = "v" + version;
                return;
            }));
        }
        /// <summary>
        /// 显示程序名称
        /// </summary>
        /// <param name="appName"></param>
        public void ShowAppName(string appName)
        {
            
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void ShowMessage(string msg)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblLoadingTxt.Text = msg;
                return;
            }));
        }
        /// <summary>
        /// 透明度动画 
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="to"></param>
        public static void FloatElement(UIElement elem, double to)
        {
            lock (elem)
            {
                if (to == 1)
                {
                    elem.Visibility = Visibility.Visible;
                }
                DoubleAnimation opacity = new DoubleAnimation()
                {
                    To = to,
                    Duration = new TimeSpan(0, 0, 0, 1, 0),
                    AutoReverse = true,
                };
                EventHandler handler = null;
                opacity.Completed += handler = (s, e) =>
                {
                    opacity.Completed -= handler;
                    if (to == 0)
                    {
                        elem.Visibility = Visibility.Collapsed;
                    }
                    opacity = null;
                };
                elem.BeginAnimation(UIElement.OpacityProperty, opacity);
            }
        }
        /// <summary>
        /// 缩放动画
        /// </summary>
        /// <param name="element">控件名</param>
        /// <param name="point">元素开始动画的位置</param>
        /// <param name="from">元素开始的大小</param>
        /// <param name="to">元素到达的大小</param>
        public static void ScaleEasingAnimationShow(FrameworkElement element, Point point, double from, double to)
        {
            lock (element)
            {
                ScaleTransform scale = new ScaleTransform();
                element.RenderTransform = scale;
                element.RenderTransformOrigin = point;//定义圆心位置        
                EasingFunctionBase easeFunction = new PowerEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Power = 5
                };
                DoubleAnimation scaleAnimation = new DoubleAnimation()
                {
                    From = from,                                      //起始值
                    To = to,                                          //结束值
                    EasingFunction = easeFunction,                    //缓动函数
                    Duration = new TimeSpan(0, 0, 0, 5, 0)            //动画播放时间
                };
                AnimationClock clock = scaleAnimation.CreateClock();
                scale.ApplyAnimationClock(ScaleTransform.ScaleXProperty, clock);
                scale.ApplyAnimationClock(ScaleTransform.ScaleYProperty, clock);
            }
        }
        /// <summary>
        /// 移动动画 
        /// </summary>
        /// <param name="top">目标点相对于上端的位置</param>
        /// <param name="left">目标点相对于左端的位置</param>
        /// <param name="elem">移动元素</param>
        public static void FloatInElement(double top, double left, UIElement elem)
        {
            try
            {
                DoubleAnimation floatY = new DoubleAnimation()
                {
                    To = top,
                    Duration = new TimeSpan(0, 0, 0, 1, 0),
                };
                DoubleAnimation floatX = new DoubleAnimation()
                {
                    To = left,
                    Duration = new TimeSpan(0, 0, 0, 1, 0),
                };

                elem.BeginAnimation(Canvas.TopProperty, floatY);
                elem.BeginAnimation(Canvas.LeftProperty, floatX);
            }
            catch (Exception)
            {
            }
        }
    }
}
