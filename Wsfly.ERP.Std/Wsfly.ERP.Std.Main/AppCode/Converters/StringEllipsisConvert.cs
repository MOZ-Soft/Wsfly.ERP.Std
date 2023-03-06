using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Converters
{
    /// <summary>
    /// 绑定数据文本超长 显示省略号
    /// </summary>
    public class StringEllipsisConvert : IValueConverter
    {
        private bool _filterHtml = false;

        public StringEllipsisConvert()
        {

        }
        public StringEllipsisConvert(bool filterHtml)
        {
            _filterHtml = filterHtml;
        }

        /// <summary>
        /// 源到目标的转换
        /// 文本超过长度时显示省略号
        /// </summary>
        /// <param name="value">绑定源生成的值</param>
        /// <param name="typeTarget">绑定目标属性的类型，也就是该方法的返回类型</param>
        /// <param name="param">要使用的转换器参数（附加参数，可以是任何数据类型或者对象引用）</param>
        /// <param name="culture">要用在转换器中的区域性(一般可以忽略)</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bindValue = value.ToString();
            int length = DataType.Int(parameter, 0);
            
            if (length > 0)
            {
                //绑定的值
                //bindValue = Core.Handler.StringHandler.RemoveAllHtmlTag(bindValue);
                bindValue = Core.Handler.StringHandler.RemoveHTML(bindValue);
                //返回截取的值
                return Core.Handler.StringHandler.SubStringsByBytes(bindValue, length);
            }
            else
            {
                return bindValue;
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
