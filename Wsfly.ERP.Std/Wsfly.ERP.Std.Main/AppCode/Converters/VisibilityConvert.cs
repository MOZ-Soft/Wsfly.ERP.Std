using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    public class VisibilityConvert : IValueConverter
    {
        /// <summary>
        /// 源到目标的转换
        /// 控件是否显示
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="typeTarget">绑定目标属性的类型，也就是该方法的返回类型</param>
        /// <param name="param">要使用的转换器参数（附加参数，可以是任何数据类型或者对象引用）</param>
        /// <param name="culture">要用在转换器中的区域性(一般可以忽略)</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int type = DataType.Int(parameter, 1);
            //1:true-Visible  false-Collapsed
            //2:true-Collapsed  false-Visible

            if (value == null || value == DBNull.Value)
            {
                if(type == 2) return System.Windows.Visibility.Visible;
                return System.Windows.Visibility.Collapsed;
            }

            bool flag = DataType.Bool(value, false);

            if (flag)
            {
                //true:
                if (type == 2) return System.Windows.Visibility.Collapsed;
                else return System.Windows.Visibility.Visible;
            }

            //false:
            if(type == 2) return System.Windows.Visibility.Visible;
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
