using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Service.Exts
{
    /// <summary>
    /// 数据服务扩展
    /// </summary>
    public class DataServiceExt
    {
        #region 完成数量
        /// <summary>
        /// 更新完成数量
        /// </summary>
        /// <returns></returns>
        public static bool UpdateWCSL(SQLiteCommand cmd, string tbName, string cellName, long glid, decimal wcsl, bool isAdd, string tranId)
        {
            //是否有完成数量列名
            if (string.IsNullOrWhiteSpace(cellName)) cellName = "WCSL";

            //是否增加完成数量
            wcsl = isAdd ? wcsl : -wcsl;

            string sql = "update [" + tbName + "] set [" + cellName + "]=ROUND(ifnull([" + cellName + "],0)+(" + wcsl + "),6) where [Id]=" + glid;
            string sqlQuery = "select * from [" + tbName + "] where [Id]=" + glid;

            DataRow fOrgRow = null;

            //更新前的内容
            fOrgRow = SQLiteDao.TranQueryRow(cmd, sqlQuery);

            //SQL
            cmd.CommandType = CommandType.Text;
            return SQLiteDao.TranExecute(cmd, sql);
        }
        #endregion

        #region 完成金额
        /// <summary>
        /// 更新完成金额
        /// </summary>
        /// <returns></returns>
        public static bool UpdateWCJE(SQLiteCommand cmd, string tbName, string cellName, long glid, decimal wcje, bool isAdd, string tranId)
        {
            //是否有完成数量列名
            if (string.IsNullOrWhiteSpace(cellName)) cellName = "WCJE";

            wcje = isAdd ? wcje : -wcje;
            string sql = "update [" + tbName + "] set [" + cellName + "]=ROUND(ifnull([" + cellName + "],0)+(" + wcje + "),6) where [Id]=" + glid;
            string sqlQuery = "select * from [" + tbName + "] where [Id]=" + glid;

            DataRow fOrgRow = null;
            DataRow fNewRow = null;

            //更新前的内容
            fOrgRow = SQLiteDao.TranQueryRow(cmd, sqlQuery);

            //SQL
            cmd.CommandType = CommandType.Text;
            //更新完成金额
            return SQLiteDao.TranExecute(cmd, sql);
        }

        #endregion

        #region 库存服务
        /// <summary>
        /// 开单更新库存
        /// </summary>
        /// <param name="tableInfo">表配置信息</param>
        /// <param name="spid">商品编号</param>
        /// <param name="orgCount">原数量</param>
        /// <param name="newCount">新数量</param>
        /// <returns></returns>
        public static bool KDUpdateStock(SQLiteCommand cmd, TableInfo tableInfo, DataRow topRow, DataRow rowDetail, long spid, string spmc, string spbh, decimal orgCount, decimal newCount, string tranId)
        {
            //商品编号
            if (spid <= 0) return false;
            //不需要更新库存
            if (orgCount == newCount) return true;

            //库存表名
            string stockTableName = tableInfo.SPStockTableName;
            if (string.IsNullOrWhiteSpace(stockTableName)) return false;

            //加库存的列
            string[] addCountCells = { "KDAddPurchaseCount", "KDAddOrderCount", "KDAddIntoCount", "KDAddOutCount", "KDAddStockCount", "KDAddAvailableCount", "KDAddProductionCount", "KDAddMaterielCount", "KDAddXQSL" };
            //减库存的列
            string[] reduceCountCells = { "KDReducePurchaseCount", "KDReduceOrderCount", "KDReduceIntoCount", "KDReduceOutCount", "KDReduceStockCount", "KDReduceAvailableCount", "KDReduceProductionCount", "KDReduceMaterielCount", "KDReduceXQSL" };
            //库存列
            string[] operateStockCells = { "CGSL", "DDSL", "RKSL", "CKSL", "KCSL", "KYSL", "SCSL", "WLSL", "XQSL" };

            //商品信息
            string spTableName = null;
            //StockHasSP(cmd, tableInfo, stockTableName, spid, ref spTableName, spmc, spbh);

            //表配置类型
            Type tbType = tableInfo.GetType();

            DataRow orgKC = null;
            decimal orgSL = 0;

            if (topRow == null) topRow = rowDetail;

            long mxid = DataType.Long(rowDetail["Id"], 0);
            long ddid = DataType.Long(topRow["Id"], 0);
            string dddh = topRow.Table.Columns.Contains("BH") ?
                            topRow["BH"].ToString() :
                            topRow.Table.Columns.Contains("DH") ?
                                topRow["DH"].ToString() :
                                "";

            try
            {
                //原库存信息
                orgKC = SQLiteDao.TranQueryRow(cmd, "select * from [" + stockTableName + "] where [SPID]=" + spid);
            }
            catch { }

            //循环要处理的列
            for (int i = 0; i < addCountCells.Length; i++)
            {
                //要增加的
                string addCellName = addCountCells[i];
                //要减少的
                string reduceCellName = reduceCountCells[i];
                //操作库存的列
                string operateCellName = operateStockCells[i];

                //反射属性
                System.Reflection.PropertyInfo propertyInfoAdd = tbType.GetProperty(addCellName);
                System.Reflection.PropertyInfo propertyInfoReduce = tbType.GetProperty(reduceCellName);

                //是否要添加数量
                bool isAddCount = (bool)propertyInfoAdd.GetValue(tableInfo, null);
                //是否要减少数量
                bool isReduceCount = (bool)propertyInfoReduce.GetValue(tableInfo, null);

                if (orgKC != null)
                {
                    //原数量
                    orgSL = DataType.Decimal(orgKC[operateCellName], 0);
                }

                //SQL
                cmd.CommandType = CommandType.Text;

                string sql = "";

                if (isAddCount)
                {
                    decimal xsl = (orgSL + (newCount - orgCount));

                    //添加数量
                    sql = " update [" + stockTableName + "] set [" + operateCellName + "]=" + xsl + " where [SPID] = " + spid;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }

                if (isReduceCount)
                {
                    decimal xsl = (orgSL - (newCount - orgCount));

                    //减少数量
                    sql = " update [" + stockTableName + "] set [" + operateCellName + "]=" + xsl + " where [SPID] = " + spid;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }

            return true;
        }
        /// <summary>
        /// 审核更新库存
        /// </summary>
        /// <param name="tableInfoJson">表配置信息</param>
        /// <param name="chooseId">选择编号</param>
        /// <param name="isAudit">是否审核 true：审核，false：反审</param>
        /// <returns></returns>
        public static bool SHUpdateStock(SQLiteCommand cmd, TableInfo tableInfo, DataRow topRow, bool isAudit, string tranId)
        {
            //表类型
            string type = tableInfo.Type;

            if (type.Equals("单表"))
            {
                //库存表名
                string stockTableName = tableInfo.SPStockTableName;
                if (string.IsNullOrWhiteSpace(stockTableName)) return false;

                //主表名
                string mainTableName = tableInfo.TableName;

                //查询数据
                if (!topRow.Table.Columns.Contains("SPID") || !topRow.Table.Columns.Contains("SL")) return false;
                long spid = DataType.Long(topRow["SPID"], 0);
                if (spid <= 0) return false;
                decimal count = DataType.Decimal(topRow["SL"], 0);
                if (count <= 0) return false;

                //更新
                SHUpdateCount(cmd, tableInfo, stockTableName, topRow, topRow, spid, count, isAudit, tranId);

                return true;
            }
            else if (type.Equals("双表"))
            {
                //库存表名
                string stockTableName = tableInfo.SubTable.SPStockTableName;
                if (string.IsNullOrWhiteSpace(stockTableName)) return false;

                //子表名
                string subTableName = tableInfo.SubTable.TableName;

                //获取子表数据
                string sql = "select * from [" + tableInfo.SubTable.TableName + "] where [ParentId]=" + topRow["Id"];
                DataTable dtDetails = SQLiteDao.TranQueryTable(cmd, sql);
                if (dtDetails == null || dtDetails.Rows.Count <= 0) return false;
                if (!dtDetails.Columns.Contains("SPID") || !dtDetails.Columns.Contains("SL")) return false;

                string spTableName = null;

                foreach (DataRow rowDetail in dtDetails.Rows)
                {
                    long spid = DataType.Long(rowDetail["SPID"], 0);
                    if (spid <= 0) continue;
                    decimal count = DataType.Decimal(rowDetail["SL"], 0);

                    //更新
                    SHUpdateCount(cmd, tableInfo.SubTable, stockTableName, topRow, rowDetail, spid, count, isAudit, tranId);
                }

                return true;
            }

            return false;
        }
        /// <summary>
        /// 审核更新库存
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableInfo"></param>
        /// <param name="stockTableName"></param>
        /// <param name="spid"></param>
        /// <param name="count"></param>
        /// <param name="isAudit"></param>
        private static void SHUpdateCount(SQLiteCommand cmd, TableInfo tableInfo, string stockTableName, DataRow topRow, DataRow rowDetail, long spid, decimal count, bool isAudit, string tranId)
        {
            //加库存的列
            string[] addCountCells = { "SHAddPurchaseCount", "SHAddOrderCount", "SHAddIntoCount", "SHAddOutCount", "SHAddStockCount", "SHAddAvailableCount", "SHAddProductionCount", "SHAddMaterielCount", "SHAddXQSL" };
            //减库存的列
            string[] reduceCountCells = { "SHReducePurchaseCount", "SHReduceOrderCount", "SHReduceIntoCount", "SHReduceOutCount", "SHReduceStockCount", "SHReduceAvailableCount", "SHReduceProductionCount", "SHReduceMaterielCount", "SHReduceXQSL" };
            //库存列
            string[] operateStockCells = { "CGSL", "DDSL", "RKSL", "CKSL", "KCSL", "KYSL", "SCSL", "WLSL", "XQSL" };

            //表配置类型
            Type tbType = tableInfo.GetType();

            if (topRow == null) topRow = rowDetail;

            DataRow orgKC = null;
            decimal orgSL = 0;
            decimal newASL = 0;
            decimal newRSL = 0;

            string spmc = rowDetail["SPMC"].ToString();
            string spbh = rowDetail["SPBH"].ToString();

            long mxid = DataType.Long(rowDetail["Id"], 0);
            long ddid = DataType.Long(topRow["Id"], 0);
            string dddh = topRow.Table.Columns.Contains("BH") ?
                            topRow["BH"].ToString() :
                            topRow.Table.Columns.Contains("DH") ?
                                topRow["DH"].ToString() :
                                "";

            try
            {
                //原库存信息
                orgKC = SQLiteDao.TranQueryRow(cmd, "select * from [" + stockTableName + "] where [SPID]=" + spid);
            }
            catch { }

            //循环要处理的列
            for (int i = 0; i < addCountCells.Length; i++)
            {
                //要增加的
                string addCellName = addCountCells[i];
                //要减少的
                string reduceCellName = reduceCountCells[i];
                //操作库存的列
                string operateCellName = operateStockCells[i];

                //反射属性
                System.Reflection.PropertyInfo propertyInfoAdd = tbType.GetProperty(addCellName);
                System.Reflection.PropertyInfo propertyInfoReduce = tbType.GetProperty(reduceCellName);

                //是否要添加数量
                bool isAddCount = (bool)propertyInfoAdd.GetValue(tableInfo, null);
                //是否要减少数量
                bool isReduceCount = (bool)propertyInfoReduce.GetValue(tableInfo, null);

                //原数量
                if (orgKC != null) orgSL = DataType.Decimal(orgKC[operateCellName], 0);

                //SQL
                string sql = "";

                //审核正向操作
                string addOperateChar = "+";
                string reduceOperateChar = "-";

                newASL = orgSL + count;
                newRSL = orgSL - count;

                //反审，反向操作
                if (!isAudit)
                {
                    addOperateChar = "-";
                    reduceOperateChar = "+";

                    newASL = orgSL - count;
                    newRSL = orgSL + count;
                }

                //SQL
                cmd.CommandType = CommandType.Text;

                if (isAddCount)
                {
                    //添加数量
                    sql = " update [" + stockTableName + "] set [" + operateCellName + "]=" + newASL + " where [SPID] = " + spid;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }

                if (isReduceCount)
                {
                    //减少数量
                    sql = " update [" + stockTableName + "] set [" + operateCellName + "]=" + newRSL + " where [SPID] = " + spid;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }
        }
        #endregion

        #region 应收应付
        /// <summary>
        /// 开单修改应收款、应付款
        /// </summary>
        /// <param name="tableInfo">表配置信息</param>
        /// <param name="khId">客户Id</param>
        /// <param name="orgMoney">原金额</param>
        /// <param name="newMoney">新金额</param>
        /// <returns></returns>
        public static bool KDUpdatePayment(SQLiteCommand cmd, TableInfo tableInfo, DataRow topRow, long khId, decimal orgMoney, decimal newMoney, string tranId)
        {
            //客户编号
            if (khId <= 0) return false;
            //不需要更新金额
            if (orgMoney == newMoney) return true;

            //供应商/客户表名
            string khTableName = GetKHTableName(cmd, tableInfo);
            if (string.IsNullOrWhiteSpace(khTableName)) return false;

            //加的列
            string[] addCells = { "KDAddYSK", "KDAddYFK" };
            //减的列
            string[] reduceCells = { "KDReduceYSK", "KDReduceYFK" };
            //金额列
            string[] operateCells = { "YSK", "YFK" };

            string sql = string.Empty;

            //表配置类型
            Type tbType = tableInfo.GetType();

            DataRow orgYSYF = null;
            decimal orgJE = 0;
            string khmc = "";
            string khbh = "";

            long ddid = DataType.Long(topRow["Id"], 0);
            string dddh = topRow.Table.Columns.Contains("BH") ?
                            topRow["BH"].ToString() :
                            topRow.Table.Columns.Contains("DH") ?
                                topRow["DH"].ToString() :
                                "";

            try
            {
                //原记录
                orgYSYF = SQLiteDao.TranQueryRow(cmd, "select * from [" + khTableName + "] where [Id]=" + khId);

                khmc = orgYSYF["KHMC"].ToString();
                khbh = orgYSYF["KHBH"].ToString();
            }
            catch { }

            //循环要处理的列
            for (int i = 0; i < addCells.Length; i++)
            {
                //要增加的
                string addCellName = addCells[i];
                //要减少的
                string reduceCellName = reduceCells[i];
                //操作的列
                string operateCellName = operateCells[i];

                //反射属性
                System.Reflection.PropertyInfo propertyInfoAdd = tbType.GetProperty(addCellName);
                System.Reflection.PropertyInfo propertyInfoReduce = tbType.GetProperty(reduceCellName);

                //是否要添加
                bool isAdd = (bool)propertyInfoAdd.GetValue(tableInfo, null);
                //是否要减少
                bool isReduce = (bool)propertyInfoReduce.GetValue(tableInfo, null);

                //原金额
                if (orgYSYF != null) orgJE = DataType.Decimal(orgYSYF[operateCellName], 0);

                //SQL
                cmd.CommandType = CommandType.Text;

                if (isAdd)
                {
                    decimal xje = (orgJE + (newMoney - orgMoney));

                    //添加金额
                    sql = " update [" + khTableName + "] set [" + operateCellName + "]=" + xje + " where [Id] = " + khId;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }

                if (isReduce)
                {
                    decimal xje = (orgJE - (newMoney - orgMoney));

                    //减少金额
                    sql = " update [" + khTableName + "] set [" + operateCellName + "]=" + xje + " where [Id] = " + khId;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }

            return true;
        }

        /// <summary>
        /// 审核修改应收款、应付款
        /// </summary>
        /// <param name="tableInfo">表配置信息</param>
        /// <param name="id">表主键Id</param>
        /// <param name="isAudit">是否审核</param>
        /// <returns></returns>
        public static bool SHUpdatePayment(SQLiteCommand cmd, TableInfo tableInfo, DataRow topRow, bool isAudit, string tranId)
        {
            //供应商/客户表名
            string khTableName = GetKHTableName(cmd, tableInfo);
            if (string.IsNullOrWhiteSpace(khTableName)) return false;

            //表类型
            string type = tableInfo.Type;
            //主表名
            string mainTBName = tableInfo.TableName;
            string subTBName = string.Empty;

            decimal totalMoney = 0;
            long khId = 0;

            //SQL
            cmd.CommandType = CommandType.Text;

            //总金额
            totalMoney = DataType.Decimal(topRow["ZJE"], 0);
            //客户编号
            khId = DataType.Long(topRow["KHID"], 0);

            if (totalMoney <= 0) return false;
            if (khId <= 0) return false;

            //加的列
            string[] addCells = { "SHAddYSK", "SHAddYFK" };
            //减的列
            string[] reduceCells = { "SHReduceYSK", "SHReduceYFK" };
            //金额列
            string[] operateCells = { "YSK", "YFK" };

            Type tbType = tableInfo.GetType();

            DataRow orgYSYF = null;
            decimal orgJE = 0;
            decimal newAJE = 0;
            decimal newRJE = 0;
            string khmc = "";
            string khbh = "";

            long ddid = DataType.Long(topRow["Id"], 0);
            string dddh = topRow.Table.Columns.Contains("BH") ?
                            topRow["BH"].ToString() :
                            topRow.Table.Columns.Contains("DH") ?
                                topRow["DH"].ToString() :
                                "";

            try
            {
                //原记录
                orgYSYF = SQLiteDao.TranQueryRow(cmd, "select * from [" + khTableName + "] where [Id]=" + khId);
                khmc = orgYSYF["KHMC"].ToString();
                khbh = orgYSYF["KHBH"].ToString();
            }
            catch { }

            for (int i = 0; i < addCells.Length; i++)
            {
                //要增加的
                string addCellName = addCells[i];
                //要减少的
                string reduceCellName = reduceCells[i];
                //操作库存的列
                string operateCellName = operateCells[i];

                //反射属性
                System.Reflection.PropertyInfo propertyInfoAdd = tbType.GetProperty(addCellName);
                System.Reflection.PropertyInfo propertyInfoReduce = tbType.GetProperty(reduceCellName);

                //是否要添加
                bool isAdd = (bool)propertyInfoAdd.GetValue(tableInfo, null);
                //是否要减少
                bool isReduce = (bool)propertyInfoReduce.GetValue(tableInfo, null);

                //原金额
                if (orgYSYF != null) orgJE = DataType.Decimal(orgYSYF[operateCellName], 0);

                //审核正向操作
                string addOperateChar = "+";
                string reduceOperateChar = "-";

                newAJE = orgJE + totalMoney;
                newRJE = orgJE - totalMoney;

                //反审，反向操作
                if (!isAudit)
                {
                    addOperateChar = "-";
                    reduceOperateChar = "+";

                    newAJE = orgJE - totalMoney;
                    newRJE = orgJE + totalMoney;
                }

                //SQL
                cmd.CommandType = CommandType.Text;
                string sql = "";

                if (isAdd)
                {
                    //添加
                    sql = " update [" + khTableName + "] set [" + operateCellName + "]=" + newAJE + " where [Id] = " + khId;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }

                if (isReduce)
                {
                    //减少
                    sql = " update [" + khTableName + "] set [" + operateCellName + "]=" + newRJE + " where [Id] = " + khId;

                    //事务执行
                    SQLiteDao.TranExecute(cmd, sql);
                }
            }

            return true;
        }

        /// <summary>
        /// 得到客户/供应商表名
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        private static string GetKHTableName(SQLiteCommand cmd, TableInfo tableInfo)
        {
            //是否有客户列
            CellInfo khidCell = tableInfo.Cells.Find(p => p.CellName.Equals("KHID"));
            if (khidCell == null) return string.Empty;

            //有指定应收应付表
            if (!string.IsNullOrWhiteSpace(tableInfo.YSYFTableName)) return tableInfo.YSYFTableName;

            //是否有外键表ID
            if (khidCell.ForeignTableId <= 0) return string.Empty;

            //客户表
            string sql = "select * from [Sys_Tables] where [Id]=" + khidCell.ForeignTableId;
            DataRow rowKH = SQLiteDao.TranQueryRow(cmd, sql);
            if (rowKH == null) return string.Empty;

            string type = rowKH["Type"].ToString();
            string subType = rowKH["SubType"].ToString();
            string khbm = rowKH["TableName"].ToString();

            //如果不是客户表和供应商表 则无法统计应收应付
            if (type != "单表") return string.Empty;
            if (!subType.Equals("客户表") && !subType.Equals("供应商表")) return string.Empty;

            //关联表名
            return khbm;
        }
        #endregion

        #region 库存表信息
        /// <summary>
        /// 添加商品库存
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="spkcb"></param>
        /// <param name="spid"></param>
        /// <param name="spmc"></param>
        /// <param name="spbh"></param>
        public static long AddSPKC(SQLiteCommand cmd, string spkcb, long spid, string spmc, string spbh)
        {
            //插入记录
            string sql = "insert into [" + spkcb + @"]([SPID],[SPMC],[SPBH],[CGSL],[DDSL],[RKSL],[CKSL],[KCSL],[KYSL],[SCSL],[WLSL],[XQSL],[KCJE],[KCJJ],[CreateDate],[SearchKeywords],[IsShow]) 
                                 values(@SPID,@SPMC,@SPBH,0,0,0,0,0,0,0,0,0,0,0,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',@SearchKeywords,1)";
            SQLiteParameter[] ps =
            {
                new SQLiteParameter("@SPID", spid),
                new SQLiteParameter("@SPMC", spmc),
                new SQLiteParameter("@SPBH", spbh),
                new SQLiteParameter("@SearchKeywords", spbh + spmc),
            };
            object objResult = null;

            //SQL
            cmd.CommandType = CommandType.Text;

            //事务执行
            objResult = SQLiteDao.TranExecuteScalar(cmd, sql, ps);

            //添加的库存结果
            long id = DataType.Long(objResult, 0);

            //添加成功 返回库存ID
            if (id > 0) return id;

            //添加失败
            throw new Exception("将商品添加到库存失败！");
        }
        /// <summary>
        /// 更新商品库存信息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="spkcb"></param>
        /// <param name="spid"></param>
        /// <param name="spmc"></param>
        /// <param name="spbh"></param>
        public static bool UpdateSPKC(SQLiteCommand cmd, string spkcb, long spid, string spmc, string spbh)
        {
            //更新记录
            string sql = "update [" + spkcb + @"] set [SPMC]=@SPMC,[SPBH]=@SPBH where [SPID]=@SPID";
            SQLiteParameter[] ps =
            {
                new SQLiteParameter("@SPMC", spmc),
                new SQLiteParameter("@SPBH", spbh),
                new SQLiteParameter("@SPID", spid),
            };

            //SQL
            cmd.CommandType = CommandType.Text;
            return SQLiteDao.TranExecute(cmd, sql, ps);
        }
        /// <summary>
        /// 删除商品库存信息
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="spkcb"></param>
        /// <param name="spid"></param>
        /// <returns></returns>
        public static bool DeleteSPKC(SQLiteCommand cmd, string spkcb, long spid)
        {
            //删除记录
            string sql = "delete from [" + spkcb + @"] where [SPID]=" + spid;

            //SQL
            cmd.CommandType = CommandType.Text;
            return SQLiteDao.TranExecute(cmd, sql);
        }
        #endregion
    }
}
