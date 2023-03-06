
using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.AppCode.Models;
using Wsfly.ERP.Std.AppCode.Exts;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Handler;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ImportDataUC.xaml 的交互逻辑
    /// </summary>
    public partial class ImportDataUC : BaseUserControl
    {
        /// <summary>
        /// 主表配置
        /// </summary>
        public TableInfo _topTableInfo = null;
        /// <summary>
        /// 表配置
        /// </summary>
        public TableInfo _tableInfo = null;
        /// <summary>
        /// 表名称
        /// </summary>
        public string _fileName = null;
        /// <summary>
        /// 父级编号
        /// </summary>
        public long _parentId = 0;
        /// <summary>
        /// 要导入的数据
        /// </summary>
        public DataTable _dtData = null;

        /// <summary>
        /// 是否已经导入
        /// </summary>
        public bool _isImported = false;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name=""></param>
        public ImportDataUC(TableInfo tableInfo, string excelPath, long parentId = 0)
        {
            _topTableInfo = tableInfo;
            _tableInfo = parentId <= 0 ? tableInfo : tableInfo.SubTable;
            _fileName = excelPath;
            _parentId = parentId;

            InitializeComponent();

            this.Loaded += ImportDataUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportDataUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnImport.IsEnabled = false;

            this.btnChooseFile.Click += BtnChooseFile_Click;
            this.btnReloadData.Click += BtnReloadData_Click;

            this.btnImport.Click += BtnImport_Click;
            this.btnCancel.Click += BtnCancel_Click;
            this.btnSuccess.Click += BtnSuccess_Click;

            //表名
            this.lblTableName.Text = _tableInfo.TableName + "(" + _tableInfo.CnName + ")";
            this.lblTableName.ToolTip = _tableInfo.TableName + "(" + _tableInfo.CnName + ")";
            this.lblFileName.Text = _fileName;
            this.lblFileName.ToolTip = _fileName;

            this.dataGrid.LoadingRow += DataGrid_LoadingRow;

            //加载数据
            if (!string.IsNullOrWhiteSpace(_fileName)) LoadExcelData();

            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSuccess_Click(object sender, RoutedEventArgs e)
        {
            if (!_isImported)
            {
                bool? flagExit = AppAlert.Alert("还未导入数据，是否确定退出？", "温馨提示", AppCode.Enums.AlertWindowButton.OkCancel);
                if (flagExit.HasValue && !flagExit.Value) return;
            }

            //列表页面
            Home.ListUC ucList = this._ParentUC as Home.ListUC;

            //刷新数据
            if (_parentId <= 0) { ucList.LoadingData(); }
            else { ucList.LoadingDetails(_parentId); }

            _ParentWindow.Close();
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始控件大小
        /// </summary>
        private void InitSize()
        {
            this.dataGrid.Width = _ParentWindow.Width - 30;
            this.dataGrid.Height = _ParentWindow.Height - 100;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._ParentWindow.Close();
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading(gridMain);

            Dictionary<string, CellInfo> dicSaveConfigs = new Dictionary<string, CellInfo>();

            //遍历所有表列
            foreach (DataGridColumn col in this.dataGrid.Columns)
            {
                StackPanel panel = col.Header as StackPanel;
                ComboBox cmb = this.GetChildObject<ComboBox>(panel);
                if (cmb == null) continue;

                string orgCellName = cmb.Tag.ToString();
                CellInfo cellInfo = (cmb.SelectedItem as CellInfo);

                if (cellInfo == null) continue;

                dicSaveConfigs.Add(orgCellName, cellInfo);
            }

            //没有要导入的数据
            if (dicSaveConfigs == null || dicSaveConfigs.Count <= 0)
            {
                HideLoading();
                AppAlert.FormTips(gridMain, "请选择要导入的列！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                bool hasIsShow = _tableInfo.Cells.Exists(p => p.CellName.Equals("IsShow"));
                bool hasIsLock = _tableInfo.Cells.Exists(p => p.CellName.Equals("IsLock"));
                bool hasCreateUserId = _tableInfo.Cells.Exists(p => p.CellName.Equals("CreateUserId"));
                bool hasCreateUserName = _tableInfo.Cells.Exists(p => p.CellName.Equals("CreateUserName"));
                bool hasCreateDate = _tableInfo.Cells.Exists(p => p.CellName.Equals("CreateDate"));
                bool hasAudit = _tableInfo.Cells.Exists(p => p.CellName.Equals("IsAudit"));
                bool hasSearchKeywords = _tableInfo.Cells.Exists(p => p.CellName.Equals("SearchKeywords"));
                bool hasBarcode = _tableInfo.Cells.Exists(p => p.IsBarcodeEAN13 || p.IsBarcodeEAN8);
                bool hasDataIndex = _tableInfo.Cells.Exists(p => p.CellName.Equals("DataIndex"));
                string keywords = "";

                //库存表
                DataTable dtKCBM = null;
                try
                {
                    if (_tableInfo.TableSubType == TableSubType.商品表)
                    {
                        //增加库存记录
                        SQLParam queryKCBM = new SQLParam()
                        {
                            TableName = AppGlobal.SysTableName_Tables,
                            Wheres = new List<Where>()
                            {
                                new Where("ParentId", _tableInfo.Id),
                                new Where("SubType", "库存表"),
                            }
                        };

                        //所有关联此商品表的库存表
                        dtKCBM = SQLiteDao.GetTable(queryKCBM);
                    }
                }
                catch (Exception ex) { }

                //明细是否有商品ID列
                CellInfo cellSPID = null;
                bool postSPIDKV = false;
                if (_parentId > 0 && _dtData.Columns.Contains("SPMC") && _dtData.Columns.Contains("SPBH"))
                {
                    cellSPID = _tableInfo.Cells.Find(p => p.CellName.Equals("SPID"));
                    postSPIDKV = true;
                }

                int rowIndex = 0;
                int failCount = 0;
                StringBuilder sbCheckMSG = new StringBuilder();

                foreach (DataRow row in _dtData.Rows)
                {
                    rowIndex++;
                    keywords = "";

                    SQLParam param = new SQLParam()
                    {

                        TableName = _tableInfo.TableName,
                        OpreateCells = new List<KeyValue>()
                    };

                    if (_parentId > 0)
                    {
                        //父级路径
                        param.OpreateCells.Add(new KeyValue("ParentId", _parentId));

                        //导入的是子表
                        //是否需要查询商品ID
                        if (postSPIDKV)
                        {
                            //增加商品ID列
                            SQLParam querySPID = new SQLParam()
                            {
                                TableName = cellSPID.ForeignTableName,
                                Wheres = new List<Where>()
                                {
                                    new Where("SPMC", row["SPMC"]),
                                    new Where("SPBH", row["SPBH"])
                                }
                            };
                            DataRow rowSPID = SQLiteDao.GetTableRow(querySPID);
                            if (rowSPID == null)
                            {
                                failCount++;
                                //sbCheckMSG.Append("第" + rowIndex + "行，未能查询到商品ID！");
                                sbCheckMSG.AppendLine("第" + rowIndex + "行，【" + row["SPMC"] + "-" + row["SPBH"] + "】未能查询到商品ID！");

                                this.Dispatcher.Invoke(new FlushClientBaseDelegate_Void(delegate ()
                                {
                                    try
                                    {
                                        this.dataGrid.GetRow(rowIndex - 1).Background = Brushes.Red;
                                    }
                                    catch { }
                                }));

                                continue;
                            }
                            long spid = DataType.Long(rowSPID["Id"], 0);
                            param.OpreateCells.Add(new KeyValue("SPID", spid));
                        }
                    }

                    bool addRow = true;
                    bool hasKeywordsCell = _tableInfo.Cells.Count(p => p.CellName.Equals("SearchKeywords")) > 0;

                    foreach (KeyValuePair<string, CellInfo> kv in dicSaveConfigs)
                    {
                        //列名
                        string orgCellName = kv.Key;
                        CellInfo cellInfo = kv.Value;

                        //没有对应列
                        if (cellInfo == null) continue;
                        //不要导入的SPID
                        if (postSPIDKV && cellInfo.CellName.Equals("SPID")) continue;

                        //值
                        object value = row[orgCellName];

                        //空值时默认值
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            //Bool值类型
                            if (cellInfo.ValType == "bool") value = false;
                            //数字类型
                            if (cellInfo.IsNumberType) value = 0;
                            //日期类型
                            if (cellInfo.IsDateType) value = DateTime.Now;
                        }
                        else
                        {
                            //Bool值类型
                            if (cellInfo.ValType == "bool")
                            {
                                if (value.ToString() == "√" || value.ToString() == "1" || value.ToString() == "是" || value.ToString().ToLower() == "true")
                                {
                                    value = true;
                                }
                                else
                                {
                                    value = false;
                                }
                            }
                        }

                        //检查数据是否附合格式
                        if (!cellInfo.AllownNull)
                        {
                            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                failCount++;
                                sbCheckMSG.Append("第" + rowIndex + "行，【" + row["SPMC"] + "-" + row["SPBH"] + "】列(" + orgCellName + ")不允许为空！");
                                addRow = false;
                                break;
                            }
                        }

                        if (!cellInfo.AllownRepeat)
                        {
                            //不允许重复
                            if (IsRepeat(cellInfo.CellName, value))
                            {
                                failCount++;
                                sbCheckMSG.Append("第" + rowIndex + "行，【" + row["SPMC"] + "-" + row["SPBH"] + "】列(" + orgCellName + ")不允许重复！");
                                addRow = false;
                                break;
                            }
                        }

                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            try
                            {
                                //需要的类型
                                Type type = Core.AppHandler.GetTypeByString(cellInfo.ValType);
                                value = Convert.ChangeType(value, type);
                            }
                            catch
                            {
                                //数据类型不正确
                                failCount++;
                                sbCheckMSG.Append("第" + rowIndex + "行，【" + row["SPMC"] + "-" + row["SPBH"] + "】列(" + orgCellName + ")数据类型不正确！");
                                addRow = false;
                                break;
                            }
                        }
                        else if (cellInfo.IsNumberType)
                        {
                            value = 0;
                        }

                        //判断列是否以配置后缀结尾
                        bool isBuildPYKeywords = AppGlobal.BuildSearchKeywordsSuffix.Contains(cellInfo.CellName.ToUpper());
                        if (hasKeywordsCell && cellInfo.ValType.Equals("string") && !string.IsNullOrWhiteSpace(value.ToString()) && isBuildPYKeywords)
                        {
                            //关键字
                            keywords += ChineseHandler.GetFirstPYLetters(value.ToString()) + value.ToString();
                        }

                        //添加要插入的一列
                        param.OpreateCells.Add(new KeyValue(cellInfo.CellName, value));
                    }

                    //不添加行
                    if (!addRow) continue;

                    //有创建人
                    if (hasCreateUserId && !param.OpreateCells.Exists(p => p.Key.Equals("CreateUserId"))) param.OpreateCells.Add(new KeyValue("CreateUserId", AppGlobal.UserInfo.UserId));
                    //有创建人姓名
                    if (hasCreateUserName && !param.OpreateCells.Exists(p => p.Key.Equals("CreateUserName"))) param.OpreateCells.Add(new KeyValue("CreateUserName", AppGlobal.UserInfo.UserName));
                    //有创建日期
                    if (hasCreateDate && !param.OpreateCells.Exists(p => p.Key.Equals("CreateDate"))) param.OpreateCells.Add(new KeyValue("CreateDate", DateTime.Now));
                    //是否有显示
                    if (hasIsShow && !param.OpreateCells.Exists(p => p.Key.Equals("IsShow"))) param.OpreateCells.Add(new KeyValue("IsShow", true));
                    //是否有锁定
                    if (hasIsLock && !param.OpreateCells.Exists(p => p.Key.Equals("IsLock"))) param.OpreateCells.Add(new KeyValue("IsLock", false));
                    //是否有关键字
                    if (hasSearchKeywords && !string.IsNullOrWhiteSpace(keywords)) param.OpreateCells.Add(new KeyValue("SearchKeywords", keywords));
                    //有审核
                    if (hasAudit)
                    {
                        if (param.OpreateCells.Count(p => p.Key.Equals("IsAudit")) <= 0) param.OpreateCells.Add(new KeyValue("IsAudit", false));
                        else param.OpreateCells.Find(p => p.Key.Equals("IsAudit")).Value = false;
                        if (param.OpreateCells.Count(p => p.Key.Equals("AuditUserId")) <= 0) param.OpreateCells.Add(new KeyValue("AuditUserId", 0));
                        if (param.OpreateCells.Count(p => p.Key.Equals("AuditUserName")) <= 0) param.OpreateCells.Add(new KeyValue("AuditUserName", ""));
                        if (param.OpreateCells.Count(p => p.Key.Equals("AuditDate")) <= 0) param.OpreateCells.Add(new KeyValue("AuditDate", DateTime.Now));
                    }

                    if (hasBarcode)
                    {
                        List<CellInfo> cellBarcodeEAN13List = _tableInfo.Cells.Where(p => p.IsBarcodeEAN13).ToList();
                        List<CellInfo> cellBarcodeEAN8List = _tableInfo.Cells.Where(p => p.IsBarcodeEAN8).ToList();

                        if (cellBarcodeEAN13List != null && cellBarcodeEAN13List.Count > 0)
                        {
                            foreach (CellInfo cell in cellBarcodeEAN13List)
                            {
                                try
                                {
                                    KeyValue itemTM = param.OpreateCells.Find(p => p.Key.Equals(cell.CellName));
                                    if (itemTM == null || itemTM.Value == null || string.IsNullOrWhiteSpace(itemTM.Value.ToString()))
                                    {
                                        //13位条码
                                        string barcode = "69" + SQLiteDao.GetSerialNo(_tableInfo.Id, 0, SQLiteDao.SerialNoType.流水号, 10);
                                        barcode = JSTMJYW(barcode);

                                        //添加或修改条码值
                                        if (itemTM == null) param.OpreateCells.Add(new KeyValue(cell.CellName, barcode));
                                        else itemTM.Value = barcode;
                                    }
                                }
                                catch (Exception ex) { }
                            }
                        }

                        if (cellBarcodeEAN8List != null && cellBarcodeEAN8List.Count > 0)
                        {
                            foreach (CellInfo cell in cellBarcodeEAN8List)
                            {
                                try
                                {
                                    KeyValue itemTM = param.OpreateCells.Find(p => p.Key.Equals(cell.CellName));
                                    if (itemTM == null || itemTM.Value == null || string.IsNullOrWhiteSpace(itemTM.Value.ToString()))
                                    {
                                        //8位条码
                                        string barcode = "69" + SQLiteDao.GetSerialNo(_tableInfo.Id, 0, SQLiteDao.SerialNoType.流水号, 5);
                                        barcode = JSTMJYW(barcode);

                                        //添加或修改条码值
                                        if (itemTM == null) param.OpreateCells.Add(new KeyValue(cell.CellName, barcode));
                                        else itemTM.Value = barcode;
                                    }
                                }
                                catch (Exception ex) { }
                            }
                        }
                    }

                    if (_tableInfo.TableSubType == TableSubType.供应商表 || _tableInfo.TableSubType == TableSubType.客户表)
                    {
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("YSK")))
                        {
                            param.OpreateCells.Add(new KeyValue("YSK", 0));
                        }
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("YFK")))
                        {
                            param.OpreateCells.Add(new KeyValue("YFK", 0));
                        }
                    }

                    if (_tableInfo.TableSubType == TableSubType.库存表)
                    {
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("CGSL"))) param.OpreateCells.Add(new KeyValue("CGSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("DDSL"))) param.OpreateCells.Add(new KeyValue("DDSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("RKSL"))) param.OpreateCells.Add(new KeyValue("RKSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("CKSL"))) param.OpreateCells.Add(new KeyValue("CKSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("KCSL"))) param.OpreateCells.Add(new KeyValue("KCSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("KYSL"))) param.OpreateCells.Add(new KeyValue("KYSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("SCSL"))) param.OpreateCells.Add(new KeyValue("SCSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("WLSL"))) param.OpreateCells.Add(new KeyValue("WLSL", 0));
                        if (!param.OpreateCells.Exists(p => p.Key.Equals("XQSL"))) param.OpreateCells.Add(new KeyValue("XQSL", 0));
                    }

                    try
                    {
                        //插入对象
                        long id = SQLiteDao.Insert(param);

                        if (id > 0)
                        {
                            //是否为商品表
                            if (_tableInfo.TableSubType == TableSubType.商品表)
                            {
                                //增加库存记录
                                if (dtKCBM != null && dtKCBM.Rows.Count > 0)
                                {
                                    foreach (DataRow rowKC in dtKCBM.Rows)
                                    {
                                        try
                                        {
                                            string kcbm = rowKC["TableName"].ToString();
                                            string spmc = "";
                                            string spbh = "";

                                            KeyValue kvSPMC = param.OpreateCells.Find(p => p.Key.Equals("SPMC"));
                                            KeyValue kvSPBH = param.OpreateCells.Find(p => p.Key.Equals("SPBH"));

                                            if (kvSPMC != null) spmc = kvSPMC.Value.ToString();
                                            if (kvSPBH != null) spbh = kvSPBH.Value.ToString();

                                            SQLParam paramInserKC = new SQLParam()
                                            {
                                                TableName = kcbm,
                                                OpreateCells = new List<KeyValue>()
                                                {
                                                    new KeyValue("SPID", id),
                                                    new KeyValue("SPMC", spmc),
                                                    new KeyValue("SPBH", spbh),
                                                    new KeyValue("CGSL", 0),
                                                    new KeyValue("DDSL", 0),
                                                    new KeyValue("RKSL", 0),
                                                    new KeyValue("CKSL", 0),
                                                    new KeyValue("KCSL", 0),
                                                    new KeyValue("KYSL", 0),
                                                    new KeyValue("SCSL", 0),
                                                    new KeyValue("WLSL", 0),
                                                    new KeyValue("XQSL", 0),
                                                }
                                            };

                                            SQLiteDao.Insert(paramInserKC);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                //成功数量
                int successCount = _dtData.Rows.Count - failCount;
                
                //是否有数据索引
                if (successCount > 0)
                {
                    try
                    {
                        //更新索引
                        string sqlDataIndex = "update " + _tableInfo.TableName + " set [DataIndex]=[Id] where [DataIndex]=1 or [DataIndex]=0 or [DataIndex] is null";
                        SQLiteDao.ExecuteSQL(sqlDataIndex);
                    }
                    catch (Exception ex) { }

                    try
                    {
                        if (_parentId > 0)
                        {
                            //更新主表总数量、总金额
                            string sqlUpdateZB = "update [" + _topTableInfo.TableName + "] set [ZSL]=(select sum([SL]) from [" + _topTableInfo.SubTable.TableName + "] where [ParentId]=" + _parentId + "),[ZJE]=(select sum([JE]) from [" + _topTableInfo.SubTable.TableName + "] where [ParentId]=" + _parentId + ") where [Id]=" + _parentId;
                            SQLiteDao.ExecuteSQL(sqlUpdateZB);
                        }
                    }
                    catch (Exception ex) { }
                }

                //已经导入
                _isImported = true;

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    try
                    {
                        if (failCount <= 0)
                        {
                            //列表刷新
                            Home.ListUC ucList = this._ParentUC as Home.ListUC;
                            AppAlert.FormTips(ucList.gridMain, "数据导入成功！", AppCode.Enums.FormTipsType.Right);

                            //刷新数据
                            if (_parentId <= 0)
                            {
                                //刷新主表数据
                                ucList.LoadingData();
                            }
                            else
                            {
                                //刷新明细表数据
                                ucList.LoadingDetails(_parentId);
                            }

                            //关闭窗口
                            _ParentWindow.Close();
                            return null;
                        }

                        string tipMsg = "数据导入结束，有" + failCount + "行导入失败！\r\n" + sbCheckMSG.ToString();
                        AppAlert.Alert(tipMsg, "温馨提示", HorizontalAlignment.Left, VerticalAlignment.Top, 400, 300);

                        HideLoading();
                    }
                    catch (Exception ex)
                    {
                    }
                    return null;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 计算条码校验位
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        private string JSTMJYW(string barcode)
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
        /// 是否重复
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsRepeat(string cellName, object value)
        {
            try
            {
                SQLParam param = new SQLParam()
                {

                    TableName = _tableInfo.TableName,
                    Wheres = new List<Where>()
                    {
                        new Where() { CellName = cellName,CellValue=value }
                    }
                };

                //是否有上级编号
                if (_parentId > 0)
                {
                    param.Wheres.Add(new Where() { CellName = "ParentId", CellValue = _parentId });
                }

                //是否有数量
                long count = SQLiteDao.GetCount(param);

                //是否重复
                return count > 0;
            }
            catch { }

            return true;
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            //选择Excel文件
            string path = UploadFileHandler.ChooseFileDialog("Excel文件|*.xls;*.xlsx");
            if (string.IsNullOrWhiteSpace(path)) return;

            //文件路径
            _fileName = path;

            //加载Excel文件
            LoadExcelData();
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadData_Click(object sender, RoutedEventArgs e)
        {
            //加载Excel文件
            LoadExcelData();
        }

        /// <summary>
        /// 加载Excel文件
        /// </summary>
        private void LoadExcelData()
        {
            //文件不存在
            if (!File.Exists(_fileName))
            {
                AppAlert.FormTips(gridMain, "文件不存在！", AppCode.Enums.FormTipsType.Info);
                this.btnImport.IsEnabled = false;
                return;
            }

            try
            {
                //导入数据
                _dtData = AppCode.Handler.NPOIHandler.ExcelImport(_fileName, null, true);
            }
            catch { }

            if (_dtData == null || _dtData.Rows.Count <= 0)
            {
                //没有内容
                AppAlert.FormTips(gridMain, "选择的文件没有数据！", AppCode.Enums.FormTipsType.Info);
                this.dataGrid.ItemsSource = null;
                this.btnImport.IsEnabled = false;
                return;
            }

            //if (_parentId > 0)
            //{
            //    //导入的是明细表
            //    if (!_dtData.Columns.Contains("SPID")) _dtData.Columns.Add(new DataColumn() { ColumnName = "SPID", DataType = typeof(long) });
            //}

            //标记未导入数据
            _isImported = false;

            //样式
            Style style = this.FindResource("DataGridCellCenter") as Style;
            int cellIndex = 2;

            foreach (DataColumn dc in _dtData.Columns)
            {
                string colName = dc.ColumnName;
                if (AppGlobal.ImportDataFilterCells.Contains(colName.ToUpper())) continue;

                Binding binding = new Binding();
                binding.Path = new PropertyPath(colName);

                DataGridTextColumn col = new DataGridTextColumn();
                col.Binding = binding;
                col.ElementStyle = style;

                StackPanel panelHead = new StackPanel();
                TextBlock lblTitle = new TextBlock();
                lblTitle.Text = colName;

                ComboBox cmb = new ComboBox();
                cmb.ItemsSource = _tableInfo.Cells;
                cmb.DisplayMemberPath = "CnName";
                cmb.Tag = colName;

                panelHead.Children.Add(lblTitle);
                panelHead.Children.Add(cmb);

                //对应列
                CellInfo cellInfo = _tableInfo.Cells.Find(p => p.CellName.Equals(colName));
                if (cellInfo != null)
                {
                    cmb.SelectedItem = cellInfo;
                }

                col.Header = panelHead;
                dataGrid.Columns.Add(col);

                cellIndex++;
            }

            this.dataGrid.ItemsSource = null;
            this.dataGrid.ItemsSource = _dtData.DefaultView;

            this.btnImport.IsEnabled = true;
        }
        /// <summary>
        /// 删除一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblDeleteRow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock lbl = sender as TextBlock;
                DataGridRow row = this.GetParentObject<DataGridRow>(lbl);
                if (row == null)
                {
                    AppAlert.FormTips(gridMain, "删除数据失败！", AppCode.Enums.FormTipsType.Info);
                    return;
                }
                DataRowView rowView = row.Item as DataRowView;

                _dtData.Rows.Remove(rowView.Row);

                AppAlert.FormTips(gridMain, "删除数据成功！", AppCode.Enums.FormTipsType.Right);
            }
            catch (Exception ex)
            {
                AppAlert.FormTips(gridMain, "删除数据失败！", AppCode.Enums.FormTipsType.Info);
            }
        }
        /// <summary>
        /// 加载行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
