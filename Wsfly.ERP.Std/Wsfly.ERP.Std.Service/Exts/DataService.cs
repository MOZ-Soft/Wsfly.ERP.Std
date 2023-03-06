using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Encryption;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Service.Models;

namespace Wsfly.ERP.Std.Service.Exts
{
    /// <summary>
    /// 数据操作服务
    /// </summary>
    public class DataService
    {
        #region 参数
        /// <summary>
        /// 添加锁
        /// </summary>
        public static object _lockInsert = new object();
        /// <summary>
        /// 更新锁
        /// </summary>
        public static object _lockUpdate = new object();
        /// <summary>
        /// 删除锁
        /// </summary>
        public static object _lockDelete = new object();
        #endregion

        #region 保存
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <returns></returns>
        public static List<SaveResult> Save(UserInfo userInfo, TableInfo tableInfo, List<SQLParam> saveItems)
        {
            //操作列表
            List<SaveResult> results = new List<SaveResult>();

            long dataIndex = 0;
            long parentId = 0;
            long insertId = 0;
            string tbName = "";

            bool hasSaveOperate = false;

            SQLParam operateRow = saveItems.Find(p => p.Action == Actions.添加);
            if (operateRow != null)
            {
                try
                {
                    //查询表最大索引
                    string sql = "select max([DataIndex]) from [" + operateRow.TableName + "]";
                    var obj = SQLiteDao.GetScalar(sql);
                    if (obj == DBNull.Value || obj == null) dataIndex = 1;
                    else dataIndex = DataType.Long(obj, 1);
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "查询表的最大索引异常！");
                    return null;
                }
            }

            //遍历要操作的记录
            int rowIndex = 1;
            foreach (SQLParam item in saveItems)
            {
                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    string msg = "";
                    bool flagSuccess = false;
                    if (item.Action == Actions.添加)
                    {
                        //数据排序索引
                        long dataRowIndex = dataIndex + rowIndex++;

                        //排序索引
                        if (!item.OpreateCells.Exists(p => p.Key.Equals("DataIndex")))
                        {
                            item.OpreateCells.Add(new KeyValue("DataIndex", dataRowIndex));
                        }

                        //操作的表名
                        if (string.IsNullOrWhiteSpace(tbName)) { tbName = item.TableName; }

                        //添加
                        insertId = Insert(userInfo, tableInfo, item, ref parentId, ref msg);
                        flagSuccess = insertId > 0;

                        hasSaveOperate = true;
                    }
                    else if (item.Action == Actions.修改)
                    {
                        //操作的表名
                        if (string.IsNullOrWhiteSpace(tbName)) { tbName = item.TableName; }

                        //修改
                        flagSuccess = Update(userInfo, tableInfo, item, ref parentId, ref msg);

                        hasSaveOperate = true;
                    }
                    else if (item.Action == Actions.删除)
                    {
                        //操作的表名
                        if (string.IsNullOrWhiteSpace(tbName)) { tbName = item.TableName; }

                        //删除
                        flagSuccess = Delete(userInfo, tableInfo, item, ref parentId, ref msg);

                        hasSaveOperate = true;
                    }

                    //添加列表
                    results.Add(new SaveResult()
                    {
                        Success = flagSuccess,
                        RowIndex = item.RowIndex,
                        ResultId = insertId,
                        Message = msg,

                        _ORGID = item._ORGID
                    });
                });
                thread.IsBackground = true;
                thread.Start();
            }

            while (results.Count < saveItems.Count)
            {
                //等待
                System.Threading.Thread.Sleep(200);
            }

            try
            {
                //更新主单总数量、总金额
                if (tableInfo.SubTable != null && tableInfo.SubTable.TableName.Equals(tbName) && parentId > 0)
                {
                    SummaryStatistics(parentId, tableInfo, tableInfo.SubTable);
                }
            }
            catch (Exception ex) { }

            try
            {
                //是否有最后执行事件
                if (tableInfo != null && tableInfo.TableName == tbName && tableInfo.Events.Exists(p => p.IsLastExecute))
                {
                    if (hasSaveOperate && tableInfo.Events.Exists(p => p.Action == "保存" && p.IsLastExecute))
                    {
                        //有保存操作
                        RunTableLastExecuteEvent2(tableInfo, parentId);
                    }
                }
                else if (tableInfo.SubTable != null && tableInfo.SubTable.TableName == tbName && tableInfo.SubTable.Events.Exists(p => p.IsLastExecute))
                {
                    if (hasSaveOperate && tableInfo.SubTable.Events.Exists(p => p.Action == "保存" && p.IsLastExecute))
                    {
                        //有保存操作
                        RunTableLastExecuteEvent2(tableInfo.SubTable, parentId);
                    }
                }
            }
            catch (Exception ex) { }

            //返回结果
            return results;
        }
        #endregion

        #region 添加
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <returns></returns>
        public static long Insert(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref string msg)
        {
            long parentId = 0;
            return Insert(userInfo, tableInfo, param, ref parentId, ref msg);
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <returns></returns>
        public static long Insert(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref long parentId, ref string msg)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            try
            {
                //===========================================================================================
                //操作表
                TableInfo operateTable = tableInfo;
                //主表行
                DataRow topRow = null;

                //事务ID
                string tranId = "SW_TJ_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                //设置参数值 null=DBNull.Value
                SetParamValue(param.OpreateCells);

                //===============================
                //添加前
                //===============================
                //判断操作的表名是否子表
                if (tableInfo.SubTable != null && tableInfo.SubTable.TableName.Equals(param.TableName))
                {
                    operateTable = tableInfo.SubTable;
                    operateTable.ParentTableName = tableInfo.TableName;

                    //是否有上级Id
                    if (!param.OpreateCells.Exists(p => p.Key.Equals("ParentId")))
                    {
                        //是从表，但是没有上级Id
                        throw new Exception("添加从表数据缺少参数：ParentId");
                    }

                    //父级Id
                    parentId = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("ParentId")).Value, 0);
                    if (parentId <= 0) throw new Exception("添加从表数据缺少参数：ParentId");

                    //主表行
                    topRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + tableInfo.TableName + "] where [Id]=" + parentId);
                    if (topRow == null) throw new Exception("添加从表数据缺少主表记录");

                    //是否有审核列
                    if (topRow.Table.Columns.Contains("IsAudit"))
                    {
                        //是否已经审核
                        if (DataType.Bool(topRow["IsAudit"], false)) throw new Exception("主表已审核无法添加");
                    }
                }

                //设置默认值
                SetDefaultValue(cmd, operateTable, param.OpreateCells, true);

                //计算数量、单价、金额
                InsertJSDJSLJE(param, operateTable);

                //计算辅助公式
                CalculationAuxiliaryFormula(param.OpreateCells, operateTable);

                //添加前执行事件
                bool flagBeginEvent = RunTableEvent_BeforeSave(cmd, operateTable, param, EventActions.保存, ref msg);
                if (!flagBeginEvent)
                {
                    throw new Exception("保存前执行事件失败，消息：" + msg);
                }

                //===============================
                //添加数据
                //===============================

                //命令模式
                cmd.CommandType = CommandType.Text;

                //生成SQL
                string sqlInsert = "insert into[" + operateTable.TableName + "]($wsfly.cells$) values($wsfly.values$)";
                //生成参数及SQL
                List<SQLiteParameter> psInsert = BuildInsertSQLAndParameters(param.OpreateCells, ref sqlInsert);
                // 是否有参数
                if (psInsert == null || psInsert.Count <= 0) throw new Exception("未找到要添加的数据");

                DataRow newRow = null;
                long addId = 0;

                lock (_lockInsert)
                {
                    //===============================
                    //保存前判断 BEGIN
                    //===============================
                    #region 保存前判断
                    #region 1、限制最大最小值
                    if (operateTable.Cells.Exists(p => p.XZZDZXZ))
                    {
                        List<CellInfo> cellXZZDZXZList = operateTable.Cells.FindAll(p => p.XZZDZXZ);
                        if (cellXZZDZXZList != null && cellXZZDZXZList.Count > 0)
                        {
                            foreach (CellInfo cell in cellXZZDZXZList)
                            {
                                var kvItem = param.OpreateCells.Find(p => p.Key.Equals(cell.CellName));
                                decimal val = DataType.Decimal(kvItem.Value, 0);
                                if (val < cell.ZXZ)
                                {
                                    msg = "列[" + cell.CnName + "]低于最小值（" + DecimalToString(cell.ZXZ) + "）！";
                                    return 0;
                                }
                                if (val > cell.ZDZ)
                                {
                                    msg = "列[" + cell.CnName + "]大于最大值（" + DecimalToString(cell.ZDZ) + "）！";
                                    return 0;
                                }
                            }
                        }
                    }
                    #endregion

                    #region 2、不可大于引用数量
                    //如果已经勾选对比引用数量 或者 对比引用数量和对比引用金额都没有勾选（即默认判断是引用数量）
                    if ((operateTable.DBYYSL || (!operateTable.DBYYSL && !operateTable.DBYYJE)) && (operateTable.BKDYYY || operateTable.BKXYYY))
                    {
                        if (operateTable.Cells.Exists(p => p.CellName.Equals("SL")) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("GLID")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("SL")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("GLID")))
                        {
                            //引用表名
                            string yybm = operateTable.WCSLTableName;
                            string yylm = "WCSL";
                            if (string.IsNullOrWhiteSpace(yybm) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("WCSLBM")) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("WCSLLM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("WCSLBM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("WCSLLM")))
                            {
                                yybm = param.OpreateCells.Find(p => p.Key.Equals("WCSLBM")).Value.ToString();
                                yylm = param.OpreateCells.Find(p => p.Key.Equals("WCSLLM")).Value.ToString();
                            }
                            if (string.IsNullOrWhiteSpace(yybm) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("SYSYYBM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("SYSYYBM")))
                            {
                                yybm = param.OpreateCells.Find(p => p.Key.Equals("SYSYYBM")).Value.ToString();
                            }

                            decimal sl = DataType.Decimal(param.OpreateCells.Find(p => p.Key.Equals("SL")).Value, 0);
                            long glid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("GLID")).Value, 0);

                            if (!string.IsNullOrWhiteSpace(yybm) && !string.IsNullOrWhiteSpace(yylm) && glid > 0)
                            {
                                //查询原数量
                                string sqlQuery = "select [SL],[" + yylm + "] from [" + yybm + "] where [Id]=" + glid;
                                DataRow rowParent = SQLiteDao.TranQueryRow(cmd, sqlQuery);
                                if (rowParent != null)
                                {
                                    decimal parentSL = DataType.Decimal(rowParent["SL"], 0);
                                    decimal parentWCSL = DataType.Decimal(rowParent[yylm], 0);

                                    if (operateTable.BKDYYY)
                                    {
                                        //不可大于引用
                                        if (operateTable.BKDYYYBL > 0)
                                        {
                                            //不可大于引用比例 如引用数量1000 不可大于引用比例120 则 不可大于 1200
                                            parentSL = parentSL * (operateTable.BKDYYYBL / 100);
                                        }

                                        if (parentWCSL + sl > parentSL)
                                        {
                                            msg = "不可大于数量（" + DecimalToString(parentSL - parentWCSL) + "）！";
                                            return 0;
                                        }
                                    }
                                    else if (operateTable.BKXYYY)
                                    {
                                        //不可小于引用
                                        if (operateTable.BKXYYYBL > 0)
                                        {
                                            //不可小于引用比例 如引用数量1000 不可大于引用比例120 则 不可小于 1200
                                            parentSL = parentSL * (operateTable.BKXYYYBL / 100);
                                        }

                                        if (parentWCSL + sl < parentSL)
                                        {
                                            msg = "不可小于数量（" + DecimalToString(parentSL - parentWCSL) + "）！";
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region 3、不可大于引用金额
                    if (operateTable.DBYYJE && (operateTable.BKDYYY || operateTable.BKXYYY))
                    {
                        if (operateTable.Cells.Exists(p => p.CellName.Equals("JE")) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("GLID")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("JE")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("GLID")))
                        {
                            //引用表名
                            string yybm = operateTable.WCJETableName;
                            string yylm = "WCJE";
                            if (string.IsNullOrWhiteSpace(yybm) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("WCJEBM")) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("WCJELM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("WCJEBM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("WCJELM")))
                            {
                                yybm = param.OpreateCells.Find(p => p.Key.Equals("WCJEBM")).Value.ToString();
                                yylm = param.OpreateCells.Find(p => p.Key.Equals("WCJELM")).Value.ToString();
                            }
                            if (string.IsNullOrWhiteSpace(yybm) &&
                                operateTable.Cells.Exists(p => p.CellName.Equals("SYSYYBM")) &&
                                param.OpreateCells.Exists(p => p.Key.Equals("SYSYYBM")))
                            {
                                yybm = param.OpreateCells.Find(p => p.Key.Equals("SYSYYBM")).Value.ToString();
                            }

                            decimal je = DataType.Decimal(param.OpreateCells.Find(p => p.Key.Equals("JE")).Value, 0);
                            long glid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("GLID")).Value, 0);

                            if (!string.IsNullOrWhiteSpace(yybm) && !string.IsNullOrWhiteSpace(yylm) && glid > 0)
                            {
                                //查询原金额
                                string sqlQuery = "select [JE],[" + yylm + "] from [" + yybm + "] where [Id]=" + glid;
                                DataRow rowParent = SQLiteDao.TranQueryRow(cmd, sqlQuery);
                                if (rowParent != null)
                                {
                                    decimal parentJE = DataType.Decimal(rowParent["JE"], 0);
                                    decimal parentWCJE = DataType.Decimal(rowParent[yylm], 0);

                                    if (operateTable.BKDYYY)
                                    {
                                        //不可大于引用
                                        if (operateTable.BKDYYYBL > 0)
                                        {
                                            //不可大于引用比例 如引用金额1000 不可大于引用比例 120 则 不可大于 1200
                                            parentJE = parentJE * (operateTable.BKDYYYBL / 100);
                                        }

                                        if (parentWCJE + je > parentJE)
                                        {
                                            msg = "不可大于金额（" + DecimalToString(parentJE - parentWCJE) + "）！";
                                            return 0;
                                        }
                                    }
                                    else if (operateTable.BKXYYY)
                                    {
                                        //不可小于引用
                                        if (operateTable.BKXYYYBL > 0)
                                        {
                                            //不可小于引用比例 如引用金额1000 不可大于引用比例 120 则 不可小于 1200
                                            parentJE = parentJE * (operateTable.BKXYYYBL / 100);
                                        }

                                        if (parentWCJE + je < parentJE)
                                        {
                                            msg = "不可小于金额（" + DecimalToString(parentJE - parentWCJE) + "）！";
                                            return 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region 4、不可大于库存
                    if (operateTable.BKDYKC)
                    {
                        if (operateTable.Cells.Exists(p => p.CellName.Equals("SPID")) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("SL")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("SPID")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("SL")))
                        {
                            long spid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("SPID")).Value, 0);
                            decimal sl = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("SL")).Value, 0);
                            string kcbm = !string.IsNullOrWhiteSpace(operateTable.SPStockTableName) ? operateTable.SPStockTableName : string.Empty;

                            if (!string.IsNullOrWhiteSpace(kcbm) && spid > 0)
                            {
                                //查询原库存
                                string sqlQuery = "select [KCSL] from [" + kcbm + "] where [SPID]=" + spid;
                                object objQueryResult = SQLiteDao.TranExecuteScalar(cmd, sqlQuery);
                                decimal currKCSL = DataType.Decimal(objQueryResult, 0);

                                //是否大于库存数量
                                if (sl > currKCSL)
                                {
                                    msg = "大于库存数量（" + DecimalToString(currKCSL) + "）！";
                                    return 0;
                                }
                            }
                        }
                    }
                    #endregion

                    #region 5、不可重复
                    if (operateTable.Cells.Exists(p => p.AllownRepeat == false))
                    {
                        foreach (CellInfo cell in operateTable.Cells.Where(p => p.AllownRepeat == false))
                        {
                            if (operateTable.TableName == "Sys_Tables" && cell.CellName == "TableName")
                            {
                                //虚拟表不判断表名重复
                                string tableType = param.OpreateCells.Find(p => p.Key == "Type").Value.ToString();
                                if (tableType == "虚拟") continue;
                            }

                            string sqlAllownRepeat = "select count(*) from [" + operateTable.TableName + "] where 1=1";
                            long parentId_Repeate = param.OpreateCells.Exists(p => p.Key.Equals("ParentId")) ? DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("ParentId")).Value, 0) : 0;
                            object value = param.OpreateCells.Exists(p => p.Key.Equals(cell.CellName)) ? param.OpreateCells.Find(p => p.Key.Equals(cell.CellName)).Value : null;
                            if (value == null) continue;

                            List<SQLiteParameter> psAllownRepeats = new List<SQLiteParameter>();

                            if (parentId_Repeate > 0)
                            {
                                sqlAllownRepeat += " and [ParentId]=@ParentId";
                                psAllownRepeats.Add(new SQLiteParameter() { ParameterName = "@ParentId", Value = parentId_Repeate });
                            }
                            if (value != null)
                            {
                                sqlAllownRepeat += " and [" + cell.CellName + "]=@Value";
                                psAllownRepeats.Add(new SQLiteParameter() { ParameterName = "@Value", Value = value });
                            }

                            object objQueryResult = SQLiteDao.TranExecuteScalar(cmd, sqlAllownRepeat, psAllownRepeats.ToArray());
                            int repeatSL = DataType.Int(objQueryResult, 0);
                            if (repeatSL > 0)
                            {
                                msg = "列[" + cell.CnName + "]不可重复！";
                                return 0;
                            }
                        }
                    }

                    if (operateTable.TableName.Equals("Sys_TableCells"))
                    {
                        //列名不可重复
                        try
                        {
                            string tableName = topRow["TableName"].ToString();
                            string tableType = topRow["Type"].ToString();
                            string cellName = param.OpreateCells.Find(p => p.Key == "CellName").Value.ToString();

                            if (tableType == "单表" || tableType == "双表" || tableType == "三表")
                            {
                                DataTable dt = SQLiteDao.TranQueryTable(cmd, "select * from [" + tableName + "] limit 1");
                                if (dt != null && dt.Columns.Contains(cellName))
                                {
                                    msg = "表列名[" + cellName + "]已经存在，请更换列名！";
                                    return 0;
                                }
                            }
                        }
                        catch { }
                    }
                    #endregion
                    #endregion
                    //===============================
                    //保存前判断 END
                    //===============================


                    //===============================
                    //添加保存 BEGIN
                    //===============================
                    //添加执行结果
                    object objResult = SQLiteDao.TranExecuteScalar(cmd, sqlInsert, psInsert.ToArray());
                    addId = DataType.Long(objResult, 0);
                    if (addId <= 0) throw new Exception("添加数据失败");

                    //新添加的行
                    newRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + operateTable.TableName + "] where [Id]=" + addId);
                    if (newRow == null) throw new Exception("获取添加的行失败");

                    //保存ID到参数
                    param.OpreateCells.Add(new KeyValue("Id", addId));
                    //===============================
                    //添加保存 END
                    //===============================


                    //===============================
                    //添加后
                    //===============================

                    //是否表维护
                    if (operateTable.TableName.Equals("Sys_Tables"))
                    {
                        //表维护
                        SaveDataTable(cmd, newRow, addId);
                    }
                    else if (operateTable.TableName.Equals("Sys_TableCells"))
                    {
                        //列维护
                        UpdateTableCellsConfig(cmd, operateTable, topRow, null, newRow, true);
                    }

                    //执行公式
                    bool flagJSGS = ProcessGS(cmd, operateTable, addId, tranId);
                    if (flagJSGS)
                    {
                        //执行公式后的行
                        newRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + operateTable.TableName + "] where [Id]=" + addId);
                        if (newRow == null) throw new Exception("获取执行公式后的行失败");
                    }

                    //同步商品信息表、商品库存
                    SyncSPXX2SPKC(cmd, tableInfo, null, newRow, tranId);

                    //开单处理应收应付
                    KDProcessYSYFK(cmd, tableInfo, topRow, newRow, null, tranId);

                    //开单处理库存
                    KDProcessStock(cmd, operateTable, topRow, newRow, null, tranId);

                    //更新完成数量
                    UpdateWCSL(Actions.添加, cmd, operateTable, newRow, null, tranId);

                    //更新完成金额
                    UpdateWCJE(Actions.添加, cmd, operateTable, newRow, null, tranId);

                    //汇总统计
                    SummaryStatistics(cmd, tableInfo, operateTable, topRow, null, newRow, tranId);

                    //生成序列号
                    BuildSerialNo(cmd, operateTable, newRow, addId, tranId);

                    //更新被引用数量
                    UpdateSYSBYYSL(cmd, operateTable, newRow, null);

                    //保存后执行事件
                    RunTableEvent(cmd, operateTable, null, newRow, EventActions.保存, ref msg, false);
                }

                //===========================================================================================

                //提交事务
                SQLiteDao.TranCommit(cmd);
                return addId;
            }
            catch (Exception ex)
            {
                //回溯事务
                SQLiteDao.TranRollback(cmd);

                //添加异常
                AppLog.WriteBugLog(ex, "执行添加事务异常，用户：" + (userInfo != null ? userInfo.UserName : "") + "，表：" + (tableInfo != null ? tableInfo.TableName : ""));
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = "执行添加事务失败";
            return 0;
        }
        /// <summary>
        /// 同步商品信息 到 商品库存
        /// </summary>
        private static void SyncSPXX2SPKC(SQLiteCommand cmd, TableInfo tableInfo, DataRow orgRow, DataRow newRow, string tranId)
        {
            //商品表同步生成库存记录
            if (tableInfo.TableSubType != TableSubType.商品表) return;

            //查询库存表
            string sql = "select * from [Sys_Tables] where [ParentId]=" + tableInfo.Id + " and [SubType]='库存表'";
            DataRow row = SQLiteDao.TranQueryRow(cmd, sql);
            if (row == null) return;

            //库存表名
            string spkcb = row.GetString("TableName");

            if (orgRow == null && newRow != null)
            {
                //添加
                DataServiceExt.AddSPKC(cmd, spkcb, newRow.GetId(), newRow.GetString("SPMC"), newRow.GetString("SPBH"));
            }
            else if (newRow == null && orgRow != null)
            {
                //删除
                DataServiceExt.DeleteSPKC(cmd, spkcb, orgRow.GetId());
            }
            else
            {
                //修改
                DataServiceExt.UpdateSPKC(cmd, spkcb, newRow.GetId(), newRow.GetString("SPMC"), newRow.GetString("SPBH"));
            }
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <returns></returns>
        public static bool Update(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref string msg)
        {
            long parentId = 0;
            return Update(userInfo, tableInfo, param, ref parentId, ref msg);
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        public static bool Update(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref long parentId, ref string msg)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            try
            {
                //===========================================================================================
                //操作表
                TableInfo operateTable = tableInfo;
                //主表行
                DataRow rowTop = null;

                //事务ID
                string tranId = "SW_GX_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                //设置参数值 null=DBNull.Value
                SetParamValue(param.OpreateCells);

                //判断操作的表名是否子表
                if (tableInfo.SubTable != null && tableInfo.SubTable.TableName.Equals(param.TableName))
                {
                    operateTable = tableInfo.SubTable;
                    operateTable.ParentTableName = tableInfo.TableName;

                    //是否有上级Id
                    if (!param.OpreateCells.Exists(p => p.Key.Equals("ParentId")))
                    {
                        //是从表，但是没有上级Id
                        throw new Exception("修改从表数据缺少参数：ParentId");
                    }

                    //父级Id
                    parentId = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("ParentId")).Value, 0);
                    if (parentId <= 0) throw new Exception("修改从表数据缺少参数：ParentId");

                    //主表行
                    rowTop = SQLiteDao.TranQueryRow(cmd, "select * from [" + tableInfo.TableName + "] where [Id]=" + parentId);
                    if (rowTop == null) throw new Exception("修改从表数据缺少主表记录");

                    //是否有审核列
                    if (rowTop.Table.Columns.Contains("IsAudit"))
                    {
                        //是否已经审核
                        if (DataType.Bool(rowTop["IsAudit"], false)) throw new Exception("主表已审核无法更新");
                    }
                }

                try
                {
                    //需要过滤的列
                    List<string> filterUpdateCells = new List<string>();
                    if (tableInfo.SubTable != null)
                    {
                        filterUpdateCells = new List<string>() { "WCSL", "WCJE", "QTSL", "YPTSL", "WPTSL", "SYSYYBM", };
                    }
                    else if (tableInfo.TableSubType == TableSubType.客户表 || tableInfo.TableSubType == TableSubType.供应商表)
                    {
                        //客户表
                        filterUpdateCells = new List<string>() { "YSK", "YFK" };
                    }

                    //要过滤的列
                    if (filterUpdateCells != null)
                    {
                        filterUpdateCells.Add("ZSL");
                        filterUpdateCells.Add("ZJE");
                        filterUpdateCells.Add("WCSL");
                        filterUpdateCells.Add("WCJE");
                        filterUpdateCells.Add("SYSBYYSL");

                        foreach (string filterCellName in filterUpdateCells)
                        {
                            KeyValue kvFilterCell = param.OpreateCells.Find(p => p.Key.ToUpper().Equals(filterCellName));
                            if (kvFilterCell != null) param.OpreateCells.Remove(kvFilterCell);
                        }
                    }
                }
                catch { }

                //获取原数据
                string whereSql = string.Empty;
                string sqlNull = null;
                List<SQLiteParameter> psWhere = BuildUpdateSQLAndParameters(param, ref sqlNull, ref whereSql);
                string sqlQueryOrgData = "select * from [" + operateTable.TableName + "] where " + whereSql;
                DataTable dtOrgData = SQLiteDao.TranQueryTable(cmd, sqlQueryOrgData, psWhere.ToArray());
                if (dtOrgData == null || dtOrgData.Rows.Count <= 0)
                {
                    msg = "要修改的数据不存在或删除！";
                    return false;
                }

                //设置默认值
                SetDefaultValue(cmd, operateTable, param.OpreateCells, false);

                //计算数量、单价、金额
                UpdateJSDJSLJE(param, operateTable);

                //计算辅助公式
                CalculationAuxiliaryFormula(param.OpreateCells, operateTable);

                //===========================================================================================

                //遍历更新所有
                foreach (DataRow row in dtOrgData.Rows)
                {
                    //更新数据行
                    bool flagUpdate = UpdateDataRow(cmd, userInfo, tableInfo, operateTable, param, rowTop, row, ref msg, tranId);
                    if (!flagUpdate) throw new Exception("更新数据出错：" + msg);
                }

                //提交事务
                SQLiteDao.TranCommit(cmd);
                return true;
            }
            catch (Exception ex)
            {
                //回溯事务
                SQLiteDao.TranRollback(cmd);

                //添加异常
                AppLog.WriteBugLog(ex, "执行更新事务异常");
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = "执行更新事务失败";
            return false;
        }
        /// <summary>
        /// 更新数据行
        /// </summary>
        /// <returns></returns>
        private static bool UpdateDataRow(SQLiteCommand cmd, UserInfo user, TableInfo tableInfo, TableInfo operateTable, SQLParam param, DataRow topRow, DataRow orgRow, ref string msg, string tranId)
        {
            List<string> filterCells = new List<string>();

            #region 判断是否可修改
            cmd.CommandType = CommandType.Text;

            //判断是否可修改
            if (orgRow.Table.Columns.Contains("SYSBYYSL"))
            {
                //是否有被引用数量
                if (DataType.Decimal(orgRow["SYSBYYSL"], 0) != 0)
                {
                    if (operateTable.TableSubType == TableSubType.商品表)
                    {
                        if (orgRow.Table.Columns.Contains("SPMC"))
                        {
                            object newSPMC = param.GetKeyValue("SPMC");
                            if (newSPMC != null && orgRow["SPMC"].ToString() != newSPMC.ToString())
                            {
                                msg = "行已有引用，名称不可修改";
                                return false;
                            }
                        }
                        if (orgRow.Table.Columns.Contains("SPBH"))
                        {
                            object newSPBH = param.GetKeyValue("SPBH");
                            if (newSPBH != null && orgRow["SPBH"].ToString().ToUpper() != newSPBH.ToString().ToUpper())
                            {
                                msg = "行已有引用，编号不可修改";
                                return false;
                            }
                        }
                    }
                    else if (operateTable.TableSubType == TableSubType.客户表 || operateTable.TableSubType == TableSubType.供应商表)
                    {
                        if (orgRow.Table.Columns.Contains("KHMC"))
                        {
                            object newKHMC = param.GetKeyValue("KHMC");
                            if (newKHMC != null && orgRow["KHMC"].ToString() != newKHMC.ToString())
                            {
                                msg = "行已有引用，名称不可修改";
                                return false;
                            }
                        }
                        if (orgRow.Table.Columns.Contains("KHBH"))
                        {
                            object newKHBH = param.GetKeyValue("KHBH");
                            if (newKHBH != null && orgRow["KHBH"].ToString().ToUpper() != newKHBH.ToString().ToUpper())
                            {
                                msg = "行已有引用，编号不可修改";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        msg = "有被引用数量不可修改";
                        return false;
                    }
                }
            }
            if (orgRow.Table.Columns.Contains("WCSL"))
            {
                //是否有完成数量
                if (DataType.Decimal(orgRow["WCSL"], 0) != 0)
                {
                    msg = "有完成数量不可修改";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("WCJE"))
            {
                //是否有完成金额
                if (DataType.Decimal(orgRow["WCJE"], 0) != 0)
                {
                    msg = "有完成金额不可修改";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("IsAudit"))
            {
                //是否已经审核
                if (DataType.Bool(orgRow["IsAudit"], false))
                {
                    msg = "已审核不可修改";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("IsLock"))
            {
                //是否已经锁定
                if (DataType.Bool(orgRow["IsLock"], false))
                {
                    msg = "已锁定不可修改";
                    return false;
                }
            }

            if (operateTable.TableType == TableType.双表 && operateTable.SubTable != null && operateTable.Cells.Exists(p => p.CellName.Equals("KHID")))
            {
                //有客户ID列
                KeyValue newKHID = param.OpreateCells.Find(p => p.Key == "KHID");
                if (newKHID != null)
                {
                    long parentId = DataType.Long(orgRow["Id"], 0);
                    long orgKHID = DataType.Long(orgRow["KHID"], 0);
                    long newKHID2 = DataType.Long(newKHID.Value, 0);

                    //查询是否有子表数据
                    cmd.CommandType = CommandType.Text;
                    object objCount = SQLiteDao.TranExecuteScalar(cmd, "select count(*) from [" + operateTable.SubTable.TableName + "] where [ParentId]=" + parentId);
                    //非同一个客户 且 有明细数据 不能修改
                    if (orgKHID != newKHID2 && DataType.Int(objCount, 0) > 0)
                    {
                        msg = "有明细记录，不可修改[KHID]";
                        return false;
                    }
                }
            }

            if (operateTable.TableType == TableType.双表 && operateTable.ThreeTable != null && operateTable.Cells.Exists(p => p.CellName.Equals("SPID")))
            {
                //修改从表且有三表 如果是从表更换商品 则不可继续
                long orgSPID = DataType.Long(orgRow["SPID"], 0);
                KeyValue newSPIDObj = param.OpreateCells.Find(p => p.Key == "SPID");
                long newSPID = newSPIDObj == null ? 0 : DataType.Long(newSPIDObj.Value, 0);

                if (orgSPID > 0 && newSPID > 0 && orgSPID != newSPID)
                {
                    msg = "有扩展记录，不可修改[SPID]";
                    return false;
                }
            }
            #endregion

            //是否修改表列配置
            if (operateTable.TableName.Equals("Sys_TableCells"))
            {
                //注意
                //当前使用SQLite版本：SQLite3，此版本中并没有提供直接更改列名与删除列的命令，变更相对较复杂会对其它功能产生影响，所以默认不可变更列名。
                try
                {
                    string tableType = topRow.GetString("Type");
                    var cellParam = param.OpreateCells.Find(p => p.Key == "CellName");
                    if (cellParam != null && tableType != "视图") param.OpreateCells.Remove(cellParam);
                }
                catch { }
            }

            //===============================
            //更新前
            //===============================
            //保存前执行事件
            bool flagBeginEvent = RunTableEvent_BeforeSave(cmd, operateTable, param, EventActions.保存, ref msg);
            if (!flagBeginEvent)
            {
                throw new Exception("更新前执行事件失败，消息：" + msg);
            }

            //===============================
            //更新数据
            //===============================

            string sqlNull = "";
            string sqlUpdateCells = "";

            //要过滤的列
            if (filterCells != null)
            {
                filterCells.Add("ZSL");
                filterCells.Add("ZJE");
                filterCells.Add("WCSL");
                filterCells.Add("WCJE");
                filterCells.Add("SYSBYYSL");

                foreach (string filterCellName in filterCells)
                {
                    KeyValue kvFilterCell = param.OpreateCells.Find(p => p.Key.ToUpper().Equals(filterCellName));
                    if (kvFilterCell != null) param.OpreateCells.Remove(kvFilterCell);
                }
            }

            //生成参数及SQL
            List<SQLiteParameter> ps = BuildUpdateSQLAndParameters(param, ref sqlUpdateCells, ref sqlNull);

            //命令模式
            cmd.CommandType = CommandType.Text;

            long updateId = DataType.Long(orgRow["Id"], 0);

            //===============================
            //更新后
            //===============================

            string sqlUpdate = string.Empty;
            DataRow newRow = null;

            lock (_lockUpdate)
            {
                //===============================
                //保存前判断 BEGIN
                //===============================
                #region 保存前判断
                #region 1、限制最大最小值
                if (operateTable.Cells.Exists(p => p.XZZDZXZ))
                {
                    List<CellInfo> cellXZZDZXZList = operateTable.Cells.FindAll(p => p.XZZDZXZ);
                    if (cellXZZDZXZList != null && cellXZZDZXZList.Count > 0)
                    {
                        foreach (CellInfo cell in cellXZZDZXZList)
                        {
                            KeyValue kvItem = param.OpreateCells.Find(p => p.Key.Equals(cell.CellName));
                            decimal val = DataType.Decimal(kvItem.Value, 0);
                            if (val < cell.ZXZ)
                            {
                                msg = "列[" + cell.CellName + "]低于最小值（" + DecimalToString(cell.ZXZ) + "）！";
                                return false;
                            }
                            if (val > cell.ZDZ)
                            {
                                msg = "列[" + cell.CellName + "]大于最大值（" + DecimalToString(cell.ZDZ) + "）！";
                                return false;
                            }
                        }
                    }
                }
                #endregion

                #region 2、不可大于引用数量
                if ((operateTable.DBYYSL || (!operateTable.DBYYSL && !operateTable.DBYYJE)) && (operateTable.BKDYYY || operateTable.BKXYYY))
                {
                    if (operateTable.Cells.Exists(p => p.CellName.Equals("SL")) &&
                        operateTable.Cells.Exists(p => p.CellName.Equals("GLID")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("SL")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("GLID")))
                    {
                        //引用表名
                        string yybm = operateTable.WCSLTableName;
                        string yylm = "WCSL";
                        if (string.IsNullOrWhiteSpace(yybm) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("WCSLBM")) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("WCSLLM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("WCSLBM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("WCSLLM")))
                        {
                            yybm = param.OpreateCells.Find(p => p.Key.Equals("WCSLBM")).Value.ToString();
                            yylm = param.OpreateCells.Find(p => p.Key.Equals("WCSLLM")).Value.ToString();
                        }
                        if (string.IsNullOrWhiteSpace(yybm) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("SYSYYBM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("SYSYYBM")))
                        {
                            yybm = param.OpreateCells.Find(p => p.Key.Equals("SYSYYBM")).Value.ToString();
                        }

                        decimal sl = DataType.Decimal(param.OpreateCells.Find(p => p.Key.Equals("SL")).Value, 0);
                        long glid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("GLID")).Value, 0);

                        if (!string.IsNullOrWhiteSpace(yybm) && !string.IsNullOrWhiteSpace(yylm) && glid > 0)
                        {
                            //查询原数量
                            string sqlQuery = "select [SL],[" + yylm + "] from [" + yybm + "] where [Id]=" + glid;
                            DataRow rowParent = SQLiteDao.TranQueryRow(cmd, sqlQuery);
                            if (rowParent != null)
                            {
                                decimal parentSL = DataType.Decimal(rowParent["SL"], 0);
                                decimal parentWCSL = DataType.Decimal(rowParent[yylm], 0);
                                decimal orgSL = DataType.Decimal(orgRow["SL"], 0);

                                //减回原数量
                                parentWCSL -= orgSL;

                                if (operateTable.BKDYYY)
                                {
                                    //不可大于引用
                                    if (operateTable.BKDYYYBL > 0)
                                    {
                                        //不可大于引用比例 如引用数量1000 不可大于引用比例120 则 不可大于 1200
                                        parentSL = parentSL * (operateTable.BKDYYYBL / 100);
                                    }

                                    if (parentWCSL + sl > parentSL)
                                    {
                                        msg = "不可大于数量（" + DecimalToString(parentSL - parentWCSL) + "）！";
                                        return false;
                                    }
                                }
                                else if (operateTable.BKXYYY)
                                {
                                    //不可小于引用
                                    if (operateTable.BKXYYYBL > 0)
                                    {
                                        //不可小于引用比例 如引用数量1000 不可大于引用比例120 则 不可小于 1200
                                        parentSL = parentSL * (operateTable.BKXYYYBL / 100);
                                    }

                                    if (parentWCSL + sl < parentSL)
                                    {
                                        msg = "不可小于数量（" + DecimalToString(parentSL - parentWCSL) + "）！";
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 3、不可大于引用金额
                if (operateTable.DBYYJE && (operateTable.BKDYYY || operateTable.BKXYYY))
                {
                    if (operateTable.Cells.Exists(p => p.CellName.Equals("JE")) &&
                        operateTable.Cells.Exists(p => p.CellName.Equals("GLID")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("JE")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("GLID")))
                    {
                        //引用表名
                        string yybm = operateTable.WCJETableName;
                        string yylm = "WCJE";
                        if (string.IsNullOrWhiteSpace(yybm) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("WCJEBM")) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("WCJELM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("WCJEBM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("WCJELM")))
                        {
                            yybm = param.OpreateCells.Find(p => p.Key.Equals("WCJEBM")).Value.ToString();
                            yylm = param.OpreateCells.Find(p => p.Key.Equals("WCJELM")).Value.ToString();
                        }
                        if (string.IsNullOrWhiteSpace(yybm) &&
                            operateTable.Cells.Exists(p => p.CellName.Equals("SYSYYBM")) &&
                            param.OpreateCells.Exists(p => p.Key.Equals("SYSYYBM")))
                        {
                            yybm = param.OpreateCells.Find(p => p.Key.Equals("SYSYYBM")).Value.ToString();
                        }

                        decimal je = DataType.Decimal(param.OpreateCells.Find(p => p.Key.Equals("JE")).Value, 0);
                        long glid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("GLID")).Value, 0);

                        if (!string.IsNullOrWhiteSpace(yybm) && !string.IsNullOrWhiteSpace(yylm) && glid > 0)
                        {
                            //查询原金额
                            string sqlQuery = "select [JE],[" + yylm + "] from [" + yybm + "] where [Id]=" + glid;
                            DataRow rowParent = SQLiteDao.TranQueryRow(cmd, sqlQuery);
                            if (rowParent != null)
                            {
                                decimal parentJE = DataType.Decimal(rowParent["JE"], 0);
                                decimal parentWCJE = DataType.Decimal(rowParent[yylm], 0);
                                decimal orgJE = DataType.Decimal(orgRow[yylm], 0);
                                //减回原金额
                                parentWCJE -= orgJE;

                                if (operateTable.BKDYYY)
                                {
                                    //不可大于引用
                                    if (operateTable.BKDYYYBL > 0)
                                    {
                                        //不可大于引用比例 如引用金额1000 不可大于引用比例120 则 不可大于 1200
                                        parentJE = parentJE * (operateTable.BKDYYYBL / 100);
                                    }

                                    if (parentWCJE + je > parentJE)
                                    {
                                        msg = "不可大于金额（" + DecimalToString(parentJE - parentWCJE) + "）！";
                                        return false;
                                    }
                                }
                                else if (operateTable.BKXYYY)
                                {
                                    //不可小于引用
                                    if (operateTable.BKXYYYBL > 0)
                                    {
                                        //不可小于引用比例 如引用金额1000 不可大于引用比例120 则 不可小于 1200
                                        parentJE = parentJE * (operateTable.BKXYYYBL / 100);
                                    }

                                    if (parentWCJE + je < parentJE)
                                    {
                                        msg = "不可小于金额（" + DecimalToString(parentJE - parentWCJE) + "）！";
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 4、不可大于库存
                if (operateTable.BKDYKC)
                {
                    if (operateTable.Cells.Exists(p => p.CellName.Equals("SPID")) &&
                        operateTable.Cells.Exists(p => p.CellName.Equals("SL")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("SPID")) &&
                        param.OpreateCells.Exists(p => p.Key.Equals("SL")))
                    {
                        long spid = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("SPID")).Value, 0);
                        decimal sl = DataType.Long(param.OpreateCells.Find(p => p.Key.Equals("SL")).Value, 0);
                        string kcbm = !string.IsNullOrWhiteSpace(operateTable.SPStockTableName) ? operateTable.SPStockTableName : string.Empty;

                        if (!string.IsNullOrWhiteSpace(kcbm) && spid > 0)
                        {
                            //查询原库存
                            string sqlQuery = "select [KCSL] from [" + kcbm + "] where [SPID]=" + spid;
                            object objQueryResult = SQLiteDao.TranExecuteScalar(cmd, sqlQuery);
                            decimal currKCSL = DataType.Decimal(objQueryResult, 0);
                            decimal orgSL = DataType.Decimal(orgRow["SL"], 0);

                            if (operateTable.KDAddStockCount)
                            {
                                //开单加库存数量 则要减去原数量
                                currKCSL -= orgSL;
                            }
                            else if (operateTable.KDReduceStockCount)
                            {
                                //开单减库存数量 则要加回原数量
                                currKCSL += orgSL;
                            }

                            //是否大于库存数量
                            if (sl > currKCSL)
                            {
                                msg = "大于库存数量（" + DecimalToString(currKCSL) + "）！";
                                return false;
                            }
                        }
                    }
                }
                #endregion
                #endregion
                //===============================
                //保存前判断 END
                //===============================


                //===============================
                //更新保存 END
                //===============================
                //更新SQL
                sqlUpdate = "update [" + operateTable.TableName + "] set " + sqlUpdateCells + " where [Id]=" + updateId;
                bool flagUpdated = SQLiteDao.TranExecute(cmd, sqlUpdate, ps.ToArray());
                if (!flagUpdated) throw new Exception("更新数据失败！");

                //更新后的行
                newRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + operateTable.TableName + "] where [Id]=" + updateId);
                if (newRow == null) throw new Exception("获取更新后的数据失败");

                //更新时间
                if (newRow.Table.Columns.Contains("UpdateDate"))
                {
                    SQLiteDao.TranExecute(cmd, "update [" + operateTable.TableName + "] set [UpdateDate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where [Id]=" + updateId);
                }
                //===============================
                //更新保存 END
                //===============================


                if (operateTable.TableName.Equals("Sys_Tables"))
                {
                    //表维护
                    UpdateTableConfig(cmd, topRow, orgRow, newRow, ref msg);
                }
                else if (operateTable.TableName.Equals("Sys_TableCells"))
                {
                    //列维护
                    //取消变更列名  
                    UpdateTableCellsConfig(cmd, operateTable, topRow, orgRow, newRow, false);
                }

                //执行公式
                bool flagJSGS = ProcessGS(cmd, operateTable, updateId, tranId);
                if (flagJSGS)
                {
                    //执行公式后的行
                    newRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + operateTable.TableName + "] where [Id]=" + updateId);
                    if (newRow == null) throw new Exception("获取执行公式后的数据失败");
                }

                //同步商品信息表、商品库存
                SyncSPXX2SPKC(cmd, tableInfo, orgRow, newRow, tranId);

                //开单处理应收应付
                KDProcessYSYFK(cmd, tableInfo, topRow, newRow, orgRow, tranId);

                //开单处理库存
                KDProcessStock(cmd, operateTable, topRow, newRow, orgRow, tranId);

                //更新完成数量
                UpdateWCSL(Actions.修改, cmd, operateTable, newRow, orgRow, tranId);

                //更新完成金额
                UpdateWCJE(Actions.修改, cmd, operateTable, newRow, orgRow, tranId);

                //汇总统计
                SummaryStatistics(cmd, tableInfo, operateTable, topRow, orgRow, newRow, tranId);

                //更新被引用数量
                UpdateSYSBYYSL(cmd, operateTable, newRow, orgRow);

                //保存后执行事件
                RunTableEvent(cmd, operateTable, orgRow, newRow, EventActions.保存, ref msg, false);
            }

            return true;
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <returns></returns>
        public static bool Delete(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref string msg)
        {
            long parentId = 0;
            return Delete(userInfo, tableInfo, param, ref parentId, ref msg);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <returns></returns>
        public static bool Delete(UserInfo userInfo, TableInfo tableInfo, SQLParam param, ref long parentId, ref string msg)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            try
            {
                //===========================================================================================
                //操作表
                TableInfo operateTable = tableInfo;
                //是否有主表
                bool hasTop = false;
                //主表行
                DataRow topRow = null;

                //事务ID
                string tranId = "SW_SC_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

                //判断操作的表名是否子表
                if (tableInfo.SubTable != null && tableInfo.SubTable.TableName.Equals(param.TableName))
                {
                    hasTop = true;
                    operateTable = tableInfo.SubTable;
                    operateTable.ParentTableName = tableInfo.TableName;

                    //是否有上级Id
                    if (!param.Wheres.Exists(p => p.CellName.Equals("ParentId")))
                    {
                        //是从表，但是没有上级Id
                        throw new Exception("删除从表数据缺少参数：ParentId");
                    }

                    //父级Id
                    parentId = DataType.Long(param.Wheres.Find(p => p.CellName.Equals("ParentId")).CellValue, 0);
                    if (parentId <= 0) throw new Exception("删除从表数据缺少参数：ParentId");

                    //主表行
                    topRow = SQLiteDao.TranQueryRow(cmd, "select * from [" + tableInfo.TableName + "] where [Id]=" + parentId);
                    if (topRow == null) throw new Exception("删除从表数据缺少主表记录");

                    //是否有审核列
                    if (topRow.Table.Columns.Contains("IsAudit"))
                    {
                        //是否已经审核
                        if (DataType.Bool(topRow["IsAudit"], false)) throw new Exception("主表已审核无法删除");
                    }
                }

                //获取原数据
                string whereSql = string.Empty;
                List<SQLiteParameter> psWhere = BuildDeleteSQLAndParameters(param, ref whereSql);
                string sqlQueryOrgData = "select * from [" + operateTable.TableName + "] " + whereSql;
                DataTable dtOrgData = SQLiteDao.TranQueryTable(cmd, sqlQueryOrgData, psWhere.ToArray());
                if (dtOrgData == null || dtOrgData.Rows.Count <= 0)
                {
                    msg = "要删除的数据不存在或已删除！";
                    return false;
                }

                //===========================================================================================

                //遍历删除所有
                foreach (DataRow row in dtOrgData.Rows)
                {
                    //删除行
                    bool flagDelete = DeleteDataRow(cmd, userInfo, tableInfo, operateTable, param, topRow, row, ref msg, tranId);
                    if (!flagDelete) throw new Exception("删除数据出错");
                }

                //三表更新上级成本合计
                if (tableInfo.TableType == TableType.三表 && parentId > 0)
                {
                    try
                    {
                        string sqlUpdateCBHJ = "update [" + tableInfo.ParentTableName + "] set [CBHJ]=(select ifnull(sum(ifnull(JE,0)),0) from [" + tableInfo.TableName + "] where [ParentId]=" + parentId + ") where [Id]=" + parentId;
                        cmd.CommandType = CommandType.Text;
                        SQLiteDao.TranExecute(cmd, sqlUpdateCBHJ);
                    }
                    catch (Exception ex) { }
                }

                //提交事务
                SQLiteDao.TranCommit(cmd);
                return true;
            }
            catch (Exception ex)
            {
                //回溯事务
                SQLiteDao.TranRollback(cmd);

                //添加异常
                AppLog.WriteBugLog(ex, "执行删除事务异常");
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = "执行删除事务失败";
            return false;
        }
        /// <summary>
        /// 删除数据行
        /// </summary>
        /// <returns></returns>
        private static bool DeleteDataRow(SQLiteCommand cmd, UserInfo user, TableInfo tableInfo, TableInfo operateTable, SQLParam param, DataRow topRow, DataRow orgRow, ref string msg, string tranId)
        {
            #region 判断是否可删除
            //判断是否可删除
            if (orgRow.Table.Columns.Contains("SYSBYYSL"))
            {
                //是否有被引用数量
                if (DataType.Decimal(orgRow["SYSBYYSL"], 0) != 0)
                {
                    msg = "有被引用数量不可删除";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("WCSL"))
            {
                //是否有完成数量
                if (DataType.Decimal(orgRow["WCSL"], 0) != 0)
                {
                    msg = "有完成数量不可删除";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("WCJE"))
            {
                //是否有完成金额
                if (DataType.Decimal(orgRow["WCJE"], 0) != 0)
                {
                    msg = "有完成金额不可删除";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("IsAudit"))
            {
                //是否已经审核
                if (DataType.Bool(orgRow["IsAudit"], false))
                {
                    msg = "已审核不可删除";
                    return false;
                }
            }
            if (orgRow.Table.Columns.Contains("IsLock"))
            {
                //是否已经锁定
                if (DataType.Bool(orgRow["IsLock"], false))
                {
                    msg = "已锁定不可删除";
                    return false;
                }
            }

            //有从表 判断是否从表有数据
            if (operateTable.SubTable != null && operateTable.IsNotSystemTable)
            {
                object objCount = SQLiteDao.TranExecuteScalar(cmd, "select count(*) from [" + operateTable.SubTable.TableName + "] where [ParentId]=" + orgRow["Id"]);
                if (DataType.Int(objCount, 0) > 0)
                {
                    msg = "子表有数据不能删除";
                    return false;
                }
            }

            //有三表 判断三表 是否被引用或有完成数量
            if (operateTable.ThreeTable != null)
            {
                string sqlQueryThreeTable = "";
                if (operateTable.ThreeTable.Cells.Exists(p => p.CellName == "WCSL"))
                {
                    sqlQueryThreeTable += "+sum(ifnull(WCSL,0))";
                }
                if (operateTable.ThreeTable.Cells.Exists(p => p.CellName == "SYSBYYSL"))
                {
                    sqlQueryThreeTable += "+sum(ifnull(SYSBYYSL,0))";
                }

                if (!string.IsNullOrWhiteSpace(sqlQueryThreeTable))
                {
                    sqlQueryThreeTable = "select " + sqlQueryThreeTable.Trim('+') + " from [" + operateTable.ThreeTable.TableName + "] where [ParentId]=" + orgRow["Id"];
                    object objCount = SQLiteDao.TranExecuteScalar(cmd, sqlQueryThreeTable);
                    if (DataType.Int(objCount, 0) > 0)
                    {
                        msg = "扩展表被引用不能删除";
                        return false;
                    }
                }
            }

            #endregion

            //===============================
            //删除前
            //===============================
            //删除前执行事件
            bool flagBeginEvent = RunTableEvent(cmd, operateTable, orgRow, null, EventActions.删除, ref msg, true);
            if (!flagBeginEvent)
            {
                throw new Exception("删除前执行事件失败，消息：" + msg);
            }

            //===============================
            //删除数据
            //===============================

            //命令模式
            cmd.CommandType = CommandType.Text;

            //删除SQL
            string sqlDelete = "delete from [" + operateTable.TableName + "] where [Id]=" + orgRow["Id"];
            SQLiteDao.TranExecute(cmd, sqlDelete);

            //===============================
            //删除后
            //===============================

            #region 删除主表 处理事务
            if (operateTable != null && operateTable.SubTable != null)
            {
                //删除主表后执行
                lock (_lockDelete)
                {
                    //1、应收应付
                    KDProcessYSYFK(cmd, operateTable, orgRow, null, orgRow, tranId);
                    //2、处理库存
                    KDProcessStock(cmd, operateTable, topRow, null, orgRow, tranId);
                    //3、完成数量
                    UpdateWCSL(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                    //4、完成金额
                    UpdateWCJE(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                }
            }
            #endregion

            #region 删除子表 处理事务
            if (topRow != null && operateTable.TableType == TableType.双表 && operateTable.SubTable == null)
            {
                lock (_lockDelete)
                {
                    //1、应收应付
                    KDProcessYSYFK(cmd, tableInfo, topRow, null, orgRow, tranId);
                    //2、库存数量
                    KDProcessStock(cmd, operateTable, topRow, null, orgRow, tranId);
                    //3、完成数量
                    UpdateWCSL(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                    //4、完成金额
                    UpdateWCJE(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                }

                if (operateTable.ThreeTable != null)
                {
                    //删除SQL
                    sqlDelete = "delete from [" + operateTable.ThreeTable.TableName + "] where [ParentId]=" + orgRow["Id"];
                    SQLiteDao.TranExecute(cmd, sqlDelete);
                }
            }
            #endregion

            #region 处理单表其它事务
            else if (topRow == null && operateTable != null && operateTable.SubTable == null)
            {
                lock (_lockDelete)
                {
                    //同步商品信息表、商品库存
                    SyncSPXX2SPKC(cmd, tableInfo, orgRow, null, tranId);
                    //1、应收应付
                    KDProcessYSYFK(cmd, operateTable, orgRow, null, orgRow, tranId);
                    //2、库存数量
                    KDProcessStock(cmd, operateTable, topRow, null, orgRow, tranId);
                    //3、完成数量
                    UpdateWCSL(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                    //4、完成金额
                    UpdateWCJE(Actions.删除, cmd, operateTable, null, orgRow, tranId);
                }
            }
            #endregion

            lock (_lockDelete)
            {
                //汇总统计
                SummaryStatistics(cmd, tableInfo, operateTable, topRow, orgRow, null, tranId);

                //更新被引用数量
                UpdateSYSBYYSL(cmd, operateTable, null, orgRow);

                #region 表维护
                //是否删除数据表
                if (operateTable.TableName.Equals("Sys_Tables"))
                {
                    string deleteTableName = orgRow["TableName"].ToString();
                    bool isBuild = DataType.Bool(orgRow["IsBuild"], true);
                    bool isSystem = DataType.Bool(orgRow["IsSystem"], true);
                    string type = orgRow["Type"].ToString();
                    string subType = orgRow["SubType"].ToString();

                    //不能删除系统表 及 无法删除未创建的表
                    if (!string.IsNullOrWhiteSpace(deleteTableName) && !deleteTableName.StartsWith("Sys_") && isBuild && !isSystem)
                    {
                        //删除数据表
                        string sqlDeleteTable = "drop table [" + deleteTableName + "]";

                        //执行删除
                        SQLiteDao.TranExecute(cmd, sqlDeleteTable);
                    }
                }
                else if (operateTable.TableName.Equals("Sys_TableCells"))
                {
                    //查询表配置
                    long tableId = DataType.Long(orgRow["ParentId"], 0);
                    string sqlQueryTable = "select * from [Sys_Tables] where [Id]=" + tableId;

                    //查询表配置
                    if (topRow != null)
                    {
                        //视图及虚拟表类型 不需要删除
                        string deleteTableType = topRow["Type"].ToString();
                        if (!deleteTableType.Equals("视图") && !deleteTableType.Equals("虚拟"))
                        {
                            //是否创建表
                            bool isBuild = DataType.Bool(topRow["IsBuild"], false);
                            if (isBuild)
                            {
                                //操作的表名
                                string deleteTableName = topRow["TableName"].ToString();
                                //列名
                                string deleteCellName = orgRow["CellName"].ToString();

                                //注意：已经取消删除列。
                                //当前使用SQLite版本：SQLite3，此版本中并没有提供直接更改列名与删除列的命令，变更或删除相对较复杂会对其它功能产生影响，所以默认不可删除列。

                                //删除列SQL
                                //string sqlDropColumn = @"alter table [" + deleteTableName + "] drop column [" + deleteCellName + @"]";
                                //删除列
                                //SQLiteDao.TranExecute(cmd, sqlDropColumn);
                            }
                        }
                    }
                }
                #endregion

                //删除后执行事件
                RunTableEvent(cmd, operateTable, orgRow, null, EventActions.删除, ref msg, false);
            }

            //删除成功
            return true;
        }
        #endregion

        #region  审核
        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        public static bool Audit(UserInfo userInfo, TableInfo tableInfo, long id, ref string msg)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            //事务ID
            string tranId = "SW_SH_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            try
            {
                //获取审核记录
                string sql = "select * from [" + tableInfo.TableName + "] where [Id]=" + id;
                DataRow topRow = SQLiteDao.TranQueryRow(cmd, sql);
                if (topRow == null)
                {
                    msg = "审核记录不存在或已删除";
                    return false;
                }

                if (!topRow.Table.Columns.Contains("IsAudit"))
                {
                    msg = "当前记录不包含审核列";
                    return false;
                }

                bool isAudit = DataType.Bool(topRow["IsAudit"], false);
                if (isAudit)
                {
                    msg = "该记录已审核，无需再审！";
                    return false;
                }

                //审核前执行事件
                bool flagBeginEvent = RunTableEvent(cmd, tableInfo, topRow, null, EventActions.审核, ref msg, true);
                if (!flagBeginEvent)
                {
                    throw new Exception("审核前执行事件失败，消息：" + msg);
                }
                //==========================================================================================

                //更新审核
                sql = "update [" + tableInfo.TableName + "] set [IsAudit]=1,[AuditUserId]=" + userInfo.UserId + ",[AuditUserName]='" + userInfo.UserName + "',[AuditDate]='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where [Id]=" + id;
                SQLiteDao.TranExecute(cmd, sql);

                //==========================================================================================
                //表维护创建表
                if (tableInfo.TableName.Equals("Sys_Tables"))
                {
                    BuildDataTable(cmd, topRow);
                }

                //审核处理库存
                SHProcessStock(cmd, tableInfo, topRow, true, tranId);

                //审核处理应收应付
                SHProcessYSYFK(cmd, tableInfo, topRow, true, tranId);

                //审核后执行事件
                RunTableEvent(cmd, tableInfo, topRow, null, EventActions.审核, ref msg, false);

                //提交事务
                SQLiteDao.TranCommit(cmd);
                return true;
            }
            catch (Exception ex)
            {
                //回溯事务
                SQLiteDao.TranRollback(cmd);

                //添加异常
                AppLog.WriteBugLog(ex, "执行审核事务异常");
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = "执行审核事务失败";
            return false;
        }
        #endregion

        #region 反审
        /// <summary>
        /// 反审
        /// </summary>
        /// <returns></returns>
        public static bool CancelAudit(UserInfo userInfo, TableInfo tableInfo, long id, ref string msg)
        {
            SQLiteCommand cmd = SQLiteDao.GetSQLiteCmd();

            //事务ID
            string tranId = "SW_FS_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            try
            {
                //获取反审记录
                string sql = "select * from [" + tableInfo.TableName + "] where [Id]=" + id;
                DataRow topRow = SQLiteDao.TranQueryRow(cmd, sql);
                if (topRow == null)
                {
                    msg = "反审记录不存在或已删除";
                    return false;
                }

                if (!topRow.Table.Columns.Contains("IsAudit"))
                {
                    msg = "当前记录不包含审核列";
                    return false;
                }

                bool isAudit = DataType.Bool(topRow["IsAudit"], false);
                if (!isAudit)
                {
                    msg = "该记录未审核，无需反审！";
                    return false;
                }

                //反审前执行事件
                bool flagBeginEvent = RunTableEvent(cmd, tableInfo, topRow, null, EventActions.反审, ref msg, true);
                if (!flagBeginEvent)
                {
                    throw new Exception("审核前执行事件失败，消息：" + msg);
                }
                //==========================================================================================

                //更新审核
                sql = "update [" + tableInfo.TableName + "] set [IsAudit]=0,[AuditUserId]=null,[AuditUserName]=null,[AuditDate]=null where [Id]=" + id;
                SQLiteDao.TranExecute(cmd, sql);

                //==========================================================================================

                //审核处理库存
                SHProcessStock(cmd, tableInfo, topRow, false, tranId);

                //审核处理应收应付
                SHProcessYSYFK(cmd, tableInfo, topRow, false, tranId);

                //反审后执行事件
                RunTableEvent(cmd, tableInfo, topRow, null, EventActions.反审, ref msg, false);

                //提交事务
                SQLiteDao.TranCommit(cmd);
                return true;
            }
            catch (Exception ex)
            {
                //回溯事务
                SQLiteDao.TranRollback(cmd);

                //添加异常
                AppLog.WriteBugLog(ex, "执行反审事务异常");
            }

            if (string.IsNullOrWhiteSpace(msg)) msg = "执行反审事务失败";
            return false;
        }
        #endregion

        #region 辅助 参数
        /// <summary>
        /// 设置参数DBNull值
        /// </summary>
        private static void SetParamValue(List<KeyValue> kvs)
        {
            if (kvs == null || kvs.Count <= 0) return;

            foreach (KeyValue item in kvs)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                }
            }
        }
        /// <summary>
        /// 生成插入SQL&参数
        /// </summary>
        /// <returns></returns>
        private static List<SQLiteParameter> BuildInsertSQLAndParameters(List<KeyValue> kvs, ref string sql)
        {
            string cells = string.Empty;
            string values = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            if (kvs != null && kvs.Count > 0)
            {
                foreach (KeyValue kv in kvs)
                {
                    //过滤列插入
                    if (kv.Key.ToUpper().Equals("ID")) continue;

                    //空
                    if (kv.Value == null) continue;

                    //列
                    cells += "[" + kv.Key + "],";

                    //值
                    values += "@" + kv.Key + ",";

                    //是否需要加密
                    kv.Value = IsNeedEncrypt(kv.Value);

                    //参数
                    ps.Add(new SQLiteParameter("@" + kv.Key, kv.Value));
                }
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
        /// <returns></returns>
        private static List<SQLiteParameter> BuildUpdateSQLAndParameters(SQLParam param, ref string cellsValues, ref string whereStr)
        {
            string where = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //取得所有对象
            foreach (KeyValue kv in param.OpreateCells)
            {
                //ID列禁止生成参数
                if (kv.Key.ToUpper().Equals("ID") || kv.Key.ToUpper().Equals("CREATEDATE")) continue;

                if (kv.Value == null)
                {
                    //空
                    cellsValues += "[" + kv.Key + "]=null,";
                    continue;
                }

                //SQL
                cellsValues += "[" + kv.Key + "]=@" + kv.Key + ",";

                //是否需要加密
                kv.Value = IsNeedEncrypt(kv.Value);

                //参数值
                ps.Add(new SQLiteParameter("@" + kv.Key, kv.Value));
            }


            if (param.Id > 0)
            {
                //根据主键修改
                where = "[Id]=@Id";
                ps.Add(new SQLiteParameter("@Id", param.Id));
            }
            else if (param.Wheres != null && param.Wheres.Count > 0)
            {
                //根据条件修改
                BuildWhere(param.Wheres, ref where, ref ps);
            }

            cellsValues = cellsValues.Trim(',').Trim();
            whereStr = where;

            return ps;
        }
        /// <summary>
        /// 生成删除SQL&参数
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static List<SQLiteParameter> BuildDeleteSQLAndParameters(SQLParam param, ref string whereStr)
        {
            string where = string.Empty;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //删除条件
            if (param.Id > 0)
            {
                where = "[Id]=@Id";
                ps.Add(new SQLiteParameter("@Id", param.Id));
            }
            else if (!string.IsNullOrWhiteSpace(param.Ids))
            {
                if (param.Ids.IndexOf(',') > 0)
                {
                    where = string.Format("[Id] in ({0})", param.Ids);
                }
                else
                {
                    where = " [Id]=" + param.Ids;
                }
            }
            else if (param.Wheres != null && param.Wheres.Count > 0)
            {
                BuildWhere(param.Wheres, ref where, ref ps);
            }

            if (!string.IsNullOrWhiteSpace(where)) where = " where " + where;

            whereStr = where;

            return ps;
        }
        /// <summary>
        /// 生成查询条件
        /// </summary>
        /// <param name="wheres"></param>
        /// <param name="where"></param>
        /// <param name="ps"></param>
        private static void BuildWhere(List<Where> wheres, ref string where, ref List<SQLiteParameter> ps)
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
                    isLeftBracketFirst = false;
                }
                else
                {
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

                //索引
                int parameterIndex = wheres.IndexOf(w);

                //查询参数
                SQLiteParameter p = new SQLiteParameter("@" + w.CellName + parameterIndex, w.CellValue);

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
        /// 是否需要加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object IsNeedEncrypt(object value)
        {
            if (value is string)
            {
                //值
                string val = value.ToString();

                //是否需要加密
                if (val.StartsWith("MZEncoding_"))
                {
                    val = val.Replace("MZEncoding_", "");
                    val = EncryptionDES.Decrypt(val);
                    val = EncryptionAES.Encrypt(val);
                    return "MZEncrypt_" + val;
                }
            }

            return value;
        }
        #endregion

        #region 辅助 统计
        /// <summary>
        /// 汇总统计
        /// </summary>
        private static void SummaryStatistics(long parentId, TableInfo tableInfo, TableInfo operateTable)
        {
            try
            {
                //上级ID
                if (parentId <= 0) return;

                //数量列
                CellInfo slCell = operateTable.Cells.Find(p => p.CellName.Equals("SL"));
                //金额列
                CellInfo jeCell = operateTable.Cells.Find(p => p.CellName.Equals("JE"));

                //即没有数量列 也没有金额列 无需统计
                if (slCell == null && jeCell == null) return;

                string sql = "";

                //统计数量
                if (tableInfo.Cells.Exists(p => p.CellName.Equals("ZSL")) && slCell != null)
                {
                    sql = "update [" + tableInfo.TableName + "] set [ZSL]=(select sum(SL) from [" + operateTable.TableName + "] where [ParentId]=" + parentId + ") where [Id]=" + parentId + ";";
                }

                //统计金额
                if (tableInfo.Cells.Exists(p => p.CellName.Equals("ZJE")) && jeCell != null)
                {
                    sql += "update [" + tableInfo.TableName + "] set [ZJE]=(select sum(JE) from [" + operateTable.TableName + "] where [ParentId]=" + parentId + ") where [Id]=" + parentId + ";";
                }

                SQLiteDao.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "更新总数量、总金额异常");
            }
        }
        /// <summary>
        /// 汇总统计
        /// </summary>
        private static void SummaryStatistics(SQLiteCommand cmd, TableInfo tableInfo, TableInfo operateTable, DataRow topRow, DataRow row, string tranId)
        {
            //上级行
            if (topRow == null) return;

            //上级Id列
            CellInfo parentIdCell = operateTable.Cells.Find(p => p.CellName.Equals("ParentId"));
            if (parentIdCell == null) return;

            //数量列
            CellInfo slCell = operateTable.Cells.Find(p => p.CellName.Equals("SL"));
            //金额列
            CellInfo jeCell = operateTable.Cells.Find(p => p.CellName.Equals("JE"));

            //即没有数量列 也没有金额列 无需统计
            if (slCell == null && jeCell == null) return;

            //上级Id
            long parentId = DataType.Long(row["ParentId"], 0);
            if (parentId <= 0) return;

            //命令类型为SQL
            cmd.CommandType = CommandType.Text;

            string sql = "";

            //统计数量
            if (topRow.Table.Columns.Contains("ZSL") && slCell != null)
            {
                sql = "update [" + tableInfo.TableName + "] set ZSL=(select sum(SL) from [" + operateTable.TableName + "] where [ParentId]=" + parentId + ") where [Id]=" + parentId + ";";
            }

            //统计金额
            if (topRow.Table.Columns.Contains("ZJE") && jeCell != null)
            {
                sql += "update [" + tableInfo.TableName + "] set ZJE=(select sum(JE) from [" + operateTable.TableName + "] where [ParentId]=" + parentId + ") where [Id]=" + parentId + ";";
            }

            SQLiteDao.TranExecute(cmd, sql);
        }
        /// <summary>
        /// 汇总统计
        /// </summary>
        private static void SummaryStatistics(SQLiteCommand cmd, TableInfo tableInfo, TableInfo operateTable, DataRow topRow, DataRow orgRow, DataRow newRow, string tranId)
        {
            //上级行
            if (topRow == null) return;

            //上级Id列
            CellInfo parentIdCell = operateTable.Cells.Find(p => p.CellName.Equals("ParentId"));
            if (parentIdCell == null) return;

            //数量列
            CellInfo slCell = operateTable.Cells.Find(p => p.CellName.Equals("SL"));
            //金额列
            CellInfo jeCell = operateTable.Cells.Find(p => p.CellName.Equals("JE"));

            //即没有数量列 也没有金额列 无需统计
            if (slCell == null && jeCell == null) return;

            decimal orgSL = 0;
            decimal orgJE = 0;

            decimal newSL = 0;
            decimal newJE = 0;

            //上级Id
            long parentId = 0;
            if (orgRow != null) parentId = DataType.Long(orgRow["ParentId"], 0);
            else if (newRow != null) parentId = DataType.Long(newRow["ParentId"], 0);
            if (parentId <= 0) return;

            if (orgRow != null && slCell != null)
            {
                orgSL = DataType.Decimal(orgRow["SL"], 0);
            }
            if (orgRow != null && jeCell != null)
            {
                orgJE = DataType.Decimal(orgRow["JE"], 0);
            }

            if (newRow != null && slCell != null)
            {
                newSL = DataType.Decimal(newRow["SL"], 0);
            }
            if (newRow != null && jeCell != null)
            {
                newJE = DataType.Decimal(newRow["JE"], 0);
            }

            decimal sl = -orgSL + newSL;
            decimal je = -orgJE + newJE;

            string sql = "";

            //统计数量
            if (topRow.Table.Columns.Contains("ZSL") && slCell != null)
            {
                sql = "update [" + tableInfo.TableName + "] set [ZSL]=[ZSL]+" + sl + " where [Id]=" + parentId + ";";
            }

            //统计金额
            if (topRow.Table.Columns.Contains("ZJE") && jeCell != null)
            {
                sql += "update [" + tableInfo.TableName + "] set [ZJE]=[ZJE]+" + je + " where [Id]=" + parentId + ";";
            }

            //命令类型为SQL
            cmd.CommandType = CommandType.Text;

            //执行SQL
            SQLiteDao.TranExecute(cmd, sql);
        }
        #endregion

        #region 辅助 函数
        /// <summary>
        /// 去掉多余的0及小数点
        /// </summary>
        /// <param name="deci"></param>
        /// <returns></returns>
        private static string DecimalToString(decimal deci)
        {
            return deci.ToString().TrimEnd('0').TrimEnd('.');
        }
        #endregion

        #region 辅助 计算
        /// <summary>
        /// 执行公式
        /// </summary>
        /// <returns></returns>
        private static bool ProcessGS(SQLiteCommand cmd, TableInfo operateTable, long id, string tranId)
        {
            //是否有表配置
            if (operateTable == null || operateTable.Cells == null) return false;

            //得到有公式的列
            List<CellInfo> cells = operateTable.Cells.Where(p => !string.IsNullOrWhiteSpace(p.Formula)).ToList();
            if (cells == null || cells.Count <= 0) return false;

            //所有有公式的列
            string sqlGS = "";
            foreach (CellInfo cell in cells)
            {
                sqlGS += " update " + operateTable.TableName + " set [" + cell.CellName + "]=" + cell.Formula + " where [Id]=" + id;
            }

            //命令类型为SQL
            cmd.CommandType = CommandType.Text;

            //执行公式
            SQLiteDao.TranExecute(cmd, sqlGS);

            return true;
        }
        /// <summary>
        /// 添加计算单价、数量、金额
        /// </summary>
        /// <param name="param"></param>
        /// <param name="operateTable"></param>
        private static void InsertJSDJSLJE(SQLParam param, TableInfo operateTable)
        {
            CellInfo cellDJ = operateTable.Cells.Find(p => p.CellName.Equals("DJ"));
            CellInfo cellSL = operateTable.Cells.Find(p => p.CellName.Equals("SL"));
            CellInfo cellJE = operateTable.Cells.Find(p => p.CellName.Equals("JE"));

            decimal dj = 0;
            decimal sl = 0;
            decimal je = 0;

            if (cellSL != null)
            {
                //数量                    
                var kvSL = param.OpreateCells.Find(p => p.Key.Equals("SL"));
                if (kvSL != null)
                {
                    sl = DataType.Decimal(kvSL.Value, 0);
                }

                if (!string.IsNullOrWhiteSpace(cellSL.Formula))
                {
                    var objSL = JSGS(cellSL, param.OpreateCells);
                    sl = DataType.Decimal(objSL, 0);
                    kvSL.Value = objSL;
                }
            }
            if (cellDJ != null)
            {
                //单价
                var kvDJ = param.OpreateCells.Find(p => p.Key.Equals("DJ"));
                if (kvDJ != null)
                {
                    dj = DataType.Decimal(kvDJ.Value, 0);
                }
                if (!string.IsNullOrWhiteSpace(cellDJ.Formula))
                {
                    var objDJ = JSGS(cellDJ, param.OpreateCells);
                    dj = DataType.Decimal(objDJ, 0);
                    kvDJ.Value = objDJ;
                }
            }
            if (cellJE != null)
            {
                //统计金额
                Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                MatchCollection mc = regex.Matches(cellJE.Formula);

                var kvJE = param.OpreateCells.Find(p => p.Key.Equals("JE"));

                if (!string.IsNullOrWhiteSpace(cellJE.Formula) && mc.Count > 0)
                {
                    kvJE.Value = JSGS(cellJE, param.OpreateCells);
                }
                else
                {
                    if (cellDJ != null && cellSL != null)
                    {
                        je = dj * sl;
                        if (cellJE.DecimalDigits > 0) je = decimal.Round(je, cellJE.DecimalDigits);
                        if (kvJE != null) kvJE.Value = je;
                    }
                }
            }
        }
        /// <summary>
        /// 更新计算单价、数量、金额
        /// </summary>
        /// <param name="param"></param>
        /// <param name="operateTable"></param>
        private static void UpdateJSDJSLJE(SQLParam param, TableInfo operateTable)
        {
            CellInfo cellDJ = operateTable.Cells.Find(p => p.CellName.Equals("DJ"));
            CellInfo cellSL = operateTable.Cells.Find(p => p.CellName.Equals("SL"));
            CellInfo cellJE = operateTable.Cells.Find(p => p.CellName.Equals("JE"));

            decimal dj = 0;
            decimal sl = 0;
            decimal je = 0;

            if (cellSL != null)
            {
                //数量                    
                var kvSL = param.OpreateCells.Find(p => p.Key.Equals("SL"));
                if (kvSL != null)
                {
                    sl = DataType.Decimal(kvSL.Value, 0);
                }

                if (!string.IsNullOrWhiteSpace(cellSL.Formula))
                {
                    var objSL = JSGS(cellSL, param.OpreateCells);
                    sl = DataType.Decimal(objSL, 0);
                    kvSL.Value = objSL;
                }
            }
            if (cellDJ != null)
            {
                //单价
                var kvDJ = param.OpreateCells.Find(p => p.Key.Equals("DJ"));
                if (kvDJ != null)
                {
                    dj = DataType.Decimal(kvDJ.Value, 0);
                }
                if (!string.IsNullOrWhiteSpace(cellDJ.Formula))
                {
                    var objDJ = JSGS(cellDJ, param.OpreateCells);
                    dj = DataType.Decimal(objDJ, 0);
                    kvDJ.Value = objDJ;
                }
            }
            if (cellJE != null)
            {
                //统计金额
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cellJE.Formula);

                var kvJE = param.OpreateCells.Find(p => p.Key.Equals("JE"));

                if (!string.IsNullOrWhiteSpace(cellJE.Formula) && mc.Count > 0)
                {
                    kvJE.Value = JSGS(cellJE, param.OpreateCells);
                }
                else
                {
                    if (cellDJ != null && cellSL != null)
                    {
                        je = dj * sl;
                        if (cellJE.DecimalDigits > 0) je = decimal.Round(je, cellJE.DecimalDigits);
                        if (kvJE != null) kvJE.Value = je;
                    }
                }
            }
        }
        /// <summary>
        /// 设置默认值
        /// </summary>
        private static void SetDefaultValue(SQLiteCommand cmd, TableInfo operateTable, List<KeyValue> OpreateCells, bool isAdd = true)
        {
            //没有参数 没有表配置
            if (OpreateCells == null || operateTable == null) return;

            foreach (CellInfo cell in operateTable.Cells)
            {
                //列值
                KeyValue kv = OpreateCells.Find(p => p.Key.Equals(cell.CellName));

                if (isAdd)
                {
                    //是否EAN13条码
                    if (cell.IsBarcodeEAN13)
                    {
                        //EAN13条码 生成12位
                        string barcode = "69" + GetSerialNo(cmd, operateTable.Id, 0, SQLiteDao.SerialNoType.流水号, 10);

                        /*
                        计算校验位的步骤如下：
                        1.将最右边一个数位作为“奇数”位，从右向左为每个字符指定奇数 / 偶数位。
                        2.对所有奇数位上的数值求和，将结果乘以3。
                        3.对所有偶数位上的数值求和。
                        4.对第2步和第3步计算的结果求和。
                        5.校验位的数字加上用第4步计算的总和数应该能够被10整除。
                        6.如果第4步计算的总和数能够被10整除，校验位就是“0”（不是10）
                        */

                        //生成13位条码
                        barcode = JSTMJYW(barcode);

                        //赋值
                        if (kv != null) { kv.Value = barcode; }
                        else OpreateCells.Add(new KeyValue(cell.CellName, barcode));
                    }
                    else if (cell.IsBarcodeEAN8)
                    {
                        //EAN8条码 生成7位
                        string barcode = "69" + GetSerialNo(cmd, operateTable.Id, 0, SQLiteDao.SerialNoType.流水号, 5);

                        //生成13位条码 去掉前面5个0 得8位
                        barcode = JSTMJYW("00000" + barcode);
                        barcode = barcode.Substring(5);

                        //赋值
                        if (kv != null) { kv.Value = barcode; }
                        else OpreateCells.Add(new KeyValue(cell.CellName, barcode));
                    }
                }
            }
        }
        /// <summary>
        /// 计算条码校验位
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private static string JSTMJYW(string barcode)
        {
            int a = 0;
            int b = 0;

            bool isA = true;
            foreach (char n in barcode)
            {
                if (isA) a += int.Parse(n.ToString());
                else b += int.Parse(n.ToString());
                isA = !isA;
            }

            int jg = a + (b * 3);
            jg = 10 - (jg % 10);
            jg = jg == 10 ? 0 : jg;

            //校验位
            return barcode += jg.ToString();
        }
        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="kvs"></param>
        private static object JSGS(CellInfo cell, List<KeyValue> kvs)
        {
            Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            MatchCollection mc = regex.Matches(cell.Formula);

            //计算公式
            string jsgs = cell.Formula;

            if (mc != null && mc.Count > 0)
            {
                //公式所有列
                foreach (System.Text.RegularExpressions.Match m in mc)
                {
                    //列名
                    string cellName = m.Value.Trim().Trim('[').Trim(']');
                    string value = "0";

                    KeyValue kv = kvs.Find(p => p.Key.Equals(cellName));
                    if (kv != null) value = kv.Value.ToString();

                    //替换公式的内容
                    jsgs = jsgs.Replace("[" + cellName + "]", value);
                }
            }

            if (!string.IsNullOrWhiteSpace(jsgs))
            {
                //计算值
                return JSGS(jsgs, cell.ValType, cell.DecimalDigits);
            }

            return 0;
        }
        /// <summary>
        /// 计算辅助公式
        /// </summary>
        /// <param name="kvs"></param>
        /// <param name="operateTable"></param>
        private static void CalculationAuxiliaryFormula(List<KeyValue> kvs, TableInfo operateTable)
        {
            //查询列
            Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");

            //是否有键值
            if (kvs == null || kvs.Count <= 0) return;
            //是否有列配置
            if (operateTable == null || operateTable.Cells == null || operateTable.Cells.Count <= 0) return;

            foreach (CellInfo cell in operateTable.Cells)
            {
                //是否有计算公式列
                CellInfo cellGS = operateTable.Cells.Find(p => p.CellName.Equals(cell.CellName + "_JSGS"));
                if (cellGS == null) continue;

                //公式KV
                var kvCellGS = kvs.Find(p => p.Key.Equals(cell.CellName + "_JSGS"));
                if (kvCellGS == null) continue;

                //要赋值的KV
                var kvCell = kvs.Find(p => p.Key.Equals(cell.CellName));

                //计算公式
                var jsgs = kvCellGS.Value.ToString();

                //获取列
                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(jsgs);
                if (mc != null && mc.Count > 0)
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        string value = "0";

                        var kv = kvs.Find(p => p.Key.Equals(cellName));
                        if (kv != null) value = kv.Value.ToString();

                        //替换公式的内容
                        jsgs = jsgs.Replace("[" + cellName + "]", value);
                    }
                }

                if (!string.IsNullOrWhiteSpace(jsgs))
                {
                    //计算公式
                    var dbValue = JSGS(jsgs, cell.ValType, cell.DecimalDigits);

                    if (kvCell == null)
                    {
                        //新添加值
                        kvs.Add(new KeyValue(cell.CellName, dbValue));
                    }
                    else
                    {
                        //得到新值
                        kvCell.Value = dbValue;
                    }
                }
            }
        }
        /// <summary>
        /// 数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetTypeByString(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;

            if (type.IndexOf(',') > 0) return Type.GetType("System.String");

            try
            {
                switch (type.ToLower())
                {
                    case "bool":
                        return Type.GetType("System.Boolean", true, true);
                    case "byte":
                        return Type.GetType("System.Byte", true, true);
                    case "sbyte":
                        return Type.GetType("System.SByte", true, true);
                    case "char":
                        return Type.GetType("System.Char", true, true);
                    case "decimal":
                        return Type.GetType("System.Decimal", true, true);
                    case "double":
                        return Type.GetType("System.Double", true, true);
                    case "float":
                        return Type.GetType("System.Single", true, true);
                    case "int":
                        return Type.GetType("System.Int32", true, true);
                    case "uint":
                        return Type.GetType("System.UInt32", true, true);
                    case "long":
                        return Type.GetType("System.Int64", true, true);
                    case "ulong":
                        return Type.GetType("System.UInt64", true, true);
                    case "object":
                        return Type.GetType("System.Object", true, true);
                    case "short":
                        return Type.GetType("System.Int16", true, true);
                    case "ushort":
                        return Type.GetType("System.UInt16", true, true);
                    case "string":
                        return Type.GetType("System.String", true, true);
                    case "date":
                    case "datetime":
                        return Type.GetType("System.DateTime", true, true);
                    case "guid":
                        return Type.GetType("System.Guid", true, true);
                    default:
                        return Type.GetType(type, true, true);
                }
            }
            catch { }

            return Type.GetType("System.String", true, true);
        }

        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="jsgs">公式 如：100*100</param>
        /// <param name="type">类型 如：typeof(float)</param>
        /// <param name="decimalDigits">小数位数</param>
        /// <returns></returns>
        private static object JSGS(string jsgs, Type type = null, int decimalDigits = 0)
        {
            string jsgsType = "";

            var dbValue = new NCalc.Expression(jsgs).Evaluate();

            Type[] types = { typeof(float), typeof(double), typeof(decimal) };
            if (type != null && types.Contains(type))
            {
                //是否数字、且有小数位数
                if (type != null && decimalDigits > 0)
                {
                    if (type == typeof(float))
                    {
                        //浮点小数
                        float fValue = DataType.Float(dbValue, 0);
                        dbValue = Math.Round(fValue, decimalDigits, MidpointRounding.AwayFromZero);
                    }
                    else if (type == typeof(double))
                    {
                        //双精度浮点小数
                        double dValue = DataType.Double(dbValue, 0);
                        dbValue = Math.Round(dValue, decimalDigits, MidpointRounding.AwayFromZero);
                    }
                    else if (type == typeof(decimal))
                    {
                        //decimal
                        decimal dValue = DataType.Decimal(dbValue, 0);
                        dbValue = Math.Round(dValue, decimalDigits, MidpointRounding.AwayFromZero);
                    }
                }
            }

            //返回值
            return dbValue;
        }
        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="jsgs">公式 如：100*100</param>
        /// <param name="typeString">类型 如：float</param>
        /// <param name="decimalDigits">小数位数</param>
        /// <returns></returns>
        private static object JSGS(string jsgs, string typeString, int decimalDigits = 0)
        {
            //列类型
            Type type = null;

            if (!string.IsNullOrWhiteSpace(typeString))
            {
                //是否包含此类型
                string[] types = { "int", "float", "double", "decimal" };
                if (types.Contains(typeString.ToString()))
                {
                    //列类型转换
                    type = GetTypeByString(typeString);
                }
            }

            //执行计算
            return JSGS(jsgs, type, decimalDigits);
        }
        #endregion

        #region 辅助 序列号
        /// <summary>
        /// 生成序列号
        /// </summary>
        private static void BuildSerialNo(SQLiteCommand cmd, TableInfo operateTable, DataRow row, long resultId, string tranId)
        {
            //是否有序列号列
            foreach (CellInfo cell in operateTable.Cells)
            {
                if (!string.IsNullOrWhiteSpace(cell.SerialNoType) && !cell.SerialNoType.Equals("Null"))
                {
                    long relativeId = 0;
                    string prefix = cell.SerialNoFormat;
                    int serialNoLength = 4;
                    SQLiteDao.SerialNoType subType = SQLiteDao.SerialNoType.流水号;

                    //Null,全表,供应商,客户,商品,年号,月号,日号
                    if (cell.SerialNoType == "供应商" || cell.SerialNoType == "客户")
                    {
                        if (row.Table.Columns.Contains("KHID"))
                        {
                            relativeId = DataType.Long(row["KHID"], 0);
                        }
                    }
                    else if (cell.SerialNoType == "商品")
                    {
                        if (row.Table.Columns.Contains("SPID"))
                        {
                            relativeId = DataType.Long(row["SPID"], 0);
                        }
                    }
                    else if (cell.SerialNoType == "年号")
                    {
                        subType = SQLiteDao.SerialNoType.年号;
                    }
                    else if (cell.SerialNoType == "月号")
                    {
                        subType = SQLiteDao.SerialNoType.月号;
                    }
                    else if (cell.SerialNoType == "日号")
                    {
                        subType = SQLiteDao.SerialNoType.日号;
                    }
                    else if (cell.SerialNoType == "年全日期")
                    {
                        subType = SQLiteDao.SerialNoType.年全日期;
                    }
                    else if (cell.SerialNoType == "月全日期")
                    {
                        subType = SQLiteDao.SerialNoType.月全日期;
                    }

                    //获取序列号
                    string serialNo = SQLiteDao.TranGetSerialNo(cmd, operateTable.Id, relativeId, subType, serialNoLength, prefix);

                    //更新序列号
                    string sql = "update [" + operateTable.TableName + "] set [" + cell.CellName + "]=@SeriNo where [Id]=" + resultId;
                    SQLiteParameter[] ps2 =
                    {
                        new SQLiteParameter() { ParameterName="@SeriNo", Value=serialNo }
                    };
                    //命令类型为SQL
                    cmd.CommandType = CommandType.Text;
                    //执行更新序列号
                    SQLiteDao.TranExecute(cmd, sql, ps2);
                }
            }
        }
        /// <summary>
        /// 获取序列号
        /// </summary>
        /// <returns></returns>
        private static string GetSerialNo(SQLiteCommand cmd, long tableId, long relativeId, SQLiteDao.SerialNoType subType, int serialNoLength = 4, string prefix = "")
        {
            return SQLiteDao.TranGetSerialNo(cmd, tableId, relativeId, subType, serialNoLength, prefix);
        }
        #endregion

        #region 辅助 表维护
        /// <summary>
        /// 保存数据表
        /// </summary>
        private static void SaveDataTable(SQLiteCommand cmd, DataRow row, long resultId)
        {
            string type = row["Type"].ToString();
            string subType = row["SubType"].ToString();

            if (type.Equals("单表") || type.Equals("双表") || type.Equals("三表"))
            {
                //表维护 添加默认列
                AddTableDefaultCells(cmd, row, resultId);
            }
            else if (type.Equals("虚拟"))
            {
                //虚拟表
                AddVirtualTableCells(cmd, resultId, row["TableName"].ToString());
            }
            else if (type.Equals("视图"))
            {
                //上级表
                long parentId = DataType.Long(row["ParentId"], 0);

                //视图 不用创建表
                if (parentId <= 0)
                {
                    //添加报表类视图默认列
                    AddViewDefaultCells(cmd, row, resultId);
                }
                else if (subType.Equals("主从视图") && parentId > 0)
                {
                    //添加主从视图默认列
                    AddViewDefaultCells(cmd, row, resultId, parentId);
                }
                else if (subType.Equals("引用视图") && parentId > 0)
                {
                    //添加引用视图默认列
                    AddViewDefaultCells(cmd, row, resultId, parentId);
                }
                else if (subType.Equals("引用扩展视图") && parentId > 0)
                {
                    //添加引用扩展视图默认列
                    AddViewDefaultCells(cmd, row, resultId, parentId);
                }
                else
                {
                    //需要加载主表和明细表所有列
                    //查询关联表的所有列，查询关联表子表的所有列（如果有子表）
                    BuildViewTable(cmd, resultId, parentId);
                }
            }
        }

        /// <summary>
        /// 添加虚拟表的列
        /// </summary>
        private static void AddVirtualTableCells(SQLiteCommand cmd, long resultId, string tableName)
        {
            //查询列维护的列名
            string sql = "select * from [Sys_TableCells] limit 1";
            DataTable dtCells = SQLiteDao.TranQueryTable(cmd, sql);
            if (dtCells == null || dtCells.Rows.Count <= 0 || dtCells.Columns.Count <= 0) return;

            //生成列名
            string cells = "";
            foreach (DataColumn col in dtCells.Columns)
            {
                if (col.ColumnName.Equals("Id")) continue;
                cells += "[" + col.ColumnName + "],";
            }
            cells = cells.Trim(',');

            //是否有列
            if (string.IsNullOrWhiteSpace(cells)) return;

            //插入已有列
            sql = "insert into [Sys_TableCells](" + cells + ") select " + cells.Replace("[ParentId]", resultId.ToString()) + " from [Sys_TableCells] where ParentId=(select Id from [Sys_Tables] where ([Type]='单表' or [Type]='双表' or [Type]='三表') and TableName='" + tableName + "')";
            SQLiteDao.TranExecute(cmd, sql);
        }

        /// <summary>
        /// 添加表的默认列
        /// </summary>
        private static void AddTableDefaultCells(SQLiteCommand cmd, DataRow row, long id)
        {
            //是否有上级表
            long parentId = DataType.Long(row["ParentId"].ToString(), 0);
            string type = row["Type"].ToString();
            string subType = row["SubType"].ToString();
            string defaultCellType = string.Empty;

            //空表
            if (subType.Equals("空表")) return;

            if (type.Equals("双表"))
            {
                if (parentId > 0) defaultCellType = "子表";
                else defaultCellType = "主表";
            }
            else if (type.Equals("三表"))
            {
                defaultCellType = "子表";
            }
            else if (subType.Equals("普通表"))
            {
                defaultCellType = "主表";
            }
            else if (subType.Equals("供应商表") || subType.Equals("客户表"))
            {
                defaultCellType = "客户表";
            }
            else
            {
                defaultCellType = subType;
            }

            //添加默认列
            TableAddDefaultCells(cmd, id, 0, defaultCellType, true);
        }
        /// <summary>
        /// 表添加默认列
        /// </summary>
        private static bool TableAddDefaultCells(SQLiteCommand cmd, long tableId, long parentId, string tableSubType, bool needPublic)
        {
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //得到默认列
            string sql = "select * from [Sys_TableDefaultCells] where [Type]=@Type " + (needPublic ? "or [Type]='通用'" : "") + " order by [Order] asc";
            SQLiteParameter[] ps =
            {
                new SQLiteParameter("@Type",tableSubType)
            };
            DataTable dtDefaultCells = SQLiteDao.TranQueryTable(cmd, sql, ps);
            if (dtDefaultCells == null || dtDefaultCells.Rows.Count <= 0) return false;

            string[] filterCellNames = { "Id", "Type", "CreateDate" };

            //查询表列结构
            sql = "select * from [Sys_TableCells] where [ParentId]=2";
            DataTable dtCells = SQLiteDao.TranQueryTable(cmd, sql);
            if (dtCells == null || dtCells.Rows.Count <= 0) return false;

            //插入SQL
            sql = "insert into [Sys_TableCells]($cells$) values($values$)";

            //循环添加默认列
            foreach (DataRow row in dtDefaultCells.Rows)
            {
                string sqlCells = "[ParentId],";
                string sqlValues = "@ParentId,";
                List<SQLiteParameter> psArray = new List<SQLiteParameter>();
                psArray.Add(new SQLiteParameter("@ParentId", tableId));

                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    //列名
                    string colName = row.Table.Columns[i].ColumnName;

                    //是否过滤的列
                    if (filterCellNames.Contains(colName)) continue;

                    //是否有此列
                    DataRow[] hasRows = dtCells.Select("[CellName]='" + colName + "'");
                    if (hasRows == null || hasRows.Length <= 0) continue;

                    sqlCells += "[" + colName + "],";
                    sqlValues += "@" + colName + ",";
                    psArray.Add(new SQLiteParameter("@" + colName, row[colName]));
                }

                string runSql = sql.Replace("$cells$", sqlCells.Trim(',')).Replace("$values$", sqlValues.Trim(','));
                //命令类型为SQL
                cmd.CommandType = CommandType.Text;
                //事务执行
                SQLiteDao.TranExecute(cmd, runSql, psArray.ToArray());
            }

            //有上级表
            if (parentId > 0)
            {
                //命令类型为SQL
                cmd.CommandType = CommandType.Text;

                //主表
                sql = "select [TableName] from [Sys_Tables] where [Type]='双表' and [Id]=" + parentId;
                object objZB = SQLiteDao.TranExecuteScalar(cmd, sql);
                if (objZB != null)
                {
                    //从表
                    sql = "select * from [Sys_Tables] where [Type]='双表' and [ParentId]=" + parentId;
                    DataTable dtMXTable = SQLiteDao.TranQueryTable(cmd, sql);

                    if (dtMXTable != null && dtMXTable.Rows.Count > 0)
                    {
                        long mxid = DataType.Long(dtMXTable.Rows[0]["Id"], 0);
                        string mxbm = dtMXTable.Rows[0]["TableName"].ToString();

                        sql = "update [Sys_Tables] set [MainTableId]=@MainTableId,[MainTableName]=@MainTableName,[SubTableId]=@SubTableId,[SubTableName]=@SubTableName where [Id]=" + tableId;
                        SQLiteParameter[] ps2 =
                        {
                            new SQLiteParameter() { ParameterName="@MainTableId", Value=parentId },
                            new SQLiteParameter() { ParameterName="@MainTableName", Value=objZB.ToString() },
                            new SQLiteParameter() { ParameterName="@SubTableId", Value=mxid },
                            new SQLiteParameter() { ParameterName="@SubTableName", Value=mxbm },
                        };
                        SQLiteDao.TranExecute(cmd, sql, ps2);
                    }
                }
            }

            //事务执行
            SQLiteDao.TranExecute(cmd, "update [Sys_TableCells] set [DataIndex]=[Id] where [ParentId]=" + tableId);

            return true;
        }
        /// <summary>
        /// 添加视图默认列
        /// </summary>
        private static void AddViewDefaultCells(SQLiteCommand cmd, DataRow row, long id, long parentId = 0)
        {
            //是否有上级表
            string type = row["Type"].ToString();
            if (!type.Equals("视图")) return;
            string subType = row["SubType"].ToString();

            //添加默认列
            TableAddDefaultCells(cmd, id, parentId, subType, false);
        }
        /// <summary>
        /// 创建视图【非创建数据库视图，仅保存到数据表列配置】
        /// </summary>
        /// <param name="viewId">视图编号</param>
        /// <param name="tableId">关联表编号</param>
        /// <returns></returns>
        private static bool BuildViewTable(SQLiteCommand cmd, long viewId, long tableId)
        {
            //没有视图或表编号
            if (viewId <= 0 || tableId <= 0) return false;

            #region 获取主表
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;

            //查询是否有表
            DataTable dt = SQLiteDao.TranQueryTable(cmd, "select * from [Sys_Tables] where [Id]=" + tableId);

            //没有表
            if (dt == null || dt.Rows == null) return false;

            //主表编号
            long mainTableId = DataType.Long(dt.Rows[0]["Id"].ToString(), 0);
            if (mainTableId <= 0) return false;

            //主表名
            string mainTableName = dt.Rows[0]["TableName"].ToString();

            //*************************************************************
            //更新视图配置的主表信息
            string sqlUpdateView = "update [Sys_Tables] set [MainTableId]=@MainTableId,[MainTableName]=@MainTableName where [Id]=" + viewId;
            SQLiteParameter[] psUpdateView =
            {
                new SQLiteParameter("@MainTableId", mainTableId),
                new SQLiteParameter("@MainTableName", mainTableName),
            };
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //执行SQL
            bool flagUpdateTable = SQLiteDao.TranExecute(cmd, sqlUpdateView, psUpdateView);
            if (!flagUpdateTable) return true;
            //*************************************************************
            //得到主表所有列
            DataTable dtMainTableCells = SQLiteDao.TranQueryTable(cmd, "select * from [Sys_TableCells] where [ParentId]=" + tableId);
            //是否有主表列
            if (dtMainTableCells == null || dtMainTableCells.Rows.Count <= 0) return false;

            //添加主表ID
            string sqlAddCell = "insert into [Sys_TableCells](ParentId,CellName,CnName,ValType,IsShow) values(" + viewId + ",'MZZB_Id','主表编号','long',0)";
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //执行SQL
            SQLiteDao.TranExecute(cmd, sqlAddCell);

            //循环主表列
            foreach (DataRow rowMainCell in dtMainTableCells.Rows)
            {
                string cellName = rowMainCell["CellName"].ToString();
                string cnName = rowMainCell["CnName"].ToString();
                string valType = rowMainCell["ValType"].ToString();
                bool isShow = DataType.Bool(rowMainCell["IsShow"], true);
                bool isQuery = DataType.Bool(rowMainCell["IsQuery"], false);
                bool canEdit = DataType.Bool(rowMainCell["CanEdit"], true);
                bool isPopShow = DataType.Bool(rowMainCell["IsPopShow"], true);

                //添加主表所有显示列
                sqlAddCell = "insert into [Sys_TableCells](ParentId,CellName,CnName,ValType,IsShow,IsQuery,CanEdit,IsPopShow) values(@ParentId,@CellName,@CnName,@ValType,@IsShow,@IsQuery,@CanEdit,@IsPopShow)";
                SQLiteParameter[] psAddCell =
                {
                    new SQLiteParameter() {ParameterName="@ParentId", Value=viewId },
                    new SQLiteParameter() {ParameterName="@CellName", Value="MZZB_" + cellName },
                    new SQLiteParameter() {ParameterName="@CnName", Value=cnName },
                    new SQLiteParameter() {ParameterName="@ValType", Value=valType },
                    new SQLiteParameter() {ParameterName="@IsShow", Value=isShow },
                    new SQLiteParameter() {ParameterName="@IsQuery", Value=isQuery },
                    new SQLiteParameter() {ParameterName="@CanEdit", Value=canEdit },
                    new SQLiteParameter() {ParameterName="@IsPopShow", Value=isPopShow }
                };
                //命令类型为SQL
                cmd.CommandType = CommandType.Text;
                //执行SQL
                SQLiteDao.TranExecute(cmd, sqlAddCell, psAddCell);
            }
            #endregion

            #region 获取子表
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //查询是否有子表
            dt = SQLiteDao.TranQueryTable(cmd, "select * from [Sys_Tables] where [ParentId]=" + tableId + " and ([Type]='单表' or [Type]='双表') ");

            //没有子表
            if (dt == null || dt.Rows == null) return false;

            //子表编号
            long subTableId = DataType.Long(dt.Rows[0]["Id"].ToString(), 0);
            if (subTableId <= 0) return false;

            //子表名
            string subTableName = dt.Rows[0]["TableName"].ToString();

            //*************************************************************
            //更新视图配置的子表信息
            sqlUpdateView = "update [Sys_Tables] set [SubTableId]=@SubTableId,[SubTableName]=@SubTableName where [Id]=" + viewId;
            SQLiteParameter[] psUpdateView2 =
            {
                new SQLiteParameter("@SubTableId", subTableId),
                new SQLiteParameter("@SubTableName", subTableName),
            };
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //执行SQL
            flagUpdateTable = SQLiteDao.TranExecute(cmd, sqlUpdateView, psUpdateView2);
            if (!flagUpdateTable) return true;
            //*************************************************************

            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //得到子表所有列
            DataTable dtSubTableCells = SQLiteDao.TranQueryTable(cmd, "select * from [Sys_TableCells] where [ParentId]=" + subTableId);
            //是否有子表列
            if (dtSubTableCells == null || dtSubTableCells.Rows.Count <= 0) return true;
            //添加子表ID
            sqlAddCell = "insert into [Sys_TableCells](ParentId,CellName,CnName,ValType,IsShow) values(" + viewId + ",'MZMX_Id','子表编号','long',0)";
            //命令类型为SQL
            cmd.CommandType = CommandType.Text;
            //执行SQL
            SQLiteDao.TranExecute(cmd, sqlAddCell);

            //循环子表列
            foreach (DataRow rowSubCell in dtSubTableCells.Rows)
            {
                string cellName = rowSubCell["CellName"].ToString();
                string cnName = rowSubCell["CnName"].ToString();
                string valType = rowSubCell["ValType"].ToString();
                bool isShow = DataType.Bool(rowSubCell["IsShow"], true);
                bool isQuery = DataType.Bool(rowSubCell["IsQuery"], false);
                bool canEdit = DataType.Bool(rowSubCell["CanEdit"], true);
                bool isPopShow = DataType.Bool(rowSubCell["IsPopShow"], true);

                //添加子表所有显示列
                sqlAddCell = "insert into [Sys_TableCells](ParentId,CellName,CnName,ValType,IsShow,IsQuery,CanEdit,IsPopShow) values(@ParentId,@CellName,@CnName,@ValType,@IsShow,@IsQuery,@CanEdit,@IsPopShow)";
                SQLiteParameter[] psAddCell =
                {
                    new SQLiteParameter() {ParameterName="@ParentId", Value=viewId },
                    new SQLiteParameter() {ParameterName="@CellName", Value="MZMX_" + cellName },
                    new SQLiteParameter() {ParameterName="@CnName", Value=cnName },
                    new SQLiteParameter() {ParameterName="@ValType", Value=valType },
                    new SQLiteParameter() {ParameterName="@IsShow", Value=isShow },
                    new SQLiteParameter() {ParameterName="@IsQuery", Value=isQuery },
                    new SQLiteParameter() {ParameterName="@CanEdit", Value=canEdit },
                    new SQLiteParameter() {ParameterName="@IsPopShow", Value=isPopShow }
                };
                //命令类型为SQL
                cmd.CommandType = CommandType.Text;
                //执行SQL
                SQLiteDao.TranExecute(cmd, sqlAddCell, psAddCell);
            }
            #endregion

            return true;
        }

        /// <summary>
        /// 创建表
        /// </summary>
        private static void BuildDataTable(SQLiteCommand cmd, DataRow row)
        {
            //已经创建表则退出
            if (DataType.Bool(row["IsBuild"].ToString(), false)) return;

            //SQL
            cmd.CommandType = CommandType.Text;

            //表编号
            long tableId = DataType.Long(row["Id"].ToString(), 0);
            long parentId = DataType.Long(row["ParentId"].ToString(), 0);
            if (tableId <= 0) return;

            //创建表
            string tbName = row["TableName"].ToString();
            string type = row["Type"].ToString();
            string subType = row["SubType"].ToString();

            //非实体表不创建表
            if (!type.Equals("单表") && !type.Equals("双表") && !type.Equals("三表")) return;

            //所有列
            List<CellInfo> cells = new List<CellInfo>();

            //主键(默认列)
            cells.Add(new CellInfo()
            {
                CellName = "Id",
                ValType = "long",
                IsIDentity = true
            });

            //是否有其它列
            DataTable dtCells = SQLiteDao.TranQueryTable(cmd, "select * from [Sys_TableCells] where [ParentId]=" + tableId);
            if (dtCells != null && dtCells.Rows.Count > 0)
            {
                foreach (DataRow colRow in dtCells.Rows)
                {
                    string colName = colRow["CellName"].ToString();
                    string valtype = colRow["ValType"].ToString();

                    //是否存在列
                    int count = cells.Count(p => p.CellName.Equals(colName));
                    if (count > 0) continue;

                    //添加列
                    cells.Add(new CellInfo()
                    {
                        CellName = colName,
                        ValType = valtype,
                        IsIDentity = false
                    });
                }
            }

            //生成列SQL
            string cellSql = string.Empty;

            bool hasIdentity = false;

            //循环所有列
            foreach (CellInfo cell in cells)
            {
                //列SQL
                cellSql += "[" + cell.CellName + "] ";

                //是否主键
                if (!hasIdentity && cell.IsIDentity)
                {
                    //长整型 + 自增加
                    cellSql += " INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,";
                    //数据索引列
                    cellSql += "[DataIndex] INTEGER DEFAULT 0,";
                    hasIdentity = true;
                }
                else
                {
                    //数据类型
                    //string,int,long,float,decimal,bool,date,datetime
                    cellSql += " " + cell.ValType;

                    //是否有默认值
                    if (!string.IsNullOrWhiteSpace(cell.DefaultValue))
                    {
                        switch (cell.ValType)
                        {
                            case "string": cellSql += " DEFAULT N'" + cell.DefaultValue + "'"; break;
                            case "bool": cellSql += " DEFAULT (" + (cell.DefaultValue.ToLower().Equals("false") ? 0 : 1) + ")"; break;
                            default: cellSql += " DEFAULT " + cell.DefaultValue; break;
                        }
                    }

                    //允许为空
                    cellSql += " null,";
                }
            }

            //创建表
            string sql = @"create table [" + tbName + @"]( " + cellSql.Trim(',') + " )";
            cmd.CommandType = CommandType.Text;
            SQLiteDao.TranExecute(cmd, sql);

            //标记表创建成功
            string sqlUpdate = "update [Sys_Tables] set [IsBuild]=1 where [Id]=" + tableId;
            SQLiteDao.TranExecute(cmd, sqlUpdate);
        }

        /// <summary>
        /// 更新表配置
        /// </summary>
        private static void UpdateTableConfig(SQLiteCommand cmd, DataRow topRow, DataRow rowOrg, DataRow rowNew, ref string msg)
        {
            //是否需要操作数据表
            string tableType = rowOrg["Type"].ToString();
            if (tableType.Equals(TableType.视图.ToString())) return;

            //原表
            if (rowOrg == null) return;

            //是否创建表
            bool isBuild = DataType.Bool(rowNew["IsBuild"].ToString(), false);
            if (!isBuild) return;

            //系统表不可更新表名
            bool isSystemTb = DataType.Bool(rowOrg["IsSystem"], false);
            if (isSystemTb) return;

            string orgTBName = rowOrg["TableName"].ToString();
            string tbName = rowNew["TableName"].ToString();

            if (!orgTBName.Equals(tbName))
            {
                //命令类型为SQL
                cmd.CommandType = CommandType.Text;

                //查询是否有此数据表
                bool flag = SQLiteDao.TableIsExists("orgTBName");
                if (!flag) return;

                //更新表名
                string sql = "ALTER TABLE '" + orgTBName + "' RENAME TO '" + tbName + "';";
                SQLiteDao.TranExecute(cmd, sql);
            }
        }
        /// <summary>
        /// 更新表列配置
        /// </summary>
        private static void UpdateTableCellsConfig(SQLiteCommand cmd, TableInfo operateTable, DataRow rowTop, DataRow rowOrg, DataRow rowNew, bool isAdd = false)
        {
            //不是更新表
            if (!operateTable.TableName.Equals("Sys_TableCells")) return;

            //是否已经生成了表
            bool isBuild = DataType.Bool(rowTop["IsBuild"].ToString(), false);
            if (!isBuild) return;

            //是否需要操作数据列
            string tableType = rowTop["Type"].ToString();
            string tableSubType = rowTop["SubType"].ToString();
            string tableName = rowTop["TableName"].ToString();
            if (tableType.Equals(TableType.视图.ToString())) return;

            string cellName = rowNew["CellName"].ToString();
            string valType = rowNew["ValType"].ToString();
            string defaultValue = rowNew["DefaultValue"].ToString();
            bool isSystemCell = DataType.Bool(rowNew["IsSystem"], false);

            //string,int,long,float,decimal,bool,date,datetime
            string valTypeSql = "";
            switch (valType)
            {
                case "bigint":
                case "long": valTypeSql = "bigint"; break;
                case "int": valTypeSql = "int"; break;
                case "float": valTypeSql = "float"; break;
                case "date": valTypeSql = "datetime"; break;
                case "datetime": valTypeSql = "datetime"; break;
                case "money": valTypeSql = "money"; break;
                case "decimal": valTypeSql = "decimal(18, 6)"; break;
                case "bit":
                case "bool": valTypeSql = "bit"; break;
                default: valTypeSql = "nvarchar"; break;
            }

            //命令类型为SQL
            cmd.CommandType = CommandType.Text;

            if (isAdd)
            {
                //添加列
                string sql = "alter table [" + tableName + "] add [" + cellName + "] " + valTypeSql;
                SQLiteDao.TranExecute(cmd, sql);
            }
            else
            {
                //修改列
                //系统列不可修改
                if (isSystemCell) return;

                //原列名
                string orgCellName = rowOrg["CellName"].ToString();
                string orgValType = rowOrg["ValType"].ToString();

                //列名及类型没有变化
                if (orgCellName.Equals(cellName) && orgValType.Equals(valType)) return;

                //注意：已经取消变更列名
                //当前使用SQLite版本：SQLite3，此版本中并没有提供直接更改列名与删除列的命令，变更相对较复杂会对其它功能产生影响，所以默认不可变更列名。
                //DataService.UpdateDataRow 方法中增加了移除变更列名的判断。
                return;
            }
        }
        /// <summary>
        /// 获取列数据类型SQL
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static string GetColumnTypeSQL(DataColumn col, string colName, string colTypeSQL = null)
        {
            if (col.ColumnName == "Id")
            {
                return @"
                             [Id]             INTEGER         PRIMARY KEY AUTOINCREMENT NOT NULL,";
            }

            string sql = "[" + colName + "] ";
            if (!string.IsNullOrWhiteSpace(colTypeSQL))
            {
                //有指定类型列
                return @"
                            " + sql + colTypeSQL + " ,";
            }

            Type dataType = col.DataType;
            if (dataType == typeof(bool))
            {
                sql += " BIT,";
            }
            else if (dataType == typeof(int))
            {
                sql += " INT,";
            }
            else if (dataType == typeof(long))
            {
                sql += " INTEGER,";
            }
            else if (dataType == typeof(decimal))
            {
                sql += " NUMERIC,";
            }
            else if (dataType == typeof(float) || dataType == typeof(double))
            {
                sql += " NUMERIC,";
            }
            else if (dataType == typeof(DateTime))
            {
                sql += " DATETIME,";
            }
            else
            {
                sql += " NVARCHAR,";
            }

            return @"
                            " + sql;
        }
        #endregion

        #region 处理库存、应收应付款
        /// <summary>
        /// 开单处理库存
        /// 添加、修改、删除 主表或明细
        /// </summary>
        /// <param name="orgRow">原数据</param>
        /// <param name="newRow">新的数据</param>
        private static bool KDProcessStock(SQLiteCommand cmd, TableInfo operateTable, DataRow topRow, DataRow newRow, DataRow orgRow, string tranId)
        {
            long newSPID = 0;
            string newSPMC = "";
            string newSPBH = "";
            long orgSPID = 0;
            string orgSPMC = "";
            string orgSPBH = "";
            string countCellName = "SL";

            //是否有数量列
            if (!operateTable.Cells.Exists(p => p.CellName.Equals(countCellName))) return false;
            //是否有商品ID列
            if (!operateTable.Cells.Exists(p => p.CellName.Equals("SPID"))) return false;

            if (newRow != null)
            {
                //没有商品编号列
                if (!newRow.Table.Columns.Contains("SPID") || !newRow.Table.Columns.Contains("SPMC") || !newRow.Table.Columns.Contains("SPBH")) return false;
                //是否有商品主键
                newSPID = DataType.Long(newRow["SPID"], 0);
                if (newSPID <= 0) newSPID = 0;
                //是否有数量列
                if (!newRow.Table.Columns.Contains(countCellName)) return false;

                newSPMC = newRow["SPMC"].ToString();
                newSPBH = newRow["SPBH"].ToString();
            }
            if (orgRow != null)
            {
                //没有商品编号列
                if (!orgRow.Table.Columns.Contains("SPID") || !orgRow.Table.Columns.Contains("SPMC") || !orgRow.Table.Columns.Contains("SPBH")) return false;
                //是否有商品主键
                orgSPID = DataType.Long(orgRow["SPID"], 0);
                if (orgSPID <= 0) orgSPID = 0;
                //是否有数量列
                if (!orgRow.Table.Columns.Contains(countCellName)) return false;

                orgSPMC = orgRow["SPMC"].ToString();
                orgSPBH = orgRow["SPBH"].ToString();
            }

            //没有商品ID
            if (newSPID <= 0 && orgSPID <= 0) return false;

            //旧行数量
            decimal orgCount = orgRow == null ? 0 : DataType.Decimal(orgRow[countCellName], 0);
            //新行数量
            decimal newCount = newRow == null ? 0 : DataType.Decimal(newRow[countCellName], 0);

            //库存没有改变
            if (newSPID == orgSPID && newCount == orgCount) return true;

            if (newSPID == orgSPID)
            {
                //调用开单修改商品库存
                return DataServiceExt.KDUpdateStock(cmd, operateTable, topRow, newRow, newSPID, newSPMC, newSPBH, orgCount, newCount, tranId);
            }
            else if (newSPID > 0 && orgSPID > 0)
            {
                //删除原商品库存
                DataServiceExt.KDUpdateStock(cmd, operateTable, topRow, orgRow, orgSPID, orgSPMC, orgSPBH, orgCount, 0, tranId);
                //添加新的商品库存
                return DataServiceExt.KDUpdateStock(cmd, operateTable, topRow, newRow, newSPID, newSPMC, newSPBH, 0, newCount, tranId);
            }
            else if (orgSPID > 0 && newSPID == 0)
            {
                //删除商品库存
                return DataServiceExt.KDUpdateStock(cmd, operateTable, topRow, orgRow, orgSPID, orgSPMC, orgSPBH, orgCount, 0, tranId);
            }
            else if (orgSPID == 0 && newSPID > 0)
            {
                //添加新的商品库存
                return DataServiceExt.KDUpdateStock(cmd, operateTable, topRow, newRow, newSPID, newSPMC, newSPBH, 0, newCount, tranId);
            }

            return false;
        }
        /// <summary>
        /// 开单处理应收应付款
        /// 添加、修改、删除 主表或明细
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableInfo">如果是主从表 则需要主从表配置一起</param>
        /// <param name="topRow"></param>
        /// <param name="newRow"></param>
        /// <param name="orgRow"></param>
        /// <returns></returns>
        private static bool KDProcessYSYFK(SQLiteCommand cmd, TableInfo tableInfo, DataRow topRow, DataRow newRow, DataRow orgRow, string tranId)
        {
            //没有主表记录
            if (topRow == null) return false;

            //是否有客户Id的列 且有关联表
            if (!tableInfo.Cells.Exists(p => p.CellName.Equals("KHID") && p.ForeignTableId > 0)) return false;
            if (orgRow != null && !orgRow.Table.Columns.Contains("JE")) return false;
            if (newRow != null && !newRow.Table.Columns.Contains("JE")) return false;

            decimal orgMoney = orgRow == null ? 0 : DataType.Decimal(orgRow["JE"], 0);
            decimal newMoney = newRow == null ? 0 : DataType.Decimal(newRow["JE"], 0);

            //相同无需更新
            if (orgMoney == newMoney) return false;

            //客户编号
            long khId = DataType.Long(topRow["KHID"], 0);
            if (khId <= 0) return false;

            //调用开单修改应收应付款
            return DataServiceExt.KDUpdatePayment(cmd, tableInfo, topRow, khId, orgMoney, newMoney, tranId);
        }
        #endregion

        #region 处理完成数量、完成金额
        /// <summary>
        /// 更新完成数量
        /// </summary>
        private static void UpdateWCSL(Actions action, SQLiteCommand cmd, TableInfo tableInfo, DataRow rowNew, DataRow rowOrg, string tranId)
        {
            //没有需要更新的完成数量表
            string tbName = tableInfo.WCSLTableName;
            string slCellName = "SL";
            string wcslCellName = "WCSL";

            if (rowNew != null)
            {
                if (string.IsNullOrWhiteSpace(tbName) && !rowNew.Table.Columns.Contains("WCSLBM")) return;
                if (!rowNew.Table.Columns.Contains(slCellName) || !rowNew.Table.Columns.Contains("GLID")) return;
            }
            else if (rowOrg != null)
            {
                if (string.IsNullOrWhiteSpace(tbName) && !rowOrg.Table.Columns.Contains("WCSLBM")) return;
                if (!rowOrg.Table.Columns.Contains(slCellName) || !rowOrg.Table.Columns.Contains("GLID")) return;
            }
            else
            {
                return;
            }

            decimal newSL = 0;
            decimal orgSL = 0;

            long newGLID = 0;
            long orgGLID = 0;

            if (rowNew != null)
            {
                newSL = DataType.Decimal(rowNew[slCellName], 0);
                newGLID = DataType.Long(rowNew["GLID"], 0);
                if (rowNew.Table.Columns.Contains("WCSLGXID")) newGLID = DataType.Long(rowNew["WCSLGXID"], 0);
            }
            if (rowOrg != null)
            {
                orgSL = DataType.Decimal(rowOrg[slCellName], 0);
                orgGLID = DataType.Long(rowOrg["GLID"], 0);
                if (rowOrg.Table.Columns.Contains("WCSLGXID")) orgGLID = DataType.Long(rowOrg["WCSLGXID"], 0);
            }

            //没有关联ID 不需要更新完成数量
            if (orgGLID == 0 && newGLID == 0) return;

            string newWCSLBM = "";
            string newWCSLLM = "WCSL";

            string orgWCSLBM = "";
            string orgWCSLLM = "WCSL";

            //是否有指定完成数据表名
            if (rowNew != null && rowNew.Table.Columns.Contains("WCSLBM"))
            {
                newWCSLBM = rowNew["WCSLBM"].ToString();
                newWCSLLM = rowNew["WCSLLM"].ToString();
            }

            if (rowOrg != null && rowOrg.Table.Columns.Contains("WCSLBM"))
            {
                orgWCSLBM = rowOrg["WCSLBM"].ToString();
                orgWCSLLM = rowOrg["WCSLLM"].ToString();
            }

            //没有完成数量表名
            if (string.IsNullOrWhiteSpace(tbName) && string.IsNullOrWhiteSpace(orgWCSLBM) && string.IsNullOrWhiteSpace(newWCSLBM)) return;

            if (action == Actions.添加)
            {
                if (!string.IsNullOrWhiteSpace(newWCSLBM))
                {
                    tbName = newWCSLBM;
                    wcslCellName = newWCSLLM;
                }

                //更新完成数量
                DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, newGLID, newSL, true, tranId);
            }
            else if (action == Actions.修改)
            {
                //1、原数据有WCSLBM 新数据有WCSLBM
                //2、有表配置的完成数量表名  原数据没有WCSLBM  新数据也没有WCSLBM
                //3、有表配置的完成数量表名  原数据有WCSLBM  新数据没有WCSLBM
                //4、有表配置的完成数量表名  原数据没有WCSLBM  新数据有WCSLBM
                //5、没有表配置的完成数量表名  原数据有WCSLBM  新数据没有WCSLBM
                //6、没有表配置的完成数量表名  原数据没有WCSLBM  新数据有WCSLBM  

                if (!string.IsNullOrWhiteSpace(orgWCSLBM) && !string.IsNullOrWhiteSpace(newWCSLBM))
                {
                    //1、原数据有WCSLBM 新数据有WCSLBM
                    if (newWCSLBM == orgWCSLBM && orgWCSLLM == newWCSLLM && newGLID == orgGLID && newSL == orgSL) return;

                    DataServiceExt.UpdateWCSL(cmd, orgWCSLBM, orgWCSLLM, orgGLID, orgSL, false, tranId);
                    DataServiceExt.UpdateWCSL(cmd, newWCSLBM, newWCSLLM, newGLID, newSL, true, tranId);
                }
                else if (!string.IsNullOrWhiteSpace(tbName))
                {
                    //有表配置的完成数量表名
                    if (string.IsNullOrWhiteSpace(orgWCSLBM) && string.IsNullOrWhiteSpace(newWCSLBM))
                    {
                        //完成数量表名、数量、关联ID没有变化 无需更新完成数量
                        if (newSL == orgSL && orgGLID == newGLID) return;

                        //2、原数据没有WCSLBM  新数据也没有WCSLBM
                        //2.1、减去原表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, orgGLID, orgSL, false, tranId);
                        //2.2、加到新表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, newGLID, newSL, true, tranId);
                    }
                    else if (!string.IsNullOrWhiteSpace(orgWCSLBM) && string.IsNullOrWhiteSpace(newWCSLBM))
                    {
                        //3、原数据有WCSLBM  新数据没有WCSLBM
                        //3.1、减去原表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, orgWCSLBM, orgWCSLLM, orgGLID, orgSL, false, tranId);
                        //3.2、加到新表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, newGLID, newSL, true, tranId);
                    }
                    else if (string.IsNullOrWhiteSpace(orgWCSLBM) && !string.IsNullOrWhiteSpace(newWCSLBM))
                    {
                        //4、原数据没有WCSLBM  新数据有WCSLBM
                        //4.1、减去原表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, orgGLID, orgSL, false, tranId);
                        //4.2、加到新表列的完成数量
                        DataServiceExt.UpdateWCSL(cmd, newWCSLBM, newWCSLLM, newGLID, newSL, true, tranId);
                    }
                }
                else
                {
                    //没有表配置的完成数量表名
                    if (!string.IsNullOrWhiteSpace(orgWCSLBM))
                    {
                        //5、原数据有WCSLBM  新数据没有WCSLBM
                        DataServiceExt.UpdateWCSL(cmd, orgWCSLBM, orgWCSLLM, orgGLID, orgSL, false, tranId);
                    }
                    else if (!string.IsNullOrWhiteSpace(newWCSLBM))
                    {
                        //6、原数据没有WCSLBM  新数据有WCSLBM
                        DataServiceExt.UpdateWCSL(cmd, newWCSLBM, newWCSLLM, newGLID, newSL, true, tranId);
                    }
                }
            }
            else if (action == Actions.删除)
            {
                if (!string.IsNullOrWhiteSpace(orgWCSLBM))
                {
                    tbName = orgWCSLBM;
                    wcslCellName = orgWCSLLM;
                }

                //更新完成数量
                DataServiceExt.UpdateWCSL(cmd, tbName, wcslCellName, orgGLID, orgSL, false, tranId);
            }
        }
        /// <summary>
        /// 更新完成金额
        /// </summary>
        private static void UpdateWCJE(Actions action, SQLiteCommand cmd, TableInfo tableInfo, DataRow rowNew, DataRow rowOrg, string tranId)
        {
            //没有需要更新的完成金额表
            string tbName = tableInfo.WCJETableName;
            string jeCellName = "JE";
            string wcjeCellName = "WCJE";

            if (rowNew != null)
            {
                if (string.IsNullOrWhiteSpace(tbName) && !rowNew.Table.Columns.Contains("WCJEBM")) return;
                if (!rowNew.Table.Columns.Contains(jeCellName)) return;
            }
            else if (rowOrg != null)
            {
                if (string.IsNullOrWhiteSpace(tbName) && !rowOrg.Table.Columns.Contains("WCJEBM")) return;
                if (!rowOrg.Table.Columns.Contains(jeCellName)) return;
            }
            else
            {
                return;
            }

            decimal newJE = 0;
            decimal orgJE = 0;

            long newGLID = 0;
            long orgGLID = 0;

            if (rowNew != null)
            {
                newJE = DataType.Decimal(rowNew[jeCellName], 0);
                if (rowNew.Table.Columns.Contains("GLID")) newGLID = DataType.Long(rowNew["GLID"], 0);
                if (rowNew.Table.Columns.Contains("WCJEGXID")) newGLID = DataType.Long(rowNew["WCJEGXID"], 0);
            }
            if (rowOrg != null)
            {
                orgJE = DataType.Decimal(rowOrg[jeCellName], 0);
                if (rowOrg.Table.Columns.Contains("GLID")) orgGLID = DataType.Long(rowOrg["GLID"], 0);
                if (rowOrg.Table.Columns.Contains("WCJEGXID")) orgGLID = DataType.Long(rowOrg["WCJEGXID"], 0);
            }

            if (orgGLID == 0 && newGLID == 0) return;

            string newWCJEBM = "";
            string newWCJELM = "WCJE";

            string orgWCJEBM = "";
            string orgWCJELM = "WCJE";

            //是否有指定完成金额表名
            if (rowNew != null && rowNew.Table.Columns.Contains("WCJEBM"))
            {
                newWCJEBM = rowNew["WCJEBM"].ToString();
                newWCJELM = rowNew["WCJELM"].ToString();
            }

            if (rowOrg != null && rowOrg.Table.Columns.Contains("WCJEBM"))
            {
                orgWCJEBM = rowOrg["WCJEBM"].ToString();
                orgWCJELM = rowOrg["WCJELM"].ToString();
            }

            //没有完成金额表名
            if (string.IsNullOrWhiteSpace(tbName) && string.IsNullOrWhiteSpace(orgWCJEBM) && string.IsNullOrWhiteSpace(newWCJEBM)) return;

            if (action == Actions.添加)
            {
                if (!string.IsNullOrWhiteSpace(newWCJEBM))
                {
                    tbName = newWCJEBM;
                    wcjeCellName = newWCJELM;
                }

                //更新完成金额
                DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, newGLID, newJE, true, tranId);
            }
            else if (action == Actions.修改)
            {
                //1、原数据有WCJEBM 新数据有WCJEBM
                //2、有表配置的完成金额表名  原数据没有WCJEBM  新数据也没有WCJEBM
                //3、有表配置的完成金额表名  原数据有WCJEBM  新数据没有WCJEBM
                //4、有表配置的完成金额表名  原数据没有WCJEBM  新数据有WCJEBM
                //5、没有表配置的完成金额表名  原数据有WCJEBM  新数据没有WCJEBM
                //6、没有表配置的完成金额表名  原数据没有WCJEBM  新数据有WCJEBM  

                if (!string.IsNullOrWhiteSpace(orgWCJEBM) && !string.IsNullOrWhiteSpace(newWCJEBM))
                {
                    //1、原数据有WCJEBM 新数据有WCJEBM
                    if (newWCJEBM == orgWCJEBM && orgWCJELM == newWCJELM && newGLID == orgGLID && newJE == orgJE) return;

                    DataServiceExt.UpdateWCJE(cmd, orgWCJEBM, orgWCJELM, orgGLID, orgJE, false, tranId);
                    DataServiceExt.UpdateWCJE(cmd, newWCJEBM, newWCJELM, newGLID, newJE, true, tranId);
                }
                else if (!string.IsNullOrWhiteSpace(tbName))
                {
                    //有表配置的完成金额表名
                    if (string.IsNullOrWhiteSpace(orgWCJEBM) && string.IsNullOrWhiteSpace(newWCJEBM))
                    {
                        //完成金额表名、数量、关联ID没有变化 无需更新完成金额
                        if (newJE == orgJE && orgGLID == newGLID) return;

                        //2、原数据没有WCJEBM  新数据也没有WCJEBM
                        //2.1、减去原表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, orgGLID, orgJE, false, tranId);
                        //2.2、加到新表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, newGLID, newJE, true, tranId);
                    }
                    else if (!string.IsNullOrWhiteSpace(orgWCJEBM) && string.IsNullOrWhiteSpace(newWCJEBM))
                    {
                        //3、原数据有WCJEBM  新数据没有WCJEBM
                        //3.1、减去原表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, orgWCJEBM, orgWCJELM, orgGLID, orgJE, false, tranId);
                        //3.2、加到新表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, newGLID, newJE, true, tranId);
                    }
                    else if (string.IsNullOrWhiteSpace(orgWCJEBM) && !string.IsNullOrWhiteSpace(newWCJEBM))
                    {
                        //4、原数据没有WCJEBM  新数据有WCJEBM
                        //4.1、减去原表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, orgGLID, orgJE, false, tranId);
                        //4.2、加到新表列的完成金额
                        DataServiceExt.UpdateWCJE(cmd, newWCJEBM, newWCJELM, newGLID, newJE, true, tranId);
                    }
                }
                else
                {
                    //没有表配置的完成金额表名
                    if (!string.IsNullOrWhiteSpace(orgWCJEBM))
                    {
                        //5、原数据有WCJEBM  新数据没有WCJEBM
                        DataServiceExt.UpdateWCJE(cmd, orgWCJEBM, orgWCJELM, orgGLID, orgJE, false, tranId);
                    }
                    else if (!string.IsNullOrWhiteSpace(newWCJEBM))
                    {
                        //6、原数据没有WCJEBM  新数据有WCJEBM
                        DataServiceExt.UpdateWCJE(cmd, newWCJEBM, newWCJELM, newGLID, newJE, true, tranId);
                    }
                }
            }
            else if (action == Actions.删除)
            {
                if (!string.IsNullOrWhiteSpace(orgWCJEBM))
                {
                    tbName = orgWCJEBM;
                    wcjeCellName = orgWCJELM;
                }

                //更新完成金额
                DataServiceExt.UpdateWCJE(cmd, tbName, wcjeCellName, orgGLID, orgJE, false, tranId);
            }
        }
        #endregion

        #region 更新被引用数量
        /// <summary>
        /// 更新被引用数量
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="operateTable"></param>
        /// <param name="newRow"></param>
        private static void UpdateSYSBYYSL(SQLiteCommand cmd, TableInfo operateTable, DataRow newRow, DataRow orgRow)
        {
            //更新商品客户被引用数量
            UpdateSPKHSYSBYYSL(cmd, operateTable, newRow, orgRow);

            if (!operateTable.Cells.Exists(p => p.CellName.Equals("GLID")) ||
                !operateTable.Cells.Exists(p => p.CellName.Equals("SYSYYBM"))) return;

            string sql = "";

            if (orgRow != null && newRow != null)
            {
                long orgGLID = DataType.Long(orgRow["GLID"], 0);
                string orgYYBM = orgRow["SYSYYBM"].ToString();

                long newGLID = DataType.Long(newRow["GLID"], 0);
                string newYYBM = newRow["SYSYYBM"].ToString();

                //相同关联ID、相同引用表名
                if (orgGLID == newGLID && orgYYBM == newYYBM) return;
            }

            if (orgRow != null)
            {
                //有旧的
                long orgGLID = DataType.Long(orgRow["GLID"], 0);
                string orgYYBM = orgRow["SYSYYBM"].ToString();
                if (orgGLID <= 0) return;
                if (string.IsNullOrWhiteSpace(orgYYBM)) return;

                sql += " update [" + orgYYBM + "] set [SYSBYYSL]=[SYSBYYSL]-1 where [Id]=" + orgGLID + ";";
            }

            if (newRow != null)
            {
                //有新的
                long newGLID = DataType.Long(newRow["GLID"], 0);
                string newYYBM = newRow["SYSYYBM"].ToString();
                if (newGLID <= 0) return;
                if (string.IsNullOrWhiteSpace(newYYBM)) return;

                sql += " update [" + newYYBM + "] set [SYSBYYSL]=[SYSBYYSL]+1 where [Id]=" + newGLID + ";";
            }

            //执行SQL
            cmd.CommandType = CommandType.Text;
            SQLiteDao.TranExecute(cmd, sql);
        }
        /// <summary>
        /// 更新商品、客户被引用数量
        /// </summary>
        /// <param name="cmd"></param>
        private static void UpdateSPKHSYSBYYSL(SQLiteCommand cmd, TableInfo operateTable, DataRow newRow, DataRow orgRow)
        {
            //商品ID列
            CellInfo cellSPID = operateTable.Cells.Find(p => p.CellName.Equals("SPID"));
            //客户ID列
            CellInfo cellKHID = operateTable.Cells.Find(p => p.CellName.Equals("KHID"));

            //SQL指令
            cmd.CommandType = CommandType.Text;

            if (cellSPID != null && cellSPID.ForeignTableId > 0)
            {
                //有商品ID外键关联表
                long orgSPID = 0;
                long newSPID = 0;

                //原商品ID
                if (orgRow != null) orgSPID = DataType.Long(orgRow["SPID"], 0);
                //新商品ID
                if (newRow != null) newSPID = DataType.Long(newRow["SPID"], 0);
                //商品ID相同 不用更新
                if (orgSPID == newSPID) return;

                //查询关联表
                string sql = "select * from [Sys_Tables] where [Id]=" + cellSPID.ForeignTableId;
                DataRow rowTable = SQLiteDao.TranQueryRow(cmd, sql);
                if (rowTable == null) return;

                string type = rowTable["Type"].ToString();
                string subType = rowTable["SubType"].ToString();
                string yybm = rowTable["TableName"].ToString();

                //库存表
                if (type == "单表" && subType.Equals("库存表") || (type == "视图" && subType == "普通表"))
                {
                    //查询库存表关联的物料表或成品表
                    long foreignTableId = DataType.Long(rowTable["ParentId"], 0);
                    if (foreignTableId <= 0) return;
                    sql = "select * from [Sys_Tables] where [Id]=" + foreignTableId;
                    rowTable = SQLiteDao.TranQueryRow(cmd, sql);
                    if (rowTable == null) return;

                    //重新获取物料表、成品表信息
                    type = rowTable["Type"].ToString();
                    subType = rowTable["SubType"].ToString();
                    yybm = rowTable["TableName"].ToString();
                }

                //更新商品被引用数量
                if (type == "单表" && subType.Equals("商品表"))
                {
                    sql = "";

                    if (orgSPID > 0)
                    {
                        //有旧的
                        sql += @"update [" + yybm + "] set [SYSBYYSL]=[SYSBYYSL]-1 where [Id]=" + orgSPID + ";";
                    }

                    if (newSPID > 0)
                    {
                        //有新的
                        sql += @"update [" + yybm + "] set [SYSBYYSL]=[SYSBYYSL]+1 where [Id]=" + newSPID + ";";
                    }

                    //执行SQL
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }

            if (cellKHID != null && cellKHID.ForeignTableId > 0)
            {
                //有客户ID外键关联表
                long orgKHID = 0;
                long newKHID = 0;

                //原客户ID
                if (orgRow != null) orgKHID = DataType.Long(orgRow["KHID"], 0);
                //新客户ID
                if (newRow != null) newKHID = DataType.Long(newRow["KHID"], 0);
                //客户ID相同 不用更新
                if (orgKHID == newKHID) return;

                //查询关联表
                string sql = "select * from [Sys_Tables] where [Id]=" + cellKHID.ForeignTableId;
                DataRow rowTable = SQLiteDao.TranQueryRow(cmd, sql);
                if (rowTable == null) return;

                string type = rowTable["Type"].ToString();
                string subType = rowTable["SubType"].ToString();
                string yybm = rowTable["TableName"].ToString();

                if (type == "单表" && (subType.Equals("客户表") || subType.Equals("供应商表")))
                {
                    sql = "";

                    if (orgKHID > 0)
                    {
                        //有旧的
                        sql += @"update [" + yybm + "] set [SYSBYYSL]=[SYSBYYSL]-1 where [Id]=" + orgKHID + ";";
                    }

                    if (newKHID > 0)
                    {
                        //有新的
                        sql += @"update [" + yybm + "] set [SYSBYYSL]=[SYSBYYSL]+1 where [Id]=" + newKHID + ";";
                    }

                    //执行SQL
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }
        }
        #endregion

        #region 审核处理库存、应收应付、需求数量
        /// <summary>
        /// 审核处理库存
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="isAudit">是否审核</param>
        private static bool SHProcessStock(SQLiteCommand cmd, TableInfo tableInfo, DataRow row, bool isAudit, string tranId)
        {
            return DataServiceExt.SHUpdateStock(cmd, tableInfo, row, isAudit, tranId);
        }
        /// <summary>
        /// 审核处理应收应付款
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isAudit"></param>
        /// <returns></returns>
        private static bool SHProcessYSYFK(SQLiteCommand cmd, TableInfo tableInfo, DataRow row, bool isAudit, string tranId)
        {
            //是否有客户Id的列 且有关联表
            if (!tableInfo.Cells.Exists(p => p.CellName.Equals("KHID") && p.ForeignTableId > 0)) return false;

            return DataServiceExt.SHUpdatePayment(cmd, tableInfo, row, isAudit, tranId);
        }
        #endregion

        #region 事件
        /// <summary>
        /// 执行表事件
        /// </summary>
        /// <returns></returns>
        private static bool RunTableEvent_BeforeSave(SQLiteCommand cmd, TableInfo operateTable, SQLParam param, EventActions action, ref string msg)
        {
            //是否有事件
            if (operateTable.Events == null || operateTable.Events.Count <= 0) return true;

            //是否有执行事件
            List<EventInfo> events = operateTable.Events.Where(p => p.Action == action.ToString() && !p.IsLastExecute).ToList();
            if (events == null || events.Count <= 0) return true;

            //参数
            List<SQLiteParameter> ps = new List<SQLiteParameter>()
            {
                new SQLiteParameter() { ParameterName = "@Id", Value = param.Id },
            };

            foreach (KeyValue kv in param.OpreateCells)
            {
                ps.Add(new SQLiteParameter() { ParameterName = "@" + kv.Key, Value = kv.Value });
            }

            foreach (EventInfo e in events)
            {
                string sql = e.BeforeExecution;
                if (string.IsNullOrWhiteSpace(sql)) continue;

                bool flag = SQLiteDao.TranExecute(cmd, sql, ps.ToArray());
            }

            return true;
        }
        /// <summary>
        /// 执行表事件
        /// </summary>
        /// <returns></returns>
        private static bool RunTableEvent(SQLiteCommand cmd, TableInfo operateTable, DataRow orgRow, DataRow newRow, EventActions action, ref string msg, bool isBefore)
        {
            //是否有事件
            if (operateTable.Events == null || operateTable.Events.Count <= 0) return true;

            //是否有执行事件
            List<EventInfo> events = operateTable.Events.Where(p => p.Action == action.ToString() && !p.IsLastExecute).ToList();
            if (events == null || events.Count <= 0) return true;

            //是否需要前缀
            bool needPrefix = (orgRow != null && newRow != null);

            //参数
            List<SQLiteParameter> ps = new List<SQLiteParameter>();
            if (orgRow != null)
            {
                foreach (DataColumn col in orgRow.Table.Columns)
                {
                    ps.Add(new SQLiteParameter() { ParameterName = (needPrefix ? "@Org_" : "@") + col.ColumnName, Value = orgRow[col.ColumnName] });
                }
            }
            if (newRow != null)
            {
                foreach (DataColumn col in newRow.Table.Columns)
                {
                    ps.Add(new SQLiteParameter() { ParameterName = "@" + col.ColumnName, Value = newRow[col.ColumnName] });
                }
            }

            foreach (EventInfo e in events)
            {
                string sql = isBefore ? e.BeforeExecution : e.AfterExecution;
                if (string.IsNullOrWhiteSpace(sql)) continue;

                bool flag = SQLiteDao.TranExecute(cmd, sql, ps.ToArray());
            }

            return true;
        }
        /// <summary>
        /// 最后执行事件
        /// </summary>
        /// <returns></returns>
        private static void RunTableLastExecuteEvent2(TableInfo tableInfo, long parentId)
        {
            //是否有事件
            if (tableInfo.Events == null || tableInfo.Events.Count <= 0) return;

            //是否有执行事件
            List<EventInfo> events = tableInfo.Events.Where(p => p.Action.Equals(EventActions.保存.ToString()) && p.IsLastExecute).ToList();
            if (events == null || events.Count <= 0) return;

            foreach (EventInfo e in events)
            {
                string sql = string.IsNullOrWhiteSpace(e.BeforeExecution) ? e.AfterExecution : e.BeforeExecution;
                if (string.IsNullOrWhiteSpace(sql)) continue;

                SQLiteParameter[] ps =
                {
                    new SQLiteParameter() { ParameterName="@ParentId", Value=parentId }
                };

                SQLiteDao.ExecuteNonQuery(sql, ps);
            }
        }

        /// <summary>
        /// 事件动作
        /// </summary>
        public enum EventActions
        {
            保存,
            删除,
            审核,
            反审
        }
        #endregion
    }
}
