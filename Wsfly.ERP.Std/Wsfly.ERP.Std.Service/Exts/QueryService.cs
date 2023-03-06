using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Service.Models;

namespace Wsfly.ERP.Std.Service.Exts
{
    public class QueryService
    {
        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <returns></returns>
        public static PageResult GetPaging(SQLParam param)
        {
            try
            {
                PageResult result = new Models.PageResult()
                {
                    PageSize = param.PageSize,
                    PageIndex = param.PageIndex,
                };
                int totalCount = 0;

                result.Data = SQLiteDao.GetPaging(param, ref totalCount);
                result.TotalCount = totalCount;

                return result;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "获取分页数据异常");
            }

            return null;
        }
        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <returns></returns>
        public static PageResult GetPaging(TableInfo tableInfo, UserInfo userInfo, SQLParam param)
        {
            try
            {
                //是否显示隐藏需要过滤
                if (tableInfo.XSYCGL && tableInfo.Cells.Exists(p => p.CellName.Equals("IsShow")))
                {
                    //表有隐藏、显示配置
                    param.Wheres.Add(new Where("IsShow", true));
                }
                else if (tableInfo.TableName.Equals("Sys_Configs"))
                {
                    //系统配置默认要隐藏不显示的配置
                    param.Wheres.Add(new Where("IsShow", true));
                }

                //非维护人员不显示的表
                if (tableInfo.TableName.Equals("Sys_Tables") && !userInfo.IsSupperAdmin)
                {
                    param.Wheres.Add(new Where() { CellName = "Id", CellValue = 99, Type = WhereType.大于 });
                }

                //需要过滤用户
                if (tableInfo.TableName.Equals("Sys_Users"))
                {
                    if (userInfo.UserId > 1) param.Wheres.Add(new Where("Id", 1, WhereType.大于));
                    else param.Wheres.Add(new Where("Id", 0, WhereType.大于));
                }

                //过滤SQL
                if (!string.IsNullOrWhiteSpace(tableInfo.Wheres))
                {
                    param.WhereSQL = tableInfo.Wheres;
                }

                //不分页
                if (tableInfo.BFY)
                {
                    param.PageSize = 9999;
                    param.DonotPaging = true;
                }

                PageResult result = new Models.PageResult()
                {
                    PageSize = param.PageSize,
                    PageIndex = param.PageIndex,
                };
                int totalCount = 0;

                //查询表数据
                if (tableInfo.IsRealTable || tableInfo.IsVTable)
                {
                    //实体表或虚拟表
                    result.Data = SQLiteDao.GetPaging(param, ref totalCount);
                    result.TotalCount = totalCount;
                }
                else if (tableInfo.IsViewTable)
                {
                    //有查询SQL
                    if (!string.IsNullOrWhiteSpace(tableInfo.QuerySQL))
                    {
                        result = GetQuerySQLData(tableInfo, param);
                    }
                    //主从视图 未完成数量，过滤客户
                    else if (!string.IsNullOrWhiteSpace(tableInfo.MainTableName) && !string.IsNullOrWhiteSpace(tableInfo.SubTableName) && tableInfo.TableSubType == TableSubType.主从视图)
                    {
                        //主从视图:主表、子表                        
                        result = GetZCViewData(tableInfo, param);
                    }
                    //引用视图 审核后，未完成数量，过滤客户
                    else if (!string.IsNullOrWhiteSpace(tableInfo.MainTableName) && !string.IsNullOrWhiteSpace(tableInfo.SubTableName) && tableInfo.TableSubType == TableSubType.引用视图)
                    {
                        //引用视图:主表、子表
                        param.Wheres.Add(new Where("MZZB_IsAudit", true));
                        result = GetYYViewData(tableInfo, param);
                    }
                    else
                    {
                        //视图表
                        result.Data = SQLiteDao.GetPaging(param, ref totalCount);
                        result.TotalCount = totalCount;
                    }
                }
                else
                {
                    result.Data = SQLiteDao.GetPaging(param, ref totalCount);
                    result.TotalCount = totalCount;
                }

                if (result.Data != null)
                {
                    //是否有汇总统计行
                    if (tableInfo.Cells.Exists(p => p.IsSummary))
                    {
                        try
                        {
                            //汇总行
                            DataRow sumRow = result.Data.NewRow();

                            //遍历列
                            foreach (CellInfo cell in tableInfo.Cells)
                            {
                                //不汇总
                                if (!cell.IsSummary) continue;

                                try
                                {
                                    //汇总统计
                                    sumRow[cell.CellName] = result.Data.Compute("sum(" + cell.CellName + ")", "TRUE");
                                }
                                catch { }
                            }

                            result.Data.Rows.Add(sumRow);
                        }
                        catch { }
                    }

                    //设置显示公式列值
                    SetShowGSColumns(result.Data, tableInfo);
                }

                //如果没有表，则生成一个空表
                if (result.Data == null)
                {
                    result.Data = BuildTable(tableInfo);
                }

                //添加默认列
                AddDefaultColumns(result.Data);

                //表名
                result.Data.TableName = tableInfo.TableName;

                //原数据
                result.OrgData = result.Data.Copy();

                return result;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "获取分页数据异常");
            }

            return null;
        }

        /// <summary>
        /// 获取查询SQL的数据
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static PageResult GetQuerySQLData(TableInfo tableInfo, SQLParam param)
        {
            string sql = tableInfo.QuerySQL;
            List<SQLiteParameter> ps = new List<SQLiteParameter>();

            //参数
            if (param.Wheres != null && param.Wheres.Count > 0)
            {
                foreach (Where w in param.Wheres)
                {
                    if (w.CellName == "RQ" && w.Type == WhereType.大于等于) w.CellName = "BeginDate";
                    else if (w.CellName == "RQ" && w.Type == WhereType.小于等于) w.CellName = "EndDate";

                    string name = "@" + w.CellName;
                    object value = w.CellValue;

                    int paramIndex = 1;
                    while (ps.Exists(p => p.ParameterName == name))
                    {
                        name = "@" + w.CellName + (paramIndex++);
                    }

                    if (w.Type == WhereType.模糊前) value = "%" + value;
                    else if (w.Type == WhereType.模糊后) value = value + "%";
                    else if (w.Type == WhereType.模糊查询) value = "%" + value + "%";

                    ps.Add(new SQLiteParameter() { ParameterName = name, Value = value });
                }
            }

            ps.Add(new SQLiteParameter() { ParameterName = "@PageSize", Value = param.PageSize });
            ps.Add(new SQLiteParameter() { ParameterName = "@PageIndex", Value = param.PageIndex });

            //获取SQL中的参数
            Regex regPararm = new Regex(@"\@[\w]+", RegexOptions.IgnoreCase);
            MatchCollection mcList = regPararm.Matches(sql);
            if (mcList != null && mcList.Count > 0)
            {
                foreach (Match mc in mcList)
                {
                    string paramName = mc.Groups[0].Value;
                    if (!ps.Exists(p => p.ParameterName.ToLower() == paramName.ToLower()))
                    {
                        //默认空值
                        ps.Add(new SQLiteParameter() { ParameterName = paramName, Value = DBNull.Value });
                    }
                }
            }


            //分页结果
            PageResult result = new Models.PageResult()
            {
                PageSize = param.PageSize,
                PageIndex = param.PageIndex
            };
            result.Data = SQLiteDao.GetTableBySQL(sql, ps.ToArray());

            //记录数量
            long totalCount = param.PageSize * param.PageIndex;
            if (result.Data.Rows.Count == param.PageSize) totalCount += 1;
            result.TotalCount = DataType.Int(totalCount, 0);

            return result;
        }
        /// <summary>
        /// 获取主从视图
        /// </summary>
        /// <returns></returns>
        private static PageResult GetZCViewData(TableInfo tableInfo, SQLParam param)
        {
            try
            {
                string sql;
                string sqlCount;
                SQLiteParameter[] ps;

                GetViewSQL(tableInfo, param, out sql, out sqlCount, out ps);

                //分页结果
                PageResult result = new Models.PageResult()
                {
                    PageSize = param.PageSize,
                    PageIndex = param.PageIndex
                };
                result.Data = SQLiteDao.GetTableBySQL(sql, ps);

                //数量
                object totalCount = SQLiteDao.ExecuteScalar(sqlCount, ps);
                result.TotalCount = DataType.Int(totalCount, 0);

                return result;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "获取主从视图异常");
            }

            return null;
        }
        /// <summary>
        /// 获取引用视图
        /// </summary>
        /// <returns></returns>
        private static PageResult GetYYViewData(TableInfo tableInfo, SQLParam param)
        {
            try
            {
                string sql;
                string sqlCount;
                SQLiteParameter[] ps;

                //生成SQL
                GetViewSQL(tableInfo, param, out sql, out sqlCount, out ps, true);

                //分页结果
                PageResult result = new PageResult()
                {
                    PageSize = param.PageSize,
                    PageIndex = param.PageIndex
                };
                result.Data = SQLiteDao.GetTableBySQL(sql, ps);

                //数量
                object totalCount = SQLiteDao.ExecuteScalar(sqlCount, ps);
                result.TotalCount = DataType.Int(totalCount, 0);

                return result;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "获取引用视图异常");
            }

            return null;
        }

        /// <summary>
        /// 获取视图查询SQL
        /// </summary>
        /// <returns></returns>
        private static void GetViewSQL(TableInfo viewTableInfo, SQLParam param, out string sql, out string sqlCount, out SQLiteParameter[] ps, bool isYYView = false)
        {
            TableInfo tableInfo = viewTableInfo.ParentTableInfo;

            string cells = "";

            foreach (CellInfo cell in tableInfo.Cells)
            {
                cells += "zb.[" + cell.CellName + "] as [MZZB_" + cell.CellName + "],";
            }

            foreach (CellInfo cell in tableInfo.SubTable.Cells)
            {
                cells += "mx.[" + cell.CellName + "] as [MZMX_" + cell.CellName + "],";
            }

            cells += "(mx.[SL]-mx.[WCSL]) WWCSL,";
            cells += "(mx.[WCSL]/mx.[SL]) WCSLJD,";

            cells += "(mx.[JE]-mx.[WCJE]) WWCJE,";
            cells += "(mx.[WCJE]/mx.[JE]) WCJEJD,";

            cells += "zb.[Id] MZZBID,mx.[Id] MZMXID,mx.[Id] GLID,";

            cells += "'" + tableInfo.TableName + "' as [MZZBBM],";
            cells += "'" + tableInfo.SubTable.TableName + "' as [MZMXBM]";

            sql = @" select " + cells + @" 
                            from [" + tableInfo.TableName + "] zb,[" + tableInfo.SubTable.TableName + @"] mx
                            where zb.[Id]=mx.[ParentId]
                        ";

            sqlCount = @" select count(*)
                                     from [" + tableInfo.TableName + "] zb,[" + tableInfo.SubTable.TableName + @"] mx
                                     where zb.[Id]=mx.[ParentId]
                                    ";

            //引用视图过滤已完成
            if (isYYView)
            {
                sql += " and mx.[WCSL]<mx.[SL] ";
                sqlCount += " and mx.[WCSL]<mx.[SL] ";
            }

            //查询条件
            string sqlWhere = "";
            List<SQLiteParameter> psList = new List<SQLiteParameter>();
            SQLiteDao.BuildWhere(param.Wheres, ref sqlWhere, ref psList);

            sqlWhere = " and " + sqlWhere.Replace("[MZZB_", "zb.[").Replace("[MZMX_", "mx.[");

            sql += sqlWhere;
            sqlCount += sqlWhere;

            //分页
            int beginCount = param.PageSize * (param.PageIndex - 1);
            sql += @" 
                                limit " + param.PageSize + " offset " + beginCount;

            ps = psList.ToArray();
        }

        /// <summary>
        /// 生成空表
        /// </summary>
        /// <returns></returns>
        private static DataTable BuildTable(TableInfo info)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(new DataColumn("Id", typeof(long)));

            foreach (CellInfo cell in info.Cells)
            {
                try
                {
                    if (cell.ValType.IndexOf(',') > 0)
                    {
                        //枚举类型
                        dt.Columns.Add(new DataColumn(cell.CellName, typeof(string)));
                        continue;
                    }

                    try
                    {
                        //根据定义生成列类型
                        Type type = Core.AppHandler.GetTypeByString(cell.ValType);
                        dt.Columns.Add(new DataColumn(cell.CellName, type));
                    }
                    catch
                    {
                        //默认字符类型
                        dt.Columns.Add(new DataColumn(cell.CellName, typeof(string)));
                    }
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "【" + info.TableName + "】生成空表失败！");
                }
            }

            return dt;
        }
        /// <summary>
        /// 添加默认的列
        /// </summary>
        private static void AddDefaultColumns(DataTable dt)
        {
            if (!dt.Columns.Contains("MZ_IsEdit"))
            {
                //是否编辑
                DataColumn colEdit = new DataColumn();
                colEdit.ColumnName = "MZ_IsEdit";
                colEdit.DataType = typeof(bool);
                colEdit.DefaultValue = false;
                dt.Columns.Add(colEdit);
            }

            if (!dt.Columns.Contains("MZ_IsNew"))
            {
                //是否新行
                DataColumn colNew = new DataColumn();
                colNew.ColumnName = "MZ_IsNew";
                colNew.DataType = typeof(bool);
                colNew.DefaultValue = false;
                dt.Columns.Add(colNew);
            }
        }
        /// <summary>
        /// 设置显示公式列
        /// </summary>
        private static void SetShowGSColumns(DataTable dt, TableInfo info)
        {
            //是否有行
            if (dt == null || dt.Rows.Count <= 0) return;

            //是否有显示公式的列
            List<CellInfo> gsCells = info.Cells.Where(p => !string.IsNullOrWhiteSpace(p.ShowGS)).ToList();
            if (gsCells.Count <= 0) return;

            foreach (CellInfo cell in gsCells)
            {
                //是否显示公式
                if (string.IsNullOrWhiteSpace(cell.ShowGS)) continue;

                //不包含显示公式列
                if (!dt.Columns.Contains(cell.CellName))
                {
                    //列类型
                    Type colType = AppHandler.GetTypeByString(cell.ValType);
                    dt.Columns.Add(new DataColumn(cell.CellName, colType));
                }

                //显示公式
                string gs = cell.ShowGS;
                //显示公式-列
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                //匹配
                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(gs);
                if (mc == null || mc.Count <= 0) continue;

                //所有行
                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        //计算公式
                        string jsgs = gs;

                        //公式所有列
                        foreach (System.Text.RegularExpressions.Match m in mc)
                        {
                            //列名
                            string cellName = m.Value.Trim().Trim('[').Trim(']');
                            //替换公式的内容
                            jsgs = jsgs.Replace("[" + cellName + "]", row[cellName].ToString());
                        }

                        //列值
                        row[cell.CellName] = JSGS(jsgs, cell.CellValueType, cell.DecimalDigits);
                    }
                    catch (Exception ex) { }
                }
            }
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
            //计算结果
            object dbValue = new NCalc.Expression(jsgs).Evaluate();

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
        /// 计算合计数
        /// </summary>
        /// <param name="operateTable"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="cells"></param>
        /// <param name="wheres"></param>
        /// <returns></returns>
        public static DataTable GetSum(TableInfo operateTable, object parentId, object id, string cells, string wheres)
        {
            try
            {
                string sql = "select ifnull(sum(" + cells + "),0) from [" + operateTable.TableName + "]";

                if (parentId != null)
                {
                    sql += " where [ParentId]=" + parentId;
                }
                else if (id != null)
                {
                    sql += " where [Id]=" + id;
                }
                else if (!string.IsNullOrWhiteSpace(wheres))
                {
                    sql += " where " + wheres;
                }

                return SQLiteDao.GetTableBySQL(sql);
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 验证审核不可大于库存
        /// </summary>
        /// <returns></returns>
        public static DataTable AuditCheckStock(TableInfo tableInfo, DataRow row, long id)
        {
            try
            {
                if (tableInfo.SubTable != null &&
                    tableInfo.SubTable.Cells.Exists(p => p.CellName == "SPID") &&
                    tableInfo.SubTable.Cells.Exists(p => p.CellName == "SL") &&
                    !string.IsNullOrWhiteSpace(tableInfo.SubTable.SPStockTableName))
                {
                    string sql = @"select kc.* 
                                            from [" + tableInfo.SubTable.TableName + @"] mx
                                            left join [" + tableInfo.SPStockTableName + @"] kc on kc.SPID=mx.SPID
                                            where mx.ParentId=" + id + " and kc.KCSL<mx.SL";
                    return SQLiteDao.GetTableBySQL(sql);
                }
                else if (tableInfo.Cells.Exists(p => p.CellName == "SPID") &&
                            tableInfo.Cells.Exists(p => p.CellName == "SL") &&
                            !string.IsNullOrWhiteSpace(tableInfo.SPStockTableName))
                {
                    long spid = row.GetLong("SPID");
                    decimal sl = row.GetDecimal("SL");

                    string sql = "select * from [" + tableInfo.SPStockTableName + "] where SPID=" + spid + " and KCSL<" + sl;
                    return SQLiteDao.GetTableBySQL(sql);
                }
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 获取历史价格
        /// </summary>
        /// <returns></returns>
        public static decimal GetHistoryPrice(long khTableId, long khid, long spid)
        {
            try
            {
                string sql = "select * from [C_PriceHistorys] where [KHTableId]=" + khTableId + " and [KHID]=" + khid + " and [SPID]=" + spid;
                DataRow row = SQLiteDao.GetTableRowBySql(sql);
                return row.GetDecimal("DJ");
            }
            catch { }

            return 0;
        }
    }
}
