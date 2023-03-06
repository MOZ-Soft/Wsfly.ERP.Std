using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Results
{
    /// <summary>
    /// 地区信息
    /// </summary>
    [Serializable]
    public class AreaInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 上级名称
        /// </summary>
        public long ParentId { get; set; }
        /// <summary>
        /// 地区名称
        /// </summary>
        public string AreaName { get; set; }
    }
}
