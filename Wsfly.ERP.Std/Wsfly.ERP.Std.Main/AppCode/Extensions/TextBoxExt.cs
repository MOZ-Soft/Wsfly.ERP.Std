using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wsfly.ERP.Std.AppCode.Extensions
{
    /// <summary>
    /// 文本框类扩展
    /// </summary>
    public static class TextBoxExt
    {
        /// <summary>
        /// 得到整数
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static int GetInt(this TextBox target)
        {
            return DataType.Int(target.Text.Trim(), 0);
        }
        /// <summary>
        /// 得到小数
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static float GetFloat(this TextBox target)
        {
            return DataType.Float(target.Text.Trim(), 0);
        }
        /// <summary>
        /// 得到Double
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static double GetDouble(this TextBox target)
        {
            return DataType.Double(target.Text.Trim(), 0);
        }
        /// <summary>
        /// 得到Decimal
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static decimal GetDecimal(this TextBox target)
        {
            return DataType.Decimal(target.Text.Trim(), 0);
        }
        /// <summary>
        /// 得到Long
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetLong(this TextBox target)
        {
            return DataType.Long(target.Text.Trim(), 0);
        }
        /// <summary>
        /// 得到日期
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTime GetDate(this TextBox target)
        {
            return DataType.DateTime(target.Text.Trim(), DateTime.Now);
        }
    }
}
