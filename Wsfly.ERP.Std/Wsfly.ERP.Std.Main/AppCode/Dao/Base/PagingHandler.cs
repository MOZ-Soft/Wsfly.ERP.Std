using System;
using System.Collections.Generic;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    public class PagingHandler
    {
        /// <summary>
        /// 得到分页SQL
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="cells">查询列</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">索引</param>
        /// <param name="where">条件</param>
        /// <param name="order">排序</param>
        /// <returns></returns>
        public static string GetPagingSql(string tableName, string cells, int pageSize, int pageIndex, string where, string order)
        {
            int beginCount = pageSize * (pageIndex - 1);

            if (!string.IsNullOrEmpty(where)) where = " WHERE " + where;
            if (!string.IsNullOrEmpty(order)) order = " ORDER BY " + order;

            string sql = "SELECT " + cells + " FROM " + tableName + where + order + " LIMIT " + pageSize + " OFFSET " + beginCount;

            return sql;
        }
        /// <summary>
        /// 查询前N条
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cells"></param>
        /// <param name="top"></param>
        /// <param name="where"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static string GetTopSql(string tableName, string cells, int top, string where, string order)
        {
            if (top <= 0) top = 10;

            if (!string.IsNullOrEmpty(where)) where = " WHERE " + where;
            if (!string.IsNullOrEmpty(order)) order = " ORDER BY " + order;

            ///查询SQL
            string sql = "SELECT " + cells + " FROM " + tableName + where + order + " LIMIT " + top;

            return sql;
        }
    }
}
