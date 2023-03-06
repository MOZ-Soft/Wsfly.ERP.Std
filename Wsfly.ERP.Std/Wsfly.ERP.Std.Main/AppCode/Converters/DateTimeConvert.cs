using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    public class DateTimeConvert : IValueConverter
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
            if (value == null || value == System.DBNull.Value) return "";

            DateTime bindValue = DataType.DateTime(value, DateTime.Now);
            int type = DataType.Int(parameter, 1);

            switch (type)
            {
                case 2: return bindValue.ToString("yyyy-MM-dd");
                case 3: return bindValue.ToString("HH:mm:ss");

                case 21: return bindValue.ToString("yyyy年MM月dd日 HH时mm分ss秒");
                case 22: return bindValue.ToString("yyyy年MM月dd日");
                case 23: return bindValue.ToString("HH时mm分ss秒");

                case 1:
                default: return bindValue.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 目标到源的转换
        /// </summary>
        /// <param name="value">绑定目标生成的值</param>
        /// <param name="typeTarget">要转换到的类型，也就是该方法的返回类型</param>
        /// <param name="param">要使用的转换器参数</param>
        /// <param name="culture">要用在转换器中的区域性</param>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
