using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 文件操作
    /// </summary>
    [Serializable]
    public class FileActionInfo
    {
        /// <summary>
        /// ICON
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 操作代码
        /// </summary>
        public string Code { get; set; }
    }
}
