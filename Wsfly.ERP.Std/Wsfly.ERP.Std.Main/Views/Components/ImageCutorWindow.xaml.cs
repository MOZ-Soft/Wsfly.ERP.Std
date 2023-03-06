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
using System.Diagnostics;
using System.IO;
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// 裁剪完成后
    /// </summary>
    public delegate void CutOverDelegate(System.Drawing.Bitmap bitmap, string path);

    /// <summary>
    /// ImageCutor.xaml 的交互逻辑
    /// </summary>
    public partial class ImageCutorWindow : BaseWindow
    {
        /// <summary>
        /// 裁剪完成 执行事件
        /// </summary>
        public event CutOverDelegate CutOverEvent;

        #region 成员变量

        private Point previousMousePoint;
        private bool isMouseLeftButtonDown;
        private Cursor oldCursor;
        private IMAdorner _IMAdorner;
        private string _FilePath;
        public int _WidthHeight = 200;

        #endregion 成员变量

        #region 初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        public ImageCutorWindow() { InitUI(); }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ImageCutorWindow(string filePath, int widthHeight = 200)
        {
            //初始界面
            InitUI();
            //设置图像
            SetImage(filePath, widthHeight);
        }
        /// <summary>
        /// 初始界面UI
        /// </summary>
        private void InitUI()
        {
            InitializeComponent();
            //事件
            this.MasterImage.Loaded += new RoutedEventHandler(MasterImage_Loaded);

            this.btnOK.Click += BtnOK_Click;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(ImageComparePanel);
            _IMAdorner = new IMAdorner(ImageComparePanel, this);
            layer.Add(_IMAdorner);
        }
        #endregion

        #region 成员方法

        /// <summary>
        /// 设置要裁剪的图片
        /// </summary>
        /// <param name="filePath"></param>
        public void SetImage(string filePath, int widthHeight = 200)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            _FilePath = filePath;
            _WidthHeight = widthHeight;

            ///加载图片
            LoadImage();
        }
        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadImage()
        {
            try
            {
                if (!File.Exists(_FilePath)) return;

                BitmapImage bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
                bitmapSource.UriSource = new Uri(_FilePath);
                bitmapSource.EndInit();
                bitmapSource.Freeze();

                this.MasterImage.Source = bitmapSource;

                if (bitmapSource.Width < this.Width && bitmapSource.Height < this.Height)
                {
                    this.MasterImage.Width = bitmapSource.Width;
                    this.MasterImage.Height = bitmapSource.Height;
                }

                this.MasterImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                this.MasterImage.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            catch { }
        }
        /// <summary>
        /// 图片加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MasterImage_Loaded(object sender, RoutedEventArgs e)
        {
            //图片居中
            ImageAlignCenter();
        }
        /// <summary>
        /// 将图片居中
        /// </summary>
        void ImageAlignCenter()
        {
            //图片显示大小
            double imageWidth = MasterImage.ActualWidth;
            double imageHeight = MasterImage.ActualHeight;

            TransformGroup group = ImageComparePanel.FindResource("ImageCompareResources") as TransformGroup;
            Debug.Assert(group != null, "Can't find transform group from image compare panel resource");
            TranslateTransform transform = group.Children[1] as TranslateTransform;
            transform.X = (this.Width - imageWidth) / 2;
            transform.Y = (this.Height - imageHeight) / 2;
        }
        #endregion 成员方法

        #region 处理方法

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MasterImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ContentControl image = sender as ContentControl;
            if (image == null) return;

            TransformGroup group = ImageComparePanel.FindResource("ImageCompareResources") as TransformGroup;
            Debug.Assert(group != null, "Can't find transform group from image compare panel resource");
            Point point = e.GetPosition(image);
            double scale = e.Delta * 0.001;
            ZoomImage(group, point, scale);
        }
        /// <summary>
        /// 放大缩小
        /// </summary>
        /// <param name="group"></param>
        /// <param name="point"></param>
        /// <param name="scale"></param>
        private static void ZoomImage(TransformGroup group, Point point, double scale)
        {
            Debug.Assert(group != null, "Oops, ImageCompareResources is removed from current control's resouce");

            Point pointToContent = group.Inverse.Transform(point);
            ScaleTransform transform = group.Children[0] as ScaleTransform;

            if (transform.ScaleX + scale < 0.2) return;

            transform.ScaleX += scale;
            transform.ScaleY += scale;
            TranslateTransform transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }
        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MasterImage_MouseMove(object sender, MouseEventArgs e)
        {
            ContentControl image = sender as ContentControl;

            if (image == null) return;

            if (this.isMouseLeftButtonDown && e.LeftButton == MouseButtonState.Pressed)
            {
                this.DoImageMove(image, e.GetPosition(image));
            }
        }
        /// <summary>
        /// 图片移动
        /// </summary>
        /// <param name="image"></param>
        /// <param name="position"></param>
        private void DoImageMove(ContentControl image, Point position)
        {
            TransformGroup group = ImageComparePanel.FindResource("ImageCompareResources") as TransformGroup;

            Debug.Assert(group != null, "Can't find transform group from image compare panel resource");

            TranslateTransform transform = group.Children[1] as TranslateTransform;

            transform.X += position.X - this.previousMousePoint.X;

            transform.Y += position.Y - this.previousMousePoint.Y;

            this.previousMousePoint = position;
        }
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MasterImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null) return;

            this.oldCursor = this.Cursor;
            this.Cursor = Cursors.SizeAll;

            img.CaptureMouse();
            this.isMouseLeftButtonDown = true;
            this.previousMousePoint = e.GetPosition(img);
        }
        /// <summary>
        /// 鼠标放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MasterImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null) return;

            this.Cursor = this.oldCursor;

            img.ReleaseMouseCapture();
            this.isMouseLeftButtonDown = false;
        }
        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;

            if (MasterImage.Source == null)
            {
                #region 打开本地资源

                string selectFileName;//存放所选文件的文件名

                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

                //获取电脑桌面的路径
                String Desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                //初始化打开为电脑桌面路径
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + Desktop + "\\";
                openFileDialog.Filter = "图像文件|*.png;*.jpg;*.bmp;*gif";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    selectFileName = System.IO.Path.GetFileName(openFileDialog.FileName);//获取文件名
                    _FilePath = openFileDialog.FileName;//获得选择的文件路径

                    //加载图片
                    LoadImage();
                }

                #endregion 打开本地资源
            }
            else
            {
                //裁剪图片
                CutImage();
            }
        }
        /// <summary>
        /// 确定裁剪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //裁剪图片
            CutImage();
        }
        /// <summary>
        /// 裁剪图片
        /// </summary>
        private void CutImage()
        {
            #region 裁剪图片

            double imageWidth = MasterImage.Source.Width;
            double imageHeight = MasterImage.Source.Height;

            //目标效果,即界面选取框相对于界面的坐标以及宽高，界面像素600*600
            double x1 = (this.Width - _WidthHeight) / 2;
            double y1 = (this.Height - _WidthHeight) / 2;
            double width1 = _WidthHeight;
            double height1 = _WidthHeight;

            //界面呈现效果
            TransformGroup group = ImageComparePanel.FindResource("ImageCompareResources") as TransformGroup;
            TranslateTransform transform1 = group.Children[1] as TranslateTransform;
            ScaleTransform transform = group.Children[0] as ScaleTransform;
            double x2 = transform1.X;
            double y2 = transform1.Y;
            double scaleX = transform.ScaleX;
            double scaleY = transform.ScaleY;

            //Image控件被设置成Uniform时会自动将原图缩放，此时需要求出原图与控件呈现图的缩放比例offset
            double width3 = MasterImage.ActualWidth;
            double height3 = MasterImage.ActualHeight;
            double offsetX = imageWidth / width3;
            double offsetY = imageHeight / height3;

            //计算原图的裁剪位置以及宽高
            double x = ((x1 - x2) / scaleX) * offsetX / 96;
            double y = ((y1 - y2) / scaleY) * offsetY / 96;
            double width = (width1 / scaleX) * offsetX / 96;
            double height = (height1 / scaleY) * offsetY / 96;

            //剪裁图片
            try
            {
                var ob = System.Drawing.Image.FromFile(_FilePath);
                var bitmap = new System.Drawing.Bitmap((int)width1, (int)height1);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);
                System.Drawing.Rectangle resultRectangle = new System.Drawing.Rectangle(0, 0, (int)width1, (int)height1);
                System.Drawing.Rectangle sourceRectangle = new System.Drawing.Rectangle((int)(x), (int)(y), (int)(width), (int)(height));
                g.DrawImage(ob, resultRectangle, sourceRectangle, System.Drawing.GraphicsUnit.Pixel);

                //完成回调函数
                if (CutOverEvent != null)
                {
                    CutOverEvent(bitmap, _FilePath);
                }

                //关闭
                this.Close();
            }
            catch (Exception ex)
            {
                AppLog.WriteSysLog("异常", "裁剪图片异常！", "异常：" + ex.Message);
            }

            #endregion 裁剪图片
        }
        #endregion 处理方法
    }

    /// <summary>
    /// 蒙版修饰器
    /// </summary>
    class IMAdorner : Adorner
    {
        ImageCutorWindow _parent;
        public double x1;
        public double y1;
        public double width;
        //public double height;

        public IMAdorner(UIElement adorned, ImageCutorWindow parent)
            : base(adorned)
        {
            _parent = parent;
        }

        /// <summary>
        /// 画蒙版
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Grid ic = this.AdornedElement as Grid;
            SolidColorBrush scb = new SolidColorBrush(Color.FromArgb(197, 173, 173, 173));
            //scb = new SolidColorBrush(Colors.Red);

            x1 = ic.Width;
            y1 = ic.Height;
            width = ic.Width - _parent._WidthHeight;
            dc.DrawRectangle(new SolidColorBrush(), new Pen(scb, width), new Rect(0, 0, x1, y1));
        }

        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _parent.MasterImage_MouseLeftButtonDown(_parent.TestContentControl1, e);
        }

        /// <summary>
        /// 鼠标左键放开
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _parent.MasterImage_MouseLeftButtonUp(_parent.TestContentControl1, e);
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _parent.MasterImage_MouseMove(_parent.TestContentControl1, e);
        }

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _parent.MasterImage_MouseWheel(_parent.TestContentControl1, e);
        }
    }
}
