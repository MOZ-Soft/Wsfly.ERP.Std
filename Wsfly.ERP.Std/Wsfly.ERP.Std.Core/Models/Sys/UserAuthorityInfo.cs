using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Sys
{
    /// <summary>
    /// 用户权限
    /// </summary>
    [Serializable]
    public class UserAuthorityInfo
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 模块编号
        /// </summary>
        public long ModuleId { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }
        /// <summary>
        /// 表编号
        /// </summary>
        public long TableId { get; set; }
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 操作编号
        /// </summary>
        public long ActionId { get; set; }
        /// <summary>
        /// 操作Code
        /// </summary>
        public int Code { get; set; }
    }
}
