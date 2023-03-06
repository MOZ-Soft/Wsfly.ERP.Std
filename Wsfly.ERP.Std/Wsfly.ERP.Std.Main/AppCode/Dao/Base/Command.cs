using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    /// <summary>
    /// 命令
    /// </summary>
    public class BaseDao<T>
    {
        #region 变量
        private string _tableName;
        private string _tableColumns;

        /// <summary>
        /// 转换对象
        /// </summary>
        public Transform<T> _transform = new Transform<T>();
        /// <summary>
        /// 数据库操作
        /// </summary>
        public DbHelper _db = new DbHelper();
        #endregion

        #region 属性
        /// <summary>
        /// 表名
        /// </summary>
        public string _TableName
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
        public string _TableColumns
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
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        public DbHelper cmd { get { return _db; } }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { set { _db.ConnectionString = value; } }
        /// <summary>
        /// 是否可以连接数据库
        /// </summary>
        //public bool CanConnectionDatabase { get { return cmd.CanConnectDatabase(); } }
        #endregion

        #region 获取SQL
        /// <summary>
        /// 将列转换为参数SQL
        /// 如：
        /// 参数：[ID],[Name],[Remark]
        /// 返回：@ID,@Name,@Remark
        /// </summary>
        /// <returns></returns>
        public string GetParameterSql()
        {
            return "@" + _TableColumns.Replace("],[", ",@").Trim('[').Trim(']');
        }
        /// <summary>
        /// 得到查询T-SQL
        /// </summary>
        /// <returns></returns>
        public string GetSelectSQL(string where)
        {
            ///查询SQL
            string sql = "SELECT * FROM " + _TableName;

            ///如果有条件 增加WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///返回SQL
            return sql;
        }
        /// <summary>
        /// 得到插入T-SQL
        /// </summary>
        /// <param name="cells">列</param>
        /// <returns></returns>
        public string GetInsertSQL()
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
        /// 得到更新T-SQL
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        public string GetUpdateSQL(string where)
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
        /// 得到删除T-SQL
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public string GetDeleteSQL(string where)
        {
            ///删除SQL
            string sql = "DELETE FROM [" + _TableName + "]";

            ///如果有条件 增加WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///返回SQL
            return sql;
        }
        #endregion

        #region 查询操作

        #region 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exists(string where)
        {
            SQLiteParameter[] ps = null;

            return Exists(where, ps);
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool Exists(string where, Dictionary<string, object> param)
        {
            SQLiteParameter[] ps = null;

            if (param != null)
            {
                ///声明数组
                ps = new SQLiteParameter[param.Count];
                int index = 0;

                //循环Pair
                foreach (KeyValuePair<string, object> pair in param)
                {
                    ///名称
                    string name = pair.Key.StartsWith("@") ? pair.Key : "@" + pair.Key;
                    ///参数
                    SQLiteParameter sp = new SQLiteParameter(name, pair.Value);

                    ps[index++] = sp;
                }
            }

            return Exists(where, ps);
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exists(string where, SQLiteParameter[] ps)
        {
            ///SQL
            string sql = "SELECT COUNT(*) FROM " + _TableName;
            ///是否有条件
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;
            ///返回值
            object result = _db.ExecuteScalar(sql, ps);
            ///如果有数字且大于0 则存在 否则不存在
            if (Core.Handler.RegexHandler.IsInt(result) && Convert.ToInt32(result) > 0)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 查询记录数
        /// <summary>
        /// 查询记录数
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public long Count(string where)
        {
            string sql = "SELECT COUNT(*) FROM " + _TableName + (string.IsNullOrEmpty(where) ? "" : " WHERE " + where);

            object objCount = this.cmd.ExecuteScalar(sql, null);

            if (objCount == null) return 0;

            return long.Parse(objCount.ToString());
        }
        #endregion

        #region 查询单个对象
        /// <summary>
        /// 根据ID查询对象
        /// </summary>
        /// <param name="id">编号</param>
        /// <returns></returns>
        public T GetObject(long id)
        {
            ///查询SQL
            string sql = GetSelectSQL("[Id]=" + id);
            ///查询结果
            IList<T> results = QueryList(sql, null);
            ///查询结果为NULL
            if (results == null || results.Count <= 0) return default(T);
            ///返回结果
            return results[0];
        }
        /// <summary>
        /// 根据条件查询对象
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public T GetObject(string where)
        {
            ///生成SQL
            string sql = "SELECT * FROM " + _TableName + " WHERE " + where + " LIMIT 1";
            ///查询结果
            IList<T> results = QueryList(sql, null);
            ///如果为空则返回 NULL
            if (results == null || results.Count <= 0) return default(T);
            ///返回结果
            return results[0];
        }
        /// <summary>
        /// 查询第一个对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public T GetTop1Object(string sql)
        {
            ///生成SQL
            sql = "SELECT * FROM (" + sql + ") newT LIMIT 1";
            ///查询结果
            IList<T> results = QueryList(sql, null);
            ///如果为空则返回 NULL
            if (results == null || results.Count <= 0) return default(T);
            ///返回结果
            return results[0];
        }
        /// <summary>
        /// 返回第一行第一列结果
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, SQLiteParameter[] parameters)
        {
            return _db.ExecuteScalar(sql, parameters);
        }
        #endregion

        #region 查询对象列表
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns>结果</returns>
        public IList<T> GetList(string where)
        {
            ///参数
            IList<T> result = null;
            ///得到SQL
            string sql = GetSelectSQL(where);
            ///得到结果
            result = QueryList(sql, null);
            ///返回结果
            return result;
        }
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="top"></param>
        /// <param name="where"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public IList<T> GetList(int top, string where, string order)
        {
            ///参数
            IList<T> result = null;
            ///查询SQL
            string sql = PagingHandler.GetTopSql(_TableName, "*", top, where, order);
            ///查询结果
            result = QueryList(sql, null);
            ///返回结果
            return result;
        }
        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="where"></param>
        /// <param name="order"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<T> GetList(int pageSize, int pageIndex, string where, string order, ref long count)
        {
            DataTable dt = GetDataTable(pageSize, pageIndex, where, order, ref count);

            return _transform.DataTableToList(dt);
        }

        #endregion

        #region 查询数据表
        /// <summary>
        /// 查询数据行
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public DataRow GetDataRow(string where)
        {
            ///变量
            DataTable result = null;
            ///查询结果
            result = GetDataTable(where);
            ///是否有值
            if (result == null || result.Rows.Count <= 0) return null;
            ///返回结果
            return result.Rows[0];
        }
        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns></returns>
        public DataTable GetDataTable(string where)
        {
            ///变量
            DataTable result = null;
            ///SQL
            string sql = GetSelectSQL(where);
            ///返回结果
            result = _db.QuerySqlReturnDataTable(sql);
            ///返回结果
            return result;
        }
        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <param name="top">前几条</param>
        /// <param name="where">条件</param>
        /// <param name="order">排序</param>
        /// <returns></returns>
        public DataTable GetDataTable(int top, string where, string order)
        {
            DataTable result = null;
            ///查询SQL
            string sql = PagingHandler.GetTopSql(_TableName, "*", top, where, order);
            ///查询数据表
            result = _db.QuerySqlReturnDataTable(sql);
            ///返回结果
            return result;
        }
        /// <summary>
        /// 分页查询数据表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="where"></param>
        /// <param name="order"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public DataTable GetDataTable(int pageSize, int pageIndex, string where, string order, ref long totalCount)
        {
            string sql = PagingHandler.GetPagingSql(_TableName, "*", pageSize, pageIndex, where, order);

            DataTable dt = _db.QuerySqlReturnDataTable(sql);

            if (!string.IsNullOrEmpty(where)) where = " WHERE " + where;
            if (!string.IsNullOrEmpty(order)) order = " ORDER BY " + order;

            //查询数量
            sql = "SELECT COUNT(*) FROM " + _TableName + where;
            object objCount = _db.QuerySqlReturnObject(sql);

            if (objCount == null) totalCount = 0;
            else totalCount = long.Parse(objCount.ToString());

            return dt;
        }
        #endregion

        #endregion

        #region 执行SQL
        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        public bool ExecuteNonQuery(string sql, SQLiteParameter[] ps = null)
        {
            return _db.ExecuteNonQuery(sql, ps);
        }
        #endregion

        #region 更新操作 非事务
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public long Add(T obj)
        {
            ///SQL
            string sql = GetInsertSQL();
            ///参数化对象
            SQLiteParameter[] ps = _transform.ObjectToParameters(obj, _TableColumns);
            ///执行SQL 返回主键
            object lastId = _db.ExecuteScalar(sql, ps);

            if (lastId != null)
            {
                return Convert.ToInt64(lastId);
            }

            return 0;
        }
        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public bool Update(T obj, string where = null)
        {
            ///默认根据Id修改
            if (string.IsNullOrEmpty(where)) where = "[Id]=" + typeof(T).GetProperty("Id").GetValue(obj, null);

            ///生成SQL
            string sql = GetUpdateSQL(where);

            ///参数化对象
            SQLiteParameter[] ps = _transform.ObjectToParameters(obj, _TableColumns);

            ///返回结果
            return _db.ExecuteNonQuery(sql, ps);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(long id)
        {
            ///SQL
            string sql = GetDeleteSQL("[Id]=@Id");
            //参数
            SQLiteParameter[] ps = {
                                    new SQLiteParameter("@Id", SqlDbType.BigInt),
                                };
            //赋Id
            ps[0].Value = id;
            //删除操作
            return _db.ExecuteNonQuery(sql, ps);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool Delete(string ids)
        {
            ///SQL条件
            string where = "[Id] IN (" + ids + ")";
            ///调用函数
            return DeleteByCondition(where);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool DeleteByCondition(string where)
        {
            ///SQL
            string sql = GetDeleteSQL(where);
            ///执行SQL
            return _db.ExecuteNonQuery(sql);
        }
        #endregion

        #region 更新操作 事务
        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            _db.DbBeginTransaction();
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            _db.DbCommitTransaction();
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTransaction()
        {
            _db.DbRollbackTransaction();
        }
        /// <summary>
        /// 事务执行脚本
        /// </summary>
        /// <param name="sql"></param>
        public bool TranUpdateSql(string sql)
        {
            return _db.TranExecuteNonQuery(sql, null);
        }
        /// <summary>
        /// 事务执行脚本带参数
        /// </summary>
        public bool TranUpdateSqlWhiteParameters(string sql, SQLiteParameter[] ps)
        {
            return _db.TranExecuteNonQuery(sql, ps);
        }
        /// <summary>
        /// 事务执行脚本带参数 返回主键
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public object TranUpdateSqlWhiteParametersScalar(string sql, SQLiteParameter[] ps)
        {
            return _db.TranExecuteScalar(sql, ps);
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 查询对象结果
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">参数</param>
        /// <returns>结果</returns>
        public T QueryObject(string sql, SQLiteParameter[] parameters)
        {
            IList<T> results = QueryList(sql, parameters);

            if (results == null || results.Count <= 0) return default(T);

            return results[0];
        }
        /// <summary>
        /// 查询列表结果
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public IList<T> QueryList(string sql, SQLiteParameter[] ps)
        {
            ///参数
            IList<T> result = null;
            ///Reader
            SQLiteDataReader reader = _db.QuerySqlReturnDataReader(sql, ps);
            ///查询结果
            result = _transform.FillCollection(reader);
            ///返回结果
            return result;
        }
        /// <summary>
        /// 查询分页 SQL
        /// </summary>
        /// <param name="tableName">查询表</param>
        /// <param name="fieldName">查询字段</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="order">排序 不需要（order by）</param>
        /// <param name="where">条件 不需要（where）</param>
        /// <returns></returns>
        public IList<T> QueryPagingList(string from, string cells, int pageSize, int pageIndex, string where, string order, ref long count)
        {
            return QueryPagingDataReader(from, cells, pageSize, pageIndex, where, order, ref count);
        }
        /// <summary>
        /// 查询分页 SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="order"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        private IList<T> QueryPagingDataReader(string from, string cells, int pageSize, int pageIndex, string where, string order, ref long count)
        {
            if (string.IsNullOrEmpty(where)) where = null;
            if (string.IsNullOrEmpty(order)) order = null;

            ///脚本
            string sql = PagingHandler.GetPagingSql(from, cells, pageSize, pageIndex, where, order);

            ///查询DataReader
            SQLiteDataReader reader = _db.QuerySqlReturnDataReader(sql, null);

            ///查询列表
            IList<T> result = _transform.FillCollection(reader);
            ///总数量
            if (count <= 0)
            {
                object obj = _db.QuerySqlReturnObject("SELECT COUNT(*) FROM " + from + (string.IsNullOrEmpty(where) ? "" : " WHERE " + where));

                count = Convert.ToInt64(obj);
            }

            ///返回Reader
            return result;
        }
        /// <summary>
        /// 过滤不安全的SQL
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FileterSql(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            return value.Replace("'", "&prime;").Replace("\"", "&Prime;");
        }
        #endregion
    }
}
