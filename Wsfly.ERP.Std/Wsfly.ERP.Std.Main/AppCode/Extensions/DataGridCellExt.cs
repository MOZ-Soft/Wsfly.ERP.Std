using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Wsfly.ERP.Std.AppCode.Extensions
{
    public static class DataGridCellExt
    {
        /// <summary>
        /// 是否编辑聚焦
        /// </summary>
        public static bool _isEditFocused = false;

        /// <summary>
        /// 是否编辑聚焦
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool GetEditFocused(this DataGridCell target)
        {
            return _isEditFocused;
        }
        /// <summary>
        /// 设置是否编辑聚焦
        /// </summary>
        /// <param name="target"></param>
        /// <param name="isFocus"></param>
        [DebuggerStepThrough]
        public static void SetEditFocused(this DataGridCell target, bool isFocus)
        {
            _isEditFocused = isFocus;
        }
    }
}
