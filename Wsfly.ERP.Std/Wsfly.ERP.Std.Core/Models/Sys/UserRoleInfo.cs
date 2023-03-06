using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Sys
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [Serializable]
    public class UserRoleInfo
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
    }
}
