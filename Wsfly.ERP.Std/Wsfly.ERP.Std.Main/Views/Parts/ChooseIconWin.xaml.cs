using Wsfly.CallService.Models;
using Wsfly.Client.PC.AppCode.Base;
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

namespace Wsfly.Client.PC.Views.Parts
{
    /// <summary>
    /// ChooseIconWin.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseIconWin : BaseUserControl
    {
        DataRow _CurrentChooseIcon = null;
        string _showIconType = "所有图标";

        /// <summary>
        /// 构造
        /// </summary>
        public ChooseIconWin(string type = "所有图标")
        {
            _showIconType = type;

            InitializeComponent();

            this.Loaded += ChooseIconWin_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseIconWin_Loaded(object sender, RoutedEventArgs e)
        {
            //加载数据
            LoadData();

            //点击事件
            this.btnChoosePic.Click += BtnChoosePic_Click;
            this.btnChoose.Click += BtnChoose_Click;

            this.btnPrevPage.Click += BtnPrevPage_Click;
            this.btnNextPage.Click += BtnNextPage_Click;

            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _PageIndex++;
            LoadData();
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            _PageIndex--;
            LoadData();
        }

        /// <summary>
        /// 选择图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChoosePic_Click(object sender, RoutedEventArgs e)
        {
            List<string> files = Client.Core.Handler.UploadFileHandler.ChooseFilesDialog(true, ".png|*.png");
            if (files == null || files.Count <= 0) return;

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //循环上传所有图片
                foreach (string path in files)
                {
                    //图标类型
                    string type = "其它";

                    try
                    {
                        //读取本地图片
                        System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                        if (img == null) continue;

                        //图片大小决定类型
                        if (img.Width == 45 && img.Height == 45)
                        {
                            type = "主菜单";
                        }
                        else if (img.Width == 32 && img.Height == 32)
                        {
                            type = "子菜单";
                        }

                        img.Dispose();
                    }
                    catch { continue; }

                    try
                    {
                        //上传到服务器
                        string serverPath = CallService.WService.UploadFile(-1, path, AppGlobal.UserInfo.UserId, AppGlobal.UserInfo.Token);

                        if (!string.IsNullOrWhiteSpace(serverPath))
                        {
                            //上传成功 保存图标
                            InsertParam param = new InsertParam()
                            {
                                DBName = AppGlobal.DBName,
                                tbName = "Sys_Icons",
                                KeyValues = new List<KeyValueItem>()
                                {
                                    new KeyValueItem("Icon", serverPath),
                                    new KeyValueItem("Type", type)
                                }
                            };

                            try
                            {
                                //插入图标
                                long id = CallService.WService.Insert(param, AppGlobal.UserInfo.Token);
                                if (id > 0)
                                {
                                    //得到插入的行
                                    DataRow row = CallService.WService.GetRow(id, "Sys_Icons", AppGlobal.UserInfo.Token);
                                    if (row == null || row.Table.Columns == null || row.Table.Columns.Count <= 0) continue;

                                    //添加到图标表
                                    AddIconItem(row);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                //隐藏等待
                HideLoading();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }
        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitSize()
        {
            this.scrollMain.Width = _ParentWindow.Width - 40;
            this.scrollMain.Height = _ParentWindow.Height - 70 - 50;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            //显示等待
            ShowLoading(gridMain);

            //线程加载数据
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                int loadCount = 0;

                while (true)
                {
                    try
                    {
                        QueryParam param = new QueryParam()
                        {
                            DBName = AppGlobal.DBName,
                            tbName = "Sys_Icons",
                            PageSize = 200,
                            PageIndex = _PageIndex,
                            OrderBys = new List<OrderBy>()
                            {
                                new OrderBy() { CellName="Type", Type=OrderType.顺序 },
                                new OrderBy() { CellName="Id", Type=OrderType.倒序 }
                            }
                        };

                        if (!_showIconType.Equals("所有图标"))
                        {
                            param.Wheres = new List<Where>() { new Where() { CellName = "Type", CellValue = _showIconType } };
                        }

                        CallService.Results.PageResult result = Wsfly.CallService.WService.GetPaging(param, AppGlobal.UserInfo.Token);

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            DataTable dt = result.Data;

                            this.panelItems.Children.Clear();

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                try
                                {
                                    //绑定到列表
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        AddIconItem(row);
                                    }
                                }
                                catch { }
                            }

                            this.btnPrevPage.IsEnabled = false;
                            this.btnNextPage.IsEnabled = false;

                            //是否显示分页
                            if (result.PageCount <= 1)
                            {
                                this.lblPageCount.Visibility = Visibility.Collapsed;
                                this.btnPrevPage.Visibility = Visibility.Collapsed;
                                this.btnNextPage.Visibility = Visibility.Collapsed;
                            }

                            this.lblPageCount.Text = "第 " + _PageIndex + "/" + result.PageCount + " 页";

                            if (result.PageIndex > 1)
                            {
                                //有上一页
                                this.btnPrevPage.IsEnabled = true;
                            }
                            if (result.PageIndex < result.PageCount)
                            {
                                //有下一页
                                this.btnNextPage.IsEnabled = true;
                            }

                            //隐藏等待
                            HideLoading();

                            return null;
                        }));

                        break;
                    }
                    catch (Exception ex)
                    {
                        loadCount++;
                        if (loadCount > 10)
                        {
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                AppAlert.FormTips(gridMain, "加载图标失败！", AppCode.Enums.FormTipsType.Error);
                                return null;
                            }));
                            break;
                        }
                    }
                }
            });
            thread.Start();
        }

        /// <summary>
        /// 添加一项图标
        /// </summary>
        /// <param name="row"></param>
        private void AddIconItem(DataRow row)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                ChooseIconItemUC item = new ChooseIconItemUC(row);
                item.Margin = new Thickness(10);
                item.Cursor = Cursors.Hand;
                item.MouseEnter += Item_MouseEnter;
                item.MouseLeave += Item_MouseLeave;
                item.MouseLeftButtonDown += Item_MouseLeftButtonDown;
                item.MouseDoubleClick += Item_MouseDoubleClick;
                this.panelItems.Children.Add(item);

                return null;
            }));
        }

        /// <summary>
        /// 双击选中图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //选择的图标
            ChooseIconItemUC item = (sender as ChooseIconItemUC);
            //当前选中
            _CurrentChooseIcon = item._IconRow;

            //选中图标回传
            ChooseIconCallBack();
        }

        /// <summary>
        /// 选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChoose_Click(object sender, RoutedEventArgs e)
        {
            //选中图标回传
            ChooseIconCallBack();
        }

        /// <summary>
        /// 选中图标回传
        /// </summary>
        private void ChooseIconCallBack()
        {
            //还没有选中图标
            if (_CurrentChooseIcon == null) return;

            //图标路径
            string icon = _CurrentChooseIcon["Icon"].ToString();
            //回调
            (_ParentWindow as Components.PopWindow).CallBack(icon);
        }

        /// <summary>
        /// 点击图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //选择的图标
            ChooseIconItemUC item = (sender as ChooseIconItemUC);
            //当前选中
            _CurrentChooseIcon = item._IconRow;

            //所有图标
            foreach (ChooseIconItemUC uc in panelItems.Children)
            {
                //取消选中
                uc.UnChoose();
            }

            //标记选中
            item.SetChoosed();
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            ChooseIconItemUC item = (sender as ChooseIconItemUC);
            if (item._IsChoose) return;
            item.border.BorderBrush = Brushes.LightGray;
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            ChooseIconItemUC item = (sender as ChooseIconItemUC);
            if (item._IsChoose) return;
            item.border.BorderBrush = Brushes.OrangeRed;
        }
    }
}
