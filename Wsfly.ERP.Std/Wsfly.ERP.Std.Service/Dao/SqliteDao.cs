using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Encryption;
using Wsfly.ERP.Std.Core.Extensions;

namespace Wsfly.ERP.Std.Service.Dao
{
    /// <summary>
    /// SQLiteDAO
    /// </summary>
    public class SQLiteDao
    {
        #region 初始数据库
        /// <summary>
        /// 默认数据库名称
        /// </summary>
        private static string _defaultDBName = "Wsfly.DC.ERP";
        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="path"></param>
        private static bool CreateDatabase(string path = null)
        {
            //数据库文件
            if (string.IsNullOrEmpty(path)) path = AppDomain.CurrentDomain.BaseDirectory + "AppData\\" + _defaultDBName + ".db";

            //已经有数据库
            if (File.Exists(path)) return false;

            try
            {
                //创建数据库
                SQLiteConnection.CreateFile(path);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 是否存在数据库
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsExistDatabase(string path = null)
        {
            //数据库文件
            if (string.IsNullOrEmpty(path)) path = AppDomain.CurrentDomain.BaseDirectory + "AppData\\" + _defaultDBName + ".db";

            //是否已经有数据库
            return File.Exists(path);
        }
        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void InitDatabase(string path = null)
        {
            try
            {
                //创建数据库文件
                bool flag = CreateDatabase(path);
                AppLog.WriteDebugLog("初始数据库结果：" + (flag ? "1" : "0"));
                if (flag)
                {
                    //数据库文件名称
                    string dbFileName = _defaultDBName;
                    if (!string.IsNullOrEmpty(path))
                    {
                        dbFileName = System.IO.Path.GetFileNameWithoutExtension(path);
                    }

                    //执行SQL
                    path = AppDomain.CurrentDomain.BaseDirectory + "AppData\\" + dbFileName + ".sql";
                    if (File.Exists(path))
                    {
                        //SQLiteCommand
                        SQLiteCommand cmd = GetSQLiteCmd();

                        //执行初始SQL
                        string sql = File.ReadAllText(path);
                        ExecuteNonQuery(cmd, sql);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region SQLiteCommand
        /// <summary>
        /// 得到SqlCmd
        /// </summary>
        /// <returns></returns>
        public static SQLiteCommand GetSQLiteCmd()
        {
            //默认数据库地址
            string defaultPath = AppDomain.CurrentDomain.BaseDirectory + "AppData\\" + _defaultDBName + ".db";

            //默认数据库连接
            string connectionString = "Data Source=" + defaultPath + ";Version=3;UseUTF16Encoding=True;";

            //返回SQLiteCommand
            return GetSQLiteCmd(connectionString);
        }
        /// <summary>
        /// 得到SQLCMD
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns></returns>
        private static SQLiteCommand GetSQLiteCmd(string connectionString)
        {
            try
            {
                //数据库连接
                SQLiteConnection conn = new SQLiteConnection(connectionString);

                //SQLiteCommand
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = conn;
                cmd.CommandTimeout = 90;

                //打开数据库连接
                cmd.Connection.Open();

                //返回SQLCMD
                return cmd;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查询
        /// <summary>
        /// 返回第一行，第一列
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns></returns>
        public static object GetScalar(SQLParam query, string returnColunm)
        {
            try
            {
                //参数检查
                if (query == null) throw new Exception("WsflyException:解析参数异常！");
                if (string.IsNullOrWhiteSpace(query.TableName)) throw new Exception("WsflyException:需要参数数据表名称！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //SQL
                string sql = @"select " + returnColunm + " from [" + query.TableName + @"] where $wsfly.where$";
                string where = string.Empty;

                //生成SQL及参数
                List<SQLiteParameter> ps = BuildQuerySQLAndParameters(query, ref sql, ref where);

                //查询数据
                return ExecuteScalar(cmd, sql, ps.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 返回第一行，第一列
        /// </summary>
        /// <param name="sql">查询SQL</param>
        /// <returns></returns>
        public static object GetScalar(string sql)
        {
            try
            {
                //参数检查
                if (string.IsNullOrWhiteSpace(sql)) throw new Exception("WsflyException:解析SQL异常！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //查询数据
                return ExecuteScalar(cmd, sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 查询数量
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns></returns>
        public static int GetCount(SQLParam query)
        {
            try
            {
                //参数记录数
                object objResult = GetScalar(query, "count(*)");

                //是否有查询结果
                return DataType.Int(objResult, 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns></returns>
        public static bool IsExist(SQLParam query)
        {
            try
            {
                return GetCount(query) > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        ///  查询数据行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataRow GetTableRowBySql(string sql)
        {
            DataTable dt = GetTableBySQL(sql);
            return dt == null || dt.Rows.Count <= 0 ? null : dt.Rows[0];
        }
        /// <summary>
        /// 查询数据行
        /// </summary>
        /// <returns></returns>
        public static DataRow GetTableRow(long id, string tbName)
        {
            string sql = string.Empty;

            try
            {
                //参数检查
                if (id <= 0) throw new Exception("WsflyException:需要参数数据Id主键！");
                if (string.IsNullOrWhiteSpace(tbName)) throw new Exception("WsflyException:需要参数数据表名称！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                sql = "select * from [" + tbName + "] where [Id]=@Id";
                SQLiteParameter[] ps =
                {
                    new SQLiteParameter("@Id",id)
                };

                //查询数据
                DataTable dt = QueryDataTable(cmd, sql, ps);

                //是否有查询结果
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回第一行
                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 查询数据行
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataRow GetTableRow(SQLParam query)
        {
            DataTable dt = GetTable(query);
            return dt == null || dt.Rows.Count <= 0 ? null : dt.Rows[0];
        }


        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetTableBySQL(string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");
                //返回数据表
                return QueryDataTable(cmd, sql, ps);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 数据表查询
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <param name="tbName">数据表名</param>
        /// <returns></returns>
        public static DataTable GetTable(SQLParam query)
        {
            string sql = string.Empty;

            try
            {
                //参数检查
                if (query == null) throw new Exception("WsflyException:解析参数异常！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //SQL
                sql = @"select * from [" + query.TableName + @"] 
                        where $wsfly.where$ 
                        order by $wsfly.order$" + (query.Top > 0 ? " limit " + query.Top : "");

                //查询条件
                string where = string.Empty;

                //生成查询SQL及参数
                List<SQLiteParameter> ps = BuildQuerySQLAndParameters(query, ref sql, ref where);

                //查询数据
                DataTable dt = QueryDataTable(cmd, sql, ps.ToArray());

                //是否有查询结果
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回数据表
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 数据表查询
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable GetTable(string tableName)
        {
            string sql = string.Empty;

            try
            {
                //参数检查
                if (string.IsNullOrWhiteSpace(tableName)) throw new Exception("WsflyException:解析参数异常！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //SQL
                sql = @"select * from [" + tableName + @"]";

                //查询数据
                DataTable dt = QueryDataTable(cmd, sql);

                //是否有查询结果
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回数据表
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 数据表查询
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static DataTable GetTable(string tableName, long parentId)
        {
            string sql = string.Empty;

            try
            {
                //参数检查
                if (string.IsNullOrWhiteSpace(tableName)) throw new Exception("WsflyException:解析参数异常！");

                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //SQL
                sql = @"select * from [" + tableName + @"] where [ParentId]=" + parentId;

                //查询数据
                DataTable dt = QueryDataTable(cmd, sql);

                //是否有查询结果
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回数据表
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 查询分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataTable GetPaging(SQLParam query, ref int totalCount)
        {
            string sql = string.Empty;

            //参数检查
            if (query == null) throw new Exception("WsflyException:解析参数异常！");

            //得到SQLCMD对象
            SQLiteCommand cmd = GetSQLiteCmd();
            if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

            try
            {
                //分页开始数量
                int beginCount = query.PageSize * (query.PageIndex - 1);

                //SQL
                sql = @"select * from [" + query.TableName + @"] 
                        where $wsfly.where$ 
                        order by $wsfly.order$
                        limit " + query.PageSize + @"
                        offset " + beginCount;

                //查询条件
                string where = string.Empty;

                //生成查询SQL及参数
                List<SQLiteParameter> ps = BuildQuerySQLAndParameters(query, ref sql, ref where);

                //查询数量
                totalCount = GetCount(query);

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                //命令：文本类型 & 执行SQL
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                if (ps != null && ps.Count > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //查询数据
                DataTable dt = new DataTable();
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                da.Fill(dt);
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回数据表
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }


        /// <summary>
        /// 查询数据集
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>DataSet</returns>
        private static DataSet QueryDataSet(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                //数据集
                DataSet ds = new DataSet();

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                //命令：文本类型 & 执行SQL
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //数据适配器 将查询出的数据填充到DataSet
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                da.Fill(ds);

                //返回
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }
        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <returns></returns>
        private static DataTable QueryDataTable(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                //数据表
                DataTable dt = new DataTable();

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                //命令：文本类型 & 执行SQL
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //数据适配器 将查询出的数据填充到DataSet
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                da.Fill(dt);

                //返回
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }
        #endregion

        #region 执行SQL
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        public static long Insert(SQLParam insert)
        {
            insert.Action = Actions.添加;
            List<SQLParam> paramList = new List<SQLParam>();
            paramList.Add(insert);
            ExecuteSQL(paramList);
            return paramList[0].ReturnId;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public static bool Update(SQLParam update)
        {
            update.Action = Actions.修改;
            List<SQLParam> paramList = new List<SQLParam>();
            paramList.Add(update);
            return ExecuteSQL(paramList);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        public static bool Delete(SQLParam delete)
        {
            delete.Action = Actions.删除;
            List<SQLParam> paramList = new List<SQLParam>();
            paramList.Add(delete);
            return ExecuteSQL(paramList);
        }
        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql"></param>
        public static bool ExecuteSQL(string sql)
        {
            return ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 执行SQL
        /// </summary>
        public static bool ExecuteSQL(string sql, SQLiteParameter[] ps)
        {
            //SQLiteCommand
            SQLiteCommand cmd = GetSQLiteCmd();
            return ExecuteNonQuery(cmd, sql, ps);
        }
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="paramList">SQL参数</param>
        /// <param name="dbname">数据库名</param>
        /// <returns></returns>
        public static bool ExecuteSQL(List<SQLParam> paramList)
        {
            //是否有SQL参数
            if (paramList == null || paramList.Count <= 0) throw new Exception("WsflyException:需要SQL参数！");

            //得到SQLCMD对象
            SQLiteCommand cmd = GetSQLiteCmd();
            if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

            try
            {
                foreach (SQLParam param in paramList)
                {
                    //生成SQL
                    string where = string.Empty;
                    string sql = string.Empty;

                    //SQL参数
                    List<SQLiteParameter> ps = new List<SQLiteParameter>();

                    if (param.Action == Actions.添加)
                    {
                        //生成插入参数及SQL
                        sql = "insert into [" + param.TableName + "]($wsfly.cells$) values($wsfly.values$)";
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("Id")))
                        {
                            param.OpreateCells.Insert(0, new KeyValue("Id", null));
                        }
                        ps = BuildInsertSQLAndParameters(param.OpreateCells, ref sql);
                    }
                    else if (param.Action == Actions.修改)
                    {
                        //生成更新参数及SQL
                        sql = "update [" + param.TableName + "] set $wsfly.cells.values$ $wsfly.where$";
                        if (!string.IsNullOrWhiteSpace(param.Ids)) param.Wheres.Add(new Where("Id", param.Ids, WhereType.包含));
                        ps = BuildUpdateSQLAndParameters(param.OpreateCells, param.Id, param.Wheres, ref sql, ref where);
                    }
                    else if (param.Action == Actions.删除)
                    {
                        //生成删除参数及SQL
                        sql = "delete from [" + param.TableName + "] $wsfly.where$";
                        ps = BuildDeleteSQLAndParameters(param.Id, param.Ids, param.Wheres, ref sql, ref where);
                    }
                    else if (param.Action == Actions.SQL)
                    {
                        //执行SQL
                        sql = param.ExecSQL;
                    }

                    //是否有条件
                    if (!string.IsNullOrWhiteSpace(param.WhereSQL))
                    {
                        if (!string.IsNullOrWhiteSpace(where)) sql += " and " + param.WhereSQL;
                        else sql += " where " + param.WhereSQL;
                    }

                    //SQLiteCommand为SQL脚本模式
                    cmd.CommandType = CommandType.Text;

                    //执行SQL
                    if (param.Action == Actions.添加)
                    {
                        //执行添加
                        param.ReturnSql = sql;

                        object objResult = ExecuteScalar(cmd, sql, ps.ToArray());
                        long newId = DataType.Long(objResult, 0);

                        //返回Id
                        param.ReturnId = newId;

                        if (newId <= 0) throw new Exception("执行(" + param.Action.ToString() + ")失败，SQL：" + sql);
                    }
                    else
                    {
                        //执行修改、删除
                        param.ReturnSql = sql;
                        bool flag = ExecuteNonQuery(cmd, sql, ps.ToArray());
                        if (!flag) throw new Exception("执行(" + param.Action.ToString() + ")失败，SQL：" + sql);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "执行SQL异常");
            }

            return false;
        }
        /// <summary>
        /// 执行非查询语句带参数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static bool ExecuteNonQuery(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.Clear();

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                //遍历参数
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //是否有影响行数
                if (cmd.ExecuteNonQuery() <= 0) return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }
        /// <summary>
        /// 执行非查询语句带参数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static bool ExecuteNonQuery(string sql, SQLiteParameter[] ps = null)
        {
            //得到SQLCMD对象
            SQLiteCommand cmd = GetSQLiteCmd();
            if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.Clear();

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                //遍历参数
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //是否有影响行数
                if (cmd.ExecuteNonQuery() <= 0) return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }

        /// <summary>
        /// 执行查询，返回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                //设置SQL及参数
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                if (sql.ToLower().StartsWith("insert into"))
                {
                    //插入SQL
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select last_insert_rowid()";
                    return cmd.ExecuteScalar();
                }
                else
                {
                    //非插入SQL
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }
        /// <summary>
        /// 执行查询，返回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, SQLiteParameter[] ps = null)
        {
            //得到SQLCMD对象
            SQLiteCommand cmd = GetSQLiteCmd();
            if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

            try
            {
                //设置SQL及参数
                cmd.CommandText = sql;
                cmd.Parameters.Clear();
                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //是否打开数据库连接
                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();

                if (sql.ToLower().StartsWith("insert into"))
                {
                    //插入SQL
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select last_insert_rowid()";
                    return cmd.ExecuteScalar();
                }
                else
                {
                    //非插入SQL
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State == ConnectionState.Open) cmd.Connection.Close();
            }
        }
        #endregion

        #region 事务执行SQL
        /// <summary>
        /// 执行事务：执行SQL
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static bool TranExecute(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    //打开数据库连接
                    cmd.Connection.Open();
                }

                if (cmd.Transaction == null)
                {
                    //执行事务
                    SQLiteTransaction tran = cmd.Connection.BeginTransaction();
                    cmd.Transaction = tran;
                }

                //执行SQL的参数
                if (cmd.Parameters != null)
                {
                    //清空参数
                    cmd.Parameters.Clear();
                }

                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //执行的SQL
                cmd.CommandText = sql;

                //执行SQL
                if (cmd.ExecuteNonQuery() <= 0) return false;

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 执行事务查询：返回第一行第一列数据
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static object TranExecuteScalar(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    //打开数据库连接
                    cmd.Connection.Open();
                }

                if (cmd.Transaction == null)
                {
                    //执行事务
                    SQLiteTransaction tran = cmd.Connection.BeginTransaction();
                    cmd.Transaction = tran;
                }

                //执行SQL的参数
                if (cmd.Parameters != null)
                {
                    //清空参数
                    cmd.Parameters.Clear();
                }

                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //执行的SQL
                cmd.CommandText = sql;

                if (sql.ToLower().Trim().StartsWith("insert into"))
                {
                    //插入SQL
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select last_insert_rowid()";
                    return cmd.ExecuteScalar();
                }
                else
                {
                    //执行SQL 返回第一行第一列
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 执行事务查询：返回DataRow
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DataRow TranQueryRow(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                //查询数据表
                DataTable dt = TranQueryTable(cmd, sql, ps);
                if (dt == null || dt.Rows.Count <= 0) return null;

                //返回第一行
                return dt.Rows[0];
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 执行事务查询：返回DataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DataTable TranQueryTable(SQLiteCommand cmd, string sql, SQLiteParameter[] ps = null)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    //打开数据库连接
                    cmd.Connection.Open();
                }

                if (cmd.Transaction == null)
                {
                    //执行事务
                    SQLiteTransaction tran = cmd.Connection.BeginTransaction();
                    cmd.Transaction = tran;
                }

                //执行SQL的参数
                if (cmd.Parameters != null)
                {
                    //清空参数
                    cmd.Parameters.Clear();
                }

                if (ps != null && ps.Length > 0)
                {
                    foreach (SQLiteParameter p in ps)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                //执行的SQL
                cmd.CommandText = sql;

                //要填充的数据表
                DataTable dt = new DataTable();

                //DataAdapter填充数据
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                //填充数据
                da.Fill(dt);
                //返回结果
                return dt;
            }
            catch (Exception ex) { }

            return null;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public static void TranCommit(SQLiteCommand cmd)
        {
            try
            {
                //是否有命令及事务
                if (cmd == null || cmd.Connection.State == ConnectionState.Closed || cmd.Transaction == null) return;
                //提交事务
                cmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public static void TranRollback(SQLiteCommand cmd)
        {
            try
            {
                //是否有命令及事务
                if (cmd == null || cmd.Connection.State == ConnectionState.Closed || cmd.Transaction == null) return;
                //回滚事务
                cmd.Transaction.Rollback();
            }
            catch (Exception ex)
            {
                string sql = cmd == null ? "" : string.IsNullOrWhiteSpace(cmd.CommandText) ? "" : cmd.CommandText;
                AppLog.WriteBugLog(ex, "回滚事务异常,执行脚本：" + sql);
            }
            finally
            {
                //关闭数据库连接
                if (cmd.Connection.State != ConnectionState.Closed)
                {
                    cmd.Connection.Close();
                }
            }
        }
        #endregion

        #region 生成SQL
        /// <summary>
        /// 生成查询SQL&参数
        /// </summary>
        /// <param name="param">查询参数</param>
        /// <param name="sql"></param>
        /// <param name="whereRef">条件SQL</param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildQuerySQLAndParameters(SQLParam param, ref string sql, ref string whereRef)
        {
            //参数
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //查询条件
            string where = "1=1";
            if (param.Wheres != null && param.Wheres.Count > 0)
            {
                BuildWhere(param.Wheres, ref where, ref ps);
            }

            //过滤SQL
            if (!string.IsNullOrWhiteSpace(param.WhereSQL))
            {
                where += " and " + param.WhereSQL;
            }

            //排序SQL
            string order = "1";
            if (param.OrderBys != null && param.OrderBys.Count > 0)
            {
                //清空排序
                order = "";

                //生成排序
                foreach (OrderBy ob in param.OrderBys)
                {
                    //随机排序
                    if (ob.Type == OrderType.随机) { order += " NEWID()"; continue; }

                    //排序条件
                    order += "[" + ob.CellName + "] " + ob.TypeSQL + ",";
                }

                //清除最后“,”
                order = order.Trim(',').Trim();
            }

            //排序SQL
            if (!string.IsNullOrWhiteSpace(param.OrderSQL))
            {
                if (order.Equals("1")) order = param.OrderSQL;
                else if (!string.IsNullOrWhiteSpace(order)) order += "," + param.OrderSQL;
                else order = param.OrderSQL;
            }

            //SQL替换
            sql = sql.Replace("$wsfly.where$", where);
            sql = sql.Replace("$wsfly.order$", order);

            //返回查询条件
            whereRef = where;

            return ps;
        }
        /// <summary>
        /// 生成插入SQL&参数
        /// </summary>
        /// <param name="insertCells">插入参数</param>
        /// <param name="sql">要执行的SQL</param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildInsertSQLAndParameters(List<KeyValue> insertCells, ref string sql)
        {
            string cells = string.Empty;
            string values = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            foreach (KeyValue kv in insertCells)
            {
                //非ID列 过滤NULL值
                if (!kv.Key.ToUpper().Equals("ID"))
                {
                    //空
                    if (kv.Value == null) continue;
                }

                //SQL
                cells += "[" + kv.Key + "],";
                values += "@" + kv.Key + ",";

                //是否需要加密
                kv.Value = IsNeedEncrypt(kv.Value);

                //参数值
                SQLiteParameter param = new SQLiteParameter() { ParameterName = "@" + kv.Key, Value = kv.Value };
                ps.Add(param);
            }

            //SQL
            cells = cells.Trim(',').Trim();
            values = values.Trim(',').Trim();

            //SQL替换
            sql = sql.Replace("$wsfly.cells$", cells);
            sql = sql.Replace("$wsfly.values$", values);

            return ps;
        }
        /// <summary>
        /// 生成插入SQL&参数
        /// </summary>
        /// <param name="row"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildInsertSQLAndParameters(DataRow row, ref string sql)
        {
            string cells = string.Empty;
            string values = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            foreach (DataColumn col in row.Table.Columns)
            {
                string colName = col.ColumnName;
                object value = row[colName];

                //过滤列插入
                if (colName.ToUpper().Equals("ID")) continue;

                //空
                if (value == null || value == DBNull.Value) continue;

                //列
                cells += "[" + colName + "],";

                //值
                values += "@" + colName + ",";

                //是否需要加密
                value = IsNeedEncrypt(value);

                //参数
                ps.Add(new SQLiteParameter() { ParameterName = "@" + colName, Value = value });
            }

            //SQL
            cells = cells.Trim(',').Trim();
            values = values.Trim(',').Trim();

            //SQL替换
            sql = sql.Replace("$wsfly.cells$", cells);
            sql = sql.Replace("$wsfly.values$", values);

            return ps;
        }
        /// <summary>
        /// 生成更新SQL&参数
        /// </summary>
        /// <param name="updateCells">更新参数</param>
        /// <param name="wheres">查询条件</param>
        /// <param name="sql">要执行的SQL</param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildUpdateSQLAndParameters(List<KeyValue> updateCells, long id, List<Where> wheres, ref string sql, ref string whereStr)
        {
            string where = string.Empty;
            string cellsValues = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            foreach (KeyValue kv in updateCells)
            {
                //过滤列
                if (kv.Key.ToUpper().Equals("ID") || kv.Key.ToUpper().Equals("CREATEDATE")) continue;

                if (kv.Value == null)
                {
                    //空
                    cellsValues += "[" + kv.Key + "]=null,";
                    continue;
                }

                //SQL
                cellsValues += "[" + kv.Key + "]=@" + kv.Key + ",";

                //参数值
                ps.Add(new SQLiteParameter() { ParameterName = "@" + kv.Key, Value = kv.Value });
            }


            if (id > 0)
            {
                //根据主键修改
                where = " [Id]=@Id";
                SQLiteParameter paramId = new SQLiteParameter() { ParameterName = "@Id", Value = id };
                ps.Add(paramId);
            }
            else if (wheres != null && wheres.Count > 0)
            {
                //根据条件修改
                BuildWhere(wheres, ref where, ref ps);
            }

            //SQL
            cellsValues = cellsValues.Trim(',').Trim();

            //是否有条件
            if (!string.IsNullOrWhiteSpace(where)) where = " where " + where;

            //判断条件
            whereStr = where;

            //SQL替换
            sql = sql.Replace("$wsfly.cells.values$", cellsValues);
            sql = sql.Replace("$wsfly.where$", where);

            return ps;
        }
        /// <summary>
        /// 生成更新参数
        /// </summary>
        /// <param name="updateCells"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildUpdateParameters(List<KeyValue> updateCells, ref string sql)
        {
            //参数列表
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            foreach (KeyValue kv in updateCells)
            {
                //过滤列
                if (kv.Key.ToUpper().Equals("ID") || kv.Key.ToUpper().Equals("CREATEDATE")) continue;

                if (kv.Value == null)
                {
                    //空
                    sql += "[" + kv.Key + "]=null,";
                    continue;
                }

                //SQL
                sql += "[" + kv.Key + "]=@" + kv.Key + ",";

                //参数值
                ps.Add(new SQLiteParameter() { ParameterName = "@" + kv.Key, Value = kv.Value });
            }

            return ps;
        }
        /// <summary>
        /// 生成删除SQL&参数
        /// </summary>
        /// <param name="id">要删除的Id主键</param>
        /// <param name="ids">要删除的Id列表</param>
        /// <param name="wheres">查询条件</param>
        /// <param name="sql">要执行的SQL</param>
        /// <param name="whereStr">条件SQL</param>
        /// <returns></returns>
        public static List<SQLiteParameter> BuildDeleteSQLAndParameters(long id, string ids, List<Where> wheres, ref string sql, ref string whereStr)
        {
            string where = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //生成删除条件
            if (id > 0)
            {
                where = "[Id]=@Id";
                ps.Add(new SQLiteParameter() { ParameterName = "@Id", Value = id });
            }
            else if (!string.IsNullOrWhiteSpace(ids))
            {
                where = string.Format("[Id] in ({0})", ids);
            }
            else if (wheres != null && wheres.Count > 0)
            {
                BuildWhere(wheres, ref where, ref ps);
            }

            //是否有条件
            if (!string.IsNullOrWhiteSpace(where)) where = " where " + where;

            //SQL替换
            sql = sql.Replace("$wsfly.where$", where);

            //删除条件
            whereStr = where;

            return ps;
        }
        /// <summary>
        /// 生成查询条件
        /// </summary>
        /// <param name="wheres">查询条件</param>
        /// <param name="where">条件SQL</param>
        /// <param name="ps">参数</param>
        public static void BuildWhere(List<Where> wheres, ref string where, ref List<SQLiteParameter> ps)
        {
            //是否有条件
            if (wheres == null || wheres.Count <= 0) return;

            //清空条件
            where = "1=1";
            //是否有左括号
            int leftBracketCount = 0;
            bool isLeftBracketFirst = false;

            foreach (Where w in wheres)
            {
                //右括号
                if (w.Type == WhereType.右括号 && leftBracketCount > 0)
                {
                    where += " ) ";
                    leftBracketCount--;
                    continue;
                }

                //查询条件
                if (leftBracketCount > 0 && isLeftBracketFirst)
                {
                    //前一个为括号，不加并列条件
                    isLeftBracketFirst = false;
                }
                else
                {
                    //And 或 Or
                    where += " " + w.Parallel.ToString() + " ";
                }

                //左括号
                if (w.Type == WhereType.左括号)
                {
                    where += " ( ";
                    leftBracketCount++;
                    isLeftBracketFirst = true;
                    continue;
                }

                //参数索引 避免重复
                int parameterIndex = wheres.IndexOf(w);

                //查询参数
                SQLiteParameter p = new SQLiteParameter() { ParameterName = "@" + w.CellName + parameterIndex, Value = w.CellValue };

                switch (w.Type)
                {
                    case WhereType.模糊查询:
                        p.Value = "%" + w.CellValue + "%";
                        break;
                    case WhereType.模糊前:
                        p.Value = "%" + w.CellValue;
                        break;
                    case WhereType.模糊后:
                        p.Value = w.CellValue + "%";
                        break;
                    case WhereType.包含:
                    case WhereType.不包含:
                        where += string.Format(w.WhereSQL, w.CellValue.ToString().Trim(','));
                        continue;
                    case WhereType.空:
                    case WhereType.非空:
                        where += w.WhereSQL;
                        continue;
                }

                //添加条件
                where += w.WhereSQL + parameterIndex;

                //添加参数
                ps.Add(p);
            }
        }
        /// <summary>
        /// 生成排序SQL
        /// </summary>
        /// <param name="orderBys"></param>
        /// <param name="orderBy"></param>
        public static void BuildOrderBy(List<OrderBy> orderBys, ref string orderBy)
        {
            //排序SQL
            string order = "1";
            if (orderBys != null && orderBys.Count > 0)
            {
                //清空排序
                order = "";

                foreach (OrderBy ob in orderBys)
                {
                    //随机排序
                    if (ob.Type == OrderType.随机) { order += " NEWID()"; continue; }

                    //排序条件
                    order += "[" + ob.CellName + "] " + ob.TypeSQL + ",";
                }

                //清除最后“,”
                order = order.Trim(',').Trim();
            }

            orderBy = order;
        }
        /// <summary>
        /// 是否需要加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object IsNeedEncrypt(object value)
        {
            if (value is string)
            {
                //值
                string val = value.ToString();

                //是否需要加密
                if (val.StartsWith("WsEncoding_"))
                {
                    val = val.Replace("WsEncoding_", "");
                    val = EncryptionDES.Decrypt(val);
                    return "WsEncrypt_" + val;
                }
            }

            return value;
        }
        #endregion

        #region 序列号相关
        static object _lockSerialNo = new object();
        /// <summary>
        /// 序列号
        /// </summary>
        /// <returns></returns>
        public static string GetSerialNo(long tableId, long relativeId, SerialNoType subType, int serialNoLength = 4, string prefix = "")
        {
            lock (_lockSerialNo)
            {
                string sql = "select * from [Sys_SerialNos] where [TableId]=" + tableId + " and [RelativeId]=" + relativeId + " and [SubType]=" + (int)subType;
                DataRow rowSerialNo = GetTableRowBySql(sql);

                long num = 1;
                string relativeNo = "";
                string currentDateValue = "";

                switch (subType)
                {
                    case SerialNoType.年号:
                        relativeNo = DateTime.Now.ToString("yyyy");
                        currentDateValue = DateTime.Now.ToString("yyyy");
                        break;
                    case SerialNoType.月号:
                        relativeNo = DateTime.Now.ToString("yyyyMM");
                        currentDateValue = DateTime.Now.ToString("yyyyMM");
                        break;
                    case SerialNoType.日号:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyyMMdd");
                        break;
                    case SerialNoType.年全日期:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyy");
                        break;
                    case SerialNoType.月全日期:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyyMM");
                        break;
                }

                if (rowSerialNo == null)
                {
                    //创建
                    Insert(new SQLParam()
                    {
                        TableName = "Sys_SerialNos",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("TableId", tableId),
                            new KeyValue("RelativeId", relativeId),
                            new KeyValue("SubType", (int)subType),
                            new KeyValue("Type", 0),
                            new KeyValue("Length", serialNoLength),
                            new KeyValue("Prefix", prefix),
                            new KeyValue("DateValue", currentDateValue),
                            new KeyValue("SerialValue", num),
                            new KeyValue("LastBuildDate", DateTime.Now),
                        }
                    });
                }
                else
                {
                    //号码加1
                    num = rowSerialNo.GetLong("SerialValue");
                    num++;

                    //上一个号的日期
                    string dateValue = rowSerialNo.GetString("DateValue");

                    //日期号时需要判断是否重置
                    if (subType == SerialNoType.年号 || subType == SerialNoType.月号 || subType == SerialNoType.日号 || subType == SerialNoType.年全日期 || subType == SerialNoType.月全日期)
                    {
                        if (dateValue != currentDateValue)
                        {
                            num = 1;
                        }
                    }

                    //更新序列号表
                    Update(new SQLParam()
                    {
                        TableName = "Sys_SerialNos",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("SerialValue", num),
                            new KeyValue("DateValue", currentDateValue),
                            new KeyValue("Length", serialNoLength),
                            new KeyValue("Prefix", prefix),
                            new KeyValue("LastBuildDate", DateTime.Now),
                        },
                        Id = rowSerialNo.GetId()
                    });
                }

                string prefixStr = (string.IsNullOrWhiteSpace(prefix) ? "" : prefix + "-");
                string numString = num.ToString().PadLeft(serialNoLength, '0');

                //流水号
                if (subType == SerialNoType.流水号)
                {
                    if (relativeId > 0)
                    {
                        relativeNo = relativeId.ToString();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(prefix))
                        {
                            int beginNum = 1;
                            for (var i = 1; i < serialNoLength; i++) beginNum *= 10;
                            beginNum += DataType.Int(numString, 1);

                            numString = beginNum.ToString();
                        }
                    }
                }

                relativeNo = (string.IsNullOrWhiteSpace(relativeNo) ? "" : relativeNo + "-");
                return prefixStr + relativeNo + numString;
            }
        }
        /// <summary>
        /// 序列号
        /// </summary>
        /// <returns></returns>
        public static string TranGetSerialNo(SQLiteCommand cmd, long tableId, long relativeId, SerialNoType subType, int serialNoLength = 4, string prefix = "")
        {
            lock (_lockSerialNo)
            {
                string sql = "select * from [Sys_SerialNos] where [TableId]=" + tableId + " and [RelativeId]=" + relativeId + " and [SubType]=" + (int)subType;
                DataRow rowSerialNo = TranQueryRow(cmd, sql);

                long num = 1;
                string relativeNo = "";
                string currentDateValue = "";

                switch (subType)
                {
                    case SerialNoType.年号:
                        relativeNo = DateTime.Now.ToString("yyyy");
                        currentDateValue = DateTime.Now.ToString("yyyy");
                        break;
                    case SerialNoType.月号:
                        relativeNo = DateTime.Now.ToString("yyyyMM");
                        currentDateValue = DateTime.Now.ToString("yyyyMM");
                        break;
                    case SerialNoType.日号:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyyMMdd");
                        break;
                    case SerialNoType.年全日期:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyy");
                        break;
                    case SerialNoType.月全日期:
                        relativeNo = DateTime.Now.ToString("yyyyMMdd");
                        currentDateValue = DateTime.Now.ToString("yyyyMM");
                        break;
                }

                if (rowSerialNo == null)
                {
                    //创建
                    sql = @"insert into [Sys_SerialNos]([TableId],[RelativeId],[SubType],[Type],[Length],[Prefix],[DateValue],[SerialValue],[LastBuildDate]) 
                                  values(@TableId,@RelativeId,@SubType,@Type,@Length,@Prefix,@DateValue,@SerialValue,@LastBuildDate)";

                    SQLiteParameter[] ps = {
                        new SQLiteParameter() { ParameterName="@TableId", Value=tableId },
                        new SQLiteParameter() { ParameterName="@RelativeId", Value=relativeId },
                        new SQLiteParameter() { ParameterName="@SubType", Value=(int)subType },
                        new SQLiteParameter() { ParameterName="@Type", Value=0 },
                        new SQLiteParameter() { ParameterName="@Length", Value=serialNoLength },
                        new SQLiteParameter() { ParameterName="@Prefix", Value=prefix },
                        new SQLiteParameter() { ParameterName="@DateValue", Value=currentDateValue },
                        new SQLiteParameter() { ParameterName="@SerialValue", Value=num },
                        new SQLiteParameter() { ParameterName="@LastBuildDate", Value=DateTime.Now },
                    };

                    TranExecute(cmd, sql, ps);
                }
                else
                {
                    //号码加1
                    num = rowSerialNo.GetLong("SerialValue");
                    num++;

                    //上一个号的日期
                    string dateValue = rowSerialNo.GetString("DateValue");

                    //日期号时需要判断是否重置
                    if (subType == SerialNoType.年号 || subType == SerialNoType.月号 || subType == SerialNoType.日号 || subType == SerialNoType.年全日期 || subType == SerialNoType.月全日期)
                    {
                        if (dateValue != currentDateValue)
                        {
                            num = 1;
                        }
                    }

                    //更新序列号表
                    sql = "update [Sys_SerialNos] set [SerialValue]=@SerialValue,[DateValue]=@DateValue,[Length]=@Length,[Prefix]=@Prefix,[LastBuildDate]=@LastBuildDate where Id=" + rowSerialNo.GetId();

                    SQLiteParameter[] ps = {
                        new SQLiteParameter() { ParameterName="@SerialValue", Value=num },
                        new SQLiteParameter() { ParameterName="@DateValue", Value=currentDateValue },
                        new SQLiteParameter() { ParameterName="@Length", Value=serialNoLength },
                        new SQLiteParameter() { ParameterName="@Prefix", Value=prefix },
                        new SQLiteParameter() { ParameterName="@LastBuildDate", Value=DateTime.Now },
                    };

                    TranExecute(cmd, sql, ps);
                }

                string prefixStr = (string.IsNullOrWhiteSpace(prefix) ? "" : prefix + "-");
                string numString = num.ToString().PadLeft(serialNoLength, '0');

                //流水号
                if (subType == SerialNoType.流水号)
                {
                    if (relativeId > 0)
                    {
                        relativeNo = relativeId.ToString();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(prefix))
                        {
                            int beginNum = 1;
                            for (var i = 1; i < serialNoLength; i++) beginNum *= 10;
                            beginNum += DataType.Int(numString, 1);

                            numString = beginNum.ToString();
                        }
                    }
                }

                relativeNo = (string.IsNullOrWhiteSpace(relativeNo) ? "" : relativeNo + "-");
                return prefixStr + relativeNo + numString;
            }
        }
        /// <summary>
        /// 序列号类型
        /// </summary>
        public enum SerialNoType
        {
            /// <summary>
            /// 一直向上增
            /// </summary>
            流水号 = 0,
            /// <summary>
            /// 每年重置
            /// 格式：yyyy-0001
            /// </summary>
            年号 = 1,
            /// <summary>
            /// 每月重置
            /// 格式：yyyyMM-0001
            /// </summary>
            月号 = 2,
            /// <summary>
            /// 每日重置
            /// 格式：yyyyMMdd-0001
            /// </summary>
            日号 = 3,

            /// <summary>
            /// 每年重置
            /// 格式：yyyyMMdd-0001
            /// </summary>
            年全日期 = 4,
            /// <summary>
            /// 每月重置
            /// 格式：yyyyMMdd-0001
            /// </summary>
            月全日期 = 5
        }
        #endregion

        #region 数据表
        /*
            不存在则插入行
            insert into Users(UserID,UserName)
            select '001','张三'  where not exists(select 1 from Users where UserID = '001')

            不存在表就创建
            create table if not exists Users(
                AutoID integer primary key autoincrement,  
                UserID varhcar(32) default '',  
                UserName varchar(64) default '' 
            )
        */
        /// <summary>
        /// 删除表
        /// </summary>
        public static void DropTable(string tableName)
        {
            try
            {
                string sql = "drop table if exists [" + tableName + "]";
                ExecuteSQL(sql);
            }
            catch { }
        }
        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <returns></returns>
        public static bool TableIsExists(string tableName)
        {
            try
            {
                string sql = "select count(*) from [sqlite_master] where [type]='table' AND [name]='" + tableName + @"';";
                object obj = GetScalar(sql);
                if (obj != null && DataType.Int(obj, 0) > 0) return true;
            }
            catch (Exception ex) { }

            return false;
        }
        /// <summary>
        /// 表重命名
        /// </summary>
        /// <returns></returns>
        public static bool RenameTable(string orgTableName, string newTableName)
        {
            try
            {
                string sql = "ALTER TABLE '" + orgTableName + "' RENAME TO '" + newTableName + "';";
                return ExecuteSQL(sql);
            }
            catch (Exception ex) { }

            return false;
        }
        /// <summary>
        /// 获取创建表的SQL
        /// </summary>
        /// <returns></returns>
        public static string GetBuildTableSQL(string tableName)
        {
            try
            {
                string sql = "SELECT * FROM [sqlite_master] WHERE type='table' and name='" + tableName + "'";
                DataRow row = GetTableRowBySql(sql);
                if (row != null) return row["sql"].ToString();
            }
            catch (Exception ex) { }

            return string.Empty;
        }
        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable GetTableStruct(string tableName)
        {
            try
            {
                string sql = "SELECT * FROM [" + tableName + "] limit 1";
                DataTable dt = GetTableBySQL(sql);
                if (dt.Rows.Count > 0) dt.Rows.RemoveAt(0);
                return dt;
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 列是否存在
        /// </summary>
        /// <returns></returns>
        public static bool TableColumnIsExists(string tableName, string columnName)
        {
            try
            {
                //得到SQLCMD对象
                SQLiteCommand cmd = GetSQLiteCmd();
                if (cmd == null) throw new Exception("WsflyException:未找到数据库连接！");

                //查询数据
                string sql = "select * from [" + tableName + "] limit 0;";
                DataTable dt = QueryDataTable(cmd, sql);

                return dt.Columns.Contains(columnName);
            }
            catch (Exception ex) { }

            return false;
        }
        /// <summary>
        /// 添加列
        /// </summary>
        /// <returns></returns>
        public static bool AddColumn(string tableName, string columnName, string type = "varchar")
        {
            try
            {
                bool isExists = TableColumnIsExists(tableName, columnName);
                if (isExists) return true;

                string sql = "alter table [" + tableName + "] add column [" + columnName + "] " + type + ";";
                ExecuteSQL(sql);

                return true;
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion

        #region 年月日计数
        /// <summary>
        /// 检查年月日计数表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool CheckYMDTable(string tableName)
        {
            try
            {
                string sql = "select count(*) from [sqlite_master] where [type]='table' AND [name]='" + tableName + @"';";
                object obj = GetScalar(sql);
                if (obj != null && DataType.Int(obj, 0) > 0) return true;


                sql = @"
CREATE TABLE [" + tableName + @"] (
	Id   INTEGER  PRIMARY KEY UNIQUE,
	[Y]   INT, [M]   INT,
	[D1] INT,[D2] INT,[D3] INT,[D4] INT,[D5] INT,[D6] INT,[D7] INT,[D8] INT,[D9] INT,[D10] INT,
	[D11] INT,[D12] INT,[D13] INT,[D14] INT,[D15] INT,[D16] INT,[D17] INT,[D18] INT,[D19] INT,[D20] INT,
	[D21] INT,[D22] INT,[D23] INT,[D24] INT,[D25] INT,[D26] INT,[D27] INT,[D28] INT,[D29] INT,[D30] INT,[D31] INT
);
";
                ExecuteSQL(sql);
                return true;
            }
            catch (Exception ex) { }

            return false;
        }
        /// <summary>
        /// 年月日计数表递增
        /// </summary>
        /// <param name="date"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static bool YMDTableIncrement(string tableName, DateTime date, int increment = 1)
        {
            try
            {
                int year = date.Year;
                int month = date.Month;
                int day = date.Day;

                string sql = "select count(*) from [" + tableName + "] where [Y]=" + year + " and [M]=" + month;
                object obj = GetScalar(sql);

                sql = "";
                if (obj == null || DataType.Int(obj, 0) < 1)
                {
                    sql += @"
insert into [" + tableName + @"]([Y],[M],[D1],[D2],[D3],[D4],[D5],[D6],[D7],[D8],[D9],[D10],[D11],[D12],[D13],[D14],[D15],[D16],[D17],[D18],[D19],[D20],[D21],[D22],[D23],[D24],[D25],[D26],[D27],[D28],[D29],[D30],[D31])
values(" + year + @"," + month + @",0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
";
                }

                sql += @"update [" + tableName + @"] set [D" + day + @"]=[D" + day + @"]+" + increment + @" where [Y]=" + year + @" and [M]=" + month + ";";

                return ExecuteSQL(sql);
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion

        #region 扩展
        /// <summary>
        /// 字段值递增
        /// </summary>
        /// <returns></returns>
        public static bool Incremental(string tableName, string colName, long id, int increment = 1)
        {
            try
            {
                string sql = @"update [" + tableName + "] set [" + colName + "]=[" + colName + "]+1 where Id=" + id;
                return ExecuteSQL(sql);
            }
            catch { }

            return false;
        }
        #endregion
    }
}
