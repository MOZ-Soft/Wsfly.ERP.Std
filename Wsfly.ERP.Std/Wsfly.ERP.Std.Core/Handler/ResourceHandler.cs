using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 当前程序集资源
    /// </summary>
    public class ResourceHandler
    {
        /// <summary>
        /// 得到资源文件
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static Stream GetResource(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            //资源名称
            Assembly assembly = Assembly.GetExecutingAssembly();
            //读取资源
            Stream stream = assembly.GetManifestResourceStream(name);

            //返回资源
            return stream;
        }



        /// <summary>
        /// 得到资源图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Image GetImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            //得到图片流
            Stream stream = GetResource(name);
            //流是否为空
            if (stream == null) return null;
            //转换为图片对象
            Image img = Image.FromStream(stream);
            //释放资源
            stream.Close();
            //返回图片
            return img;
        }

        /// <summary>
        /// 得到 WPF ImageBrush
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Windows.Media.ImageBrush GetImageBrush(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            System.Windows.Media.ImageSource bitmapImage = GetImageSource(name);
            System.Windows.Media.ImageBrush brush = new System.Windows.Media.ImageBrush();
            brush.ImageSource = bitmapImage;

            return brush;
        }
        /// <summary>
        /// 得到WPF ImageSource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Windows.Media.ImageSource GetImageSource(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Stream stream = GetResource(name);

            if (stream == null) return null;

            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        /// <summary>
        /// 得到ICON图标
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Icon GetIcon(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            //得到图片流
            Stream stream = GetResource(name);
            //流是否为空
            if (stream == null) return null;
            //得到ICON
            Icon icon = Icon.FromHandle(new Bitmap(stream).GetHicon());
            //释放资源
            stream.Close();
            //返回图片
            return icon;
        }

        /// <summary>
        /// 得到声音文件
        /// </summary>
        /// <param name="name"></param>
        public static Stream GetWav(string name)
        {
            return GetResource(name);
        }

    }
}
