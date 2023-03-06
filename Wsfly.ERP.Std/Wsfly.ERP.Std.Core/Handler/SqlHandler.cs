using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class SqlHandler
    {
        /// <summary>
        /// 过滤不安全的SQL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FilterSql(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return value.Replace("'", "&prime;").Replace("\"", "&Prime;");
        }
        /// <summary>
        /// 生成Where条件
        /// </summary>
        /// <param name="CellNames"></param>
        /// <returns></returns>
        public static string BuildWhere(string keywords, string[] CellNames)
        {
            string where = "1=1";

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                string[] keys = SqlHandler.FilterSql(keywords).Split(' ');

                where += " AND (";

                foreach (string key in keys)
                {
                    foreach (string cell in CellNames)
                    {
                        where += "[" + cell + "] LIKE '%" + key + "%' OR";
                    }
                }

                where = where.Substring(0, where.Length - 3);

                where += ")";
            }

            return where;
        }
    }
}
