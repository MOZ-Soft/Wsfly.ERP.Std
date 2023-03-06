using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Enums
{
    /// <summary>
    /// 状态枚举
    /// </summary>
    public enum StateEnums
    {
        等待处理 = 0,
        正在处理 = 1,
        处理完成 = 2,
        处理异常 = 3
    }
}
