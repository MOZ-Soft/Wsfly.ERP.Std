using KeQuan.CallService.Models;
using KeQuan.Client.PC.AppCode.Models;
using KeQuan.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace KeQuan.Client.PC.Views.PopWin
{
    /// <summary>
    /// DropDownTable.xaml 的交互逻辑
    /// </summary>
    public partial class DropDownTable : UserControl
    {
        /// <summary>
        /// 委托
        /// </summary>
        public delegate void FlushClientBaseDelegate();

        /// <summary>
        /// 文本改变委托
        /// </summary>
        /// <param name="txt"></param>
        public delegate void Txt_Change_Delegate(DropDownTable uc, string txt, int pageIndex, int pageSize);
        /// <summary>
        /// 文件改变事件
        /// </summary>
        public event Txt_Change_Delegate Txt_Change_Event;

        /// <summary>
        /// 选择行回调委托
        /// </summary>
        /// <param name="row"></param>
        public delegate void ChooseCallBack_Delegate(DropDownTable uc, DataRow row);
        /// <summary>
        /// 选择行回调事件
        /// </summary>
        public event ChooseCallBack_Delegate ChooseCallBack_Event;

        /// <summary>
        /// 数据表配置
        /// </summary>
        public TableInfo _tableInfo;

        /// <summary>
        /// 数据表编号
        /// </summary>
        public long TableId { get; set; }
        /// <summary>
        /// 操作的列名
        /// </summary>
        public string CellName
        {
            get
            {
                //指定返回的列名
                if (!string.IsNullOrWhiteSpace(CellInfo.ReturnCellName)) return CellInfo.ReturnCellName;
                //返回对应默认
                return CellInfo.CellName;
            }
        }
        /// <summary>
        /// 选中后显示的列名
        /// </summary>
        public string ShowCellName { get; set; }
        /// <summary>
        /// 操作列信息
        /// </summary>
        public CellInfo CellInfo { get; set; }
        /// <summary>
        /// 是否已经选择
        /// </summary>
        public bool IsChoosed { get; set; }
        /// <summary>
        /// 是否键盘选择
        /// </summary>
        public bool IsKeboardSelected { get; set; }
        /// <summary>
        /// 选择的行
        /// </summary>
        public DataRowView ChoosedRow { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        /// <summary>
        /// 内容依赖属性
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropDownTable), new PropertyMetadata(""));
        /// <summary>
        /// 是否清除
        /// </summary>
        public bool IsClear { get; set; }
        /// <summary>
        /// 是否鼠标在控件上面
        /// </summary>
        public bool IsHover { get; set; }
        /// <summary>
        /// 是否鼠标在列表上面
        /// </summary>
        public bool IsHoverList { get; set; }
        /// <summary>
        /// 是否生成了列
        /// </summary>
        public bool IsBuildColumns { get; set; }
        /// <summary>
        /// 存储过程参数
        /// </summary>
        public DataTable _TableProcParams { get; set; }
        /// <summary>
        /// 列表页面
        /// </summary>
        public Home.ListUC ListUC { get; set; }
        /// <summary>
        /// 是否显示清空
        /// </summary>
        public bool ShowClear { get; set; }
        /// <summary>
        /// 是否查询
        /// </summary>
        public bool IsQuery { get; set; }
        /// <summary>
        /// 模块编号
        /// </summary>
        public long ModuleId { get; set; }
        /// <summary>
        /// 是否正在加载数据
        /// </summary>
        public bool IsLoadingData { get; set; }
        /// <summary>
        /// 是否显示分页
        /// </summary>
        public bool IsShowPageing { get; set; }
        /// <summary>
        /// 是否显示在下面
        /// </summary>
        public bool IsShowPageingBottom { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public DropDownTable()
        {
            //未加载数据
            IsLoadingData = false;

            //初始
            InitializeComponent();

            //事件
            this.Loaded += DropDownTable_Loaded;
            this.popup.MouseEnter += DropDownTable_MouseEnter;
            this.popup.MouseLeave += DropDownTable_MouseLeave;
            this.listView.MouseDoubleClick += ListView_MouseDoubleClick;
            this.listView.MouseEnter += ListView_MouseEnter;
            this.listView.MouseLeave += ListView_MouseLeave;

            //分页
            this.btnFirst.PreviewMouseDown += BtnFirst_PreviewMouseDown;
            this.btnPrev.PreviewMouseDown += BtnPrev_PreviewMouseDown;
            this.btnNext.PreviewMouseDown += BtnNext_PreviewMouseDown;
            this.btnLast.PreviewMouseDown += BtnLast_PreviewMouseDown;
            this.btnClose.PreviewMouseDown += BtnClose_PreviewMouseDown;

            this.btnFirst2.PreviewMouseDown += BtnFirst_PreviewMouseDown;
            this.btnPrev2.PreviewMouseDown += BtnPrev_PreviewMouseDown;
            this.btnNext2.PreviewMouseDown += BtnNext_PreviewMouseDown;
            this.btnLast2.PreviewMouseDown += BtnLast_PreviewMouseDown;
            this.btnClose2.PreviewMouseDown += BtnClose_PreviewMouseDown;

            this.btnClose3.PreviewMouseDown += BtnClose_PreviewMouseDown;

            this.popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDownTable_Loaded(object sender, RoutedEventArgs e)
        {
            this.txt.Text = Text;

            this.txt.TextChanged += Txt_TextChanged;
            this.txt.GotFocus += Txt_GotFocus;
            this.btnDropDown.Click += BtnDropDown_Click;
            this.btnDropDown.MouseEnter += BtnDropDown_MouseEnter;
            this.btnDropDown.MouseLeave += BtnDropDown_MouseLeave;

            this.txt.PreviewKeyDown += Txt_PreviewKeyDown;
            this.listView.PreviewKeyDown += ListView_PreviewKeyDown;
            this.listView.Name = "lvDropDownTable";
            this.RegisterName(listView.Name, listView);

            if (ShowClear)
            {
                this.colClear.Width = new GridLength(40);
                this.btnClear.Click += BtnClear_Click;
            }
        }

        #region 分页
        /// <summary>
        /// 索引
        /// </summary>
        int _pageIndex = 1;
        /// <summary>
        /// 分页尺码
        /// </summary>
        int _pageSize = 10;
        /// <summary>
        /// 总数量
        /// </summary>
        long _totalCount = 0;
        /// <summary>
        /// 分页模式
        /// </summary>
        int _pageModule = 0;
        /// <summary>
        /// 显示的数据
        /// </summary>
        DataTable _dtData = null;
        /// <summary>
        /// 页数
        /// </summary>
        private int _pageCount
        {
            get
            {
                try { return Core.Handler.MathHandler.CalculatePageCount(_totalCount, _pageSize); }
                catch { }

                return 1;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsLoadingData = false;
            IsHover = false;
            IsHoverList = false;
            IsKeboardSelected = false;
            this.popup.StaysOpen = false;
            this.popup.IsOpen = false;
        }
        /// <summary>
        /// 最后一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLast_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _pageIndex = _pageCount;

            //激活分页按钮
            ActivatePageBtn();

            //加载数据
            LoadDataToShow();
        }
        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNext_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _pageIndex++;

            //激活分页按钮
            ActivatePageBtn();

            //加载数据
            LoadDataToShow();
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrev_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _pageIndex--;

            //激活分页按钮
            ActivatePageBtn();

            //加载数据
            LoadDataToShow();
        }
        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFirst_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _pageIndex = 1;

            //激活分页按钮
            ActivatePageBtn();

            //加载数据
            LoadDataToShow();
        }

        /// <summary>
        /// 激活分页按钮
        /// </summary>
        private void ActivatePageBtn()
        {
            IsShowPageing = false;
            this.panelPages.Visibility = Visibility.Collapsed;
            this.panelPages2.Visibility = Visibility.Collapsed;

            this.btnFirst.IsEnabled = false;
            this.btnPrev.IsEnabled = false;
            this.btnNext.IsEnabled = false;
            this.btnLast.IsEnabled = false;

            this.btnFirst2.IsEnabled = false;
            this.btnPrev2.IsEnabled = false;
            this.btnNext2.IsEnabled = false;
            this.btnLast2.IsEnabled = false;

            if (_pageIndex > 1)
            {
                this.btnFirst.IsEnabled = true;
                this.btnPrev.IsEnabled = true;

                this.btnFirst2.IsEnabled = true;
                this.btnPrev2.IsEnabled = true;
            }

            if (_pageModule == -1)
            {
                //按数据表数量
                //如果大于pageSize则可以点击下一页
                this.btnLast.Visibility = Visibility.Collapsed;
                this.btnLast2.Visibility = Visibility.Collapsed;

                if (_dtData != null && _dtData.Rows.Count >= _pageSize)
                {
                    //有数据且数据大于分页尺码
                    this.btnNext.IsEnabled = true;
                    this.btnNext2.IsEnabled = true;

                    //显示分页
                    //this.panelPages.Visibility = Visibility.Visible;                    
                    //this.panelPages2.Visibility = Visibility.Visible;

                    //显示分页按钮
                    IsShowPageing = true;
                    ShowPagingButtons();
                }

                if (_pageIndex > 1 || (_dtData != null && _dtData.Rows.Count >= _pageSize))
                {
                    //页码大于1 或 数据大于分页尺码
                    //显示分页按钮
                    IsShowPageing = true;
                    ShowPagingButtons();
                }
            }
            else
            {
                //常规分页模式
                if (_pageIndex < _pageCount && _pageCount > 1)
                {
                    this.btnNext.IsEnabled = true;
                    this.btnLast.IsEnabled = true;

                    this.btnNext2.IsEnabled = true;
                    this.btnLast2.IsEnabled = true;
                }

                if (_pageCount > 1)
                {
                    //显示分页
                    //this.panelPages.Visibility = Visibility.Visible;
                    //this.panelPages2.Visibility = Visibility.Visible;

                    //显示分页按钮
                    IsShowPageing = true;
                    ShowPagingButtons();
                }
            }
        }
        /// <summary>
        /// 显示分页按钮
        /// </summary>
        private void ShowPagingButtons()
        {
            if (!IsShowPageing) return;

            Point point = this.listView.TranslatePoint(new Point(), this.txt);

            //不显示无分页的按钮
            this.panelNoPage.Visibility = Visibility.Collapsed;

            if (point.Y < 0 || IsShowPageingBottom)
            {
                //展开在上方
                this.panelPages.Visibility = Visibility.Collapsed;
                this.panelPages2.Visibility = Visibility.Visible;
                this.popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            }
            else
            {
                //展开在下方
                this.panelPages.Visibility = Visibility.Visible;
                this.panelPages2.Visibility = Visibility.Collapsed;
                this.popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            }
        }
        #endregion

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txt.Text = "";
        }

        /// <summary>
        /// 鼠标离开控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDownTable_MouseLeave(object sender, MouseEventArgs e)
        {
            IsHover = false;
            this.popup.StaysOpen = false;
        }

        /// <summary>
        /// 鼠标进入控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropDownTable_MouseEnter(object sender, MouseEventArgs e)
        {
            IsHover = true;
            this.popup.StaysOpen = true;
            ShowPagingButtons();
        }

        /// <summary>
        /// 鼠标离开下拉列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseLeave(object sender, MouseEventArgs e)
        {
            IsHoverList = false;
        }

        /// <summary>
        /// 鼠标进入下拉列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseEnter(object sender, MouseEventArgs e)
        {
            IsHoverList = true;
        }

        /// <summary>
        /// 输入框键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                IsKeboardSelected = true;

                if (this.listView.ItemsSource == null)
                {
                    if (Txt_Change_Event != null)
                    {
                        //文件改变事件
                        Txt_Change_Event(this, this.txt.Text.Trim(), _pageIndex, _pageSize);
                    }
                }

                //展开列表
                //popup.IsOpen = true;
                //焦点给输入框
                this.listView.Focus();
                //e.Source = this;
            }
            else if (e.Key == Key.Enter)
            {
                CallReturnRow();
            }
            else if (e.Key == Key.Left || e.Key == Key.Right)
            {

            }
        }

        /// <summary>
        /// 列表键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CallReturnRow();
            }
            else if (e.Key == Key.Up)
            {
                if (this.listView.SelectedIndex == 0)
                {
                    txt.Focus();
                }
            }

            //e.Source = this;
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDropDown_MouseLeave(object sender, MouseEventArgs e)
        {
            this.btnPolygon.Fill = AppGlobal.ColorToBrush("#333333");
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDropDown_MouseEnter(object sender, MouseEventArgs e)
        {
            this.btnPolygon.Fill = AppGlobal.ColorToBrush("#ffffff");
        }

        /// <summary>
        /// 展开表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDropDown_Click(object sender, RoutedEventArgs e)
        {
            if (Txt_Change_Event != null)
            {
                //文件改变事件
                Txt_Change_Event(this, this.txt.Text.Trim(), 1, 10);
            }
        }

        /// <summary>
        /// 显示数据
        /// </summary>
        /// <param name="dt"></param>
        public void ShowData(DataTable dt, long totalCount, bool needPaging = true)
        {
            //显示的数据
            _dtData = dt;

            //选择的行为空
            ChoosedRow = null;
            
            //不显示无分页的按钮
            this.panelNoPage.Visibility = Visibility.Visible;

            if (needPaging)
            {
                //总数量
                _totalCount = totalCount;
                if (_totalCount == -1)
                {
                    //存储过程查询结果 没有返回总记录数
                    _pageModule = -1;
                }

                //激活分页按钮
                ActivatePageBtn();
            }

            if (dt == null)
            {
                this.listView.ItemsSource = null;
                popup.IsOpen = false;
                return;
            }

            if (_tableInfo != null)
            {
                //数据表名
                dt.TableName = _tableInfo.TableName;
            }

            //是否有表配置
            if (_tableInfo == null)
            {
                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    //加载表配置
                    LoadTableConfigs();

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //创建列
                        BuildColumns();
                        //已生成列
                        IsBuildColumns = true;

                        this.listView.ItemsSource = dt.DefaultView;
                        popup.IsOpen = true;
                    }));
                });
                thread.IsBackground = true;
                thread.Start();
            }
            //已加载表配置，但是未生成列
            else if (_tableInfo != null && !IsBuildColumns)
            {
                //创建列
                BuildColumns();
                //已生成列
                IsBuildColumns = true;

                this.listView.ItemsSource = dt.DefaultView;
                popup.IsOpen = true;
            }
            //直接绑定数据源
            else
            {
                this.listView.ItemsSource = dt.DefaultView;
                popup.IsOpen = true;
            }

            IsChoosed = false;
        }

        /// <summary>
        /// 创建表列
        /// </summary>
        private void BuildColumns()
        {
            if (_tableInfo == null) return;

            GridView view = this.listView.View as GridView;
            view.Columns.Clear();

            try
            {
                bool popShow = _tableInfo.Cells.Exists(p => p.IsPopShow);

                foreach (CellInfo cell in _tableInfo.Cells)
                {
                    //是否过滤
                    if (cell == null || !cell.IsShow || cell.ValType.Equals("bool")) continue;

                    if (popShow)
                    {
                        //用户选择显示的列
                        if (!cell.IsPopShow) continue;
                    }
                    else
                    {
                        //默认过滤的列
                        if (AppGlobal.DropDownTable_BuildFilterCells.Contains(cell.CellName.ToUpper())) continue;
                    }

                    //绑定
                    Binding binding = new Binding(cell.CellName);

                    //日期
                    if (cell.ValType.Equals("date") || cell.ValType.Equals("datetime"))
                    {
                        //绑定格式
                        binding.StringFormat = cell.ValType.Equals("date") ? "{0:yyyy-MM-dd}" : "{0:yyyy-MM-dd HH:mm:ss}";
                    }

                    //视图列
                    GridViewColumn col = new GridViewColumn();
                    col.Header = cell.CnName;
                    col.DisplayMemberBinding = binding;

                    //添加到列
                    view.Columns.Add(col);
                }
            }
            catch { }
        }

        /// <summary>
        /// 加载表列
        /// </summary>
        public void LoadTableConfigs()
        {
            if (ListUC != null)
            {
                //获取表配置
                _tableInfo = ListUC.GetRelationTable(TableId);
                return;
            }

            //加载表配置
            _tableInfo = AppGlobal.LoadTableConfigInfo(TableId);
        }


        /// <summary>
        /// 聚焦输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = true;
        }

        /// <summary>
        /// 内容改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //第一页
            _pageIndex = 1;

            //选择的行为空
            ChoosedRow = null;

            //加载数据显示
            LoadDataToShow();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadDataToShow()
        {
            IsLoadingData = true;

            //内容
            Text = this.txt.Text.Trim();

            //清空内容
            if (string.IsNullOrWhiteSpace(Text)) IsClear = true;
            else IsClear = false;

            if (Txt_Change_Event != null)
            {
                //文件改变事件
                Txt_Change_Event(this, Text, _pageIndex, _pageSize);
            }
        }

        /// <summary>
        /// 双击得到行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CallReturnRow();
        }
        /// <summary>
        /// 调用回调行
        /// </summary>
        private void CallReturnRow()
        {
            //选择行
            DataRowView row = null;

            //没有选择行
            if (this.listView.SelectedItem == null)
            {
                if (this.listView.Items.Count > 0)
                {
                    //默认为首行
                    row = this.listView.Items[0] as DataRowView;
                }
                else
                {
                    //无行可选择
                    return;
                }
            }
            else
            {
                //选择行
                row = this.listView.SelectedItem as DataRowView;
            }

            //标记已经选择
            IsChoosed = true;
            IsKeboardSelected = false;

            if (CellInfo != null && row.Row.Table.Columns.Contains(CellName))
            {
                //选择的值
                txt.Text = row[CellName].ToString();
                //更新绑定的值
                Text = txt.Text;
            }
            else if (!string.IsNullOrWhiteSpace(ShowCellName) && row.Row.Table.Columns.Contains(ShowCellName))
            {
                //选择的值
                txt.Text = row[ShowCellName].ToString();
                //更新绑定的值
                Text = txt.Text;
            }
            else if (row.Row.Table.Columns.Contains("Id"))
            {
                //选择的编号
                txt.Text = row["Id"].ToString();
                //更新绑定的值
                Text = txt.Text;
            }

            //选择的行
            ChoosedRow = row;

            //返回的行
            DataRow returnRow = row.Row;

            //添加指定返回的列
            AddTSColumn(returnRow);

            //选择行回调
            if (ChooseCallBack_Event != null)
            {
                //回调
                ChooseCallBack_Event(this, returnRow);
            }

            popup.IsOpen = false;
        }
        /// <summary>
        /// 添加指定返回的列
        /// </summary>
        /// <param name="returnRow"></param>
        private void AddTSColumn(DataRow returnRow)
        {
            string newColumnName = null;

            if (_tableInfo.TableSubType == TableSubType.物料表 ||
               _tableInfo.TableSubType == TableSubType.半成品表 ||
               _tableInfo.TableSubType == TableSubType.成品表)
            {
                //返回增加列 SPID
                newColumnName = "SPID";
            }
            else if (_tableInfo.TableSubType == TableSubType.客户表 ||
               _tableInfo.TableSubType == TableSubType.供应商表)
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
        }
    }
}
