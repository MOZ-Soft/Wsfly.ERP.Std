using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Service.Exts;
using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// QuickBuildTableUC.xaml 的交互逻辑
    /// </summary>
    public partial class QuickBuildTableUC : BaseUserControl
    {
        //表信息
        TableInfo _tableInfo = null;

        /// <summary>
        /// 快速建表 构造
        /// </summary>
        public QuickBuildTableUC()
        {
            //表配置
            _tableInfo = AppGlobal.GetTableConfig(1);
            _tableInfo.SubTable = AppGlobal.GetTableConfig(2);

            //构造
            InitializeComponent();

            //窗口大小改变
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;

            //加载事件
            this.Loaded += QuickBuildTableUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuickBuildTableUC_Loaded(object sender, RoutedEventArgs e)
        {
            //设置页面大小
            SetUCSize();

            //点击切换Tab
            foreach (UIElement uiTabHeader in this.panelTabHeaders.Children)
            {
                if (uiTabHeader.GetType() == typeof(Button))
                {
                    Button btnHeader = uiTabHeader as Button;
                    btnHeader.Click += BtnHeader_Click;
                }
            }

            //表名
            this.txtCnName.TextChanged += TxtCnName_TextChanged;

            //主从表参数及事件
            this.ddlZCZDKHGL.TableId = 1;
            this.ddlZCZDKHGL.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlZCZDKHGL.ChooseCallBack_Event += DdlZCZDKHGL_ChooseCallBack_Event;
            this.ddlZCMXSPGL.TableId = 1;
            this.ddlZCMXSPGL.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlZCMXSPGL.ChooseCallBack_Event += DdlZCMXSPGL_ChooseCallBack_Event;
            this.ddlZCMXKCB.TableId = 1;
            this.ddlZCMXKCB.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlZCMXKCB.ChooseCallBack_Event += DdlZCMXKCB_ChooseCallBack_Event; ;
            this.btnZCBuild.Click += BtnZCBuild_Click;

            //三表参数及事件
            this.ddlSBZDKHGL.TableId = 1;
            this.ddlSBZDKHGL.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlSBZDKHGL.ChooseCallBack_Event += DdlSBZDKHGL_ChooseCallBack_Event;
            this.ddlSBMXSPGL.TableId = 1;
            this.ddlSBMXSPGL.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlSBMXSPGL.ChooseCallBack_Event += DdlSBMXSPGL_ChooseCallBack_Event;
            this.ddlSBKZSPGL.TableId = 1;
            this.ddlSBKZSPGL.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlSBKZSPGL.ChooseCallBack_Event += DdlSBKZSPGL_ChooseCallBack_Event;
            this.ddlSBMXKCB.TableId = 1;
            this.ddlSBMXKCB.Txt_Change_Event += DDLSelectTable_Txt_Change_Event;
            this.ddlSBMXKCB.ChooseCallBack_Event += DdlSBMXKCB_ChooseCallBack_Event;
            this.btnSBBuild.Click += BtnSBBuild_Click; ;
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //设置页面大小
            SetUCSize();
        }
        /// <summary>
        /// 设置页面大小
        /// </summary>
        private void SetUCSize()
        {
            this.scrollMain.Height = AppData.MainWindow.WinHeight - 2 - 100 - 40;
        }
        /// <summary>
        /// 点击Tab头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHeader_Click(object sender, RoutedEventArgs e)
        {
            //切换样式
            foreach (UIElement uiTabHeader in this.panelTabHeaders.Children)
            {
                if (uiTabHeader.GetType() == typeof(Button))
                {
                    Button btnHeader = uiTabHeader as Button;
                    btnHeader.Foreground = Brushes.Black;
                    btnHeader.Background = Brushes.Transparent;
                }
            }

            foreach (UIElement uiTabContent in this.panelTabContents.Children)
            {
                if (uiTabContent.GetType() == typeof(StackPanel))
                {
                    (uiTabContent as StackPanel).Visibility = Visibility.Collapsed;
                }
            }

            //当前按钮
            Button btnCurrent = sender as Button;
            btnCurrent.Foreground = Brushes.White;
            btnCurrent.Background = Brushes.Black;

            string tag = btnCurrent.Tag.ToString();

            //当前内容
            StackPanel panelCurrent = this.panelTabContents.FindName(tag) as StackPanel;
            panelCurrent.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 中文表名改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtCnName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cn = this.txtCnName.Text.Trim();
            string en = "C_" + Core.Handler.ChineseHandler.GetFirstPYLetters(cn);
            this.txtEnName.Text = en;
        }


        #region 主从双表
        long zckhglbID = 0;
        string zckhglbCN = "";
        string zckhglbEN = "";

        long zcspglbID = 0;
        string zcspglbCN = "";
        string zcspglbEN = "";

        long zckcbID = 0;
        string zckcbCN = "";
        string zckcbEN = "";

        /// <summary>
        /// 选择主从客户关联
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlZCZDKHGL_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            zckhglbID = DataType.Long(row["Id"], 0);
            zckhglbCN = row["CnName"].ToString();
            zckhglbEN = row["TableName"].ToString();

            this.ddlZCZDKHGLBM.Text = zckhglbEN;
            this.ddlZCMXSPGL.SetFocus();
        }
        /// <summary>
        /// 选择主从商品关联
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlZCMXSPGL_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            zcspglbID = DataType.Long(row["Id"], 0);
            zcspglbCN = row["CnName"].ToString();
            zcspglbEN = row["TableName"].ToString();

            this.ddlZCMXSPGLBM.Text = zcspglbEN;
            this.ddlZCMXKCB.SetFocus();
        }
        /// <summary>
        /// 选择主从库存表
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlZCMXKCB_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            zckcbID = DataType.Long(row["Id"], 0);
            zckcbCN = row["CnName"].ToString();
            zckcbEN = row["TableName"].ToString();

            this.ddlZCMXKCBBM.Text = zckcbEN;
            this.cbZCBuildZCBB.Focus();
        }
        /// <summary>
        /// 生成表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZCBuild_Click(object sender, RoutedEventArgs e)
        {
            string cn = this.txtCnName.Text.Trim();
            string en = this.txtEnName.Text.Trim();

            if (!en.StartsWith("C_")) en = "C_" + en;

            bool scZCBB = this.cbZCBuildZCBB.IsChecked.Value;
            bool scYYBB = this.cbZCBuildYYBB.IsChecked.Value;

            bool autoAudit = this.cbZCAutoAudit.IsChecked.Value;

            if (string.IsNullOrWhiteSpace(cn))
            {
                AppAlert.FormTips(gridMain, "请输入表中文名称！");
                return;
            }

            if (string.IsNullOrWhiteSpace(en))
            {
                AppAlert.FormTips(gridMain, "请输入表英文名称！");
                return;
            }

            if (zckhglbID <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择主表客户引用表！");
                return;
            }

            if (zcspglbID <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择从表商品引用表！");
                return;
            }

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread threadBuildZCTable = new System.Threading.Thread(delegate ()
            {
                //生成主从表
                BuildZCTable(cn, en, scZCBB, scYYBB, autoAudit);
            });
            threadBuildZCTable.IsBackground = true;
            threadBuildZCTable.Start();
        }
        /// <summary>
        /// 生成主从表
        /// </summary>
        private void BuildZCTable(string cn, string en, bool scZCBB, bool scYYBB, bool autoAudit)
        {
            try
            {
                string msg = "";

                //判断表名是否存在
                SQLParam param = new SQLParam()
                {

                    TableName = _tableInfo.TableName,
                    Wheres = new List<Where>()
                    {
                        new Where() { Type = WhereType.左括号 },
                        new Where() { CellName="TableName", CellValue=en, Parallel=ParallelType.Or },
                        new Where() { CellName="TableName", CellValue=en+"MX", Parallel=ParallelType.Or },
                        new Where() { Type = WhereType.右括号 },
                    }
                };
                long count = SQLiteDao.GetCount(param);
                if (count > 0)
                {
                    Alert("创建主从表失败，原因：\r\n主表或从表表名已经存在！");
                    HideLoading();
                    return;
                }

                //应收应付表名
                string ysyfTableName = "";
                TableInfo tableYSYF = AppGlobal.GetTableConfig(zckhglbEN);
                if (tableYSYF.TableSubType == TableSubType.客户表 || tableYSYF.TableSubType == TableSubType.供应商表)
                {
                    ysyfTableName = zckhglbEN;
                }


                //应收
                bool kdAddYS = false;
                bool kdReduceYS = false;
                bool shAddYS = false;
                bool shReduceYS = false;

                //应付
                bool kdAddYF = false;
                bool kdReduceYF = false;
                bool shAddYF = false;
                bool shReduceYF = false;

                //库存
                bool kdAddKC = false;
                bool kdReduceKC = false;
                bool shAddKC = false;
                bool shReduceKC = false;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //应收
                    kdAddYS = this.cbZCKDAddYS.IsChecked.Value;
                    kdReduceYS = this.cbZCKDReduceYS.IsChecked.Value;
                    shAddYS = this.cbZCSHAddYS.IsChecked.Value;
                    shReduceYS = this.cbZCSHReduceYS.IsChecked.Value;

                    //应付
                    kdAddYF = this.cbZCKDAddYF.IsChecked.Value;
                    kdReduceYF = this.cbZCKDReduceYF.IsChecked.Value;
                    shAddYF = this.cbZCSHAddYF.IsChecked.Value;
                    shReduceYF = this.cbZCSHReduceYF.IsChecked.Value;

                    //库存
                    kdAddKC = this.cbZCKDAddKC.IsChecked.Value;
                    kdReduceKC = this.cbZCKDReduceKC.IsChecked.Value;
                    shAddKC = this.cbZCSHAddKC.IsChecked.Value;
                    shReduceKC = this.cbZCSHReduceKC.IsChecked.Value;
                }));

                //创建主表
                SQLParam insertParams = new SQLParam()
                {
                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", 0),
                        new KeyValue("ParentTableName", ""),
                        new KeyValue("Type", "双表"),
                        new KeyValue("SubType", "普通表"),
                        new KeyValue("CnName", cn),
                        new KeyValue("TableName", en),
                        new KeyValue("IsSystem", false),
                        new KeyValue("IsBuild", false),
                        new KeyValue("IsListEdit", true),
                        new KeyValue("IsAudit", false),
                        new KeyValue("IsFilterKH", false),
                        new KeyValue("IsFilterKHById", false),
                        new KeyValue("ZDCX", true),
                        new KeyValue("SearchKeywords", cn + en),
                        new KeyValue("MainTableId", 0),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("SubTableId", 0),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("YSYFTableName", ysyfTableName),
                        new KeyValue("SPStockTableName", ""),
                        new KeyValue("WCSLTableName", ""),

                        new KeyValue("KDAddYSK", kdAddYS),
                        new KeyValue("KDReduceYSK", kdReduceYS),
                        new KeyValue("SHAddYSK", shAddYS),
                        new KeyValue("SHReduceYSK", shReduceYS),

                        new KeyValue("KDAddYFK", kdAddYF),
                        new KeyValue("KDReduceYFK", kdReduceYF),
                        new KeyValue("SHAddYFK", shAddYF),
                        new KeyValue("SHReduceYFK", shReduceYF),

                        new KeyValue("YLTP", true),
                        new KeyValue("DYTP", true),
                        new KeyValue("DBYYSL", false),
                        new KeyValue("DBYYJE", false),
                    }
                };
                long zbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                if (zbid <= 0)
                {
                    Alert("创建主表失败，原因：\r\n" + msg);
                    return;
                }

                try
                {
                    //设置主表客户列关联表
                    string sql = "update [Sys_TableCells] set [ForeignTableId]=" + zckhglbID + ",[ForeignTableName]='" + zckhglbEN + "',[IsAddPopTable]=1 where [ParentId]=" + zbid + " and ([CellName]='KHID' or [CellName]='KHMC' or [CellName]='KHBH')";
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch { }

                //创建从表
                insertParams = new SQLParam()
                {
                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", zbid),
                        new KeyValue("ParentTableName", en),
                        new KeyValue("Type", "双表"),
                        new KeyValue("SubType", "普通表"),
                        new KeyValue("CnName", cn+"明细"),
                        new KeyValue("TableName", en+"MX"),
                        new KeyValue("IsSystem", false),
                        new KeyValue("IsBuild", false),
                        new KeyValue("IsListEdit", true),
                        new KeyValue("IsAudit", false),
                        new KeyValue("IsFilterKH", false),
                        new KeyValue("IsFilterKHById", false),
                        new KeyValue("ZDCX", true),
                        new KeyValue("SearchKeywords", cn+ "明细" + en + "MX"),
                        new KeyValue("MainTableId", zbid),
                        new KeyValue("MainTableName", en),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("SubTableId", 0),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("YSYFTableName", ""),
                        new KeyValue("SPStockTableName", zckcbEN),
                        new KeyValue("WCSLTableName", ""),

                        new KeyValue("KDAddIntoCount", kdAddKC),//加入库数量
                        new KeyValue("KDAddStockCount", kdAddKC),
                        new KeyValue("KDAddAvailableCount", kdAddKC),
                        new KeyValue("KDAddOutCount", kdReduceKC), //加出库数量
                        new KeyValue("KDReduceStockCount", kdReduceKC),
                        new KeyValue("KDReduceAvailableCount", kdReduceKC),

                        new KeyValue("SHAddIntoCount", shAddKC),//加入库数量
                        new KeyValue("SHAddStockCount", shAddKC),
                        new KeyValue("SHAddAvailableCount", shAddKC),
                        new KeyValue("SHAddOutCount", shReduceKC), //加出库数量
                        new KeyValue("SHReduceStockCount", shReduceKC),
                        new KeyValue("SHReduceAvailableCount", shReduceKC),

                        new KeyValue("YLTP", true),
                        new KeyValue("DYTP", true),
                        new KeyValue("BKDYYY", true),
                        new KeyValue("DBYYSL", true),
                        new KeyValue("DBYYJE", false),
                    }
                };
                long mxid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                if (mxid <= 0)
                {
                    Alert("创建从表失败，原因：\r\n" + msg);
                    return;
                }

                //修改主表的子表信息
                DataService.Update(AppGlobal.UserInfo, _tableInfo, new SQLParam()
                {
                    Id = zbid,
                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("SubTableId", mxid),
                        new KeyValue("SubTableName", en+"MX"),
                    }
                }, ref msg);

                try
                {
                    //设置从表商品列关联表
                    string sql = "update [Sys_TableCells] set [ForeignTableId]=" + zcspglbID + ",[ForeignTableName]='" + zcspglbEN + "',[IsAddPopTable]=1 where [ParentId]=" + mxid + " and ([CellName]='SPID' or [CellName]='SPMC' or [CellName]='SPBH')";

                    //是否引用视图
                    if (zcspglbEN.StartsWith("C_YY"))
                    {
                        if (zcspglbEN.EndsWith("KZ"))
                        {
                            //引用表为三表扩展视图
                            sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(ifnull([ReturnCellName],''),'MZMX_','MZKZ_') where [ParentId]=" + mxid;
                            sql += "; update [Sys_TableCells] set [ReturnCellName]='MZKZBM' where [CellName]='SYSYYBM' and [ParentId]=" + mxid;
                        }
                    }
                    else
                    {
                        //是非引用视图时 去除返回列的前缀
                        sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(replace(ifnull([ReturnCellName],''),'MZMX_',''),'MZZB_','') where [ParentId]=" + mxid;
                    }

                    SQLiteDao.ExecuteSQL(sql);
                }
                catch { }

                //是否生成明细报表
                if (scZCBB)
                {
                    string zcbm = en + "MXBB";

                    insertParams = new SQLParam()
                    {

                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", zbid),
                            new KeyValue("ParentTableName", en),
                            new KeyValue("Type", "视图"),
                            new KeyValue("SubType", "主从视图"),
                            new KeyValue("CnName", cn+"明细报表"),
                            new KeyValue("TableName", zcbm),
                            new KeyValue("IsSystem", false),
                            new KeyValue("IsBuild", false),
                            new KeyValue("IsListEdit", true),
                            new KeyValue("IsAudit", false),
                            new KeyValue("IsFilterKH", false),
                            new KeyValue("IsFilterKHById", false),
                            new KeyValue("ZDCX", true),
                            new KeyValue("SearchKeywords", cn+ "明细报表" + zcbm),
                            new KeyValue("MainTableId", zbid),
                            new KeyValue("MainTableName", en),
                            new KeyValue("MainTableName", ""),
                            new KeyValue("SubTableId", mxid),
                            new KeyValue("SubTableName", en+"MX"),
                            new KeyValue("SubTableName", ""),
                            new KeyValue("YSYFTableName", ""),
                            new KeyValue("SPStockTableName", ""),
                            new KeyValue("WCSLTableName", ""),
                            new KeyValue("YLTP", true),
                            new KeyValue("DYTP", true),
                            new KeyValue("DBYYSL", false),
                            new KeyValue("DBYYJE", false),
                        }
                    };

                    //创建明细报表
                    long zcbbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                    if (zcbbid <= 0)
                    {
                        Alert("创建明细报表失败，原因：\r\n" + msg);
                        return;
                    }
                }

                //是否生成引用报表
                if (scYYBB)
                {
                    string yybm = en.Replace("C_", "C_YY");

                    insertParams = new SQLParam()
                    {
                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", zbid),
                            new KeyValue("ParentTableName", en),
                            new KeyValue("Type", "视图"),
                            new KeyValue("SubType", "引用视图"),
                            new KeyValue("CnName", "引用" + cn),
                            new KeyValue("TableName", yybm),
                            new KeyValue("IsSystem", false),
                            new KeyValue("IsBuild", false),
                            new KeyValue("IsListEdit", true),
                            new KeyValue("IsAudit", false),
                            new KeyValue("IsFilterKH", true),
                            new KeyValue("IsFilterKHById", true),
                            new KeyValue("YYHYC", true),
                            new KeyValue("YYHGB", true),
                            new KeyValue("ZDCX", true),
                            new KeyValue("SearchKeywords", "引用" + cn + yybm),
                            new KeyValue("MainTableId", zbid),
                            new KeyValue("MainTableName", en),
                            new KeyValue("MainTableName", ""),
                            new KeyValue("SubTableId", mxid),
                            new KeyValue("SubTableName", en+"MX"),
                            new KeyValue("SubTableName", ""),
                            new KeyValue("YSYFTableName", ""),
                            new KeyValue("SPStockTableName", ""),
                            new KeyValue("WCSLTableName", ""),
                            new KeyValue("YLTP", true),
                            new KeyValue("DYTP", true),
                            new KeyValue("DBYYSL", false),
                            new KeyValue("DBYYJE", false),
                        }
                    };

                    //创建引用表
                    long yybbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                    if (yybbid <= 0)
                    {
                        Alert("创建引用报表失败，原因：\r\n" + msg);
                        return;
                    }

                    //添加到引用模块
                    InsertToYYModule(yybbid, yybm, "引用" + cn);
                }

                try
                {
                    //设置表索引
                    string sql = "update [Sys_Tables] set [DataIndex]=[Id] where [DataIndex]=1 or [DataIndex]=0 or [DataIndex] is null";
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch (Exception ex) { }

                try
                {
                    //设置表列为系统列
                    string sql = "update [Sys_TableCells] set [IsSystem]=1 where [ParentId]=" + zbid + " or [ParentId]=" + mxid;
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch (Exception ex) { }

                //是否自动审核
                if (autoAudit)
                {
                    try
                    {
                        //调用审核主表
                        DataService.Audit(AppGlobal.UserInfo, _tableInfo, zbid, ref msg);

                        //调用审核从表
                        DataService.Audit(AppGlobal.UserInfo, _tableInfo, mxid, ref msg);
                    }
                    catch (Exception ex) { }
                }

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //需要刷新表配置
                    AppData.MainWindow.ShowRenewTC = true;

                    //生成成功
                    AppAlert.FormTips(gridMain, "生成主从双表成功！", AppCode.Enums.FormTipsType.Right);

                    this.txtCnName.Text = "";
                    this.txtEnName.Text = "";

                    this.ddlZCZDKHGL.SearchText = "";
                    this.ddlZCZDKHGLBM.Text = "";
                    this.ddlZCMXSPGL.SearchText = "";
                    this.ddlZCMXSPGLBM.Text = "";
                    this.ddlZCMXKCB.SearchText = "";
                    this.ddlZCMXKCBBM.Text = "";

                    zckhglbID = 0;
                    zckhglbCN = "";
                    zckhglbEN = "";
                    zcspglbID = 0;
                    zcspglbCN = "";
                    zcspglbEN = "";
                    zckcbID = 0;
                    zckcbCN = "";
                    zckcbEN = "";
                    return null;
                }));
            }
            catch (Exception ex)
            {
                Alert("生成主从双表失败，原因：" + ex.Message);
            }
            finally
            {
                HideLoading();
            }
        }

        #endregion

        #region 三表结构
        long sbkhglbID = 0;
        string sbkhglbCN = "";
        string sbkhglbEN = "";
        long sbmxspglbID = 0;
        string sbmxspglbCN = "";
        string sbmxspglbEN = "";
        long sbkzspglbID = 0;
        string sbkzspglbCN = "";
        string sbkzspglbEN = "";
        long sbmxkcbID = 0;
        string sbmxkcbCN = "";
        string sbmxkcbEN = "";

        /// <summary>
        /// 三表主表客户关联
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlSBZDKHGL_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            sbkhglbID = DataType.Long(row["Id"], 0);
            sbkhglbCN = row["CnName"].ToString();
            sbkhglbEN = row["TableName"].ToString();

            this.ddlSBZDKHGLBM.Text = zckhglbEN;
            this.ddlSBMXSPGL.SetFocus();
        }
        /// <summary>
        /// 明细表商品关联
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlSBMXSPGL_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            sbmxspglbID = DataType.Long(row["Id"], 0);
            sbmxspglbCN = row["CnName"].ToString();
            sbmxspglbEN = row["TableName"].ToString();

            this.ddlSBMXSPGLBM.Text = zckhglbEN;
            this.ddlSBKZSPGL.SetFocus();
        }
        /// <summary>
        /// 扩展表商品关联
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlSBKZSPGL_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            sbkzspglbID = DataType.Long(row["Id"], 0);
            sbkzspglbCN = row["CnName"].ToString();
            sbkzspglbEN = row["TableName"].ToString();

            this.ddlSBKZSPGLBM.Text = sbkzspglbEN;
            this.ddlSBMXKCB.SetFocus();
        }
        /// <summary>
        /// 明细表关联库存表
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdlSBMXKCB_ChooseCallBack_Event(Controls.DropDownTable uc, System.Data.DataRow row)
        {
            sbmxkcbID = DataType.Long(row["Id"], 0);
            sbmxkcbCN = row["CnName"].ToString();
            sbmxkcbEN = row["TableName"].ToString();

            this.ddlSBMXKCBBM.Text = zckhglbEN;
        }
        /// <summary>
        /// 创建三表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSBBuild_Click(object sender, RoutedEventArgs e)
        {
            string cn = this.txtCnName.Text.Trim();
            string en = this.txtEnName.Text.Trim();

            if (!en.StartsWith("C_")) en = "C_" + en;

            bool scZCBB = this.cbSBBuildZCBB.IsChecked.Value;
            bool scYYBB = this.cbSBBuildYYBB.IsChecked.Value;

            bool autoAudit = this.cbSBAutoAudit.IsChecked.Value;

            if (string.IsNullOrWhiteSpace(cn))
            {
                AppAlert.FormTips(gridMain, "请输入表中文名称！");
                return;
            }

            if (string.IsNullOrWhiteSpace(en))
            {
                AppAlert.FormTips(gridMain, "请输入表英文名称！");
                return;
            }

            if (sbkhglbID <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择主表客户引用表！");
                return;
            }

            if (sbmxspglbID <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择从表商品引用表！");
                return;
            }

            if (sbkzspglbID <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择扩展表商品引用表！");
                return;
            }

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread threadBuildSBTable = new System.Threading.Thread(delegate ()
            {
                //生成三表
                BuildSBTable(cn, en, scZCBB, scYYBB, autoAudit);
            });
            threadBuildSBTable.IsBackground = true;
            threadBuildSBTable.Start();
        }
        /// <summary>
        /// 生成三表结构
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="en"></param>
        /// <param name="scZCBB"></param>
        /// <param name="scYYBB"></param>
        /// <param name="autoAudit"></param>
        private void BuildSBTable(string cn, string en, bool scZCBB, bool scYYBB, bool autoAudit)
        {
            try
            {
                string msg = "";

                //======================================================
                //判断表名是否存在
                SQLParam param = new SQLParam()
                {

                    TableName = _tableInfo.TableName,
                    Wheres = new List<Where>()
                    {
                        new Where() { Type = WhereType.左括号 },
                        new Where() { CellName="TableName", CellValue=en, Parallel=ParallelType.Or },
                        new Where() { CellName="TableName", CellValue=en+"MX", Parallel=ParallelType.Or },
                        new Where() { CellName="TableName", CellValue=en+"MXKZ", Parallel=ParallelType.Or },
                        new Where() { Type = WhereType.右括号 },
                    }
                };
                long count = SQLiteDao.GetCount(param);
                if (count > 0)
                {
                    Alert("创建三表失败，原因：\r\n主表、从表或扩展表表名已经存在！");
                    HideLoading();
                    return;
                }

                //应收应付表名
                string ysyfTableName = "";
                TableInfo tableYSYF = AppGlobal.GetTableConfig(sbkhglbEN);
                if (tableYSYF.TableSubType == TableSubType.客户表 || tableYSYF.TableSubType == TableSubType.供应商表)
                {
                    ysyfTableName = sbkhglbEN;
                }

                //应收
                bool kdAddYS = false;
                bool kdReduceYS = false;
                bool shAddYS = false;
                bool shReduceYS = false;

                //应付
                bool kdAddYF = false;
                bool kdReduceYF = false;
                bool shAddYF = false;
                bool shReduceYF = false;

                //库存
                bool kdAddKC = false;
                bool kdReduceKC = false;
                bool shAddKC = false;
                bool shReduceKC = false;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    //应收
                    kdAddYS = this.cbSBKDAddYS.IsChecked.Value;
                    kdReduceYS = this.cbSBKDReduceYS.IsChecked.Value;
                    shAddYS = this.cbSBSHAddYS.IsChecked.Value;
                    shReduceYS = this.cbSBSHReduceYS.IsChecked.Value;

                    //应付
                    kdAddYF = this.cbSBKDAddYF.IsChecked.Value;
                    kdReduceYF = this.cbSBKDReduceYF.IsChecked.Value;
                    shAddYF = this.cbSBSHAddYF.IsChecked.Value;
                    shReduceYF = this.cbSBSHReduceYF.IsChecked.Value;

                    //库存
                    kdAddKC = this.cbSBKDAddKC.IsChecked.Value;
                    kdReduceKC = this.cbSBKDReduceKC.IsChecked.Value;
                    shAddKC = this.cbSBSHAddKC.IsChecked.Value;
                    shReduceKC = this.cbSBSHReduceKC.IsChecked.Value;
                }));

                //======================================================
                //创建主表
                SQLParam insertParams = new SQLParam()
                {

                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", 0),
                        new KeyValue("ParentTableName", ""),
                        new KeyValue("Type", "双表"),
                        new KeyValue("SubType", "普通表"),
                        new KeyValue("CnName", cn),
                        new KeyValue("TableName", en),
                        new KeyValue("IsSystem", false),
                        new KeyValue("IsBuild", false),
                        new KeyValue("IsListEdit", true),
                        new KeyValue("IsAudit", false),
                        new KeyValue("IsFilterKH", false),
                        new KeyValue("IsFilterKHById", false),
                        new KeyValue("ZDCX", true),
                        new KeyValue("SearchKeywords", cn + en),
                        new KeyValue("MainTableId", 0),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("SubTableId", 0),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("YSYFTableName", ysyfTableName),
                        new KeyValue("SPStockTableName", ""),
                        new KeyValue("WCSLTableName", ""),

                        new KeyValue("KDAddYSK", kdAddYS),
                        new KeyValue("KDReduceYSK", kdReduceYS),
                        new KeyValue("SHAddYSK", shAddYS),
                        new KeyValue("SHReduceYSK", shReduceYS),

                        new KeyValue("KDAddYFK", kdAddYF),
                        new KeyValue("KDReduceYFK", kdReduceYF),
                        new KeyValue("SHAddYFK", shAddYF),
                        new KeyValue("SHReduceYFK", shReduceYF),

                        new KeyValue("YLTP", true),
                        new KeyValue("DYTP", true),
                        new KeyValue("DBYYSL", false),
                        new KeyValue("DBYYJE", false),
                    }
                };
                long zbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                if (zbid <= 0)
                {
                    Alert("创建主表失败，原因：\r\n" + msg);
                    return;
                }

                try
                {
                    //设置主表客户列关联表
                    string sql = "update [Sys_TableCells] set [ForeignTableId]=" + sbkhglbID + ",[ForeignTableName]='" + sbkhglbEN + "',[IsAddPopTable]=1 where [ParentId]=" + zbid + " and ([CellName]='KHID' or [CellName]='KHMC' or [CellName]='KHBH')";
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch { }

                //======================================================
                //创建从表
                insertParams = new SQLParam()
                {

                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", zbid),
                        new KeyValue("ParentTableName", en),
                        new KeyValue("Type", "双表"),
                        new KeyValue("SubType", "普通表"),
                        new KeyValue("CnName", cn+"明细"),
                        new KeyValue("TableName", en+"MX"),
                        new KeyValue("IsSystem", false),
                        new KeyValue("IsBuild", false),
                        new KeyValue("IsListEdit", true),
                        new KeyValue("IsAudit", false),
                        new KeyValue("IsFilterKH", false),
                        new KeyValue("IsFilterKHById", false),
                        new KeyValue("ZDCX", true),
                        new KeyValue("SearchKeywords", cn+ "明细" + en + "MX"),
                        new KeyValue("MainTableId", zbid),
                        new KeyValue("MainTableName", en),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("SubTableId", 0),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("YSYFTableName", ""),
                        new KeyValue("SPStockTableName", sbmxkcbEN),
                        new KeyValue("WCSLTableName", ""),

                        new KeyValue("KDAddIntoCount", kdAddKC),//加入库数量
                        new KeyValue("KDAddStockCount", kdAddKC),
                        new KeyValue("KDAddAvailableCount", kdAddKC),
                        new KeyValue("KDAddOutCount", kdReduceKC), //加出库数量
                        new KeyValue("KDReduceStockCount", kdReduceKC),
                        new KeyValue("KDReduceAvailableCount", kdReduceKC),

                        new KeyValue("SHAddIntoCount", shAddKC),//加入库数量
                        new KeyValue("SHAddStockCount", shAddKC),
                        new KeyValue("SHAddAvailableCount", shAddKC),
                        new KeyValue("SHAddOutCount", shReduceKC), //加出库数量
                        new KeyValue("SHReduceStockCount", shReduceKC),
                        new KeyValue("SHReduceAvailableCount", shReduceKC),

                        new KeyValue("YLTP", true),
                        new KeyValue("DYTP", true),
                        new KeyValue("DBYYSL", false),
                        new KeyValue("DBYYJE", false),
                    }
                };
                long mxid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                if (mxid <= 0)
                {
                    Alert("创建从表失败，原因：\r\n" + msg);
                    return;
                }

                //修改主表的子表信息
                DataService.Update(AppGlobal.UserInfo, _tableInfo, new SQLParam()
                {
                    Id = zbid,

                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("SubTableId", mxid),
                        new KeyValue("SubTableName", en+"MX"),
                    }
                }, ref msg);

                try
                {
                    //设置从表商品列关联表
                    string sql = "update [Sys_TableCells] set [ForeignTableId]=" + sbmxspglbID + ",[ForeignTableName]='" + sbmxspglbEN + "',[IsAddPopTable]=1 where [ParentId]=" + mxid + " and ([CellName]='SPID' or [CellName]='SPMC' or [CellName]='SPBH')";

                    //是否引用视图
                    if (sbmxspglbEN.StartsWith("C_YY"))
                    {
                        if (sbmxspglbEN.EndsWith("KZ"))
                        {
                            //引用表为三表扩展视图
                            sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(ifnull([ReturnCellName],''),'MZMX_','MZKZ_') where [ParentId]=" + mxid;
                            sql += "; update [Sys_TableCells] set [ReturnCellName]='MZKZBM' where [CellName]='SYSYYBM' and [ParentId]=" + mxid;
                        }
                    }
                    else
                    {
                        //是非引用视图时 去除返回列的前缀
                        sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(replace(ifnull([ReturnCellName],''),'MZMX_',''),'MZZB_','') where [ParentId]=" + mxid;
                    }

                    SQLiteDao.ExecuteSQL(sql);
                }
                catch { }

                //======================================================
                //创建扩展表
                insertParams = new SQLParam()
                {
                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", mxid),
                        new KeyValue("ParentTableName", en + "MX"),
                        new KeyValue("Type", "三表"),
                        new KeyValue("SubType", "普通表"),
                        new KeyValue("CnName", cn+"明细扩展"),
                        new KeyValue("TableName", en+"MXKZ"),
                        new KeyValue("IsSystem", false),
                        new KeyValue("IsBuild", false),
                        new KeyValue("IsListEdit", true),
                        new KeyValue("IsAudit", false),
                        new KeyValue("IsFilterKH", false),
                        new KeyValue("IsFilterKHById", false),
                        new KeyValue("ZDCX", true),
                        new KeyValue("SearchKeywords", cn+ "明细扩展" + en + "MXKZ"),
                        new KeyValue("MainTableId", mxid),
                        new KeyValue("MainTableName", en + "MX"),
                        new KeyValue("MainTableName", ""),
                        new KeyValue("SubTableId", 0),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("SubTableName", ""),
                        new KeyValue("YSYFTableName", ""),
                        new KeyValue("SPStockTableName", ""),
                        new KeyValue("WCSLTableName", ""),
                        new KeyValue("YLTP", true),
                        new KeyValue("DYTP", true),
                        new KeyValue("DBYYSL", false),
                        new KeyValue("DBYYJE", false),
                    }
                };
                long sbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                if (sbid <= 0)
                {
                    Alert("创建扩展表失败，原因：\r\n" + msg);
                    return;
                }

                //修改从表的子表信息
                DataService.Update(AppGlobal.UserInfo, _tableInfo, new SQLParam()
                {
                    Id = zbid,
                    TableName = _tableInfo.TableName,
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("SubTableId", sbid),
                        new KeyValue("SubTableName", en+"MXKZ"),
                    }
                }, ref msg);

                try
                {
                    //设置扩展表商品列关联表
                    string sql = "update [Sys_TableCells] set [ForeignTableId]=" + sbkzspglbID + ",[ForeignTableName]='" + sbkzspglbEN + "',[IsAddPopTable]=1 where [ParentId]=" + sbid + " and ([CellName]='SPID' or [CellName]='SPMC' or [CellName]='SPBH')";

                    //是否引用视图
                    if (sbkzspglbEN.StartsWith("C_YY"))
                    {
                        if (sbkzspglbEN.EndsWith("KZ"))
                        {
                            //引用表为三表扩展视图
                            sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(ifnull([ReturnCellName],''),'MZMX_','MZKZ_') where [ParentId]=" + sbid;
                            sql += "; update [Sys_TableCells] set [ReturnCellName]='MZKZBM' where [CellName]='SYSYYBM' and [ParentId]=" + sbid;
                        }
                    }
                    else
                    {
                        //是非引用视图时 去除返回列的前缀
                        sql += "; update [Sys_TableCells] set [ReturnCellName]=replace(replace(ifnull([ReturnCellName],''),'MZMX_',''),'MZZB_','') where [ParentId]=" + sbid;
                    }

                    SQLiteDao.ExecuteSQL(sql);
                }
                catch { }

                //======================================================
                //是否生成明细报表
                if (scZCBB)
                {
                    string zcbm = en + "MXBB";

                    insertParams = new SQLParam()
                    {

                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", zbid),
                            new KeyValue("ParentTableName", en),
                            new KeyValue("Type", "视图"),
                            new KeyValue("SubType", "主从视图"),
                            new KeyValue("CnName", cn+"明细报表"),
                            new KeyValue("TableName", zcbm),
                            new KeyValue("IsSystem", false),
                            new KeyValue("IsBuild", false),
                            new KeyValue("IsListEdit", true),
                            new KeyValue("IsAudit", false),
                            new KeyValue("IsFilterKH", false),
                            new KeyValue("IsFilterKHById", false),
                            new KeyValue("ZDCX", true),
                            new KeyValue("SearchKeywords", cn+ "明细报表" + zcbm),
                            new KeyValue("MainTableId", zbid),
                            new KeyValue("MainTableName", en),
                            new KeyValue("MainTableName", ""),
                            new KeyValue("SubTableId", mxid),
                            new KeyValue("SubTableName", en+"MX"),
                            new KeyValue("SubTableName", ""),
                            new KeyValue("YSYFTableName", ""),
                            new KeyValue("SPStockTableName", ""),
                            new KeyValue("WCSLTableName", ""),
                            new KeyValue("YLTP", true),
                            new KeyValue("DYTP", true),
                            new KeyValue("DBYYSL", false),
                            new KeyValue("DBYYJE", false),
                        }
                    };

                    //创建明细报表
                    long zcbbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                    if (zcbbid <= 0)
                    {
                        Alert("创建明细报表失败，原因：\r\n" + msg);
                        return;
                    }
                }

                //======================================================
                //是否生成引用报表
                if (scYYBB)
                {
                    string yybm = en.Replace("C_", "C_YY");

                    insertParams = new SQLParam()
                    {

                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", zbid),
                            new KeyValue("ParentTableName", en),
                            new KeyValue("Type", "视图"),
                            new KeyValue("SubType", "引用视图"),
                            new KeyValue("CnName", "引用" + cn),
                            new KeyValue("TableName", yybm),
                            new KeyValue("IsSystem", false),
                            new KeyValue("IsBuild", false),
                            new KeyValue("IsListEdit", true),
                            new KeyValue("IsAudit", false),
                            new KeyValue("IsFilterKH", true),
                            new KeyValue("IsFilterKHById", true),
                            new KeyValue("YYHYC", true),
                            new KeyValue("YYHGB", true),
                            new KeyValue("ZDCX", true),
                            new KeyValue("SearchKeywords", "引用" + cn + yybm),
                            new KeyValue("MainTableId", zbid),
                            new KeyValue("MainTableName", en),
                            new KeyValue("MainTableName", ""),
                            new KeyValue("SubTableId", mxid),
                            new KeyValue("SubTableName", en+"MX"),
                            new KeyValue("SubTableName", ""),
                            new KeyValue("YSYFTableName", ""),
                            new KeyValue("SPStockTableName", ""),
                            new KeyValue("WCSLTableName", ""),
                            new KeyValue("YLTP", true),
                            new KeyValue("DYTP", true),
                            new KeyValue("DBYYSL", false),
                            new KeyValue("DBYYJE", false),
                        }
                    };

                    //创建引用表
                    long yybbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                    if (yybbid <= 0)
                    {
                        Alert("创建引用报表失败，原因：\r\n" + msg);
                        return;
                    }

                    //添加到引用模块
                    InsertToYYModule(yybbid, yybm, "引用" + cn);
                }

                //生成引用明细扩展报表
                if (scYYBB)
                {
                    string yykzbm = en.Replace("C_", "C_YY") + "KZ";

                    insertParams = new SQLParam()
                    {

                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", zbid),
                            new KeyValue("ParentTableName", en),
                            new KeyValue("Type", "视图"),
                            new KeyValue("SubType", "引用扩展视图"),
                            new KeyValue("CnName", "引用" + cn + "扩展"),
                            new KeyValue("TableName", yykzbm),
                            new KeyValue("IsSystem", false),
                            new KeyValue("IsBuild", false),
                            new KeyValue("IsListEdit", true),
                            new KeyValue("IsAudit", false),
                            new KeyValue("IsFilterKH", true),
                            new KeyValue("IsFilterKHById", true),
                            new KeyValue("YYHYC", true),
                            new KeyValue("YYHGB", true),
                            new KeyValue("ZDCX", true),
                            new KeyValue("SearchKeywords", "引用" + cn + yykzbm),
                            new KeyValue("MainTableId", zbid),
                            new KeyValue("MainTableName", en),
                            new KeyValue("MainTableName", ""),
                            new KeyValue("SubTableId", mxid),
                            new KeyValue("SubTableName", en+"MX"),
                            new KeyValue("SubTableName", ""),
                            new KeyValue("YSYFTableName", ""),
                            new KeyValue("SPStockTableName", ""),
                            new KeyValue("WCSLTableName", ""),
                            new KeyValue("YLTP", true),
                            new KeyValue("DYTP", true),
                            new KeyValue("DBYYSL", false),
                            new KeyValue("DBYYJE", false),
                        }
                    };

                    //创建引用扩展表
                    long yykzbbid = DataService.Insert(AppGlobal.UserInfo, _tableInfo, insertParams, ref msg);
                    if (yykzbbid <= 0)
                    {
                        Alert("创建引用扩展报表失败，原因：\r\n" + msg);
                        return;
                    }

                    //添加到引用模块
                    InsertToYYModule(yykzbbid, yykzbm, "引用" + cn + "扩展");
                }

                //======================================================
                //设置表索引
                try
                {
                    string sql = "update [Sys_Tables] set [DataIndex]=[Id] where [DataIndex]=1 or [DataIndex]=0 or [DataIndex] is null";
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch (Exception ex) { }

                try
                {
                    //设置表列为系统列
                    string sql = "update [Sys_TableCells] set [IsSystem]=1 where [ParentId]=" + zbid + " or [ParentId]=" + mxid + " or [ParentId]=" + sbid;
                    SQLiteDao.ExecuteSQL(sql);
                }
                catch (Exception ex) { }

                //======================================================
                //是否自动审核
                if (autoAudit)
                {
                    try
                    {
                        //调用审核主表
                        DataService.Audit(AppGlobal.UserInfo, _tableInfo, zbid, ref msg);

                        //调用审核从表
                        DataService.Audit(AppGlobal.UserInfo, _tableInfo, mxid, ref msg);

                        //调用审核三表
                        DataService.Audit(AppGlobal.UserInfo, _tableInfo, sbid, ref msg);
                    }
                    catch (Exception ex) { }
                }

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //需要刷新表配置
                    AppData.MainWindow.ShowRenewTC = true;

                    //生成成功
                    AppAlert.FormTips(gridMain, "生成三表成功！", AppCode.Enums.FormTipsType.Right);

                    this.txtCnName.Text = "";
                    this.txtEnName.Text = "";

                    this.ddlSBZDKHGL.SearchText = "";
                    this.ddlSBZDKHGLBM.Text = "";
                    this.ddlSBMXSPGL.SearchText = "";
                    this.ddlSBMXSPGLBM.Text = "";
                    this.ddlSBKZSPGL.SearchText = "";
                    this.ddlSBKZSPGLBM.Text = "";
                    this.ddlSBMXKCB.SearchText = "";
                    this.ddlSBMXKCBBM.Text = "";

                    sbkhglbID = 0;
                    sbkhglbCN = "";
                    sbkhglbEN = "";

                    sbmxspglbID = 0;
                    sbmxspglbCN = "";
                    sbmxspglbEN = "";

                    sbkzspglbID = 0;
                    sbkzspglbCN = "";
                    sbkzspglbEN = "";

                    sbmxkcbID = 0;
                    sbmxkcbCN = "";
                    sbmxkcbEN = "";
                    return null;
                }));
            }
            catch (Exception ex)
            {
                Alert("生成三表失败，原因：" + ex.Message);
            }
            finally
            {
                HideLoading();
            }
        }
        #endregion

        #region 辅助
        /// <summary>
        /// 添加到引用模块
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="tableName"></param>
        /// <param name="tableNameCN"></param>
        private void InsertToYYModule(long tableId, string tableName, string tableNameCN)
        {
            try
            {
                SQLParam insertParams = new SQLParam()
                {
                    TableName = "Sys_ModuleDetails",
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("ParentId", 99),
                        new KeyValue("TableId", tableId),
                        new KeyValue("TableName", tableName),
                        new KeyValue("ModuleName", tableNameCN),
                        new KeyValue("ModuleCode", ""),
                        new KeyValue("Url", ""),
                        new KeyValue("Icon", ":PackIconMaterial:AccessPoint:#000000"),
                        new KeyValue("CreateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                        new KeyValue("SearchKeywords", tableName+tableNameCN),
                        new KeyValue("Order", 999),
                        new KeyValue("IsShow", true),
                        new KeyValue("IsPC", true),
                        new KeyValue("IsWeb", true),
                        new KeyValue("IsApp", true),
                        new KeyValue("IsWeiXin", true),
                    }
                };
                SQLiteDao.Insert(insertParams);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 主从选择表输入事件
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="txt"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        private void DDLSelectTable_Txt_Change_Event(Controls.DropDownTable uc, string txt, int pageIndex, int pageSize)
        {
            var result = QueryService.GetPaging(new SQLParam()
            {
                TableName = "Sys_Tables",
                PageSize = pageSize,
                PageIndex = pageIndex,
                Wheres = new List<Where>()
                {
                    new Where("SearchKeywords", txt, WhereType.模糊查询)
                }
            });

            //显示数据
            uc._pageSize = pageSize;
            uc.ShowData(result.Data, result.TotalCount);
        }
        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="msg"></param>
        private void Alert(string msg)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                AppAlert.Alert(msg);
                return null;
            }));
        }
        #endregion

        #region 事件
        /// <summary>
        /// 生成表结束后事件
        /// </summary>
        private void BuidedTable_Event()
        {
            try
            {
                string sql = "update [Sys_Tables] set [DataIndex]=[Id]";
                SQLiteDao.ExecuteSQL(sql);
            }
            catch { }
        }
        #endregion
    }
}

