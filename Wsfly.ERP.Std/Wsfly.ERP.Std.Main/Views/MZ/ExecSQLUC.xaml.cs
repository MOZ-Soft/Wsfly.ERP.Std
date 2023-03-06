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
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// ExecSQLUC.xaml 的交互逻辑
    /// </summary>
    public partial class ExecSQLUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ExecSQLUC()
        {
            InitializeComponent();

            this.Loaded += ExecSQLUC_Loaded;
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecSQLUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnExec.Click += BtnExec_Click;
            this.btnClear.Click += BtnClear_Click;
            
            this.txtSearch.TextChanged += TxtSearch_TextChanged;
            this.txtSearch.GotFocus += TxtSearch_GotFocus;
            this.txtSearch.LostFocus += TxtSearch_LostFocus;
            this.tvTables.SelectedItemChanged += TvTables_SelectedItemChanged;

            this.txtSql.PreviewKeyDown += TxtSql_PreviewKeyDown;

            LoadTables();

            InitSize();
        }

        /// <summary>
        /// 按键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSql_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                //粘贴
                string code = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    code = code.Replace("\t", "    ");
                    Clipboard.SetText(code);
                }
            }

            if (e.Key == Key.Tab)
            {
                InsertSQL("    ");
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.F5)
            {
                ExecuteSQL();
                e.Handled = true;
                return;
            }
        }
        

        /// <summary>
        /// 插入SQL
        /// </summary>
        /// <param name="insertSql"></param>
        private void InsertSQL(string insertSql)
        {
            int sIndex = this.txtSql.SelectionStart;
            string sql = this.txtSql.Text;
            sql = sql.Insert(sIndex, insertSql);
            this.txtSql.Text = sql;
            this.txtSql.SelectionStart = sIndex + insertSql.Length;
        }

        /// <summary>
        /// 搜索失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtSearch.Text)) this.txtSearch.Text = "请输入要搜索的表名";
        }

        /// <summary>
        /// 搜索得到焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.txtSearch.Text.Equals("请输入要搜索的表名")) this.txtSearch.Text = "";
        }

        /// <summary>
        /// 搜索变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.txtSearch.Text.Equals("请输入要搜索的表名")) return;
            if (!this.tvTables.HasItems) return;

            //关键字
            string keyword = this.txtSearch.Text.Trim().ToUpper();

            //根节点
            TreeViewItem item = this.tvTables.Items[0] as TreeViewItem;

            //所有表
            for (int i = 0; i < item.Items.Count; i++)
            {
                //子项
                TreeViewItem subItem = (item.Items[i] as TreeViewItem);
                if (subItem == null) continue;

                //所有显示
                subItem.Visibility = Visibility.Visible;

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    TableItem tItem = subItem.Tag as TableItem;
                    if (tItem == null) continue;

                    if (!tItem.ShowName.ToUpper().Contains(keyword))
                    {
                        //表名称中不包含关键字
                        subItem.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// 选择数据表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvTables_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvTables.SelectedItem == null) return;

            TreeViewItem nodeItem = this.tvTables.SelectedItem as TreeViewItem;
            if (nodeItem == null || nodeItem.Tag == null) return;

            TableItem item = nodeItem.Tag as TableItem;

            string appendText = "";

            if (item.IsFilter) return;
            else if (item.IsTable)
            {
                string tbName = item.TableName;
                appendText = " [" + tbName + "]";
            }
            else
            {
                string cellName = item.CellName;
                if (string.IsNullOrWhiteSpace(cellName)) return;
                appendText = " [" + cellName + "]";
            }

            //插入
            InsertSQL(appendText);
        }

        /// <summary>
        /// 加载所有表
        /// </summary>
        private void LoadTables()
        {
            System.Threading.Thread threadLoadTables = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = AppGlobal.SysTableName_Tables,
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy() { CellName="TableName" }
                        }
                    };

                    //查询所有表
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0) return;

                    //要显示的列表
                    List<TableItem> list = new List<TableItem>();

                    //遍历表
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new TableItem()
                        {
                            IsTable = true,
                            Id = DataType.Long(row["Id"], 0),
                            CnName = row["CnName"].ToString(),
                            TableName = row["TableName"].ToString(),
                            ShowName = row["CnName"] + "[" + row["TableName"] + "]"
                        });
                    }

                    //绑定
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        TreeViewItem nodeRoot = new TreeViewItem();
                        nodeRoot.Header = "所有数据表";
                        nodeRoot.IsExpanded = true;

                        foreach (TableItem item in list)
                        {
                            TreeViewItem nodeTable = new TreeViewItem();
                            nodeTable.Header = item.ShowName;
                            nodeTable.IsExpanded = false;
                            nodeTable.Tag = item;
                            nodeTable.Expanded += NodeTable_Expanded;

                            TreeViewItem nodeCell = new TreeViewItem();
                            nodeCell.Header = "正在加载...";
                            nodeCell.IsEnabled = false;
                            nodeTable.Items.Add(nodeCell);

                            nodeRoot.Items.Add(nodeTable);
                        }

                        this.tvTables.Items.Add(nodeRoot);
                        this.tvTables.DisplayMemberPath = "ShowName";

                        return null;
                    }));
                }
                catch { }
            });
            threadLoadTables.IsBackground = true;
            threadLoadTables.Start();
        }

        /// <summary>
        /// 展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeTable_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem nodeItem = sender as TreeViewItem;
            if (nodeItem == null || nodeItem.Tag == null) return;

            //项
            TableItem item = nodeItem.Tag as TableItem;
            if (item == null) return;

            //已经加载过了
            if (item.IsLoadCells) return;

            //标记已经加载
            item.IsLoadCells = true;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        
                        TableName = AppGlobal.SysTableName_TableCells,
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="ParentId", CellValue=item.Id }
                        },
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy() { CellName="CellName" }
                        }
                    };

                    //查找表的列
                    DataTable dt = SQLiteDao.GetTable(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //清空子项
                        nodeItem.Items.Clear();
                        //是否有列
                        if (dt == null || dt.Rows.Count <= 0) return null;

                        //循环所有列
                        foreach (DataRow row in dt.Rows)
                        {
                            TableItem tItem = new TableItem()
                            {
                                IsFilter = false,
                                IsLoadCells = true,
                                IsTable = false,
                                CellName = row["CellName"].ToString(),
                                CnName = row["CNName"].ToString(),
                                ShowName = row["CNName"].ToString() + "[" + row["CellName"].ToString() + "]"
                            };

                            TreeViewItem nodeCell = new TreeViewItem();
                            nodeCell.Header = tItem.ShowName;
                            nodeCell.IsExpanded = false;
                            nodeCell.Tag = tItem;

                            nodeItem.Items.Add(nodeCell);
                        }

                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    item.IsLoadCells = false;
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitSize()
        {
            this.Height = AppData.MainWindow.WinHeight - 100;
            this.txtSql.Height = this.Height - 120;
        }
        
        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExec_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSQL();
        }
        /// <summary>
        /// 执行SQL
        /// </summary>
        private void ExecuteSQL()
        {
            string sql = this.txtSql.Text.Trim();

            if (string.IsNullOrWhiteSpace(sql))
            {
                AppAlert.FormTips(gridMain, "请输入要执行的SQL！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            bool flag = SQLiteDao.ExecuteSQL(sql);

            if (flag)
            {
                AppAlert.FormTips(gridMain, "执行SQL成功！", AppCode.Enums.FormTipsType.Right);
            }
            else
            {
                AppAlert.FormTips(gridMain, "执行SQL失败！", AppCode.Enums.FormTipsType.Error);
            }
        }

        /// <summary>
        /// 清空SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtSql.Text = "";
        }
    }
    /// <summary>
    /// 数据表项
    /// </summary>
    [Serializable]
    public class TableItem
    {
        /// <summary>
        /// 是否加载子级
        /// </summary>
        public bool IsLoadCells { get; set; }
        /// <summary>
        /// 是否过滤的节点
        /// </summary>
        public bool IsFilter { get; set; }
        /// <summary>
        /// 是否表
        /// </summary>
        public bool IsTable { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 中文名称
        /// </summary>
        public string CnName { get; set; }
        /// <summary>
        /// 数据表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string CellName { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string ShowName { get; set; }
        /// <summary>
        /// 所有列
        /// </summary>
        public List<TableItem> Cells { get; set; }
    }
}
