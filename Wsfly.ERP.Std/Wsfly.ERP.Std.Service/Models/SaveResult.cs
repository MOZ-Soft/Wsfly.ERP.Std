using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wsfly.ERP.Std.Service.Models
{
    public class SaveResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 行索引
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// 结果ID
        /// </summary>
        public long ResultId { get; set; }
        /// <summary>
        /// 新行的值
        /// </summary>
        public DataRow NewRow { get; set; }


        /// <summary>
        /// 原ID
        /// </summary>
        public long _ORGID { get; set; }
    }
}
