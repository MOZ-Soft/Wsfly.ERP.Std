using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Wsfly.ERP.Std.Core.Handler;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// WPF图片处理
    /// </summary>
    public class ImageBrushHandler
    {
        #region 图像转画笔
        /// <summary>
        /// 得到图像WPF画笔
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageBrush GetImageBrush(Bitmap bitmap)
        {
            ImageBrush brush = new ImageBrush();

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                brush.ImageSource = result;
            }

            return brush;
        }
        /// <summary>
        /// 得到图像WPF画笔
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ImageBrush GetImageBrush(Stream stream)
        {
            ImageBrush brush = new ImageBrush();

            using (stream)
            {
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                brush.ImageSource = result;
            }

            return brush;
        }
        #endregion

        #region 图片路径转画笔
        /// <summary>
        /// 得到本地图像画笔
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ImageBrush GetLocalImageBrush(string path)
        {
            //不存在文件
            if (!System.IO.File.Exists(path)) return null;

            //转换为图像
            //var image = System.Drawing.Image.FromFile(path);
            //var bitmap = new System.Drawing.Bitmap(image);
            //var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
            //                                                                      IntPtr.Zero,
            //                                                                      System.Windows.Int32Rect.Empty,
            //                                                                      BitmapSizeOptions.FromEmptyOptions()
            //                    );
            //bitmap.Dispose();
            //image.Dispose();

            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.UriSource = new Uri(path);
            bitmapSource.EndInit();
            bitmapSource.Freeze();

            //得到画笔
            var brush = new ImageBrush(bitmapSource);

            return brush;
        }
        /// <summary>
        /// 得到网络图像画笔
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ImageBrush GetNetImageBrush(string url)
        {
            try
            {
                //转换为图像
                //var image = WebHandler.GetImgFile(url);
                //var bitmap = new System.Drawing.Bitmap(image);
                //var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                //                                                                      IntPtr.Zero,
                //                                                                      System.Windows.Int32Rect.Empty,
                //                                                                      BitmapSizeOptions.FromEmptyOptions()
                //                    );
                //bitmap.Dispose();
                //image.Dispose();

                //得到图像
                var image = WebHandler.GetImgFile(url);
                if (image != null)
                {
                    MemoryStream stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Png);
                    BitmapImage img = new BitmapImage();
                    using (stream)
                    {
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.StreamSource = stream;
                        img.EndInit();
                        img.Freeze();
                    }
                    return new ImageBrush(img);
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region 得到ImageSouce
        /// <summary>
        /// 得到ImageSouce
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageSource GetImageSource(Bitmap bitmap)
        {
            BitmapImage result = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
            }

            return result;
        }
        /// <summary>
        /// 得到ImageSouce
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ImageSource GetImageSource(Stream stream)
        {
            BitmapImage result = new BitmapImage();

            using (stream)
            {
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
            }

            return result;
        }
        /// <summary>
        /// 得到本地ImageSouce
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ImageSource GetLocalImageSource(string path)
        {
            //不存在文件
            if (!System.IO.File.Exists(path)) return null;

            //转换为图像
            //var image = System.Drawing.Image.FromFile(path);
            //var bitmap = new System.Drawing.Bitmap(image);
            //var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
            //                                                                      IntPtr.Zero,
            //                                                                      System.Windows.Int32Rect.Empty,
            //                                                                      BitmapSizeOptions.FromEmptyOptions()
            //                    );
            ////释放资源
            //bitmap.Dispose();
            //image.Dispose();

            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.UriSource = new Uri(path);
            bitmapSource.EndInit();
            bitmapSource.Freeze();

            return bitmapSource;
        }
        /// <summary>
        /// 得到网络ImageSouce
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ImageSource GetNetImageSource(string url)
        {
            try
            {
                //转换为图像
                //var image = WebHandler.GetImgFile(url);
                //var bitmap = new System.Drawing.Bitmap(image);
                //var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                //                                                                      IntPtr.Zero,
                //                                                                      System.Windows.Int32Rect.Empty,
                //                                                                      BitmapSizeOptions.FromEmptyOptions()
                //                    );
                //bitmap.Dispose();
                //image.Dispose();

                var image = WebHandler.GetImgFile(url);
                if (image != null)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Png);
                        if (stream != null && stream.Length > 0)
                        {
                            return GetImageSource(stream);
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 得到ImageSource
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource GetIconImageSouce(Icon icon)
        {
            if (icon == null) return null;

            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch { }

            return null;
        }

        #endregion
    }
}
