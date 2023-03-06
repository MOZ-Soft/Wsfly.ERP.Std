
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 复制数据
    /// </summary>
    [Serializable]
    public class CopyDataInfo
    {
        /// <summary>
        /// 表配置
        /// </summary>
        public TableInfo TableInfo { get; set; }
        /// <summary>
        /// 主表记录
        /// </summary>
        public DataRow TopRowData { get; set; }
        /// <summary>
        /// 子表记录
        /// </summary>
        public DataTable SubTableData { get; set; }


    }
}
