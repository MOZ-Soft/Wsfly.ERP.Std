using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Service.Models
{
    /// <summary>
    /// 查询分页返回结果
    /// </summary>
    [Serializable]
    public class PageResult
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; set; }
        /// <summary>
        /// 分页尺码
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 当前页数
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get
            {
                if (PageSize == 0) return 0;
                return (int)((TotalCount + PageSize - 1) / PageSize);
            }
        }
        /// <summary>
        /// 分页数据
        /// </summary>
        public DataTable Data { get; set; }
        /// <summary>
        /// 分页源数据
        /// </summary>
        public DataTable OrgData { get; set; }
        /// <summary>
        /// 汇总数据
        /// </summary>
        public DataTable Summarys { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public decimal ZSL { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal ZJE { get; set; }

        /// <summary>
        /// 数据排序
        /// </summary>
        /// <param name="sort"></param>
        public void DataSort(string cellName = "Id", OrderType order = OrderType.顺序)
        {
            if (this.Data == null) return;
            if (!this.Data.Columns.Contains(cellName)) return;

            //排序
            DataView dv = this.Data.AsDataView();
            dv.Sort = cellName + " " + (order == OrderType.顺序 ? "asc" : "desc");
            this.Data = dv.ToTable();
        }
    }
}
