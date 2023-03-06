using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    /// <summary>
    /// 共用服务
    /// </summary>
    public class CommonService
    {
        /// <summary>
        /// 数据库操作
        /// </summary>
        static DbHelper _db = new DbHelper();
        /// <summary>
        /// 初始数据库
        /// </summary>
        static string _sqlInitDB = @"";

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void InitDatabase(string path = null)
        {
            try
            {
                string root = AppDomain.CurrentDomain.BaseDirectory;
                root = root.Trim('\\');

                if (string.IsNullOrEmpty(path))
                {
                    path = root + "\\AppData\\Wsfly.db";
                }

                //已经有数据库
                if (File.Exists(path)) return;

                //还没有创建数据库
                _db.CreateDatabase(path);

                //执行SQL
                if (!string.IsNullOrWhiteSpace(_sqlInitDB))
                {
                    _db.ExecuteNonQuery(_sqlInitDB);
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "初始化数据库异常");
            }
        }
        /// <summary>
        /// 是否能连接到数据库
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectDatabase()
        {
            try
            {
                return _db.CanConnectDatabase();
            }
            catch (Exception) { }

            return false;
        }
        /// <summary>
        /// 执行脚本
        /// </summary>
        /// <param name="sql"></param>
        public static void ExecuteSql(string sql)
        {
            _db.ExecuteNonQuery(sql);
        }
    }
}
