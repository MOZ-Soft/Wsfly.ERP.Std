using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 颜色笔刷
    /// </summary>
    public class ColorBrushHandler
    {
        /// <summary>
        /// HTML RGB颜色值转Color对象
        /// </summary>
        /// <param name="htmlRGB">HTML颜色 如：#E0E0E0</param>
        /// <returns></returns>
        public static System.Windows.Media.Color HtmlRGBToColor(string htmlRGB)
        {
            try
            {
                return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(htmlRGB);
            }
            catch { }

            return System.Windows.Media.Colors.Transparent;
        }
        /// <summary>
        /// 将HTML RGB颜色值转Color对象
        /// </summary>
        /// <param name="htmlRGB">HTML颜色 如：#E0E0E0</param>
        /// <returns></returns>
        public static System.Windows.Media.Color HtmlRGBToColor2(string htmlRGB)
        {
            try
            {
                int r = 0, g = 0, b = 0;
                if (htmlRGB.StartsWith("#"))
                {
                    int v = Convert.ToInt32(htmlRGB.Substring(1), 16);
                    r = (v >> 16) & 255; g = (v >> 8) & 255; b = v & 255;
                }
                return System.Windows.Media.Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
            }
            catch { }

            return System.Windows.Media.Colors.Transparent;
        }
        /// <summary>
        /// 颜色转笔刷
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Windows.Media.SolidColorBrush ColorToBrush(System.Windows.Media.Color color)
        {
            return new System.Windows.Media.SolidColorBrush(color);
        }
        /// <summary>
        /// HTML RGB颜色值转笔刷
        /// </summary>
        /// <param name="htmlRGB"></param>
        /// <returns></returns>
        public static System.Windows.Media.SolidColorBrush HtmlRGBToBrush(string htmlRGB)
        {
            return new System.Windows.Media.SolidColorBrush(HtmlRGBToColor(htmlRGB));
        }
    }
}
