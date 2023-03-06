using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 传递参数
    /// </summary>
    [Serializable]
    public class PostParamInfo
    {
        /// <summary>
        /// 主表行
        /// </summary>
        public DataRowView TopRow { get; set; }
        /// <summary>
        /// 子表行
        /// </summary>
        public DataRowView BottomRow { get; set; }
        /// <summary>
        /// 键值
        /// </summary>
        public List<KeyValue> KeyValues { get; set; }
    }
}
