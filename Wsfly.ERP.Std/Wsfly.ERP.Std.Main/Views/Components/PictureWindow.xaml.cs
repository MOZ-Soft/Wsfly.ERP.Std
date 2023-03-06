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
using Wsfly.ERP.Std.Core.Handler;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// PictureWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PictureWindow : AppCode.Base.BaseWindow
    {
        #region 属性

        int index = 0;
        List<Image> _pictures = new List<Image>();

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<Image> Pictures
        {
            get { return _pictures; }
            set { _pictures = value; }
        }
        /// <summary>
        /// 显示图片
        /// </summary>
        public Image Picture
        {
            set { AddPicture(value); }
        }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string PictureUrl
        {
            set
            {
                ImageSource imgSource = ImageBrushHandler.GetLocalImageSource(value);
                if (imgSource == null) return;

                Image img = new Image();
                img.Source = imgSource;

                AddPicture(img);
            }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public PictureWindow()
        {
            InitUI();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="img"></param>
        public PictureWindow(Image img)
        {
            AddPicture(img);

            InitUI();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url"></param>
        public PictureWindow(string url)
        {
            PictureUrl = url;

            InitUI();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="imgs"></param>
        public PictureWindow(List<Image> imgs)
        {
            Pictures = imgs;

            InitUI();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitUI()
        {
            InitializeComponent();

            this.Topmost = true;

            this.Loaded += new RoutedEventHandler(PictureWindow_Loaded);
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PictureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Pictures == null || Pictures.Count <= 0)
            {
                TextBlock txtEmpty = new TextBlock();
                txtEmpty.Text = "未找到要显示的图片，请重试！";
                txtEmpty.Foreground = Brushes.Black;
                txtEmpty.FontSize = 14;

                this.pContent.Children.Add(txtEmpty);
                return;
            }

            ShowPicture();
        }
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="img"></param>
        void AddPicture(Image img)
        {
            img.Width = img.Source.Width;
            img.Height = img.Source.Height;

            Pictures.Add(img);
        }
        /// <summary>
        /// 显示图片
        /// </summary>
        void ShowPicture()
        {
            Image img = Pictures[index];
            img.RenderTransform = this.gridMain.FindResource("ImageView") as TransformGroup;
            img.MouseWheel += new MouseWheelEventHandler(img_MouseWheel);
            img.MouseLeftButtonDown+=new MouseButtonEventHandler(img_MouseLeftButtonDown);
            img.MouseMove += new MouseEventHandler(img_MouseMove);
            img.MouseUp += new MouseButtonEventHandler(img_MouseUp);

            this.pContent.Children.Add(img);
        }        

        #region 鼠标控制图片
        bool mouseDown = false;
        Point mouseXY;
        /// <summary>
        /// 鼠标点下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void img_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            if (mouseDown)
            {
                if (e.LeftButton != MouseButtonState.Pressed) return;

                var group = this.gridMain.FindResource("ImageView") as TransformGroup;
                var transform = group.Children[1] as TranslateTransform;
                var position = e.GetPosition(img);
                transform.X -= mouseXY.X - position.X;
                transform.Y -= mouseXY.Y - position.Y;
                mouseXY = position;
            }
        }
        /// <summary>
        /// 鼠标放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            img.ReleaseMouseCapture();
            mouseDown = false;
        }
        /// <summary>
        /// 鼠标滑轮放大、缩小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;

            TransformGroup group = this.gridMain.FindResource("ImageView") as TransformGroup;
            var point = e.GetPosition(img);
            var delta = e.Delta * 0.001;

            var pointToContent = group.Inverse.Transform(point);
            var transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + delta < 0.1) return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            var transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }
        #endregion
        
    }
}
