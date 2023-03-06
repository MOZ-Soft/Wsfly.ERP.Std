
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.AppCode.Models
{
    public class DropDownInfo
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string CellName { get; set; }
        /// <summary>
        /// 列信息
        /// </summary>
        public CellInfo CellInfo { get; set; }
        /// <summary>
        /// 选择值
        /// </summary>
        public object ChooseValue { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public System.Data.DataTable Data { get; set; }

    }
}
