using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Sys
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
        /// 上级表配置
        /// </summary>
        public TableInfo ParentTableInfo { get; set; }
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
        /// 表类型：单表，双表，三表，视图，虚拟
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 表类型：单表，双表，三表，视图，虚拟
        /// </summary>
        public TableType TableType { get { return (TableType)Enum.Parse(typeof(TableType), Type.ToString()); } }
        /// <summary>
        /// 表子类型：普通表,商品表,库存表
        /// </summary>
        public string SubType { get; set; }
        /// <summary>
        /// 表子类型：普通表,商品表,成品表
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
        /// 表列
        /// </summary>
        public List<CellInfo> Cells { get; set; }
        /// <summary>
        /// 子表
        /// </summary>
        public TableInfo SubTable { get; set; }
        /// <summary>
        /// 三表
        /// </summary>
        public TableInfo ThreeTable { get; set; }
        /// <summary>
        /// 是否已经创建
        /// </summary>
        public bool IsBuild { get; set; }
        /// <summary>
        /// 是否系统表
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// 列表编辑
        /// </summary>
        public bool IsListEdit { get; set; }
        /// <summary>
        /// 表备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 报表文件路径
        /// </summary>
        public string ReportFileName { get; set; }


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
        /// 应收应付表
        /// </summary>
        public string YSYFTableName { get; set; }
        /// <summary>
        /// 商品库存表名
        /// </summary>
        public string SPStockTableName { get; set; }

        /// <summary>
        /// 完成数量表
        /// </summary>
        public string WCSLTableName { get; set; }
        /// <summary>
        /// 完成金额表
        /// </summary>
        public string WCJETableName { get; set; }

        /// <summary>
        /// 分页尺码
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 指定引用 仅关键表返回
        /// </summary>
        public bool ZDYY { get; set; }
        /// <summary>
        /// 显示隐藏过滤
        /// </summary>
        public bool XSYCGL { get; set; }
        /// <summary>
        /// 自动查询
        /// </summary>
        public bool ZDCX { get; set; }

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
        /// 打印图片
        /// </summary>
        public bool DYTP { get; set; }
        /// <summary>
        /// 不分页
        /// </summary>
        public bool BFY { get; set; }
        /// <summary>
        /// 显示主键
        /// </summary>
        public bool XSZJ { get; set; }
        /// <summary>
        /// 单条打印
        /// </summary>
        public bool DTDY { get; set; }
        /// <summary>
        /// 引用后移除
        /// </summary>
        public bool YYHYC { get; set; }
        /// <summary>
        /// 引用后关闭
        /// </summary>
        public bool YYHGB { get; set; }
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
            get { return TableType == TableType.单表 || TableType == TableType.双表 || TableType == TableType.三表; }
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
        /// 是否根据KHID过滤客户
        /// </summary>
        public bool IsFilterKHById { get; set; }

        /// <summary>
        /// 表事件
        /// </summary>
        public List<EventInfo> Events { get; set; }
        /// <summary>
        /// 右键菜单
        /// </summary>
        public List<MenuInfo> Menus { get; set; }
        /// <summary>
        /// 视图列
        /// </summary>
        public List<string> ViewCells { get; set; }

        /// <summary>
        /// 明细最大记录条数
        /// </summary>
        public int MXZDSL { get; set; }
        /// <summary>
        /// 明细是否可导入数据
        /// </summary>
        public bool MXDRSJ { get; set; }
        /// <summary>
        /// 对比引用数量
        /// </summary>
        public bool DBYYSL { get; set; }
        /// <summary>
        /// 对比引用金额
        /// </summary>
        public bool DBYYJE { get; set; }
        /// <summary>
        /// 不可大于引用
        /// </summary>
        public bool BKDYYY { get; set; }
        /// <summary>
        /// 不可小于引用
        /// </summary>
        public bool BKXYYY { get; set; }
        /// <summary>
        /// 不可大于引用比例
        /// </summary>
        public int BKDYYYBL { get; set; }
        /// <summary>
        /// 不可小于引用比例
        /// </summary>
        public int BKXYYYBL { get; set; }
        /// <summary>
        ///  不可大于库存
        /// </summary>
        public bool BKDYKC { get; set; }
        /// <summary>
        ///  审核不可大于库存
        /// </summary>
        public bool SHBKDYKC { get; set; }
        /// <summary>
        /// 预览图片
        /// </summary>
        public bool YLTP { get; set; }

        /// <summary>
        /// 主表过滤条件
        /// </summary>
        public string Wheres { get; set; }
        /// <summary>
        /// 子表过滤条件
        /// </summary>
        public string SubWheres { get; set; }
        /// <summary>
        /// 查询SQL
        /// </summary>
        public string QuerySQL { get; set; }

        /// <summary>
        /// 行颜色格式[DJ]*[SL]<0,#FF0000;[JE]<100,#FFFF00;
        /// </summary>
        public string RowColorFormat { get; set; }

        /// <summary>
        /// 是否记住单价
        /// </summary>
        public bool RememberPrice { get; set; }
        /// <summary>
        /// 是否保存价格历史
        /// </summary>
        public bool EnablePriceHistorys { get; set; }

        /// <summary>
        /// 是否非系统表
        /// </summary>
        public bool IsNotSystemTable { get { return TableName.StartsWith("C_"); } }
        /// <summary>
        /// 模块编号
        /// </summary>
        public long ModuleId { get; set; }
        /// <summary>
        /// 模块明细
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 主表比例
        /// </summary>
        public double TopBL { get; set; }
        /// <summary>
        /// 子表比例
        /// </summary>
        public double BottomBL { get; set; }
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
        /// 返回分组
        /// </summary>
        public int ReturnGroup { get; set; }
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
        /// 是否查询焦点
        /// </summary>
        public bool IsQueryFocus { get; set; }
        /// <summary>
        /// 是否系统列
        /// </summary>
        public bool IsSystem { get; set; }
        /// <summary>
        /// 是否富文本
        /// </summary>
        public bool IsFullText { get; set; }
        /// <summary>
        /// 是否使用UE编辑器
        /// </summary>
        public bool IsUMEditor { get; set; }
        /// <summary>
        /// 是否代码
        /// </summary>
        public bool IsCode { get; set; }
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
        /// 添加默认弹窗
        /// </summary>
        public bool IsAddPopTable { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 是否EAN8条码
        /// </summary>
        public bool IsBarcodeEAN8 { get; set; }
        /// <summary>
        /// 是否EAN13条码
        /// </summary>
        public bool IsBarcodeEAN13 { get; set; }

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
        /// 拼音格式
        /// </summary>
        public string PYGS { get; set; }
        /// <summary>
        /// 生成拼音简码
        /// </summary>
        public string SCPYJM { get; set; }
        /// <summary>
        /// 是否需要权限
        /// </summary>
        public bool IsAuthority { get; set; }

        /// <summary>
        /// 列排序
        /// </summary>
        public int UserCellOrder { get; set; }
        /// <summary>
        /// 列宽度
        /// </summary>
        public int UserCellWidth { get; set; }

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

        /// <summary>
        /// 限制下拉值
        /// </summary>
        public bool XZXLZ { get; set; }

        /// <summary>
        /// 限制最大最小值
        /// </summary>
        public bool XZZDZXZ { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public decimal ZDZ { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public decimal ZXZ { get; set; }


        public bool IsIDentity { get; set; }

        /// <summary>
        /// 列值类型
        /// string,int,long,float,decimal,bool,date,datetime
        /// </summary>
        public Type CellValueType
        {
            get
            {
                switch (ValType)
                {
                    case "string": return typeof(string);
                    case "int": return typeof(int);
                    case "long": return typeof(long);
                    case "float": return typeof(float);
                    case "decimal": return typeof(decimal);
                    case "bool": return typeof(bool);
                    case "date":
                    case "datetime":
                        return typeof(DateTime);
                }

                return typeof(string);
            }
        }
        /// <summary>
        /// 是否数字类型
        /// </summary>
        public bool IsNumberType
        {
            get
            {
                switch (ValType)
                {
                    case "int":
                    case "long":
                    case "float":
                    case "decimal":
                        return true;
                }

                return false;
            }
        }
        /// <summary>
        /// 是否日期类型
        /// </summary>
        public bool IsDateType
        {
            get
            {
                switch (ValType)
                {
                    case "date":
                    case "datetime":
                        return true;
                }

                return false;
            }
        }
    }

    /// <summary>
    /// 事件信息
    /// </summary>
    [Serializable]
    public class EventInfo
    {
        /// <summary>
        /// 事件动作
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 执行前
        /// </summary>
        public string BeforeExecution { get; set; }
        /// <summary>
        /// 执行后
        /// </summary>
        public string AfterExecution { get; set; }
        /// <summary>
        /// 最后执行
        /// </summary>
        public bool IsLastExecute { get; set; }
    }

    /// <summary>
    /// 表菜单信息
    /// </summary>
    [Serializable]
    public class MenuInfo
    {
        /// <summary>
        /// 表ID
        /// </summary>
        public long TableId { get; set; }
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// 执行SQL
        /// </summary>
        public string ExecuteSQL { get; set; }

        /// <summary>
        /// 执行后刷新主表
        /// </summary>
        public bool RefreshTop { get; set; }
        /// <summary>
        /// 执行后刷新从表
        /// </summary>
        public bool RefreshBottom { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }
    }

    /// <summary>
    /// 表类型
    /// </summary>
    [Serializable]
    public enum TableType
    {
        单表,
        双表,
        三表,
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
        商品表,
        库存表,
        客户表,
        供应商表,
        主从视图,
        引用视图
    }
}
