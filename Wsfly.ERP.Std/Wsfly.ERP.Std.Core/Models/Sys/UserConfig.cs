using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Sys
{
    /// <summary>
    /// 个人配置
    /// </summary>
    [Serializable]
    public class UserConfig
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 主题名称
        /// </summary>
        public string ThemeName { get; set; }
        /// <summary>
        /// 是否直接退出程序
        /// </summary>
        public bool IsDirctExit { get; set; }
        /// <summary>
        /// 是否全屏显示
        /// </summary>
        public bool IsFullScreen { get; set; }
    }
}
