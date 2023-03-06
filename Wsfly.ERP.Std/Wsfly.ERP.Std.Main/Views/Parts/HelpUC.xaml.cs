

using Wsfly.ERP.Std.AppCode.Base;
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
using Wsfly.ERP.Std.Service.Models;
using Wsfly.ERP.Std.Service.Exts;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// HelpUC.xaml 的交互逻辑
    /// </summary>
    public partial class HelpUC : BaseUserControl
    {
        public string _Keywords = string.Empty;

        /// <summary>
        /// 构造
        /// </summary>
        public HelpUC()
        {
            InitializeComponent();

            this.Loaded += HelpUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSearch.Click += BtnSearch_Click;

            LoadData();

            LoadRightData();

            InitSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        /// <summary>
        /// 大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始大小
        /// </summary>
        private void InitSize()
        {
            this.Height = AppData.MainWindow.WinHeight - 82;

            this.scrollList.Height = AppData.MainWindow.WinHeight - 232;

            panelHelpList.Width = AppData.MainWindow.WinWidth - 500;
        }

        /// <summary>
        /// 加载右侧数据
        /// </summary>
        private void LoadRightData()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                SQLParam param = new SQLParam()
                {
                    
                    TableName = "Sys_Helps",
                    Top = 10,
                    Wheres = new List<Where>()
                    {
                        new Where() { CellName = "CategoryId", CellValue = 1 }
                    },
                    OrderBys = new List<OrderBy>()
                    {
                        new OrderBy() { CellName="Views", Type= OrderType.倒序 }
                    }
                };

                try
                {
                    //查询常见问题
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            DataTableToRightList(dt, panelQuestions);
                            return null;
                        }));
                    }
                }
                catch { }

                try
                {
                    param.Wheres = new List<Where>() {
                        new Where() { CellName = "CategoryId", CellValue = 2 }
                    };


                    //查询新手帮助
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            DataTableToRightList(dt, panelNewers);
                            return null;
                        }));
                    }
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 转到右侧列表
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="panel"></param>
        private void DataTableToRightList(DataTable dt, StackPanel panel)
        {
            if (dt == null || dt.Rows.Count <= 0) return;

            int index = 0;

            SolidColorBrush bFont = AppGlobal.ColorToBrush("#444444");

            foreach (DataRow row in dt.Rows)
            {
                long id = DataType.Long(row["Id"], 0);
                if (id <= 0) continue;

                string title = row["Title"].ToString();
                if (string.IsNullOrWhiteSpace(title)) continue;

                index++;

                TextBlock lbl = new TextBlock();
                lbl.Tag = row;
                lbl.Text = index + "." + Wsfly.ERP.Std.Core.Handler.StringHandler.SubStringsByBytes(title, 20);
                lbl.ToolTip = title;
                lbl.Cursor = Cursors.Hand;
                lbl.Foreground = bFont;
                lbl.Margin = new Thickness(0, 0, 0, 8);
                lbl.MouseLeftButtonDown += Lbl_MouseLeftButtonDown;

                panel.Children.Add(lbl);
            }
        }

        /// <summary>
        /// 点击帮助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock lbl = sender as TextBlock;

            DataRow row = lbl.Tag as DataRow;
            if (row == null) return;

            string title = row["Title"].ToString();

            //标题
            if (string.IsNullOrWhiteSpace(title)) title = "查看帮助";

            //帮助查看
            Views.Parts.HelpDetailUC uc = new Parts.HelpDetailUC(row);

            //窗口
            Components.PageWindow win = new Components.PageWindow(uc, title);
            uc._ParentWindow = win;
            win.Show();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            ShowLoading(gridMain);

            this.panelHelpList.Children.Clear();
            this.panelPages.Children.Clear();

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_Helps",
                        PageIndex = _PageIndex,
                        PageSize = 10,
                        Wheres = new List<Where>(),
                        OrderBys = new List<OrderBy>()
                        {
                             new OrderBy() { CellName="Views", Type= OrderType.倒序 }
                        }
                    };

                    if (!string.IsNullOrWhiteSpace(_Keywords))
                    {
                        param.Wheres.Add(new Where() { CellName = "Title", CellValue = _Keywords, Type = WhereType.模糊查询 });
                    }

                    //得到分页数据
                    PageResult result = QueryService.GetPaging(param);
                    DataTable dt = result.Data;
                    if (dt == null || dt.Rows.Count <= 0)
                    {
                        HideLoading();
                        return;
                    }

                    //显示数据
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string categoryName = row["CategoryName"].ToString();
                            string title = row["Title"].ToString();
                            string content = row["Content"].ToString();
                            int views = DataType.Int(row["Views"].ToString(), 0);
                            string datetime = DataType.DateTime(row["Content"].ToString(), DateTime.Now).ToString("yyyy-MM-dd");

                            content = Core.Handler.StringHandler.RemoveHTML(content);

                            StackPanel panel = new StackPanel();

                            TextBlock lblTitle = new TextBlock();
                            TextBlock lblContent = new TextBlock();
                            TextBlock lblCreateDate = new TextBlock();

                            lblTitle.HorizontalAlignment = HorizontalAlignment.Left;
                            lblTitle.Tag = row;
                            lblTitle.Text = Wsfly.ERP.Std.Core.Handler.StringHandler.SubStringsByBytes(title, 40);
                            lblTitle.FontSize = 16;
                            lblTitle.Cursor = Cursors.Hand;
                            lblTitle.Style = this.FindResource("Link") as Style;
                            lblTitle.MouseLeftButtonDown += Lbl_MouseLeftButtonDown;
                            lblTitle.Margin = new Thickness(0, 0, 0, 2);

                            lblContent.Text = Wsfly.ERP.Std.Core.Handler.StringHandler.SubStringsByBytes(content, 250);
                            lblContent.FontSize = 14;
                            lblContent.TextWrapping = TextWrapping.Wrap;

                            lblCreateDate.Text = "[" + categoryName + "]  访问：" + views + "　日期：" + datetime;
                            lblCreateDate.Foreground = Brushes.Gray;
                            lblCreateDate.Margin = new Thickness(0, 5, 0, 0);

                            panel.Margin = new Thickness(0, 0, 0, 20);

                            panel.Children.Add(lblTitle);
                            panel.Children.Add(lblContent);
                            panel.Children.Add(lblCreateDate);

                            this.panelHelpList.Children.Add(panel);
                        }

                        //隐藏加载中
                        HideLoading();

                        //显示分页
                        ShowPages(result, this.panelPages, new ClickPage_Deletage(delegate (int pageIndex, int pageCount)
                        {
                            //点击分页
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                try
                                {
                                    if (pageIndex > pageCount)
                                    {
                                        AppAlert.FormTips(gridMain, "跳转失败，超过最大页数！", AppCode.Enums.FormTipsType.Info);
                                        return null;
                                    }

                                    //当前分页
                                    _PageIndex = pageIndex;

                                    //重新加载数据
                                    LoadData();
                                }
                                catch { }

                                return null;
                            }));

                        }));

                        return null;
                    }));
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        HideLoading();
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            //关键字
            _Keywords = this.txtKeywords.Text.Trim();
            //重新加载数据
            LoadData();
        }
    }
}
