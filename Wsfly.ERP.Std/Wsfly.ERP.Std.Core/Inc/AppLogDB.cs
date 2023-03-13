using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;

/// <summary>
/// 日志助手
/// </summary>
public class AppLogDB
{
    /// <summary>
    /// 写流水日志
    /// </summary>
    public static void AddJournaliseLog(string title, string type = "")
    {
        AddLog(TableType.Journalises, title, type);
    }
    /// <summary>
    /// 写异常日志
    /// </summary>
    public static void AddBugLog(Exception ex, string title = "", string type = "")
    {
        long id = AddLog(TableType.Errors, title, type, 0, ex.Message, ex.Source, ex.StackTrace);

        if (id > 0)
        {
            //递归子异常
            RecursionInnerException(ex, type, id);
        }
    }
    /// <summary>
    /// 递归列出子异常
    /// </summary>
    private static void RecursionInnerException(Exception ex, string type, long parentId)
    {
        if (ex.InnerException != null)
        {
            long id = AddLog(TableType.Errors, "", type, parentId, ex.Message, ex.Source, ex.StackTrace);
            RecursionInnerException(ex.InnerException, type, id);
        }
    }
    /// <summary>
    /// 写调试日志
    /// </summary>
    public static void AddDebugLog(string title, string type = "")
    {
        AddLog(TableType.Debugs, title, type);
    }
    /// <summary>
    /// 写错误日志
    /// </summary>
    public static void AddErrorLog(string title, string type = "")
    {
        AddLog(TableType.Errors, title, type);
    }
    /// <summary>
    /// 写操作日志
    /// </summary>
    public static void AddOperateLog(string title, string type = "")
    {
        AddLog(TableType.Operates, title, type);
    }
    /// <summary>
    /// 写系统日志
    /// </summary>
    public static void AddSysLog(string title, string type = "")
    {
        AddLog(TableType.Logs, title, type);
    }

    #region SQLite
    /// <summary>
    /// SQLite命令
    /// </summary>
    static SQLiteCommand _cmd = null;
    /// <summary>
    /// 验证数据文件
    /// </summary>
    private static bool CheckLogDB()
    {
        try
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "AppLog\\";
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

            string dbFileName = path + "logs.db";
            if (!System.IO.File.Exists(dbFileName))
            {
                //创建数据库
                SQLiteConnection.CreateFile(dbFileName);

                //生成表
                BuildLogTable(TableType.Logs);
                BuildLogTable(TableType.Journalises);
                BuildLogTable(TableType.Debugs);
                BuildLogTable(TableType.Errors);
                BuildLogTable(TableType.Operates);
            }

            return true;
        }
        catch (Exception ex) { }

        return false;
    }
    /// <summary>
    /// 日志表
    /// </summary>
    /// <param name="tableName"></param>
    private static void BuildLogTable(TableType type)
    {
        string tableName = type.ToString();

        string sql = @"CREATE TABLE [" + tableName + @"] (
                                        [Id]         INTEGER  PRIMARY KEY AUTOINCREMENT,
                                        [ParentId]       INTEGER,
                                        [Type]             VARCHAR,
                                        [Title]              VARCHAR,
                                        [Message]       VARCHAR,
                                        [Source]          VARCHAR,
                                        [StackTrace]    VARCHAR,
                                        [CreateDate]   DATETIME
                                    );";

        ExecuteSQL(sql);
    }
    /// <summary>
    /// 添加日志
    /// </summary>
    private static long AddLog(TableType tableType, string Title, string Type, long ParentId = 0, string Message = "", string Source = "", string StackTrace = "")
    {
        try
        {
            string tableName = tableType.ToString();
            string sql = @"insert into [" + tableName + @"]([ParentId],[Type],[Title],[Message],[Source],[StackTrace],[CreateDate]) 
                                                                               values(@ParentId,@Type,@Title,@Message,@Source,@StackTrace,@CreateDate)";
            SQLiteParameter[] ps = {
                new SQLiteParameter() { ParameterName="@ParentId", Value=ParentId },
                new SQLiteParameter() { ParameterName="@Title", Value=Title },
                new SQLiteParameter() { ParameterName="@Type", Value=Type },
                new SQLiteParameter() { ParameterName="@Message", Value=Message },
                new SQLiteParameter() { ParameterName="@Source", Value=Source },
                new SQLiteParameter() { ParameterName="@StackTrace", Value=StackTrace },
                new SQLiteParameter() { ParameterName="@CreateDate", Value=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            };

            if (ExecuteSQL(sql, ps))
            {
                _cmd.CommandText = "select last_insert_rowid()";
                var last_insert_rowid = _cmd.ExecuteScalar();
                return Convert.ToInt64(last_insert_rowid);
            }
        }
        catch (Exception ex) { }

        return 0;
    }
    /// <summary>
    /// 执行SQL 
    /// </summary>
    private static bool ExecuteSQL(string sql, SQLiteParameter[] ps = null)
    {
        try
        {
            if (_cmd == null)
            {
                //检查数据库文件
                CheckLogDB();

                if (_cmd == null)
                {
                    //数据路径
                    string dbPath = AppDomain.CurrentDomain.BaseDirectory + "AppLog\\logs.db";

                    //数据库连接
                    string connectionString = "Data Source=" + dbPath + ";Version=3;UseUTF16Encoding=True;";
                    SQLiteConnection conn = new SQLiteConnection(connectionString);

                    //SQLiteCommand
                    _cmd = new SQLiteCommand();
                    _cmd.Connection = conn;
                    _cmd.CommandTimeout = 90;

                    //打开数据库连接
                    _cmd.Connection.Open();
                }
            }

            //是否打开数据库连接
            if (_cmd.Connection.State != System.Data.ConnectionState.Open)
            {
                _cmd.Connection.Open();
            }

            _cmd.CommandText = sql;
            _cmd.Parameters.Clear();

            //遍历参数
            if (ps != null && ps.Length > 0)
            {
                foreach (SQLiteParameter p in ps)
                {
                    _cmd.Parameters.Add(p);
                }
            }

            //是否有影响行数
            if (_cmd.ExecuteNonQuery() > 0) return true;
        }
        catch (Exception ex) { }

        return false;
    }

    /// <summary>
    /// 表类型
    /// </summary>
    private enum TableType
    {
        Logs,
        Journalises,
        Debugs,
        Errors,
        Operates
    }
    #endregion
}
