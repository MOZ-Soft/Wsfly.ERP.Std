using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;

using MahApps.Metro.IconPacks;

using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.AppCode.Models;
using Wsfly.ERP.Std.AppCode.Exts;
using Wsfly.ERP.Std.AppCode.Enums;
using Wsfly.ERP.Std.AppCode.Extensions;
using Wsfly.ERP.Std.Views.Parts;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Handler;
using Wsfly.ERP.Std.Core.Encryption;
using Wsfly.ERP.Std.Core;
using Wsfly.ERP.Std.Service.Models;
using Wsfly.ERP.Std.Service.Exts;
using Wsfly.ERP.Std.Core.Extensions;

namespace Wsfly.ERP.Std.Views.Home
{
    /// <summary>
    /// ListUC.xaml 的交互逻辑
    /// </summary>
    public partial class ListUC : BaseUserControl
    {
        #region user32
        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwExtraInfo"></param>
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(System.Windows.Forms.Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        #endregion

        #region 变量&属性
        /// <summary>
        /// 主表高度比例
        /// </summary>
        double _topBL = 1;
        /// <summary>
        /// 明细表高度比例
        /// </summary>
        double _bottomBL = 0;
        /// <summary>
        /// 表编号
        /// </summary>
        long _tableId = 0;
        /// <summary>
        /// 模块编号
        /// </summary>
        long _moduleId = 0;
        /// <summary>
        /// 模块编号列表
        /// </summary>
        List<long> _moduleIds = new List<long>();
        /// <summary>
        /// 模块名称
        /// </summary>
        string _moduleName = string.Empty;
        /// <summary>
        /// 首次初始界面
        /// </summary>
        bool _firstInit = true;
        /// <summary>
        /// 主表索引
        /// </summary>
        long _topTableIndex = 0;
        /// <summary>
        /// 明细表索引
        /// </summary>
        long _bottomTableIndex = 0;
        /// <summary>
        /// 是否加载操作按钮
        /// </summary>
        bool _isLoadActions = false;
        /// <summary>
        /// 是否加载操作按钮成功
        /// </summary>
        bool _isLoadedActionsSuccess = false;
        /// <summary>
        /// 是否选择主表
        /// </summary>
        bool _isChooseTop = true;
        /// <summary>
        /// 编辑行编号
        /// </summary>
        int _editRowIndex = 0;
        /// <summary>
        /// 是否弹出窗口
        /// </summary>
        bool _isPopWindow = false;
        /// <summary>
        /// 是否选择回传窗口
        /// </summary>
        bool _isChooseCallbackWindow = false;
        /// <summary>
        /// 是否扩展列表窗口
        /// </summary>
        bool _isExtListWindow = false;
        /// <summary>
        /// 是否加载数据
        /// </summary>
        bool _isLoadedTopData = false;
        /// <summary>
        /// 是否加载明细数据
        /// </summary>
        bool _isLoadedBottomData = false;
        /// <summary>
        /// 是否初始视图分页
        /// </summary>
        bool _isInitViewPaging = false;
        /// <summary>
        /// 表配置
        /// _tableConfig 主表
        /// _tableConfig.SubTable 从表
        /// _tableConfig.SubTable.ThreeTable 扩展表
        /// </summary>
        TableInfo _tableConfig = null;
        /// <summary>
        /// 加载数据提示层
        /// </summary>
        Components.FormTipsView _viewLoadDataTips = null;
        /// <summary>
        /// 列表操作
        /// </summary>
        AppCode.Enums.ListActionEnum _listAction = AppCode.Enums.ListActionEnum.Null;
        /// <summary>
        /// 列表操作快捷键
        /// </summary>
        List<ListAction> _listQuicks = new List<ListAction>();

        /// <summary>
        /// 查询的列
        /// </summary>
        List<CellInfo> _queryCells = new List<CellInfo>();
        /// <summary>
        /// 查询条件
        /// </summary>
        List<Where> _queryWheres = new List<Where>();
        /// <summary>
        /// 扩展条件
        /// </summary>
        List<Where> _extWheres = new List<Where>();
        /// <summary>
        /// 更多查询条件
        /// </summary>
        string _moreQueryWheres = "";

        /// <summary>
        /// 主表排序
        /// </summary>
        List<OrderBy> _topOrderBys = new List<OrderBy>();
        string _topOrderBysSql = "";
        bool _topOrderDefault = false;

        /// <summary>
        /// 子表排序
        /// </summary>
        List<OrderBy> _bottomOrderBys = new List<OrderBy>();
        string _bottomOrderBysSql = "";
        bool _bottomOrderDefault = false;

        /// <summary>
        /// 是否查询后焦点定位到数据表
        /// </summary>
        bool _isQueryDataFocusTable = true;

        /// <summary>
        /// 查询条件控件数量
        /// </summary>
        int _queryWhereCtrlCount = 0;

        /// <summary>
        /// 选择主表编号
        /// </summary>
        long _chooseTopId = 0;
        /// <summary>
        /// 三表上级明细编号
        /// </summary>
        long _extListParentId = 0;
        /// <summary>
        /// 三表上级主表是否审核
        /// </summary>
        bool _extListIsAudit = false;
        /// <summary>
        /// 三表上级打印数据
        /// </summary>
        DataSet _extListPrintDataSet = null;
        /// <summary>
        /// 三表上级主表表ID
        /// </summary>
        long _extListTopTableId = 0;

        /// <summary>
        /// 主表选择单元格值
        /// </summary>
        string _topSelectCellText = string.Empty;
        /// <summary>
        /// 子表选择单元格值
        /// </summary>
        string _bottomSelectCellText = string.Empty;

        /// <summary>
        /// 主表分页尺码
        /// </summary>
        int TopPageSize
        {
            get
            {
                if (_tableConfig.PageSize > 0) return _tableConfig.PageSize;
                return _PageSize;
            }
        }
        /// <summary>
        /// 从表分页尺码
        /// </summary>
        int BottomPageSize
        {
            get
            {
                if (_tableConfig.SubTable.PageSize > 0) return _tableConfig.SubTable.PageSize;
                return _PageSize;
            }
        }

        /// <summary>
        /// 默认打印模版
        /// </summary>
        DataRow _defaultPrintTemplate = null;

        /// <summary>
        /// 主表分页结果
        /// </summary>
        PageResult _topPageResult = null;
        /// <summary>
        /// 明细表分页结果
        /// </summary>
        PageResult _bottomPageResult = null;

        /// <summary>
        /// 视图所有列名
        /// </summary>
        List<string> _viewCellNames = new List<string>();

        /// <summary>
        /// 加载表配置是否失败
        /// </summary>
        bool _loadingTableConfigError = false;
        /// <summary>
        /// 列的顺序或宽度变更
        /// </summary>
        bool _columnSizeOrOrderChange = false;

        /// <summary>
        /// 传递过来的参数
        /// </summary>
        public PostParamInfo _PostParams { get; set; }
        /// <summary>
        /// 存储过程的参数表
        /// </summary>
        DataTable _ProcParams { get; set; }
        /// <summary>
        /// 查询的参数
        /// </summary>
        Dictionary<string, object> _QueryParams { get; set; }
        /// <summary>
        /// 如果是第一次加载存储过程参数
        /// </summary>
        bool _IsFirstLoadProcParams = true;
        /// <summary>
        /// 已返回的行
        /// </summary>
        List<DataRow> _returnRows = new List<DataRow>();
        /// <summary>
        /// 锁定加载关联表
        /// </summary>
        object _lockLoadRelationTable = new object();
        /// <summary>
        /// 关联数据表
        /// </summary>
        List<TableInfo> _RelationTables = new List<TableInfo>();
        /// <summary>
        /// 获取关联表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TableInfo GetRelationTable(long tableId)
        {
            //如果没有加载过此表，则加载一次
            lock (_lockLoadRelationTable)
            {
                //关联表是否已经加载
                TableInfo tableInfo = _RelationTables.Find(p => p.Id == tableId);
                if (tableInfo != null) return tableInfo;

                //表配置信息
                tableInfo = AppGlobal.GetTableConfig(tableId);
                //添加到关联表信息
                if (tableInfo != null) _RelationTables.Add(tableInfo);
                //返回表信息
                return tableInfo;
            }
        }

        /// <summary>
        /// 上下比例调整委托
        /// </summary>
        /// <param name="topHeight"></param>
        /// <param name="bottomHeight"></param>
        public delegate void GridBLChange_Delegate(double topHeight, double bottomHeight);
        /// <summary>
        /// 上下比例调整事件
        /// </summary>
        public event GridBLChange_Delegate GridBLChange_Event;


        /// <summary>
        /// 权限用户列表 如：1,2,3,4,5
        /// </summary>
        public string _AuthorityUserIds = null;
        #endregion

        #region 构造&加载
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="tableId">主表编号</param>
        /// <param name="moduleId">模块编号</param>
        /// <param name="isPopWin">弹窗</param>
        public ListUC(long tableId, long moduleId, List<long> moduleIds, string moduleName)
        {
            //初始列表界面
            InitListUC(tableId, moduleId, moduleIds, moduleName);
        }
        /// <summary>
        /// 构造选择回传窗口
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="moduleId"></param>
        /// <param name="moduleIds"></param>
        /// <param name="moduleName"></param>
        /// <param name="isChooseCallback"></param>
        public ListUC(long tableId, long moduleId, List<long> moduleIds, string moduleName, bool isChooseCallback)
        {
            //是否弹窗
            _isPopWindow = true;
            _isChooseCallbackWindow = true;

            //初始列表界面
            InitListUC(tableId, moduleId, moduleIds, moduleName);
        }
        /// <summary>
        /// 构造扩展列表窗口
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="moduleId"></param>
        /// <param name="moduleIds"></param>
        /// <param name="moduleName"></param>
        /// <param name="isExtList"></param>
        /// <param name="extListParentId"></param>
        /// <param name="extListIsAudit"></param>
        /// <param name="extPrintDataSet"></param>
        /// <param name="extListTopTableId"></param>
        public ListUC(long tableId, long moduleId, List<long> moduleIds, string moduleName, bool isExtList, long extListParentId, bool extListIsAudit, DataSet extPrintDataSet, long extListTopTableId)
        {
            //是否弹窗
            _isPopWindow = true;
            //是否扩展列表
            _isExtListWindow = true;
            //扩展上表明细ID
            _extListParentId = extListParentId;
            //扩展上表主表是否审核
            _extListIsAudit = extListIsAudit;
            //扩展上表及主表的数据
            _extListPrintDataSet = extPrintDataSet;
            //扩展上表主表ID
            _extListTopTableId = extListTopTableId;

            //初始列表界面
            InitListUC(tableId, moduleId, moduleIds, moduleName);
        }
        /// <summary>
        /// 初始列表界面
        /// </summary>
        private void InitListUC(long tableId, long moduleId, List<long> moduleIds, string moduleName)
        {
            //操作数据表编号
            _tableId = tableId;
            if (moduleId <= 0)
            {
                //操作模块编号
                AppGlobal.GetModuleId(tableId, ref _moduleId, ref _moduleIds);
            }
            else
            {
                //操作模块编号
                _moduleId = moduleId;
                _moduleIds = moduleIds;
                _moduleName = moduleName;
            }

            //分页尺码
            _PageSize = AppGlobal.GetSysConfigReturnInt("System_PageSize", _PageSize);

            //初始界面
            InitializeComponent();

            //不要间隔
            this.Margin = new Thickness(0);

            //查询行不显示 如果有查询条件时才显示
            gridQueryRow.Height = new GridLength(0);

            //明细表不显示 如果是双表才显示
            gridBottomRow.Height = new GridLength(0);
            this.dataGridBottom.Visibility = Visibility.Hidden;
            this.panelTreeUC.Visibility = Visibility.Hidden;


            //不可拖动行高
            if (!AppGlobal.GetSysConfigReturnBool("System_UI_RowHeightDrag", true))
            {
                this.dataGridTop.CanUserResizeRows = false;
                this.dataGridTop.CanUserResizeRows = false;

                this.dataGridBottom.CanUserResizeRows = false;
                this.dataGridBottom.CanUserResizeRows = false;
            }
            //最小行高
            if (AppGlobal.GetSysConfigReturnDouble("System_UI_RowHeight", 0) > 0)
            {
                double rowHeight = AppGlobal.GetSysConfigReturnDouble("System_UI_RowHeight", 0);

                this.dataGridTop.RowHeight = rowHeight;
                this.dataGridTop.MinRowHeight = rowHeight;

                this.dataGridBottom.RowHeight = rowHeight;
                this.dataGridBottom.MinRowHeight = rowHeight;
            }

            //表格边框颜色
            string borderColor = AppGlobal.GetSysConfigReturnString("System_UI_GridBoderColor");
            if (!string.IsNullOrWhiteSpace(borderColor))
            {
                Brush brushBorderColor = AppGlobal.HtmlColorToBrush(borderColor);

                this.dataGridTop.BorderBrush = brushBorderColor;
                this.dataGridTop.BorderThickness = new Thickness(1);
                this.dataGridBottom.BorderBrush = brushBorderColor;
                this.dataGridBottom.BorderThickness = new Thickness(1);
            }

            //加载事件
            this.Loaded += ListUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isPopWindow)
            {
                //弹出窗口初始页面大小
                InitPopWinSize();

                //弹出窗口大小改变事件
                Components.PopWindow win = (_ParentWindow as Components.PopWindow);
                win.SizeChanged += WinPopWindow_SizeChanged;
                win.CloseWindow_Event += ListUC_CloseWindow_Event;
                win.SizeChanged += MainWindow_SizeChanged;
            }
            else
            {
                //初始窗口大小
                InitSize();
                AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
            }

            //是否拥有模块权限
            if (!HasModuleAuthority()) return;

            //重新加载数据按钮事件
            this.btnReload.Click += BtnReload_Click;

            //查询事件
            this.btnQuery.Click += BtnQuery_Click;


            //选择主表行
            //this.dataGridTop.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
            this.dataGridTop.SelectionChanged += DataGridTop_SelectionChanged;
            this.dataGridTop.LoadingRow += DataGridTop_LoadingRow;
            this.dataGridTop.InitializingNewItem += DataGridTop_InitializingNewItem;
            this.dataGridTop.RowEditEnding += DataGridTop_RowEditEnding;
            this.dataGridTop.MouseDoubleClick += DataGridTop_MouseDoubleClick;
            this.dataGridTop.ColumnDisplayIndexChanged += DataGridTop_ColumnDisplayIndexChanged;
            this.dataGridTop.CellEditEnding += DataGridTop_CellEditEnding;
            this.dataGridTop.PreviewMouseLeftButtonDown += DataGridTop_PreviewMouseLeftButtonDown;
            this.dataGridTop.PreviewKeyDown += DataGridTop_PreviewKeyDown;
            this.dataGridTop.Sorting += DataGridTop_Sorting;

            this.borderActions.MouseDown += BorderActions_MouseDown;
            this.borderActions.MouseUp += BorderActions_MouseUp;
            //this.borderActions.MouseMove += BorderActions_MouseMove;
            this.MouseMove += BorderActions_MouseMove;

            //this.dataGridBottom.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
            this.dataGridBottom.LoadingRow += DataGridBottom_LoadingRow;
            this.dataGridBottom.InitializingNewItem += DataGridBottom_InitializingNewItem;
            this.dataGridBottom.RowEditEnding += DataGridBottom_RowEditEnding;
            this.dataGridBottom.SelectionChanged += DataGridBottom_SelectionChanged;
            this.dataGridBottom.ColumnDisplayIndexChanged += DataGridBottom_ColumnDisplayIndexChanged;
            this.dataGridBottom.CellEditEnding += DataGridBottom_CellEditEnding;
            this.dataGridBottom.PreviewMouseLeftButtonDown += DataGridBottom_PreviewMouseLeftButtonDown;
            this.dataGridBottom.PreviewKeyDown += DataGridBottom_PreviewKeyDown;
            this.dataGridBottom.Sorting += DataGridBottom_Sorting;


            this.btnTopPageExt.MouseLeftButtonDown += BtnTopPageExt_MouseLeftButtonDown;
            this.btnBottomPageExt.MouseLeftButtonDown += BtnBottomPageExt_MouseLeftButtonDown;

            this.imgTopPreview.MouseLeftButtonDown += ImgTopPreview_MouseLeftButtonDown;
            this.imgBottomPreview.MouseLeftButtonDown += ImgBottomPreview_MouseLeftButtonDown;

            //保存列排序
            this.btnSaveCellOrder.Click += BtnSaveCellOrder_Click;

            //加载数据
            LoadData();

            if (_isPopWindow)
            {
                //弹出窗体快捷键
                _ParentWindow.KeyUp += MainWindow_KeyUp;
            }
            else
            {
                //列表快捷键
                AppData.MainWindow.KeyUp += MainWindow_KeyUp;
            }
        }

        /// <summary>
        /// 从表预览图点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgBottomPreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double tag = DataType.Double(this.imgBottomPreview.Tag, 0);
            if (this.imgBottomPreview.Width < tag)
            {
                this.imgBottomPreview.Width = tag;
                this.imgBottomPreview.Height = tag;
            }
            else
            {
                this.imgBottomPreview.Width = 50;
                this.imgBottomPreview.Height = 50;
            }
        }
        /// <summary>
        /// 主表预览图点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgTopPreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double tag = DataType.Double(this.imgTopPreview.Tag, 0);
            if (this.imgTopPreview.Width < tag)
            {
                this.imgTopPreview.Width = tag;
                this.imgTopPreview.Height = tag;
            }
            else
            {
                this.imgTopPreview.Width = 50;
                this.imgTopPreview.Height = 50;
            }
        }

        /// <summary>
        /// 展开或收起分页 子表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBottomPageExt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border borderBottomPage = sender as Border;
            int tag = DataType.Int(borderBottomPage.Tag, 1);
            if (tag == 1)
            {
                this.borderBottomPages.Visibility = Visibility.Collapsed;
                borderBottomPage.Tag = 0;
            }
            else
            {
                this.borderBottomPages.Visibility = Visibility.Visible;
                borderBottomPage.Tag = 1;
            }
        }

        /// <summary>
        /// 展开或收起分页 主表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTopPageExt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border borderTopPage = sender as Border;
            int tag = DataType.Int(borderTopPage.Tag, 1);
            if (tag == 1)
            {

                this.borderTopPages.Visibility = Visibility.Collapsed;
                borderTopPage.Tag = 0;
            }
            else
            {
                this.borderTopPages.Visibility = Visibility.Visible;
                borderTopPage.Tag = 1;
            }
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }
        /// <summary>
        /// 窗口大小改变
        /// </summary>
        private void InitSize()
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.Width = AppData.MainWindow.WinWidth - 82;
                this.Height = AppData.MainWindow.WinHeight - 82;

                if (_ParentWindow != null && _ParentWindow is Views.Components.PopWindow)
                {
                    //弹出窗口
                    this.Width = _ParentWindow.Width - 10;
                    this.Height = _ParentWindow.Height - 50;
                }

                //查询行高度
                if (_queryWhereCtrlCount > 0)
                {
                    double winWidth = this.Width;
                    winWidth -= 120;

                    double queryCtrlRowCount = winWidth / 320;
                    int queryCtrlRowCountInt = queryCtrlRowCount < 1 ? 1 : (int)queryCtrlRowCount;
                    double queryCtrlRows = (double)_queryWhereCtrlCount / (double)queryCtrlRowCountInt;
                    int queryCtrlRowsInt = (int)queryCtrlRows;
                    if (queryCtrlRowsInt < queryCtrlRows) queryCtrlRowsInt++;

                    double heightVal = (queryCtrlRowsInt + 1) * 30;
                    this.gridQueryRow.Height = new GridLength(heightVal);
                }

                this.gridMain.Width = this.Width;
                this.gridMain.Height = this.Height;

                double height = this.gridMain.Height - this.gridQueryRow.Height.Value - this.gridActionsRow.Height.Value;
                double topHeight = height * _topBL;
                double bottomHeight = height * _bottomBL;

                this.dataGridTop.MaxHeight = topHeight;
                this.dataGridBottom.MaxHeight = bottomHeight;
                this.panelTreeUC.MaxHeight = bottomHeight;

                this.gridTopRow.Height = new GridLength(topHeight);
                this.gridBottomRow.Height = new GridLength(bottomHeight);

                //比例调整事件
                if (GridBLChange_Event != null)
                {
                    GridBLChange_Event(topHeight, bottomHeight);
                }

                return null;
            }));
        }

        /// <summary>
        /// 弹出窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinPopWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitPopWinSize();
        }
        /// <summary>
        /// 初始弹窗大小
        /// </summary>
        private void InitPopWinSize()
        {
            try
            {
                Components.PopWindow win = (_ParentWindow as Components.PopWindow);
                win.scrollMainFrame.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                win.scrollMainFrame.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                this.Width = win.ActualWidth - 10;
                this.Height = win.ActualHeight - 50;

                this.gridMain.Height = this.Height;

                double height = this.gridMain.Height - this.gridQueryRow.Height.Value - this.gridActionsRow.Height.Value;
                this.gridTopRow.Height = new GridLength(height);
            }
            catch { }
        }
        /// <summary>
        /// 弹窗关闭事件
        /// </summary>
        /// <param name="win"></param>
        private void ListUC_CloseWindow_Event(Components.PopWindow win)
        {
            CloseUC();
        }
        /// <summary>
        /// 是否有权限
        /// </summary>
        /// <returns></returns>
        private bool HasModuleAuthority()
        {
            //是否有权限
            bool hasAuthority = false;

            try
            {
                //是否三表结构扩展窗口
                if (_isExtListWindow && _extListTopTableId > 0)
                {
                    //是否拥有表权限
                    hasAuthority = AppGlobal.HasTableAuthorityWithUser(_extListTopTableId);
                }
                else
                {
                    //是否拥有表权限
                    hasAuthority = AppGlobal.HasTableAuthorityWithUser(_tableId);
                }
            }
            catch { }

            //是否有权限
            if (!hasAuthority)
            {
                this.panelNonAuthority.Visibility = Visibility.Visible;
                this.borderActions.Visibility = Visibility.Collapsed;

                this.dataGridTop.Visibility = Visibility.Collapsed;
                this.dataGridBottom.Visibility = Visibility.Collapsed;

                this.panelTreeUC.Visibility = Visibility.Collapsed;

                //有权限
                return false;
            }

            return true;
        }
        #endregion

        #region 加载表配置、生成数据列、生成查询控件
        /// <summary>
        /// 选择图标事件
        /// </summary>
        public event RoutedEventHandler BtnChooseIcon_Event;
        /// <summary>
        /// 选择文件事件
        /// </summary>
        public event RoutedEventHandler BtnChooseFile_Event;
        /// <summary>
        /// 上传文件事件
        /// </summary>
        public event RoutedEventHandler BtnUploadFile_Event;
        /// <summary>
        /// 选择数据表事件
        /// </summary>
        public event RoutedEventHandler BtnChooseTable_Event;
        /// <summary>
        /// 编辑弹出文本事件
        /// </summary>
        public event RoutedEventHandler BtnEditPopText_Event;
        /// <summary>
        /// 编辑富文本事件
        /// </summary>
        public event RoutedEventHandler BtnEditFullText_Event;

        /// <summary>
        /// 下拉列表文本改变事件
        /// </summary>
        public event RoutedEventHandler TxtChange_Event;

        /// <summary>
        /// 加载表配置
        /// </summary>
        private void LoadTableConfig()
        {
            //是否已经加载
            if (_tableConfig != null) return;

            //绑定列表事件
            //选择图标事件
            BtnChooseIcon_Event += ListUC_BtnChooseIcon_Event;
            //选择文件事件
            BtnChooseFile_Event += ListUC_BtnChooseFile_Event;
            //上传文件事件
            BtnUploadFile_Event += ListUC_BtnUploadFile_Event;
            //选择数据表事件
            BtnChooseTable_Event += ListUC_BtnChooseTable_Event;
            //编辑弹出文本
            BtnEditPopText_Event += ListUC_BtnEditPopText_Event;
            //编辑富文本
            BtnEditFullText_Event += ListUC_BtnEditFullText_Event;
            //内容改变
            TxtChange_Event += ListUC_TxtChange_Event;

            try
            {
                //********************************************************************************************
                #region 加载主表配置
                //表配置信息
                _tableConfig = AppGlobal.GetTableConfig(_tableId);
                //是否加载主表成功
                if (_tableConfig == null)
                {
                    //未创建表
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //需要刷新表配置
                        AppData.MainWindow.ShowRenewTC = true;

                        this.lblLoadTableErrorText.Text = "加载表配置失败,请检查表主键(" + _tableId + ")是否存在！";
                        return null;
                    }));

                    _loadingTableConfigError = true;
                    return;
                }

                try
                {
                    if (_isPopWindow)
                    {
                        //弹窗引用后关闭
                        (_ParentWindow as Components.PopWindow)._YYHGB = _tableConfig.YYHGB;
                    }
                }
                catch { }

                #endregion

                #region 查询表是否创建
                if (_tableConfig.IsRealTable)
                {
                    try
                    {
                        //用户自主创建的表
                        if (_tableConfig.TableName.StartsWith("C_"))
                        {
                            //查询表是否创建
                            bool flagHasTable = SQLiteDao.TableIsExists(_tableConfig.TableName);
                            if (!flagHasTable)
                            {
                                //未创建表
                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    AppAlert.Alert("数据表未创建，请检查！", "加载表失败：");
                                    this.lblLoadTableErrorText.Text = "数据表未创建，请检查！";
                                    return null;
                                }));
                                _loadingTableConfigError = true;
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "查询表是否创建异常");
                    }
                }
                #endregion

                //********************************************************************************************
                #region 加载主表列
                //加载主表列
                List<long> addedCellIds = new List<long>();

                if (_tableConfig.Cells != null && _tableConfig.Cells.Count > 0)
                {
                    //是否有查询
                    bool hasQuery = false;
                    _queryWhereCtrlCount = 0;

                    //表列
                    var listCells = _tableConfig.Cells.OrderBy(p => p.UserCellOrder);
                    foreach (CellInfo cellInfo in listCells)
                    {
                        if (!_tableConfig.IsRealTable)
                        {
                            //得到所有列名 - 视图使用
                            _viewCellNames.Add(cellInfo.CellName);
                        }

                        //上级对象
                        if (cellInfo.CellName.Equals("ParentId") && cellInfo.ForeignTableId > 0) cellInfo.IsShow = true;

                        //添加列
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //默认显示列
                            Visibility visibility = cellInfo.IsShow ? Visibility.Visible : Visibility.Hidden;

                            //添加数据列
                            AddDataGridColumn(dataGridTop, cellInfo, visibility);

                            //已经添加了的列编号
                            addedCellIds.Add(cellInfo.Id);

                            //是否查询列
                            if (cellInfo.IsQuery)
                            {
                                if (BuildQueryCtls(cellInfo))
                                {
                                    hasQuery = true;
                                    _queryWhereCtrlCount++;
                                }
                            }
                            return null;
                        }));
                    }

                    //显示查询行
                    if (hasQuery)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //更多查询条件
                            BuildMoreQueryCtls();

                            this.gridQueryRow.Height = new GridLength(55);
                            InitSize();
                            return null;
                        }));
                    }
                }
                #endregion

                //要显示主键的表
                if (_tableConfig.TableName.Equals("Sys_Tables") || _tableConfig.XSZJ)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //显示主键
                        this.ColumnTopId.Visibility = Visibility.Visible;
                        return null;
                    }));
                }

                try
                {
                    //增加主表右键菜单
                    AddRightKeyMenus();
                }
                catch { }

                //单表结构
                if (_tableConfig.IsSingleTable)
                {
                    return;
                }
                if (_isPopWindow)
                {
                    //弹窗只加载单表
                    return;
                }

                //********************************************************************************************
                //加载子表
                //获取子表配置
                _tableConfig.SubTable = AppGlobal.GetSubTableConfig(_tableConfig.Id);

                //是否有子表
                if (_tableConfig.SubTable != null)
                {
                    #region 加载三表
                    //加载三表配置
                    _tableConfig.SubTable.ThreeTable = AppGlobal.GetThreeTableConfig(_tableConfig.SubTable.Id);
                    #endregion

                    #region 加载主表显示比例
                    //加载主表显示比例
                    try
                    {
                        if (_tableConfig.TopBL > 0)
                        {
                            _topBL = _tableConfig.TopBL;
                            _bottomBL = 1 - _topBL;// _tableConfig.BottomBL;
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                InitSize();
                                return null;
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载主表显示比例异常");
                    }
                    #endregion

                    #region 表是否创建
                    if (_tableConfig.SubTable.IsRealTable)
                    {
                        try
                        {
                            //用户自主创建的表
                            if (_tableConfig.SubTable.TableName.StartsWith("C_"))
                            {
                                //查询表是否创建
                                bool flagHasTable = SQLiteDao.TableIsExists(_tableConfig.SubTable.TableName);
                                if (!flagHasTable)
                                {
                                    //未创建表
                                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                    {
                                        AppAlert.Alert("数据子表未创建，请检查！", "加载表失败：");
                                        this.lblLoadTableErrorText.Text = "数据子表未创建，请检查！";
                                        return null;
                                    }));
                                    _loadingTableConfigError = true;
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.AddException(ex, "查询表是否创建异常");
                        }
                    }
                    #endregion

                    //********************************************************************************************
                    #region 加载子表列
                    if (_tableConfig.SubTable.Cells != null && _tableConfig.SubTable.Cells.Count > 0)
                    {
                        var listSubCells = _tableConfig.SubTable.Cells.OrderBy(p => p.UserCellOrder);
                        foreach (CellInfo cellInfo in listSubCells)
                        {
                            //添加列
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                //默认显示列
                                Visibility visibility = cellInfo.IsShow ? Visibility.Visible : Visibility.Hidden;

                                //添加数据列
                                AddDataGridColumn(dataGridBottom, cellInfo, visibility);
                                return null;
                            }));
                        }
                    }
                    #endregion

                    //要显示主键的表
                    if (_tableConfig.SubTable.XSZJ)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //显示主键
                            this.ColumnBottomId.Visibility = Visibility.Visible;
                            return null;
                        }));
                    }

                    try
                    {
                        //增加子表右键菜单
                        AddRightKeyMenus(false);
                    }
                    catch { }

                    //初始从表搜索
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        InitSearchBottomData();
                        return null;
                    }));
                }
            }
            catch (Exception ex)
            {
                //加载表失败
                ErrorHandler.AddException(ex, "加载表配置失败");
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    this.lblLoadTableErrorText.Text = "加载表配置失败！";
                    return null;
                }));
                _loadingTableConfigError = true;
                return;
            }
        }

        /// <summary>
        /// 更多查询条件
        /// </summary>
        private void BuildMoreQueryCtls()
        {
            if (_tableConfig.TableType == TableType.视图 && _tableConfig.TableSubType == TableSubType.主从视图)
            {
                if (_tableConfig.Cells.Exists(p => p.CellName.Equals("MZMX_SL")) &&
                    _tableConfig.Cells.Exists(p => p.CellName.Equals("MZMX_WCSL")))
                {
                    //生成已经完成查询条件控件
                    CellInfo cellQueryYJWC = new CellInfo()
                    {
                        CnName = "已经完成",
                        CellName = "SYS_SFYJWC",
                        ValType = "bool",
                    };

                    //生成控件
                    BuildQueryCtls(cellQueryYJWC);
                }
            }
        }

        /// <summary>
        /// 添加绑定列到数据表 生成控件
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="row"></param>
        private void AddDataGridColumn(DataGrid dataGrid, CellInfo cellInfo, Visibility visibility = Visibility.Visible)
        {
            string cellName = cellInfo.CellName;
            string cnName = cellInfo.CnName;
            string valType = cellInfo.ValType;
            int cellWidth = cellInfo.CellWidth;

            //不可编辑的列
            if (AppGlobal.List_CannotEditCells.Contains(cellInfo.CellName.ToUpper()))
            {
                cellInfo.CanEdit = false;
            }

            //枚举类型 分割符
            valType = valType.Replace("，", ",");

            //是否只读
            bool isReadOnly = false;

            //内容居中样式
            Style elementStyle = null;
            Style elementEditingStyle = null;

            //动态绑定
            Binding binding = new Binding();
            Binding bindingEdit = new Binding();
            binding.Path = new PropertyPath(cellName);
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingEdit.Path = new PropertyPath(cellName);
            bindingEdit.Mode = BindingMode.TwoWay;
            bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            //模版列
            DataGridTemplateColumn templateColumn = new DataGridTemplateColumn();
            templateColumn.Header = cnName;
            templateColumn.IsReadOnly = isReadOnly;
            templateColumn.Visibility = visibility;
            templateColumn.SortMemberPath = cellName;
            if (cellWidth > 0) templateColumn.Width = new DataGridLength(cellWidth);

            //文本列
            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = cnName;
            textColumn.IsReadOnly = isReadOnly;
            textColumn.Visibility = visibility;
            textColumn.SortMemberPath = cellName;
            if (cellWidth > 0) textColumn.Width = new DataGridLength(cellWidth);

            //复选框列
            DataGridCheckBoxColumn checkBoxColumn = new DataGridCheckBoxColumn();
            checkBoxColumn.Header = cnName;
            checkBoxColumn.IsReadOnly = isReadOnly;
            checkBoxColumn.Visibility = visibility;
            checkBoxColumn.SortMemberPath = cellName;
            if (cellWidth > 0) checkBoxColumn.Width = new DataGridLength(cellWidth);

            if (cellInfo.IsEncrypt)
            {
                //加密的列
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.MinWidthProperty, 40D);
                lbl.SetValue(TextBlock.TextProperty, "******");
                dataTemplate.VisualTree = lbl;
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.Header = cnName;
                dataGrid.Columns.Add(templateColumn);
            }
            else if (valType.IndexOf(',') > 0)
            {
                #region 下拉列表

                //下拉列表值
                List<string> vals = valType.Split(',').ToList<string>();

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                //FrameworkElementFactory bd = new FrameworkElementFactory(typeof(Border));
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.MinWidthProperty, 80D);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);
                dataTemplate.VisualTree = lbl;
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.Header = cnName;

                //可编辑
                if (cellInfo.CanEdit)
                {
                    //编辑模式
                    DataTemplate dataTemplateEditing = new DataTemplate();
                    FrameworkElementFactory cmb = new FrameworkElementFactory(typeof(ComboBox));
                    cmb.SetValue(ComboBox.ItemsSourceProperty, vals);
                    cmb.SetValue(ComboBox.TextProperty, binding);
                    cmb.SetValue(ComboBox.MarginProperty, new Thickness(3));
                    cmb.SetValue(ComboBox.IsEditableProperty, true);
                    cmb.SetValue(ComboBox.IsEnabledProperty, true);
                    cmb.SetValue(ComboBox.TagProperty, cellName);
                    //cmb.AddHandler(LoadedEvent, new RoutedEventHandler(ListComboBox_Loaded));
                    //cmb.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(Cmb_SelectionChanged));
                    cmb.AddHandler(LostFocusEvent, new RoutedEventHandler(Cmb_LostFocus));
                    dataTemplateEditing.VisualTree = cmb;
                    templateColumn.CellEditingTemplate = dataTemplateEditing;
                }

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (cellName.Equals("Icon") && valType.Equals("string"))
            {
                #region 图标列

                //编辑模版
                bindingEdit.Converter = new AppCode.Converters.IconConverter();

                binding.Mode = BindingMode.Default;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.Default;

                Binding bindingIconColor = new Binding();
                bindingIconColor.Path = new PropertyPath(cellName);
                bindingIconColor.Mode = BindingMode.TwoWay;
                bindingIconColor.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                bindingIconColor.Converter = new AppCode.Converters.IconColorConverter();

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory panel = new FrameworkElementFactory(typeof(WrapPanel));
                FrameworkElementFactory img = new FrameworkElementFactory(typeof(Image));
                img.SetValue(Image.SourceProperty, bindingEdit);
                img.SetBinding(Image.TagProperty, binding);
                img.SetValue(Image.WidthProperty, 24D);
                img.SetValue(Image.HeightProperty, 24D);
                img.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                img.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
                FrameworkElementFactory icon = new FrameworkElementFactory(typeof(PackIconMaterial));
                icon.SetValue(PackIconMaterial.KindProperty, bindingEdit);
                icon.SetValue(PackIconMaterial.ForegroundProperty, bindingIconColor);
                icon.SetValue(PackIconMaterial.WidthProperty, 24D);
                icon.SetValue(PackIconMaterial.HeightProperty, 24D);
                icon.SetValue(PackIconMaterial.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                icon.SetValue(PackIconMaterial.VerticalAlignmentProperty, VerticalAlignment.Center);
                panel.AppendChild(img);
                panel.AppendChild(icon);
                dataTemplate.VisualTree = panel;

                templateColumn.CellTemplate = dataTemplate;
                templateColumn.Header = cnName;
                templateColumn.MinWidth = 100;

                //可编辑
                if (cellInfo.CanEdit)
                {
                    bindingEdit.Mode = BindingMode.Default;
                    bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger.Default;

                    //编辑模式
                    DataTemplate dataTemplateEditing = new DataTemplate();
                    FrameworkElementFactory gridEdit = new FrameworkElementFactory(typeof(Grid));
                    FrameworkElementFactory imgEdit = new FrameworkElementFactory(typeof(Image));
                    imgEdit.SetValue(Image.SourceProperty, bindingEdit);
                    imgEdit.SetBinding(Image.TagProperty, binding);
                    imgEdit.SetValue(Image.WidthProperty, 24D);
                    imgEdit.SetValue(Image.HeightProperty, 24D);
                    imgEdit.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    imgEdit.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
                    FrameworkElementFactory iconEdit = new FrameworkElementFactory(typeof(PackIconMaterial));
                    iconEdit.SetValue(PackIconMaterial.KindProperty, bindingEdit);
                    iconEdit.SetValue(PackIconMaterial.ForegroundProperty, bindingIconColor);
                    iconEdit.SetValue(PackIconMaterial.WidthProperty, 24D);
                    iconEdit.SetValue(PackIconMaterial.HeightProperty, 24D);
                    iconEdit.SetValue(PackIconMaterial.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                    iconEdit.SetValue(PackIconMaterial.VerticalAlignmentProperty, VerticalAlignment.Center);
                    FrameworkElementFactory btn = new FrameworkElementFactory(typeof(Button));
                    FrameworkElementFactory btnChooseText = new FrameworkElementFactory(typeof(TextBlock));
                    btnChooseText.SetValue(TextBlock.TextProperty, "选择");
                    btnChooseText.SetValue(TextBlock.NameProperty, "btnText");
                    btn.AppendChild(btnChooseText);
                    btn.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                    btn.SetValue(Button.TagProperty, cellName);
                    btn.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                    btn.AddHandler(Button.ClickEvent, BtnChooseIcon_Event);
                    gridEdit.AppendChild(imgEdit);
                    gridEdit.AppendChild(iconEdit);
                    gridEdit.AppendChild(btn);
                    dataTemplateEditing.VisualTree = gridEdit;

                    templateColumn.CellEditingTemplate = dataTemplateEditing;
                }

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (valType.Equals("date") || valType.Equals("datetime"))
            {
                #region 日期格式

                //绑定格式
                binding.StringFormat = valType.Equals("date") ? "{0:yyyy-MM-dd}" : "{0:yyyy-MM-dd HH:mm:ss}";

                double cWidth = valType.Equals("date") ? 100 : 140;
                if (cellWidth < cWidth)
                {
                    templateColumn.Width = new DataGridLength(cWidth);
                }

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory contentFactory = new FrameworkElementFactory(typeof(StackPanel));
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);

                contentFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                contentFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
                contentFactory.AppendChild(lbl);
                dataTemplate.VisualTree = contentFactory;

                templateColumn.CellTemplate = dataTemplate;
                templateColumn.Header = cnName;

                //可编辑
                if (cellInfo.CanEdit)
                {
                    //编辑模式
                    DataTemplate dataTemplateEditing = new DataTemplate();
                    FrameworkElementFactory contentFactoryEditing = new FrameworkElementFactory(typeof(StackPanel));
                    FrameworkElementFactory datePicker = new FrameworkElementFactory(typeof(DatePicker));
                    datePicker.SetBinding(DatePicker.SelectedDateProperty, binding);
                    datePicker.SetValue(DatePicker.VerticalAlignmentProperty, VerticalAlignment.Center);
                    datePicker.SetValue(DatePicker.TagProperty, cellInfo.CellName);
                    datePicker.AddHandler(DatePicker.LostFocusEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                    {
                        //日期控件失去焦点
                        DatePicker dp = sender as DatePicker;
                        if (!dp.SelectedDate.HasValue) dp.SelectedDate = DateTime.Now;
                        DateTime dt = dp.SelectedDate.Value;

                        //操作的数据表
                        DataGrid dataGrid2 = this.GetParentObject<DataGrid>(dp);
                        DataRowView row = (dataGrid2.SelectedItem as DataRowView);

                        //标记更新
                        row[dp.Tag.ToString()] = dp.SelectedDate;
                        row[AppGlobal.DataGridEditStateCellName] = true;
                    }));

                    contentFactoryEditing.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
                    contentFactoryEditing.AppendChild(datePicker);
                    dataTemplateEditing.VisualTree = contentFactoryEditing;

                    templateColumn.CellEditingTemplate = dataTemplateEditing;
                }

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (valType.Equals("bool"))
            {
                //复选框
                elementStyle = this.FindResource("DataGridCellCenter_CheckBox") as Style;
                elementEditingStyle = this.FindResource("DataGridCellCenter_CheckBox_Editing") as Style;

                checkBoxColumn.IsReadOnly = !cellInfo.CanEdit;

                checkBoxColumn.Binding = binding;
                checkBoxColumn.ElementStyle = elementStyle;
                checkBoxColumn.EditingElementStyle = elementEditingStyle;

                dataGrid.Columns.Add(checkBoxColumn);
                return;
            }
            else if (cellInfo.IsDropDown && cellInfo.ForeignTableId > 0)
            {
                #region 下拉列表 自动完成 忽略的方法

                /* 忽略的方法
                //下拉项
                AppCode.Models.DropDownInfo ddInfo = new DropDownInfo()
                {
                    Name = cellName,
                    ChooseValue = string.Empty
                };

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.MinWidthProperty, 80D);
                lbl.SetBinding(TextBlock.TextProperty, binding);
                dataTemplate.VisualTree = lbl;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory cmb = new FrameworkElementFactory(typeof(ComboBox));
                cmb.SetValue(ComboBox.TextProperty, binding);
                cmb.SetValue(ComboBox.MarginProperty, new Thickness(3));
                cmb.SetValue(ComboBox.IsEditableProperty, true);
                cmb.SetValue(ComboBox.IsEnabledProperty, true);
                cmb.SetValue(ComboBox.TagProperty, ddInfo);
                cmb.AddHandler(ComboBox.LoadedEvent, new RoutedEventHandler(ComboBoxAutoComple_Loaded));
                //cmb.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, TxtChange_Event, false);
                dataTemplateEditing.VisualTree = cmb;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                */
                #endregion

                #region 下拉展开表
                try
                {
                    //线程加载表配置
                    System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            //加载关联表
                            GetRelationTable(cellInfo.ForeignTableId);
                        }
                        catch (Exception ex) { }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
                catch { }

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.MinWidthProperty, 80D);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);
                dataTemplate.VisualTree = lbl;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory ddt = new FrameworkElementFactory(typeof(Controls.DropDownTable));
                if (!cellInfo.XZXLZ) ddt.SetBinding(Controls.DropDownTable.TextProperty, binding);
                ddt.SetBinding(Controls.DropDownTable.SearchTextProperty, binding);
                ddt.SetValue(TagProperty, cellInfo);
                ddt.AddHandler(LoadedEvent, new RoutedEventHandler(DropDownTable_Loaded));
                dataTemplateEditing.VisualTree = ddt;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (cellInfo.ForeignTableId > 0)
            {
                try
                {
                    //线程加载表配置
                    System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            //加载关联表
                            GetRelationTable(cellInfo.ForeignTableId);
                        }
                        catch (Exception ex) { }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
                catch { }

                #region 弹窗表列

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory panel = new FrameworkElementFactory(typeof(WrapPanel));
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetBinding(TextBlock.TextProperty, binding);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                dataTemplate.VisualTree = lbl;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory gridEdit = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory lbl2 = new FrameworkElementFactory(typeof(TextBlock));
                lbl2.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl2.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl2.SetBinding(TextBlock.TextProperty, binding);
                FrameworkElementFactory btn = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnChooseText = new FrameworkElementFactory(typeof(TextBlock));
                btnChooseText.SetValue(TextBlock.TextProperty, "选择");
                btnChooseText.SetValue(TextBlock.NameProperty, "btnText");
                btn.AppendChild(btnChooseText);
                btn.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                //btn.SetValue(Button.ContentProperty, "选择");
                btn.SetValue(Button.TagProperty, cellInfo);
                btn.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btn.SetValue(Button.MarginProperty, new Thickness(0, 0, 60, 0));
                btn.AddHandler(Button.ClickEvent, BtnChooseTable_Event);
                FrameworkElementFactory btnClear = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnClearText = new FrameworkElementFactory(typeof(TextBlock));
                btnClearText.SetValue(TextBlock.TextProperty, "清除");
                btnClearText.SetValue(TextBlock.NameProperty, "btnText");
                btnClear.AppendChild(btnClearText);
                btnClear.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                //btnClear.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                btnClear.SetValue(Button.MarginProperty, new Thickness(0, 0, 10, 0));
                //btnClear.SetValue(Button.ContentProperty, "清除");
                btnClear.SetValue(Button.TagProperty, cellInfo);
                btnClear.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btnClear.AddHandler(Button.ClickEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                {
                    //点击清除
                    try
                    {
                        Button btnClear2 = sender as Button;
                        CellInfo cell2 = btnClear2.Tag as CellInfo;
                        TextBlock lbl3 = this.GetChildObject<TextBlock>(this.GetParentObject<Grid>(btnClear2));
                        lbl3.Text = null;
                        DataGrid dataGrid2 = this.GetParentObject<DataGrid>(btnClear2);
                        DataRowView row = (dataGrid2.SelectedItem as DataRowView);
                        row[cell2.CellName] = DBNull.Value;
                        row[AppGlobal.DataGridEditStateCellName] = true;

                        //清除关联
                        ClearRelativeTable(cell2);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "点击清除按钮异常");
                    }
                }));

                gridEdit.AppendChild(lbl2);
                gridEdit.AppendChild(btn);
                gridEdit.AppendChild(btnClear);
                dataTemplateEditing.VisualTree = gridEdit;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;
                templateColumn.MinWidth = 200;

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (cellInfo.IsPopEdit && valType.Equals("string"))
            {
                #region 弹出编辑文本窗口列

                binding.Converter = new AppCode.Converters.StringEllipsisConvert();
                binding.ConverterParameter = 2000;

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 55, 0));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);
                FrameworkElementFactory btnDetail = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnDetailText = new FrameworkElementFactory(typeof(TextBlock));
                btnDetailText.SetValue(TextBlock.TextProperty, "查看");
                btnDetailText.SetValue(TextBlock.NameProperty, "btnText");
                btnDetail.AppendChild(btnDetailText);
                btnDetail.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                //btnDetail.SetValue(Button.ContentProperty, "查看");
                btnDetail.SetValue(Button.TagProperty, bindingEdit);
                btnDetail.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btnDetail.AddHandler(Button.ClickEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                {
                    //点击查看
                    try
                    {
                        Button btnShowDetail = sender as Button;
                        if (btnShowDetail.Tag == null || string.IsNullOrWhiteSpace(btnShowDetail.Tag.ToString())) return;

                        Parts.ShowPopTextUC uc = new Parts.ShowPopTextUC(btnShowDetail.Tag.ToString());
                        Views.Components.PageWindow win = new Views.Components.PageWindow(uc, "查看内容");
                        uc._ParentWindow = win;
                        win.Show();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "点击查看按钮异常");
                    }
                }));
                grid.AppendChild(lbl);
                grid.AppendChild(btnDetail);
                dataTemplate.VisualTree = grid;

                //TextBox txt = new TextBox();
                //txt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory gridEdit = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory txt = new FrameworkElementFactory(typeof(TextBox));
                txt.SetValue(TextBox.TextProperty, bindingEdit);
                txt.SetValue(TextBox.MaxHeightProperty, 36d);
                txt.SetValue(TextBox.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                FrameworkElementFactory btn = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnEditText = new FrameworkElementFactory(typeof(TextBlock));
                btnEditText.SetValue(TextBlock.TextProperty, "编辑");
                btnEditText.SetValue(TextBlock.NameProperty, "btnText");
                btn.AppendChild(btnEditText);
                btn.SetValue(Button.StyleProperty, this.FindResource("btnListCellEdit") as Style);
                //btn.SetValue(Button.ContentProperty, "编辑");
                btn.SetValue(Button.TagProperty, cellName);
                btn.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btn.AddHandler(Button.ClickEvent, BtnEditPopText_Event);
                gridEdit.AppendChild(txt);
                gridEdit.AppendChild(btn);
                dataTemplateEditing.VisualTree = gridEdit;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;
                templateColumn.MinWidth = 200;

                if (cellWidth <= 0) templateColumn.Width = new DataGridLength(200);

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (cellInfo.IsFullText && valType.Equals("string"))
            {
                #region 弹出富文本编辑窗口

                binding.Converter = new AppCode.Converters.StringEllipsisConvert();
                binding.ConverterParameter = 2000;

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 55, 0));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);
                FrameworkElementFactory btnDetail = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnDetailText = new FrameworkElementFactory(typeof(TextBlock));
                btnDetailText.SetValue(TextBlock.TextProperty, "查看");
                btnDetailText.SetValue(TextBlock.NameProperty, "btnText");
                btnDetail.AppendChild(btnDetailText);
                btnDetail.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                btnDetail.SetValue(Button.TagProperty, bindingEdit);
                btnDetail.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btnDetail.AddHandler(Button.ClickEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                {
                    //点击查看
                    try
                    {
                        Button btnShowDetail = sender as Button;
                        if (btnShowDetail.Tag == null || string.IsNullOrWhiteSpace(btnShowDetail.Tag.ToString())) return;

                        Parts.ShowPopFullTextUC uc = new Parts.ShowPopFullTextUC(btnShowDetail.Tag.ToString());
                        Views.Components.PageWindow win = new Views.Components.PageWindow(uc, "查看内容");
                        uc._ParentWindow = win;
                        win.Show();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "点击查看按钮异常");
                    }
                }));
                grid.AppendChild(lbl);
                grid.AppendChild(btnDetail);
                dataTemplate.VisualTree = grid;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory gridEdit = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory txt = new FrameworkElementFactory(typeof(TextBox));
                txt.SetValue(TextBox.TextProperty, bindingEdit);
                FrameworkElementFactory btn = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnEditText = new FrameworkElementFactory(typeof(TextBlock));
                btnEditText.SetValue(TextBlock.TextProperty, "编辑");
                btnEditText.SetValue(TextBlock.NameProperty, "btnText");
                btn.AppendChild(btnEditText);
                btn.SetValue(Button.StyleProperty, this.FindResource("btnListCellEdit") as Style);
                //btn.SetValue(Button.ContentProperty, "编辑");
                btn.SetValue(Button.TagProperty, cellName);
                btn.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btn.AddHandler(Button.ClickEvent, BtnEditFullText_Event);


                gridEdit.AppendChild(txt);
                gridEdit.AppendChild(btn);
                dataTemplateEditing.VisualTree = gridEdit;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;
                templateColumn.MinWidth = 200;

                if (cellWidth <= 0) templateColumn.Width = new DataGridLength(200);

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if ((cellInfo.IsFile || cellInfo.IsPic) && valType.Equals("string"))
            {
                #region 文件图片列

                Binding bindingFile = new Binding();
                bindingFile.Path = new PropertyPath(cellName);
                bindingFile.Converter = new AppCode.Converters.FileConvert();

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory gridFile = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory lblFile = new FrameworkElementFactory(typeof(TextBlock));
                lblFile.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lblFile.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lblFile.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lblFile.SetValue(TextBlock.MinWidthProperty, 200D);
                lblFile.SetValue(TextBlock.NameProperty, "lblShowText");
                lblFile.SetBinding(TextBlock.ToolTipProperty, binding);
                lblFile.SetBinding(TextBlock.TextProperty, bindingFile);

                FrameworkElementFactory btnDownload = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnDownloadText = new FrameworkElementFactory(typeof(TextBlock));
                btnDownloadText.SetValue(TextBlock.TextProperty, "另存为");
                btnDownloadText.SetValue(TextBlock.NameProperty, "btnText");
                btnDownload.AppendChild(btnDownloadText);
                btnDownload.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                //btnDownload.SetValue(Button.ContentProperty, btnDownloadText);
                btnDownload.SetValue(Button.MarginProperty, new Thickness(0, 0, 10, 0));
                btnDownload.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btnDownload.SetBinding(Button.TagProperty, binding);

                btnDownload.AddHandler(Button.ClickEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                {
                    //点击下载
                    try
                    {
                        Button btnFileUrl = sender as Button;
                        if (btnFileUrl.Tag == null || string.IsNullOrWhiteSpace(btnFileUrl.Tag.ToString())) return;

                        string path = btnFileUrl.Tag.ToString();
                        string saveDir = Core.Handler.UploadFileHandler.ChooseFolderDialog();

                        if (string.IsNullOrWhiteSpace(saveDir)) return;

                        string orgFilePath = AppDomain.CurrentDomain.BaseDirectory + path;
                        string saveFilePath = saveDir + System.IO.Path.GetFileName(path);
                        System.IO.File.Copy(orgFilePath, saveFilePath);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "点击下载按钮异常");
                    }
                }));
                gridFile.AppendChild(lblFile);

                dataTemplate.VisualTree = gridFile;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory gridEdit = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory txt = new FrameworkElementFactory(typeof(TextBox));
                txt.SetValue(TextBox.TextProperty, bindingEdit);

                FrameworkElementFactory btnUpload = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnUploadText = new FrameworkElementFactory(typeof(TextBlock));
                btnUploadText.SetValue(TextBlock.TextProperty, "上传");
                btnUploadText.SetValue(TextBlock.NameProperty, "btnUploadText");
                btnUpload.AppendChild(btnUploadText);
                btnUpload.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                btnUpload.SetValue(Button.TagProperty, cellName);
                btnUpload.SetValue(Button.MarginProperty, new Thickness(0, 0, 110, 0));
                btnUpload.AddHandler(Button.ClickEvent, BtnUploadFile_Event);
                btnUpload.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);

                FrameworkElementFactory btnChoose = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnChooseText = new FrameworkElementFactory(typeof(TextBlock));
                btnChooseText.SetValue(TextBlock.TextProperty, "选择");
                btnChooseText.SetValue(TextBlock.NameProperty, "btnText");
                btnChoose.AppendChild(btnChooseText);
                btnChoose.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                btnChoose.SetValue(Button.TagProperty, cellName);
                btnChoose.SetValue(Button.MarginProperty, new Thickness(0, 0, 60, 0));
                btnChoose.AddHandler(Button.ClickEvent, BtnChooseFile_Event);
                btnChoose.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);

                FrameworkElementFactory btnClear = new FrameworkElementFactory(typeof(Button));
                FrameworkElementFactory btnClearText = new FrameworkElementFactory(typeof(TextBlock));
                btnClearText.SetValue(TextBlock.TextProperty, "清除");
                btnClearText.SetValue(TextBlock.NameProperty, "btnText");
                btnClear.AppendChild(btnClearText);
                btnClear.SetValue(Button.StyleProperty, this.FindResource("btnListCell") as Style);
                //btnClear.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                btnClear.SetValue(Button.MarginProperty, new Thickness(0, 0, 10, 0));
                btnClear.SetValue(Button.TagProperty, cellInfo);
                btnClear.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Right);
                btnClear.AddHandler(Button.ClickEvent, new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                {
                    //点击清除
                    try
                    {
                        Button btnClear2 = sender as Button;
                        CellInfo cell2 = btnClear2.Tag as CellInfo;
                        TextBox txt2 = this.GetChildObject<TextBox>(this.GetParentObject<Grid>(btnClear2));
                        txt2.Text = "";
                        DataGrid dataGrid2 = this.GetParentObject<DataGrid>(btnClear2);
                        DataRowView row = (dataGrid2.SelectedItem as DataRowView);
                        row[cell2.CellName] = DBNull.Value;
                        row[AppGlobal.DataGridEditStateCellName] = true;
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "点击清除按钮异常");
                    }
                }));
                gridEdit.AppendChild(txt);
                gridEdit.AppendChild(btnUpload);
                gridEdit.AppendChild(btnChoose);
                gridEdit.AppendChild(btnClear);
                dataTemplateEditing.VisualTree = gridEdit;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if (cellInfo.IsArea && valType.Equals("string"))
            {
                #region 地区类型
                binding.Converter = new AppCode.Converters.StringEllipsisConvert();
                binding.ConverterParameter = 20;

                //普通模式
                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                lbl.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetBinding(TextBlock.TextProperty, binding);
                dataTemplate.VisualTree = lbl;

                //编辑模式
                DataTemplate dataTemplateEditing = new DataTemplate();
                FrameworkElementFactory area = new FrameworkElementFactory(typeof(Controls.AreaUC));
                area.SetValue(UserControl.VerticalAlignmentProperty, VerticalAlignment.Center);
                area.SetBinding(Controls.AreaUC.BindTextProperty, binding);
                area.SetValue(TagProperty, cellInfo);
                dataTemplateEditing.VisualTree = area;

                //列属性
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.CellEditingTemplate = dataTemplateEditing;
                templateColumn.Header = cnName;

                //添加到列表
                dataGrid.Columns.Add(templateColumn);
                return;
                #endregion
            }
            else if ((valType.Equals("decimal") || valType.Equals("double") || valType.Equals("float")) && cellInfo.ShowBL)
            {
                #region 显示比例
                bindingEdit.StringFormat = "{0:0.00}%";

                DataTemplate dataTemplate = new DataTemplate();
                FrameworkElementFactory panel = new FrameworkElementFactory(typeof(WrapPanel));
                panel.SetValue(WrapPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

                FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
                lbl.SetValue(TextBlock.CursorProperty, Cursors.Hand);
                lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                lbl.SetValue(TextBlock.TagProperty, binding);
                lbl.SetBinding(TextBlock.TextProperty, bindingEdit);

                FrameworkElementFactory proBar = new FrameworkElementFactory(typeof(ProgressBar));
                proBar.SetValue(ProgressBar.MarginProperty, new Thickness(10, 0, 10, 0));
                proBar.SetValue(ProgressBar.MaximumProperty, 100D);
                proBar.SetBinding(ProgressBar.ValueProperty, binding);
                proBar.SetValue(ProgressBar.WidthProperty, 200D);
                proBar.SetValue(ProgressBar.HeightProperty, 20D);

                panel.AppendChild(proBar);
                panel.AppendChild(lbl);

                dataTemplate.VisualTree = panel;
                templateColumn.CellTemplate = dataTemplate;
                templateColumn.Header = cnName;
                templateColumn.MinWidth = 300;
                dataGrid.Columns.Add(templateColumn);
                #endregion
            }
            else if (valType.Equals("int") || valType.Equals("long") || valType.Equals("double") || valType.Equals("float") || valType.Equals("money") || valType.Equals("decimal"))
            {
                if (cellInfo.DecimalDigits > 0 && (valType.Equals("double") || valType.Equals("money") || valType.Equals("float") || valType.Equals("decimal")))
                {
                    binding.StringFormat = "{0:0.";
                    for (int i = 0; i < cellInfo.DecimalDigits; i++) binding.StringFormat += "#";
                    binding.StringFormat += "}";
                }

                #region 数字类型
                if (cellInfo.CanEdit)
                {
                    //可编辑
                    if (valType.Equals("double") || valType.Equals("float") || valType.Equals("money") || valType.Equals("decimal"))
                    {
                        //修复绑定数字类型 无法输入小数点的BUG
                        binding.Converter = new AppCode.Converters.DecimalConverter();
                    }

                    //普通模式
                    elementStyle = this.FindResource("DataGridCellCenter_Number") as Style;
                    textColumn.Binding = binding;
                    textColumn.ElementStyle = elementStyle;

                    //编辑模式
                    elementEditingStyle = this.FindResource("DataGridCellCenter_Editing") as Style;
                    textColumn.EditingElementStyle = elementEditingStyle;
                    dataGrid.Columns.Add(textColumn);
                }
                else
                {
                    //不可编辑
                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                    lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                    lbl.SetValue(TextBlock.MinWidthProperty, 40D);
                    lbl.SetBinding(TextBlock.TextProperty, binding);
                    dataTemplate.VisualTree = lbl;
                    templateColumn.CellTemplate = dataTemplate;
                    templateColumn.Header = cnName;
                    dataGrid.Columns.Add(templateColumn);
                }
                #endregion
            }
            else
            {
                #region 默认显示
                //默认显示
                if (cellInfo.CanEdit)
                {
                    //可编辑
                    //普通模式
                    elementStyle = this.FindResource("DataGridCellCenter") as Style;
                    textColumn.Binding = binding;
                    textColumn.ElementStyle = elementStyle;
                    //编辑模式
                    elementEditingStyle = this.FindResource("DataGridCellCenter_Editing") as Style;
                    textColumn.EditingElementStyle = elementEditingStyle;
                    dataGrid.Columns.Add(textColumn);
                }
                else
                {
                    //不可编辑
                    DataTemplate dataTemplate = new DataTemplate();
                    FrameworkElementFactory lbl = new FrameworkElementFactory(typeof(TextBlock));
                    lbl.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    lbl.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 10, 0));
                    lbl.SetValue(TextBlock.MinWidthProperty, 40D);
                    lbl.SetValue(TextBlock.NameProperty, "lblShowText");
                    //lbl.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                    lbl.SetBinding(TextBlock.TextProperty, binding);
                    dataTemplate.VisualTree = lbl;
                    templateColumn.CellTemplate = dataTemplate;
                    templateColumn.Header = cnName;
                    dataGrid.Columns.Add(templateColumn);
                }
                #endregion
            }
        }

        /// <summary>
        /// 生成查询控件
        /// </summary>
        /// <param name="cellInfo"></param>
        private bool BuildQueryCtls(CellInfo cellInfo)
        {
            if (this.gridQueryRow.Height.Value <= 0)
            {
                //显示查询行
                this.gridQueryRow.Height = new GridLength(55);
                //InitSize();
            }

            //值类型
            string valType = cellInfo.ValType.ToString();

            WrapPanel panel = new WrapPanel();
            panel.VerticalAlignment = VerticalAlignment.Center;
            panel.Margin = new Thickness(0, 0, 20, 5);
            panel.Width = 300;

            TextBlock lbl = new TextBlock();
            lbl.Text = cellInfo.CnName;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.Foreground = AppGlobal.ColorToBrush("#666");
            lbl.FontSize = 14;
            lbl.Margin = new Thickness(0, 0, 5, 0);
            lbl.Width = 80;
            lbl.TextAlignment = TextAlignment.Right;

            panel.Children.Add(lbl);

            UIElement element = null;

            int ctlHeight = 24;

            //生成查询控件
            if (valType.IndexOf(',') > 0)
            {
                //下拉列表
                string[] vals = ("," + valType).Split(',');
                ComboBox cmb = new ComboBox();
                cmb.ItemsSource = vals;
                cmb.VerticalAlignment = VerticalAlignment.Center;
                cmb.Height = ctlHeight;
                cmb.Width = 210;

                //自动查询
                if (_tableConfig.ZDCX) cmb.SelectionChanged += CmbQuery_SelectionChanged;

                element = cmb;
            }
            else if (valType.Equals("date") || valType.Equals("datetime"))
            {
                //日期范围
                DatePicker dp1 = new DatePicker();
                dp1.VerticalAlignment = VerticalAlignment.Center;
                dp1.Height = ctlHeight;
                dp1.Width = 100;

                DatePicker dp2 = new DatePicker();
                dp2.VerticalAlignment = VerticalAlignment.Center;
                dp2.Height = ctlHeight;
                dp2.Width = 100;

                TextBlock lblTo = new TextBlock();
                lblTo.VerticalAlignment = VerticalAlignment.Center;
                lblTo.Text = " - ";
                lblTo.Width = 10;

                if (cellInfo.CellName.Equals("RQ") || cellInfo.CellName.Equals("MZZB_RQ"))
                {
                    //本月,本季,本年,前一月,前三月,前后一月,前后三月
                    DateTime dt = DateTime.Now;
                    DateTime dtBegin = new DateTime(dt.Year, dt.Month, 1);
                    DateTime dtEnd = dtBegin.AddMonths(1).AddDays(-1);

                    if (!string.IsNullOrWhiteSpace(cellInfo.RQFW))
                    {
                        switch (cellInfo.RQFW)
                        {
                            case "本月": break;
                            case "本季":
                                dtBegin = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day);
                                dtEnd = dtBegin.AddMonths(3).AddDays(-1);
                                break;
                            case "本年":
                                dtBegin = new DateTime(dt.Year, 1, 1);
                                dtEnd = new DateTime(dt.Year, 12, 1).AddMonths(1).AddDays(-1);
                                break;
                            case "前一月":
                                dtBegin = dt.AddMonths(-1);
                                dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
                                dtEnd = new DateTime(dt.Year, dt.Month, 1).AddMonths(1).AddDays(-1);
                                break;
                            case "前三月":
                                dtBegin = dt.AddMonths(-3);
                                dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
                                dtEnd = new DateTime(dt.Year, dt.Month, 1).AddMonths(1).AddDays(-1);
                                break;
                            case "前后一月":
                                dtBegin = dt.AddMonths(-1);
                                dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
                                dtEnd = dt.AddMonths(2);
                                dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, 1).AddDays(-1);
                                break;
                            case "前后三月":
                                dtBegin = dt.AddMonths(-3);
                                dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, 1);
                                dtEnd = dt.AddMonths(4);
                                dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, 1).AddDays(-1);
                                break;
                        }
                    }

                    dp1.SelectedDate = dtBegin;
                    dp2.SelectedDate = dtEnd;
                }

                //自动查询
                if (_tableConfig.ZDCX)
                {
                    dp1.SelectedDateChanged += Dp1Query_SelectedDateChanged;
                    dp2.SelectedDateChanged += Dp2Query_SelectedDateChanged;
                }

                //注册名称
                this.panelQuerys.RegisterName(cellInfo.CellName + "_From", dp1);
                panel.Children.Add(dp1);

                panel.Children.Add(lblTo);

                this.panelQuerys.RegisterName(cellInfo.CellName + "_End", dp2);
                panel.Children.Add(dp2);

                //添加到显示
                if (cellInfo.CellName.Equals("RQ") || cellInfo.CellName.Equals("MZZB_RQ"))
                {
                    //默认放在第一位
                    this.panelQuerys.Children.Insert(0, panel);
                }
                else
                {
                    //正常加载
                    this.panelQuerys.Children.Add(panel);
                }
                //添加到查询的列
                _queryCells.Add(cellInfo);
                return true;
            }
            else if (valType.Equals("bool"))
            {
                //复选框
                CheckBox cb = new CheckBox();
                cb.VerticalAlignment = VerticalAlignment.Center;
                cb.Height = ctlHeight;
                cb.Width = 210;
                cb.IsThreeState = true;
                cb.IsChecked = null;

                //自动查询
                if (_tableConfig.ZDCX)
                {
                    cb.Checked += CbQuery_Checked;
                    cb.Unchecked += CbQuery_Unchecked;
                }

                element = cb;
            }
            else if (cellInfo.IsDropDown && cellInfo.ForeignTableId > 0)
            {
                //下拉外键表
                Controls.DropDownTable ddt = new Controls.DropDownTable();
                ddt.IsQuery = true;
                ddt.Tag = cellInfo;
                ddt.VerticalAlignment = VerticalAlignment.Center;
                ddt.TableId = cellInfo.ForeignTableId;
                ddt.CellInfo = cellInfo;
                ddt.Txt_Change_Event += Ddt_Txt_Change_Event;
                ddt.Height = ctlHeight;
                ddt.ChooseCallBack_Event += DdtQuery_ChooseCallBack_Event;
                ddt.Width = 210;
                ddt.ShowClear = true;

                try
                {
                    if (_PostParams != null && _tableConfig.IsFilterKH && !_tableConfig.IsFilterKHById && (cellInfo.CellName.Equals("KHBH") || cellInfo.CellName.Equals("MZZB_KHBH")))
                    {
                        //传递的参数是否有值
                        object queryVal = GetPostParamValue(cellInfo.CellName);
                        if (queryVal != null)
                        {
                            ddt.Text = queryVal.ToString();
                            ddt.txt.Text = queryVal.ToString();
                        }
                    }
                    else if (_PostParams != null && _tableConfig.IsFilterKH && (cellInfo.CellName.Equals("KHID") || cellInfo.CellName.Equals("MZZB_KHID")))
                    {
                        //传递的参数是否有值
                        object queryVal = GetPostParamValue(cellInfo.CellName);
                        if (queryVal != null)
                        {
                            ddt.Text = queryVal.ToString();
                            ddt.txt.Text = queryVal.ToString();
                        }
                    }
                }
                catch { }

                element = ddt;
            }
            else if (cellInfo.IsArea && valType.Equals("string"))
            {
                //地区
                Controls.AreaUC uc = new Controls.AreaUC();
                element = uc;
            }
            else
            {
                //输入框
                TextBox txt = new TextBox();
                txt.Style = this.FindResource("txt") as Style;
                txt.Height = ctlHeight;
                txt.VerticalAlignment = VerticalAlignment.Center;
                txt.Width = 210;

                try
                {
                    if (_PostParams != null && _tableConfig.IsFilterKH && !_tableConfig.IsFilterKHById && (cellInfo.CellName.Equals("KHBH") || cellInfo.CellName.Equals("MZZB_KHBH")))
                    {
                        //传递的参数是否有值
                        object queryVal = GetPostParamValue(cellInfo.CellName);
                        if (queryVal != null)
                        {
                            txt.Text = queryVal.ToString();
                        }
                    }
                    else if (_PostParams != null && _tableConfig.IsFilterKH && (cellInfo.CellName.Equals("KHID") || cellInfo.CellName.Equals("MZZB_KHID")))
                    {
                        //传递的参数是否有值
                        object queryVal = GetPostParamValue(cellInfo.CellName);
                        if (queryVal != null)
                        {
                            txt.Text = queryVal.ToString();
                        }
                    }
                }
                catch { }

                //自动查询
                if (_tableConfig.ZDCX) txt.TextChanged += TxtQuery_TextChanged;

                element = txt;
            }

            //注册名称
            this.panelQuerys.RegisterName(cellInfo.CellName, element);
            panel.Children.Add(element);

            //添加到显示
            this.panelQuerys.Children.Add(panel);
            //添加到查询的列
            _queryCells.Add(cellInfo);
            return true;
        }

        #region 查询改变事件
        /// <summary>
        /// 查询控件
        /// </summary>
        private object _txtQuerySender = null;
        /// <summary>
        /// 最后自动查询时间
        /// </summary>
        private DateTime _lastAutoQueryDateTime = DateTime.Now;
        /// <summary>
        /// 定时自动查询
        /// </summary>
        System.Timers.Timer _timerAutoQuery = null;
        /// <summary>
        /// 自动查询数据
        /// </summary>
        private void AutoQueryData()
        {
            try
            {
                if (_timerAutoQuery != null)
                {
                    _timerAutoQuery.Close();
                    _timerAutoQuery.Dispose();
                    _timerAutoQuery = null;
                }
            }
            catch (Exception ex) { }

            try
            {
                //默认延时1秒
                double autoQueryDelay = 1;
                autoQueryDelay = AppGlobal.GetSysConfigReturnDouble("System_AutoQueryDelay", autoQueryDelay);

                _timerAutoQuery = new System.Timers.Timer();
                _timerAutoQuery.Interval = autoQueryDelay * 1000;
                _timerAutoQuery.Elapsed += TimerAutoQuery_Elapsed;
                _timerAutoQuery.Start();
            }
            catch (Exception ex) { }

            //最后查询时间
            _lastAutoQueryDateTime = DateTime.Now;

        }
        /// <summary>
        /// 延时1秒后自动查询
        /// 此处作用是避免连续输入时 多次查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerAutoQuery_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerAutoQuery.Close();
            _timerAutoQuery.Dispose();
            _timerAutoQuery = null;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //自动查询不需要定位到数据行
                _isQueryDataFocusTable = false;
                //查询数据
                QueryData();
                return null;
            }));
        }

        /// <summary>
        /// 开始日期改变后更新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dp2Query_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 结束日期改变后更新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dp1Query_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 输入框改变后更新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            //聚焦
            _txtQuerySender = sender;

            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 未选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbQuery_Unchecked(object sender, RoutedEventArgs e)
        {
            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbQuery_Checked(object sender, RoutedEventArgs e)
        {
            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbQuery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //自动查询数据
            AutoQueryData();
        }
        /// <summary>
        /// 关闭选择下拉列表
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="row"></param>
        private void DdtQuery_ChooseCallBack_Event(Controls.DropDownTable uc, DataRow row)
        {
            uc.popup.IsOpen = false;

            //自动查询
            if (_tableConfig.ZDCX)
            {
                //自动查询数据
                AutoQueryData();
            }
        }
        #endregion

        /// <summary>
        /// 得到传递过来的参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private object GetPostParamValue(string key)
        {
            object value = null;

            if (_PostParams.TopRow != null && _PostParams.TopRow.Row.Table.Columns.Contains(key))
            {
                //主表有匹配列
                value = _PostParams.TopRow[key];
            }
            else if (_PostParams.BottomRow != null && _PostParams.BottomRow.Row.Table.Columns.Contains(key))
            {
                //子表有匹配列
                value = _PostParams.BottomRow[key];
            }
            else if (_PostParams.KeyValues.Exists(p => p.Key.Equals(key)))
            {
                //参数有匹配键
                KeyValue kv = _PostParams.KeyValues.Find(p => p.Key.Equals(key));
                value = kv.Value;
            }

            //是否视图
            if (key.StartsWith("MZZB_"))
            {
                string newKey = key.Replace("MZZB_", "");
                return GetPostParamValue(newKey);
            }
            else if (key.StartsWith("MZMX_"))
            {
                string newKey = key.Replace("MZMX_", "");
                return GetPostParamValue(newKey);
            }

            return value;
        }

        /// <summary>
        /// 关闭选择数据回传
        /// </summary>
        /// <param name="win"></param>
        private void Win_CloseWindow_Event(Components.PopWindow win)
        {
            if (win._UIElement is Views.Home.ListUC)
            {
                if ((win._UIElement as Views.Home.ListUC).dataGridTop.SelectedItem == null)
                {
                    (win._TargetObj as Button).Content = "请选择";
                    (win._TargetObj as Button).Tag = null;
                }
            }
        }

        /// <summary>
        /// 选择数据回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinQueryTable_CallBack_Event(Components.PopWindow win, object param)
        {
            //列信息
            CellInfo cellInfo = win._CellInfo;
            if (cellInfo == null) return;

            DataRow row = null;

            //返回类型
            if (param is DataRowView) row = (param as DataRowView).Row;
            else row = param as DataRow;

            if (row == null) return;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    string cellName = cellInfo.CellName;
                    //有设置返回的列名
                    if (!string.IsNullOrWhiteSpace(cellInfo.ReturnCellName)) cellName = cellInfo.ReturnCellName;

                    if (!row.Table.Columns.Contains(cellName))
                    {
                        AppAlert.FormTips(gridMain, "选择的行没有对应列！", FormTipsType.Info);
                        win.Close();
                        return null;
                    }

                    object val = row[cellName];

                    (win._TargetObj as Button).Content = "已选择：" + val.ToString();
                    (win._TargetObj as Button).Tag = val;
                    win.Close();
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "选择数据回传异常");
                }

                return null;
            }));
        }

        #endregion

        #region 数据表事件【DataGrid自带事件】
        /// <summary>
        /// 点击主表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isChooseTop = true;

            this.borderChooseTop.Visibility = Visibility.Visible;
            this.borderChooseBottom.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 点击子表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isChooseTop = false;

            this.borderChooseTop.Visibility = Visibility.Collapsed;
            this.borderChooseBottom.Visibility = Visibility.Visible;

            if (e.ClickCount >= 2)
            {
                //双击子表时 判断是否有三表
                if (_tableConfig.SubTable.ThreeTable != null)
                {
                    //打开三表
                    OpenThreeTable();
                }
            }
        }
        /// <summary>
        /// 主表单元格编辑结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_listAction != ListActionEnum.编辑 && _listAction != ListActionEnum.开单) return;

            string colName = (e.Column as DataGridColumn).SortMemberPath;
            object value = null;

            if (colName.Equals("IsSelected")) return;

            //当前选中单元格
            //DataGridCell currentCell = sender as DataGridCell;
            //currentCell.SetEditFocused(false);

            //当前行
            DataRowView row = e.Row.DataContext as DataRowView;
            Type valType = row[colName].GetType();

            //列配置信息
            CellInfo cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(colName));

            if (e.EditingElement is TextBox)
            {
                TextBox txt = (e.EditingElement as TextBox);
                value = txt.Text;

                if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Users))
                {
                    //用户信息表
                    if (colName.Equals("Password") && !string.IsNullOrWhiteSpace(txt.Text) && !txt.Text.StartsWith("MZEncoding_") && !txt.Text.StartsWith("MZEncrypt_"))
                    {
                        //密码加密
                        value = "MZEncoding_" + EncryptionDES.Encrypt(value.ToString());
                        txt.Text = value.ToString();
                    }
                }
            }
            else if (e.Column is DataGridCheckBoxColumn)
            {
                //筛选框列
                value = (e.EditingElement as CheckBox).IsChecked;
            }
            else if (e.Column is DataGridTemplateColumn)
            {
                //模版列
                value = GetDataGridTemplateColumnValue(dataGridTop, valType, e);
            }
            else if (e.EditingElement is ContentPresenter)
            {

            }

            //重新计算公式
            ReJSZ(_tableConfig, dataGridTop, e.Row, colName, value, row);

            //重新生成拼音简码
            ReSCPYJM(_tableConfig, row);

            try
            {
                //没有值
                if (value == null) return;
                //添加值
                row[colName] = value;

                //标记列已经编辑
                row[AppGlobal.DataGridEditStateCellName] = true;

                //生成拼音
                BuildPY(dataGridTop, _tableConfig.Cells, cellInfo, row, value);
            }
            catch (Exception ex)
            {
                //ErrorHandler.AddException(ex, "生成拼音异常");
            }
        }
        /// <summary>
        /// 明细表单元格编辑结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_listAction != ListActionEnum.编辑 && _listAction != ListActionEnum.添加) return;

            string colName = (e.Column as DataGridColumn).SortMemberPath;
            object value = null;

            if (colName.Equals("IsSelected")) return;

            //当前选中单元格
            //DataGridCell currentCell = sender as DataGridCell;
            //currentCell.SetEditFocused(false);

            //当前行
            DataRowView row = e.Row.DataContext as DataRowView;
            if (row == null) return;

            //值类型
            Type valType = row[colName].GetType();

            //列配置信息
            CellInfo cellInfo = _tableConfig.SubTable.Cells.Find(p => p.CellName.Equals(colName));

            if (e.EditingElement is TextBox)
            {
                TextBox txt = (e.EditingElement as TextBox);
                value = txt.Text;
            }
            else if (e.Column is DataGridCheckBoxColumn)
            {
                //筛选框列
                value = (e.EditingElement as CheckBox).IsChecked;
            }
            else if (e.Column is DataGridTemplateColumn)
            {
                //模版列
                value = GetDataGridTemplateColumnValue(this.dataGridBottom, valType, e);
            }
            else if (e.EditingElement is ContentPresenter)
            {

            }

            //重新计算公式
            ReJSZ(_tableConfig.SubTable, dataGridBottom, e.Row, colName, value, row);

            //重新生成拼音格式
            ReSCPYJM(_tableConfig.SubTable, row);

            try
            {
                //没有值
                if (value == null) return;

                //设置值
                row[colName] = value;

                //标记列已经编辑
                row[AppGlobal.DataGridEditStateCellName] = true;

                //生成拼音
                BuildPY(dataGridBottom, _tableConfig.SubTable.Cells, cellInfo, row, value);
            }
            catch (Exception ex)
            {
                //ErrorHandler.AddException(ex, "生成拼音异常");
            }
        }
        /// <summary>
        /// 重新计算列值
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="gridData"></param>
        /// <param name="e"></param>
        /// <param name="colName"></param>
        /// <param name="value"></param>
        /// <param name="row"></param>
        private void ReJSZ(TableInfo tableInfo, DataGrid gridData, DataGridRow dgrow, string colName, object value, DataRowView row)
        {
            //复制一行
            DataRow newRow = row.Row.Table.NewRow();
            newRow.ItemArray = (object[])row.Row.ItemArray.Clone();

            try
            {
                //赋值
                newRow[colName] = value;
            }
            catch { }

            try
            {
                var cellDJ = tableInfo.Cells.Find(p => p.CellName.Equals("DJ"));
                var cellSL = tableInfo.Cells.Find(p => p.CellName.Equals("SL"));
                var cellJE = tableInfo.Cells.Find(p => p.CellName.Equals("JE"));

                if (cellDJ != null && cellSL != null && cellJE != null)
                {
                    decimal dj = DataType.Decimal(newRow["DJ"], 0);
                    decimal sl = DataType.Decimal(newRow["SL"], 0);

                    row["JE"] = dj * sl;
                    //设置单元格值
                    SetGridCellValue(gridData, dgrow, "JE", row["JE"]);
                }
            }
            catch { }

            //重新计算单元格公式值
            foreach (CellInfo cell in tableInfo.Cells)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(cell.ShowGS))
                    {
                        //计算显示公式
                        var jsgsResult = JSSHOWGS(cell, newRow);
                        row[cell.CellName] = jsgsResult;
                        newRow[cell.CellName] = jsgsResult;
                        //设置单元格值
                        SetGridCellValue(gridData, dgrow, cell.CellName, row[cell.CellName]);
                    }
                    else if (!string.IsNullOrWhiteSpace(cell.Formula))
                    {
                        //计算保存公式
                        var jsgsResult = JSGS(cell, newRow);
                        row[cell.CellName] = jsgsResult;
                        newRow[cell.CellName] = jsgsResult;
                        //设置单元格值
                        SetGridCellValue(gridData, dgrow, cell.CellName, row[cell.CellName]);
                    }

                    //计算公式
                    CellInfo cellJSGS = tableInfo.Cells.Find(p => p.CellName.Equals(cell.CellName + "_JSGS"));
                    if (cellJSGS != null)
                    {
                        var objJSGS = newRow[cell.CellName + "_JSGS"];
                        if (objJSGS == null || string.IsNullOrWhiteSpace(objJSGS.ToString())) continue;

                        //计算公式后的值
                        var jsgsResult = JSGS(cell, objJSGS.ToString(), newRow);
                        row[cell.CellName] = jsgsResult;
                        newRow[cell.CellName] = jsgsResult;
                        //设置单元格值
                        SetGridCellValue(gridData, dgrow, cell.CellName, row[cell.CellName]);
                    }
                }
                catch { }
            }
        }
        /// <summary>
        /// 重新生成拼音简码
        /// </summary>
        private void ReSCPYJM(TableInfo tableInfo, DataRowView row)
        {
            foreach (CellInfo cell in tableInfo.Cells)
            {
                if (!string.IsNullOrWhiteSpace(cell.SCPYJM))
                {
                    //生成拼音简码
                    row[cell.CellName] = GetSCPYJM(cell, tableInfo, row);
                }
            }
        }

        /// <summary>
        /// 设置单元格值
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowContainer"></param>
        /// <param name="cellName"></param>
        /// <param name="value"></param>
        private void SetGridCellValue(DataGrid dataGrid, DataGridRow rowContainer, string cellName, object value)
        {
            //没有值
            if (value == null) return;

            try
            {
                VisualTreePlus plus = new VisualTreePlus();
                DataGridCellsPresenter presenter = DataGridPlus.GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter != null)
                {
                    for (int cIndex = 0; cIndex < dataGrid.Columns.Count; cIndex++)
                    {
                        //操作列名
                        string colName = dataGrid.Columns[cIndex].SortMemberPath;
                        if (string.IsNullOrWhiteSpace(colName) || !colName.Equals(cellName)) continue;

                        //得到单元格
                        DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                        if (cell == null)
                        {
                            dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[cIndex]);
                            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                        }
                        //是否得到单元格
                        if (cell == null) continue;

                        //所有子控件
                        List<DependencyObject> ctlSubs = plus.GetChildren(cell);

                        //赋值
                        foreach (DependencyObject c in ctlSubs)
                        {
                            if (c is Button)
                            {
                                continue;
                            }
                            else if (c is TextBlock)
                            {
                                TextBlock lbl = c as TextBlock;
                                if (lbl.Name.Equals("btnText")) continue;
                                lbl.Text = value.ToString();
                            }
                            else if (c is TextBox)
                            {
                                TextBox txt = c as TextBox;
                                txt.Text = value.ToString();
                            }
                        }

                        //标记无需编辑
                        cell.IsEditing = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "设置单元格值异常");
            }
        }

        /// <summary>
        /// 打开三表
        /// </summary>
        private void OpenThreeTable()
        {
            //没有第三表
            if (_tableConfig.SubTable.ThreeTable == null) return;

            //选择主表的行
            DataRowView rowTop = this.dataGridTop.SelectedItem as DataRowView;
            if (rowTop == null) return;

            //主表数据ID
            long topId = DataType.Long(rowTop["Id"], 0);
            if (topId <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择主表数据行！", FormTipsType.Info);
                return;
            }

            //选择明细表的行
            DataRowView rowBottom = this.dataGridBottom.SelectedItem as DataRowView;
            if (rowBottom == null) return;

            //明细表数据ID
            long parentId = DataType.Long(rowBottom["Id"], 0);
            if (parentId <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择明细数据行！", FormTipsType.Info);
                return;
            }

            //主表选择的行
            DataTable dtZB = rowTop.Row.Table.Clone();
            dtZB.TableName = "EXT_ZB";
            DataRow newTopRow = dtZB.NewRow();
            newTopRow.ItemArray = rowTop.Row.ItemArray;
            dtZB.Rows.Add(newTopRow);

            //从表选择的行
            DataTable dtMX = rowBottom.Row.Table.Clone();
            dtMX.TableName = "EXT_MX";
            DataRow newBottomRow = dtMX.NewRow();
            newBottomRow.ItemArray = rowBottom.Row.ItemArray;
            dtMX.Rows.Add(newBottomRow);

            //选择的行加到数据集
            DataSet dsExtPrintData = new DataSet();
            dsExtPrintData.Tables.Add(dtZB);
            dsExtPrintData.Tables.Add(dtMX);

            //是否审核
            bool isAudit = SelectItemIsAudit(this.dataGridTop);

            //三表
            TableInfo threeTable = _tableConfig.SubTable.ThreeTable;

            //数据列表
            ListUC uc = new ListUC(threeTable.Id, _moduleId, _moduleIds, _moduleName + "扩展", true, parentId, isAudit, dsExtPrintData, _tableConfig.Id);
            uc.Margin = new Thickness(0);
            uc.Padding = new Thickness(0);
            uc._extWheres = new List<Where>()
            {
                new Where("ParentId", parentId)
            };

            //弹出窗口
            Components.PopWindow win = new Components.PopWindow(uc, _moduleName + "扩展");
            uc._ParentWindow = win;
            uc._ParentUC = this;

            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            double width = screenWidth * 0.8;
            double height = screenHeight * 0.8;

            if (width < 1000) width = 1000;
            if (height < 640) height = 640;

            win.Width = width;
            win.Height = height;

            //窗口属性
            win._TargetObj = this;
            win._ShowMaxBtn = true;
            win.ShowDialog();
        }

        /// <summary>
        /// 得到模版列的值
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private object GetDataGridTemplateColumnValue(DataGrid dataGrid, Type valType, DataGridCellEditEndingEventArgs e)
        {
            object value = null;

            DataGridCell cell = DataGridPlus.GetCell(dataGrid, dataGrid.SelectedIndex, e.Column.DisplayIndex);
            ComboBox cmb = DataGridPlus.GetVisualChild<ComboBox>(cell);
            DatePicker dp = DataGridPlus.GetVisualChild<DatePicker>(cell);
            Image img = DataGridPlus.GetVisualChild<Image>(cell);
            TextBox txt = DataGridPlus.GetVisualChild<TextBox>(cell);
            TextBlock lbl = DataGridPlus.GetVisualChild<TextBlock>(cell);
            Controls.DropDownTable ddt = DataGridPlus.GetVisualChild<Controls.DropDownTable>(cell);
            Controls.DropDownTable ddt2 = this.GetChildObject<Controls.DropDownTable>(cell);
            Controls.AreaUC area = DataGridPlus.GetVisualChild<Controls.AreaUC>(cell);

            if (area != null)
            {
                //地区
                value = area.Text;
            }
            else if (cmb != null)
            {
                //下拉列表
                value = cmb.SelectedItem;
            }
            else if (dp != null)
            {
                //日期
                value = dp.SelectedDate;
            }
            else if (img != null)
            {
                //图标
                if (img.Tag != null)
                {
                    value = img.Tag;
                }
            }
            else if (ddt != null)
            {
                if (!ddt.IsChoosed && ddt.IsClear && ddt.IsHoverList)
                {
                    e.Cancel = true;
                    return null;
                }

                if (!ddt.IsChoosed && !ddt.IsClear)
                {
                    //取消
                    e.Cancel = true;
                    return null;
                }
                else if (!ddt.IsChoosed && ddt.IsKeboardSelected)
                {
                    //取消
                    e.Cancel = true;
                    return null;
                }
                else if (ddt.IsLoadingData && ddt.IsHover)
                {
                    //正在加载数据
                    e.Cancel = true;
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(ddt.Text) && ddt.IsClear)
                {
                    value = "";
                }
                else
                {
                    //value = ddt.Text;
                    return null;
                }
            }
            else if (txt != null)
            {
                //值
                value = txt.Text;
            }
            else if (lbl != null)
            {
                //值
                value = lbl.Text;
            }
            else
            {
                //取消
                e.Cancel = true;
                return null;
            }

            //数字默认值
            if ((valType == typeof(int) || valType == typeof(long) ||
                valType == typeof(ushort) || valType == typeof(short) ||
                valType == typeof(uint) || valType == typeof(ulong) ||
                valType == typeof(decimal) || valType == typeof(float) ||
                valType == typeof(double)))
            {
                try
                {
                    //是否有值
                    if (string.IsNullOrWhiteSpace(value.ToString())) value = 0;
                    //类型转换
                    else Convert.ChangeType(value, valType);
                }
                catch
                {
                    //类型转换异常
                    value = 0;
                }
            }

            return value;
        }

        /// <summary>
        /// 生成拼音简码
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="row"></param>
        /// <param name="value"></param>
        private void BuildPY(DataGrid dataGrid, List<CellInfo> cells, CellInfo cellInfo, DataRowView row, object value)
        {
            try
            {
                //拼音简码
                if (cellInfo != null && cellInfo.PYGroup % 2 == 1)
                {
                    CellInfo cellInfo2 = cells.Find(p => p.PYGroup == (cellInfo.PYGroup + 1));
                    if (cellInfo2 != null)
                    {
                        string cn = value.ToString();

                        //默认名称英文
                        //if (cn.Contains("名称")) cn = cn.Replace("名称", "Name");
                        //else if (cn.Contains("编号")) cn = cn.Replace("编号", "Id");

                        //拼音
                        string pinying = Core.Handler.ChineseHandler.GetFirstPYLetters(cn);

                        //单元格值
                        row[cellInfo2.CellName] = pinying;

                        //找到对应单元格
                        DataGridCell cell2 = dataGrid.GetCell(dataGrid.SelectedIndex, cellInfo2.CellName);

                        if (cell2 != null)
                        {
                            //查找文本控件
                            TextBlock lbl = GetChildObject<TextBlock>(cell2);
                            TextBox txt = GetChildObject<TextBox>(cell2);

                            //是否是TextBlock
                            if (lbl != null) lbl.Text = pinying;
                            //是否是TextBox
                            if (txt != null) txt.Text = pinying;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "生成拼音简码异常");
            }

            try
            {
                //有生成拼音格式的
                List<CellInfo> cellGS = cells.Where(p => !string.IsNullOrWhiteSpace(p.PYGS)).ToList();
                if (cellGS != null && cellGS.Count > 0)
                {
                    string pattern = @"\[[^\]]*\]";

                    foreach (CellInfo cell in cellGS)
                    {
                        //格式：[SPMC][DW][YS][GG]
                        string pygs = cell.PYGS;
                        MatchCollection mc = Regex.Matches(pygs, pattern);
                        if (mc != null && mc.Count > 0)
                        {
                            foreach (Match m in mc)
                            {
                                string cellName = m.Value.Trim('[').Trim(']');
                                if (row.Row.Table.Columns.Contains(cellName))
                                {
                                    //内容
                                    string nr = row[cellName].ToString();
                                    //拼音
                                    string pinying = Core.Handler.ChineseHandler.GetFirstPYLetters(nr);
                                    //替换值
                                    pygs = pygs.Replace("[" + cellName + "]", pinying);
                                }
                            }

                            //单元格值
                            row[cell.CellName] = pygs;

                            //找到对应单元格
                            DataGridCell cell2 = dataGrid.GetCell(dataGrid.SelectedIndex, cell.CellName);

                            if (cell2 != null)
                            {
                                //查找文本控件
                                TextBlock lbl = GetChildObject<TextBlock>(cell2);
                                TextBox txt = GetChildObject<TextBox>(cell2);

                                //是否是TextBlock
                                if (lbl != null) lbl.Text = pygs;
                                //是否是TextBox
                                if (txt != null) txt.Text = pygs;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 主表列索引变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            //退出时更新
            _columnSizeOrOrderChange = true;
            return;
        }

        /// <summary>
        /// 明细表列索引变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            //退出时更新
            _columnSizeOrOrderChange = true;
            return;
        }

        /// <summary>
        /// 主表列的宽度变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_ColumnHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //退出时更新
            _columnSizeOrOrderChange = true;
            return;
        }

        /// <summary>
        /// 明细列的宽度变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_ColumnHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //退出时更新
            _columnSizeOrOrderChange = true;
            return;
        }


        /// <summary>
        /// 主表添加新项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            try
            {
                DataRowView view = (e.NewItem as DataRowView);

                SetDefaultValue(view, _tableConfig);

                if (_isPopWindow && _isExtListWindow) view["ParentId"] = _extListParentId;

                view["Id"] = -DateTime.Now.ToFileTime();
                view[AppGlobal.DataGridNewStateCellName] = true;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "主表添加新项异常");
            }
        }

        /// <summary>
        /// 明细表添加新项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            try
            {
                DataRowView view = (e.NewItem as DataRowView);

                //设置默认值
                SetDefaultValue(view, _tableConfig.SubTable);

                view["Id"] = -DateTime.Now.ToFileTime();
                view["ParentId"] = _chooseTopId;
                view[AppGlobal.DataGridNewStateCellName] = true;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "子表添加新项异常");
            }
        }

        /// <summary>
        /// 主表加载行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1 + _topTableIndex;
            //e.Row.Background = Brushes.Transparent;
            DataRowView rowView = e.Row.Item as DataRowView;

            try
            {
                //总金额负数时 显示 的背景色
                string bgColor = AppGlobal.GetSysConfigReturnString("System_UI_NegativeBackground");
                if (!string.IsNullOrWhiteSpace(bgColor))
                {
                    decimal zje = 0;
                    if (_tableConfig.Cells.Exists(p => p.CellName.Equals("ZJE")))
                    {
                        zje = DataType.Decimal(rowView["ZJE"], 0);
                    }
                    if (_tableConfig.Cells.Exists(p => p.CellName.Equals("MZZB_ZJE")))
                    {
                        zje = DataType.Decimal(rowView["MZZB_ZJE"], 0);
                    }

                    if (zje < 0)
                    {
                        //背景色
                        e.Row.Background = AppGlobal.HtmlColorToBrush(bgColor);
                        //e.Row.HeaderStyle = this.FindResource("DataGridRowHeaderRed") as Style;
                    }
                }
            }
            catch { }

            //行背景颜色配置
            try
            {
                string rowColorFormat = _tableConfig.RowColorFormat;
                if (!string.IsNullOrWhiteSpace(rowColorFormat))
                {
                    string[] rowColorFormats = rowColorFormat.Split(';');
                    foreach (string format in rowColorFormats)
                    {
                        if (string.IsNullOrWhiteSpace(format)) continue;
                        if (format.IndexOf(',') <= 0) continue;

                        string gs = format.Split(',')[0];
                        string color = format.Split(',')[1];

                        if (string.IsNullOrWhiteSpace(gs) || string.IsNullOrWhiteSpace(color)) continue;

                        //计算公式-列
                        Regex regex = new Regex(@"\[[^\]]+?\]");
                        //匹配
                        MatchCollection mc = regex.Matches(gs);
                        if (mc != null && mc.Count > 0)
                        {
                            foreach (Match m in mc)
                            {
                                //列名
                                string cellName = m.Value.Trim().Trim('[').Trim(']');
                                //是否存在列
                                if (_tableConfig.Cells.Exists(p => p.CellName.Equals(cellName)))
                                {
                                    gs = gs.Replace("[" + cellName + "]", rowView[cellName].ToString());
                                }
                            }
                        }

                        try
                        {
                            //计算公式结果
                            var jsjg = new NCalc.Expression(gs).Evaluate();
                            if (jsjg != null && DataType.Bool(jsjg, false))
                            {
                                //设置背景色
                                e.Row.Background = AppGlobal.HtmlColorToBrush(color);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "根据主表行颜色公式设置行背景色异常");
            }
        }
        /// <summary>
        /// 明细表加载行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1 + _bottomTableIndex;
            //e.Row.Background = Brushes.Transparent;
            DataRowView rowView = e.Row.Item as DataRowView;

            //行背景颜色配置
            try
            {
                string rowColorFormat = _tableConfig.SubTable.RowColorFormat;
                if (!string.IsNullOrWhiteSpace(rowColorFormat))
                {
                    string[] rowColorFormats = rowColorFormat.Split(';');
                    foreach (string format in rowColorFormats)
                    {
                        if (string.IsNullOrWhiteSpace(format)) continue;
                        if (format.IndexOf(',') <= 0) continue;

                        string gs = format.Split(',')[0];
                        string color = format.Split(',')[1];

                        if (string.IsNullOrWhiteSpace(gs) || string.IsNullOrWhiteSpace(color)) continue;

                        //计算公式-列
                        Regex regex = new Regex(@"\[[^\]]+?\]");
                        //匹配
                        MatchCollection mc = regex.Matches(gs);
                        if (mc != null && mc.Count > 0)
                        {
                            foreach (Match m in mc)
                            {
                                //列名
                                string cellName = m.Value.Trim().Trim('[').Trim(']');
                                //是否存在列
                                if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals(cellName)))
                                {
                                    gs = gs.Replace("[" + cellName + "]", rowView[cellName].ToString());
                                }
                            }
                        }

                        try
                        {
                            //计算公式结果
                            var jsjg = new NCalc.Expression(gs).Evaluate();
                            if (jsjg != null && DataType.Bool(jsjg, false))
                            {
                                //设置背景色
                                e.Row.Background = AppGlobal.HtmlColorToBrush(color);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "根据从表行颜色公式设置行背景色异常");
            }
        }
        /// <summary>
        /// 表选择单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            int currentRowIndex = dataGrid.SelectedIndex;
            int currentCellIndex = dataGrid.CurrentCell.Column.DisplayIndex;

            //当前选中单元格
            DataGridCell currentCell = dataGrid.GetCell(currentRowIndex, currentCellIndex);

            if (!currentCell.IsEditing) return;

            if (currentCell.Column is DataGridTextColumn)
            {
                //文本框列
                TextBox txt = this.GetChildObject<TextBox>(currentCell);
                if (txt != null)
                {
                    txt.Focus();
                    txt.SelectAll();
                }
            }
            else if (currentCell.Column is DataGridCheckBoxColumn)
            {
                //复选框列
                CheckBox cb = this.GetChildObject<CheckBox>(currentCell);
                if (cb.IsChecked.HasValue && cb.IsChecked.Value)
                {
                    cb.IsChecked = false;
                }
                else
                {
                    cb.IsChecked = true;
                }
            }
            else if (currentCell.Column is DataGridTemplateColumn)
            {
                //模版列
                ComboBox cmb = this.GetChildObject<ComboBox>(currentCell);
                DatePicker dp = this.GetChildObject<DatePicker>(currentCell);
                TextBox txt = this.GetChildObject<TextBox>(currentCell);
                Controls.DropDownTable ddt = this.GetChildObject<Controls.DropDownTable>(currentCell);
                Controls.AreaUC area = this.GetChildObject<Controls.AreaUC>(currentCell);

                if (cmb != null)
                {
                    cmb.Focus();
                    return;
                }
                else if (dp != null)
                {
                    dp.Focus();
                    return;
                }
                else if (ddt != null)
                {
                    ddt.txt.Focus();
                    ddt.txt.SelectAll();
                    return;
                }
                else if (txt != null)
                {
                    txt.Focus();
                    txt.SelectAll();
                    return;
                }
                else if (area != null)
                {
                    area.ddlProvince.Focus();
                }
            }
        }
        /// <summary>
        /// 主表选择行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //下拉数据表的事件
            try
            {
                if (e.OriginalSource != null)
                {
                    //是否为下拉数据表的事件
                    string originalSourceName = ((System.Windows.FrameworkElement)e.OriginalSource).Name;
                    if (originalSourceName.Equals("lvDropDownTable")) return;
                }
            }
            catch { }

            //主表焦点
            _isChooseTop = true;

            this.borderChooseTop.Visibility = Visibility.Visible;
            this.borderChooseBottom.Visibility = Visibility.Collapsed;

            //得到主表行
            DataRowView row = (this.dataGridTop.SelectedItem as DataRowView);
            if (row == null)
            {
                //清空子表数据
                this.dataGridBottom.ItemsSource = null;

                //主表焦点
                _isChooseTop = true;

                this.borderChooseTop.Visibility = Visibility.Visible;
                this.borderChooseBottom.Visibility = Visibility.Collapsed;

                return;
            }

            //没有编号列
            if (row.Row.Table.Columns.Contains("Id"))
            {
                //主表记录编号
                long id = DataType.Long(row["Id"].ToString(), 0);

                if (id > 0)
                {
                    //显示按钮
                    DisableListBtns(SelectItemIsAudit(row));

                    //选择主表项
                    _chooseTopId = id;
                }

                //加载明细数据
                _bottomSearchKeywords = string.Empty;
                this.txtSeachBottom.Text = "";
                LoadDetails(id);
            }

            //预览图片
            PreviewPicture(_tableConfig, row);
        }

        /// <summary>
        /// 主表排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_Sorting(object sender, DataGridSortingEventArgs e)
        {
            //排序条件
            System.ComponentModel.SortDescriptionCollection sorts = this.dataGridTop.Items.SortDescriptions;
            string dtOrderBy = string.Empty;

            //是否有排序条件
            if (sorts != null && sorts.Count > 0)
            {
                _topOrderBys = new List<OrderBy>();

                foreach (System.ComponentModel.SortDescription sd in sorts)
                {
                    OrderType ot = OrderType.顺序;
                    if (sd.Direction == System.ComponentModel.ListSortDirection.Descending) ot = OrderType.倒序;

                    _topOrderBys.Add(new OrderBy(sd.PropertyName, ot));

                    //表排序
                    dtOrderBy = sd.PropertyName + " " + (sd.Direction == System.ComponentModel.ListSortDirection.Ascending ? " ASC" : " DESC") + ",";
                }

                _topOrderBysSql = dtOrderBy.Trim(',');
            }
        }

        /// <summary>
        /// 子表排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_Sorting(object sender, DataGridSortingEventArgs e)
        {
            //排序条件
            System.ComponentModel.SortDescriptionCollection sorts = this.dataGridBottom.Items.SortDescriptions;
            string dtOrderBy = string.Empty;

            //是否有排序条件
            if (sorts != null && sorts.Count > 0)
            {
                _bottomOrderBys = new List<OrderBy>();

                foreach (System.ComponentModel.SortDescription sd in sorts)
                {
                    OrderType ot = OrderType.顺序;
                    if (sd.Direction == System.ComponentModel.ListSortDirection.Descending) ot = OrderType.倒序;

                    _bottomOrderBys.Add(new OrderBy(sd.PropertyName, ot));

                    //表排序
                    dtOrderBy = sd.PropertyName + " " + (sd.Direction == System.ComponentModel.ListSortDirection.Ascending ? " ASC" : " DESC") + ",";
                }

                _bottomOrderBysSql = dtOrderBy.Trim(',');
            }
        }

        /// <summary>
        /// 选择明细表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //明细表焦点
            _isChooseTop = false;

            this.borderChooseTop.Visibility = Visibility.Collapsed;
            this.borderChooseBottom.Visibility = Visibility.Visible;

            //得到当前选中行
            DataRowView row = (this.dataGridBottom.SelectedItem as DataRowView);
            if (row == null) return;

            //预览图片
            PreviewPicture(_tableConfig.SubTable, row);
        }
        private string _prevPreviewPicture = string.Empty;
        /// <summary>
        /// 预览图片
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="row"></param>
        private void PreviewPicture(TableInfo tableInfo, DataRowView row)
        {
            //是否预览图片
            if (!tableInfo.YLTP) return;

            //是否有图片列
            CellInfo cell = tableInfo.Cells.Find(p => p.IsPic);
            if (cell == null) return;

            //是否有图片
            string pic = row[cell.CellName].ToString();
            if (string.IsNullOrWhiteSpace(pic))
            {
                //隐藏图片
                this.imgTopPreview.Visibility = Visibility.Collapsed;
                this.imgBottomPreview.Visibility = Visibility.Collapsed;

                //清除原图片
                this.imgTopPreview.Source = null;
                this.imgBottomPreview.Source = null;
                return;
            }

            //是否和目前显示的图片相同
            if (pic.Equals(_prevPreviewPicture)) return;
            else
            {
                //清除原图片
                this.imgTopPreview.Source = null;
                this.imgBottomPreview.Source = null;
            }

            //当前预览图片 防止重复下载
            _prevPreviewPicture = pic;
            //回收垃圾
            AppGlobal.GCCollect();

            double tag = DataType.Double(this.imgTopPreview.Tag, 0);
            if (tag < 50)
            {
                double previewSize = AppGlobal.GetSysConfigReturnDouble("System_UI_ImagePreviewSize", 100);
                if (previewSize < 50) previewSize = 50;
                this.imgTopPreview.Tag = previewSize;
                this.imgBottomPreview.Tag = previewSize;

                this.imgTopPreview.Width = previewSize;
                this.imgTopPreview.Height = previewSize;

                this.imgBottomPreview.Width = previewSize;
                this.imgBottomPreview.Height = previewSize;
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + pic;

            if (_isChooseTop)
            {
                this.imgTopPreview.Source = AppGlobal.GetImageSource(path);
                this.imgTopPreview.Visibility = Visibility.Visible;
            }
            else
            {
                this.imgBottomPreview.Source = AppGlobal.GetImageSource(path);
                this.imgBottomPreview.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 弹窗双击行 双击返回行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //不是弹窗选择
            if (!_isChooseCallbackWindow) return;
            //操作时无法返回
            if (_listAction != ListActionEnum.Null) return;

            try
            {
                Point aP = e.GetPosition(dataGridTop);
                IInputElement obj = dataGridTop.InputHitTest(aP);
                DependencyObject target = obj as DependencyObject;

                while (target != null)
                {
                    if (target is DataGridRow)
                    {
                        break;
                    }
                    target = VisualTreeHelper.GetParent(target);
                }

                if (target != null)
                {
                    int rowIndex = (target as DataGridRow).GetIndex();
                    DataRowView row = this.dataGridTop.Items[rowIndex] as DataRowView;

                    //同一行回传是否提示
                    bool needTips = AppGlobal.GetSysConfigReturnBool("System_SameRowReturnNeedTip", true);
                    if (needTips && _returnRows.Contains(row.Row))
                    {
                        bool? flagReturn = AppAlert.Alert("此行已经回传，是否确定再次回传？", "确定再次回传？", AlertWindowButton.OkCancel);
                        if (flagReturn.HasValue && !flagReturn.Value) return;
                    }

                    //回传行
                    ReturnRow(row.Row);

                    try
                    {
                        //引用移除
                        if (_tableConfig.YYHYC || AppGlobal.GetSysConfigReturnBool("System_RemoveReturnRow"))
                        {
                            //移除行
                            (this.dataGridTop.ItemsSource as DataView).Table.Rows.Remove(row.Row);
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "双击返回行异常");
            }
        }
        /// <summary>
        /// 回传行
        /// </summary>
        /// <param name="returnRow"></param>
        private void ReturnRow(DataRow returnRow)
        {
            //添加到已经回传
            _returnRows.Add(returnRow);

            string newColumnName = null;

            if (_tableConfig.TableSubType == TableSubType.商品表)
            {
                //返回增加列 SPID
                newColumnName = "SPID";
            }
            else if (_tableConfig.TableSubType == TableSubType.客户表 ||
               _tableConfig.TableSubType == TableSubType.供应商表)
            {
                //返回增加列 KHID
                newColumnName = "KHID";
            }

            //返回增加列
            if (!string.IsNullOrWhiteSpace(newColumnName))
            {
                //如果不存在列，则添加列
                if (!returnRow.Table.Columns.Contains(newColumnName))
                {
                    //添加返回指定列名
                    returnRow.Table.Columns.Add(new DataColumn(newColumnName, typeof(long)));
                }

                //添加的列赋值
                returnRow[newColumnName] = returnRow["Id"];
            }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                (_ParentWindow as Components.PopWindow).CallBack(returnRow);
                AppAlert.FormTips(gridMain, "已加" + (_ParentWindow as Components.PopWindow).CallBackRowCount + "行！", FormTipsType.Info, true, 1);
                return null;
            }));
        }

        /// <summary>
        /// 主表添加/修改行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                //是否有行
                DataRowView row = e.Row.Item as DataRowView;
                if (row == null) return;

                //标记
                if (row.IsEdit) row[AppGlobal.DataGridEditStateCellName] = row.IsEdit;
                if (row.IsNew) row[AppGlobal.DataGridNewStateCellName] = row.IsNew;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "主表添加/修改行异常");
            }
        }
        /// <summary>
        /// 明细表增加/编辑行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            try
            {
                //是否有行
                DataRowView row = e.Row.Item as DataRowView;
                if (row == null) return;

                //标记
                if (row.IsEdit) row[AppGlobal.DataGridEditStateCellName] = row.IsEdit;
                if (row.IsNew) row[AppGlobal.DataGridNewStateCellName] = row.IsNew;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "子表添加/修改行异常");
            }
        }

        /// <summary>
        /// 主表按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridTop_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //数据表热键
            DataGridHotKey(this.dataGridTop, e);
        }
        /// <summary>
        /// 子表按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridBottom_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //数据表热键
            DataGridHotKey(this.dataGridBottom, e);
        }
        /// <summary>
        /// 数据表热键
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="e"></param>
        private void DataGridHotKey(DataGrid dataGrid, KeyEventArgs e)
        {
            if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Column == null) return;

            int maxRowCount = dataGrid.Items.Count - 1;
            int maxCellCount = dataGrid.Columns.Count - 1;

            int currentRowIndex = dataGrid.SelectedIndex;
            int currentCellIndex = dataGrid.CurrentCell.Column.DisplayIndex;

            //当前选中单元格
            DataGridCell currentCell = dataGrid.GetCell(currentRowIndex, currentCellIndex);
            //if (currentCell == null) return;

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.A))
            {
                e.Handled = true;
            }

            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                (Keyboard.IsKeyDown(Key.Up) || Keyboard.IsKeyDown(Key.Down) || Keyboard.IsKeyDown(Key.Left) || Keyboard.IsKeyDown(Key.Right)))
            {
                e.Handled = true;
            }

            if (_listAction == ListActionEnum.开单 || _listAction == ListActionEnum.添加 || _listAction == ListActionEnum.编辑)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
                {
                    //AppAlert.FormTips(gridMain, e.Key.ToString(), FormTipsType.Info);

                    //上下左右移动
                    if (e.Key == Key.Up || e.Key == Key.Down)
                    {
                        //下拉列表直接为焦点
                        if (currentCell != null && currentCell.Column is DataGridTemplateColumn)
                        {
                            ComboBox cmb = this.GetChildObject<ComboBox>(currentCell);
                            Controls.DropDownTable ddt = this.GetChildObject<Controls.DropDownTable>(currentCell);
                            if (cmb != null)
                            {
                                if (!cmb.IsFocused) cmb.Focus();
                                return;
                            }
                            else if (ddt != null)
                            {
                                //if (!ddt.listView.IsFocused) ddt.listView.Focus();
                                //e.Handled = true;
                                return;
                            }
                        }
                    }
                    else if (e.Key == Key.Left || e.Key == Key.Right)
                    {
                        if (currentCell != null && currentCell.Column is DataGridTemplateColumn)
                        {
                            Controls.DropDownTable ddt = this.GetChildObject<Controls.DropDownTable>(currentCell);
                            if (ddt != null) currentCell.Focus();
                        }
                    }

                    //上下左右移动单元格焦点
                    dataGrid.CommitEdit();
                    return;
                }
                else if (e.Key == Key.Enter)
                {
                    //当前编辑行
                    if (currentCell != null && currentCell.IsEditing)
                    {
                        try
                        {
                            int cCount = dataGrid.Columns.Count();

                            if (currentCellIndex < cCount - 1)
                            {
                                DataGridCell dgc = dataGrid.GetCell(currentRowIndex, currentCellIndex + 1);
                                dgc.Focus();
                            }
                            else if (currentCellIndex > 0)
                            {
                                DataGridCell dgc = dataGrid.GetCell(currentRowIndex, currentCellIndex - 1);
                                dgc.Focus();
                            }
                        }
                        catch { }
                    }

                    if (_isChooseTop)
                    {
                        //最后一行才添加行
                        if (this.dataGridTop.SelectedIndex == this.dataGridTop.Items.Count - 1)
                        {
                            TopAddNewRow();
                        }
                    }
                    else
                    {
                        //最后一行才添加行
                        if (this.dataGridBottom.SelectedIndex == this.dataGridBottom.Items.Count - 1)
                        {
                            BottomAddNewRow();
                        }
                    }
                }
                else if (e.Key == Key.Execute)
                {
                    //设置单元格编辑
                    SetCellEdit(dataGrid, currentCell, e);
                }
            }
        }

        /// <summary>
        /// 主表单元格点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopDataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //单元格
            DataGridCell dataGridCell = sender as DataGridCell;
            if (dataGridCell == null) return;

            //列名
            string cellName = dataGridCell.Column.SortMemberPath;

            try
            {
                //选择主表单元格
                if (dataGridCell.Content is TextBlock)
                {
                    _topSelectCellText = ((TextBlock)dataGridCell.Content).Text;
                }
                else
                {
                    _topSelectCellText = (this.dataGridTop.SelectedItem as DataRowView)[cellName].ToString();
                }
            }
            catch { }

            //表
            if (this.dataGridTop.IsReadOnly) return;

            if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables) && cellName.Equals("ParentId"))
            {
                //表维护的 ParentId 可编辑
            }
            else
            {
                if (AppGlobal.List_CannotEditCells.Contains(cellName.ToUpper()))
                {
                    //不可编辑
                    dataGridCell.IsEditing = false;
                    return;
                }

                //列信息
                CellInfo cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(cellName));
                if (cellInfo == null) return;

                if (!cellInfo.CanEdit)
                {
                    //不可编辑
                    dataGridCell.IsEditing = false;
                }
            }
        }
        /// <summary>
        /// 子表单元格点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomDataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //单元格
            DataGridCell dataGridCell = sender as DataGridCell;
            if (dataGridCell == null) return;

            //列名
            string cellName = dataGridCell.Column.SortMemberPath;

            try
            {
                //选择子表单元格
                if (dataGridCell.Content is TextBlock)
                {
                    _bottomSelectCellText = ((TextBlock)dataGridCell.Content).Text;
                }
                else
                {
                    _bottomSelectCellText = (this.dataGridBottom.SelectedItem as DataRowView)[cellName].ToString();
                }
            }
            catch { }

            //表
            if (this.dataGridBottom.IsReadOnly) return;
            if (_tableConfig.TableName == "Sys_Areas" && cellName == "ParentId") return;

            if (AppGlobal.List_CannotEditCells.Contains(cellName.ToUpper()))
            {
                //不可编辑
                dataGridCell.IsEditing = false;
                return;
            }

            //列信息
            CellInfo cellInfo = _tableConfig.SubTable.Cells.Find(p => p.CellName.Equals(cellName));
            if (cellInfo == null) return;

            if (!cellInfo.CanEdit)
            {
                //不可编辑
                dataGridCell.IsEditing = false;
            }
        }

        /// <summary>
        /// 主表单元格聚焦
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopDataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //当前是否编辑模式
                if (_listAction != ListActionEnum.开单 && _listAction != ListActionEnum.添加 && _listAction != ListActionEnum.编辑) return;

                //当前选中单元格
                DataGridCell currentCell = sender as DataGridCell;

                //设置单元格编辑
                SetCellEdit(this.dataGridTop, currentCell, null);
            }
            catch { }
        }
        /// <summary>
        /// 子表单元格聚焦
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomDataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //当前是否编辑模式
                if (_listAction != ListActionEnum.开单 && _listAction != ListActionEnum.添加 && _listAction != ListActionEnum.编辑) return;

                //当前选中单元格
                DataGridCell currentCell = sender as DataGridCell;

                //设置单元格编辑
                SetCellEdit(this.dataGridBottom, currentCell, null);
            }
            catch { }
        }

        /// <summary>
        /// 设置单元格编辑
        /// </summary>
        private void SetCellEdit(DataGrid dataGrid, DataGridCell currentCell, KeyEventArgs e)
        {
            if (currentCell != null && currentCell.IsEditing)
            {
                if (!currentCell.GetEditFocused()) return;
                currentCell.SetEditFocused(false);

                if (currentCell.Column is DataGridTextColumn)
                {
                    //文本框列
                    TextBox txt = this.GetChildObject<TextBox>(currentCell);
                    if (txt != null && !txt.IsFocused)
                    {
                        txt.Focus();
                        txt.SelectAll();
                        if (e != null) e.Handled = true;
                    }
                }
                else if (currentCell.Column is DataGridCheckBoxColumn)
                {
                    //复选框列
                    CheckBox cb = this.GetChildObject<CheckBox>(currentCell);
                    if (cb != null) cb.Focus();
                }
                else if (currentCell.Column is DataGridTemplateColumn)
                {
                    //模版列
                    ComboBox cmb = this.GetChildObject<ComboBox>(currentCell);
                    DatePicker dp = this.GetChildObject<DatePicker>(currentCell);
                    Button btn = this.GetChildObject<Button>(currentCell);
                    TextBox txt = this.GetChildObject<TextBox>(currentCell);
                    Controls.DropDownTable ddt = this.GetChildObject<Controls.DropDownTable>(currentCell);
                    Controls.AreaUC area = this.GetChildObject<Controls.AreaUC>(currentCell);

                    if (cmb != null)
                    {
                        if (!cmb.IsFocused)
                        {
                            cmb.Focus();
                            if (e != null) e.Handled = true;
                        }
                    }
                    else if (dp != null)
                    {
                        if (!dp.IsFocused)
                        {
                            dp.Focus();
                            return;
                        }
                    }
                    else if (ddt != null)
                    {
                        if (!ddt.IsFocused)
                        {
                            ddt.txt.Focus();
                            ddt.txt.SelectAll();
                            if (e != null) e.Handled = true;
                        }
                        return;
                    }
                    else if (btn != null)
                    {
                        if (!btn.IsFocused)
                        {
                            btn.Focus();
                            if (e != null) e.Handled = true;
                        }
                        return;
                    }
                    else if (area != null)
                    {
                        if (!area.IsFocused)
                        {
                            area.ddlProvince.Focus();
                        }
                    }
                }

                currentCell.SetEditFocused(false);
            }
            else
            {
                if (currentCell != null)
                {
                    //当前单元格是否可编辑
                    bool canEdit = CanEditCell(dataGrid, currentCell);
                    if (canEdit)
                    {
                        //进入编辑模式
                        currentCell.IsEditing = true;
                        currentCell.SetEditFocused(true);

                        //模版列单元格
                        if (currentCell.Column is DataGridTemplateColumn) return;
                        //复选框单元格
                        if (currentCell.Column is DataGridCheckBoxColumn) return;

                        //模拟按执行键
                        keybd_event(System.Windows.Forms.Keys.Execute, 0, 0, 0);
                        keybd_event(System.Windows.Forms.Keys.Execute, 0, 2, 0);
                    }
                }
            }
        }
        /// <summary>
        /// 设置单元格编辑时的焦点
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="currentCell"></param>
        private void SetCellEdited_Focus(DataGrid dataGrid, DataGridCell currentCell, RoutedEventArgs e)
        {
            if (currentCell.Column is DataGridTextColumn)
            {
                //文本框列
                TextBox txt = this.GetChildObject<TextBox>(currentCell);
                if (txt != null)
                {
                    e.Handled = true;
                    txt.Focus();
                    txt.SelectAll();
                }
            }
            else if (currentCell.Column is DataGridCheckBoxColumn)
            {
                //复选框列
            }
            else if (currentCell.Column is DataGridTemplateColumn)
            {
                //模版列
                ComboBox cmb = this.GetChildObject<ComboBox>(currentCell);
                DatePicker dp = this.GetChildObject<DatePicker>(currentCell);
                TextBox txt = this.GetChildObject<TextBox>(currentCell);
                Controls.DropDownTable ddt = this.GetChildObject<Controls.DropDownTable>(currentCell);
                Controls.AreaUC area = this.GetChildObject<Controls.AreaUC>(currentCell);

                if (cmb != null)
                {
                    //if (cmb.IsFocused) return;
                    //e.Handled = true;
                    //cmb.Focus();
                }
                else if (dp != null)
                {
                    //if (dp.IsFocused) return;
                    //e.Handled = true;
                    //dp.Focus();
                }
                else if (ddt != null)
                {
                    //if (ddt.txt.IsFocused) return;
                    //e.Handled = true;
                    //ddt.txt.Focus();
                    //ddt.txt.SelectAll();
                }
                else if (txt != null)
                {
                    if (txt.IsFocused) return;
                    e.Handled = true;
                    txt.Focus();
                    txt.SelectAll();
                }
                else if (area != null)
                {
                    if (area.ddlProvince.IsFocused || area.ddlCity.IsFocused) return;
                    e.Handled = true;
                    area.ddlProvince.Focus();
                }
            }
        }
        /// <summary>
        /// 列是否可以编辑
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private bool CanEditCell(DataGrid dataGrid, DataGridCell cell)
        {
            string[] filterEditCells = { "IsSelected", "Id", AppGlobal.DataGridEditStateCellName, AppGlobal.DataGridNewStateCellName, "CreateDate" };
            string cellName = cell.Column.SortMemberPath;
            if (filterEditCells.Contains(cellName)) return false;

            //列信息
            CellInfo cellInfo = null;

            //列是否禁用编辑
            if (dataGrid == this.dataGridTop)
            {
                cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(cellName));
            }
            else if (dataGrid == this.dataGridBottom)
            {
                cellInfo = _tableConfig.SubTable.Cells.Find(p => p.CellName.Equals(cellName));
            }

            //是否找到列配置
            if (cellInfo == null) return false;

            //是否可编辑
            return cellInfo.CanEdit;
        }
        /// <summary>
        /// 键盘左右移动
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="e"></param>
        public void MoveLeftRight(UIElement uc, KeyEventArgs e)
        {
            DataGridCell cell = FindVisualParent<DataGridCell>(uc);
            cell.Focus();
        }
        #endregion

        #region 数据表列控件事件【单元格内控件事件】

        /// <summary>
        /// 选择图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnChooseIcon_Event(object sender, RoutedEventArgs e)
        {
            Button btn = (sender as Button);
            string cellName = btn.Tag.ToString();

            DataRowView row = null;

            //更新表
            if (_isChooseTop)
            {
                row = this.dataGridTop.SelectedItem as DataRowView;
            }
            else
            {
                row = this.dataGridBottom.SelectedItem as DataRowView;
            }

            //值
            string value = row[cellName].ToString();

            //图标列表
            Views.Parts.ChooseFontIconWin uc = new Parts.ChooseFontIconWin(value);
            //弹出窗口
            Views.Components.PopWindow win = new Components.PopWindow(uc, "选择图标");

            //上级窗口
            uc._ParentWindow = win;

            //窗口属性
            win._TargetObj = sender;
            win._ShowMaxBtn = true;
            win.CallBack_Event += WinChooseIcon_CallBack_Event;
            win.ShowDialog();
        }
        /// <summary>
        /// 选择图标回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinChooseIcon_CallBack_Event(Components.PopWindow win, object param)
        {
            Button btn = (win._TargetObj as Button);
            string cellName = btn.Tag.ToString();

            try
            {
                //设置选择图标
                VisualTreePlus plus = new VisualTreePlus();
                List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                foreach (DependencyObject child in ctls)
                {
                    if (child is Image)
                    {
                        Image icon = child as Image;
                        icon.Tag = param.ToString();
                        //icon.Source = ImageBrushHandler.GetImageSource(param.ToString());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "设置选择图标异常");
            }

            DataRowView row = null;

            //更新表
            if (_isChooseTop)
            {
                row = this.dataGridTop.SelectedItem as DataRowView;
            }
            else
            {
                row = this.dataGridBottom.SelectedItem as DataRowView;
            }

            //值
            row[cellName] = param;

            //关闭窗口
            win.Close();
        }
        /// <summary>
        /// 上传文件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnUploadFile_Event(object sender, RoutedEventArgs e)
        {
            //选择文件
            string path = Core.Handler.UploadFileHandler.ChooseFileDialog();
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string fileName = AppGlobal.UploadFile(path);

                //按钮
                Button btn = (sender as Button);

                try
                {
                    //设置上传文件
                    VisualTreePlus plus = new VisualTreePlus();
                    List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                    foreach (DependencyObject child in ctls)
                    {
                        if (child is TextBox)
                        {
                            TextBox txt = child as TextBox;
                            txt.Text = fileName;
                            break;
                        }
                    }

                    btn.Content = "上传";
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "设置选择图标异常");
                }
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnChooseFile_Event(object sender, RoutedEventArgs e)
        {
            //选择文件窗口
            Views.Parts.FileManagerUC uc = new Parts.FileManagerUC();

            //弹出窗口
            Views.Components.PopWindow win = new Components.PopWindow(uc, "选择文件");

            //上级窗口
            uc._ParentWindow = win;

            //窗口属性
            win._TargetObj = sender;
            win._ShowMaxBtn = true;
            win.CallBack_Event += WinChooseFile_CallBack_Event;
            win.ShowDialog();
        }
        /// <summary>
        /// 选择文件回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinChooseFile_CallBack_Event(Components.PopWindow win, object param)
        {
            Button btn = (win._TargetObj as Button);
            string cellName = btn.Tag.ToString();

            //选择的文件
            FileItemInfo file = param as FileItemInfo;
            if (file == null) return;

            try
            {
                //设置选择图标
                VisualTreePlus plus = new VisualTreePlus();
                List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                foreach (DependencyObject child in ctls)
                {
                    if (child is TextBox)
                    {
                        TextBox txt = child as TextBox;
                        txt.Text = file.FilePath;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "设置选择图标异常");
            }

            DataRowView row = null;

            //更新表
            if (_isChooseTop)
            {
                row = this.dataGridTop.SelectedItem as DataRowView;
            }
            else
            {
                row = this.dataGridBottom.SelectedItem as DataRowView;
            }

            //值
            row[cellName] = file.FilePath;
            row[AppGlobal.DataGridEditStateCellName] = true;

            //关闭窗口
            win.Close();
        }

        /// <summary>
        /// 选择数据表事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnChooseTable_Event(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            CellInfo cellInfo = btn.Tag as CellInfo;

            long tableId = 0;
            bool yyhgb = false;

            if (_isChooseTop)
            {
                //根据列名得到列
                List<CellInfo> cells = _tableConfig.Cells.Where(p => p.CellName.Equals(cellInfo.CellName)).ToList();
                if (cells == null || cells.Count <= 0) return;
                //外键表
                tableId = cells[0].ForeignTableId;
                yyhgb = _tableConfig.YYHGB;
            }
            else
            {
                //根据列名得到列
                List<CellInfo> cells = _tableConfig.SubTable.Cells.Where(p => p.CellName.Equals(cellInfo.CellName)).ToList();
                if (cells == null || cells.Count <= 0) return;
                //外键表
                tableId = cells[0].ForeignTableId;
                yyhgb = _tableConfig.SubTable.YYHGB;
            }

            //线程得到模块
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = AppGlobal.SysTableName_ModuleDetails,
                        Wheres = new List<Where>()
                        {
                            new Where("TableId", tableId)
                        }
                    };
                    DataTable dtModules = SQLiteDao.GetTable(param);
                    if (dtModules != null && dtModules.Rows.Count > 0)
                    {
                        long moduleId = 0;
                        string moduleName = string.Empty;
                        List<long> moduleIds = new List<long>();

                        foreach (DataRow row in dtModules.Rows)
                        {
                            //模块编号
                            long mid = DataType.Long(row["Id"], 0);
                            moduleIds.Add(mid);

                            if (moduleId <= 0)
                            {
                                moduleId = mid;
                                moduleName = row["ModuleName"].ToString();
                            }
                        }

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //数据列表
                            ListUC uc = new ListUC(tableId, moduleId, moduleIds, moduleName, true);
                            uc.Margin = new Thickness(0);
                            uc.Padding = new Thickness(0);

                            //传递数据
                            if (tableId > 1)
                            {
                                uc._PostParams = new PostParamInfo();
                                uc._PostParams.KeyValues = new List<KeyValue>();
                                uc._PostParams.TopRow = this.dataGridTop.SelectedItem == null ? null : this.dataGridTop.SelectedItem as DataRowView;

                                if (this.dataGridBottom.ItemsSource != null && this.dataGridBottom.SelectedItem != null)
                                {
                                    //子表行
                                    uc._PostParams.BottomRow = this.dataGridBottom.SelectedItem as DataRowView;
                                }

                                if (_QueryParams != null && _QueryParams.Count > 0)
                                {
                                    foreach (KeyValuePair<string, object> kv in _QueryParams)
                                    {
                                        //键值
                                        uc._PostParams.KeyValues.Add(new KeyValue(kv.Key, kv.Value));
                                    }
                                }
                            }

                            moduleName = moduleName.Replace("管理", "");

                            //弹出窗口
                            Components.PopWindow win = new Components.PopWindow(uc, "选择" + moduleName);

                            //上级窗口
                            uc._ParentWindow = win;
                            uc._ParentUC = this;

                            //窗口属性
                            win._TargetObj = sender;
                            win._ShowMaxBtn = true;
                            win._YYHGB = yyhgb;
                            win.CallBack_Event += WinChooseTable_CallBack_Event;
                            win.ShowDialog();

                            return null;
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //数据列表
                            ListUC uc = new ListUC(tableId, 0, null, "", true);
                            uc.Margin = new Thickness(0);
                            uc.Padding = new Thickness(0);

                            uc._PostParams = new PostParamInfo();
                            uc._PostParams.KeyValues = new List<KeyValue>();
                            uc._PostParams.TopRow = this.dataGridTop.SelectedItem == null ? null : this.dataGridTop.SelectedItem as DataRowView;

                            if (this.dataGridBottom.ItemsSource != null && this.dataGridBottom.SelectedItem != null)
                            {
                                //子表行
                                uc._PostParams.BottomRow = this.dataGridBottom.SelectedItem as DataRowView;
                            }

                            if (_QueryParams != null && _QueryParams.Count > 0)
                            {
                                foreach (KeyValuePair<string, object> kv in _QueryParams)
                                {
                                    //键值
                                    uc._PostParams.KeyValues.Add(new KeyValue(kv.Key, kv.Value));
                                }
                            }

                            //弹出窗口
                            Components.PopWindow win = new Components.PopWindow(uc, "选择数据");

                            //上级窗口
                            uc._ParentWindow = win;

                            //窗口属性
                            win._TargetObj = sender;
                            win._ShowMaxBtn = true;
                            win.CallBack_Event += WinChooseTable_CallBack_Event;
                            win.ShowDialog();

                            return null;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "选择数据表事件异常");
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 弹出列表选择
        /// </summary>
        /// <param name="cellInfo"></param>
        private void PopChooseListTable(CellInfo cellInfo)
        {
            bool yyhgb = false;

            if (_isChooseTop)
            {
                yyhgb = _tableConfig.YYHGB;
            }
            else
            {
                yyhgb = _tableConfig.SubTable.YYHGB;
            }

            try
            {
                ListUC uc = new ListUC(cellInfo.ForeignTableId, 0, null, "", true);
                uc.Margin = new Thickness(0);
                uc.Padding = new Thickness(0);

                uc._PostParams = new PostParamInfo();
                uc._PostParams.KeyValues = new List<KeyValue>();
                uc._PostParams.TopRow = this.dataGridTop.SelectedItem == null ? null : this.dataGridTop.SelectedItem as DataRowView;

                if (this.dataGridBottom.ItemsSource != null && this.dataGridBottom.SelectedItem != null)
                {
                    //子表行
                    uc._PostParams.BottomRow = this.dataGridBottom.SelectedItem as DataRowView;
                }

                if (_QueryParams != null && _QueryParams.Count > 0)
                {
                    foreach (KeyValuePair<string, object> kv in _QueryParams)
                    {
                        //键值
                        uc._PostParams.KeyValues.Add(new KeyValue(kv.Key, kv.Value));
                    }
                }

                //弹出窗口
                Components.PopWindow win = new Components.PopWindow(uc, "选择数据");

                //上级窗口
                uc._ParentWindow = win;

                //窗口属性
                win._TargetObj = cellInfo;
                win._ShowMaxBtn = true;
                win._YYHGB = yyhgb;
                win.CallBack_Event += WinChooseTable_CallBack_Event;
                win.CloseWindow_Event += WinChooseTable_CloseWindow_Event;
                win.ShowDialog();
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 关闭引用窗口
        /// </summary>
        /// <param name="win"></param>
        private void WinChooseTable_CloseWindow_Event(Components.PopWindow win)
        {
            if (win._TargetObj is CellInfo)
            {
                if (win.CallBackRowCount <= 0)
                {
                    try
                    {
                        if (_isChooseTop)
                        {
                            TopAddNewRow(true);
                        }
                        else
                        {
                            BottomAddNewRow(true);
                        }
                    }
                    catch (Exception ex) { }
                }
                return;
            }
        }
        /// <summary>
        /// 选择数据记录回调
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinChooseTable_CallBack_Event(Components.PopWindow win, object param)
        {
            if (win._TargetObj is CellInfo)
            {
                try
                {
                    //设置返回行
                    SetReturnRow(param as DataRow, win._TargetObj as CellInfo, true);
                }
                catch (Exception ex) { }

                try
                {
                    //关闭窗口
                    if (win._YYHGB) win.Close();
                }
                catch (Exception ex) { }
                return;
            }

            //是否新添加行
            bool isNewRow = win.CallBackRowCount > 1;

            //点击的按钮
            Button btn = (win._TargetObj as Button);
            CellInfo cellInfo = btn.Tag as CellInfo;

            if (param == null)
            {
                //清空
                ClearRelativeTable(cellInfo);

                try
                {
                    //关闭窗口
                    if (win._YYHGB) win.Close();
                }
                catch (Exception ex) { }
                return;
            }

            //返回的行
            DataRow returnRow = param as DataRow;

            //调试日志
            //if (_isChooseTop) AppCode.Handler.DebugHandler.AddDebug(_moduleId, this.Title, _tableConfig, "选择数据回传行", returnRow);
            //else AppCode.Handler.DebugHandler.AddDebug(_moduleId, this.Title, _tableConfig.SubTable, "选择数据回传行", returnRow);

            if (!isNewRow && cellInfo.ValType.Equals("long") && returnRow.Table.Columns.Contains("Id"))
            {
                try
                {
                    //设置选择主键
                    VisualTreePlus plus = new VisualTreePlus();
                    List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                    foreach (DependencyObject child in ctls)
                    {
                        if (child is TextBlock)
                        {
                            TextBlock lbl = child as TextBlock;
                            if (!string.IsNullOrWhiteSpace(cellInfo.ReturnCellName))
                            {
                                //返回对应的列名
                                lbl.Text = returnRow[cellInfo.ReturnCellName].ToString();
                            }
                            else
                            {
                                lbl.Text = returnRow["Id"].ToString();
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "选择数据记录回调异常");
                }
            }

            //设置返回行
            SetReturnRow(returnRow, cellInfo, isNewRow);

            try
            {
                //关闭窗口
                if (win._YYHGB) win.Close();
            }
            catch (Exception ex) { }
        }



        /// <summary>
        /// 编辑大文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnEditPopText_Event(object sender, RoutedEventArgs e)
        {
            //按钮
            Button btn = sender as Button;
            string text = string.Empty;
            try
            {
                VisualTreePlus plus = new VisualTreePlus();
                List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                foreach (DependencyObject child in ctls)
                {
                    if (child is TextBox)
                    {
                        text = (child as TextBox).Text;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "编辑大文本异常");
            }

            //选择文本编辑框
            Parts.EditPopTextUC uc = new Parts.EditPopTextUC(text);

            //弹出窗口
            //Ctls.PopWindow win = new Ctls.PopWindow(uc, "编辑文本");
            Views.Components.PageWindow win = new Views.Components.PageWindow(uc, "编辑文本");

            //上级窗口
            uc._ParentWindow = win;

            //窗口属性
            win._TargetObj = sender;
            win._ShowMaxBtn = true;

            win.CallBack_Event += WinEditText_CallBack_Event;
            win.ShowDialog();
        }
        /// <summary>
        /// 编辑富文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_BtnEditFullText_Event(object sender, RoutedEventArgs e)
        {
            //按钮
            Button btn = sender as Button;
            string text = string.Empty;
            try
            {
                VisualTreePlus plus = new VisualTreePlus();
                List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                foreach (DependencyObject child in ctls)
                {
                    if (child is TextBox)
                    {
                        text = (child as TextBox).Text;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "编辑富文本异常");
            }

            bool isUMEditor = true;

            try
            {
                //是否使用UMEditor
                string cellName = btn.Tag.ToString();
                if (_isChooseTop)
                {
                    isUMEditor = _tableConfig.Cells.Find(p => p.CellName.Equals(cellName)).IsUMEditor;
                }
                else
                {
                    isUMEditor = _tableConfig.SubTable.Cells.Find(p => p.CellName.Equals(cellName)).IsUMEditor;
                }
            }
            catch { }

            //富文件编辑窗口
            Views.Parts.EditFullTextUC uc = new Parts.EditFullTextUC(text, !isUMEditor);

            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            double width = screenWidth * 0.8;
            double height = screenHeight * 0.8;

            //弹出窗口
            Views.Components.PageWindow win = new Views.Components.PageWindow(uc, "编辑富文本");
            win.Width = width;
            win.Height = height;

            //上级窗口
            uc._ParentWindow = win;

            //窗口属性
            win._TargetObj = sender;
            win._ShowMaxBtn = true;
            win.CallBack_Event += WinEditText_CallBack_Event;
            win.ShowDialog();
        }
        /// <summary>
        /// 编辑完文本回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinEditText_CallBack_Event(Components.PageWindow win, object param)
        {
            Button btn = (win._TargetObj as Button);
            string cellName = btn.Tag.ToString();

            string text = param.ToString();

            try
            {
                VisualTreePlus plus = new VisualTreePlus();
                List<DependencyObject> ctls = plus.GetChildren(btn.Parent);

                foreach (DependencyObject child in ctls)
                {
                    if (child is TextBox)
                    {
                        TextBox txt = child as TextBox;
                        txt.Text = text;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "编辑完文本回传异常");
            }

            DataRowView row = null;

            //更新表
            if (_isChooseTop)
            {
                row = this.dataGridTop.SelectedItem as DataRowView;
            }
            else
            {
                row = this.dataGridBottom.SelectedItem as DataRowView;
            }

            //值
            row[cellName] = text;
            row[AppGlobal.DataGridEditStateCellName] = true;

            //关闭窗口
            win.Close();
        }


        /// <summary>
        /// 下拉列表加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cmb = (sender as ComboBox);
            cmb.SelectionChanged += Cmb_SelectionChanged;
            cmb.LostFocus += Cmb_LostFocus;
        }
        /// <summary>
        /// 下拉列表失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmb_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBox cmb = (sender as ComboBox);
                string cellName = cmb.Tag.ToString();

                List<string> vals = cmb.ItemsSource as List<string>;

                string text = cmb.Text.Trim();
                if (!vals.Contains(text))
                {
                    vals.Add(text);
                }

                cmb.SelectedItem = text;

                //选择改变
                ComBoxSelectionChanged(sender);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 下拉列表选择变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComBoxSelectionChanged(sender);
        }
        /// <summary>
        /// 下拉列表选择变更
        /// </summary>
        /// <param name="sender"></param>
        private void ComBoxSelectionChanged(object sender)
        {
            try
            {
                ComboBox cmb = (sender as ComboBox);
                string cellName = cmb.Tag.ToString();

                try
                {
                    VisualTreePlus plus = new VisualTreePlus();
                    List<DependencyObject> ctls = plus.GetChildren(cmb);

                    foreach (DependencyObject child in ctls)
                    {
                        if (child is TextBlock)
                        {
                            TextBlock lbl = child as TextBlock;
                            lbl.Text = cmb.Text.ToString();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "下拉列表编辑完成异常1");
                }

                DataRowView row = null;

                //更新表
                if (_isChooseTop)
                {
                    row = this.dataGridTop.SelectedItem as DataRowView;
                }
                else
                {
                    row = this.dataGridBottom.SelectedItem as DataRowView;
                }

                //值
                row[cellName] = cmb.Text;
                row[AppGlobal.DataGridEditStateCellName] = true;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "下拉列表编辑完成异常2");
            }
        }

        #region 自动完成下拉列表
        /// <summary>
        /// 自动完成下拉列表 加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxAutoComple_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            cmb.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, TxtChange_Event, false);
            cmb.SelectionChanged += ComboBoxAutoComple_SelectionChanged;
        }

        /// <summary>
        /// 选择项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxAutoComple_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            if (cmb == null || cmb.SelectedItem == null) return;

            //下拉列表信息
            DropDownInfo ddInfo = cmb.Tag as DropDownInfo;

            //得到列信息
            GetDropDownCellInfo(ref ddInfo);

            //选择记录
            DataRowView row = cmb.SelectedItem as DataRowView;

            ddInfo.ChooseValue = cmb.Text;
            cmb.Tag = ddInfo;
            cmb.Text = cmb.Text;
            return;

            string showText = string.Empty;

            DataGrid datagrid = FindVisualParent<DataGrid>(cmb);
            DataRowView drv = datagrid.SelectedItem as DataRowView;

            //列类型
            if (ddInfo.CellInfo.ValType == "long")
            {
                drv[ddInfo.CellName] = row["Id"];
                ddInfo.ChooseValue = row["Id"].ToString();
                showText = row["Id"].ToString();
            }
            else
            {
                drv[ddInfo.CellName] = cmb.Text;
                ddInfo.ChooseValue = cmb.Text;
                showText = cmb.Text;
            }

            //赋值
            cmb.Tag = ddInfo;
            cmb.Text = showText;
        }

        /// <summary>
        /// 下拉列表输入文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListUC_TxtChange_Event(object sender, RoutedEventArgs e)
        {
            //下拉列表
            ComboBox cmb = sender as ComboBox;
            //当前选择值
            string text = cmb.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            //下拉列表信息
            DropDownInfo ddInfo = cmb.Tag as DropDownInfo;
            if (ddInfo.ChooseValue != null && !string.IsNullOrWhiteSpace(ddInfo.ChooseValue.ToString()))
            {
                //当前无需加载值
                if (ddInfo.ChooseValue.ToString().Equals(text.ToString())) return;
            }

            //选择的项
            if (cmb.Items.Contains(text)) return;

            //得到列信息
            long tableId = GetDropDownCellInfo(ref ddInfo);
            var tableInfo = AppGlobal.GetTableConfig(tableId);

            //查询数据表
            DataTable dt = SQLiteDao.GetTable(new SQLParam()
            {
                TableName = tableInfo.TableName,
                Wheres = new List<Where>()
                {
                    new Where("SearchKeywords", text, WhereType.模糊查询)
                }
            });

            ddInfo.Data = dt;
            cmb.Tag = ddInfo;

            if (dt == null)
            {
                cmb.ItemsSource = null;
            }
            else
            {
                //绑定数据
                if (string.IsNullOrWhiteSpace(cmb.DisplayMemberPath))
                {
                    string displayMember = "Id";
                    if (dt.Columns.Contains("CnName"))
                    {
                        displayMember = "CnName";
                    }
                    else if (dt.Columns.Contains("Name"))
                    {
                        displayMember = "Name";
                    }
                    else
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (col.ColumnName.EndsWith("Name"))
                            {
                                displayMember = col.ColumnName;
                                break;
                            }
                        }
                    }
                }

                List<string> vals = new List<string>();

                foreach (DataRow r in dt.Rows)
                {
                    vals.Add(r["Id"].ToString());
                }

                cmb.ItemsSource = vals;
            }
        }
        /// <summary>
        /// 得到下拉列信息
        /// </summary>
        /// <param name="ddInfo"></param>
        /// <returns>外键表编号</returns>
        private long GetDropDownCellInfo(ref DropDownInfo ddInfo)
        {
            long foreignTableId = 0;

            if (ddInfo.CellInfo == null)
            {
                //列名称
                string cellName = ddInfo.CellName;

                if (_isChooseTop)
                {
                    //根据列名得到列
                    List<CellInfo> cells = _tableConfig.Cells.Where(p => p.CellName.Equals(cellName)).ToList();
                    if (cells == null || cells.Count <= 0) return 0;

                    ddInfo.CellInfo = cells[0];
                    foreignTableId = ddInfo.CellInfo.ForeignTableId;
                }
                else
                {
                    //根据列名得到列
                    List<CellInfo> cells = _tableConfig.SubTable.Cells.Where(p => p.CellName.Equals(cellName)).ToList();
                    if (cells == null || cells.Count <= 0) return 0;

                    ddInfo.CellInfo = cells[0];
                    foreignTableId = ddInfo.CellInfo.ForeignTableId;
                }
            }
            else
            {
                foreignTableId = ddInfo.CellInfo.ForeignTableId;
            }

            return foreignTableId;
        }
        #endregion

        #region 下拉数据表
        /// <summary>
        /// 加载后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDownTable_Loaded(object sender, RoutedEventArgs e)
        {
            Controls.DropDownTable ddt = sender as Controls.DropDownTable;
            CellInfo cellInfo = ddt.Tag as CellInfo;
            ddt.TableId = cellInfo.ForeignTableId;
            ddt.CellInfo = cellInfo;
            ddt.ChooseCallBack_Event += Ddt_ChooseCallBack_Event;
            ddt.LostFocusCallBack_Event += Ddt_LostFocusCallBack_Event;
            ddt.Txt_Change_Event += Ddt_Txt_Change_Event;
            ddt.ListUC = this;
        }

        /// <summary>
        /// 内容文本改变
        /// </summary>
        /// <param name="uc"></param>
        /// <param name="txt"></param>
        private void Ddt_Txt_Change_Event(Controls.DropDownTable uc, string txt, int pageIndex, int pageSize)
        {
            //是否有表信息
            if (uc._tableInfo == null)
            {
                //加载表信息
                uc.LoadTableConfigs();
            }

            //清空且不是查询栏
            if (string.IsNullOrWhiteSpace(txt) && !uc.IsQuery)
            {
                //清空关联表值
                ClearRelativeTable(uc.CellInfo);
            }

            long khid = 0;

            if (!uc.IsQuery)
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    try
                    {
                        if (_isChooseTop)
                        {
                            //标记已经编辑
                            DataRowView topRow = this.dataGridTop.SelectedItem as DataRowView;
                            if (topRow != null) topRow[AppGlobal.DataGridEditStateCellName] = true;
                        }
                        else
                        {
                            //标记已经编辑
                            DataRowView bottomRow = this.dataGridBottom.SelectedItem as DataRowView;
                            if (bottomRow != null) bottomRow[AppGlobal.DataGridEditStateCellName] = true;
                        }
                    }
                    catch (Exception ex) { }
                    return null;
                }));
            }

            #region 不是查询 获取KHBH/KHID
            if (!uc.IsQuery)
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    try
                    {
                        //是否有选择行  是否有客户编号列
                        if (this.dataGridTop.SelectedItem != null && _tableConfig.Cells.Exists(p => p.CellName.Equals("KHID")))
                        {
                            //当前选择的行
                            DataRowView row = this.dataGridTop.SelectedItem as DataRowView;
                            if (row == null) return null;

                            //客户ID
                            khid = DataType.Long(row["KHID"], 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "下拉数据表获取客户编号异常");
                    }
                    return null;
                }));
            }
            #endregion

            //过滤关键字
            txt = Core.Handler.SqlHandler.FilterSql(txt);

            TableInfo tableInfo = uc._tableInfo;

            SQLParam param = new SQLParam()
            {
                TableName = tableInfo.TableName,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Wheres = new List<Where>(),
                OrderBys = new List<OrderBy>()
            };

            if (khid > 0)
            {
                if (tableInfo.Cells.Exists(p => p.CellName == "KHID"))
                {
                    param.Wheres.Add(new Where("KHID", khid));
                }
                else if (tableInfo.Cells.Exists(p => p.CellName == "MZZB_KHID"))
                {
                    param.Wheres.Add(new Where("MZZB_KHID", khid));
                }
            }

            if (!string.IsNullOrWhiteSpace(txt))
            {
                param.Wheres.Add(new Where("SearchKeywords", txt, WhereType.模糊查询));
            }

            //分页
            var result = QueryService.GetPaging(tableInfo, AppGlobal.UserInfo, param);

            //显示数据
            uc._pageSize = pageSize;
            uc.ShowData(result.Data, result.TotalCount);
            return;
        }
        /// <summary>
        /// 选择项后
        /// </summary>
        /// <param name="row"></param>
        private void Ddt_ChooseCallBack_Event(Controls.DropDownTable uc, DataRow row)
        {
            //设置其它值
            SetReturnRow(row, uc.CellInfo);

            try
            {
                DataGridCell cell = FindVisualParent<DataGridCell>(uc);
                if (cell != null) { cell.Focus(); }
                else
                {
                    DataGrid dataGrid = null;

                    if (_isChooseTop)
                    {
                        dataGrid = this.dataGridTop;
                    }
                    else
                    {
                        dataGrid = this.dataGridBottom;
                    }

                    int rowIndex = dataGrid.SelectedIndex;
                    int colIndex = 0;

                    foreach (DataGridColumn column in dataGrid.Columns)
                    {
                        if (column.SortMemberPath.Equals(uc.CellName))
                        {
                            colIndex = column.DisplayIndex;
                            break;
                        }
                    }

                    cell = dataGrid.GetCell(rowIndex, colIndex);
                    cell.Focus();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /// <summary>
        /// 失去焦点事件
        /// </summary>
        /// <param name="uc"></param>
        private void Ddt_LostFocusCallBack_Event(Controls.DropDownTable uc, Key key)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    DataGrid dataGrid = null;

                    if (_isChooseTop)
                    {
                        dataGrid = this.dataGridTop;
                    }
                    else
                    {
                        dataGrid = this.dataGridBottom;
                    }

                    if (dataGrid == null) return null;

                    int currentRowIndex = dataGrid.SelectedIndex;
                    int currentCellIndex = dataGrid.CurrentCell.Column.DisplayIndex;

                    if (key == Key.Left)
                    {
                        currentCellIndex -= 1;
                    }
                    else
                    {
                        currentCellIndex += 1;
                    }

                    DataGridCell rightCell = dataGrid.GetCell(currentRowIndex, currentCellIndex);
                    rightCell?.Focus();
                }
                catch (Exception ex) { }
                return null;
            }));
        }
        #endregion

        /// <summary>
        /// 清空关联表的值
        /// </summary>
        /// <param name="uc"></param>
        private void ClearRelativeTable(CellInfo cellInfo)
        {
            DataGrid dataGrid = null;
            int rowIndex = 0;
            TableInfo tableInfo = null;
            DataRowView row = null;

            //更新表
            if (_isChooseTop)
            {
                //主表
                dataGrid = this.dataGridTop;
                //所有列
                tableInfo = _tableConfig;
            }
            else
            {
                //子表
                dataGrid = this.dataGridBottom;
                //所有列
                tableInfo = _tableConfig.SubTable;
            }

            try
            {
                //选择索引
                rowIndex = dataGrid.SelectedIndex;
                //选择的行
                row = dataGrid.SelectedItem as DataRowView;

                //表维护
                if (tableInfo.TableName.Equals(AppGlobal.SysTableName_Tables) || tableInfo.TableName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    if ((cellInfo.CellName.Equals("ParentId") || cellInfo.CellName.Equals("ParentTableName")) && tableInfo.TableName.Equals(AppGlobal.SysTableName_Tables))
                    {
                        //表返回行
                        row["ParentId"] = 0;
                        row["ParentTableName"] = "";
                    }
                    else if ((cellInfo.CellName.Equals("ForeignTableId") || cellInfo.CellName.Equals("ForeignTableName")) && tableInfo.TableName.Equals(AppGlobal.SysTableName_TableCells))
                    {
                        //表列返回行
                        row["ForeignTableId"] = 0;
                        row["ForeignTableName"] = "";
                    }
                    return;
                }

                foreach (CellInfo cell in tableInfo.Cells)
                {
                    string valType = cell.ValType.ToLower();

                    dynamic val;
                    if (valType.Equals("date") || valType.Equals("datetime"))
                    {
                        val = DateTime.Now;
                    }
                    else if (valType.Equals("int") || valType.Equals("long") || valType.Equals("float") || valType.Equals("double") || valType.Equals("decimal"))
                    {
                        val = 0;
                    }
                    else if (valType.Equals("bool"))
                    {
                        val = false;
                    }
                    else
                    {
                        val = "";
                    }

                    if (cellInfo.ReturnGroup > 0)
                    {
                        if (cell.ReturnGroup == cellInfo.ReturnGroup || cell.ReturnGroup == 0)
                        {
                            if (cell.ForeignTableId == cellInfo.ForeignTableId)
                            {
                                row[cell.CellName] = val;
                            }
                        }
                    }
                    else
                    {
                        if (cell.ForeignTableId == cellInfo.ForeignTableId)
                        {
                            row[cell.CellName] = val;
                        }
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 其它按钮事件【重新加载数据/执行查询/复制粘贴】

        /// <summary>
        /// 重新加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            this.panelLoadDataError.Visibility = Visibility.Collapsed;
            this.borderActions.Visibility = Visibility.Visible;

            this.dataGridTop.Visibility = Visibility.Visible;
            this.dataGridBottom.Visibility = Visibility.Collapsed;

            //查询数据焦点聚焦到数据表
            _isQueryDataFocusTable = true;

            //加载数据
            LoadData();

            if (_tableConfig != null && _tableConfig.SubTable != null && !_isPopWindow)
            {
                //双表结构
                this.dataGridBottom.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            //查询数据聚焦到数据表
            _isQueryDataFocusTable = true;

            //查询数据
            QueryData();
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        private void QueryData()
        {
            //重置页索引
            _PageIndex = 1;
            _PageIndexSub = 1;

            _bottomOrderBys = new List<OrderBy>();
            _bottomOrderBysSql = "";

            _queryWheres = new List<Where>();

            //更多查询条件
            _moreQueryWheres = "";

            //生成查询条件
            BuildQueryWhere();

            //查询数据
            LoadData();
        }
        /// <summary>
        /// 生成查询条件
        /// </summary>
        private void BuildQueryWhere()
        {
            //是否有需要查询的
            if (_queryCells == null || _queryCells.Count <= 0) return;

            foreach (CellInfo cell in _queryCells)
            {
                //值类型
                string valType = cell.ValType;

                #region 日期格式
                if (valType.Equals("date") || valType.Equals("datetime"))
                {
                    //日期范围
                    DatePicker dt1 = this.panelQuerys.FindName(cell.CellName + "_From") as DatePicker;
                    DatePicker dt2 = this.panelQuerys.FindName(cell.CellName + "_End") as DatePicker;

                    if (dt1.SelectedDate.HasValue)
                    {
                        Where whereFrom = new Where()
                        {
                            CellName = cell.CellName,
                            CellValue = dt1.SelectedDate.Value.ToString("yyyy-MM-dd"),
                            Type = WhereType.大于等于
                        };

                        if (_queryWheres.Exists(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.大于等于))
                        {
                            //已经存在查询条件 直接更换值
                            _queryWheres.Find(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.大于等于).CellValue = whereFrom.CellValue;
                        }
                        else
                        {
                            //添加新的查询条件
                            _queryWheres.Add(whereFrom);
                        }

                        //设置查询值
                        if (cell.CellName.Equals("RQ") || cell.CellName.Equals("MZZB_RQ")) SetQueryParams("BeginDate", whereFrom.CellValue);
                        else SetQueryParams(cell.CellName + "_From", whereFrom.CellValue);
                    }
                    else
                    {
                        //移除条件
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.大于等于));
                        //设置查询值
                        if (cell.CellName.Equals("RQ") || cell.CellName.Equals("MZZB_RQ")) SetQueryParams("BeginDate", null);
                        else SetQueryParams(cell.CellName + "_From", null);
                    }

                    if (dt2.SelectedDate.HasValue)
                    {
                        Where whereEnd = new Where()
                        {
                            CellName = cell.CellName,
                            CellValue = dt2.SelectedDate.Value.ToString("yyyy-MM-dd 23:59:59"),
                            Type = WhereType.小于等于
                        };

                        if (_queryWheres.Exists(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.小于等于))
                        {
                            //已经存在查询条件 直接更换值
                            _queryWheres.Find(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.小于等于).CellValue = whereEnd.CellValue;
                        }
                        else
                        {
                            //添加新的查询条件
                            _queryWheres.Add(whereEnd);
                        }

                        //设置查询值
                        if (cell.CellName.Equals("RQ") || cell.CellName.Equals("MZZB_RQ")) SetQueryParams("EndDate", whereEnd.CellValue);
                        else SetQueryParams(cell.CellName + "_End", whereEnd.CellValue);
                    }
                    else
                    {
                        //移除条件
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName) && p.Type == WhereType.小于等于));
                        //设置查询值
                        if (cell.CellName.Equals("RQ") || cell.CellName.Equals("MZZB_RQ")) SetQueryParams("EndDate", null);
                        else SetQueryParams(cell.CellName + "_End", null);
                    }

                    continue;
                }
                #endregion

                //是否找到对象
                object obj = this.panelQuerys.FindName(cell.CellName);

                //是否有对象
                if (obj == null && _queryWheres.Exists(p => p.CellName.Equals(cell.CellName)))
                {
                    //移除查询条件
                    _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                    continue;
                }

                //得到对象
                UIElement ele = obj as UIElement;

                Where where = new Where()
                {
                    CellName = cell.CellName
                };

                if (ele is TextBox)
                {
                    //文本框
                    string text = (ele as TextBox).Text.Trim();
                    where.CellValue = text;
                    where.Type = WhereType.模糊查询;

                    //没有值
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }
                    else
                    {
                        try
                        {
                            //转换类型
                            Type type = Core.AppHandler.GetTypeByString(cell.ValType);
                            where.CellValue = Convert.ChangeType(text, type);
                            SetQueryParams(where.CellName, where.CellValue);
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.AddException(ex, "转换类型异常");
                        }
                    }
                }
                else if (ele is ComboBox)
                {
                    //下拉列表的值
                    object objSelect = (ele as ComboBox).SelectedItem;

                    //没有值
                    if (objSelect == null || string.IsNullOrWhiteSpace(objSelect.ToString()))
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }

                    where.CellValue = objSelect;
                    where.Type = WhereType.相等;
                    SetQueryParams(where.CellName, where.CellValue);
                }
                else if (ele is CheckBox)
                {
                    //复选框
                    bool? isCheck = (ele as CheckBox).IsChecked;

                    if (cell.CellName.Equals("SYS_SFYJWC"))
                    {
                        //是否已经完成
                        if (isCheck == null)
                        {
                            //不判断
                        }
                        else if (isCheck.Value)
                        {
                            //已经完成的
                            _moreQueryWheres = "mx.[WCSL]>=mx.[SL]";
                        }
                        else
                        {
                            //未完成的
                            _moreQueryWheres = "mx.[WCSL]<mx.[SL]";
                        }
                        continue;
                    }

                    if (isCheck == null)
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }

                    where.CellValue = isCheck.Value;
                    where.Type = WhereType.相等;
                    SetQueryParams(where.CellName, where.CellValue);
                }
                else if (ele is Controls.DropDownTable)
                {
                    //下拉选择数据表
                    Controls.DropDownTable ddt = ele as Controls.DropDownTable;

                    string txt = ddt.Text;
                    if (string.IsNullOrWhiteSpace(txt))
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }

                    where.CellValue = ddt.Text;
                    where.Type = WhereType.模糊查询;
                    SetQueryParams(where.CellName, where.CellValue);
                }
                else if (ele is Button)
                {
                    //按钮
                    object btnTag = (ele as Button).Tag;
                    if (btnTag == null)
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }
                    where.CellValue = btnTag;
                    where.Type = WhereType.相等;
                    SetQueryParams(where.CellName, where.CellValue);
                }
                else if (ele is Views.Controls.AreaUC)
                {
                    //地区
                    string provinceCity = (ele as Views.Controls.AreaUC).Text;
                    if (string.IsNullOrWhiteSpace(provinceCity))
                    {
                        _queryWheres.Remove(_queryWheres.Find(p => p.CellName.Equals(cell.CellName)));
                        SetQueryParams(where.CellName, null);
                        continue;
                    }
                    where.CellValue = provinceCity;
                    where.Type = WhereType.模糊后;
                    SetQueryParams(where.CellName, where.CellValue);
                }

                if (_queryWheres.Exists(p => p.CellName.Equals(cell.CellName)))
                {
                    //已经存在查询条件 直接更换值
                    _queryWheres.Find(p => p.CellName.Equals(cell.CellName)).CellValue = where.CellValue;
                }
                else
                {
                    //添加新的查询条件
                    _queryWheres.Add(where);
                }
                SetQueryParams(where.CellName, where.CellValue);
            }
        }
        /// <summary>
        /// 设置查询值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SetQueryParams(string key, object value)
        {
            //新实例
            if (_QueryParams == null) _QueryParams = new Dictionary<string, object>();

            //是否存在Key
            if (_QueryParams.ContainsKey(key))
            {
                //存在Key 更新值
                _QueryParams[key] = value;
            }
            else
            {
                //不存在Key 添加值
                _QueryParams.Add(key, value);
            }
        }
        /// <summary>
        /// 鼠标右键菜单
        /// </summary>
        private void AddRightKeyMenus(bool isTop = true)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    if (isTop)
                    {
                        //主表右键菜单
                        ContextMenu rightKeyMenus = new ContextMenu();

                        MenuItem itemCopyCell = new MenuItem();
                        itemCopyCell.Header = "复制文本";
                        itemCopyCell.Click += ItemCopyCell_Click;

                        MenuItem itemCopy = new MenuItem();
                        itemCopy.Header = "复制明细";
                        itemCopy.Click += ItemCopy_Click;

                        MenuItem itemPaste = new MenuItem();
                        itemPaste.Header = "粘贴明细";
                        itemPaste.Click += ItemPaste_Click;

                        rightKeyMenus.Items.Add(itemCopyCell);

                        //右键列表
                        if (_tableConfig.IsRealTable)
                        {
                            rightKeyMenus.Items.Add(itemCopy);
                            rightKeyMenus.Items.Add(itemPaste);
                        }

                        if (_tableConfig.TableType == TableType.单表 || _tableConfig.TableType == TableType.虚拟)
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            MenuItem itemMoveUp = new MenuItem();
                            itemMoveUp.Header = "上移一行";
                            itemMoveUp.Click += TopItemMoveUp_Click;
                            rightKeyMenus.Items.Add(itemMoveUp);

                            MenuItem itemMoveDown = new MenuItem();
                            itemMoveDown.Header = "下移一行";
                            itemMoveDown.Click += TopItemMoveDown_Click;
                            rightKeyMenus.Items.Add(itemMoveDown);
                        }

                        //管理员设置列排序
                        if (AppGlobal.UserInfo.Id == 1)
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            MenuItem itemSetColumnDefaultOrder = new MenuItem();
                            itemSetColumnDefaultOrder.Header = "设置为列默认排序";
                            itemSetColumnDefaultOrder.Click += TopItemSetColumnDefaultOrder_Click;
                            rightKeyMenus.Items.Add(itemSetColumnDefaultOrder);

                            MenuItem itemDeleteOtherUserColunOrder = new MenuItem();
                            itemDeleteOtherUserColunOrder.Header = "删除其它用户列排序";
                            itemDeleteOtherUserColunOrder.Click += TopItemDeleteOtherUserColumnOrder_Click;
                            rightKeyMenus.Items.Add(itemDeleteOtherUserColunOrder);
                        }

                        //表维护
                        if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            MenuItem itemSetSystem = new MenuItem();
                            itemSetSystem.Header = "设置系统列";
                            itemSetSystem.Click += ItemSetSystem_Click;
                            rightKeyMenus.Items.Add(itemSetSystem);

                            MenuItem itemCancelSystem = new MenuItem();
                            itemCancelSystem.Header = "取消系统列";
                            itemCancelSystem.Click += ItemCancelSystem_Click;
                            rightKeyMenus.Items.Add(itemCancelSystem);
                        }

                        //定义菜单
                        if (_tableConfig.Menus != null && _tableConfig.Menus.Count > 0)
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            foreach (MenuInfo menu in _tableConfig.Menus)
                            {
                                if (string.IsNullOrWhiteSpace(menu.MenuName)) continue;
                                if (string.IsNullOrWhiteSpace(menu.ExecuteSQL)) continue;

                                MenuItem topMenu = new MenuItem();
                                topMenu.Tag = menu;
                                topMenu.Header = menu.MenuName;
                                topMenu.Click += TopMenu_Click;
                                rightKeyMenus.Items.Add(topMenu);
                            }
                        }

                        //关联托盘控件
                        this.dataGridTop.ContextMenu = rightKeyMenus;
                    }
                    else
                    {
                        //子表右键菜单
                        ContextMenu rightKeyMenus = new ContextMenu();

                        MenuItem itemCopyCell = new MenuItem();
                        itemCopyCell.Header = "复制文本";
                        itemCopyCell.Click += BottomItemCopyCell_Click;
                        rightKeyMenus.Items.Add(itemCopyCell);

                        MenuItem itemMoveUp = new MenuItem();
                        itemMoveUp.Header = "上移一行";
                        itemMoveUp.Click += BottomItemMoveUp_Click;
                        rightKeyMenus.Items.Add(itemMoveUp);

                        MenuItem itemMoveDown = new MenuItem();
                        itemMoveDown.Header = "下移一行";
                        itemMoveDown.Click += BottomItemMoveDown_Click;
                        rightKeyMenus.Items.Add(itemMoveDown);

                        //管理员设置列排序
                        if (AppGlobal.UserInfo.Id == 1)
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            MenuItem itemSetColumnDefaultOrder = new MenuItem();
                            itemSetColumnDefaultOrder.Header = "设置为列默认排序";
                            itemSetColumnDefaultOrder.Click += BottomItemSetColumnDefaultOrder_Click;
                            rightKeyMenus.Items.Add(itemSetColumnDefaultOrder);

                            MenuItem itemDeleteOtherUserColunOrder = new MenuItem();
                            itemDeleteOtherUserColunOrder.Header = "删除其它用户列排序";
                            itemDeleteOtherUserColunOrder.Click += BottomItemDeleteOtherUserColumnOrder_Click;
                            rightKeyMenus.Items.Add(itemDeleteOtherUserColunOrder);
                        }

                        //定义菜单
                        if (_tableConfig.SubTable.Menus != null && _tableConfig.SubTable.Menus.Count > 0)
                        {
                            rightKeyMenus.Items.Add(new Separator());

                            foreach (MenuInfo menu in _tableConfig.SubTable.Menus)
                            {
                                if (string.IsNullOrWhiteSpace(menu.MenuName)) continue;
                                if (string.IsNullOrWhiteSpace(menu.ExecuteSQL)) continue;

                                MenuItem bottomMenu = new MenuItem();
                                bottomMenu.Tag = menu;
                                bottomMenu.Header = menu.MenuName;
                                bottomMenu.Click += BottomMenu_Click;
                                rightKeyMenus.Items.Add(bottomMenu);
                            }
                        }

                        //关联托盘控件
                        this.dataGridBottom.ContextMenu = rightKeyMenus;
                    }
                }
                catch { }

                return null;
            }));
        }

        /// <summary>
        /// 主表菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopMenu_Click(object sender, RoutedEventArgs e)
        {
            //右键项
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            //菜单项
            MenuInfo menu = menuItem.Tag as MenuInfo;

            //执行事件
            RightKeyEvent(menu, true);
        }
        /// <summary>
        /// 子表菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomMenu_Click(object sender, RoutedEventArgs e)
        {
            //右键项
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            //菜单项
            MenuInfo menu = menuItem.Tag as MenuInfo;

            //执行事件
            RightKeyEvent(menu, false);
        }

        /// <summary>
        /// 设置表列维护所有为系统列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemSetSystem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView topSelectItem = this.dataGridTop.SelectedItem as DataRowView;

                SQLParam param = new SQLParam()
                {
                    TableName = "Sys_TableCells",
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("IsSystem",true)
                    },
                    Wheres = new List<Where>()
                    {
                        new Where("ParentId", topSelectItem["Id"])
                    }
                };
                bool flag = SQLiteDao.Update(param);
                if (flag)
                {
                    try
                    {
                        //数据表
                        DataView dt = this.dataGridBottom.ItemsSource as DataView;
                        foreach (DataRow row in dt.Table.Rows)
                        {
                            row["IsSystem"] = true;
                        }
                    }
                    catch { }

                    AppAlert.FormTips(gridMain, "设置所有列为系统列成功！", FormTipsType.Right);
                    return;
                }

                AppAlert.FormTips(gridMain, "设置所有列为系统列失败！");
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "设置表的所有列为系统列异常");
            }
        }
        /// <summary>
        /// 取消系统列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemCancelSystem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView topSelectItem = this.dataGridTop.SelectedItem as DataRowView;

                SQLParam param = new SQLParam()
                {

                    TableName = "Sys_TableCells",
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("IsSystem",false)
                    },
                    Wheres = new List<Where>()
                    {
                        new Where("ParentId", topSelectItem["Id"])
                    }
                };
                bool flag = SQLiteDao.Update(param);
                if (flag)
                {
                    try
                    {
                        //数据表
                        DataView dt = this.dataGridBottom.ItemsSource as DataView;
                        foreach (DataRow row in dt.Table.Rows)
                        {
                            row["IsSystem"] = false;
                        }
                    }
                    catch { }

                    AppAlert.FormTips(gridMain, "取消所有列为系统列成功！", FormTipsType.Right);
                    return;
                }

                AppAlert.FormTips(gridMain, "取消所有列为系统列失败！");
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "取消表的所有列为系统列异常");
            }
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPaste_Click(object sender, RoutedEventArgs e)
        {
            //复制的数据
            CopyDataInfo data = AppGlobal.RightKeyCopyData;
            if (data == null || data.TopRowData == null) return;
            if (data.TableInfo == null) return;

            if (data.TableInfo.Id != _tableConfig.Id)
            {
                AppAlert.FormTips(gridMain, "不同表无法粘贴！", FormTipsType.Info);
                return;
            }

            DataRowView topSelectItem = this.dataGridTop.SelectedItem as DataRowView;
            if (_tableConfig.Cells.Exists(p => p.CellName.Equals("IsAudit")))
            {
                //存在审核列， 判断是否审核
                bool isAudit = DataType.Bool(topSelectItem["IsAudit"], true);
                if (isAudit)
                {
                    AppAlert.FormTips(gridMain, "已审核无法粘贴！", FormTipsType.Info);
                    return;
                }
            }

            if (data.SubTableData == null)
            {
                //主表数据
                DataRow rowTop = data.TopRowData;

                //数据表
                _topPageResult.Data.Rows.Add(_topPageResult.Data.NewRow());
                DataRowView newRow = _topPageResult.Data.DefaultView[_topPageResult.Data.Rows.Count - 1];

                //新行
                newRow["Id"] = -DateTime.Now.ToFileTime();
                newRow[AppGlobal.DataGridNewStateCellName] = true;

                //默认行默认值
                SetDefaultValue(newRow, _tableConfig);

                foreach (CellInfo cell in _tableConfig.Cells)
                {
                    //过滤的列
                    if (AppGlobal.List_PasteFilterCells.Contains(cell.CellName.ToUpper())) continue;
                    //数据
                    newRow[cell.CellName] = rowTop[cell.CellName];
                }

                //主表设置编辑
                SetTopEdit(false);
            }
            else
            {
                //子表数据
                DataTable tableSub = data.SubTableData;

                //主表是否有选择项
                DataRowView topRow = this.dataGridTop.SelectedItem as DataRowView;
                if (topRow == null) return;
                long parentId = DataType.Long(topRow["Id"], 0);

                foreach (DataRow row in tableSub.Rows)
                {
                    //表行
                    _bottomPageResult.Data.Rows.Add(_bottomPageResult.Data.NewRow());
                    DataRowView newRow = _bottomPageResult.Data.DefaultView[_bottomPageResult.Data.Rows.Count - 1];

                    //默认行默认值
                    SetDefaultValue(newRow, _tableConfig.SubTable);

                    foreach (CellInfo cell in _tableConfig.SubTable.Cells)
                    {
                        //列维护
                        if (_tableConfig.SubTable.TableName.Equals(AppGlobal.SysTableName_TableCells))
                        {
                            if (cell.CellName.Equals("IsShow")) //是否显示
                            {
                                newRow[cell.CellName] = row[cell.CellName];
                                continue;
                            }

                            if (cell.CellName.Equals("IsSystem")) //是否系统列
                            {
                                newRow[cell.CellName] = false;
                                continue;
                            }

                            if (cell.CellName.Equals("CnName")) //列名（中文）
                            {
                                newRow[cell.CellName] = row[cell.CellName];
                                continue;
                            }
                        }

                        //过滤的列
                        if (AppGlobal.List_PasteFilterCells.Contains(cell.CellName.ToUpper())) continue;

                        //数据
                        newRow[cell.CellName] = row[cell.CellName];
                    }

                    //新行
                    newRow["Id"] = -DateTime.Now.ToFileTime();
                    newRow["ParentId"] = parentId;
                    newRow[AppGlobal.DataGridNewStateCellName] = true;
                }

                //设置子表编辑
                SetBottomEdit(false);
            }
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemCopy_Click(object sender, RoutedEventArgs e)
        {
            //没有选择项
            if (this.dataGridTop.SelectedItem == null) return;

            //当前选择的主表
            DataRow row = (this.dataGridTop.SelectedItem as DataRowView).Row;

            //子表数据
            DataTable dt = null;
            if (_bottomPageResult != null && _bottomPageResult.Data != null)
            {
                dt = _bottomPageResult.Data;
            }

            //复制的数据
            AppGlobal.RightKeyCopyData = new CopyDataInfo()
            {
                TopRowData = row,
                SubTableData = dt,
                TableInfo = _tableConfig
            };

            AppAlert.FormTips(gridMain, "复制成功！", FormTipsType.Right);
        }
        /// <summary>
        /// 复制单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemCopyCell_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(_topSelectCellText, false);
            AppAlert.FormTips(gridMain, "复制成功！", FormTipsType.Right);
        }
        /// <summary>
        /// 复制单元格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomItemCopyCell_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(_bottomSelectCellText, false);
            AppAlert.FormTips(gridMain, "复制成功！", FormTipsType.Right);
        }

        /// <summary>
        /// 保存列排序及宽度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveCellOrder_Click(object sender, RoutedEventArgs e)
        {
            //提示
            //AppAlert.FormTips(gridMain, "请稍候，正在保存列排序...", FormTipsType.Waiting);

            //显示等待
            ShowLoading(gridMain);

            //保存列排序及宽度
            SaveCellOrderAndWidth(true);
        }

        /// <summary>
        /// 上移一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopItemMoveUp_Click(object sender, RoutedEventArgs e)
        {
            //没有选择项
            if (this.dataGridTop.SelectedItem == null) return;

            //当前选择的行
            int selectRowIndex = this.dataGridTop.SelectedIndex;
            if (selectRowIndex < 1) return;

            DataRow row = (this.dataGridTop.SelectedItem as DataRowView).Row;

            //上表没有排序列
            if (!row.Table.Columns.Contains("DataIndex")) return;

            long zbid = row.GetLong("Id");
            if (zbid <= 0) return;

            long dataIndex = row.GetLong("DataIndex");
            DataRow[] frontRows = _topPageResult.Data.Select("[DataIndex]<" + dataIndex);
            if (frontRows.Length > 0)
            {
                var frontRows2 = frontRows.OrderByDescending(r => r["DataIndex"]).ToArray();
                long frontDataIndex = frontRows2[0].GetLong("DataIndex");

                string sql = "update [" + _tableConfig.TableName + "] set [DataIndex]=" + dataIndex + " where [DataIndex]=" + frontDataIndex + ";";
                sql += "update [" + _tableConfig.TableName + "] set [DataIndex]=" + frontDataIndex + " where [Id]=" + zbid + ";";

                SQLiteDao.ExecuteNonQuery(sql);

                ReloadZB();
            }
        }
        /// <summary>
        /// 下移一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopItemMoveDown_Click(object sender, RoutedEventArgs e)
        {
            //没有选择项
            if (this.dataGridTop.SelectedItem == null) return;

            //当前选择的行
            int selectRowIndex = this.dataGridTop.SelectedIndex;
            DataRow row = (this.dataGridTop.SelectedItem as DataRowView).Row;

            //上表没有排序列
            if (!row.Table.Columns.Contains("DataIndex")) return;

            long zbid = row.GetLong("Id");
            if (zbid <= 0) return;

            long dataIndex = row.GetLong("DataIndex");
            DataRow[] backRows = _topPageResult.Data.Select("[DataIndex]>" + dataIndex);
            if (backRows.Length > 0)
            {
                var backRows2 = backRows.OrderBy(r => r["DataIndex"]).ToArray();
                long backDataIndex = backRows2[0].GetLong("DataIndex");

                string sql = "update [" + _tableConfig.TableName + "] set [DataIndex]=" + dataIndex + " where [DataIndex]=" + backDataIndex + ";";
                sql += "update [" + _tableConfig.TableName + "] set [DataIndex]=" + backDataIndex + " where [Id]=" + zbid + ";";

                SQLiteDao.ExecuteNonQuery(sql);

                ReloadZB();
            }
        }
        /// <summary>
        /// 上移一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomItemMoveUp_Click(object sender, RoutedEventArgs e)
        {
            //没有选择项
            if (this.dataGridBottom.SelectedItem == null) return;

            //当前选择的行
            int selectRowIndex = this.dataGridBottom.SelectedIndex;
            if (selectRowIndex < 1) return;

            DataRow row = (this.dataGridBottom.SelectedItem as DataRowView).Row;

            //下表没有排序列
            if (!row.Table.Columns.Contains("DataIndex")) return;

            long parentid = row.GetLong("ParentId");
            long mxid = row.GetLong("Id");
            if (mxid <= 0) return;

            long dataIndex = row.GetLong("DataIndex");
            DataRow[] frontRows = _bottomPageResult.Data.Select("[DataIndex]<" + dataIndex);
            if (frontRows.Length > 0)
            {
                var frontRows2 = frontRows.OrderByDescending(r => r["DataIndex"]).ToArray();
                long frontDataIndex = frontRows2[0].GetLong("DataIndex");

                string sql = "update [" + _tableConfig.SubTable.TableName + "] set [DataIndex]=" + dataIndex + " where [ParentId]=" + parentid + " and [DataIndex]=" + frontDataIndex + ";";
                sql += "update [" + _tableConfig.SubTable.TableName + "] set [DataIndex]=" + frontDataIndex + " where [Id]=" + mxid + ";";

                SQLiteDao.ExecuteNonQuery(sql);

                ReloadCB();
            }
        }
        /// <summary>
        /// 下移一行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomItemMoveDown_Click(object sender, RoutedEventArgs e)
        {
            //没有选择项
            if (this.dataGridBottom.SelectedItem == null) return;

            //当前选择的行
            int selectRowIndex = this.dataGridBottom.SelectedIndex;
            DataRow row = (this.dataGridBottom.SelectedItem as DataRowView).Row;

            //下表没有排序列
            if (!row.Table.Columns.Contains("DataIndex")) return;

            long parentid = row.GetLong("ParentId");
            long mxid = row.GetLong("Id");
            if (mxid <= 0) return;

            long dataIndex = row.GetLong("DataIndex");
            DataRow[] backRows = _bottomPageResult.Data.Select("[DataIndex]>" + dataIndex);
            if (backRows.Length > 0)
            {
                var backRows2 = backRows.OrderBy(r => r["DataIndex"]).ToArray();
                long backDataIndex = backRows2[0].GetLong("DataIndex");

                string sql = "update [" + _tableConfig.TableName + "] set [DataIndex]=" + dataIndex + " where [DataIndex]=" + backDataIndex + ";";
                sql += "update [" + _tableConfig.TableName + "] set [DataIndex]=" + backDataIndex + " where [Id]=" + mxid + ";";

                SQLiteDao.ExecuteNonQuery(sql);

                ReloadCB();
            }
        }

        /// <summary>
        /// 主表 删除其它用户列排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopItemDeleteOtherUserColumnOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag = SQLiteDao.ExecuteSQL("delete from [Sys_UserTableConfigs] where [TableId]=" + _tableConfig.Id);
                this.Dispatcher.Invoke(new FlushClientBaseDelegate_Void(delegate ()
                {
                    if (flag)
                    {
                        AppAlert.FormTips(gridMain, "删除其它用户列排序成功！", FormTipsType.Right);
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "删除其它用户列排序失败！", FormTipsType.Info);
                    }
                }));
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 主表 设置列默认排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopItemSetColumnDefaultOrder_Click(object sender, RoutedEventArgs e)
        {
            string sql = "";

            foreach (DataGridColumn col in this.dataGridTop.Columns)
            {
                //列名
                string colName = col.SortMemberPath;
                //是否有此列
                CellInfo cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(colName));
                if (cellInfo == null) continue;

                int order = col.DisplayIndex;
                cellInfo.UserCellOrder = order;

                int width = 0;
                if (!col.Width.IsAuto)
                {
                    width = Convert.ToInt32(col.Width.Value);
                }

                sql += "update [Sys_TableCells] set [Order]=" + order + " where [ParentId]=" + _tableConfig.Id + " and [CellName]='" + colName + "';";
            }

            try
            {
                bool flag = SQLiteDao.ExecuteSQL(sql);
                this.Dispatcher.Invoke(new FlushClientBaseDelegate_Void(delegate ()
                {
                    if (flag)
                    {
                        AppAlert.FormTips(gridMain, "设置列默认排序成功！", FormTipsType.Right);
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "设置列默认排序失败！", FormTipsType.Info);
                    }
                }));
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 从表 删除其它用户列排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomItemDeleteOtherUserColumnOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag = SQLiteDao.ExecuteSQL("delete from [Sys_UserTableConfigs] where [TableId]=" + _tableConfig.SubTable.Id);
                this.Dispatcher.Invoke(new FlushClientBaseDelegate_Void(delegate ()
                {
                    if (flag)
                    {
                        AppAlert.FormTips(gridMain, "删除其它用户列排序成功！", FormTipsType.Right);
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "删除其它用户列排序失败！", FormTipsType.Info);
                    }
                }));
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 从表 设置列默认排序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomItemSetColumnDefaultOrder_Click(object sender, RoutedEventArgs e)
        {
            string sql = "";

            foreach (DataGridColumn col in this.dataGridBottom.Columns)
            {
                //列名
                string colName = col.SortMemberPath;
                //是否有此列
                CellInfo cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(colName));
                if (cellInfo == null) continue;

                int order = col.DisplayIndex;
                cellInfo.UserCellOrder = order;

                int width = 0;
                if (!col.Width.IsAuto)
                {
                    width = Convert.ToInt32(col.Width.Value);
                }

                sql += "update [Sys_TableCells] set [Order]=" + order + " where [ParentId]=" + _tableConfig.SubTable.Id + " and [CellName]='" + colName + "';";
            }

            try
            {
                bool flag = SQLiteDao.ExecuteSQL(sql);
                this.Dispatcher.Invoke(new FlushClientBaseDelegate_Void(delegate ()
                {
                    if (flag)
                    {
                        AppAlert.FormTips(gridMain, "设置列默认排序成功！", FormTipsType.Right);
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "设置列默认排序失败！", FormTipsType.Info);
                    }
                }));
            }
            catch (Exception ex) { }
        }

        #endregion

        #region 加载主表数据
        System.Threading.Thread _threadLoadZBData = null;
        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            //正在加载数据提示
            ShowLoading(gridMain);

            //结束上一个加载明细线程线程
            try { if (_threadLoadZBData != null) _threadLoadZBData.Abort(); }
            catch { }

            //线程加载数据
            _threadLoadZBData = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //加载数据
                    LoadingData();
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "加载主表数据异常");
                }
            });
            _threadLoadZBData.IsBackground = true;
            _threadLoadZBData.Start();
        }
        /// <summary>
        /// 加载主表数据
        /// </summary>
        public void LoadingData()
        {
            #region 表配置
            if (_tableConfig == null)
            {
                int loadCount = 0;

                while (true)
                {
                    try
                    {
                        //获取表配置
                        LoadTableConfig();

                        //加载表配置失败
                        if (_loadingTableConfigError) throw new Exception("加载表配置失败！");

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            try
                            {
                                //单表、双表操作时 默认不可对列进行排序
                                if (_tableConfig.IsRealTable)
                                {
                                    //不可拖动列排序
                                    this.dataGridTop.CanUserReorderColumns = false;
                                    this.dataGridBottom.CanUserReorderColumns = false;
                                }

                                //生成默认条件
                                BuildQueryWhere();
                            }
                            catch { }
                            return null;
                        }));
                    }
                    catch
                    {
                        loadCount++;
                        if (loadCount > 3)
                        {
                            //加载错误
                            LoadingTableConfigError();
                            return;
                        }
                    }

                    //加载成功
                    if (_tableConfig != null) break;
                }
            }
            #endregion

            #region 加载表配置失败
            //加载表配置失败
            if (_tableConfig == null || _loadingTableConfigError)
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    LoadingTableConfigError();
                    return null;
                }));
                return;
            }
            #endregion

            #region 加载操作按钮
            System.Threading.Thread threadLoadActions = new System.Threading.Thread(delegate ()
            {
                if (!_isLoadedActionsSuccess)
                {
                    int loadCount = 0;
                    while (true)
                    {
                        try
                        {
                            //加载操作按钮
                            LoadActions();
                            break;
                        }
                        catch
                        {
                            loadCount++;
                            if (loadCount > 3)
                            {
                                //加载错误
                                LoadingTableConfigError();
                                return;
                            }
                        }
                    }
                }
            });
            threadLoadActions.Start();
            #endregion

            #region 是否双表
            if (_firstInit && _tableConfig.SubTable != null && !_isPopWindow)
            {
                _firstInit = false;

                //没有设置显示比例时 初始默认
                if (_topBL == 1)
                {
                    _topBL = 0.5;
                    _bottomBL = 0.5;
                }

                InitSize();

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    this.dataGridBottom.Visibility = Visibility.Visible;
                    return null;
                }));
            }
            #endregion

            //读取表数据
            string tableName = _tableConfig.TableName;
            SQLParam param = new SQLParam()
            {
                TableName = tableName,
                PageSize = TopPageSize,
                PageIndex = _PageIndex,
                Wheres = new List<Where>(),
                WhereSQL = _tableConfig.Wheres
            };

            //查询条件
            if (_queryWheres != null && _queryWheres.Count > 0)
            {
                param.Wheres.AddRange(_queryWheres);
            }

            //扩展条件
            if (_extWheres != null && _extWheres.Count > 0)
            {
                param.Wheres.AddRange(_extWheres);
            }

            //过滤客户
            if (_tableConfig.IsFilterKHById)
            {
                if (_PostParams != null && _PostParams.TopRow != null && _PostParams.TopRow.Row.Table.Columns.Contains("KHID"))
                {
                    object val = _PostParams.TopRow["KHID"];
                    if (_tableConfig.Cells.Exists(p => p.CellName == "KHID")) param.Wheres.Add(new Where("KHID", val));
                    else if (_tableConfig.Cells.Exists(p => p.CellName == "MZZB_KHID")) param.Wheres.Add(new Where("MZZB_KHID", val));
                }
            }

            #region 排序
            //排序条件
            string dtOrderBy = string.Empty;
            _topOrderDefault = false;

            //是否有排序条件
            if (_topOrderBys != null && _topOrderBys.Count > 0)
            {
                param.OrderBys = _topOrderBys;
                dtOrderBy = _topOrderBysSql;
            }
            else
            {
                if ((_tableConfig.IsRealTable || _tableConfig.IsVTable))
                {
                    if (_tableConfig.TableOrderType == TableOrderType.顺序)
                    {
                        dtOrderBy = "[DataIndex] ASC";
                        param.OrderBys = new List<OrderBy>()
                        {
                            new OrderBy("DataIndex", OrderType.顺序)
                        };
                    }
                    else
                    {
                        _topOrderDefault = true;
                        dtOrderBy = "[DataIndex] ASC";
                        param.OrderBys = new List<OrderBy>()
                        {
                            new OrderBy("DataIndex", OrderType.倒序)
                        };
                    }
                }
            }
            #endregion

            DataTable data = new DataTable();
            bool loadDataError = false;
            bool isProcViewPaging = false;

            //隐藏等待加载数据
            HideLoading();
            //显示等待列表加载
            ShowListLoading("请稍候，正在加载主表数据...");

            try
            {
                //加载分页数据
                _topPageResult = QueryService.GetPaging(_tableConfig, AppGlobal.UserInfo, param);
                _topTableIndex = TopPageSize * (_PageIndex - 1);
                data = _topPageResult.Data;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "【" + _tableConfig.TableName + "】加载主表数据异常！");
                loadDataError = true;
            }

            //加载数据失败
            if (_topPageResult == null)
            {
                //加载数据错误
                if (loadDataError)
                {
                    LoadingDataError();
                }
                return;
            }

            try
            {
                //数据排序
                DataView dv = data.AsDataView();
                if (data != null && data.Rows.Count > 0) dv.Sort = dtOrderBy;
                data = dv.ToTable();
                _topPageResult.Data = data;
            }
            catch { }


            //显示完成
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //绑定表配置到数据表
                dataGridTop.ItemsSource = null;
                dataGridTop.ItemsSource = data.AsDataView();

                //主表加载成功
                _isLoadedTopData = true;

                //移除正在加载数据提示层
                if (_viewLoadDataTips != null)
                {
                    this.gridMain.Children.Remove(_viewLoadDataTips);
                }

                try
                {
                    //是否需要查询后的数据定位到最后一行
                    if (_isQueryDataFocusTable)
                    {
                        //有加载到数据
                        var queryFocusCell = _tableConfig.Cells.Find(p => p.IsQueryFocus && p.IsQuery);
                        if (queryFocusCell != null)
                        {
                            try
                            {
                                //查询焦点列                    
                                UIElement elementQueryCtl = (UIElement)this.panelQuerys.FindName(queryFocusCell.CellName);
                                if (elementQueryCtl != null)
                                {
                                    elementQueryCtl.Focus();
                                }
                            }
                            catch { }
                        }
                        else if (data != null && data.Rows.Count > 0)
                        {
                            try
                            {
                                //定位最后一行选中
                                int selectedIndex = data.Rows.Count - 1;
                                this.dataGridTop.Focus();
                                this.dataGridTop.SelectedIndex = selectedIndex;
                                this.dataGridTop.ScrollIntoView(this.dataGridTop.SelectedItem);
                                this.dataGridTop.ScrollIntoView(this.dataGridTop.Items[this.dataGridTop.Items.Count - 1]);
                                this.dataGridTop.GetCell(selectedIndex, 1)?.Focus();
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                //是否有分页
                bool hasPaging = false;
                if (!_tableConfig.BFY)
                {
                    if (_PageIndex == 1)
                    {
                        //隐藏分页栏
                        this.gridTopPage.Visibility = Visibility.Collapsed;
                    }

                    #region 有返回页数 生成分页按钮
                    if (_topPageResult != null && _topPageResult.PageCount > 1)
                    {
                        hasPaging = true;
                        isProcViewPaging = false;

                        //生成分页按钮
                        this.gridTopPage.Visibility = Visibility.Visible;
                        ShowPages(_topPageResult, panelTopPages, new ClickPage_Deletage(delegate (int pageIndex, int pageCount)
                        {
                            //点击分页
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                try
                                {
                                    if (pageIndex > pageCount)
                                    {
                                        AppAlert.FormTips(gridMain, "跳转失败，超过最大页数！", FormTipsType.Info);
                                        return null;
                                    }

                                    //当前分页
                                    _PageIndex = pageIndex;

                                    //查询数据聚焦到数据表
                                    _isQueryDataFocusTable = true;

                                    //重新加载数据
                                    LoadingData();

                                    //定位最后一行
                                    int selectedIndex = this.dataGridTop.Items.Count - 1;
                                    this.dataGridTop.SelectedIndex = selectedIndex;
                                    this.dataGridTop.ScrollIntoView(this.dataGridTop.SelectedItem);
                                    this.dataGridTop.ScrollIntoView(this.dataGridTop.Items[this.dataGridTop.SelectedIndex]);
                                    DataGridCell selectCell = this.dataGridTop.GetCell(selectedIndex, 1);//?.Focus();
                                    if (selectCell != null) selectCell.Focus();
                                }
                                catch (Exception ex)
                                {
                                    ErrorHandler.AddException(ex, "【" + _tableConfig.TableName + "】生成分页按钮异常");
                                }

                                return null;
                            }));
                        }), 6);
                    }
                    #endregion

                    #region 过程视图分页
                    //显示过程视图分页
                    if (isProcViewPaging && (_PageIndex > 1 || (_topPageResult != null && _topPageResult.Data.Rows.Count >= TopPageSize)))
                    {
                        hasPaging = true;

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            try
                            {
                                //显示分页栏
                                this.gridTopPage.Visibility = Visibility.Visible;

                                if (!_isInitViewPaging)
                                {
                                    //清空分页按钮
                                    this.panelTopPages.Children.Clear();

                                    //样式
                                    Style defaultBorder = this.FindResource("borderPageIndex") as Style;
                                    Style currentBorder = this.FindResource("borderPageIndex_Current") as Style;

                                    Style defaultLabel = this.FindResource("lblPageIndex") as Style;
                                    Style currentLabel = this.FindResource("lblPageIndex_Current") as Style;

                                    Border borderFirst = new Border();
                                    TextBlock lblFirst = new TextBlock();
                                    lblFirst.Text = "首页";
                                    lblFirst.Style = defaultLabel;
                                    lblFirst.Margin = new Thickness(10, 0, 10, 0);
                                    borderFirst.Visibility = Visibility.Collapsed;
                                    borderFirst.Style = defaultBorder;
                                    borderFirst.Child = lblFirst;
                                    borderFirst.MouseLeftButtonDown += BorderFirst_MouseLeftButtonDown;

                                    Border borderPrev = new Border();
                                    TextBlock lblPrev = new TextBlock();
                                    lblPrev.Text = "上一页";
                                    lblPrev.Style = defaultLabel;
                                    lblPrev.Margin = new Thickness(10, 0, 10, 0);
                                    borderPrev.Visibility = Visibility.Collapsed;
                                    borderPrev.Style = defaultBorder;
                                    borderPrev.Child = lblPrev;
                                    borderPrev.MouseLeftButtonDown += BorderPrev_MouseLeftButtonDown;

                                    this.panelTopPages.RegisterName("borderFirstPage", borderFirst);
                                    this.panelTopPages.RegisterName("borderPrevPage", borderPrev);

                                    this.panelTopPages.Children.Add(borderFirst);
                                    this.panelTopPages.Children.Add(borderPrev);


                                    Border borderCurrent = new Border();
                                    TextBlock lblCurrentPageIndex = new TextBlock();
                                    lblCurrentPageIndex.Text = _PageIndex.ToString();
                                    lblCurrentPageIndex.Style = defaultLabel;
                                    borderCurrent.Style = defaultBorder;
                                    borderCurrent.Child = lblCurrentPageIndex;
                                    this.panelTopPages.RegisterName("lblCurrentPageIndex", lblCurrentPageIndex);

                                    Border borderNext = new Border();
                                    TextBlock lblNext = new TextBlock();
                                    lblNext.Text = "下一页";
                                    lblNext.Style = defaultLabel;
                                    lblNext.Margin = new Thickness(10, 0, 10, 0);
                                    borderNext.Style = defaultBorder;
                                    borderNext.Child = lblNext;
                                    borderNext.MouseLeftButtonDown += BorderNext_MouseLeftButtonDown;

                                    this.panelTopPages.Children.Add(borderCurrent);
                                    this.panelTopPages.Children.Add(borderNext);

                                    _isInitViewPaging = true;
                                }

                                if (_PageIndex > 1)
                                {
                                    //显示首页
                                    Border borderFirstPage = this.panelTopPages.FindName("borderFirstPage") as Border;
                                    borderFirstPage.Visibility = Visibility.Visible;

                                    //显示上一页
                                    Border borderPrevPage = this.panelTopPages.FindName("borderPrevPage") as Border;
                                    borderPrevPage.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    //不显示首页
                                    Border borderFirstPage = this.panelTopPages.FindName("borderFirstPage") as Border;
                                    borderFirstPage.Visibility = Visibility.Collapsed;

                                    //不显示上一页
                                    Border borderPrevPage = this.panelTopPages.FindName("borderPrevPage") as Border;
                                    borderPrevPage.Visibility = Visibility.Collapsed;
                                }

                                //当前页数
                                TextBlock lblCurrentPageIndex2 = this.panelTopPages.FindName("lblCurrentPageIndex") as TextBlock;
                                lblCurrentPageIndex2.Text = _PageIndex.ToString();
                            }
                            catch (Exception ex)
                            {
                                ErrorHandler.AddException(ex, "【" + _tableConfig.TableName + "】生成过程视图分页异常");
                            }

                            return null;
                        }));
                    }
                    #endregion
                }

                //列表是否有滚动条
                try
                {
                    System.Windows.Automation.Peers.DataGridAutomationPeer lvap = new System.Windows.Automation.Peers.DataGridAutomationPeer(this.dataGridTop);
                    var svap = lvap.GetPattern(System.Windows.Automation.Peers.PatternInterface.Scroll) as System.Windows.Automation.Peers.ScrollViewerAutomationPeer;
                    var scroll = svap.Owner as ScrollViewer;

                    double marginRightMargin = 0;

                    double pagingBottomMargin = 0;
                    double imgPreviewBottomMargin = 0;

                    if (hasPaging)
                    {//有分页栏
                        imgPreviewBottomMargin = 45;

                        if (scroll.ViewportWidth < scroll.ExtentWidth)
                        {
                            //水平滚动条显示
                            pagingBottomMargin = 20;
                            imgPreviewBottomMargin += pagingBottomMargin;
                        }
                        if (scroll.ViewportHeight <= scroll.ExtentHeight)
                        {
                            //垂直滚动条显示
                            marginRightMargin = 20;
                        }
                    }
                    else
                    {//无分页栏
                        if (scroll.ViewportWidth < scroll.ExtentWidth)
                        {
                            //水平滚动条显示
                            imgPreviewBottomMargin = 20;
                        }
                        if (scroll.ViewportHeight <= scroll.ExtentHeight)
                        {
                            //垂直滚动条显示
                            marginRightMargin = 20;
                        }
                    }

                    this.gridTopPage.Margin = new Thickness(0, 0, marginRightMargin, pagingBottomMargin);
                    this.imgTopPreview.Margin = new Thickness(0, 0, marginRightMargin, imgPreviewBottomMargin);
                }
                catch { }

                //隐藏正在加载中
                HideLoading();
                //隐藏列表加载中
                HideListLoading();
                return null;
            }));
        }


        #region 视图分页
        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderFirst_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _PageIndex = 1;
            LoadingData();
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderPrev_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_PageIndex <= 1) return;
            _PageIndex--;
            LoadingData();
        }
        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderNext_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_topPageResult == null || _topPageResult.Data == null || _topPageResult.Data.Rows.Count < TopPageSize)
            {
                AppAlert.FormTips(gridMain, "已经没有更多数据了！", FormTipsType.Info);
                return;
            }

            _PageIndex++;
            LoadingData();
        }
        #endregion

        /// <summary>
        /// 加载数据失败
        /// </summary>
        private void LoadingDataError()
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.panelLoadDataError.Visibility = Visibility.Visible;
                this.borderActions.Visibility = Visibility.Collapsed;

                this.dataGridTop.Visibility = Visibility.Collapsed;
                this.dataGridBottom.Visibility = Visibility.Collapsed;

                HideLoading();

                HideListLoading();

                return null;
            }));
        }
        /// <summary>
        /// 加载表配置失败
        /// </summary>
        private void LoadingTableConfigError()
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.panelLoadTableError.Visibility = Visibility.Visible;
                this.borderActions.Visibility = Visibility.Collapsed;

                this.dataGridTop.Visibility = Visibility.Collapsed;
                this.dataGridBottom.Visibility = Visibility.Collapsed;

                //隐藏等待
                HideLoading();

                //隐藏列表等待
                HideListLoading();

                return null;
            }));
        }
        #endregion

        #region 加载子表数据
        System.Threading.Thread _threadLoadMXData = null;
        /// <summary>
        /// 加载明细数据
        /// </summary>
        /// <param name="id"></param>
        private void LoadDetails(long id)
        {
            //单表
            if (id <= 0) return;
            if (_tableConfig.IsSingleTable || _tableConfig.SubTable == null) return;

            //选择多项时不加载数据
            if (this.dataGridTop.SelectedItems != null && this.dataGridTop.SelectedItems.Count > 1)
            {
                //清空子表数据
                this.dataGridBottom.ItemsSource = null;
                return;
            }

            //结束上一个加载明细线程线程
            try { if (_threadLoadMXData != null) _threadLoadMXData.Abort(); }
            catch { }

            //线程加载数据
            _threadLoadMXData = new System.Threading.Thread(delegate ()
            {
                LoadingDetails(id);
            });
            _threadLoadMXData.IsBackground = true;
            _threadLoadMXData.Start();
        }
        /// <summary>
        /// 加载明细表数据
        /// </summary>
        public void LoadingDetails(long id)
        {
            //加载表配置失败
            if (_tableConfig == null || string.IsNullOrWhiteSpace(_tableConfig.SubTable.TableName))
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //隐藏加载中
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                    return null;
                }));
                return;
            }

            //关联列名
            string connectCellName = string.Empty;

            string tableName = _tableConfig.SubTable.TableName;
            SQLParam param = new SQLParam()
            {
                TableName = tableName,
                PageSize = BottomPageSize,
                PageIndex = _PageIndexSub,
                Wheres = new List<Where>()
                {
                    new Where("ParentId", id)
                },
                WhereSQL = _tableConfig.SubTable.Wheres,
                OrderBys = new List<OrderBy>(),
            };

            //搜索过滤数据
            if (!string.IsNullOrWhiteSpace(_bottomSearchKeywords))
            {
                param.Wheres.Add(new Where() { Type = WhereType.左括号 });
                if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SearchKeywords")))
                {
                    //是否有搜索关键字列
                    param.Wheres.Add(new Where("SearchKeywords", _bottomSearchKeywords, WhereType.模糊查询, ParallelType.Or));
                }
                if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SPMC")))
                {
                    //是否有商品名称列
                    param.Wheres.Add(new Where("SPMC", _bottomSearchKeywords, WhereType.模糊查询, ParallelType.Or));
                }
                if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SPBH")))
                {
                    //是否有商品编号列
                    param.Wheres.Add(new Where("SPBH", _bottomSearchKeywords, WhereType.模糊查询, ParallelType.Or));
                }
                param.Wheres.Add(new Where() { Type = WhereType.右括号 });
            }

            #region 排序
            //排序条件
            string dtOrderBy = string.Empty;
            _bottomOrderDefault = false;

            //是否有排序条件
            if (_bottomOrderBys != null && _bottomOrderBys.Count > 0)
            {
                param.OrderBys = _bottomOrderBys;
                dtOrderBy = _bottomOrderBysSql;
            }
            else
            {
                //排序
                if (tableName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    param.OrderBys.Add(new OrderBy()
                    {
                        CellName = "DataIndex",
                        Type = OrderType.顺序
                    });
                }
                else if (_tableConfig.SubTable.TableOrderType == TableOrderType.顺序)
                {
                    param.OrderBys.Add(new OrderBy()
                    {
                        CellName = "DataIndex",
                        Type = OrderType.顺序
                    });
                    dtOrderBy = "[DataIndex] ASC";
                }
                else
                {
                    _bottomOrderDefault = true;
                    param.OrderBys.Add(new OrderBy()
                    {
                        CellName = "DataIndex",
                        Type = OrderType.倒序
                    });
                    dtOrderBy = "[DataIndex] ASC";
                }
            }
            #endregion

            //隐藏等待加载数据
            HideLoading();
            //显示等待列表加载
            ShowListLoading("请稍候，正在加载主表数据...");

            DataTable data = new DataTable();
            try
            {
                //得到数据
                var mxPageResult = QueryService.GetPaging(_tableConfig.SubTable, AppGlobal.UserInfo, param);

                //明细表数据
                if (mxPageResult.Data != null &&
                    mxPageResult.Data.Rows.Count > 0 &&
                    mxPageResult.Data.Columns.Contains("ParentId"))
                {
                    long zbSelectId = 0;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        DataRowView rowZB = (this.dataGridTop.SelectedItem as DataRowView);
                        zbSelectId = DataType.Long(rowZB["Id"], 0);
                    }));

                    if (zbSelectId > 0 && zbSelectId != mxPageResult.Data.Rows[0].GetParentId())
                    {
                        //当前ParentId不等于选择主表Id，数据无效
                        return;
                    }
                }

                _bottomPageResult = mxPageResult;

                _bottomTableIndex = BottomPageSize * (_PageIndexSub - 1);
                data = _bottomPageResult.Data;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "【" + _tableConfig.SubTable.TableName + "】调用服务加载数据失败！");
            }

            try
            {
                //数据排序
                DataView dv = data.AsDataView();
                if (data != null && data.Rows.Count > 0) dv.Sort = dtOrderBy;
                data = dv.ToTable();
                _bottomPageResult.Data = data;
            }
            catch { }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //隐藏等待加载
                HideListLoading();
                //绑定表配置到数据表
                dataGridBottom.ItemsSource = null;
                //加载数据失败
                if (_bottomPageResult == null) return null;

                dataGridBottom.ItemsSource = data.AsDataView();
                _isLoadedBottomData = true;

                //移除正在加载数据提示层
                if (_viewLoadDataTips != null)
                {
                    this.gridMain.Children.Remove(_viewLoadDataTips);
                    _viewLoadDataTips = null;
                }

                bool hasPaging = false;

                if (!_tableConfig.SubTable.BFY)
                {
                    if (_PageIndexSub == 1)
                    {
                        //隐藏分页按钮
                        hasPaging = false;
                        this.gridBottomPage.Visibility = Visibility.Collapsed;
                    }

                    if (_bottomPageResult != null && _bottomPageResult.PageCount > 1)
                    {
                        //生成分页按钮
                        hasPaging = true;
                        this.gridBottomPage.Visibility = Visibility.Visible;
                        ShowPages(_bottomPageResult, this.panelBottomPages, new ClickPage_Deletage(delegate (int pageIndex, int pageCount)
                        {
                            //点击分页
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                try
                                {
                                    if (pageIndex > pageCount)
                                    {
                                        AppAlert.FormTips(gridMain, "跳转失败，超过最大页数！", FormTipsType.Info);
                                        return null;
                                    }

                                    //当前分页
                                    _PageIndexSub = pageIndex;

                                    //重新加载数据
                                    LoadingDetails(id);
                                }
                                catch (Exception ex)
                                {
                                    ErrorHandler.AddException(ex, "【" + _tableConfig.SubTable.TableName + "】生成分页按钮失败");
                                }

                                return null;
                            }));
                        }), 6);
                    }
                }

                //列表是否显示滚动条
                try
                {
                    System.Windows.Automation.Peers.DataGridAutomationPeer lvap = new System.Windows.Automation.Peers.DataGridAutomationPeer(this.dataGridBottom);
                    var svap = lvap.GetPattern(System.Windows.Automation.Peers.PatternInterface.Scroll) as System.Windows.Automation.Peers.ScrollViewerAutomationPeer;
                    var scroll = svap.Owner as ScrollViewer;

                    double marginRightMargin = 0;

                    double pagingBottomMargin = 0;
                    double imgPreviewBottomMargin = 0;

                    if (hasPaging)
                    {//有分页栏
                        imgPreviewBottomMargin = 45;
                        if (scroll.ViewportWidth < scroll.ExtentWidth)
                        {
                            //水平滚动条显示
                            pagingBottomMargin = 20;
                            imgPreviewBottomMargin += pagingBottomMargin;
                        }

                        if (scroll.ViewportHeight <= scroll.ExtentHeight)
                        {
                            //垂直滚动条显示
                            marginRightMargin = 20;
                        }
                    }
                    else
                    {//没有分页栏
                        if (scroll.ViewportWidth < scroll.ExtentWidth)
                        {
                            //水平滚动条显示
                            imgPreviewBottomMargin = 20;
                        }

                        if (scroll.ViewportHeight <= scroll.ExtentHeight)
                        {
                            //垂直滚动条显示
                            marginRightMargin = 20;
                        }
                    }

                    this.gridBottomPage.Margin = new Thickness(0, 0, marginRightMargin, pagingBottomMargin);
                    this.imgBottomPreview.Margin = new Thickness(0, 0, marginRightMargin, imgPreviewBottomMargin);
                }
                catch { }

                //隐藏加载中
                HideLoading();
                //隐藏列表等待
                HideListLoading();
                return null;
            }));
        }
        #endregion

        #region 设置返回行值
        /// <summary>
        /// 返回的行
        /// </summary>
        /// <param name="returnRow"></param>
        /// <param name="cellName"></param>
        private void SetReturnRow(DataRow returnRow, CellInfo cellInfo, bool isNewRow = false)
        {
            int rowIndex = 0;
            DataGrid dataGrid = null;
            DataRowView row = null;
            List<CellInfo> dataGridCells = null;
            TableInfo tableInfo = null;

            //返回分组
            int cellReturnGroup = cellInfo.ReturnGroup;

            bool isBreak = false;

            try
            {
                //更新表
                if (_isChooseTop)
                {
                    //主表
                    dataGrid = this.dataGridTop;
                    //选择索引
                    rowIndex = dataGrid.SelectedIndex;
                    //所有列
                    dataGridCells = _tableConfig.Cells;

                    tableInfo = _tableConfig;

                    if (isNewRow)
                    {
                        //新行
                        _topPageResult.Data.Rows.Add(_topPageResult.Data.NewRow());
                        row = _topPageResult.Data.DefaultView[_topPageResult.Data.Rows.Count - 1];

                        SetDefaultValue(row, _tableConfig);

                        row["Id"] = -DateTime.Now.ToFileTime();
                        row[AppGlobal.DataGridNewStateCellName] = true;

                        //是否表维护
                        isBreak = IsTableEditReturnRow(_tableConfig.TableName, cellInfo, row, returnRow, true);

                        if (!isBreak)
                        {
                            //设置新行的值
                            SetReturnNewRowValues(cellReturnGroup, dataGridCells, row, returnRow);
                            //this._bottomPageResult.Data.Rows.Add(row.Row.ItemArray);
                        }

                        try
                        {
                            //标记编辑
                            DataGridPlus.SetRowEdit(dataGrid, dataGrid.Items.Count - 1, false);
                        }
                        catch (Exception ex) { }
                        return;
                    }

                    //选择的行
                    row = dataGrid.SelectedItem as DataRowView;

                    //是否表维护
                    isBreak = IsTableEditReturnRow(_tableConfig.TableName, cellInfo, row, returnRow);
                }
                else
                {
                    //子表
                    dataGrid = this.dataGridBottom;
                    //选择子表索引
                    rowIndex = dataGrid.SelectedIndex;
                    //所有列
                    dataGridCells = _tableConfig.SubTable.Cells;

                    tableInfo = _tableConfig.SubTable;

                    //单价历史
                    if (_tableConfig.Cells.Exists(p => p.CellName.Equals("KHID")) &&
                        _tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("DJ")) &&
                        _tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SPID")) &&
                        returnRow.Table.Columns.Contains("SPID"))
                    {
                        //单价历史
                        SetPriceHistory(returnRow);
                    }

                    if (isNewRow)
                    {
                        //新行
                        _bottomPageResult.Data.Rows.Add(_bottomPageResult.Data.NewRow());
                        row = _bottomPageResult.Data.DefaultView[_bottomPageResult.Data.Rows.Count - 1];

                        SetDefaultValue(row, _tableConfig.SubTable);

                        row["Id"] = -DateTime.Now.ToFileTime();
                        row[AppGlobal.DataGridNewStateCellName] = true;

                        //是否表维护
                        isBreak = IsTableEditReturnRow(_tableConfig.SubTable.TableName, cellInfo, row, returnRow, true);

                        if (!isBreak)
                        {
                            //设置新行的值
                            SetReturnNewRowValues(cellReturnGroup, dataGridCells, row, returnRow);
                            //this._bottomPageResult.Data.Rows.Add(row.Row.ItemArray);
                        }

                        try
                        {
                            //标记编辑
                            DataGridPlus.SetRowEdit(dataGrid, dataGrid.Items.Count - 1, false);
                        }
                        catch (Exception ex) { }
                        return;
                    }

                    //选择的行
                    row = dataGrid.SelectedItem as DataRowView;

                    //是否表维护
                    isBreak = IsTableEditReturnRow(_tableConfig.SubTable.TableName, cellInfo, row, returnRow);

                }
            }
            catch (Exception ex) { }

            //标记已经编辑
            row[AppGlobal.DataGridEditStateCellName] = true;

            //不继续执行
            if (isBreak) return;

            try
            {
                VisualTreePlus plus = new VisualTreePlus();
                DataGridRow rowContainer = dataGrid.GetRow(rowIndex);
                DataGridCellsPresenter presenter = DataGridPlus.GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter != null)
                {
                    for (int cIndex = 0; cIndex < dataGrid.Columns.Count; cIndex++)
                    {
                        //操作列名
                        string colName = dataGrid.Columns[cIndex].SortMemberPath;
                        if (string.IsNullOrWhiteSpace(colName) || AppGlobal.List_ReturnRowFilterCells.Contains(colName.ToUpper())) continue;

                        //得到单元格
                        DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                        if (cell == null)
                        {
                            dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[cIndex]);
                            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                        }
                        //是否得到单元格
                        //if (cell == null) continue;

                        //是否有指定返回列
                        string returnColName = colName;
                        //是否有指定返回的表名
                        string returnTableName = string.Empty;

                        try
                        {
                            CellInfo currentCellInfo = dataGridCells.Find(p => p.CellName.Equals(colName));
                            if (currentCellInfo != null)
                            {
                                //指定默认值
                                if (!string.IsNullOrWhiteSpace(currentCellInfo.ReturnCellName))
                                {
                                    //指定返回的列名
                                    returnColName = currentCellInfo.ReturnCellName;
                                    //指定返回的表名
                                    returnTableName = currentCellInfo.ForeignTableName;

                                    if (!returnRow.Table.Columns.Contains(returnColName))
                                    {
                                        //不存在指定的列名
                                        returnColName = colName;
                                    }
                                }

                                if (currentCellInfo.DefaultValue.StartsWith("(User.") || currentCellInfo.DefaultValue.StartsWith("(Sys.") || currentCellInfo.DefaultValue.StartsWith("(Date.")) continue;
                            }

                            if (cellReturnGroup > 0)
                            {
                                //有指定返回分组
                                if (currentCellInfo == null || currentCellInfo.ReturnGroup != cellReturnGroup) continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.AddException(ex, "指定返回异常");
                        }


                        //返回行不包含列
                        if (!returnRow.Table.Columns.Contains(returnColName)) continue;
                        //是否指定返回的表
                        if (!string.IsNullOrWhiteSpace(returnTableName) && !returnRow.Table.TableName.Equals(returnTableName)) continue;

                        //赋值
                        row[colName] = returnRow[returnColName];

                        if (cell != null)
                        {
                            //选择无标记无需编辑
                            if (colName.ToUpper().Equals(cellInfo.CellName)) cell.IsEditing = false;

                            //所有子控件
                            List<DependencyObject> ctlSubs = plus.GetChildren(cell);

                            //赋值
                            foreach (DependencyObject c in ctlSubs)
                            {
                                if (c is Button)
                                {
                                    continue;
                                }
                                else if (c is TextBlock)
                                {
                                    TextBlock lbl = c as TextBlock;
                                    if (lbl.Name.Equals("btnText")) continue;
                                    lbl.Text = returnRow[returnColName].ToString();
                                }
                                else if (c is TextBox)
                                {
                                    TextBox txt = c as TextBox;
                                    txt.Text = returnRow[returnColName].ToString();
                                }
                            }

                            //标记无需编辑
                            cell.IsEditing = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "返回赋值异常");
            }

            try
            {
                DataGridRow dgrow = dataGrid.GetRow(rowIndex);

                var cellDJ = tableInfo.Cells.Find(p => p.CellName.Equals("DJ"));
                var cellSL = tableInfo.Cells.Find(p => p.CellName.Equals("SL"));
                var cellJE = tableInfo.Cells.Find(p => p.CellName.Equals("JE"));

                if (cellDJ != null && cellSL != null && cellJE != null)
                {
                    decimal dj = DataType.Decimal(row["DJ"], 0);
                    decimal sl = DataType.Decimal(row["SL"], 0);

                    row["JE"] = dj * sl;
                    //设置单元格值
                    SetGridCellValue(dataGrid, dgrow, "JE", row["JE"]);
                }

                //重新计算单元格公式值
                foreach (CellInfo cell in tableInfo.Cells)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(cell.Formula))
                        {
                            //计算公式
                            JSGS(cell, row);
                            //设置单元格值
                            SetGridCellValue(dataGrid, dgrow, cell.CellName, row[cell.CellName]);
                        }

                        //计算公式
                        CellInfo cellJSGS = tableInfo.Cells.Find(p => p.CellName.Equals(cell.CellName + "_JSGS"));
                        if (cellJSGS != null)
                        {
                            var objJSGS = row[cell.CellName + "_JSGS"];
                            if (objJSGS == null || string.IsNullOrWhiteSpace(objJSGS.ToString())) continue;

                            //计算公式后的值
                            JSGS(cell, objJSGS.ToString(), row);
                            //设置单元格值
                            SetGridCellValue(dataGrid, dgrow, cell.CellName, row[cell.CellName]);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(cellInfo.ReturnCellName) && returnRow.Table.Columns.Contains(cellInfo.ReturnCellName))
            {
                //返回对应的列名
                row[cellInfo.CellName] = returnRow[cellInfo.ReturnCellName];
            }
            else if (cellInfo.ValType.Equals("long") && returnRow.Table.Columns.Contains("Id"))
            {
                //Id值
                row[cellInfo.CellName] = returnRow["Id"];
            }
        }
        /// <summary>
        /// 设置返回新行的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="returnRow"></param>
        private void SetReturnNewRowValues(int cellReturnGroup, List<CellInfo> cells, DataRowView row, DataRow returnRow)
        {
            foreach (CellInfo cell in cells)
            {
                try
                {
                    string cellName = cell.CellName;
                    string returnColName = cellName;
                    string returnTableName = cell.ForeignTableName;

                    //是否有接收列名
                    if (string.IsNullOrWhiteSpace(cellName) || AppGlobal.List_ReturnRowFilterCells.Contains(cellName.ToUpper())) continue;

                    //返回对应列
                    if (!string.IsNullOrWhiteSpace(cell.ReturnCellName)) returnColName = cell.ReturnCellName.Trim();
                    //返回指定表
                    if (!string.IsNullOrWhiteSpace(returnTableName) && !returnRow.Table.TableName.Equals(returnTableName)) continue;
                    //返回指定分组
                    if (cellReturnGroup > 0 && (cell.ReturnGroup != cellReturnGroup)) continue;

                    //返回行不包含列
                    if (!returnRow.Table.Columns.Contains(returnColName)) continue;

                    //指定默认值
                    if (cell.DefaultValue.StartsWith("(User.") || cell.DefaultValue.StartsWith("(Sys.") || cell.DefaultValue.StartsWith("(Date.")) continue;

                    //赋值
                    row[cellName] = returnRow[returnColName];
                }
                catch { }
            }
        }
        /// <summary>
        /// 是否表维护返回行
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cellInfo"></param>
        /// <param name="row"></param>
        /// <param name="returnRow"></param>
        /// <returns></returns>
        private bool IsTableEditReturnRow(string tableName, CellInfo cellInfo, DataRowView row, DataRow returnRow, bool isNew = false)
        {
            //表维护不替换
            if (tableName.Equals(AppGlobal.SysTableName_Tables) ||
                tableName.Equals(AppGlobal.SysTableName_TableCells))
            {
                if ((cellInfo.CellName.Equals("ParentId") || cellInfo.CellName.Equals("ParentTableName")) && tableName.Equals(AppGlobal.SysTableName_Tables))
                {
                    //表返回行
                    row["ParentId"] = returnRow["Id"];
                    row["ParentTableName"] = returnRow["TableName"];

                    if (!isNew)
                    {
                        SetTableReturnCell(true, returnRow, "ParentTableName", "TableName");
                        SetTableReturnCell(true, returnRow, "ParentId", "Id");
                        //if (cellInfo.CellName.Equals("ParentId")) SetTableReturnCell(true, returnRow, "ParentTableName", "TableName");
                        //else if (cellInfo.CellName.Equals("ParentTableName")) SetTableReturnCell(true, returnRow, "ParentId", "Id");
                    }
                }
                else if ((cellInfo.CellName.Equals("ForeignTableId") || cellInfo.CellName.Equals("ForeignTableName")) && tableName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    //表列返回行
                    row["ForeignTableId"] = returnRow["Id"];
                    row["ForeignTableName"] = returnRow["TableName"];

                    if (!isNew)
                    {
                        SetTableReturnCell(false, returnRow, "ForeignTableName", "TableName");
                        SetTableReturnCell(false, returnRow, "ForeignTableId", "Id");

                        //if (cellInfo.CellName.Equals("ForeignTableId")) SetTableReturnCell(false, returnRow, "ForeignTableName", "TableName");
                        //else if (cellInfo.CellName.Equals("ForeignTableName")) SetTableReturnCell(false, returnRow, "ForeignTableId", "Id");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(cellInfo.ReturnCellName))
                {
                    //返回对应的列名
                    row[cellInfo.CellName] = returnRow[cellInfo.ReturnCellName];
                }
                else if (!cellInfo.ValType.Equals("long") && returnRow.Table.Columns.Contains(cellInfo.CellName))
                {
                    //返回相同的列名
                    row[cellInfo.CellName] = returnRow[cellInfo.CellName];
                }
                else
                {
                    row[cellInfo.CellName] = returnRow["Id"];
                }
                return true;
            }

            return false;
        }
        /// <summary>
        /// 设置表维护返回列值
        /// </summary>
        /// <param name="isTopTable"></param>
        /// <param name="returnRow"></param>
        /// <param name="setColName"></param>
        /// <param name="returnColName"></param>
        private void SetTableReturnCell(bool isTopTable, DataRow returnRow, string setColName, string returnColName)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                VisualTreePlus plus = new VisualTreePlus();
                DataGrid dataGrid = isTopTable ? this.dataGridTop : this.dataGridBottom;
                int rowIndex = dataGrid.SelectedIndex;
                DataGridRow rowContainer = dataGrid.GetRow(rowIndex);
                DataGridCellsPresenter presenter = DataGridPlus.GetVisualChild<DataGridCellsPresenter>(rowContainer);

                for (int cIndex = 0; cIndex < dataGrid.Columns.Count; cIndex++)
                {
                    //单元格 数据列名
                    string colName = dataGrid.Columns[cIndex].SortMemberPath;
                    if (!colName.Equals(setColName)) continue;

                    //得到单元格
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                    if (cell == null)
                    {
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[cIndex]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(cIndex);
                    }
                    //是否得到单元格
                    if (cell == null) continue;

                    //所有子控件
                    List<DependencyObject> ctlSubs = plus.GetChildren(cell);

                    //赋值
                    foreach (DependencyObject c in ctlSubs)
                    {
                        try
                        {
                            if (c is Button)
                            {
                                continue;
                            }
                            else if (c is TextBlock)
                            {
                                TextBlock lbl = c as TextBlock;
                                if (lbl.Name.Equals("btnText")) continue;
                                lbl.Text = returnRow[returnColName].ToString();
                            }
                            else if (c is TextBox)
                            {
                                TextBox txt = c as TextBox;
                                txt.Text = returnRow[returnColName].ToString();
                            }
                        }
                        catch (Exception ex) { }
                    }

                    //标记无需编辑
                    cell.IsEditing = false;
                    break;
                }
                return null;
            }));
        }
        /// <summary>
        /// 设置返回行历史价格
        /// </summary>
        private void SetPriceHistory(DataRow returnRow)
        {
            //是否启动历史价格功能
            bool enablePriceHistorys = AppGlobal.GetSysConfigReturnBool("EnablePriceHistorys");
            bool enablePriceHistorys2 = AppGlobal.GetSysConfigReturnBool("System_EnablePriceHistorys");
            if (!enablePriceHistorys && !enablePriceHistorys2) return;

            long spid = 0;

            try
            {
                //默认值
                if (returnRow.Table.Columns.Contains("SPID")) spid = DataType.Long(returnRow["SPID"], 0);

                //没有商品编号
                if (spid == 0) return;

                //客户编号
                DataRowView rowTop = this.dataGridTop.SelectedItem as DataRowView;
                long khid = DataType.Long(rowTop["KHID"], 0);

                //客户编号列关联的表编号
                CellInfo khidCell = _tableConfig.Cells.Find(p => p.CellName.Equals("KHID"));
                var newDJ = QueryService.GetHistoryPrice(khidCell.ForeignTableId, khid, spid);
                if (DataType.Decimal(newDJ, 0) <= 0) return;

                if (returnRow.Table.Columns.Contains("DJ")) returnRow["DJ"] = newDJ;
                else
                {
                    returnRow.Table.Columns.Add(new DataColumn("DJ", typeof(decimal)));
                    returnRow["DJ"] = newDJ;
                }

                if (returnRow.Table.Columns.Contains("MZMX_DJ")) returnRow["MZMX_DJ"] = newDJ;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "设置返回行历史价格异常");
            }
        }
        #endregion

        #region 设置行默认值
        /// <summary>
        /// 设置添加行默认值
        /// </summary>
        /// <param name="info"></param>
        private void SetDefaultValue(DataRowView rv, TableInfo info)
        {
            SetDefaultValue(rv.Row, info);
        }
        /// <summary>
        /// 设置添加行默认值
        /// </summary>
        /// <param name="rv"></param>
        /// <param name="info"></param>
        private void SetDefaultValue(DataRow rv, TableInfo info)
        {
            //所有列遍历
            foreach (CellInfo cell in info.Cells)
            {
                //返回到明细表
                if (!info.TableName.Equals(_tableConfig.TableName))
                {
                    //上级默认值
                    if (cell.CellName.Equals("ParentId"))
                    {
                        rv[cell.CellName] = _chooseTopId;
                        continue;
                    }
                }

                //返回到第三表（扩展表）
                if (_isPopWindow && _isExtListWindow)
                {
                    //上级默认值
                    if (cell.CellName.Equals("ParentId"))
                    {
                        rv[cell.CellName] = _extListParentId;
                        continue;
                    }
                }

                try
                {
                    //得到列的默认值
                    rv[cell.CellName] = GetCellDefaultValue(cell);
                    continue;
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "得到列的默认值异常");
                }

                try
                {
                    //默认类型值
                    rv[cell.CellName] = GetDefaultValue(cell.ValType);
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "根据类型得到默认值异常");
                }
            }
        }
        /// <summary>
        /// 得到列的默认值
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private object GetCellDefaultValue(CellInfo cell)
        {
            try
            {
                //关联列不需要生成默认值
                if (cell.ForeignTableId > 0) return DBNull.Value;

                if (cell.DefaultValue.Equals("(getdate())"))
                {
                    //当前日期
                    if (cell.ValType == "date") return DateTime.Now.Date;
                    return DateTime.Now;
                }
                else if (cell.CellName.Equals("AuditDate"))
                {
                    //审核日期
                    return DBNull.Value;
                }
                else if (cell.CellName.Equals("UserId") || cell.CellName.Equals("CreateUserId"))
                {
                    //用户编号
                    return AppGlobal.UserInfo.UserId;
                }
                else if (cell.CellName.Equals("UserName") || cell.CellName.Equals("CreateUserName"))
                {
                    //用户姓名
                    return AppGlobal.UserInfo.UserName;
                }
                else if (cell.CellName.Equals("CreateDate") || cell.CellName.Equals("ModifyDate") || cell.CellName.Equals("UpdateDate"))
                {
                    //创建时间和修改时间
                    return DateTime.Now;
                }
                else if ((cell.ValType == "date" || cell.ValType == "datetime") && cell.AllownNull)
                {
                    //可为空的日期列
                    return DBNull.Value;
                }
                else if (cell.DefaultValue.StartsWith("(User."))
                {
                    //默认值为用户信息
                    System.Text.RegularExpressions.Regex regexName = new System.Text.RegularExpressions.Regex(@"\(User\.(?<Name>[^)]*?)\)");
                    if (regexName.IsMatch(cell.DefaultValue))
                    {
                        //列名
                        string cellName = regexName.Match(cell.DefaultValue).Groups["Name"].ToString();

                        if (cellName.ToLower().Equals("id")) return AppGlobal.UserInfo.UserId;
                        else if (cellName.ToLower().Equals("roleid")) return AppGlobal.UserInfo.RoleId;
                        else if (cellName.ToLower().Equals("rolename")) return AppGlobal.UserInfo.RoleName;
                        else if (cellName.ToLower().Equals("bmid")) return AppGlobal.UserInfo.BMID;
                        else if (cellName.ToLower().Equals("bmmc")) return AppGlobal.UserInfo.BMMC;
                        else if (cellName.ToLower().Equals("xm")) return AppGlobal.UserInfo.UserName;
                        else if (cellName.ToLower().Equals("sj")) return AppGlobal.UserInfo.Mobile;
                        else if (cellName.ToLower().Equals("userid")) return AppGlobal.UserInfo.UserId;
                        else if (cellName.ToLower().Equals("username")) return AppGlobal.UserInfo.UserName;

                        //从用户数据选择对应列
                        if (!string.IsNullOrWhiteSpace(cellName))
                        {
                            if (AppGlobal.UserDataRow.Table.Columns.Contains(cellName))
                            {
                                return AppGlobal.UserDataRow[cellName];
                            }
                        }
                    }
                }
                else if (cell.DefaultValue.StartsWith("(Sys."))
                {
                    //默认值为系统配置Sys_Configs
                    System.Text.RegularExpressions.Regex regexName = new System.Text.RegularExpressions.Regex(@"\(Sys\.(?<Name>[^)]*?)\)");
                    if (regexName.IsMatch(cell.DefaultValue))
                    {
                        //是否有Key
                        string sysKey = regexName.Match(cell.DefaultValue).Groups["Name"].ToString();
                        if (!string.IsNullOrWhiteSpace(sysKey))
                        {
                            //是否有值
                            object objVal = AppGlobal.GetSysConfig(sysKey);
                            if (objVal != null)
                            {
                                try
                                {
                                    //转换类型
                                    Type type2 = Core.AppHandler.GetTypeByString(cell.ValType);
                                    return Convert.ChangeType(objVal, type2);
                                }
                                catch { }
                            }
                        }
                    }
                }
                else if (cell.DefaultValue.StartsWith("(Date."))
                {
                    //日期内容格式 如年：(Date.yyyy)  全格式为：yyyy-MM-dd HH:mm:ss
                    System.Text.RegularExpressions.Regex regexName = new System.Text.RegularExpressions.Regex(@"\(Date\.(?<Format>[^)]*?)\)");
                    if (regexName.IsMatch(cell.DefaultValue))
                    {
                        string format = regexName.Match(cell.DefaultValue).Groups["Format"].ToString();
                        if (!string.IsNullOrWhiteSpace(format))
                        {
                            try
                            {
                                string dtValue = DateTime.Now.ToString(format);
                                Type type3 = Core.AppHandler.GetTypeByString(cell.ValType);
                                return Convert.ChangeType(dtValue, type3);
                            }
                            catch { }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(cell.DefaultValue))
                {
                    //默认值
                    Type type = Core.AppHandler.GetTypeByString(cell.ValType);
                    return Convert.ChangeType(cell.DefaultValue, type);
                }
                else
                {
                    //类型默认值
                    return GetDefaultValue(cell.ValType);
                }
            }
            catch
            {
                return DBNull.Value;
            }
        }
        /// <summary>
        /// 得到类型默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object GetDefaultValue(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;

            var dbValue = new object();

            switch (type.ToLower())
            {
                case "decimal":
                case "double":
                case "float":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "short":
                case "ushort": dbValue = 0; break;
                case "date":
                case "datetime": dbValue = DateTime.Now; break;
                case "bool": dbValue = false; break;
                case "byte": dbValue = default(byte); break;
                case "sbyte": dbValue = default(sbyte); break;
                case "object": dbValue = default(object); break;
                case "char": dbValue = default(char); break;
                case "string": dbValue = string.Empty; break;
                case "guid": dbValue = Guid.NewGuid(); break;
                default: dbValue = null; break;
            }

            return dbValue;
        }
        /// <summary>
        /// 得到列值
        /// </summary>
        /// <param name="view"></param>
        /// <param name="cell"></param>
        private KeyValue GetCellValue(DataRowView view, CellInfo cell)
        {
            if (!view.Row.Table.Columns.Contains(cell.CellName)) return null;

            var dbValue = view[cell.CellName];

            if (dbValue is DBNull)
            {
                //默认值
                dbValue = GetCellDefaultValue(cell);
            }
            else
            {
                //列类型
                Type type = AppHandler.GetTypeByString(cell.ValType);

                try
                {
                    //转换类型
                    dbValue = Convert.ChangeType(dbValue, type);
                }
                catch
                {
                    //是否有设定默认值
                    dbValue = GetCellDefaultValue(cell);
                }


                //是否有小数位数
                if (cell.DecimalDigits > 0)
                {
                    try
                    {
                        if (type == typeof(float))
                        {
                            //浮点小数
                            float fValue = DataType.Float(dbValue, 0);
                            dbValue = Math.Round(fValue, cell.DecimalDigits);
                        }
                        else if (type == typeof(double))
                        {
                            //双精度浮点小数
                            double dValue = DataType.Double(dbValue, 0);
                            dbValue = Math.Round(dValue, cell.DecimalDigits);
                        }
                        else if (type == typeof(decimal))
                        {
                            //decimal
                            decimal dValue = DataType.Decimal(dbValue, 0);
                            dbValue = Math.Round(dValue, cell.DecimalDigits);
                        }
                    }
                    catch { }
                }
            }

            //没有值
            if (dbValue == DBNull.Value) return null;

            //返回对象
            return new KeyValue(cell.CellName, dbValue);
        }
        #endregion

        #region 辅助数据表
        /// <summary>
        /// 主表添加行
        /// </summary>
        private void TopAddNewRow(bool skipDefaultAddOpenWin = false)
        {
            //判断是否有开单权限
            bool hasAuth = AppGlobal.HasAuthority(_moduleId, (int)ListActionEnum.开单);
            if (!hasAuth) return;

            _isChooseTop = true;

            this.borderChooseTop.Visibility = Visibility.Visible;
            this.borderChooseBottom.Visibility = Visibility.Collapsed;

            if (!skipDefaultAddOpenWin)
            {
                try
                {
                    CellInfo cellPopTable = _tableConfig.Cells.Find(p => p.IsAddPopTable && p.ForeignTableId > 0);
                    if (cellPopTable != null)
                    {
                        //弹出选择列表
                        this.dataGridTop.Focus();
                        PopChooseListTable(cellPopTable);
                        return;
                    }
                }
                catch (Exception ex) { }
            }

            //新行
            DataRow newRow = _topPageResult.Data.NewRow();
            _topPageResult.Data.Rows.Add(newRow);
            DataRowView row = _topPageResult.Data.DefaultView[_topPageResult.Data.Rows.Count - 1];

            SetDefaultValue(row, _tableConfig);

            row["Id"] = -DateTime.Now.ToFileTime();
            row[AppGlobal.DataGridNewStateCellName] = true;

            try
            {
                //得到新添加的行
                DataGridRow topGridNewRow = this.dataGridTop.GetRow(this.dataGridTop.Items.Count - 1);
                topGridNewRow.Loaded += TopGridNewRow_Loaded;
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 主表新增加行加载成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopGridNewRow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //标记编辑
                DataGridPlus.SetRowEdit(dataGridTop, this.dataGridTop.Items.Count - 1, false);
            }
            catch (Exception ex) { }

            try
            {
                //选择最后一行
                this.dataGridTop.SelectedIndex = this.dataGridTop.Items.Count - 1;
                this.dataGridTop.SelectedItem = this.dataGridTop.Items[this.dataGridTop.Items.Count - 1];
                this.dataGridTop.ScrollIntoView(this.dataGridTop.SelectedItem);
            }
            catch (Exception ex) { }

            try
            {
                //聚焦最后一行的第一列
                DataGridCell dgc = this.dataGridTop.GetCell(this.dataGridTop.SelectedIndex, 0);
                dgc.Focus();
                //SetCellEdit(this.dataGridTop, dgc, null);
                keybd_event(System.Windows.Forms.Keys.Right, 0, 0, 0);
                keybd_event(System.Windows.Forms.Keys.Right, 0, 2, 0);
            }
            catch { }

            try
            {
                //取消事件
                (sender as DataGridRow).Loaded -= TopGridNewRow_Loaded;
            }
            catch { }
        }

        /// <summary>
        /// 子表添加行
        /// </summary>
        private void BottomAddNewRow(bool skipDefaultAddOpenWin = false)
        {
            //判断是否有添加权限
            bool hasAuth = AppGlobal.HasAuthority(_moduleId, (int)ListActionEnum.添加);
            if (!hasAuth) return;

            _isChooseTop = false;

            this.borderChooseTop.Visibility = Visibility.Collapsed;
            this.borderChooseBottom.Visibility = Visibility.Visible;

            if (!skipDefaultAddOpenWin)
            {
                try
                {
                    CellInfo cellPopTable = _tableConfig.SubTable.Cells.Find(p => p.IsAddPopTable && p.ForeignTableId > 0);
                    if (cellPopTable != null)
                    {
                        //弹出选择列表
                        this.dataGridBottom.Focus();
                        PopChooseListTable(cellPopTable);
                        return;
                    }
                }
                catch (Exception ex) { }
            }

            //新行
            DataRow newRow = _bottomPageResult.Data.NewRow();
            _bottomPageResult.Data.Rows.Add(newRow);
            DataRowView row = _bottomPageResult.Data.DefaultView[_bottomPageResult.Data.Rows.Count - 1];

            SetDefaultValue(row, _tableConfig.SubTable);

            row["Id"] = -DateTime.Now.ToFileTime();
            row[AppGlobal.DataGridNewStateCellName] = true;

            try
            {
                //得到新添加的行
                DataGridRow bottomGridNewRow = this.dataGridBottom.GetRow(this.dataGridBottom.Items.Count - 1);
                bottomGridNewRow.Loaded += BottomGridNewRow_Loaded;
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 从表新增加的行加载成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomGridNewRow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //标记编辑
                DataGridPlus.SetRowEdit(dataGridBottom, this.dataGridBottom.Items.Count - 1, false);
            }
            catch (Exception ex) { }

            try
            {
                //选择最后一行
                this.dataGridBottom.SelectedIndex = this.dataGridBottom.Items.Count - 1;
                this.dataGridBottom.SelectedItem = this.dataGridBottom.Items[this.dataGridBottom.Items.Count - 1];
                this.dataGridBottom.ScrollIntoView(this.dataGridBottom.SelectedItem);
            }
            catch (Exception ex) { }

            try
            {
                //取消事件
                (sender as DataGridRow).Loaded -= BottomGridNewRow_Loaded;
            }
            catch { }
        }
        #endregion

        #region 操作事件【中间栏操作按钮事件】
        /// <summary>
        /// 加载操作按钮
        /// </summary>
        private void LoadActions()
        {
            if (_isLoadActions) return;
            _isLoadActions = true;

            try
            {
                DataTable dt = null;

                if (_moduleId > 0)
                {
                    //查询所有操作
                    dt = AppGlobal.TableOperateActions.Copy();
                    if (dt == null || dt.Rows.Count <= 0) return;
                }
                else
                {
                    //空表结构
                    dt = new DataTable();
                    dt.Columns.Add(new DataColumn("Id", typeof(long)));
                    dt.Columns.Add(new DataColumn("ActionName", typeof(string)));
                    dt.Columns.Add(new DataColumn("Icon", typeof(string)));
                    dt.Columns.Add(new DataColumn("Code", typeof(int)));
                    dt.Columns.Add(new DataColumn("Quick", typeof(string)));
                }

                if (_tableConfig.SubTable != null && _tableConfig.SubTable.ThreeTable != null)
                {
                    //扩展按钮
                    DataRow rowExtend = dt.NewRow();
                    rowExtend["Id"] = -1;
                    rowExtend["ActionName"] = "扩展";
                    rowExtend["Icon"] = "ArrangeSendBackward";
                    rowExtend["Code"] = "-7";
                    rowExtend["Quick"] = "F6";
                    dt.Rows.Add(rowExtend);
                }

                //刷新按钮
                DataRow rowReload = dt.NewRow();
                rowReload["Id"] = -1;
                rowReload["ActionName"] = "刷新";
                rowReload["Icon"] = "Refresh";
                rowReload["Code"] = "-1";
                rowReload["Quick"] = "F5";
                dt.Rows.Add(rowReload);

                if (_isPopWindow)
                {
                    //要回传的窗口
                    if (_isChooseCallbackWindow)
                    {
                        //引用按钮
                        DataRow rowQuote = dt.NewRow();
                        rowQuote["Id"] = -1;
                        rowQuote["ActionName"] = "引用";
                        rowQuote["Icon"] = "CheckboxMarkedCircleOutline";
                        rowQuote["Code"] = "-3";
                        rowQuote["Quick"] = "";
                        dt.Rows.Add(rowQuote);
                    }

                    //退出按钮
                    DataRow rowExit = dt.NewRow();
                    rowExit["Id"] = -1;
                    rowExit["ActionName"] = "退出";
                    rowExit["Icon"] = "WindowClose";
                    rowExit["Code"] = "-2";
                    rowExit["Quick"] = "";
                    dt.Rows.Add(rowExit);
                }

                //单表、双表、三表、虚拟表 操作时可对列进行排序
                if (_tableConfig.IsRealTable || _tableConfig.IsVTable)
                {
                    //列排序按钮
                    DataRow rowOrder = dt.NewRow();
                    rowOrder["Id"] = -1;
                    rowOrder["ActionName"] = "列排序";
                    rowOrder["Icon"] = "SwapHorizontal";
                    rowOrder["Code"] = "-5";
                    rowOrder["Quick"] = "B";
                    dt.Rows.Add(rowOrder);
                }

                //查询设置按钮
                DataRow rowQuery = dt.NewRow();
                rowQuery["Id"] = -1;
                rowQuery["ActionName"] = "查询";
                rowQuery["Icon"] = "Magnify";
                rowQuery["Code"] = "-6";
                rowQuery["Quick"] = "F";
                dt.Rows.Add(rowQuery);

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //是否单表
                    bool isSingleTable = _tableConfig.IsSingleTable;
                    Style btnListAction = this.FindResource("btnListAction") as Style;

                    this.panelActions.Children.Clear();

                    //循环所有行
                    foreach (DataRow row in dt.Rows)
                    {
                        long id = DataType.Long(row["Id"].ToString(), 0);
                        string name = row["ActionName"].ToString();
                        string icon = row["Icon"].ToString();
                        int code = DataType.Int(row["Code"].ToString(), 0);
                        string key = row["Quick"].ToString();

                        //非按钮
                        if (code > 900) continue;

                        //视图表 不包括 开单等
                        if (_tableConfig.TableType == TableType.视图 && AppGlobal.ViewTableNotShowActionCodes.Contains(code)) continue;

                        //操作代码大于0 需要权限验证
                        if (code > 0)
                        {
                            //是否有权限
                            if (!AppGlobal.HasAuthorityByCode(_moduleIds, code)) continue;
                        }

                        //库存表权限
                        if (_tableConfig.TableSubType == TableSubType.库存表)
                        {
                            if (code == (int)ListActionEnum.开单 ||
                                code == (int)ListActionEnum.添加 ||
                                code == (int)ListActionEnum.编辑 ||
                                code == (int)ListActionEnum.删除 ||
                                code == (int)ListActionEnum.保存)
                            {
                                //库存表不需要开单、添加、编辑、删除、保存
                                continue;
                            }
                        }

                        //单表不要添加按钮
                        if (code == (int)ListActionEnum.添加 && _tableConfig.SubTable == null) continue;
                        //单表开单按钮 替换为 添加样式
                        if (code == (int)ListActionEnum.开单 && _tableConfig.SubTable == null)
                        {
                            DataRow[] rowAddActions = dt.Select("[Code]=" + (int)ListActionEnum.添加);
                            if (rowAddActions != null && rowAddActions.Count() > 0)
                            {
                                name = rowAddActions[0]["ActionName"].ToString();
                                icon = rowAddActions[0]["Icon"].ToString();
                                key = rowAddActions[0]["Quick"].ToString();
                            }
                        }

                        //如果没有审核列则不显示审核按钮
                        if (!_tableConfig.Cells.Exists(p => p.CellName.Equals("IsAudit")))
                        {
                            //不要审核按钮
                            if (code == (int)ListActionEnum.审核 || code == (int)ListActionEnum.反审) continue;
                        }

                        //操作时间
                        ListAction action = new ListAction()
                        {
                            Id = id,
                            Code = code,
                            Icon = icon,
                            KeyName = key,
                            ActionName = name
                        };
                        _listQuicks.Add(action);

                        Button btn = new Button();
                        btn.Style = btnListAction;
                        btn.Width = 50;
                        btn.Height = 50;
                        btn.Tag = action;
                        btn.IsEnabledChanged += BtnListAction_IsEnabledChanged;

                        PackIconMaterial pi = new PackIconMaterial();
                        pi.Width = 18;
                        pi.Height = 18;
                        pi.Foreground = Brushes.Black;
                        pi.Margin = new Thickness(0, (12), 0, 0);
                        pi.VerticalAlignment = VerticalAlignment.Center;
                        pi.HorizontalAlignment = HorizontalAlignment.Center;
                        pi.Kind = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), icon);

                        TextBlock lbl = new TextBlock();
                        lbl.HorizontalAlignment = HorizontalAlignment.Center;
                        lbl.Text = name + (!string.IsNullOrWhiteSpace(key) ? "(" + key + ")" : "");

                        StackPanel panel = new StackPanel();
                        panel.Children.Add(pi);
                        panel.Children.Add(lbl);
                        panel.HorizontalAlignment = HorizontalAlignment.Center;
                        panel.VerticalAlignment = VerticalAlignment.Center;

                        btn.Content = panel;
                        btn.Click += BtnListAction_Click;

                        this.panelActions.Children.Add(btn);
                    }

                    //加载成功
                    _isLoadedActionsSuccess = true;

                    return null;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //未在加载
                _isLoadActions = false;
            }
        }
        /// <summary>
        /// 是否启用按钮变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListAction_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ListAction action = (sender as Button).Tag as ListAction;
            action.IsEnable = (sender as Button).IsEnabled;
        }
        /// <summary>
        /// 点击列表按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnListAction_Click(object sender, RoutedEventArgs e)
        {
            //操作事件代码
            ListAction action = (sender as Button).Tag as ListAction;
            if (action == null) return;

            switch (action.Code)
            {
                case (int)ListActionEnum.刷新:
                    //刷新
                    ReloadData();
                    break;
                case (int)ListActionEnum.退出:
                    //退出
                    ExitPopWin();
                    break;
                case (int)ListActionEnum.引用:
                    //引用
                    QuoteBack();
                    break;
                case (int)ListActionEnum.开单:
                    //开单
                    TopAddRow(action);
                    break;
                case (int)ListActionEnum.添加:
                    //添加
                    BottomAddRow(action);
                    break;
                case (int)ListActionEnum.编辑:
                    //编辑
                    EditRow(action);
                    break;
                case (int)ListActionEnum.删除:
                    //删除
                    DeleteRow(action);
                    break;
                case (int)ListActionEnum.保存:
                    //保存
                    Save(action);
                    break;
                case (int)ListActionEnum.打印:
                    //打印
                    Print(action);
                    break;
                case (int)ListActionEnum.打印设置:
                    //打印设置
                    PrintSetting(action);
                    break;
                case (int)ListActionEnum.审核:
                    //审核
                    AuditRow(action);
                    break;
                case (int)ListActionEnum.反审:
                    //反审
                    CancelAudit(action);
                    break;
                case (int)ListActionEnum.导入:
                    //导入
                    ImportData(action);
                    break;
                case (int)ListActionEnum.导出:
                    //导出
                    ExportData(action);
                    break;
                case (int)ListActionEnum.取消:
                    //取消
                    CancelAction(action);
                    break;
                case (int)ListActionEnum.列排序:
                    //列排序
                    SortCellOrder(action);
                    break;
                case (int)ListActionEnum.查询设置:
                    //查询设置
                    SetQueryCells(action);
                    break;
                case (int)ListActionEnum.扩展三表:
                    //打开扩展三表
                    OpenThreeTable();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 引用
        /// 1、如果有选择 则引用已选择
        /// 2、如果没有选择 则提示引用所有
        /// </summary>
        private void QuoteBack()
        {
            bool isFullTableRows = false;
            bool hasChooseRows = false;
            if (this.dataGridTop.SelectedItems != null && this.dataGridTop.SelectedItems.Count > 0)
            {
                //有选择的行
                hasChooseRows = true;
            }
            else if (!hasChooseRows)
            {
                //没有选择行
                bool? result = AppAlert.Alert("将本页所有记录回传！", "是否全表引用？", AlertWindowButton.OkCancel);
                if (result.HasValue && result.Value)
                {
                    isFullTableRows = true;
                }
            }

            //单次引用不可超过N行
            int maxQuoteCount = AppGlobal.GetSysConfigReturnInt("List_MaxQuoteCount", 200);
            if (this.dataGridTop.SelectedItems.Count > maxQuoteCount)
            {
                AppAlert.FormTips(gridMain, "单次引用不可超过" + maxQuoteCount + "行，请分批引用！");
                return;
            }

            List<DataRow> returnRows = new List<DataRow>();

            if (hasChooseRows)
            {
                foreach (DataRowView row in this.dataGridTop.SelectedItems)
                {
                    //返回行
                    returnRows.Add(row.Row);
                }
            }
            else if (isFullTableRows)
            {
                //全表引用
                for (int i = 0; i < this.dataGridTop.Items.Count; i++)
                {
                    //行
                    DataRowView row = this.dataGridTop.Items[i] as DataRowView;
                    //返回行
                    returnRows.Add(row.Row);
                }
            }

            //显示等待
            ShowListLoading();

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    foreach (DataRow row in returnRows)
                    {
                        //返回行
                        ReturnRow(row);
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this._ParentWindow.Close();
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "引用数据失败，请重试！");
                        return null;
                    }));

                    ErrorHandler.AddException(ex, "引用数据回传异常");
                }
                finally
                {
                    //隐藏提示
                    HideListLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 退出窗体
        /// </summary>
        private void ExitPopWin()
        {
            _ParentWindow.Close();
        }

        /// <summary>
        /// 按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            //不是活动窗口
            if (!_isPopWindow)
            {
                if (!IsActive || AppData.MainWindow._CurrentActivePage != this) return;
            }

            if (e.Key == Key.F5)
            {
                if (_listAction == ListActionEnum.Null)
                {
                    //刷新数据
                    ReloadData();
                    return;
                }
            }
            else if (e.Key == Key.F6)
            {
                //打开第三表（扩展表）
                OpenThreeTable();
                return;
            }
            else if (e.Key == Key.Escape)
            {
                //退出
                if (_isPopWindow)
                {
                    (_ParentWindow as Components.PopWindow).Close();
                    return;
                }
            }

            //不是Ctrl组合键
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) return;

            //列排序时禁用所有按钮快捷键
            if (_listAction == ListActionEnum.列排序) return;

            if (e.Key == Key.Q)
            {
                //查询数据焦点聚焦到数据表
                _isQueryDataFocusTable = true;
                //生成查询条件
                BuildQueryWhere();
                //查询数据
                LoadData();
                return;
            }
            else if (e.Key == Key.B)
            {
                //列排序
                ListAction actionCellOrder = new ListAction()
                {
                    ActionName = "列排序",
                    Code = -5,
                    Icon = "Actions.action.cellorder.png",
                    IsEnable = true,
                    KeyName = "B"
                };
                SortCellOrder(actionCellOrder);
                return;
            }
            else if (e.Key == Key.F)
            {
                //查询设置
                ListAction actionQuerySet = new ListAction()
                {
                    ActionName = "列排序",
                    Code = -6,
                    Icon = ".Actions.action.query.png",
                    IsEnable = true,
                    KeyName = "F"
                };
                SetQueryCells(actionQuerySet);
                return;
            }

            if (e.Key == Key.PageUp)
            {
                //上一页
                GoToPage(-1);
                return;
            }
            else if (e.Key == Key.PageDown)
            {
                //下一页
                GoToPage(1);
                return;
            }

            //有快捷操作
            if (_listQuicks != null)
            {
                //是否有操作
                ListAction action = _listQuicks.Find(p => p.QuickKey == e.Key);

                //没有快捷操作
                if (action == null) return;

                if (action != null)
                {
                    //禁用
                    if (!action.IsEnable) return;

                    if (_listAction == ListActionEnum.Null)
                    {
                        //无任何操作时
                        switch (action.ActionEnum)
                        {
                            case ListActionEnum.刷新:
                                ReloadData();
                                break;
                            case ListActionEnum.Null:
                                break;
                            case ListActionEnum.开单:
                                TopAddRow(action);
                                break;
                            case ListActionEnum.添加:
                                BottomAddRow(action);
                                e.Handled = true;
                                break;
                            case ListActionEnum.编辑:
                                EditRow(action);
                                break;
                            case ListActionEnum.删除:
                                DeleteRow(action);
                                break;
                            case ListActionEnum.审核:
                                AuditRow(action);
                                break;
                            case ListActionEnum.反审:
                                CancelAudit(action);
                                break;
                            case ListActionEnum.打印:
                                Print(action);
                                break;
                            case ListActionEnum.打印设置:
                                PrintSetting(action);
                                break;
                            case ListActionEnum.导入:
                                ImportData(action);
                                break;
                            case ListActionEnum.导出:
                                ExportData(action);
                                break;
                            default:
                                break;

                        }
                    }
                    else if (_listAction == ListActionEnum.开单 || _listAction == ListActionEnum.添加 || _listAction == ListActionEnum.编辑)
                    {
                        //编辑状态时
                        switch (action.ActionEnum)
                        {
                            case ListActionEnum.Null:
                                break;
                            case ListActionEnum.删除:
                                DeleteRow(action);
                                break;
                            case ListActionEnum.保存:
                                Save(action);
                                break;
                            case ListActionEnum.取消:
                                CancelAction(action);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 快捷键翻页
        /// </summary>
        /// <param name="pi"></param>
        private void GoToPage(int pi)
        {
            if (!_isChooseTop)
            {
                //子表
                try
                {
                    //得到主表行
                    DataRowView row = (this.dataGridTop.SelectedItem as DataRowView);
                    if (row == null)
                    {
                        //清空子表数据
                        this.dataGridBottom.ItemsSource = null;
                        return;
                    }

                    //没有编号列
                    if (!row.Row.Table.Columns.Contains("Id")) return;

                    //主表记录编号
                    long id = DataType.Long(row["Id"].ToString(), 0);

                    //新页数
                    int newPageIndex = _PageIndexSub + pi;
                    if (newPageIndex < 1)
                    {
                        AppAlert.FormTips(gridMain, "已经是第一页！", FormTipsType.Info);
                        return;
                    }
                    if (newPageIndex > _bottomPageResult.PageCount)
                    {
                        AppAlert.FormTips(gridMain, "已经是最后一页！", FormTipsType.Info);
                        return;
                    }

                    //当前分页
                    _PageIndexSub = newPageIndex;

                    //重新加载数据
                    LoadingDetails(id);
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "子表快捷键翻页异常");
                }
            }
            else
            {
                //主表
                try
                {
                    //新页数
                    int newPageIndex = _PageIndex + pi;
                    if (newPageIndex < 1)
                    {
                        AppAlert.FormTips(gridMain, "已经是第一页！", FormTipsType.Info);
                        return;
                    }
                    if (newPageIndex > _topPageResult.PageCount)
                    {
                        AppAlert.FormTips(gridMain, "已经是最后一页！", FormTipsType.Info);
                        return;
                    }

                    //当前分页
                    _PageIndex = newPageIndex;

                    //重新加载数据
                    LoadingData();
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "主表快捷键翻页异常");
                }
            }
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        private void ReloadData(bool chooseLastRow = false)
        {
            if (_isChooseTop)
            {
                //刷新主表
                ReloadZB(chooseLastRow);
            }
            else
            {
                //刷新从表
                ReloadCB(chooseLastRow);
            }
        }
        /// <summary>
        /// 刷新主表
        /// </summary>
        private void ReloadZB(bool chooseLastRow = false)
        {
            //选择主表
            int rowSelectedIndex = this.dataGridTop.SelectedIndex;

            //清除绑定
            this.dataGridTop.ItemsSource = null;

            //加载数据
            LoadingData();

            try
            {
                if (chooseLastRow)
                {
                    //定位最后一行
                    this.dataGridTop.SelectedIndex = this.dataGridTop.Items.Count - 1;
                }
                else
                {
                    //定位上一次选择的行数
                    this.dataGridTop.SelectedIndex = rowSelectedIndex;
                }

                //滚动条定位
                this.dataGridTop.ScrollIntoView(this.dataGridTop.SelectedItem);
                this.dataGridTop.ScrollIntoView(this.dataGridTop.Items[this.dataGridTop.SelectedIndex]);
                this.dataGridTop.GetCell(this.dataGridTop.SelectedIndex, 1)?.Focus();
            }
            catch { }
        }
        /// <summary>
        /// 刷新从表
        /// </summary>
        /// <param name="chooseLastRow"></param>
        private void ReloadCB(bool chooseLastRow = false)
        {
            //选择子表
            int rowSelectedIndex = this.dataGridBottom.SelectedIndex;

            //清除绑定
            this.dataGridBottom.ItemsSource = null;

            //加载明细
            LoadingDetails(_chooseTopId);

            try
            {
                if (!chooseLastRow)
                {
                    //定位上一次选择的行数
                    this.dataGridBottom.SelectedIndex = rowSelectedIndex;
                }
                else
                {
                    //定位最后一行
                    this.txtSeachBottom.Text = "";
                    _bottomSearchKeywords = "";
                    this.dataGridBottom.SelectedIndex = this.dataGridBottom.Items.Count - 1;
                }

                //滚动条定位
                this.dataGridBottom.ScrollIntoView(this.dataGridBottom.SelectedItem);
                this.dataGridBottom.ScrollIntoView(this.dataGridBottom.Items[this.dataGridBottom.SelectedIndex]);
                this.dataGridBottom.GetCell(this.dataGridBottom.SelectedIndex, 1)?.Focus();
            }
            catch { }
        }

        /// <summary>
        /// 主表添加行
        /// </summary>
        private void TopAddRow(ListAction action)
        {
            //是否有数据源
            if (this.dataGridTop.ItemsSource == null) return;

            //主表设置可编辑
            SetTopEdit();
        }
        /// <summary>
        /// 主表设置可编辑
        /// </summary>
        private void SetTopEdit(bool addNewRow = true)
        {
            //当前操作
            _listAction = ListActionEnum.开单;
            //禁用按钮
            DisableListBtns();

            if (addNewRow)
            {
                //添加一行
                TopAddNewRow();
            }

            //设置自动添加行、可编辑
            this.dataGridTop.CanUserAddRows = false;
            this.dataGridTop.IsReadOnly = false;

            //不可排序
            this.dataGridTop.CanUserSortColumns = false;
            this.dataGridBottom.CanUserSortColumns = false;

            //不可拖动列排序
            this.dataGridTop.CanUserReorderColumns = false;
            this.dataGridBottom.CanUserReorderColumns = false;

            //清空明细表数据
            this.dataGridBottom.IsEnabled = false;
            this.dataGridBottom.ItemsSource = null;

            try
            {
                //最后一行
                this.dataGridTop.SelectedIndex = this.dataGridTop.Items.Count - 1;
                this.dataGridTop.GetCell(this.dataGridTop.Items.Count - 1, 1)?.Focus();
            }
            catch { }
        }

        /// <summary>
        /// 明细表添加行
        /// </summary>
        private void BottomAddRow(ListAction action)
        {
            if (this.dataGridTop.SelectedItem == null)
            {
                this.dataGridTop.SelectedIndex = this.dataGridTop.Items.Count - 1;
            }

            //是否有数据源
            if (this.dataGridBottom.ItemsSource == null) return;

            //主表选择行
            DataRow bottomSelectRow = this.dataGridBottom.SelectedItem != null ? (this.dataGridBottom.SelectedItem as DataRowView).Row : null;

            //设置子表编辑
            SetBottomEdit();
        }
        /// <summary>
        /// 设置子表编辑
        /// </summary>
        private void SetBottomEdit(bool addNewRow = true)
        {
            //当前操作
            _listAction = ListActionEnum.添加;
            //禁用按钮
            DisableListBtns();

            try
            {
                //主表选中样式
                this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderBrush = Brushes.Blue;
                this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderThickness = new Thickness(0, 1, 0, 1);
            }
            catch { }

            if (addNewRow)
            {
                //子表添加行
                BottomAddNewRow();
            }

            try
            {
                this.dataGridTop.IsEnabled = false;
                this.dataGridBottom.CanUserAddRows = false;
                this.dataGridBottom.IsReadOnly = false;
                this.dataGridBottom.ScrollIntoView(this.dataGridBottom.Items[this.dataGridBottom.Items.Count - 1]);
            }
            catch { }

            //不可排序
            this.dataGridTop.CanUserSortColumns = false;
            this.dataGridBottom.CanUserSortColumns = false;

            //不可拖动列排序
            this.dataGridTop.CanUserReorderColumns = false;
            this.dataGridBottom.CanUserReorderColumns = false;

            try
            {
                //最后一行
                this.dataGridBottom.SelectedIndex = this.dataGridBottom.Items.Count - 1;
                this.dataGridBottom.GetCell(this.dataGridBottom.Items.Count - 1, 1)?.Focus();
            }
            catch { }
        }

        /// <summary>
        /// 编辑
        /// </summary>
        private void EditRow(ListAction action)
        {
            int prevEditRowIndex = _editRowIndex;
            DataGrid dataGrid = null;
            DataRowView bottomEditRow = null;
            DataRow chooseRow = null;

            if (_isChooseTop)
            {
                //编辑主表
                dataGrid = dataGridTop;
                _editRowIndex = dataGridTop.SelectedIndex;
                //是否审核
                if (SelectItemIsAudit(dataGridTop)) return;

                //选择的行
                chooseRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

                //标记编辑
                DataGridPlus.SetRowEdit(dataGridTop, _editRowIndex, false);

                this.dataGridBottom.IsEnabled = false;
                this.dataGridTop.IsReadOnly = false;
                this.dataGridTop.CanUserAddRows = false;
                this.dataGridTop.CanUserSortColumns = false;
            }
            else
            {
                //编辑明细表
                dataGrid = dataGridBottom;
                _editRowIndex = dataGridBottom.SelectedIndex;
                //是否审核
                if (SelectItemIsAudit(dataGridBottom)) return;

                //选择的行
                chooseRow = this.dataGridBottom.SelectedItem != null ? (this.dataGridBottom.SelectedItem as DataRowView).Row : null;

                //标记编辑
                DataGridPlus.SetRowEdit(dataGridBottom, _editRowIndex, false);
                //滚动到编辑行
                bottomEditRow = dataGridBottom.SelectedItem as DataRowView;

                try
                {
                    //主表选中样式
                    this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderBrush = Brushes.Blue;
                    this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderThickness = new Thickness(0, 1, 0, 1);
                }
                catch { }

                this.dataGridTop.IsEnabled = false;
                this.dataGridBottom.IsReadOnly = false;
                this.dataGridBottom.CanUserAddRows = false;
                this.dataGridBottom.CanUserSortColumns = false;
            }

            //不可拖动列排序
            this.dataGridTop.CanUserReorderColumns = false;
            this.dataGridBottom.CanUserReorderColumns = false;

            //上一行编辑取消
            if (prevEditRowIndex >= 0)
            {
                DataGridPlus.SetRowReadOnly(dataGrid, prevEditRowIndex);
            }

            //当前操作
            _listAction = ListActionEnum.编辑;
            //禁用按钮
            DisableListBtns();

            //滚动到编辑行
            if (this.dataGridTop.SelectedItem != null) this.dataGridTop.ScrollIntoView(this.dataGridTop.SelectedItem);
            if (bottomEditRow != null) this.dataGridBottom.ScrollIntoView(bottomEditRow);
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void DeleteRow(ListAction action)
        {
            if (_isChooseTop)
            {
                //主表
                DeleteTop(action);
            }
            else
            {
                //明细表
                DeleteDetails(action);
            }
        }

        #region 删除扩展
        /// <summary>
        /// 删除主表数据
        /// </summary>
        private void DeleteTop(ListAction action)
        {
            //是否有删除
            if (dataGridTop.SelectedItems == null || dataGridTop.SelectedItems.Count <= 0) return;

            //再次询问是否删除
            bool? isDelete = AppAlert.Alert("是否确定删除？\r\n删除后无法恢复！", "是否确定删除？", AlertWindowButton.OkCancel);
            if (!isDelete.HasValue || !isDelete.Value) return;

            //要删除的行
            List<DataRowView> deleteRows = new List<DataRowView>();

            //显示等待
            ShowLoading(gridMain);

            //要删除的Id列表
            string ids = "";

            //循环所有行
            for (int i = dataGridTop.SelectedItems.Count - 1; i >= 0; i--)
            {
                if (i < 0) continue;
                //行
                DataRowView row = dataGridTop.SelectedItems[i] as DataRowView;
                if (row == null) continue;
                long id = DataType.Long(row["Id"], 0);

                //是否有锁定列
                if (row.Row.Table.Columns.Contains("IsSD"))
                {
                    //如行已经锁定 则不可删除
                    bool isSD = DataType.Bool(row["IsSD"], false);
                    if (isSD) continue;
                }

                //是否有锁定列
                if (row.Row.Table.Columns.Contains("IsLock"))
                {
                    //如行已经锁定 则不可删除
                    bool isLock = DataType.Bool(row["IsLock"], false);
                    if (isLock) continue;
                }

                //表维护，系统表不可删除
                if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                {
                    bool isSystemTb = DataType.Bool(row["IsSystem"], false);
                    if (isSystemTb) continue;
                }

                if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                {
                    //表名
                    string deleteTableName = row["TableName"].ToString();
                    //不能删除系统表
                    if (deleteTableName.StartsWith(AppGlobal.SysTableStartWith)) continue;
                }

                if (id <= 0)
                {
                    try
                    {
                        //还没有保存到数据库，直接删除
                        (this.dataGridTop.ItemsSource as DataView).Table.Rows.Remove(row.Row);
                    }
                    catch (Exception ex) { }
                    continue;
                }

                //是否审核
                if (SelectItemIsAudit(row)) continue;

                //要删除的行
                deleteRows.Add(row);

                //编号列表
                ids += id + ",";
            }

            //删除结束
            if (string.IsNullOrWhiteSpace(ids))
            {
                HideLoading();
                //隐藏列表等待
                HideListLoading();
                return;
            }

            //要删除的Id列表
            ids = ids.Trim(',');

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //删除主表参数
                    SQLParam paramDelete = new SQLParam()
                    {
                        TableName = _tableConfig.TableName,
                        Ids = ids
                    };

                    //调用删除服务
                    string msg = string.Empty;
                    bool flag = false;
                    long parentId = 0;

                    //直接删除
                    flag = DataService.Delete(AppGlobal.UserInfo, _tableConfig, paramDelete, ref parentId, ref msg);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            //删除成功
                            AppAlert.FormTips(gridMain, "删除成功！", FormTipsType.Right);

                            for (int i = dataGridTop.SelectedItems.Count - 1; i >= 0; i--)
                            {
                                //行
                                DataRowView row = dataGridTop.SelectedItems[i] as DataRowView;

                                //是否审核
                                bool isAudit = SelectItemIsAudit(row);
                                if (isAudit) continue;

                                //移除行
                                _topPageResult.Data.Rows.Remove(row.Row);
                            }

                            //删除成功事件
                            DeleteSuccess_Event(deleteRows, ids, true);
                        }
                        else
                        {
                            //删除失败
                            if (string.IsNullOrWhiteSpace(msg)) msg = "删除失败";
                            AppAlert.FormTips(gridMain, msg, FormTipsType.Info);
                        }

                        //隐藏等待
                        HideLoading();
                        //隐藏列表等待
                        HideListLoading();
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "删除主表数据异常");
                    //隐藏等待
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 删除明细表数据
        /// </summary>
        private void DeleteDetails(ListAction action)
        {
            //是否有需要删除
            if (dataGridBottom.SelectedItems == null || dataGridBottom.SelectedItems.Count <= 0) return;

            //主表选择的行
            DataRowView rowTop = this.dataGridTop.SelectedItem as DataRowView;

            //要删除的行
            List<DataRowView> deleteRows = new List<DataRowView>();

            //再次询问是否删除
            bool? isDelete = AppAlert.Alert("是否确定删除？\r\n删除后无法恢复！", "是否确定删除？", AlertWindowButton.OkCancel);
            if (!isDelete.HasValue || !isDelete.Value) return;

            //显示等待
            ShowLoading(gridMain);

            //要删除的Id列表
            string ids = "";

            //循环所有行
            for (int i = dataGridBottom.SelectedItems.Count - 1; i >= 0; i--)
            {
                if (i < 0) continue;
                //行
                DataRowView row = dataGridBottom.SelectedItems[i] as DataRowView;
                if (row == null) continue;
                long id = DataType.Long(row["Id"], 0);

                //是否有锁定列
                if (row.Row.Table.Columns.Contains("IsSD"))
                {
                    //如行已经锁定 则不可删除
                    bool isSD = DataType.Bool(row["IsSD"], false);
                    if (isSD) continue;
                }

                //是否有锁定列
                if (row.Row.Table.Columns.Contains("IsLock"))
                {
                    //如行已经锁定 则不可删除
                    bool isLock = DataType.Bool(row["IsLock"], false);
                    if (isLock) continue;
                }

                //表维护，系统列不可删除
                if (_tableConfig.SubTable.TableName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    bool isSystemCell = DataType.Bool(row["IsSystem"], false);
                    if (isSystemCell) continue;
                }

                if (id <= 0)
                {
                    //还没有保存到数据库，直接删除
                    bool flagDRemove = false;
                    try
                    {
                        (this.dataGridBottom.ItemsSource as DataView).Table.Rows.Remove(row.Row);
                        flagDRemove = true;
                    }
                    catch (Exception ex) { }
                    if (!flagDRemove)
                    {
                        try
                        {
                            //直接删除
                            row.Delete();
                        }
                        catch (Exception ex) { }
                    }
                    continue;
                }

                //是否审核
                if (SelectItemIsAudit(row)) continue;

                //要删除的行
                deleteRows.Add(row);

                //编号列表
                ids += id + ",";
            }

            //删除结束
            if (string.IsNullOrWhiteSpace(ids))
            {
                HideLoading();
                //隐藏列表等待
                HideListLoading();
                return;
            }

            //要删除的Id列表
            ids = ids.Trim(',');

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //主表Id
                    long parentId = DataType.Long(rowTop["Id"], 0);

                    //调用删除服务
                    SQLParam paramDelete = new SQLParam()
                    {
                        TableName = _tableConfig.SubTable.TableName,
                        Ids = ids,
                        Wheres = new List<Where>()
                        {
                            new Where("ParentId", parentId)
                        }
                    };

                    string msg = string.Empty;
                    long parentId2 = 0;

                    bool flag = DataService.Delete(AppGlobal.UserInfo, _tableConfig, paramDelete, ref parentId2, ref msg);
                    if (string.IsNullOrWhiteSpace(msg)) msg = "删除失败";

                    if (flag)
                    {
                        //更新主表行
                        UpdateTopRow(parentId);
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        try
                        {
                            if (flag)
                            {
                                //删除成功
                                AppAlert.FormTips(gridMain, "删除成功！", FormTipsType.Right);

                                for (int i = dataGridBottom.SelectedItems.Count - 1; i >= 0; i--)
                                {
                                    //行
                                    DataRowView row = dataGridBottom.SelectedItems[i] as DataRowView;

                                    //是否审核
                                    bool isAudit = SelectItemIsAudit(row);
                                    if (isAudit) continue;

                                    //移除行
                                    _bottomPageResult.Data.Rows.Remove(row.Row);
                                }

                                //删除成功事件
                                DeleteSuccess_Event();
                            }
                            else
                            {
                                //删除失败
                                AppAlert.FormTips(gridMain, msg, FormTipsType.Info);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.AddException(ex, "删除子表数据异常");
                        }

                        HideLoading();
                        //隐藏列表等待
                        HideListLoading();
                        return null;
                    }));
                }
                catch
                {
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 删除成功
        /// </summary>
        private void DeleteSuccess_Event(List<DataRowView> deleteRows = null, string ids = null, bool isDeleteTop = false)
        {
            string tbName = _tableConfig.TableName;
            string detailTBName = _tableConfig.SubTable != null ? _tableConfig.SubTable.TableName : string.Empty;

            try
            {
                if (tbName.Equals(AppGlobal.SysTableName_Modules) || detailTBName.Equals(AppGlobal.SysTableName_ModuleDetails))
                {
                    //更新菜单
                    AppData.MainWindow.ReloadMenus();
                }
            }
            catch { }

            try
            {
                if (detailTBName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    //需要刷新表配置
                    AppData.MainWindow.ShowRenewTC = true;
                }
            }
            catch { }
        }

        #endregion

        /// <summary>
        /// 是否正在保存
        /// </summary>
        bool _isSaving = false;
        /// <summary>
        /// 保存
        /// </summary>
        private void Save(ListAction action)
        {
            if (_isSaving) return;
            _isSaving = true;

            DataGrid dataGrid = null;

            if (_isChooseTop)
            {
                //保存主表
                SaveTopTable(action);

                //操作的表
                dataGrid = this.dataGridTop;
            }
            else
            {
                //保存明细表
                SaveDetailTable(action);

                //操作的表
                dataGrid = this.dataGridBottom;
            }

            //去掉编辑模式
            if (_editRowIndex > -1)
            {
                DataGridPlus.SetRowReadOnly(dataGrid, _editRowIndex);
                _editRowIndex = -1;
            }
        }

        #region 保存扩展
        /// <summary>
        /// 保存主表
        /// </summary>
        private void SaveTopTable(ListAction action)
        {
            List<NeedSaveRow> rows = new List<NeedSaveRow>();
            List<NeedSaveRow> saveFailRows = new List<NeedSaveRow>();
            List<NotCheckPassRow> notCheckPassRows = new List<NotCheckPassRow>();

            if (!(this.dataGridTop.ItemsSource as DataView).Table.Columns.Contains(AppGlobal.DataGridEditStateCellName))
            {
                //没有标识列
                AppAlert.FormTips(gridMain, "没有标识列，无法保存！", FormTipsType.Info);
                _isSaving = false;
                return;
            }

            //检查错误消息
            StringBuilder sbCheckErrorMsg = new StringBuilder();

            #region 是否有添加、更改的行
            for (int i = 0; i < this.dataGridTop.Items.Count; i++)
            {
                //行记录
                DataRowView row = this.dataGridTop.Items[i] as DataRowView;

                DataGridRow gRow = this.dataGridTop.GetRow(i);
                if (gRow != null)
                {
                    gRow.BorderBrush = Brushes.Red;
                    gRow.BorderThickness = new Thickness(0);
                    gRow.ToolTip = null;
                }

                //没有行
                if (row == null || SelectItemIsAudit(row)) continue;

                //是否编辑项
                bool isEdit = DataType.Bool(row[AppGlobal.DataGridEditStateCellName], false);
                //是否新增项
                bool isNew = DataType.Bool(row[AppGlobal.DataGridNewStateCellName], false);

                //非编辑和增加的行
                if (!isEdit && !isNew) continue;

                //添加到待更新行列表
                rows.Add(new NeedSaveRow()
                {
                    Index = i,
                    Row = row
                });
            }
            #endregion

            #region 添加、更改的行提交
            if (rows != null && rows.Count > 0)
            {
                //显示等待
                ShowLoading(gridMain);

                //是否最后一行
                bool isLastRow = false;

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    //是否有父级
                    bool hasParent = _tableConfig.Cells.Exists(p => p.CellName.Equals("ParentId"));

                    //要添加的项列表
                    List<SQLParam> tranSaveRows = new List<SQLParam>();

                    foreach (NeedSaveRow r in rows)
                    {
                        //行
                        DataRowView row = r.Row;

                        //是否最后一行
                        isLastRow = rows.Last().Equals(r);

                        //是否编辑项
                        bool isEdit = DataType.Bool(row[AppGlobal.DataGridEditStateCellName], false);
                        //是否新增项
                        bool isNew = DataType.Bool(row[AppGlobal.DataGridNewStateCellName], false);
                        //原Id
                        long orgId = DataType.Long(row["Id"], 0);

                        //生成关键字
                        if (_tableConfig.Cells.Exists(p => p.CellName.Equals("SearchKeywords")))
                        {
                            row["SearchKeywords"] = BuildKeywords(_tableConfig, row);
                        }

                        if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                        {
                            //表维护
                            string tbName = row["TableName"].ToString();
                            if (!tbName.StartsWith("C_") && !tbName.StartsWith("MZ_") && !tbName.StartsWith("Sys_"))
                            {
                                row["TableName"] = "C_" + row["TableName"];
                            }
                        }

                        #region 添加、更改的行转键值

                        //列值
                        List<KeyValue> kvs = new List<KeyValue>();

                        //验证值是否通过
                        bool checkValuePass = true;
                        //string checkMsg = string.Empty;

                        //所有列-值
                        foreach (CellInfo cell in _tableConfig.Cells)
                        {
                            //审核日期不能设置
                            if (cell.CellName.Equals("AuditDate")) continue;

                            //是否有生成拼音简码
                            if (!string.IsNullOrWhiteSpace(cell.SCPYJM))
                            {
                                row[cell.CellName] = GetSCPYJM(cell, _tableConfig, row);
                            }

                            //是否默认
                            if (cell.CellName.Equals("CreateUserId"))
                            {
                                if (isNew) row["CreateUserId"] = AppGlobal.UserInfo.UserId;
                                else continue;
                            }
                            else if (cell.CellName.Equals("CreateUserName"))
                            {
                                if (isNew) row["CreateUserName"] = AppGlobal.UserInfo.UserName;
                                else continue;
                            }
                            else if (cell.CellName.Equals("SL") && !string.IsNullOrWhiteSpace(cell.Formula))
                            {
                                //数量
                                JSGS(cell, row);
                            }
                            else if (cell.CellName.Equals("DJ") && !string.IsNullOrWhiteSpace(cell.Formula))
                            {
                                //单价
                                JSGS(cell, row);
                            }
                            else if (cell.CellName.Equals("JE"))
                            {
                                //统计金额
                                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.Formula);
                                if (!string.IsNullOrWhiteSpace(cell.Formula) && mc.Count > 0)
                                {
                                    JSGS(cell, row);
                                }
                                else
                                {
                                    if (_tableConfig.Cells.Exists(p => p.CellName.Equals("DJ")) &&
                                        _tableConfig.Cells.Exists(p => p.CellName.Equals("SL")))
                                    {
                                        row["JE"] = DataType.Decimal(row["DJ"], 0) * DataType.Decimal(row["SL"], 0);
                                    }
                                }
                            }
                            else
                            {
                                //验证值
                                if (!cell.AllownNull)
                                {
                                    //不允许为空
                                    if (row[cell.CellName] is DBNull || row[cell.CellName] == null || string.IsNullOrWhiteSpace(row[cell.CellName].ToString()))
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可为空！");
                                        break;
                                    }
                                }

                                if (!cell.AllownRepeat)
                                {
                                    //不允许重复
                                    long parentId = 0;
                                    long currentRowId = DataType.Long(row["Id"].ToString(), 0);
                                    if (hasParent) parentId = DataType.Long(row["ParentId"], 0);
                                    bool needCheckRepeat = true;

                                    if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                                    {
                                        if (row["Type"].ToString().Equals("虚拟"))
                                        {
                                            //虚拟表不需要检查是否存在表名
                                            needCheckRepeat = false;
                                        }
                                    }

                                    //是否重复
                                    if (needCheckRepeat && IsRepeat(_tableConfig.TableName, cell.CellName, row[cell.CellName], currentRowId, parentId))
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]内容重复！");
                                        break;
                                    }
                                }
                            }

                            #region 判断最大最小值
                            if (cell.XZZDZXZ)
                            {
                                //限制最大最小值
                                try
                                {
                                    decimal val = DataType.Decimal(row[cell.CellName], 0);
                                    if (val > cell.ZDZ)
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可大于" + cell.ZDZ + "！");
                                        break;
                                    }
                                    if (val < cell.ZXZ)
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可小于" + cell.ZXZ + "！");
                                        break;
                                    }
                                }
                                catch (Exception ex) { }
                            }
                            #endregion



                            if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                            {
                                //如果是系统表，则表名无法更改
                                bool isSystemTb = DataType.Bool(row["IsSystem"], false);
                                if (cell.CellName.Equals("TableName") && isSystemTb)
                                {
                                    continue;
                                }
                            }

                            //得到列值
                            KeyValue kv = GetCellValue(row, cell);
                            if (kv == null) continue;
                            kvs.Add(kv);
                        }
                        #endregion

                        #region 验证是否可修改
                        if (!isNew)
                        {
                            //修改ID
                            long updateId = DataType.Long(row["Id"], 0);
                            //是否可修改
                            bool flagCanUpdate = CanUpdate(row, updateId, true);
                            if (!flagCanUpdate)
                            {
                                //不可编辑
                                checkValuePass = false;
                                sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：已有引用不可编辑！");
                            }
                        }
                        #endregion

                        #region 是否验证通过
                        if (!checkValuePass)
                        {
                            //验证值未通过，不保存
                            notCheckPassRows.Add(new NotCheckPassRow()
                            {
                                Row = row,
                            });

                            continue;
                        }
                        #endregion

                        #region 是否记忆价格
                        //是否记录价格历史
                        bool isEnablePriceHistorys = _tableConfig.RememberPrice;
                        if (!isEnablePriceHistorys) isEnablePriceHistorys = AppGlobal.GetSysConfigReturnBool("System_EnablePriceHistorys");
                        if (!isEnablePriceHistorys) isEnablePriceHistorys = AppGlobal.GetSysConfigReturnBool("EnablePriceHistorys");
                        _tableConfig.EnablePriceHistorys = isEnablePriceHistorys;
                        #endregion

                        #region 表维护
                        if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                        {
                            //去除空格
                            var kvTableName = kvs.Find(p => p.Key.Equals("TableName"));
                            if (kvTableName != null) kvTableName.Value = kvTableName.Value.ToString().Trim();
                        }
                        #endregion

                        //判断当前行是什么操作
                        if (isNew)
                        {
                            #region 添加
                            //添加
                            SQLParam param = new SQLParam()
                            {
                                Action = Actions.添加,
                                TableName = _tableConfig.TableName,
                                OpreateCells = kvs,

                                RowIndex = r.Index,
                                _ORGID = orgId,
                            };

                            //添加到要插入的列表
                            tranSaveRows.Add(param);

                            #endregion
                        }
                        else
                        {
                            #region 更改
                            //得到编号
                            long id = DataType.Long(row["Id"].ToString(), 0);
                            //没有编号 无法更新
                            if (id <= 0) continue;

                            if (_tableConfig.Cells.Exists(p => p.CellName.Equals("UpdateDate")))
                            {
                                //更新时间
                                row["UpdateDate"] = DateTime.Now;
                            }

                            //原数据
                            DataRow orgRow = null;
                            try
                            {
                                orgRow = _topPageResult.OrgData.Select("[Id]=" + id)[0];
                            }
                            catch { }

                            //调试日志
                            //AppCode.Handler.DebugHandler.AddDebug(_moduleId, this.Title, _tableConfig, "调用修改事务", kvs, orgRow);

                            //更新
                            SQLParam param = new SQLParam()
                            {
                                Action = Actions.修改,
                                Id = id,
                                TableName = _tableConfig.TableName,
                                OpreateCells = kvs,

                                RowIndex = r.Index,
                                _ORGID = orgId,
                            };

                            //添加到要更新的列表
                            tranSaveRows.Add(param);
                            #endregion
                        }
                    }

                    //全都验证通过才提交
                    if (notCheckPassRows.Count > 0)
                    {
                        SaveTopNotCheckPassEvent(notCheckPassRows, sbCheckErrorMsg);
                    }
                    else
                    {
                        //保存提示等待
                        ShowListLoading("请稍候，正在保存...");

                        try
                        {
                            //调用服务保存
                            List<SaveResult> results = DataService.Save(AppGlobal.UserInfo, _tableConfig, tranSaveRows);
                            //保存主表结果
                            SaveTopTableResult(results);
                        }
                        catch (Exception ex)
                        {
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                //清空等待
                                this.listWaiting.Visibility = Visibility.Collapsed;
                                AppAlert.FormTips(gridMain, "保存失败，请重新提交！");
                                return null;
                            }));

                            ErrorHandler.AddException(ex, "保存失败！");
                        }
                        finally
                        {
                            _isSaving = false;
                        }
                    }

                    //保存主表已提交
                    //SaveTopSubmited();
                    HideLoading();
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                //没有需要保存的记录
                AppAlert.FormTips(gridMain, "没有添加或更新任何数据！", FormTipsType.Info);
                _isSaving = false;
            }
            #endregion
        }

        /// <summary>
        /// 保存主表未通过
        /// </summary>
        private void SaveTopNotCheckPassEvent(List<NotCheckPassRow> notCheckPassRows, StringBuilder sbCheckErrorMsg)
        {
            _isSaving = false;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    string tipMsg = "未提交，有" + (notCheckPassRows.Count) + "行未通过！\r\n" + sbCheckErrorMsg.ToString();
                    AppAlert.Alert(tipMsg, "温馨提示", HorizontalAlignment.Left, VerticalAlignment.Top, 400, 300);

                    //隐藏等待
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                }
                catch { }

                return null;
            }));
        }
        /// <summary>
        /// 保存主表已提交
        /// </summary>
        private void SaveTopSubmited()
        {
            _isSaving = false;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //启用
                    this.dataGridTop.IsEnabled = true;
                    this.dataGridBottom.IsEnabled = true;

                    //只读
                    this.dataGridTop.IsReadOnly = true;
                    this.dataGridBottom.IsReadOnly = true;

                    //可排序
                    this.dataGridTop.CanUserSortColumns = true;
                    this.dataGridBottom.CanUserSortColumns = true;

                    //当前无操作
                    _listAction = ListActionEnum.Null;

                    //启用所有按钮
                    DisableListBtns(false);

                    //隐藏等待
                    HideLoading();
                }
                catch { }

                return null;
            }));
        }
        /// <summary>
        /// 保存主表显示结果
        /// </summary>
        private void SaveTopTableResult(List<SaveResult> results)
        {
            _isSaving = false;

            if (results == null || results.Exists(p => p.Success == false))
            {
                //有保存失败的
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //清空等待
                    this.listWaiting.Visibility = Visibility.Collapsed;
                    string tipMsg = "保存失败！";

                    if (results != null)
                    {
                        tipMsg = "保存结束，有" + (results.Count(p => p.Success == false)) + "行保存失败！";
                        foreach (SaveResult result in results)
                        {
                            if (!result.Success)
                            {
                                tipMsg += "\r\n第" + (result.RowIndex + 1) + "行：" + result.Message;
                            }
                        }

                        try
                        {
                            //添加成功的行 取消新行标记
                            var successRows = results.FindAll(p => p.Success && p._ORGID != 0);
                            if (successRows != null)
                            {
                                //标记添加成功的
                                for (int i = 0; i < this.dataGridTop.Items.Count; i++)
                                {
                                    DataRowView row = this.dataGridTop.Items[i] as DataRowView;
                                    var orgId = DataType.Long(row["Id"], 0);
                                    var success = successRows.Find(p => p._ORGID == orgId);
                                    if (success != null)
                                    {
                                        row[AppGlobal.DataGridNewStateCellName] = false;
                                        row[AppGlobal.DataGridEditStateCellName] = false;
                                        row["Id"] = success.ResultId;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    AppAlert.Alert(tipMsg, "温馨提示", HorizontalAlignment.Left, VerticalAlignment.Top, 400, 300);
                    return null;
                }));
                return;
            }

            //保存主表已提交
            SaveTopSubmited();

            //表名
            string tbName = _tableConfig.TableName;

            try
            {
                //插入成功 判断是否模块管理
                if (tbName.Equals(AppGlobal.SysTableName_Modules))
                {
                    //更新菜单
                    AppData.MainWindow.ReloadMenus();
                }
                else if (tbName.Equals(AppGlobal.SysTableName_Tables) ||
                         tbName.Equals(AppGlobal.SysTableName_TableActionEvents) ||
                         tbName.Equals(AppGlobal.SysTableName_TableMenus))
                {
                    //需要刷新表配置
                    AppData.MainWindow.ShowRenewTC = true;
                }
            }
            catch (Exception ex) { }

            try
            {
                if (tbName == "Sys_PrintData")
                {
                    if (!AppGlobal.GetSysConfigReturnBool("System_Guide_SetPrintData"))
                    {
                        //已经设置了打印数据
                        AppGlobal.SetSysConfig("System_Guide_SetPrintData", "true");
                        AppData.MainWindow.ReloadGuide();
                    }
                }
            }
            catch { }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //清空等待
                    this.listWaiting.Visibility = Visibility.Collapsed;

                    //刷新主表
                    if (_listAction == ListActionEnum.开单) ReloadData(true);
                    else ReloadData();
                }
                catch { }

                return null;
            }));
        }

        /// <summary>
        /// 保存从表
        /// </summary>
        private void SaveDetailTable(ListAction action)
        {
            //未能获取数据源
            if (this.dataGridBottom.ItemsSource == null)
            {
                _isSaving = false;
                return;
            }

            if (!(this.dataGridBottom.ItemsSource as DataView).Table.Columns.Contains(AppGlobal.DataGridEditStateCellName))
            {
                //没有标识列
                AppAlert.FormTips(gridMain, "没有标识列，无法保存！", FormTipsType.Info);
                _isSaving = false;
                return;
            }

            //主表记录
            DataRowView rowTop = this.dataGridTop.SelectedItem as DataRowView;
            List<NeedSaveRow> rows = new List<NeedSaveRow>();
            List<NeedSaveRow> saveFailRows = new List<NeedSaveRow>();
            List<NotCheckPassRow> notCheckPassRows = new List<NotCheckPassRow>();

            //主表ID
            long _parentId = DataType.Long(rowTop["Id"], 0);

            //检查错误消息
            StringBuilder sbCheckErrorMsg = new StringBuilder();

            #region 是否有添加、更改的行
            for (int i = 0; i < this.dataGridBottom.Items.Count; i++)
            {
                //行记录
                DataRowView row = this.dataGridBottom.Items[i] as DataRowView;

                DataGridRow gRow = this.dataGridBottom.GetRow(i);
                if (gRow != null)
                {
                    gRow.BorderBrush = Brushes.Red;
                    gRow.BorderThickness = new Thickness(0);
                    gRow.ToolTip = null;
                }

                //没有行
                if (row == null || SelectItemIsAudit(row)) continue;

                //是否编辑项
                bool isEdit = DataType.Bool(row[AppGlobal.DataGridEditStateCellName], false);
                //是否新增项
                bool isNew = DataType.Bool(row[AppGlobal.DataGridNewStateCellName], false);

                //非编辑和增加的行
                if (!isEdit && !isNew) continue;

                //添加到待更新行列表
                rows.Add(new NeedSaveRow()
                {
                    Index = i,
                    Row = row
                });
            }
            #endregion

            #region 添加、更改的行提交
            if (rows != null && rows.Count > 0)
            {
                //显示等待
                ShowLoading(gridMain);

                //是否最后一行
                bool isLastRow = false;

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    //要添加/修改的项列表
                    List<SQLParam> tranSaveRows = new List<SQLParam>();

                    foreach (NeedSaveRow r in rows)
                    {
                        //行
                        DataRowView row = r.Row;

                        //是否最后一行
                        isLastRow = rows.Last().Equals(r);

                        //是否编辑项
                        bool isEdit = DataType.Bool(row[AppGlobal.DataGridEditStateCellName], false);
                        //是否新增项
                        bool isNew = DataType.Bool(row[AppGlobal.DataGridNewStateCellName], false);
                        //原Id
                        long orgId = DataType.Long(row["Id"], 0);

                        //生成关键字
                        if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SearchKeywords")))
                        {
                            row["SearchKeywords"] = BuildKeywords(_tableConfig.SubTable, row);
                        }

                        #region 将行转为更新的键值

                        //列值
                        List<KeyValue> kvs = new List<KeyValue>();

                        //验证值是否通过
                        bool checkValuePass = true;

                        //所有列-值
                        foreach (CellInfo cell in _tableConfig.SubTable.Cells)
                        {
                            //是否有生成拼音简码
                            if (!string.IsNullOrWhiteSpace(cell.SCPYJM))
                            {
                                row[cell.CellName] = GetSCPYJM(cell, _tableConfig.SubTable, row);
                            }

                            //是否默认
                            if (cell.CellName.Equals("CreateUserId"))
                            {
                                if (isNew) row["CreateUserId"] = AppGlobal.UserInfo.UserId;
                                else continue;
                            }
                            else if (cell.CellName.Equals("CreateUserName"))
                            {
                                if (isNew) row["CreateUserName"] = AppGlobal.UserInfo.UserName;
                                else continue;
                            }
                            else if (cell.CellName.Equals("SL") && !string.IsNullOrWhiteSpace(cell.Formula))
                            {
                                //数量
                                JSGS(cell, row);
                            }
                            else if (cell.CellName.Equals("DJ") && !string.IsNullOrWhiteSpace(cell.Formula))
                            {
                                //单价
                                JSGS(cell, row);
                            }
                            else if (cell.CellName.Equals("JE"))
                            {
                                //统计金额
                                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.Formula);
                                if (!string.IsNullOrWhiteSpace(cell.Formula) && mc.Count > 0)
                                {
                                    JSGS(cell, row);
                                }
                                else
                                {
                                    if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("DJ")) &&
                                        _tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SL")))
                                    {
                                        row["JE"] = DataType.Decimal(row["DJ"], 0) * DataType.Decimal(row["SL"], 0);
                                    }
                                }
                            }
                            else
                            {
                                #region 验证值
                                //验证值
                                if (!cell.AllownNull)
                                {
                                    //不允许为空
                                    if (row[cell.CellName] is DBNull || row[cell.CellName] == null || string.IsNullOrWhiteSpace(row[cell.CellName].ToString()))
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可为空！");
                                        break;
                                    }
                                }

                                if (!cell.AllownRepeat)
                                {
                                    //不允许重复
                                    long currentRowId = DataType.Long(row["Id"].ToString(), 0);
                                    long parentId = DataType.Long(row["ParentId"], 0);
                                    bool needCheckRepeat = true;

                                    //是否重复
                                    if (needCheckRepeat && IsRepeat(_tableConfig.SubTable.TableName, cell.CellName, row[cell.CellName], currentRowId, parentId))
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]数据重复！");
                                        break;
                                    }
                                }
                                #endregion
                            }

                            #region 判断最大最小值
                            if (cell.XZZDZXZ)
                            {
                                //限制最大最小值
                                try
                                {
                                    decimal val = DataType.Decimal(row[cell.CellName], 0);
                                    if (val > cell.ZDZ)
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可大于" + cell.ZDZ + "！");
                                        break;
                                    }
                                    if (val < cell.ZXZ)
                                    {
                                        checkValuePass = false;
                                        sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：列[" + cell.CnName + "]不可小于" + cell.ZXZ + "！");
                                        break;
                                    }
                                }
                                catch (Exception ex) { }
                            }
                            #endregion

                            //表维护
                            if (_tableConfig.SubTable.TableName.Equals(AppGlobal.SysTableName_TableCells))
                            {
                                //如果是系统列，则列名无法更改
                                bool isSystemCell = DataType.Bool(row["IsSystem"], false);
                                if (cell.CellName.Equals("CellName") && isSystemCell)
                                {
                                    continue;
                                }
                            }

                            //得到列值
                            KeyValue kv = GetCellValue(row, cell);
                            if (kv == null) continue;
                            kvs.Add(kv);
                        }
                        #endregion

                        #region 验证是否可修改
                        if (!isNew)
                        {
                            //修改ID
                            long updateId = DataType.Long(row["Id"], 0);
                            //是否可修改
                            bool flagCanUpdate = CanUpdate(row, updateId, false);
                            if (!flagCanUpdate)
                            {
                                //不可编辑
                                checkValuePass = false;
                                sbCheckErrorMsg.AppendLine("第" + (r.Index + 1) + "行：已有引用不可编辑！");
                            }
                        }
                        #endregion

                        #region 是否验证通过
                        if (!checkValuePass)
                        {
                            //验证值未通过，不保存
                            notCheckPassRows.Add(new NotCheckPassRow()
                            {
                                Row = row,
                            });
                            continue;
                        }
                        #endregion

                        #region 是否记忆价格历史
                        //是否记录价格历史
                        bool isEnablePriceHistorys = _tableConfig.SubTable.RememberPrice;
                        if (!isEnablePriceHistorys) isEnablePriceHistorys = AppGlobal.GetSysConfigReturnBool("System_EnablePriceHistorys");
                        if (!isEnablePriceHistorys) isEnablePriceHistorys = AppGlobal.GetSysConfigReturnBool("EnablePriceHistorys");
                        _tableConfig.SubTable.EnablePriceHistorys = isEnablePriceHistorys;
                        #endregion

                        #region 列维护
                        if (_tableConfig.SubTable.TableName.Equals(AppGlobal.SysTableName_TableCells) ||
                            _tableConfig.SubTable.TableName.Equals(AppGlobal.SysTableName_TableDefaultCells))
                        {
                            //去除空格
                            var kvCellItem = kvs.Find(p => p.Key.Equals("CellName"));
                            if (kvCellItem != null) kvCellItem.Value = kvCellItem.Value.ToString().Trim();
                        }
                        #endregion

                        //判断当前行是什么操作
                        if (isNew)
                        {
                            #region 添加
                            SQLParam param = new SQLParam()
                            {
                                TableName = _tableConfig.SubTable.TableName,
                                OpreateCells = kvs,

                                RowIndex = r.Index,
                                _ORGID = orgId
                            };

                            //操作到待处理列表
                            tranSaveRows.Add(param);
                            #endregion
                        }
                        else
                        {
                            #region 更新
                            //得到编号
                            long id = DataType.Long(row["Id"].ToString(), 0);
                            //没有编号 无法更新
                            if (id <= 0) continue;

                            if (_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("UpdateDate")))
                            {
                                //更新时间
                                row["UpdateDate"] = DateTime.Now;
                            }

                            //原数据
                            DataRow orgRow = null;
                            try
                            {
                                orgRow = _bottomPageResult.OrgData.Select("[Id]=" + id)[0];
                            }
                            catch { }

                            //更新
                            SQLParam param = new SQLParam()
                            {
                                Action = Actions.修改,
                                Id = id,
                                TableName = _tableConfig.SubTable.TableName,
                                OpreateCells = kvs,

                                RowIndex = r.Index,
                                _ORGID = orgId
                            };

                            //操作到待处理列表
                            tranSaveRows.Add(param);
                            #endregion
                        }
                    }

                    //全都验证通过才提交
                    if (notCheckPassRows.Count > 0)
                    {
                        SaveDetailNotCheckPassEvent(notCheckPassRows, sbCheckErrorMsg);
                    }
                    else
                    {
                        #region 判断是否超过明细记录条数
                        if (_tableConfig.SubTable.MXZDSL > 0)
                        {
                            //判断是否超过明细记录条数
                            int newAddCount = tranSaveRows.Count(p => p.Action == Actions.添加);
                            if (newAddCount > 0)
                            {
                                if (newAddCount > _tableConfig.SubTable.MXZDSL)
                                {
                                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                    {
                                        try
                                        {
                                            //提示
                                            _isSaving = false;
                                            string tipMsg = "未提交，明细最多" + _tableConfig.SubTable.MXZDSL + "行！";
                                            AppAlert.FormTips(gridMain, tipMsg, FormTipsType.Info);
                                        }
                                        catch { }
                                        finally
                                        {
                                            //隐藏等待
                                            HideLoading();
                                        }

                                        return null;
                                    }));

                                    _isSaving = false;
                                    return;
                                }

                                //查询已有记录数
                                SQLParam queryMXJLSL = new SQLParam()
                                {

                                    TableName = _tableConfig.SubTable.TableName,
                                    Wheres = new List<Where>()
                                    {
                                        new Where("ParentId", _parentId)
                                    }
                                };
                                long hasCount = SQLiteDao.GetCount(queryMXJLSL);
                                if (hasCount + newAddCount > _tableConfig.SubTable.MXZDSL)
                                {
                                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                    {
                                        try
                                        {
                                            //提示
                                            _isSaving = false;
                                            string tipMsg = "未提交，明细最多" + _tableConfig.SubTable.MXZDSL + "行！";
                                            AppAlert.FormTips(gridMain, tipMsg, FormTipsType.Info);
                                        }
                                        catch { }
                                        finally
                                        {
                                            //隐藏等待
                                            HideLoading();
                                        }

                                        return null;
                                    }));

                                    _isSaving = false;
                                    return;
                                }
                            }
                        }
                        #endregion

                        //保存提示等待
                        ShowListLoading("请稍候，正在保存...");

                        try
                        {
                            //调用服务保存
                            List<SaveResult> results = DataService.Save(AppGlobal.UserInfo, _tableConfig, tranSaveRows);

                            //保存从表结果
                            SaveDetailTableResult(_parentId, results);
                        }
                        catch (Exception ex)
                        {
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                //清空等待
                                this.listWaiting.Visibility = Visibility.Collapsed;
                                AppAlert.FormTips(gridMain, "保存失败，请重新提交！");
                                return null;
                            }));

                            ErrorHandler.AddException(ex, "保存子表失败！");
                        }
                        finally
                        {
                            _isSaving = false;
                        }
                    }

                    //隐藏提示
                    HideLoading();
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                //没有需要保存的记录
                AppAlert.FormTips(gridMain, "没有添加或更新任何数据！", FormTipsType.Info);
                _isSaving = false;
            }
            #endregion
        }

        /// <summary>
        /// 保存从表未通过
        /// </summary>
        private void SaveDetailNotCheckPassEvent(List<NotCheckPassRow> notCheckPassRows, StringBuilder sbCheckErrorMsg)
        {
            _isSaving = false;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    string tipMsg = "未提交，有" + (notCheckPassRows.Count) + "行未通过！\r\n" + sbCheckErrorMsg.ToString();
                    AppAlert.Alert(tipMsg, "温馨提示", HorizontalAlignment.Left, VerticalAlignment.Top, 400, 300);

                    //隐藏等待
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                }
                catch { }

                return null;
            }));
        }
        /// <summary>
        /// 保存从表已提交
        /// </summary>
        private void SaveDetailSubmited()
        {
            _isSaving = false;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //启用
                this.dataGridTop.IsEnabled = true;
                this.dataGridBottom.IsEnabled = true;

                //只读
                this.dataGridTop.IsReadOnly = true;
                this.dataGridBottom.IsReadOnly = true;

                //可排序
                this.dataGridTop.CanUserSortColumns = true;
                this.dataGridBottom.CanUserSortColumns = true;

                try
                {
                    //取消主表选中样式
                    this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderBrush = Brushes.Blue;
                    this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderThickness = new Thickness(0);
                }
                catch { }

                //当前无操作
                _listAction = ListActionEnum.Null;

                //启用所有按钮
                DisableListBtns(false);

                //隐藏等待
                HideLoading();

                return null;
            }));
        }
        /// <summary>
        /// 保存从表显示结果
        /// </summary>
        /// <param name="view"></param>
        private void SaveDetailTableResult(long parentId, List<SaveResult> results)
        {
            _isSaving = false;

            if (results == null || results.Exists(p => p.Success == false))
            {
                //有保存失败
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //清空等待
                    this.listWaiting.Visibility = Visibility.Collapsed;
                    string tipMsg = "保存失败！";

                    if (results != null)
                    {
                        tipMsg = "保存结束，有" + (results.Count(p => p.Success == false)) + "行保存失败！";
                        foreach (SaveResult result in results)
                        {
                            if (!result.Success)
                            {
                                tipMsg += "\r\n第" + (result.RowIndex + 1) + "行：" + result.Message;
                            }
                        }

                        try
                        {
                            //添加成功的行 取消新行标记
                            var successRows = results.FindAll(p => p.Success && p._ORGID != 0);
                            if (successRows != null)
                            {
                                //标记添加成功的
                                for (int i = 0; i < this.dataGridBottom.Items.Count; i++)
                                {
                                    DataRowView row = this.dataGridBottom.Items[i] as DataRowView;
                                    var orgId = DataType.Long(row["Id"], 0);
                                    var success = successRows.Find(p => p._ORGID == orgId);
                                    if (success != null)
                                    {
                                        row[AppGlobal.DataGridNewStateCellName] = false;
                                        row[AppGlobal.DataGridEditStateCellName] = false;
                                        row["Id"] = success.ResultId;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    AppAlert.Alert(tipMsg, "温馨提示", HorizontalAlignment.Left, VerticalAlignment.Top, 400, 300);
                    return null;
                }));
                return;
            }

            //保存从表已提交
            SaveDetailSubmited();

            //明细表名称
            string detailTBName = _tableConfig.SubTable.TableName;

            //更新主表行
            UpdateTopRow(parentId);

            try
            {
                if (detailTBName.Equals(AppGlobal.SysTableName_TableCells))
                {
                    //需要刷新表配置
                    AppData.MainWindow.ShowRenewTC = true;
                }
            }
            catch { }

            try
            {
                //插入成功 判断是否模块管理
                if (detailTBName.Equals(AppGlobal.SysTableName_ModuleDetails))
                {
                    //更新菜单
                    AppData.MainWindow.ReloadMenus();
                }
            }
            catch { }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //清空等待
                this.listWaiting.Visibility = Visibility.Collapsed;

                try
                {
                    //刷新子表
                    if (_listAction == ListActionEnum.添加) ReloadData(true);
                    else ReloadData();
                }
                catch { }

                return null;
            }));
        }

        /// <summary>
        /// 是否大于库存
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <param name="row"></param>
        /// <param name="kcsl"></param>
        /// <returns></returns>
        private bool IsMaxKCSL(TableInfo tableInfo, DataRow row, ref decimal kcsl, bool isTop = true)
        {
            //是否有数量列
            decimal sl = DataType.Decimal(row["SL"], 0);
            if (sl <= 0) return false;

            string kcbm = tableInfo.SPStockTableName;
            if (string.IsNullOrWhiteSpace(kcbm)) { return false; }

            //是否有商品ID列
            if (!row.Table.Columns.Contains("SPID") || !row.Table.Columns.Contains("SL")) return false;
            long spid = DataType.Long(row["SPID"], 0);
            if (spid <= 0) return false;

            decimal orgSL = 0;

            try
            {
                try
                {
                    //ID
                    long id = DataType.Long(row["Id"], 0);
                    if (id > 0)
                    {
                        //原数据
                        DataTable dtOrg = _topPageResult.OrgData;
                        if (!isTop) dtOrg = _bottomPageResult.OrgData;

                        DataRow orgRow = dtOrg.Select("[Id]=" + id)[0];
                        if (orgRow != null)
                        {
                            //如果修改的数量小于原数量 则 表示库存足够
                            orgSL = DataType.Decimal(orgRow["SL"], 0);
                            if (sl <= orgSL) return false;
                        }
                    }
                }
                catch { }

                //查询参数
                SQLParam param = new SQLParam()
                {
                    TableName = kcbm,
                    Wheres = new List<Where>()
                    {
                        new Where() { CellName = "SPID",CellValue=spid },
                    }
                };

                //库存记录
                DataRow rowKC = SQLiteDao.GetTableRow(param);
                if (!rowKC.Table.Columns.Contains("KCSL")) return false;

                //得到库存数量
                kcsl = DataType.Decimal(rowKC["KCSL"], 0);

                //原库存
                if (tableInfo.KDAddStockCount) kcsl -= orgSL;
                else if (tableInfo.KDReduceStockCount) kcsl += orgSL;

                return sl > kcsl;
            }
            catch { }

            return false;
        }
        /// <summary>
        /// 是否重复
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cellName"></param>
        /// <param name="value"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private bool IsRepeat(string tableName, string cellName, object value, long currentId, long parentId)
        {
            try
            {
                SQLParam param = new SQLParam()
                {

                    TableName = tableName,
                    Wheres = new List<Where>()
                    {
                        new Where() { CellName = cellName,CellValue=value }
                    }
                };

                //是否有当前编号
                if (currentId > 0)
                {
                    param.Wheres.Add(new Where() { CellName = "Id", CellValue = currentId, Type = WhereType.不等于 });
                }
                //是否有上级编号
                if (parentId > 0)
                {
                    param.Wheres.Add(new Where() { CellName = "ParentId", CellValue = parentId });
                }

                //是否有数量
                long count = SQLiteDao.GetCount(param);

                //是否重复
                return count > 0;
            }
            catch (Exception ex) { }

            return true;
        }
        /// <summary>
        /// 锁定引用
        /// </summary>
        /// <param name="tbName"></param>
        /// <param name="id"></param>
        private void LockOrder(string tbName, long id)
        {
            SQLParam param = new SQLParam()
            {

                TableName = tbName,
                Id = id,
                OpreateCells = new List<KeyValue>()
                {
                    new KeyValue("IsSD", true)
                }
            };

            try
            {
                //更新引用
                bool flag = SQLiteDao.Update(param);
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "锁定引用异常");
            }
        }

        #region 计算公式
        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="row"></param>
        private void JSGS(CellInfo cell, DataRowView row)
        {
            //计算公式
            string jsgs = !string.IsNullOrWhiteSpace(cell.ShowGS) ? cell.ShowGS : cell.Formula;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.Formula);

            //保存公式
            if (mc != null && mc.Count > 0)
            {
                try
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        //替换公式的内容
                        jsgs = jsgs.Replace("[" + cellName + "]", row[cellName].ToString());
                    }
                }
                catch (Exception ex)
                {
                    //表名
                    string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                    //添加异常
                    ErrorHandler.AddException(ex, "表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + cell.Formula + "】，计算公式【" + jsgs + "】");
                }
            }

            //计算公式值
            row[cell.CellName] = JSGS(cell, jsgs);
        }
        /// <summary>
        /// 单元格失去焦点 计算保存公式
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="row"></param>
        private object JSGS(CellInfo cell, DataRow row)
        {
            //计算公式
            string jsgs = cell.Formula;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.Formula);

            if (mc != null && mc.Count > 0)
            {
                try
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        //替换公式的内容
                        jsgs = jsgs.Replace("[" + cellName + "]", row[cellName].ToString());
                    }
                }
                catch (Exception ex)
                {
                    //ErrorHandler.AddException(ex, "主表【" + _tableConfig.TableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + cell.Formula + "】，计算公式【" + jsgs + "】");
                    //表名
                    string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                    //添加异常
                    ErrorHandler.AddException(ex, "表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + cell.Formula + "】，计算公式【" + jsgs + "】");
                }
            }

            return JSGS(cell, jsgs);
        }
        /// <summary>
        /// 单元格失去焦点 计算显示公式
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="row"></param>
        private object JSSHOWGS(CellInfo cell, DataRow row)
        {
            //计算公式
            string jsgs = cell.ShowGS;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.ShowGS);

            if (mc != null && mc.Count > 0)
            {
                try
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        //替换公式的内容
                        jsgs = jsgs.Replace("[" + cellName + "]", row[cellName].ToString());
                    }
                }
                catch (Exception ex)
                {
                    //表名
                    string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                    //添加异常
                    ErrorHandler.AddException(ex, "表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + cell.ShowGS + "】，计算公式【" + jsgs + "】");
                }
            }

            return JSGS(cell, jsgs);
        }
        /// <summary>
        /// 单元格失去焦点  计算公式列
        /// </summary>
        /// <param name="jsgs"></param>
        /// <param name="row"></param>
        private void JSGS(CellInfo cell, string jsgs, DataRowView row)
        {
            string runJSGS = jsgs;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(runJSGS);
            if (mc != null && mc.Count > 0)
            {
                try
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        //替换公式的内容
                        runJSGS = runJSGS.Replace("[" + cellName + "]", row[cellName].ToString());
                    }
                }
                catch (Exception ex)
                {
                    //表名
                    string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                    //添加异常
                    ErrorHandler.AddException(ex, "主表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + jsgs + "】，计算公式【" + runJSGS + "】");
                }
            }

            //计算公式值
            row[cell.CellName] = JSGS(cell, runJSGS);
        }
        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="jsgs"></param>
        /// <param name="row"></param>
        private object JSGS(CellInfo cell, string jsgs, DataRow row)
        {
            string runJSGS = jsgs;

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
            System.Text.RegularExpressions.MatchCollection mc = regex.Matches(runJSGS);

            if (mc != null && mc.Count > 0)
            {
                try
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        //替换公式的内容
                        runJSGS = runJSGS.Replace("[" + cellName + "]", row[cellName].ToString());
                    }
                }
                catch (Exception ex)
                {
                    //表名
                    string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                    //添加异常
                    ErrorHandler.AddException(ex, "表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，原公式【" + jsgs + "】，计算公式【" + runJSGS + "】");
                }
            }

            return JSGS(cell, runJSGS);
        }
        /// <summary>
        /// 计算公式
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="jsgs"></param>
        /// <returns></returns>
        private object JSGS(CellInfo cell, string jsgs)
        {
            if (string.IsNullOrWhiteSpace(jsgs)) return 0;

            try
            {
                //列值
                return AppGlobal.JSGS(jsgs, cell.ValType, cell.DecimalDigits);
            }
            catch (Exception ex)
            {
                //表名
                string tableName = _tableConfig.Cells.Exists(p => p.Id == cell.Id) ? _tableConfig.TableName : _tableConfig.SubTable == null ? _tableConfig.TableName : _tableConfig.SubTable.TableName;
                //添加异常
                ErrorHandler.AddException(ex, "主表【" + tableName + "】列【" + cell.CellName + "】计算公式异常，计算公式【" + jsgs + "】");
            }

            return 0;
        }
        #endregion

        /// <summary>
        /// 获取生成拼音简码
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="tableInfo"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private string GetSCPYJM(CellInfo cell, TableInfo tableInfo, DataRowView row)
        {
            try
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\[[^\]]+?\]");
                System.Text.RegularExpressions.MatchCollection mc = regex.Matches(cell.SCPYJM);

                string pyjm = cell.SCPYJM;

                if (mc != null && mc.Count > 0)
                {
                    //公式所有列
                    foreach (System.Text.RegularExpressions.Match m in mc)
                    {
                        //列名
                        string cellName = m.Value.Trim().Trim('[').Trim(']');
                        string value = "";

                        //找到列值
                        CellInfo cellVal = tableInfo.Cells.Find(p => p.CellName.Equals(cellName));
                        if (cellVal != null)
                        {
                            try
                            {
                                value = row[cellVal.CellName].ToString();
                                value = Core.Handler.ChineseHandler.GetFirstPYLetters(value);
                            }
                            catch { }
                        }

                        //替换公式的内容
                        pyjm = pyjm.Replace("[" + cellName + "]", value);
                    }
                }

                //生成拼音简码
                return pyjm;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        /// <summary>
        /// 验证是否可以更新
        /// </summary>
        /// <param name="updateRow"></param>
        /// <param name="updateId"></param>
        private bool CanUpdate(DataRowView updateRow, long updateId, bool isTop)
        {
            //系统配置版本
            int sysConfigVersion = AppGlobal.GetSysConfigReturnInt("System_Config_Version");

            if (_tableConfig.TableType == TableType.单表)
            {
                //是否有完成数量或完成金额
                bool flag = CheckContinueOperate(updateRow, null, updateId, true);
                if (!flag) return false;

                if (_tableConfig.TableSubType == TableSubType.商品表)
                {
                    if (sysConfigVersion < 2)
                    {
                        //原行
                        DataRow orgRow = SQLiteDao.GetTableRow(updateId, _tableConfig.TableName);

                        if (orgRow.Table.Columns.Contains("SPMC"))
                        {
                            if (orgRow["SPMC"].ToString() != updateRow["SPMC"].ToString()) return false;
                        }
                        if (orgRow.Table.Columns.Contains("SPBH"))
                        {
                            if (orgRow["SPBH"].ToString().ToUpper() != updateRow["SPBH"].ToString().ToUpper()) return false;
                        }
                    }
                }
                else if (_tableConfig.TableSubType == TableSubType.客户表 || _tableConfig.TableSubType == TableSubType.供应商表)
                {
                    //客户表
                    if (sysConfigVersion < 2)
                    {
                        //原行
                        DataRow orgRow = SQLiteDao.GetTableRow(updateId, _tableConfig.TableName);

                        decimal yfk = DataType.Decimal(orgRow["YFK"], 0);
                        decimal ysk = DataType.Decimal(orgRow["YSK"], 0);

                        if (orgRow.Table.Columns.Contains("KHMC"))
                        {
                            if (orgRow["KHMC"].ToString() != updateRow["KHMC"].ToString())
                            {
                                //如果修改了客户名称
                                //应收款和应付款如果大于0 则不可修改
                                if (yfk > 0 || ysk > 0) return false;
                            }
                        }
                        if (orgRow.Table.Columns.Contains("KHBH"))
                        {
                            if (orgRow["KHBH"].ToString().ToUpper() != updateRow["KHBH"].ToString().ToUpper())
                            {
                                //如果修改了客户编号
                                //应收款和应付款如果大于0 则不可修改
                                if (yfk > 0 || ysk > 0) return false;
                            }
                        }
                    }
                }
            }
            else if (_tableConfig.TableType == TableType.双表)
            {
                //是修改主表，且主表有客户ID和子表有金额字段
                if (isTop && _tableConfig.Cells.Exists(p => p.CellName.Equals("KHID")))
                {
                    //有客户ID列
                    //查找原数据
                    DataRow[] rows = _topPageResult.OrgData.Select("[Id]=" + updateId);
                    long orgKHID = DataType.Long(rows[0]["KHID"], 0);
                    long newKHID = DataType.Long(updateRow["KHID"], 0);

                    //主表判断是否有明细内容
                    SQLParam paramQuery = new SQLParam()
                    {

                        TableName = _tableConfig.SubTable.TableName,
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="ParentId", CellValue=updateId }
                        }
                    };

                    //是否有明细记录
                    long mxCount = SQLiteDao.GetCount(paramQuery);

                    //非同一个客户 且 有明细数据 不能修改
                    if (orgKHID != newKHID && mxCount > 0) return false;
                }
                else
                {
                    //子表直接判断是否有完成数量或完成金额
                    return CheckContinueOperate(updateRow, null, updateId, isTop);
                }
            }

            //可修改
            return true;
        }
        /// <summary>
        /// 是否可以继续操作：更新、反审
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool CheckContinueOperate(DataRowView row, object parentId, object id, bool isTop, bool isThreeTable = false)
        {
            try
            {
                string cells = "";
                TableInfo operateTable = isThreeTable ? _tableConfig.SubTable.ThreeTable : isTop ? _tableConfig : _tableConfig.SubTable;
                string tableName = operateTable.TableName;

                //是否有被引用数量列
                bool flagHasSYSBYYSL = row.Row.Table.Columns.Contains("SYSBYYSL");
                //是否有完成数量列
                bool flagHasWCSL = row.Row.Table.Columns.Contains("WCSL");
                //是否有完成金额列
                bool flagHasWCJE = row.Row.Table.Columns.Contains("WCJE");

                if (!isTop)
                {
                    flagHasSYSBYYSL = operateTable.Cells.Exists(p => p.CellName.Equals("SYSBYYSL"));
                    flagHasWCSL = operateTable.Cells.Exists(p => p.CellName.Equals("WCSL"));
                    flagHasWCJE = operateTable.Cells.Exists(p => p.CellName.Equals("WCJE"));
                }

                if ((_tableConfig.TableType == TableType.单表 || _tableConfig.TableType == TableType.虚拟) && (
                    _tableConfig.TableSubType == TableSubType.商品表 ||
                    _tableConfig.TableSubType == TableSubType.客户表 || _tableConfig.TableSubType == TableSubType.供应商表))
                {
                    //列出的表不汇总被引用数量
                }
                else
                {
                    if (flagHasSYSBYYSL) cells = "abs(ifnull([SYSBYYSL],0))";
                }

                if (flagHasWCSL) cells = (string.IsNullOrWhiteSpace(cells) ? "" : cells + "+") + "abs(ifnull([WCSL],0))";
                if (flagHasWCJE) cells = (string.IsNullOrWhiteSpace(cells) ? "" : cells + "+") + "abs(ifnull([WCJE],0))";

                if (!string.IsNullOrWhiteSpace(cells))
                {
                    string wheres = null;

                    //查询三表是否被引用
                    if (isThreeTable)
                    {
                        wheres = "[ParentId] in (select [Id] from [" + _tableConfig.SubTable.TableName + "] where [ParentId]=" + id + ")";

                        parentId = 0;
                        id = 0;
                    }

                    DataTable dtSum = QueryService.GetSum(operateTable, parentId, id, cells, wheres);
                    if (dtSum != null && dtSum.Rows.Count > 0)
                    {
                        double val = DataType.Double(dtSum.Rows[0][0], 0);
                        if (val <= 0) return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }
        /// <summary>
        /// 更新主表行
        /// </summary>
        /// <returns></returns>
        private void UpdateTopRow(long topId)
        {
            try
            {
                //更新后的主表行
                DataRow topRowNew = topRowNew = SQLiteDao.GetTableRow(topId, _tableConfig.TableName);
                if (topRowNew != null)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //当前显示的主表行
                        DataRowView topRow = this.dataGridTop.SelectedItem as DataRowView;
                        foreach (CellInfo cell in _tableConfig.Cells)
                        {
                            try
                            {
                                //显示的主表行 是否有此列
                                if (!topRow.Row.Table.Columns.Contains(cell.CellName)) continue;
                                topRow[cell.CellName] = topRowNew[cell.CellName];
                            }
                            catch (Exception ex) { }
                        }
                        return false;
                    }));
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        /// <summary>
        /// 打印
        /// </summary>
        private void Print(ListAction action)
        {
            //是否可打印
            bool flagCanPrint = CanPrint();
            if (!flagCanPrint)
            {
                AppAlert.FormTips(gridMain, "需审核后才可打印！", FormTipsType.Info);
                return;
            }

            //主表选择行
            DataRow topSelectRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

            if (_defaultPrintTemplate == null)
            {
                //查询是否有默认模版
                ShowLoading(gridMain);

                ListAction actionSetting = _listQuicks.Find(p => p.Code == 7);

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        //查询模版参数
                        SQLParam param = new SQLParam()
                        {

                            TableName = AppGlobal.SysTableName_PrintTemplates,
                            Wheres = new List<Where>()
                            {
                                new Where() { CellName="TableId", CellValue=_tableId }
                            }
                        };

                        //查询打印模版
                        DataTable dt = SQLiteDao.GetTable(param);
                        if (dt == null || dt.Rows.Count <= 0)
                        {
                            //隐藏等待
                            HideLoading();
                            //隐藏列表等待
                            HideListLoading();

                            //没有任何打印模版，直接显示打印设置
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                //显示打印设置
                                PrintSetting(actionSetting);
                                return null;
                            }));

                            return;
                        }

                        //是否有默认的打印模版
                        DataRow[] rowDefaults = dt.Select("[IsDefault]=1");

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            if (rowDefaults == null || rowDefaults.Length <= 0)
                            {
                                //有打印模版 但没有默认打印模版
                                //显示打印模版供用户选择
                                PrintSetting(actionSetting);
                            }
                            else
                            {
                                //隐藏等待
                                HideLoading();
                                //隐藏列表等待
                                HideListLoading();

                                //默认打印模版
                                _defaultPrintTemplate = rowDefaults[0];

                                //显示打印预览
                                string code = _defaultPrintTemplate["TemplateCode"].ToString();
                                int version = DataType.Int(_defaultPrintTemplate["Version"], 0);
                                ShowPrintView(code, version);
                            }
                            return null;
                        }));
                    }
                    catch
                    {
                        HideLoading();
                        //隐藏列表等待
                        HideListLoading();
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            AppAlert.FormTips(gridMain, "加载打印模版失败！", FormTipsType.Info);
                            return null;
                        }));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                //显示打印预览
                //有默认打印模版
                string code = _defaultPrintTemplate["TemplateCode"].ToString();
                int version = DataType.Int(_defaultPrintTemplate["Version"], 0);
                ShowPrintView(code, version);
            }
        }

        /// <summary>
        /// 打印设置
        /// </summary>
        private void PrintSetting(ListAction action)
        {
            //隐藏等待
            HideLoading();
            //隐藏列表等待
            HideListLoading();

            //主表选择行
            DataRow topSelectRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

            DataSet ds = null;
            bool flagLoadData = false;
            string refMsg = "";

            try
            {
                //生成打印数据
                flagLoadData = BuildPrintData(out ds, ref refMsg);
            }
            catch (Exception ex) { }

            if (!flagLoadData)
            {
                if (string.IsNullOrWhiteSpace(refMsg)) refMsg = "加载打印数据失败，无法加载打印控件！";
                AppAlert.FormTips(gridMain, refMsg, FormTipsType.Info);
                return;
            }

            Parts.PrintTemplateListUC uc = new Parts.PrintTemplateListUC(_tableConfig.Id, ds, this);
            uc._ParentUC = this;
            uc.Width = 600;
            uc.Height = 400;
            uc.HorizontalAlignment = HorizontalAlignment.Center;
            uc.VerticalAlignment = VerticalAlignment.Center;
            gridMain.Children.Add(uc);

            if (this.gridMain.ColumnDefinitions.Count > 0) Grid.SetColumnSpan(uc, this.gridMain.ColumnDefinitions.Count);
            if (this.gridMain.RowDefinitions.Count > 0) Grid.SetRowSpan(uc, this.gridMain.RowDefinitions.Count);
        }

        #region 打印扩展
        /// <summary>
        /// 是否可以打印
        /// </summary>
        /// <returns></returns>
        private bool CanPrint()
        {
            //是否双表结构
            if (_tableConfig.TableType == TableType.双表)
            {
                //是否有选择行
                if (this.dataGridTop.SelectedItem == null) return false;

                //主表行
                DataRow topRow = (this.dataGridTop.SelectedItem as DataRowView).Row;

                //审核后打印
                if (_tableConfig.SHHDY && _tableConfig.Cells.Exists(p => p.CellName.Equals("IsAudit")))
                {
                    //是否审核
                    bool isAudit = DataType.Bool(topRow["IsAudit"], false);
                    if (!isAudit) return false;
                }
            }

            return true;
        }
        /// <summary>
        /// 显示打印预览
        /// </summary>
        /// <param name="row"></param>
        public void ShowPrintView(string code, int version)
        {
            //是否可打印
            bool flagCanPrint = CanPrint();
            if (!flagCanPrint)
            {
                AppAlert.FormTips(gridMain, "需审核后才可打印！", FormTipsType.Info);
                return;
            }

            try
            {
                //数据集
                DataSet ds = null;
                string refMsg = "";

                //生成打印数据
                bool flagLoadData = BuildPrintData(out ds, ref refMsg);
                if (!flagLoadData)
                {
                    if (string.IsNullOrWhiteSpace(refMsg)) refMsg = "加载打印数据失败，无法加载打印控件！";
                    AppAlert.FormTips(gridMain, refMsg, FormTipsType.Info);
                    return;
                }

                //是否有打印数据
                if (ds == null || ds.Tables.Count <= 0)
                {
                    AppAlert.FormTips(gridMain, "没有打印数据！", FormTipsType.Info);
                    return;
                }

                if (string.IsNullOrWhiteSpace(code))
                {
                    //没有设置报表模版，自动加载默认报表文件
                    AppAlert.FormTips(gridMain, "请先设置打印模版！", FormTipsType.Info);
                    return;
                }

                //是否显示打印预览
                bool showPrintPreview = AppGlobal.GetSysConfigReturnBool("System_UI_ShowPrintPreview", true);

                //调用打印
                //List<BitmapImage> images = AppCode.Handler.PrintHandler.GetPrintImage(ds, code);
                string pdfPath = AppCode.Handler.PrintHandler.GetPrintPdf(ds, code);

                //打印预览
                Components.PdfiumViewerWindow pdfPrintWindow = new Components.PdfiumViewerWindow(pdfPath);
                pdfPrintWindow.Show();

                //释放数据
                ds.Dispose();
                ds = null;
            }
            catch (Exception ex)
            {
                //日志
                ErrorHandler.AddException(ex, "打印【" + _tableConfig.CnName + "-" + _tableConfig.TableName + "】异常，加载打印控件失败！");
                //提示
                AppAlert.FormTips(gridMain, "加载打印控件失败！", FormTipsType.Info);
            }

            //回收垃圾
            AppGlobal.GCCollect();
        }
        /// <summary>
        /// 生成打印数据
        /// </summary>
        /// <param name="ds"></param>
        private bool BuildPrintData(out DataSet ds, ref string refMsg)
        {
            //数据集
            ds = new DataSet("DSPrint");

            if (_bottomPageResult != null && _bottomPageResult.Data != null)
            {
                //双表结构
                //主表
                DataRow topRow = (this.dataGridTop.SelectedItem as DataRowView).Row;
                if (topRow == null || !topRow.Table.Columns.Contains("Id"))
                {
                    refMsg = "请选择主表数据！";
                    return false;
                }

                long topId = DataType.Long(topRow["Id"], 0);
                SQLParam queryTop = new SQLParam()
                {
                    TableName = _tableConfig.TableName,
                    Wheres = new List<Where>()
                    {
                        new Where("Id", topId)
                    }
                };
                DataTable dtMain = SQLiteDao.GetTable(queryTop);
                if (dtMain == null || dtMain.Rows.Count <= 0)
                {
                    refMsg = "主表数据不存在！";
                    return false;
                }

                //子表
                DataTable dtSub = GetOperateData(_bottomPageResult.Data, false);
                if (dtSub == null || dtSub.Rows.Count <= 0)
                {
                    refMsg = "从表没有数据！";
                    return false;
                }

                //判断是否关联数据
                DataRow[] subRows = dtSub.Select("[ParentId]=" + topId);
                if (subRows == null || subRows.Length != dtSub.Rows.Count)
                {
                    refMsg = "主表与从表未关联，请确认是否正在加载！";
                    return false;
                }

                //添加大写金额
                AddDXJEColumn(dtMain);
                //添加套打金额
                AddTDJEColumn(dtMain);
                //处理图片
                ProcessPic(dtMain, _tableConfig);
                //生成二维码列
                AddEWMColumn(dtMain, _tableConfig);

                //处理图片
                ProcessPic(dtSub, _tableConfig.SubTable);
                //生成二维码列
                AddEWMColumn(dtMain, _tableConfig.SubTable);

                //复制表
                DataTable dtZB = dtMain.Copy();
                DataTable dtMX = dtSub.Copy();

                dtZB.TableName = "主表";
                dtMX.TableName = "从表";

                //设置打印表列名
                SetPrintTableColumns(dtZB, _tableConfig);
                SetPrintTableColumns(dtMX, _tableConfig.SubTable);

                ds.Tables.Add(dtZB);
                ds.Tables.Add(dtMX);
            }
            else
            {
                //单表结构
                DataTable dtMain = null;

                if (_tableConfig.DTDY)
                {
                    //单条打印
                    DataRowView dRow = this.dataGridTop.SelectedItem as DataRowView;
                    if (dRow != null)
                    {
                        dtMain = dRow.Row.Table.Clone();
                        dtMain.ImportRow(dRow.Row);
                    }
                }
                else
                {
                    //全表打印
                    dtMain = GetOperateData(_topPageResult.Data, true);
                }

                if (dtMain == null || dtMain.Rows.Count <= 0)
                {
                    refMsg = "主表没有数据，无法加载打印控件！";
                    return false;
                }

                //添加大写金额
                AddDXJEColumn(dtMain);
                //添加套打金额
                AddTDJEColumn(dtMain);
                //处理图片
                ProcessPic(dtMain, _tableConfig);
                //生成二维码列
                AddEWMColumn(dtMain, _tableConfig);

                //复制一个表
                DataTable dtCopy = dtMain.Copy();
                dtCopy.TableName = "主表";

                //设置打印表列名
                SetPrintTableColumns(dtCopy, _tableConfig);

                ds.Tables.Add(dtCopy);
            }

            //其它数据
            BuildElsePrintData(ref ds);

            return true;
        }

        /// <summary>
        /// 设置表列名
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="cell"></param>
        private void SetPrintTableColumns(DataTable dt, TableInfo tableInfo)
        {
            //需要移除的列名
            List<string> removeCells = new List<string>();

            string[] filterCells = { "大写总金额", "大写金额", "套打总金额", "套打金额", "二维码_内容", "二维码_图片" };

            //遍历表名
            foreach (DataColumn col in dt.Columns)
            {
                //过滤的列名
                if (filterCells.Contains(col.ColumnName)) continue;
                if (col.ColumnName.EndsWith("_图片")) continue;

                //表配置的列名
                CellInfo cell = tableInfo.Cells.Find(p => p.CellName == col.ColumnName);
                if (cell == null || !cell.IsShow)
                {
                    removeCells.Add(col.ColumnName);
                }
                else
                {
                    col.ColumnName = cell.CnName;
                }
            }

            //移除表列
            foreach (string cell in removeCells)
            {
                dt.Columns.Remove(cell);
            }
        }

        /// <summary>
        /// 生成二维码列
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableInfo"></param>
        private void AddEWMColumn(DataTable dt, TableInfo tableInfo)
        {
            if (dt == null || dt.Rows.Count <= 0) return;

            //是否生成二维码
            bool buildEWM = AppGlobal.GetSysConfigReturnBool("System_Print_BuildEWMColumn");
            if (!buildEWM) return;

            dt.Columns.Add(new DataColumn("二维码_内容", typeof(string)));
            dt.Columns.Add(new DataColumn("二维码_图片", typeof(byte[])));

            string ewmPath = AppDomain.CurrentDomain.BaseDirectory + "AppData\\EWM\\" + tableInfo.Id + "\\";
            if (!System.IO.Directory.Exists(ewmPath)) System.IO.Directory.CreateDirectory(ewmPath);

            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    //行转Json
                    Dictionary<string, object> dicRow = new Dictionary<string, object>();
                    if (row.Table.Columns.Contains("Id")) dicRow.Add("Id", row["Id"]);
                    if (row.Table.Columns.Contains("BH")) dicRow.Add("BH", row["BH"]);
                    if (row.Table.Columns.Contains("DH")) dicRow.Add("DH", row["DH"]);
                    if (row.Table.Columns.Contains("ZB_BH")) dicRow.Add("ZB_BH", row["ZB_BH"]);
                    if (row.Table.Columns.Contains("ZB_Id")) dicRow.Add("ZB_Id", row["ZB_Id"]);
                    if (row.Table.Columns.Contains("MX_Id")) dicRow.Add("MX_Id", row["MX_Id"]);

                    dicRow.Add("_SYS_TableId", tableInfo.Id);
                    dicRow.Add("_SYS_TableName", tableInfo.TableName);

                    //二维码信息
                    string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(dicRow);
                    row["二维码_内容"] = jsonStr;

                    //二维码路径
                    try
                    {
                        //二维码路径
                        if (row.Table.Columns.Contains("Id")) ewmPath += row["Id"].ToString() + ".jpg";
                        else if (row.Table.Columns.Contains("BH")) ewmPath += row["BH"].ToString() + ".jpg";
                        else if (row.Table.Columns.Contains("DH")) ewmPath += row["DH"].ToString() + ".jpg";
                        else if (row.Table.Columns.Contains("ZB_BH")) ewmPath += row["ZB_BH"].ToString() + ".jpg";
                        else if (row.Table.Columns.Contains("ZB_Id")) ewmPath += row["ZB_Id"].ToString() + ".jpg";
                        else if (row.Table.Columns.Contains("MX_Id")) ewmPath += row["MX_Id"].ToString() + ".jpg";

                        System.Drawing.Bitmap img = Core.AppHandler.BuildEWM(jsonStr);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        row["二维码_图片"] = ms.GetBuffer();
                        img.Dispose();
                    }
                    catch { }
                }
                catch { }
            }
        }

        /// <summary>
        /// 处理图片
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="tableInfo"></param>
        private void ProcessPic(DataTable dt, TableInfo tableInfo)
        {
            if (dt == null || dt.Rows.Count <= 0) return;

            //是否打印图片
            bool printPictures = AppGlobal.GetSysConfigReturnBool("System_Print_PrintPictures");
            if (!printPictures && !_tableConfig.DYTP) return;

            //遍历列
            foreach (CellInfo cell in tableInfo.Cells)
            {
                try
                {
                    //如果不存在列
                    if (!dt.Columns.Contains(cell.CellName)) continue;

                    //图片列生成图片
                    if (cell.IsPic && !dt.Columns.Contains(cell.CnName + "_图片"))
                    {
                        dt.Columns.Add(cell.CellName + "_图片", typeof(byte[]));
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            //内容
                            string content = row[cell.CellName].ToString();
                            if (string.IsNullOrWhiteSpace(content)) continue;

                            string filePath = AppGlobal.GetUploadFilePath(content);
                            if (System.IO.File.Exists(filePath))
                            {
                                byte[] bytes = FileHandler.FileToBytes(filePath);
                                row[cell.CellName + "_图片"] = bytes;
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        /// <summary>
        /// 添加大写列
        /// </summary>
        /// <param name="dt"></param>
        private void AddDXJEColumn(DataTable dt)
        {
            if (dt.Columns.Contains("ZJE"))
            {
                if (!dt.Columns.Contains("大写总金额"))
                {
                    dt.Columns.Add(new DataColumn("大写总金额", typeof(string)));
                }

                foreach (DataRow row in dt.Rows)
                {
                    decimal money = DataType.Decimal(row["ZJE"], 0);
                    string cnMoney = Core.Handler.ChineseHandler.ConvertToCurrency(money);
                    row["大写总金额"] = cnMoney;
                }
            }

            if (dt.Columns.Contains("JE") && !dt.Columns.Contains("JEDX"))
            {
                if (!dt.Columns.Contains("大写金额"))
                {
                    dt.Columns.Add(new DataColumn("大写金额", typeof(string)));
                }

                foreach (DataRow row in dt.Rows)
                {
                    decimal money = DataType.Decimal(row["JE"], 0);
                    string cnMoney = Core.Handler.ChineseHandler.ConvertToCurrency(money);
                    row["大写金额"] = cnMoney;
                }
            }
        }
        /// <summary>
        /// 添加套打金额列
        /// </summary>
        /// <param name="dt"></param>
        private void AddTDJEColumn(DataTable dt)
        {
            if (dt.Columns.Contains("ZJE"))
            {
                if (!dt.Columns.Contains("套打总金额"))
                {
                    dt.Columns.Add(new DataColumn("套打总金额", typeof(string)));
                }

                foreach (DataRow row in dt.Rows)
                {
                    decimal money = DataType.Decimal(row["ZJE"], 0);
                    string cnMoney = MoneyToTD(money);
                    row["套打总金额"] = cnMoney;
                }
            }

            if (dt.Columns.Contains("JE") && !dt.Columns.Contains("JEDX"))
            {
                if (!dt.Columns.Contains("套打金额"))
                {
                    dt.Columns.Add(new DataColumn("套打金额", typeof(string)));
                }

                foreach (DataRow row in dt.Rows)
                {
                    decimal money = DataType.Decimal(row["JE"], 0);
                    string cnMoney = MoneyToTD(money);
                    row["套打金额"] = cnMoney;
                }
            }
        }
        /// <summary>
        /// 转套打金额
        /// </summary>
        private string MoneyToTD(decimal money)
        {
            //零壹贰叁肆伍陆柒捌玖
            string m = money.ToString("0.00");

            m = m.Replace("0", "零");
            m = m.Replace("1", "壹");
            m = m.Replace("2", "贰");
            m = m.Replace("3", "叁");
            m = m.Replace("4", "肆");
            m = m.Replace("5", "伍");
            m = m.Replace("6", "陆");
            m = m.Replace("7", "柒");
            m = m.Replace("8", "捌");
            m = m.Replace("9", "玖");
            m = m.Replace(".", "");

            return m;
        }
        /// <summary>
        /// 生成其它配置数据
        /// </summary>
        /// <param name="ds"></param>
        private void BuildElsePrintData(ref DataSet ds)
        {
            try
            {
                //日期
                DateTime? dtBegin = null;
                DateTime? dtEnd = null;

                DatePicker dp1 = this.panelQuerys.FindName("RQ_From") as DatePicker;
                DatePicker dp2 = this.panelQuerys.FindName("RQ_End") as DatePicker;

                if (dp1 == null)
                {
                    dp1 = this.panelQuerys.FindName("MZZB_RQ_From") as DatePicker;
                    dp2 = this.panelQuerys.FindName("MZZB_RQ_End") as DatePicker;
                }

                if (dp1 != null) dtBegin = dp1.SelectedDate;
                if (dp2 != null) dtEnd = dp2.SelectedDate;

                //表、列
                DataTable dtElseData = new DataTable();
                dtElseData.Columns.Add(new DataColumn("开始日期", typeof(DateTime)));
                dtElseData.Columns.Add(new DataColumn("结束日期", typeof(DateTime)));
                dtElseData.Columns.Add(new DataColumn("操作员", typeof(string)));
                dtElseData.Columns.Add(new DataColumn("模块名称", typeof(string)));
                dtElseData.Columns.Add(new DataColumn("表名称", typeof(string)));

                if (!dtBegin.HasValue)
                {
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("RQ") || ds.Tables[0].Columns.Contains("MZZB_RQ"))
                    {
                        try
                        {
                            //包含日期
                            string cellName = "RQ";
                            if (ds.Tables[0].Columns.Contains("RQ")) cellName = "RQ";
                            else if (ds.Tables[0].Columns.Contains("MZZB_RQ")) cellName = "MZZB_RQ";

                            DataView dv = ds.Tables[0].DefaultView;
                            dv.Sort = "[" + cellName + "] ASC";
                            dtBegin = DataType.DateTime(dv.ToTable().Rows[0][cellName], DateTime.Now);
                        }
                        catch { }
                    }
                    else
                    {
                        dtBegin = DateTime.Now;
                    }
                }
                if (!dtEnd.HasValue)
                {
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns.Contains("RQ") || ds.Tables[0].Columns.Contains("MZZB_RQ"))
                    {
                        try
                        {
                            //包含日期
                            string cellName = "RQ";
                            if (ds.Tables[0].Columns.Contains("RQ")) cellName = "RQ";
                            else if (ds.Tables[0].Columns.Contains("MZZB_RQ")) cellName = "MZZB_RQ";

                            DataView dv = ds.Tables[0].DefaultView;
                            dv.Sort = "[" + cellName + "] DESC";
                            dtEnd = DataType.DateTime(dv.ToTable().Rows[0][cellName], DateTime.Now);
                        }
                        catch { }
                    }
                    else
                    {
                        dtEnd = DateTime.Now;
                    }
                }


                //数据
                DataRow newRow = dtElseData.NewRow();
                newRow["开始日期"] = dtBegin.Value;
                newRow["结束日期"] = dtEnd.Value;
                newRow["操作员"] = AppGlobal.UserInfo.UserName;
                newRow["模块名称"] = _moduleName;
                newRow["表名称"] = _tableConfig.CnName;

                //添加到数据集
                dtElseData.Rows.Add(newRow);
                dtElseData.TableName = "操作数据";
                ds.Tables.Add(dtElseData);
            }
            catch { }

            try
            {
                //加载打印数据
                DataTable dtDYSJ = SQLiteDao.GetTable("Sys_PrintData");
                if (dtDYSJ != null)
                {
                    //获取打印数据表配置
                    TableInfo tableDYSJ = AppGlobal.GetTableConfig("Sys_PrintData");
                    if (tableDYSJ != null) ProcessPic(dtDYSJ, tableDYSJ);
                    dtDYSJ.TableName = "打印数据";
                    SetPrintTableColumns(dtDYSJ, tableDYSJ);
                    ds.Tables.Add(dtDYSJ);
                }
            }
            catch { }

            try
            {
                //是否有扩展打印数据
                if (_extListPrintDataSet != null && _extListPrintDataSet.Tables != null && _extListPrintDataSet.Tables.Count > 0)
                {
                    //复制数据表
                    foreach (DataTable dtExtPrintData in _extListPrintDataSet.Tables)
                    {
                        ds.Tables.Add(dtExtPrintData.Copy());
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 设置默认打印报表
        /// </summary>
        /// <param name="row"></param>
        public void SetDefaultPrintTemplate(DataRow row)
        {
            _defaultPrintTemplate = row;
        }
        #endregion

        private static readonly object _lockAudit = new object();
        private static readonly object _lockUnAudit = new object();

        /// <summary>
        /// 审核
        /// </summary>
        private void AuditRow(ListAction action)
        {
            //是否有选择行
            if (dataGridTop.SelectedItems == null || dataGridTop.SelectedItems.Count <= 0) return;

            //显示等待
            //ShowLoading(gridMain);
            ShowListLoading();

            lock (_lockAudit)
            {
                for (int i = dataGridTop.SelectedItems.Count - 1; i >= 0; i--)
                {
                    if (i < 0) continue;

                    try
                    {
                        //行
                        DataRowView rowView = dataGridTop.SelectedItems[i] as DataRowView;
                        if (rowView == null) continue;
                        long id = DataType.Long(rowView["Id"], 0);
                        if (id <= 0) continue;

                        System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                        {
                            try
                            {
                                bool flag = false;
                                string msg = string.Empty;

                                //审核不可大于库存判断
                                if (_tableConfig.SHBKDYKC || (_tableConfig.SubTable != null && _tableConfig.SubTable.SHBKDYKC))
                                {
                                    //查询大于库存的记录
                                    DataTable dtGreaterThanKCSL = QueryService.AuditCheckStock(_tableConfig, rowView.Row, id);
                                    //是否有超过库存的记录
                                    if (dtGreaterThanKCSL != null && dtGreaterThanKCSL.Rows.Count > 0)
                                    {
                                        string tipsMsg = "";
                                        int rowIndex = 1;
                                        foreach (DataRow rowMore in dtGreaterThanKCSL.Rows)
                                        {
                                            tipsMsg += (rowIndex++).ToString() + "." + rowMore["SPMC"] + "大于实际库存" + rowMore["KCSL"] + "\r\n";
                                        }
                                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                        {
                                            AppAlert.Alert(tipsMsg, "审核失败", AlertWindowButton.Ok, true, HorizontalAlignment.Left, (rowIndex > 4 ? VerticalAlignment.Top : VerticalAlignment.Center));
                                            return null;
                                        }));
                                        return;
                                    }
                                }

                                //有子表，但是没有数据 不可审核
                                if (_tableConfig.SubTable != null)
                                {
                                    SQLParam queryMXSL = new SQLParam()
                                    {

                                        TableName = _tableConfig.SubTable.TableName,
                                        Wheres = new List<Where>()
                                        {
                                            new Where("ParentId", id)
                                        }
                                    };
                                    long count = SQLiteDao.GetCount(queryMXSL);
                                    if (count <= 0)
                                    {
                                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                        {
                                            AppAlert.FormTips(gridMain, "明细表暂无数据，无法审核！", FormTipsType.Info);
                                            return null;
                                        }));
                                        return;
                                    }
                                }

                                try
                                {
                                    //审核
                                    flag = DataService.Audit(AppGlobal.UserInfo, _tableConfig, id, ref msg);
                                }
                                catch (Exception ex) { }

                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    if (flag)
                                    {
                                        //审核成功
                                        rowView["IsAudit"] = true;
                                        rowView["AuditUserId"] = AppGlobal.UserInfo.UserId;
                                        rowView["AuditUserName"] = AppGlobal.UserInfo.UserName;
                                        rowView["AuditDate"] = DateTime.Now;

                                        if (_tableConfig.TableName.Equals(AppGlobal.SysTableName_Tables))
                                        {
                                            rowView["IsBuild"] = true;
                                        }

                                        //禁用按钮
                                        DisableListBtns(true);
                                    }
                                    else
                                    {
                                        //失败
                                        if (string.IsNullOrWhiteSpace(msg)) msg = "审核失败，请重试！";
                                        AppAlert.FormTips(gridMain, msg, FormTipsType.Info);
                                    }

                                    //隐藏提示
                                    HideLoading();
                                    //隐藏列表等待
                                    HideListLoading();
                                    return null;
                                }));
                            }
                            catch (Exception ex)
                            {
                                ErrorHandler.AddException(ex, "审核记录项异常");
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "审核记录异常");
                    }
                }
            }

            //隐藏加载
            HideLoading();
            //隐藏列表等待
            HideListLoading();
        }

        #region 审核扩展
        /// <summary>
        /// 是否可以反审
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CanCancelAudit(long id, DataRowView row)
        {
            if (_tableConfig.TableType == TableType.单表)
            {
                return CheckContinueOperate(row, null, id, true);
            }
            else if (_tableConfig.TableType == TableType.双表)
            {
                //判断子表是否可以“继续操作”
                bool flag = CheckContinueOperate(row, id, null, false);
                if (flag)
                {
                    //判断主表是否可以“继续操作”
                    flag = CheckContinueOperate(row, null, id, true);
                }

                if (flag && _tableConfig.SubTable.ThreeTable != null)
                {
                    //判断三表是否可以“继续操作”
                    flag = CheckContinueOperate(row, null, id, false, true);
                }

                return flag;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// 反审
        /// </summary>
        private void CancelAudit(ListAction action)
        {
            //是否有选择行
            if (dataGridTop.SelectedItems == null || dataGridTop.SelectedItems.Count <= 0) return;

            //再次询问是否反审
            bool? isCancelAudit = AppAlert.Alert("是否确定反审？", "是否确定反审？", AlertWindowButton.OkCancel);
            if (!isCancelAudit.HasValue || !isCancelAudit.Value) return;

            //显示等待
            ShowLoading(gridMain);

            lock (_lockUnAudit)
            {
                for (int i = dataGridTop.SelectedItems.Count - 1; i >= 0; i--)
                {
                    if (i < 0) continue;

                    //行
                    DataRowView rowView = dataGridTop.SelectedItems[i] as DataRowView;
                    if (rowView == null) continue;
                    long id = DataType.Long(rowView["Id"], 0);
                    if (id <= 0) continue;

                    bool canFS = false;

                    try
                    {
                        //查询是否可反审
                        canFS = CanCancelAudit(id, rowView);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "判断是否可反审异常！");
                    }

                    if (!canFS)
                    {
                        AppAlert.FormTips(gridMain, "此单已有引用，无法反审！", FormTipsType.Info);
                        continue;
                    }

                    try
                    {
                        System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                        {
                            bool flag = false;
                            string msg = string.Empty;

                            try
                            {
                                //默认审核流程
                                flag = DataService.CancelAudit(AppGlobal.UserInfo, _tableConfig, id, ref msg);

                                //处理界面
                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    if (flag)
                                    {
                                        //反审成功
                                        rowView["IsAudit"] = false;
                                        rowView["AuditUserId"] = 0;
                                        rowView["AuditUserName"] = "";
                                        rowView["AuditDate"] = DBNull.Value;

                                        //禁用按钮
                                        DisableListBtns(false);
                                    }
                                    else
                                    {
                                        //失败
                                        if (string.IsNullOrWhiteSpace(msg)) msg = "反审失败，请重试！";
                                        AppAlert.FormTips(gridMain, msg, FormTipsType.Info);
                                    }

                                    //隐藏提示
                                    HideLoading();
                                    //隐藏列表等待
                                    HideListLoading();
                                    //刷新数据
                                    _isQueryDataFocusTable = false;
                                    ReloadZB();
                                    return null;
                                }));
                            }
                            catch (Exception ex)
                            {
                                ErrorHandler.AddException(ex, "反审记录项异常");
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "反审记录异常");
                    }
                }
            }

            //隐藏加载
            HideLoading();
            //隐藏列表等待
            HideListLoading();
        }

        /// <summary>
        /// 导入
        /// </summary>
        private void ImportData(ListAction action)
        {
            //主表选择行
            DataRow topSelectRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

            DataRow rowTop = null;

            if (_tableConfig.SubTable != null && (_tableConfig.MXDRSJ || _tableConfig.SubTable.MXDRSJ))
            {
                //双表结构，且可导入明细数据

                //打开选择导入到的表
                Parts.ImportDataChooseUC ucChooseDR = new ImportDataChooseUC();
                ucChooseDR._ParentUC = this;
                ucChooseDR.Width = 300;
                ucChooseDR.Height = 150;
                ucChooseDR.HorizontalAlignment = HorizontalAlignment.Center;
                ucChooseDR.VerticalAlignment = VerticalAlignment.Center;
                ucChooseDR.ChooseImportDataTo_Event += UcChooseDR_ChooseImportDataTo_Event;

                //是否有选择行
                if (dataGridTop.SelectedItems == null || dataGridTop.SelectedItems.Count <= 0) return;
                DataRowView topRowView = dataGridTop.SelectedItems[0] as DataRowView;
                if (topRowView != null)
                {
                    //主表行
                    rowTop = topRowView.Row;
                }

                if (topRowView == null)
                {
                    //没有选择主表记录
                    ucChooseDR.btnMX.IsEnabled = false;
                }
                else if (topRowView.Row.Table.Columns.Contains("IsAudit") && DataType.Bool(topRowView["IsAudit"], false))
                {
                    //有审核列，且已经审核
                    ucChooseDR.btnMX.IsEnabled = false;
                }

                gridMain.Children.Add(ucChooseDR);

                if (this.gridMain.ColumnDefinitions.Count > 0) Grid.SetColumnSpan(ucChooseDR, this.gridMain.ColumnDefinitions.Count);
                if (this.gridMain.RowDefinitions.Count > 0) Grid.SetRowSpan(ucChooseDR, this.gridMain.RowDefinitions.Count);
            }
            else
            {
                //导入数据到主表
                ImportDataTo(true);
            }
        }
        /// <summary>
        /// 导入数据到
        /// </summary>
        /// <param name="to"></param>
        private void UcChooseDR_ChooseImportDataTo_Event(ImportDataTo to)
        {
            //导入数据
            ImportDataTo(to == Parts.ImportDataTo.主表);
        }
        /// <summary>
        /// 导入数据到
        /// </summary>
        /// <param name="importTop">是否导入到主表</param>
        private void ImportDataTo(bool importTop)
        {
            //选择Excel文件
            string filePath = Core.Handler.UploadFileHandler.ChooseFileDialog("Excel文件|*.xls;*.xlsx;");
            if (string.IsNullOrWhiteSpace(filePath)) return;

            //导出文件
            Parts.ImportDataUC uc = null;

            if (importTop)
            {
                //导入到主表
                uc = new Parts.ImportDataUC(_tableConfig, filePath);
            }
            else
            {
                //导入到明细
                DataRowView row = this.dataGridTop.SelectedItem as DataRowView;
                long parentId = DataType.Long(row["Id"], 0);
                uc = new Parts.ImportDataUC(_tableConfig, filePath, parentId);
            }

            Components.PopWindow win = new Components.PopWindow(uc, "导入数据");
            uc._ParentWindow = win;
            uc._ParentUC = this;
            win.ShowDialog();
        }

        /// <summary>
        /// 导出
        /// </summary>
        private void ExportData(ListAction action)
        {
            //主表选择行
            DataRow topSelectRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

            DataTable dt = null;
            string tableType = string.Empty;
            List<CellInfo> cells = new List<CellInfo>();

            if (_isChooseTop)
            {
                tableType = "主表";
                cells = _tableConfig.Cells;

                //获取数据
                dt = GetOperateData(_topPageResult.Data, true);
            }
            else
            {
                tableType = "子表";
                cells = _tableConfig.SubTable.Cells;

                //获取数据
                dt = GetOperateData(_bottomPageResult.Data, false);
            }

            if (dt == null)
            {
                AppAlert.FormTips(gridMain, "没有需要导出的" + tableType + "数据！", FormTipsType.Info);
                return;
            }

            try
            {
                //要移除的列
                List<DataColumn> removeColumns = new List<DataColumn>();

                //表配置
                TableInfo tableInfo = _isChooseTop ? _tableConfig : _tableConfig.SubTable;
                foreach (DataColumn col in dt.Columns)
                {
                    try
                    {
                        //查询列且显示
                        CellInfo cell = tableInfo.Cells.Find(p => p.CellName.Equals(col.ColumnName) && p.IsShow);
                        if (cell != null) continue;

                        //不存在列 或隐藏的列
                        removeColumns.Add(col);
                    }
                    catch (Exception ex) { }
                }

                //移除列
                foreach (DataColumn column in removeColumns)
                {
                    dt.Columns.Remove(column);
                }
            }
            catch (Exception ex) { }

            try
            {
                //列顺序
                int columnIndex = 0;
                if (_isChooseTop && _tableConfig.TableType == TableType.视图)
                {
                    //根据当前的列顺序排序
                    var colnums = this.dataGridTop.Columns.OrderBy(p => p.DisplayIndex);
                    foreach (DataGridColumn col in colnums)
                    {
                        string cellName = col.SortMemberPath;
                        if (!string.IsNullOrWhiteSpace(cellName) && dt.Columns.Contains(cellName))
                        {
                            dt.Columns[columnIndex].SetOrdinal(columnIndex + 1);
                            dt.Columns[cellName].SetOrdinal(columnIndex);
                            columnIndex++;
                        }
                    }
                }
                else
                {
                    //根据表配置排序
                    foreach (CellInfo cell in cells.OrderBy(p => p.UserCellOrder))
                    {
                        if (dt.Columns.Contains(cell.CellName))
                        {
                            dt.Columns[columnIndex].SetOrdinal(columnIndex + 1);
                            dt.Columns[cell.CellName].SetOrdinal(columnIndex);
                            columnIndex++;
                        }
                    }
                }
            }
            catch (Exception ex) { }

            //选择保存目录
            string saveDir = Core.Handler.UploadFileHandler.ChooseFolderDialog();
            if (string.IsNullOrWhiteSpace(saveDir)) return;

            if (!System.IO.Directory.Exists(saveDir))
            {
                System.IO.Directory.CreateDirectory(saveDir);
                return;
            }

            //保存的文件名
            string fileName = _tableConfig.CnName + "-" + DateTime.Now.ToString("yyyyMMdd");
            string fileExt = ".xls";

            //保存的文件路径
            string saveFileName = FileHandler.BuildNotRepeatFilePath(saveDir, fileName, fileExt);

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //导出表
                    AppCode.Handler.NPOIHandler.ExportExcel(dt, saveFileName, cells);

                    //隐藏等待
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();

                    //打开目录
                    System.Diagnostics.Process.Start("Explorer", "/select," + saveFileName);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "导出Excel成功！", FormTipsType.Right);
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "导出Excel失败！", FormTipsType.Info);
                        return null;
                    }));
                    ErrorHandler.AddException(ex, "导出Excel失败！");
                    HideLoading();
                    //隐藏列表等待
                    HideListLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 取消操作
        /// </summary>
        private void CancelAction(ListAction action)
        {
            _isSaving = false;

            //主表选择行
            DataRow topSelectRow = this.dataGridTop.SelectedItem != null ? (this.dataGridTop.SelectedItem as DataRowView).Row : null;

            //取消
            DataGrid dataGrid = null;
            try
            {
                if (_isChooseTop)
                {
                    //主表
                    dataGrid = dataGridTop;
                    dataGrid.ItemsSource = null;
                }
                else
                {
                    //明细表
                    dataGrid = dataGridBottom;
                    dataGrid.ItemsSource = null;

                }
            }
            catch { }

            try
            {
                //取消行选中样式
                this.dataGridTop.GetRow(this.dataGridTop.SelectedIndex).BorderThickness = new Thickness(0);
            }
            catch { }

            //刷新数据
            ReloadData();

            //去掉编辑模式
            if (_editRowIndex > -1)
            {
                DataGridPlus.SetRowReadOnly(dataGrid, _editRowIndex);
                _editRowIndex = -1;
            }

            //当前无操作
            _listAction = ListActionEnum.Null;

            //是否审核
            DisableListBtns(SelectItemIsAudit(this.dataGridTop));

            //只读
            this.dataGridTop.IsReadOnly = true;
            this.dataGridBottom.IsReadOnly = true;

            //启用
            this.dataGridTop.IsEnabled = true;
            this.dataGridBottom.IsEnabled = true;

            //可排序
            this.dataGridTop.CanUserSortColumns = true;
            this.dataGridBottom.CanUserSortColumns = true;

            try
            {
                //滚动到最后一行
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
                dataGrid.SelectedIndex = dataGrid.Items.Count - 1;
            }
            catch { }
        }

        /// <summary>
        /// 查询设置
        /// </summary>
        /// <param name="action"></param>
        private void SetQueryCells(ListAction action)
        {
            Parts.QuerySettingUC uc = new Parts.QuerySettingUC(_tableConfig);
            uc._ParentUC = this;
            uc.Width = 400;
            uc.Height = 400;
            uc.HorizontalAlignment = HorizontalAlignment.Center;
            uc.VerticalAlignment = VerticalAlignment.Center;
            gridMain.Children.Add(uc);

            if (this.gridMain.ColumnDefinitions.Count > 0) Grid.SetColumnSpan(uc, this.gridMain.ColumnDefinitions.Count);
            if (this.gridMain.RowDefinitions.Count > 0) Grid.SetRowSpan(uc, this.gridMain.RowDefinitions.Count);
        }
        /// <summary>
        /// 查询设置回传
        /// </summary>
        /// <param name="listQuerys"></param>
        internal void SetQueryCells_Callback(List<long> listQuerys, UIElement ele)
        {
            AppAlert.FormTips(gridMain, "查询设置成功！");
            this.gridMain.Children.Remove(ele);

            //清空原查询条件
            this.panelQuerys.Children.Clear();
            _queryCells.Clear();
            _queryWhereCtrlCount = 0;

            if (listQuerys == null || listQuerys.Count <= 0)
            {
                //隐藏所有查询条件
                this.gridQueryRow.Height = new GridLength(0);
            }
            else
            {
                //遍历是否有查询
                foreach (CellInfo cell in _tableConfig.Cells)
                {
                    try
                    {
                        if (cell.IsQuery)
                        {
                            if (cell.CellName.Equals("RQ") || cell.CellName.Equals("MZZB_RQ"))
                            {
                                //日期格式
                                this.panelQuerys.UnregisterName(cell.CellName + "_From");
                                this.panelQuerys.UnregisterName(cell.CellName + "_End");
                                cell.IsQuery = false;
                            }
                            else
                            {
                                //如果原列是查询，则取消注册
                                this.panelQuerys.UnregisterName(cell.CellName);
                                cell.IsQuery = false;
                            }
                        }

                        //不是查询列
                        if (!listQuerys.Contains(cell.Id)) continue;

                        //标记为是查询
                        cell.IsQuery = true;

                        //生成查询控件
                        if (BuildQueryCtls(cell))
                        {
                            _queryWhereCtrlCount++;
                        }
                    }
                    catch (Exception ex) { }
                }

                //是否需要生成更多查询条件
                BuildMoreQueryCtls();

                this.gridQueryRow.Height = new GridLength(55);
            }

            //重新计算页面大小
            InitSize();
        }
        /// <summary>
        /// 列排序
        /// </summary>
        /// <param name="action"></param>
        private void SortCellOrder(ListAction action)
        {
            //列排序操作
            _listAction = ListActionEnum.列排序;

            //不可拖动列排序
            this.dataGridTop.CanUserReorderColumns = true;
            this.dataGridBottom.CanUserReorderColumns = true;

            //禁用查询
            this.btnQuery.IsEnabled = false;

            //显示列排序提示
            this.panelSortCellOrder.Visibility = Visibility.Visible;

            if (_tableConfig.TableType == TableType.双表)
            {
                if (_bottomPageResult != null && _bottomPageResult.PageCount <= 1) this.panelSortCellOrder.Margin = new Thickness(0);
                else this.panelSortCellOrder.Margin = new Thickness(0, 0, 0, 50);
            }

            //禁用所有按钮
            DisableListBtns();
        }

        /// <summary>
        /// 右键菜单事件
        /// </summary>
        /// <param name="action">操作事件</param>
        private void RightKeyEvent(MenuInfo info, bool isTop)
        {
            DataGrid dataGrid = null;
            List<DataRow> rows = new List<DataRow>();
            List<NotCheckPassRow2> notPassRows = new List<NotCheckPassRow2>();

            if (isTop)
            {
                //主表
                if (_tableConfig.Menus == null || _tableConfig.Menus.Count <= 0) return;
                dataGrid = this.dataGridTop;
            }
            else
            {
                //子表
                if (_tableConfig.SubTable.Menus == null || _tableConfig.SubTable.Menus.Count <= 0) return;
                dataGrid = this.dataGridBottom;
            }

            foreach (DataRowView row in dataGrid.SelectedItems)
            {
                rows.Add(row.Row);
            }

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //所有行
                foreach (DataRow row in rows)
                {
                    try
                    {
                        //是否有行
                        if (row == null) continue;

                        //原数据
                        long id = 0;
                        string orgData = Newtonsoft.Json.JsonConvert.SerializeObject(row);

                        //编号
                        if (row.Table.Columns.Contains("Id")) id = DataType.Long(row["Id"], 0);

                        //执行SQL
                        List<SQLiteParameter> ps = new List<SQLiteParameter>();

                        foreach (DataColumn col in row.Table.Columns)
                        {
                            ps.Add(new SQLiteParameter() { ParameterName = "@" + col.ColumnName, Value = row[col.ColumnName] });
                        }

                        SQLiteDao.ExecuteSQL(info.ExecuteSQL, ps.ToArray());
                    }
                    catch (Exception ex)
                    {
                        notPassRows.Add(new NotCheckPassRow2()
                        {
                            Message = "执行事件异常",
                            Row = row
                        });
                    }
                }

                //处理未成功的行
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    if (notPassRows != null && notPassRows.Count > 0)
                    {
                        if (rows.Count == 1)
                        {
                            AppAlert.FormTips(gridMain, notPassRows[0].Message, FormTipsType.Info);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "执行结束，有" + notPassRows.Count + "条记录执行失败！", FormTipsType.Info);

                            for (int i = 0; i < dataGrid.Items.Count; i++)
                            {
                                DataRowView r = dataGrid.SelectedItem as DataRowView;
                                NotCheckPassRow2 r2 = notPassRows.Find(p => p.Row == r.Row);

                                if (r2 != null)
                                {
                                    DataGridRow gRow = dataGrid.GetRow(i);
                                    gRow.BorderBrush = Brushes.Red;
                                    gRow.BorderThickness = new Thickness(0, 1, 0, 1);
                                    gRow.ToolTip = r2.Message;
                                }
                            }
                        }
                    }
                    else
                    {
                        AppAlert.FormTips(gridMain, "执行成功！", FormTipsType.Right);
                    }

                    return null;
                }));

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //执行后是否有刷新任务
                    if (info.RefreshTop) { ReloadZB(); }
                    else if (info.RefreshBottom) { ReloadCB(); }
                    return null;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 获取操作数据
        /// </summary>
        /// <returns></returns>
        private DataTable GetOperateData(DataTable dt, bool isChooseTop)
        {
            //查询参数
            SQLParam param = new SQLParam()
            {
                DonotPaging = true,
                Wheres = new List<Where>()
            };

            //排序条件
            List<OrderBy> orderbys = null;
            string dtOrderBy = string.Empty;
            TableInfo tableInfo = _tableConfig;

            if (isChooseTop)
            {
                //选择主表
                param.TableName = _tableConfig.TableName;
                param.WhereSQL = _tableConfig.Wheres;

                //查询条件
                if (_queryWheres != null && _queryWheres.Count > 0)
                {
                    param.Wheres.AddRange(_queryWheres);
                }

                //扩展条件
                if (_extWheres != null && _extWheres.Count > 0)
                {
                    param.Wheres.AddRange(_extWheres);
                }

                orderbys = _topOrderBys;
                dtOrderBy = _topOrderBysSql;

                if (_topOrderDefault && (_tableConfig.IsRealTable || _tableConfig.IsVTable))
                {
                    //主表是默认排序
                    dtOrderBy = "[DataIndex] ASC";
                }
            }
            else
            {
                //选择从表
                tableInfo = _tableConfig.SubTable;
                DataRowView topRow = this.dataGridTop.SelectedItem as DataRowView;

                param.TableName = _tableConfig.SubTable.TableName;
                param.Wheres.Add(new Where() { CellName = "ParentId", CellValue = topRow["Id"] });
                param.WhereSQL = _tableConfig.SubTable.Wheres;

                orderbys = _bottomOrderBys;
                dtOrderBy = _bottomOrderBysSql;

                if (_bottomOrderDefault || (string.IsNullOrWhiteSpace(dtOrderBy) && _tableConfig.SubTable.TableOrderType == TableOrderType.顺序))
                {
                    //从表是默认排序
                    dtOrderBy = "[DataIndex] ASC";
                }
            }

            //查询数据
            param.DonotPaging = true;
            param.PageSize = 9999;
            param.PageIndex = 1;
            param.OrderBys = orderbys;
            param.OrderSQL = dtOrderBy;
            var result = QueryService.GetPaging(tableInfo, AppGlobal.UserInfo, param);
            return result.Data;
        }


        #endregion

        #region 中间栏操作辅助
        /// <summary>
        /// 选择的行是否审核
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        private bool SelectItemIsAudit(DataGrid dataGrid)
        {
            DataRowView row = dataGrid.SelectedItem as DataRowView;
            return SelectItemIsAudit(row);
        }
        /// <summary>
        /// 行是否审核
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool SelectItemIsAudit(DataRowView row)
        {
            if (row == null || !row.Row.Table.Columns.Contains("IsAudit")) return false;
            return DataType.Bool(row["IsAudit"].ToString(), false);
        }

        /// <summary>
        /// 生成关键字
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private string BuildKeywords(TableInfo tabInfos, DataRowView row)
        {
            string keywords = string.Empty;

            foreach (DataColumn col in row.Row.Table.Columns)
            {
                //列名
                string cellName = col.ColumnName.ToUpper();

                //过滤的列
                if (AppGlobal.FilterKeywordsCells.Contains(cellName.ToUpper())) continue;

                if (AppGlobal.BuildSearchKeywordsSuffix.Contains(cellName))
                {
                    //默认搜索关键字
                    string value = row[cellName].ToString();
                    keywords += Core.Handler.ChineseHandler.GetFirstPYLetters(value, false);
                    keywords += value;
                }
                else if (cellName.Contains("NUMBER"))
                {
                    //编号
                    keywords += row[col.ColumnName].ToString();
                }
                else
                {
                    //表列信息
                    CellInfo tbCellInfo = tabInfos.Cells.Find(p => p.CellName.Equals(col.ColumnName));
                    if (tbCellInfo == null) continue; //return keywords.ToUpper();

                    //是否搜索关键字列
                    if (tbCellInfo.IsSearchKeywords)
                    {
                        //包含名称
                        string value = row[cellName].ToString();
                        keywords += Core.Handler.ChineseHandler.GetFirstPYLetters(value, false);
                        keywords += value;
                    }
                }
            }

            return keywords.ToUpper();
        }

        #endregion

        #region 中间栏按钮状态
        /// <summary>
        /// 禁用列表按钮
        /// </summary>
        /// <param name="isAudit">是否审核</param>
        private void DisableListBtns(bool? isAudit = null)
        {
            //启用按钮的值
            Dictionary<int, int[]> dicEnableBtns = new Dictionary<int, int[]>()
            {
                { -7, new int[]{ 0 } },                                 //打开扩展三表
                { -6, new int[]{ 0 } },                                 //查询设置
                { -5, new int[]{ 0 } },                                 //列排序
                { -3, new int[]{ 6, 7, 11 } },                                  //扩展三表上级表已审核
                { -2, new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 10, 11 } },     //未审核
                { -1, new int[]{ 1, 6, 7, 9, 10, 11 } },                    //已审核
                { 0, new int[]{ 0 } },                                  //所有按钮
                { 1, new int[]{ 4, 5, 12 } },                        //开单
                { 2, new int[]{ 2, 4, 5, 12 } },                    //添加
                { 3, new int[]{ 4, 5, 12} },                         //编辑
                { 4, new int[]{ } },                                    //删除
                { 5, new int[]{ } },                                    //保存
                { 6, new int[]{ } },                                    //打印
                { 7, new int[]{ } },                                    //打印设置
                { 8, new int[]{ } },                                    //审核
                { 9, new int[]{ } },                                    //反审
                { 10, new int[]{ } },                                   //导入
                { 11, new int[]{ } },                                   //导出
                { 12, new int[]{ } },                                   //取消
            };

            //操作按钮索引
            int dicIndex = (int)_listAction;

            //是否有标记审核
            if (isAudit.HasValue && _listAction == ListActionEnum.Null)
            {
                dicIndex = isAudit.Value ? -1 : -2;
            }

            //是否第三表（扩展表）的上级主表已经审核
            if (_isPopWindow && _isExtListWindow && _extListIsAudit)
            {
                dicIndex = -3;
            }

            //启用的按钮编号
            List<int> btnEnables = dicEnableBtns[dicIndex].ToList();

            if (btnEnables == null || btnEnables.Count == 0)
            {
                //启用所有按钮
                EnableAllBtns();
                return;
            }

            if (btnEnables != null && btnEnables.Count == 1 && btnEnables[0].Equals(0))
            {
                //禁用所有按钮
                DisEnableAllBtns();
                return;
            }

            if (_listAction == ListActionEnum.开单 || _listAction == ListActionEnum.添加 || _listAction == ListActionEnum.编辑)
            {
                //包含取消操作
                btnEnables.Add((int)ListActionEnum.取消);
            }
            else if (_listAction != ListActionEnum.列排序)
            {
                //列排序
                btnEnables.Add((int)ListActionEnum.列排序);
            }

            if (_listAction != ListActionEnum.开单 && _listAction != ListActionEnum.添加 && _listAction != ListActionEnum.编辑)
            {
                //扩展三表
                btnEnables.Add((int)ListActionEnum.扩展三表);
                //查询设置
                btnEnables.Add((int)ListActionEnum.查询设置);
                //刷新
                btnEnables.Add((int)ListActionEnum.刷新);
            }

            //引用
            btnEnables.Add((int)ListActionEnum.引用);
            //退出
            btnEnables.Add((int)ListActionEnum.退出);

            //遍历操作行所有控件
            foreach (UIElement ele in this.panelActions.Children)
            {
                //是否按钮
                if (!(ele is Button)) continue;

                //禁用所有按钮
                Button btn = ele as Button;
                btn.IsEnabled = false;

                //按钮操作码
                ListAction action = btn.Tag as ListAction;

                //是否启用按钮
                if (btnEnables.Contains(action.Code))
                {
                    //启用
                    btn.IsEnabled = true;
                }
                else if (action.Code >= 100)
                {
                    //自定义的按钮
                    btn.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// 是否显示审核按钮
        /// </summary>
        private void IsShowAuditBtns()
        {
            //没有审批流程
            if (this.dataGridTop.SelectedItem == null) return;

            //选择的行
            DataRowView topRow = this.dataGridTop.SelectedItem as DataRowView;

            Button btnAudit = null;
            Button btnCancelAudit = null;

            foreach (UIElement ele in this.panelActions.Children)
            {
                Button btn = ele as Button;
                ListAction btnAction = btn.Tag as ListAction;
                if (btnAction.Code == 8)
                {
                    btnAudit = btn;
                }
                else if (btnAction.Code == 9)
                {
                    btnCancelAudit = btn;
                }
            }

            int auditIndex = topRow.Row.Table.Columns.Contains("AuditIndex") ? DataType.Int(topRow["AuditIndex"], 0) : 0;
            long createUserId = topRow.Row.Table.Columns.Contains("CreateUserId") ? DataType.Long(topRow["CreateUserId"], 0) : 0;

            if (auditIndex == 0)
            {
                //只显示本用户创建的记录提交审核
                btnAudit.Visibility = Visibility.Collapsed;
                btnCancelAudit.Visibility = Visibility.Collapsed;

                long userId = DataType.Long(topRow["CreateUserId"], 0);
                if (userId == AppGlobal.UserInfo.UserId)
                {
                    btnAudit.Visibility = Visibility.Visible;
                    btnCancelAudit.Visibility = Visibility.Collapsed;
                }
                return;
            }

            if (btnAudit != null)
            {
                //审核按钮
                btnAudit.Visibility = Visibility.Collapsed;
            }
            if (btnCancelAudit != null)
            {
                //反审按钮
                btnCancelAudit.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 启用所有按钮
        /// </summary>
        private void EnableAllBtns()
        {
            foreach (UIElement ele in this.panelActions.Children)
            {
                Button btn = ele as Button;
                btn.IsEnabled = true;
            }

            this.dataGridTop.IsEnabled = true;
            this.dataGridBottom.IsEnabled = true;
        }

        /// <summary>
        /// 禁用所有按钮
        /// </summary>
        private void DisEnableAllBtns()
        {
            foreach (UIElement ele in this.panelActions.Children)
            {
                Button btn = ele as Button;
                btn.IsEnabled = false;
            }
        }
        #endregion

        #region 鼠标移动中间行
        /// <summary>
        /// 是否鼠标在中间栏按下
        /// </summary>
        bool _isMouseDownActions = false;
        double _topHeight = 0;
        double _bottomHeight = 0;
        Point _startPoint = new Point();
        Point _endPoint = new Point();


        /// <summary>
        /// 鼠标按住中间栏移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderActions_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDownActions) return;

            _endPoint = Mouse.GetPosition(this);
            double y = _endPoint.Y - _startPoint.Y;

            try
            {
                double topHeight = _topHeight + y;
                double bottomHeight = _bottomHeight - y;

                this.dataGridTop.MaxHeight = topHeight;
                this.dataGridBottom.MaxHeight = bottomHeight;
                this.panelTreeUC.MaxHeight = bottomHeight;

                this.gridTopRow.Height = new GridLength(topHeight);
                this.gridBottomRow.Height = new GridLength(bottomHeight);

                //比例调整事件
                if (GridBLChange_Event != null)
                {
                    GridBLChange_Event(topHeight, bottomHeight);
                }
            }
            catch { }
        }

        /// <summary>
        /// 鼠标在中间栏放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderActions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDownActions = false;
            this.Cursor = Cursors.Arrow;

            _startPoint = new Point();
            _endPoint = new Point();

            double height = this.gridMain.Height - gridQueryRow.Height.Value - gridActionsRow.Height.Value;
            double topHeight = gridTopRow.Height.Value;
            double bottomHeight = gridBottomRow.Height.Value;

            _topBL = topHeight / height;
            _bottomBL = bottomHeight / height;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //参数
                Dictionary<string, object> dic = new Dictionary<string, object>()
                {

                    { "userId", AppGlobal.UserInfo.UserId },
                    { "tableId", _tableConfig.Id },
                    { "topBL", DataType.Float(_topBL.ToString("0.00"), 0) }
                };

                //主表比例
                _tableConfig.TopBL = _topBL;

                try
                {
                    //修改主表显示比例
                    bool flag = UserService.SetTableTopBL(AppGlobal.UserInfo, _tableConfig.Id, DataType.Float(_topBL.ToString("0.00"), 0));
                    if (flag)
                    {
                        //成功
                    }
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 鼠标在中间栏按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderActions_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.ScrollNS;
            _topHeight = gridTopRow.Height.Value;
            _bottomHeight = gridBottomRow.Height.Value;
            _startPoint = Mouse.GetPosition(this);
            _isMouseDownActions = true;
        }
        #endregion

        #region 从表查询数据事件
        /// <summary>
        /// 延时查询从表数据
        /// </summary>
        System.Timers.Timer _timerQueryBottomData = null;
        /// <summary>
        /// 从表搜索关键字
        /// </summary>
        string _bottomSearchKeywords = string.Empty;
        /// <summary>
        /// 初始搜索从表数据
        /// </summary>
        private void InitSearchBottomData()
        {
            if (_tableConfig.SubTable == null)
            {
                this.borderBottomSearch.Visibility = Visibility.Collapsed;
                return;
            }
            if (!_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SearchKeywords")) &&
                !_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SPMC")) &&
                !_tableConfig.SubTable.Cells.Exists(p => p.CellName.Equals("SPBH")))
            {
                this.borderBottomSearch.Visibility = Visibility.Collapsed;
                return;
            }

            this.borderBottomSearch.Visibility = Visibility.Visible;

            //this.txtSeachBottom.TextChanged += TxtSeachBottom_TextChanged;
            this.txtSeachBottom.PreviewKeyDown += TxtSeachBottom_PreviewKeyDown;

            this.borderBottomSearch.MouseEnter += BorderBottomSearch_MouseEnter;
            this.borderBottomSearch.MouseLeave += BorderBottomSearch_MouseLeave;
            this.txtSeachBottom.LostFocus += TxtSeachBottom_LostFocus;
        }
        /// <summary>
        /// 输入框失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSeachBottom_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_bottomSearchKeywords)) return;

            this.panelSearchBottom.Visibility = Visibility.Collapsed;
            this.lblSearchBottomExp.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderBottomSearch_MouseEnter(object sender, MouseEventArgs e)
        {
            this.panelSearchBottom.Visibility = Visibility.Visible;
            this.lblSearchBottomExp.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderBottomSearch_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_bottomSearchKeywords)) return;

            this.panelSearchBottom.Visibility = Visibility.Collapsed;
            this.lblSearchBottomExp.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 从表输入框内容改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSeachBottom_TextChanged(object sender, TextChangedEventArgs e)
        {
            //从表搜索关键字
            //_bottomSearchKeywords = this.txtSeachBottom.Text.Trim();
        }
        /// <summary>
        /// 当用户按下键盘时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSeachBottom_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //从表搜索关键字
            _bottomSearchKeywords = this.txtSeachBottom.Text.Trim();

            if (_timerQueryBottomData != null)
            {
                _timerQueryBottomData.Stop();
                _timerQueryBottomData.Dispose();
                _timerQueryBottomData = null;
            }

            _timerQueryBottomData = new System.Timers.Timer();
            _timerQueryBottomData.Elapsed += _timerQueryBottomData_Elapsed;
            _timerQueryBottomData.Interval = 1000;
            _timerQueryBottomData.Start();
        }
        /// <summary>
        /// 触发搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerQueryBottomData_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_timerQueryBottomData != null)
            {
                _timerQueryBottomData.Stop();
                _timerQueryBottomData.Dispose();
                _timerQueryBottomData = null;
            }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //从表搜索关键字
                    _bottomSearchKeywords = this.txtSeachBottom.Text.Trim();
                    //重新加载子表数据
                    ReloadCB();
                }
                catch { }
                return null;
            }));
        }
        #endregion

        #region 数据表是否全选
        /// <summary>        
        /// 主表全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTopSelectedAll_Checked(object sender, RoutedEventArgs e)
        {
            this.dataGridTop.SelectAll();
        }

        /// <summary>
        /// 主表全不选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTopSelectedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.dataGridTop.UnselectAll();
        }

        /// <summary>
        /// 子表全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbBottomSelectedAll_Checked(object sender, RoutedEventArgs e)
        {
            this.dataGridBottom.SelectAll();
        }

        /// <summary>
        /// 子表全不选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbBottomSelectedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.dataGridBottom.UnselectAll();
        }
        #endregion

        #region 退出操作
        /// <summary>
        /// 退出操作
        /// </summary>
        public override void CloseUC()
        {
            //是否有表配置 是否更改了宽度或顺序
            if (_tableConfig == null || _tableConfig.Cells == null || _tableConfig.Cells.Count <= 0 || !_columnSizeOrOrderChange) return;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //保存列排序及宽度
                SaveCellOrderAndWidth();

                return null;
            }));
        }
        /// <summary>
        /// 保存列排序及宽度
        /// </summary>
        private void SaveCellOrderAndWidth(bool needReload = false)
        {
            //是否有表配置 是否更改了宽度或顺序
            if (_tableConfig == null || _tableConfig.Cells == null || _tableConfig.Cells.Count <= 0 || !_columnSizeOrOrderChange) return;

            //所有列
            List<Newtonsoft.Json.Linq.JObject> cellsTop = new List<Newtonsoft.Json.Linq.JObject>();
            List<Newtonsoft.Json.Linq.JObject> cellsBottom = null;

            //上下比例
            if (_tableConfig.SubTable != null)
            {
                _tableConfig.TopBL = _topBL;
            }

            foreach (DataGridColumn col in this.dataGridTop.Columns)
            {
                //列名
                string colName = col.SortMemberPath;
                //是否有此列
                CellInfo cellInfo = _tableConfig.Cells.Find(p => p.CellName.Equals(colName));
                if (cellInfo == null) continue;

                int order = col.DisplayIndex;
                int width = 0;
                if (!col.Width.IsAuto)
                {
                    width = Convert.ToInt32(col.Width.Value);
                }

                Newtonsoft.Json.Linq.JObject cell = new Newtonsoft.Json.Linq.JObject();
                cell.Add("CellOrder", order);
                cell.Add("CellWidth", width);
                cell.Add("CellId", cellInfo.Id);

                cellInfo.CellWidth = width;
                cellInfo.UserCellWidth = width;
                cellInfo.UserCellOrder = order;

                //更新排序及宽度
                AppGlobal.SetTableCellOrderAndWidth(_tableConfig.Id, cellInfo.Id, order, width);

                cellsTop.Add(cell);
            }

            if (_tableConfig.SubTable != null && _tableConfig.SubTable.Cells != null && _tableConfig.SubTable.Cells.Count > 0)
            {
                //子表
                cellsBottom = new List<Newtonsoft.Json.Linq.JObject>();

                foreach (DataGridColumn col in this.dataGridBottom.Columns)
                {
                    //列名
                    string colName = col.SortMemberPath;
                    //是否有此列
                    CellInfo cellInfo = _tableConfig.SubTable.Cells.Find(p => p.CellName.Equals(colName));
                    if (cellInfo == null) continue;

                    int order = col.DisplayIndex;

                    int width = 0;
                    if (!col.Width.IsAuto)
                    {
                        width = Convert.ToInt32(col.Width.Value);
                    }

                    Newtonsoft.Json.Linq.JObject cell = new Newtonsoft.Json.Linq.JObject();
                    cell.Add("CellOrder", order);
                    cell.Add("CellWidth", width);
                    cell.Add("CellId", cellInfo.Id);

                    cellInfo.CellWidth = width;
                    cellInfo.UserCellWidth = width;
                    cellInfo.UserCellOrder = order;

                    //更新排序及宽度
                    AppGlobal.SetTableCellOrderAndWidth(_tableConfig.SubTable.Id, cellInfo.Id, order, width);

                    cellsBottom.Add(cell);
                }
            }

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool flag = UserService.SetTableCellWidthOrder(AppGlobal.UserInfo, _tableConfig.Id, cellsTop);
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "设置主表列排序及宽度异常");
                }

                try
                {
                    //是否有子表
                    if (cellsBottom != null)
                    {
                        bool flag = UserService.SetTableCellWidthOrder(AppGlobal.UserInfo, _tableConfig.SubTable.Id, cellsBottom);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "设置子表列排序及宽度异常");
                }

                //重新加载页面
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //标记未修改列排序和宽度
                    _columnSizeOrOrderChange = false;

                    if (_isPopWindow || !needReload)
                    {
                        //弹出窗口保存
                        HideLoading();
                        //隐藏列排序提示
                        this.panelSortCellOrder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //打开列表页面
                        ListUC uc = new ListUC(_tableId, _moduleId, _moduleIds, _moduleName);
                        //关闭页面
                        AppData.MainWindow.RemovePageAndOpenNewPage(UCPageIndex, uc, _moduleId, Title);
                    }

                    return null;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 辅助
        /// <summary>
        /// 找到上级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        /// <summary>
        /// 打印文档
        /// </summary>
        /// <param name="pathStr"></param>
        /// <returns></returns>
        public static bool PrintFile(string pathStr)
        {
            try
            {
                if (System.IO.File.Exists(pathStr) == false) return false;

                var pr = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = pathStr,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        Verb = "Print"
                    }
                };
                pr.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 显示列表等待
        /// </summary>
        /// <param name="tips"></param>
        private void ShowListLoading(string tips = "请稍候...")
        {
            //显示等待
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.listWaiting.Visibility = Visibility.Visible;
                this.listWaiting.RestartTimer();
                this.listWaiting.ToolTip = tips;
                return null;
            }));
        }
        /// <summary>
        /// 隐藏列表等待
        /// </summary>
        private void HideListLoading()
        {
            //隐藏等待
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.listWaiting.Visibility = Visibility.Collapsed;
                return null;
            }));
        }
        #endregion
    }
}
