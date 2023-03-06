using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    /// <summary>
    /// 绑定计算值
    /// </summary>
    public class CalculationConvert : IValueConverter
    {
        /// <summary>
        /// 源到目标的转换
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="typeTarget">绑定目标属性的类型，也就是该方法的返回类型</param>
        /// <param name="parameter">要使用的转换器参数（附加参数，可以是任何数据类型或者对象引用）</param>
        /// <param name="culture">要用在转换器中的区域性(一般可以忽略)</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = DataType.Double(value, 300);
            string jsgs = "";
            if (parameter != null && !string.IsNullOrWhiteSpace(parameter.ToString()))
            {
                jsgs = parameter.ToString();
            }

            //是否有公式
            if (string.IsNullOrWhiteSpace(jsgs)) return value;

            //替换公式值
            jsgs = jsgs.Replace("{0}", value.ToString());

            //计算
            return new System.Data.DataTable().Compute(jsgs, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
