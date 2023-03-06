using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Service.Exts
{
    /// <summary>
    /// 表配置服务
    /// </summary>
    public class TableConfigsService
    {
        /// <summary>
        /// 加载终端表配置信息
        /// </summary>
        /// <returns></returns>
        public static List<TableInfo> LoadClientTableInfos(long userId)
        {
            try
            {
                //1
                //加载所有表
                DataTable dtTables = SQLiteDao.GetTable("Sys_Tables");
                if (dtTables == null || dtTables.Rows.Count <= 0) return null;

                //2
                //加载所有列
                DataTable dtCells = SQLiteDao.GetTable("Sys_TableCells");
                if (dtCells == null || dtCells.Rows.Count <= 0) return null;
                dtCells.AsDataView().Sort = "Order";

                //3
                //加载所有审批流程  取消

                //4
                //加载所有表事件
                DataTable dtEvents = SQLiteDao.GetTable(new SQLParam()
                {
                    TableName = "Sys_TableActionEvents",
                    OrderBys = new List<OrderBy>()
                    {
                        new OrderBy("Order", OrderType.顺序)
                    }
                });

                //5
                //加载表右键菜单
                DataTable dtMenus = SQLiteDao.GetTable("Sys_TableMenus");

                //表配置列表
                List<TableInfo> tables = new List<TableInfo>();

                foreach (DataRow rowTable in dtTables.Rows)
                {
                    //1
                    //表配置信息
                    TableInfo tableInfo = DataRowToTableInfo(rowTable);

                    //列配置初始化
                    tableInfo.Cells = new List<CellInfo>();
                    tableInfo.ViewCells = new List<string>();
                    tableInfo.Events = new List<EventInfo>();
                    tableInfo.Menus = new List<MenuInfo>();

                    //表ID
                    long tableId = DataType.Long(rowTable["Id"], 0);

                    //2
                    //加载表列
                    if (dtCells != null && dtCells.Rows.Count > 0)
                    {
                        DataRow[] rowCells = dtCells.Select("[ParentId]=" + tableId);
                        if (rowCells != null && rowCells.Length > 0)
                        {
                            foreach (DataRow rowCell in rowCells)
                            {
                                //列配置
                                CellInfo cellInfo = DataRowToCell(rowCell);

                                //添加列配置
                                tableInfo.Cells.Add(cellInfo);
                                //所有列名
                                tableInfo.ViewCells.Add(cellInfo.CellName);
                            }
                        }
                    }

                    //4
                    //加载表事件
                    if (dtEvents != null && dtEvents.Rows.Count > 0)
                    {
                        DataRow[] rowEvents = dtEvents.Select("[TableId]=" + tableId);
                        if (rowEvents != null && rowEvents.Length > 0)
                        {
                            bool hasIsLastExecute = rowEvents[0].Table.Columns.Contains("IsLastExecute");

                            foreach (DataRow rowEvent in rowEvents)
                            {
                                try
                                {
                                    EventInfo eventInfo = new EventInfo()
                                    {
                                        Action = rowEvent["Action"].ToString(),
                                        BeforeExecution = rowEvent["BeforeExecution"].ToString(),
                                        AfterExecution = rowEvent["AfterExecution"].ToString(),
                                    };

                                    if (hasIsLastExecute)
                                    {
                                        eventInfo.IsLastExecute = DataType.Bool(rowEvent["IsLastExecute"], false);
                                    }

                                    tableInfo.Events.Add(eventInfo);
                                }
                                catch { }
                            }
                        }
                    }

                    //5
                    //加载表菜单
                    if (dtMenus != null && dtMenus.Rows.Count > 0)
                    {
                        DataRow[] rowMenus = dtMenus.Select("[TableId]=" + tableId);
                        if (rowMenus != null && rowMenus.Length > 0)
                        {
                            foreach (DataRow rowMenu in rowMenus)
                            {
                                tableInfo.Menus.Add(new MenuInfo()
                                {
                                    TableId = DataType.Int(rowMenu["TableId"], 0),
                                    TableName = rowMenu["TableName"].ToString(),
                                    MenuName = rowMenu["MenuName"].ToString(),
                                    ExecuteSQL = rowMenu["ExecuteSQL"].ToString(),
                                    RefreshTop = DataType.Bool(rowMenu["RefreshTop"], false),
                                    RefreshBottom = DataType.Bool(rowMenu["RefreshBottom"], false),
                                    Remark = rowMenu["Remark"].ToString(),
                                    CreateDate = DataType.DateTime(rowMenu["CreateDate"], DateTime.Now),
                                });
                            }
                        }
                    }

                    tables.Add(tableInfo);
                }

                //加载用户自定义设置
                List<TableInfo> listTables = LoadUserCustomSettings(tables, userId);

                //遍历表配置
                foreach (TableInfo table in listTables)
                {
                    if (table.ParentId < 1) continue;

                    table.ParentTableInfo = listTables.Find(p => p.Id == table.ParentId);

                    if (table.TableType == TableType.双表)
                    {
                        table.ParentTableInfo.SubTable = table;
                    }
                    else if (table.TableType == TableType.三表)
                    {
                        table.ParentTableInfo.ThreeTable = table;
                    }
                }

                return listTables;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "加载数据库的终端表配置信息异常");
            }

            return null;
        }
        /// <summary>
        /// 加载用户自定义设置
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static List<TableInfo> LoadUserCustomSettings(List<TableInfo> list, long userId)
        {
            List<TableInfo> tables = list.Clone();

            try
            {
                //1
                //加载用户列配置
                DataTable dtUserTableConfigs = SQLiteDao.GetTable(new Dao.SQLParam()
                {
                    TableName = "Sys_UserTableConfigs",
                    Wheres = new List<Where>()
                    {
                        new Where("UserId", userId)
                    }
                });

                if (dtUserTableConfigs == null || dtUserTableConfigs.Rows.Count < 1) return list;

                //===========================================================


                //2
                //加载显示比例
                DataTable dtTopBomttomBL = SQLiteDao.GetTable(new Dao.SQLParam()
                {
                    TableName = "Sys_UserTableConfigs",
                    Wheres = new List<Where>()
                    {
                        new Where("UserId", userId),
                        new Where("TableCellId", 0)
                    }
                });

                foreach (TableInfo table in tables)
                {
                    //用户列配置
                    try
                    {
                        DataRow[] rows = dtUserTableConfigs.Select("[TableId]=" + table.Id);
                        if (rows != null && rows.Length > 0)
                        {
                            foreach (DataRow row in rows)
                            {
                                var cell = table.Cells.Find(p => p.Id == DataType.Long(row["TableCellId"], 0));
                                if (cell != null)
                                {
                                    cell.UserCellOrder = DataType.Int(row["CellOrder"], 0);
                                    cell.UserCellWidth = DataType.Int(row["CellWidth"], 0);
                                    cell.CellWidth = DataType.Int(row["CellWidth"], 0);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }

                    //上下表显示比例
                    if (dtTopBomttomBL != null && dtTopBomttomBL.Rows.Count > 0)
                    {
                        try
                        {
                            DataRow[] rows = dtTopBomttomBL.Select("[TableId]=" + table.Id);
                            if (rows != null && rows.Length > 0)
                            {
                                double bl = DataType.Double(rows[0]["TopBL"], 0);
                                if (bl > 0)
                                {
                                    table.TopBL = bl;
                                    table.BottomBL = 1 - bl;
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                }

            }
            catch (Exception ex) { }

            return tables;
        }

        /// <summary>
        /// 数据行表配置信息转表对象
        /// </summary>
        /// <param name="rowTable"></param>
        /// <returns></returns>
        private static TableInfo DataRowToTableInfo(DataRow rowTable)
        {
            //主表信息
            TableInfo tableInfo = new TableInfo();

            //得到所有属性
            System.Reflection.PropertyInfo[] propertys = typeof(TableInfo).GetProperties();

            //遍历属性
            foreach (System.Reflection.PropertyInfo property in propertys)
            {
                string pName = "";

                try
                {
                    //属性名称
                    pName = property.Name;

                    //是否包含列
                    if (rowTable.Table.Columns.Contains(pName))
                    {
                        //空值
                        if (rowTable[pName] == DBNull.Value) continue;

                        //赋值
                        try
                        {
                            property.SetValue(tableInfo, Convert.ChangeType(rowTable[property.Name], property.PropertyType), null);
                        }
                        catch (Exception ex)
                        {
                            property.SetValue(tableInfo, rowTable[pName], null);
                        }
                    }
                }
                catch (Exception ex) { }
            }

            tableInfo.Cells = new List<CellInfo>();
            tableInfo.ViewCells = new List<string>();

            return tableInfo;
        }
        /// <summary>
        /// 数据行列配置转列配置对象
        /// </summary>
        private static CellInfo DataRowToCell(DataRow row)
        {
            //列配置信息
            CellInfo cellInfo = new CellInfo();

            //得到所有属性
            System.Reflection.PropertyInfo[] propertys = typeof(CellInfo).GetProperties();

            //遍历属性
            foreach (System.Reflection.PropertyInfo property in propertys)
            {
                string pName = "";

                try
                {
                    //属性名称
                    pName = property.Name;

                    //是否包含列
                    if (row.Table.Columns.Contains(pName))
                    {
                        //空值
                        if (row[pName] == DBNull.Value) continue;

                        //赋值
                        try
                        {
                            property.SetValue(cellInfo, Convert.ChangeType(row[property.Name], property.PropertyType), null);
                        }
                        catch (Exception ex)
                        {
                            property.SetValue(cellInfo, row[pName], null);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            //不显示关联主表列
            if (cellInfo.IsForeignKey) cellInfo.IsShow = false;
            //不显示关联主表列
            else if (cellInfo.CellName.Equals("ParentId")) cellInfo.IsShow = false;
            //关键字列不显示
            else if (cellInfo.CellName.Equals("SearchKeywords")) cellInfo.IsShow = false;

            return cellInfo;
        }
    }
}
