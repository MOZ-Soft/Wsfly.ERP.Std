using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std
{
    /// <summary>
    /// 异常辅助
    /// </summary>
    public class ErrorHandler
    {
        /// <summary>
        /// 添加异常日志
        /// </summary>
        /// <param name="ex"></param>
        public static void AddException(Exception ex, string describe = "")
        {

        }
        /// <summary>
        /// 递归异常
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="ex"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static DataTable RecursionException(DataTable dt, Exception ex, long parentId, long id, string describe = "")
        {
            if (ex == null) return dt;

            //当前ID
            long currentId = id;

            string msgExt = "";

            try
            {
                msgExt += "\r\n\r\n客户端版本：" + AppGlobal.LocalConfig.Version;
                msgExt += "\r\n操作帐户：" + AppGlobal.UserInfo.UserName;
            }
            catch { }

            //异常行
            DataRow row = dt.NewRow();
            row["Id"] = currentId;
            row["ParentId"] = parentId;
            row["Describe"] = describe;
            row["Message"] = ex.Message + msgExt;
            row["Source"] = ex.Source;
            row["StackTrace"] = ex.StackTrace;
            row["IsSend"] = false;
            row["CreateDate"] = DateTime.Now;

            dt.Rows.Add(row);

            //下一个ID
            id++;

            if (ex.InnerException != null)
            {
                //递归异常
                return RecursionException(dt, ex.InnerException, currentId, id);
            }

            return dt;
        }
    }
}
