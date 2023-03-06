using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Wsfly.ERP.Std.Core.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// 得到行 如：Id=1
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DataRow GetRow(this DataTable target, string columnName, string cellValue)
        {
            if (target == null) return null;
            if (!target.Columns.Contains(columnName)) return null;
            DataRow[] rows = target.Select("[" + columnName + "]='" + cellValue + "'");
            if (rows == null || rows.Length <= 0) return null;
            return rows[0];
        }

        /// <summary>
        /// 得到行根据Id列 如：Id=1
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DataRow GetRowByIdColumn(this DataTable target, long id)
        {
            return target.GetRow("Id", id.ToString());
        }

        /// <summary>
        /// 转为集合列表
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static List<Dictionary<string, object>> ToDictionary(this DataTable target)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            if (target == null || target.Rows.Count <= 0) return list;

            foreach (DataRow row in target.Rows)
            {
                Dictionary<string, object> dicRow = new Dictionary<string, object>();

                foreach (DataColumn column in target.Columns)
                {
                    dicRow.Add(column.ColumnName, row[column.ColumnName]);
                }

                list.Add(dicRow);
            }

            return list;
        }
        /// <summary>
        /// 转为键值集合
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Dictionary<string, object> ToKeyValueDictionary(this DataTable target)
        {
            Dictionary<string, object> dicKeyValue = new Dictionary<string, object>();

            if (target == null || target.Rows.Count <= 0) return dicKeyValue;
            if (!target.Columns.Contains("Key") || !target.Columns.Contains("Value")) return dicKeyValue;

            foreach (DataRow row in target.Rows)
            {
                try
                {
                    dicKeyValue.Add(row.GetString("Key"), row["Value"]);
                }
                catch { }
            }

            return dicKeyValue;
        }

        /// <summary>
        /// 转换列类型
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <param name="toType"></param>
        public static void ChangeColumnType(this DataTable target, string columnName, Type toType)
        {
            if (target == null) return;

            //增加备份列
            target.Columns.Add(new DataColumn(columnName + "_bak", toType));
            if (target.Rows.Count <= 0) return;

            //遍历数据
            foreach (DataRow row in target.Rows)
            {
                try
                {
                    row[columnName + "_bak"] = Convert.ChangeType(row[columnName], toType);
                }
                catch { }
            }

            //移除原列 再将备份列名改为原列名
            target.Columns.Remove(columnName);
            target.Columns[columnName + "_bak"].ColumnName = columnName;
        }
        /// <summary>
        /// 是否有排序列
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool HasDataIndex(this DataTable target)
        {
            if (target == null) return false;
            return target.Columns.Contains("DataIndex");
        }
        /// <summary>
        /// 保留列
        /// </summary>
        /// <param name="retainColumns"></param>
        public static void RetainColumns(this DataTable target, string[] retainColumns)
        {
            try
            {
                List<DataColumn> removeColumns = new List<DataColumn>();
                foreach (DataColumn col in target.Columns)
                {
                    if (!retainColumns.Contains(col.ColumnName))
                    {
                        removeColumns.Add(col);
                    }
                }

                foreach (DataColumn col in removeColumns)
                {
                    target.Columns.Remove(col);
                }
            }
            catch { }
        }
        /// <summary>
        /// 克隆表和数据
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DataTable CloneDataTableAndData(this DataTable target)
        {
            if (target == null) return null;
            if (target.Rows.Count <= 0) return target.Clone();

            var dt = target.Clone();
            foreach (DataRow row in target.Rows)
            {
                dt.ImportRow(row);
            }
            return dt;
        }
    }
}