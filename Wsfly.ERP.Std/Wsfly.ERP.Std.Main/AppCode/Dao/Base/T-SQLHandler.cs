using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    /// <summary>
    /// 生成SQL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TSQLHandler<T>
    {
        #region 属性
        private static string _tableName;//表名
        private static string _tableColumns;//列名
        /// <summary>
        /// 表名
        /// </summary>
        public static string _TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    T _default = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
                    //无数据表名，获取表名
                    object tbName = typeof(T).GetProperty("_TableName").GetValue(_default, null);
                    if (tbName == null) return "未配置数据表的名称";
                    _tableName = tbName.ToString().Trim('[').Trim(']');
                }

                return _tableName;
            }
        }
        /// <summary>
        /// 表列
        /// </summary>
        public static string _TableColumns
        {
            get
            {
                if (string.IsNullOrEmpty(_tableColumns))
                {
                    T _default = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
                    object tbColumns = typeof(T).GetProperty("_TableColumns").GetValue(_default, null);
                    if (tbColumns == null) return "未配置数据表列的名称";
                    _tableColumns = tbColumns.ToString();
                }
                return _tableColumns;
            }
        }
        #endregion


        /// <summary>
        /// 得到查询数据SQL
        /// </summary>
        /// <returns></returns>
        public static string GetSelectSql(string where)
        {
            ///查询SQL
            string sql = "SELECT * FROM " + _TableName;

            ///如果有条件 增加WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///返回SQL
            return sql;
        }
        /// <summary>
        /// 得到插入数据SQL
        /// </summary>
        /// <returns></returns>
        public static string GetInsertSql()
        {
            ///去除表列的前后[]
            string cells = _TableColumns;
            cells = cells.Trim('[').Trim(']');

            ///生成@CellName模式
            cells = "@" + cells.Replace("],[", ",@");

            ///整合T-SQL
            string sql = "INSERT INTO [" + _TableName + "](" + _TableColumns + ") VALUES(" + cells + ")";

            ///返回
            return sql;
        }
        /// <summary>
        /// 得到更新数据SQL
        /// </summary>
        /// <returns></returns>
        public static string GetUpdateSql(string where)
        {
            ///SQL
            string sql = "UPDATE [" + _TableName + "] SET ";

            ///将列分成数组
            string[] cls = _TableColumns.Split(',');

            ///循环组装T-SQL [CellName]=@CellName,
            for (int i = 0; i < cls.Length; i++)
            {
                sql += cls[i] + "=@" + cls[i].Trim('[').Trim(']') + ",";
            }

            ///去除最后的“，”
            sql = sql.Trim(',');

            ///如果有条件 增加WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///返回T-SQL
            return sql;
        }
        /// <summary>
        /// 得到删除数据SQL
        /// </summary>
        /// <returns></returns>
        public static string GetDeleteSql(string where)
        {
            ///删除SQL
            string sql = "DELETE FROM [" + _TableName + "]";

            ///如果有条件 增加WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///返回SQL
            return sql;
        }


        /// <summary>
        /// 将列转换为参数SQL
        /// 如：
        /// 参数：[ID],[Name],[Remark]
        /// 返回：@ID,@Name,@Remark
        /// </summary>
        /// <returns></returns>
        public static string GetParameterSql()
        {
            return "@" + _TableColumns.Replace("],[", ",@").Trim('[').Trim(']');
        }
        /// <summary>
        /// 得到SQL的参数列映射
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static SQLiteParameter[] GetParameters(T obj)
        {
            Transform<T> transform = new Transform<T>();
            return transform.ObjectToParameters(obj, _TableColumns);
        }
    }
}
