using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Service.Exts
{
    /// <summary>
    /// 定时事件服务
    /// </summary>
    public class TimeEventService
    {
        private static DataTable _dtEvents = null;
        private static long _runCount = -1;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            try
            {
                _dtEvents = SQLiteDao.GetTable(new SQLParam()
                {
                    TableName = "Sys_TimerEvents",
                    Wheres = new List<Where>()
                    {
                        new Where("IsAudit", true)
                    },
                    OrderBys = new List<OrderBy>()
                    {
                        new OrderBy("Order", OrderType.顺序)
                    }
                });
            }
            catch { }

            if (_dtEvents == null || _dtEvents.Rows.Count <= 0) return;

            System.Timers.Timer timer = new System.Timers.Timer(60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            //首次执行
            RunEvents();
        }
        /// <summary>
        /// 每分钟执行一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunEvents();
        }
        /// <summary>
        /// 执行事件
        /// </summary>
        private static void RunEvents()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                _runCount++;

                foreach (DataRow row in _dtEvents.Rows)
                {
                    int Frequency = row.GetInt("Frequency");

                    if (Frequency > 0 && _runCount % Frequency != 0) continue;

                    try
                    {
                        //执行事件
                        string sql = row.GetString("ExecuteSQL");
                        if (string.IsNullOrWhiteSpace(sql)) continue;
                        SQLiteDao.ExecuteNonQuery(sql);

                        //更新执行记录
                        sql = "update [Sys_TimerEvents] set [LastExecDate]=datetime('now','localtime'),[ExecCount]=ifnull([ExecCount],0)+1 where Id=" + row.GetId();
                        SQLiteDao.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex) { }
                }
            });
            thread.Start();
        }
    }
}
