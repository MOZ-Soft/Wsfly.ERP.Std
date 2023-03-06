using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 未验证通过的行
    /// </summary>
    [Serializable]
    public class NotCheckPassRow2
    {
        /// <summary>
        /// 行
        /// </summary>
        public DataRow Row { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
