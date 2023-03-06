using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    public class IconColorConverter : IValueConverter
    {
        /// <summary>
        /// 源到目标的转换
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="typeTarget">绑定目标属性的类型，也就是该方法的返回类型</param>
        /// <param name="param">要使用的转换器参数（附加参数，可以是任何数据类型或者对象引用）</param>
        /// <param name="culture">要用在转换器中的区域性(一般可以忽略)</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bindValue = value.ToString();
            int type = DataType.Int(parameter, 0);

            if (string.IsNullOrWhiteSpace(bindValue)) return null;

            try
            {
                if (bindValue.StartsWith(":"))
                {
                    string packType = bindValue.Split(':')[1];
                    string iconStr = bindValue.Split(':')[2];
                    string colorStr = bindValue.Split(':')[3];

                    Brush brush = Brushes.Black;

                    try
                    {
                        brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
                    }
                    catch (Exception ex) { }

                    return brush;
                }
            }
            catch (Exception ex) { }
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
