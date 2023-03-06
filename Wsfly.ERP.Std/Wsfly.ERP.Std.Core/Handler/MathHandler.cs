using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 计算辅助
    /// </summary>
    public class MathHandler
    {
        /// <summary>
        /// 计算总页数
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static int CalculatePageCount(long totalCount, int pageSize)
        {
            if (totalCount <= 0) return 0;

            return (int)(totalCount + pageSize - 1) / pageSize;
        }
    }
}
