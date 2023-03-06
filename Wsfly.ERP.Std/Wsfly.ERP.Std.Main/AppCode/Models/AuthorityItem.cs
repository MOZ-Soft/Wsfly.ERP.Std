using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 权限项
    /// </summary>
    public class AuthorityItem
    {
        /// <summary>
        /// 模块行
        /// </summary>
        public DataRow ModuleRow { get; set; }
        /// <summary>
        /// 子模块行
        /// </summary>
        public DataRow ModuleDetailRow { get; set; }
        /// <summary>
        /// 操作行
        /// </summary>
        public DataRow ActionRow { get; set; }
    }
}
