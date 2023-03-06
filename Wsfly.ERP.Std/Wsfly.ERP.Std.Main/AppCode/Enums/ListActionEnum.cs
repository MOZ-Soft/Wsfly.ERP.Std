using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Enums
{
    /// <summary>
    /// 列表操作枚举
    /// </summary>
    public enum ListActionEnum
    {
        扩展三表 = -7,
        查询设置 = -6,
        列排序 = -5,
        保存布局 = -4,
        引用 = -3,
        退出 = -2,
        刷新 = -1,
        Null = 0,
        开单 = 1,
        添加 = 2,
        编辑 = 3,
        删除 = 4,
        保存 = 5,
        打印 = 6,
        打印设置 = 7,
        审核 = 8,
        反审 = 9,
        导入 = 10,
        导出 = 11,
        取消 = 12
    }
}
