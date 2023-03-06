using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Enums
{
    /// <summary>
    /// 项目状态
    /// </summary>
    public enum WorkFeedbackProjectState
    {
        未完成,
        已完成,
        已取消
    }
    /// <summary>
    /// 反馈状态
    /// </summary>
    public enum WorkFeedbackProjectDetailState
    {
        未处理,
        已处理,
        已忽略,
        已取消,
        已变更
    }
}
