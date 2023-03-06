using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    /// <summary>
    /// 绑定Bool值
    /// </summary>
    public class BoolConvert : IValueConverter
    {
        /// <summary>
        /// 源到目标的转换
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="typeTarget">绑定目标属性的类型，也就是该方法的返回类型</param>
        /// <param name="param">要使用的转换器参数（附加参数，可以是任何数据类型或者对象引用）</param>
        /// <param name="culture">要用在转换器中的区域性(一般可以忽略)</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == System.DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return "否";
            }

            bool flag = DataType.Bool(value, false);

            return flag ? "是" : "否";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
