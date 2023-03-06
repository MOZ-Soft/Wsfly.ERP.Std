using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeQuan.Client.PC.AppCode.Models
{
    /// <summary>
    /// 数据表信息
    /// </summary>
    [Serializable]
    public class TableInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 关联表ID
        /// </summary>
        public long ParentId { get; set; }
        /// <summary>
        /// 关联表名
        /// </summary>
        public string ParentTableName { get; set; }
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表名称-中文
        /// </summary>
        public string CnName { get; set; }
        /// <summary>
        /// 表类型：单表，双表，视图，虚拟
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 表类型：单表，双表，视图，虚拟
        /// </summary>
        public TableType TableType { get { return (TableType)Enum.Parse(typeof(TableType), Type.ToString()); } }
        /// <summary>
        /// 表子类型：普通表,成品表,半成品表,物料表,成品库存表,半成品库存表,物料库存表
        /// </summary>
        public string SubType { get; set; }
        /// <summary>
        /// 表子类型：普通表,成品表,半成品表,物料表,成品库存表,半成品库存表,物料库存表
        /// </summary>
        public TableSubType TableSubType
        {
            get
            {
                return string.IsNullOrWhiteSpace(SubType) ?
                    TableSubType.Null :
                    (TableSubType)Enum.Parse(typeof(TableSubType), SubType.ToString());
            }
        }
        /// <summary>
        /// 是否树型结构
        /// </summary>
        public bool IsTree
        {
            get
            {
                if (IsSingleTable && TableSubType == TableSubType.树型结构) return true;
                return false;
            }
        }
        /// <summary>
        /// 表列
        /// </summary>
        public List<CellInfo> Cells { get; set; }
        /// <summary>
        /// 子表
        /// </summary>
        public TableInfo SubTable { get; set; }
        /// <summary>
        /// 是否系统表
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// 表备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 报表文件路径
        /// </summary>
        public string ReportFileName { get; set; }
        /// <summary>
        /// 报表数据存储过程
        /// </summary>
        public string ReportProcName { get; set; }


        /// <summary>
        /// 主表名称 - 视图
        /// </summary>
        public string MainTableName { get; set; }
        /// <summary>
        /// 主表关联列名称 - 视图
        /// </summary>
        public string MainTableCellName { get; set; }
        /// <summary>
        /// 子表名称 - 视图
        /// </summary>
        public string SubTableName { get; set; }
        /// <summary>
        /// 子表关联列名称 - 视图
        /// </summary>
        public string SubTableCellName { get; set; }
        /// <summary>
        /// 存储过程名称 - 视图
        /// </summary>
        public string ProcName { get; set; }


        /// <summary>
        /// 成品库存表名
        /// </summary>
        public string CPStockTableName { get; set; }
        /// <summary>
        /// 半成品库存表名
        /// </summary>
        public string BCPStockTableName { get; set; }
        /// <summary>
        /// 物料库存表名
        /// </summary>
        public string WLStockTableName { get; set; }

        /// <summary>
        /// 完成数量表
        /// </summary>
        public string WCSLTableName { get; set; }
        /// <summary>
        /// 完成金额表
        /// </summary>
        public string WCJETableName { get; set; }

        /// <summary>
        /// MRP计划表
        /// </summary>
        public string MRPTableName { get; set; }
        /// <summary>
        /// MRP明细记录表
        /// </summary>
        public string MRPMXB { get; set; }

        #region 开单增加
        /// <summary>
        /// 开单增加采购库存
        /// </summary>
        public bool KDAddPurchaseCount { get; set; }
        /// <summary>
        /// 开单增加订单库存
        /// </summary>
        public bool KDAddOrderCount { get; set; }
        /// <summary>
        /// 开单增加入库库存
        /// </summary>
        public bool KDAddIntoCount { get; set; }
        /// <summary>
        /// 开单增加出库库存
        /// </summary>
        public bool KDAddOutCount { get; set; }
        /// <summary>
        /// 开单增加库存
        /// </summary>
        public bool KDAddStockCount { get; set; }
        /// <summary>
        /// 开单增加可用库存
        /// </summary>
        public bool KDAddAvailableCount { get; set; }
        /// <summary>
        /// 开单增加生产库存
        /// </summary>
        public bool KDAddProductionCount { get; set; }
        /// <summary>
        /// 开单增加物料库存
        /// </summary>
        public bool KDAddMaterielCount { get; set; }
        /// <summary>
        /// 开单增加需求数量
        /// </summary>
        public bool KDAddXQSL { get; set; }
        #endregion

        #region 开单减少
        /// <summary>
        /// 开单减少采购库存
        /// </summary>
        public bool KDReducePurchaseCount { get; set; }
        /// <summary>
        /// 开单减少订单库存
        /// </summary>
        public bool KDReduceOrderCount { get; set; }
        /// <summary>
        /// 开单减少入库库存
        /// </summary>
        public bool KDReduceIntoCount { get; set; }
        /// <summary>
        /// 开单减少出库库存
        /// </summary>
        public bool KDReduceOutCount { get; set; }
        /// <summary>
        /// 开单减少库存
        /// </summary>
        public bool KDReduceStockCount { get; set; }
        /// <summary>
        /// 开单减少可用库存
        /// </summary>
        public bool KDReduceAvailableCount { get; set; }
        /// <summary>
        /// 开单减少生产库存
        /// </summary>
        public bool KDReduceProductionCount { get; set; }
        /// <summary>
        /// 开单减少物料库存
        /// </summary>
        public bool KDReduceMaterielCount { get; set; }
        /// <summary>
        /// 开单减少需求数量
        /// </summary>
        public bool KDReduceXQSL { get; set; }
        #endregion

        #region 审核增加
        /// <summary>
        /// 审核增加采购库存
        /// </summary>
        public bool SHAddPurchaseCount { get; set; }
        /// <summary>
        /// 审核增加订单库存
        /// </summary>
        public bool SHAddOrderCount { get; set; }
        /// <summary>
        /// 审核增加入库库存
        /// </summary>
        public bool SHAddIntoCount { get; set; }
        /// <summary>
        /// 审核增加出库库存
        /// </summary>
        public bool SHAddOutCount { get; set; }
        /// <summary>
        /// 审核增加库存
        /// </summary>
        public bool SHAddStockCount { get; set; }
        /// <summary>
        /// 审核增加可用库存
        /// </summary>
        public bool SHAddAvailableCount { get; set; }
        /// <summary>
        /// 审核增加生产库存
        /// </summary>
        public bool SHAddProductionCount { get; set; }
        /// <summary>
        /// 审核增加物料库存
        /// </summary>
        public bool SHAddMaterielCount { get; set; }
        /// <summary>
        /// 审核增加需求数量
        /// </summary>
        public bool SHAddXQSL { get; set; }
        #endregion

        #region 审核减少
        /// <summary>
        /// 审核减少采购库存
        /// </summary>
        public bool SHReducePurchaseCount { get; set; }
        /// <summary>
        /// 审核减少订单库存
        /// </summary>
        public bool SHReduceOrderCount { get; set; }
        /// <summary>
        /// 审核减少入库库存
        /// </summary>
        public bool SHReduceIntoCount { get; set; }
        /// <summary>
        /// 审核减少出库库存
        /// </summary>
        public bool SHReduceOutCount { get; set; }
        /// <summary>
        /// 审核减少库存
        /// </summary>
        public bool SHReduceStockCount { get; set; }
        /// <summary>
        /// 审核减少可用库存
        /// </summary>
        public bool SHReduceAvailableCount { get; set; }
        /// <summary>
        /// 审核减少生产库存
        /// </summary>
        public bool SHReduceProductionCount { get; set; }
        /// <summary>
        /// 审核减少物料库存
        /// </summary>
        public bool SHReduceMaterielCount { get; set; }
        /// <summary>
        /// 审核减少需求数量
        /// </summary>
        public bool SHReduceXQSL { get; set; }
        #endregion

        #region 应收款、应付款
        /// <summary>
        /// 开单添加应收款
        /// </summary>
        public bool KDAddYSK { get; set; }
        /// <summary>
        /// 开单添加应付款
        /// </summary>
        public bool KDAddYFK { get; set; }
        /// <summary>
        /// 开单减少应收款
        /// </summary>
        public bool KDReduceYSK { get; set; }
        /// <summary>
        /// 开单减少应付款
        /// </summary>
        public bool KDReduceYFK { get; set; }
        /// <summary>
        /// 审核添加应收款
        /// </summary>
        public bool SHAddYSK { get; set; }
        /// <summary>
        /// 审核添加应付款
        /// </summary>
        public bool SHAddYFK { get; set; }
        /// <summary>
        /// 审核减少应收款
        /// </summary>
        public bool SHReduceYSK { get; set; }
        /// <summary>
        /// 审核减少应付款
        /// </summary>
        public bool SHReduceYFK { get; set; }
        #endregion

        #region 报表名
        /// <summary>
        /// 进库表
        /// </summary>
        public string JKB { get; set; }
        /// <summary>
        /// 进库退货表
        /// </summary>
        public string JKTHB { get; set; }
        /// <summary>
        /// 出库表
        /// </summary>
        public string CKB { get; set; }
        /// <summary>
        /// 出库退货表
        /// </summary>
        public string CKTHB { get; set; }
        /// <summary>
        /// 盘点表名
        /// </summary>
        public string PDB { get; set; }
        #endregion

        /// <summary>
        /// 审核后打印
        /// </summary>
        public bool SHHDY { get; set; }
        /// <summary>
        /// 文件不可下载
        /// </summary>
        public bool WJBKXZ { get; set; }
        /// <summary>
        /// 不分页
        /// </summary>
        public bool BFY { get; set; }
        /// <summary>
        /// 显示主键
        /// </summary>
        public bool XSZJ { get; set; }
        /// <summary>
        /// 数据排序
        /// </summary>
        public string TableOrder { get; set; }
        /// <summary>
        /// 数据排序类型
        /// </summary>
        public TableOrderType TableOrderType
        {
            get
            {
                try
                {
                    return string.IsNullOrWhiteSpace(TableOrder) ? TableOrderType.默认 : (TableOrderType)Enum.Parse(typeof(TableOrderType), TableOrder.ToString());
                }
                catch { }

                return TableOrderType.默认;
            }
        }

        /// <summary>
        /// 是否单表结构
        /// </summary>
        public bool IsSingleTable
        {
            get { return TableType == TableType.单表 || TableType == TableType.视图; }
        }
        /// <summary>
        /// 是否有真实数据表
        /// </summary>
        public bool IsRealTable
        {
            get { return TableType == TableType.单表 || TableType == TableType.双表; }
        }
        /// <summary>
        /// 是否视图
        /// </summary>
        public bool IsViewTable
        {
            get { return TableType == TableType.视图; }
        }
        /// <summary>
        /// 是否虚拟表
        /// </summary>
        public bool IsVTable
        {
            get { return TableType == TableType.虚拟; }
        }

        /// <summary>
        /// 是否过滤客户
        /// </summary>
        public bool IsFilterKH { get; set; }

        /// <summary>
        /// 表事件
        /// </summary>
        public List<EventInfo> Events { get; set; }
        /// <summary>
        /// 视图列
        /// </summary>
        public List<string> ViewCells { get; set; }


        /// <summary>
        /// 不可大于引用
        /// </summary>
        public bool BKDYYY { get; set; }
        /// <summary>
        /// 不可小于引用
        /// </summary>
        public bool BKXYYY { get; set; }
        /// <summary>
        /// 预览图片
        /// </summary>
        public bool YLTP { get; set; }
        /// <summary>
        /// 初始查询条件
        /// </summary>
        public bool CSCXTJ { get; set; }

        /// <summary>
        /// 主表过滤条件
        /// </summary>
        public string Wheres { get; set; }
        /// <summary>
        /// 子表过滤条件
        /// </summary>
        public string SubWheres { get; set; }
    }

    /// <summary>
    /// 表列信息
    /// </summary>
    [Serializable]
    public class CellInfo
    {
        /// <summary>
        /// 列编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string CellName { get; set; }
        /// <summary>
        /// 列名-中文
        /// </summary>
        public string CnName { get; set; }
        /// <summary>
        /// 列宽度
        /// </summary>
        public int CellWidth { get; set; }
        /// <summary>
        /// 是否可以编辑
        /// </summary>
        public bool CanEdit { get; set; }
        /// <summary>
        /// 值类型
        /// </summary>
        public string ValType { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 公式
        /// </summary>
        public string Formula { get; set; }
        /// <summary>
        /// 拼音简码分组
        /// </summary>
        public int PYGroup { get; set; }
        /// <summary>
        /// 序号类型
        /// </summary>
        public string SerialNoType { get; set; }
        /// <summary>
        /// 序号格式
        /// </summary>
        public string SerialNoFormat { get; set; }
        /// <summary>
        /// 外键表
        /// </summary>
        public long ForeignTableId { get; set; }
        /// <summary>
        /// 外键表名
        /// </summary>
        public string ForeignTableName { get; set; }
        /// <summary>
        /// 是否下拉列表
        /// </summary>
        public bool IsDropDown { get; set; }
        /// <summary>
        /// 返回选择表的对应列
        /// </summary>
        public string ReturnCellName { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; }
        /// <summary>
        /// 是否关联主键列
        /// </summary>
        public bool IsForeignKey { get; set; }
        /// <summary>
        /// 是否查询列
        /// </summary>
        public bool IsQuery { get; set; }
        /// <summary>
        /// 是否系统列
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// 是否富文本
        /// </summary>
        public bool IsFullText { get; set; }
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool AllownNull { get; set; }
        /// <summary>
        /// 是否允许重复
        /// </summary>
        public bool AllownRepeat { get; set; }
        /// <summary>
        /// 是否弹出编辑
        /// </summary>
        public bool IsPopEdit { get; set; }
        /// <summary>
        /// 是否弹出显示
        /// </summary>
        public bool IsPopShow { get; set; }
        /// <summary>
        /// 是否文件
        /// </summary>
        public bool IsFile { get; set; }
        /// <summary>
        /// 是否图片
        /// </summary>
        public bool IsPic { get; set; }
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypt { get; set; }
        /// <summary>
        /// 是否地区
        /// </summary>
        public bool IsArea { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否搜索关键字列
        /// </summary>
        public bool IsSearchKeywords { get; set; }
        /// <summary>
        /// 小数位数
        /// </summary>
        public int DecimalDigits { get; set; }
        /// <summary>
        /// 汇总统计
        /// </summary>
        public bool IsSummary { get; set; }

        /// <summary>
        /// 显示比例
        /// </summary>
        public bool ShowBL { get; set; }
        /// <summary>
        /// 树型显示
        /// </summary>
        public bool ShowSX { get; set; }
        /// <summary>
        /// 显示公式
        /// </summary>
        public string ShowGS { get; set; }
        /// <summary>
        /// 日期范围
        /// </summary>
        public string RQFW { get; set; }

        /// <summary>
        /// 进库表
        /// </summary>
        public string JKB { get; set; }
        /// <summary>
        /// 进库退货
        /// </summary>
        public string JKTHB { get; set; }
        /// <summary>
        /// 出库表
        /// </summary>
        public string CKB { get; set; }
        /// <summary>
        /// 出库退货表
        /// </summary>
        public string CKTHB { get; set; }
        /// <summary>
        /// 盘点表
        /// </summary>
        public string PDB { get; set; }
    }

    /// <summary>
    /// 事件信息
    /// </summary>
    [Serializable]
    public class EventInfo
    {
        /// <summary>
        /// 执行代码
        /// </summary>
        public int ActionCode { get; set; }
        /// <summary>
        /// 执行前执行
        /// </summary>
        public string ClickingProc { get; set; }
        /// <summary>
        /// 执行后执行
        /// </summary>
        public string ClickedProc { get; set; }
    }

    /// <summary>
    /// 表类型
    /// </summary>
    [Serializable]
    public enum TableType
    {
        单表,
        双表,
        视图,
        虚拟
    }
    /// <summary>
    /// 数据排序
    /// </summary>
    [Serializable]
    public enum TableOrderType
    {
        默认,
        顺序,
        倒序
    }
    /// <summary>
    /// 表子类型
    /// </summary>
    [Serializable]
    public enum TableSubType
    {
        Null,
        普通表,
        空表,
        成品表,
        半成品表,
        物料表,
        成品库存表,
        半成品库存表,
        物料库存表,
        客户表,
        供应商表,
        树型结构,
        树型结构明细,
        客户汇总,
        商品汇总,
        进出存报表,
        主从视图,
        引用视图
    }
}
