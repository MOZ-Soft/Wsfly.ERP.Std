using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Service.Exts
{
    public class UserService
    {
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="un"></param>
        /// <param name="pwd"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static DataRow Login(string un, string pwd, ref string msg)
        {
            try
            {
                //密码加密
                pwd = Core.Encryption.EncryptionAES.Encrypt(pwd);
                pwd = "MZEncrypt_" + pwd;

                SQLParam param = new SQLParam()
                {
                    TableName = "Sys_Users",
                    Wheres = new List<Where>()
                    {
                        new Where("LoginName", un),
                        new Where("Password", pwd),
                    }
                };

                DataRow row = SQLiteDao.GetTableRow(param);

                return row;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "登陆异常");
            }

            return null;
        }
        /// <summary>
        /// 设置主表显示比例
        /// </summary>
        /// <returns></returns>
        public static bool SetTableTopBL(UserInfo userInfo, long tableId, float topBL)
        {
            if (topBL < 0) return false;

            string tbName = "Sys_UserTableConfigs";

            string sql = "";

            try
            {
                //更新比例
                sql = "update [" + tbName + "] set [TopBL]=" + topBL + " where [UserId]=" + userInfo.Id + " and [TableId]=" + tableId + " and [TableCellId]=0";
                bool flag = SQLiteDao.ExecuteNonQuery(sql);
                if (flag) return true;

                //更新失败 则添加
                sql = "insert into [" + tbName + "]([UserId],[TableId],[TableCellId],[TopBL]) values(" + userInfo.Id + "," + tableId + ",0," + topBL + ")";
                return SQLiteDao.ExecuteNonQuery(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 设置表列宽度和顺序
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="tableId"></param>
        /// <param name="cellsTop"></param>
        /// <returns></returns>
        public static bool SetTableCellWidthOrder(UserInfo userInfo, long tableId, List<Newtonsoft.Json.Linq.JObject> cells)
        {
            try
            {
                //得到所有列
                if (cells == null || cells.Count <= 0) return false;

                long userId = userInfo.Id;
                string tbName = "Sys_UserTableConfigs";

                foreach (Newtonsoft.Json.Linq.JObject cell in cells)
                {
                    long cellId = cell.Value<long>("CellId");
                    int order = cell.Value<int>("CellOrder");
                    int width = cell.Value<int>("CellWidth");

                    //更新
                    string sql = "update [" + tbName + "] set [CellOrder]=" + order + ",[CellWidth]=" + width + " where [UserId]=" + userId + " and [TableId]=" + tableId + " and [TableCellId]=" + cellId;
                    bool flag = SQLiteDao.ExecuteNonQuery(sql);
                    if (!flag)
                    {
                        //更新失败 则添加
                        sql = "insert into [" + tbName + "]([UserId],[TableId],[TableCellId],[CellOrder],[CellWidth]) values(" + userId + "," + tableId + "," + cellId + "," + order + "," + width + ")";
                        SQLiteDao.ExecuteNonQuery(sql);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用表列的排序及宽度异常");
                throw ex;
            }
        }

        /// <summary>
        /// 设置角色权限
        /// </summary>
        /// <param name="dtRoleAuthoritys"></param>
        /// <returns></returns>
        public static bool SetRoleAuthoritys(DataTable dtRoleAuthoritys)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            try
            {
                //是否有数据
                if (dtRoleAuthoritys == null || dtRoleAuthoritys.Rows.Count <= 0) return false;

                //删除原记录
                string sql = "delete from [Sys_RoleAuthoritys]";
                SQLiteDao.TranExecute(cmd, sql);

                //遍历插入数据
                foreach (DataRow row in dtRoleAuthoritys.Rows)
                {
                    sql = "insert into [Sys_RoleAuthoritys]([RoleId],[ModuleId],[ActionId]) values(" + row.GetLong("RoleId") + "," + row.GetLong("ModuleId") + "," + row.GetLong("ActionId") + ");";
                    SQLiteDao.TranExecute(cmd, sql);
                }

                SQLiteDao.TranCommit(cmd);
                return true;
            }
            catch (Exception ex)
            {
                try { SQLiteDao.TranRollback(cmd); }
                catch { }
            }

            return false;
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        public static bool UpdatePassword(UserInfo userInfo, string oldPwd, string newPwd)
        {
            try
            {
                //密码加密
                oldPwd = "MZEncrypt_" + Core.Encryption.EncryptionAES.Encrypt(oldPwd);
                DataRow row = SQLiteDao.GetTableRow(userInfo.UserId, "Sys_Users");
                if (row.GetString("Password") != oldPwd)
                {
                    return false;
                }

                //修改密码
                newPwd = "MZEncrypt_" + Core.Encryption.EncryptionAES.Encrypt(newPwd);
                return SQLiteDao.Update(new SQLParam()
                {
                    TableName = "Sys_Users",
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("Password", newPwd)
                    },
                    Id = userInfo.Id
                });
            }
            catch (Exception ex) { }

            return false;
        }
    }
}
