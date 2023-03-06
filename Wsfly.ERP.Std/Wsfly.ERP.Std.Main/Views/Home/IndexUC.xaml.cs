using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.Views.Parts;
using Wsfly.ERP.Std.Views.Controls;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.AppCode.Handler;
using Wsfly.ERP.Std.Views.MZ;
using Wsfly.ERP.Std.Views.Components;

namespace Wsfly.ERP.Std.Views.Home
{
    /// <summary>
    /// IndexUC.xaml 的交互逻辑
    /// </summary>
    public partial class IndexUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public IndexUC()
        {
            InitializeComponent();

            this.Loaded += IndexUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndexUC_Loaded(object sender, RoutedEventArgs e)
        {
            //加载任务导航
            LoadGuideNav();

            //加载常用功能
            LoadQuickActions();

            //加载首页报表中心
            LoadReports();

            //加载日程安排
            LoadSchedule();

            //加载帮助
            LoadHelps();

            //窗口大小
            InitWinSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;

            //绑定鼠标控制左区域滚动
            BindMouseControlScrollEvent();
            this.scrollLeft.ScrollChanged += ScrollLeft_ScrollChanged;


            string appVersion = AppGlobal.GetSysConfigReturnString("System_AppVersion");

            this.lblAppVersion.Text = appVersion + "-" + AppGlobal.LocalConfig.Version;
            this.lblToWebsite.MouseLeftButtonDown += LblToWebsite_MouseLeftButtonDown;

            //检查是否有新版本
            string newVersion = "";
            bool hasNewVersion = UpgradeHandler.CheckUpgradeHasNewVersion(ref newVersion);
            if (hasNewVersion)
            {
                this.lblLastVersion.Text = "点击升级到：" + appVersion + "-" + newVersion;
                this.lblLastVersion.MouseLeftButtonDown += LblLastVersion_MouseLeftButtonDown;
                this.lblLastVersion.Foreground = Brushes.Blue;
                this.lblLastVersion.Cursor = Cursors.Hand;
            }
            else
            {
                this.lblLastVersion.Text = "当前已是最新版本";
                this.lblLastVersion.Foreground = Brushes.Gray;
            }

            this.btnGuideNav.Click += BtnGuideNav_Click;
        }


        #region 鼠标移动滚动内容
        /// <summary>
        /// 是否鼠标点下
        /// </summary>
        bool _isMouseDown = false;
        /// <summary>
        /// 鼠标位置
        /// </summary>
        double _mouseY = 0;
        /// <summary>
        /// 距离顶部位置
        /// </summary>
        double _marginTop = 0;

        /// <summary>
        /// 绑定鼠标控件滚动事件
        /// </summary>
        private void BindMouseControlScrollEvent()
        {
            //滚动条移动
            this.borderScrollInner.MouseLeftButtonDown += BorderScrollInner_MouseLeftButtonDown;
            this.MouseMove += UC_MouseMove;
            this.MouseUp += UC_MouseUp;
            this.MouseLeave += UC_MouseLeave;
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            _isMouseDown = false;
        }

        /// <summary>
        /// 鼠标放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isMouseDown) return;
            _isMouseDown = false;
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            double outerHeight = this.borderScrollOuter.ActualHeight - 2;
            double rY = e.GetPosition(this).Y;
            double moveY = rY - _mouseY;

            double top = (_marginTop + moveY);
            if (top <= 0) top = 0;
            else if (top > (outerHeight - 220)) top = outerHeight - 220D;

            this.borderScrollInner.Margin = new Thickness(0, top, 0, 0);

            //相对比例
            double bl = top / (outerHeight - 220D);
            double sh = this.scrollLeft.ScrollableHeight;
            this.scrollLeft.ScrollToVerticalOffset(sh * bl);
        }

        /// <summary>
        /// 鼠标点下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderScrollInner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _mouseY = e.GetPosition(this).Y;
            _marginTop = this.borderScrollInner.Margin.Top;
        }

        /// <summary>
        /// 滚动区域变化发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollLeft_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //鼠标控制
            if (_isMouseDown) return;

            double scrollableHeight = this.scrollLeft.ScrollableHeight;
            if (scrollableHeight <= 0)
            {
                this.borderScrollInner.Visibility = Visibility.Collapsed;
                return;
            }

            this.borderScrollInner.Visibility = Visibility.Visible;

            double verticalOffset = this.scrollLeft.VerticalOffset;
            double outerHeight = this.borderScrollOuter.ActualHeight;
            double marginTop = (outerHeight - 220) * (verticalOffset / scrollableHeight);

            this.borderScrollInner.Margin = new Thickness(0, marginTop, 0, 0);
        }
        #endregion

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitWinSize();
        }
        /// <summary>
        /// 初始窗口大小
        /// </summary>
        private void InitWinSize()
        {
            if (AppData.MainWindow.WinWidth < 102) return;

            this.Width = AppData.MainWindow.WinWidth - 102;
            this.Height = AppData.MainWindow.WinHeight - 102;

            double leftWidth = this.Width - 30 - 250;
            double leftHeight = this.Height - 20;

            if (leftWidth <= 0) return;

            this.scrollLeft.Width = leftWidth;
            this.scrollLeft.Height = leftHeight;
            this.scrollLeft.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }


        #region 引导导航
        /// <summary>
        /// 点击导航按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGuideNav_Click(object sender, RoutedEventArgs e)
        {
            string guide = (sender as Button).Tag.ToString();
            switch (guide)
            {
                case "System_Guide_Registed":
                    {
                        AppData.MainWindow.ShowRegister();
                    }
                    break;
                case "System_Guide_SetPrintData":
                    {
                        AppData.MainWindow.AddPage(99, new ListUC(99, 60, new List<long>() { 60 }, "打印数据"), "打印数据");
                    }
                    break;
                case "System_Guide_FirstUpdatePassword":
                    {
                        SysSettingsUC uc = new SysSettingsUC();
                        AppData.MainWindow.AddPage(AppModules.系统设置, uc, "系统设置");
                    }
                    break;
            }
        }
        /// <summary>
        /// 加载引导导航
        /// </summary>
        public void LoadGuideNav()
        {
            this.panelGuideNav.Visibility = Visibility.Collapsed;
            this.lblGuideNav.Text = "";
            this.btnGuideNav.Tag = "";

            bool System_Guide_SetPrintData = AppGlobal.GetSysConfigReturnBool("System_Guide_SetPrintData");
            if (!System_Guide_SetPrintData)
            {
                this.panelGuideNav.Visibility = Visibility.Visible;
                this.lblGuideNav.Text = "您还未设置打印参数，请点击按钮跳转进入设置！";
                this.btnGuideNav.Tag = "System_Guide_SetPrintData";
                return;
            }

            bool System_Guide_FirstUpdatePassword = AppGlobal.GetSysConfigReturnBool("System_Guide_FirstUpdatePassword");
            if (!System_Guide_FirstUpdatePassword)
            {
                this.panelGuideNav.Visibility = Visibility.Visible;
                this.lblGuideNav.Text = "请修改默认密码！";
                this.btnGuideNav.Tag = "System_Guide_FirstUpdatePassword";
                return;
            }

            bool System_Guide_Registed = AppGlobal.GetSysConfigReturnBool("System_Guide_Registed");
            if (!System_Guide_Registed)
            {
                this.panelGuideNav.Visibility = Visibility.Visible;
                this.lblGuideNav.Text = "此管理系统《标准版》终身免费使用，请点击右侧按钮填写授权认证资料！";
                this.btnGuideNav.Tag = "System_Guide_Registed";
                return;
            }
        }
        #endregion

        #region 常用功能
        /// <summary>
        /// 加载常用功能
        /// </summary>
        private void LoadQuickActions()
        {
            try
            {
                //清空
                this.panelActions.Children.Clear();
                //this.panelPopularFunctions.Visibility = Visibility.Collapsed;
            }
            catch { }

            bool Home_PopularFunctions = AppGlobal.GetSysConfigReturnBool("Home_PopularFunctions", true);
            if (!Home_PopularFunctions) return;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //未加载菜单成功时
                while (AppGlobal.SubMenus == null)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                //没有用户 或 已退出
                if (AppGlobal.UserInfo == null) return;

                try
                {
                    SQLParam param = new SQLParam()
                    {
                        Top = 10,
                        TableName = "Sys_UserQuickActions",
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="UserId", CellValue=AppGlobal.UserInfo.UserId }
                        },
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy("Count", OrderType.倒序)
                        }
                    };

                    //数据表结果
                    DataTable dt = null;

                    dt = SQLiteDao.GetTable(param);

                    //是否有数据
                    if (dt == null || dt.Rows.Count <= 0)
                    {
                        return;
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //样式
                        Style btnStyle = this.FindResource("btnSubMenu") as Style;

                        foreach (DataRow row in dt.Rows)
                        {
                            long moduleId = DataType.Long(row["ModuleDetailId"], 0);
                            if (moduleId <= 0) continue;

                            //是否有权限
                            if (!AppGlobal.HasAuthority(moduleId)) continue;

                            //查找菜单
                            DataRow[] rowSubMenus = AppGlobal.SubMenus.Select("[Id]=" + moduleId + " and [IsPC]=1");
                            if (rowSubMenus == null || rowSubMenus.Length <= 0) continue;

                            SubNavItemUC btn = new SubNavItemUC(rowSubMenus[0]);
                            btn.Cursor = Cursors.Hand;
                            btn.Width = 80;
                            btn.Height = 100;
                            btn.HasAuthority = true;
                            btn.PreviewMouseDown += Btn_PreviewMouseDown;

                            //添加到UI界面
                            this.panelActions.Children.Add(btn);
                        }

                        this.panelPopularFunctions.Visibility = Visibility.Visible;

                        return null;
                    }));
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 点击常用功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var menu = (sender as SubNavItemUC).MenuInfo;
            AppData.MainWindow.ClickMenu(menu);
        }
        #endregion

        #region 汇总报表
        /// <summary>
        /// 加载首页汇总报表
        /// </summary>
        private void LoadReports()
        {
            //汇总报表
            this.panelHomeReports.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 日程安排
        /// <summary>
        /// 加载日程安排
        /// </summary>
        private void LoadSchedule()
        {
            ScheduleUC scheduleUC = new ScheduleUC();
            scheduleUC.FocusVisualStyle = null;
            this.borderSchedule.Child = scheduleUC;
        }
        #endregion

        #region 右侧内容
        /// <summary>
        /// 启动升级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblLastVersion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //启动升级，不提示
                string path = AppDomain.CurrentDomain.BaseDirectory + "Upgrade.exe";
                System.Diagnostics.Process.Start(path);
                //当前程序退出
                App.Current.Shutdown(-1);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 访问官网
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblToWebsite_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://erp.wsfly.com/");
        }
        /// <summary>
        /// 查看更多帮助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppData.MainWindow.ShowHelp();
        }

        /// <summary>
        /// 加载帮助
        /// </summary>
        private void LoadHelps()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    string v = AppGlobal.GetSysConfigReturnString("System_AppVersion");
                    string result = Wsfly.ERP.Std.Core.Handler.WebHandler.GetHtml("http://erp.wsfly.com/Moz/GetHelpList?a=std&v=" + v, Encoding.UTF8);

                    if (string.IsNullOrWhiteSpace(result)) return;
                    Dictionary<string, dynamic> dicResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                    if (dicResult == null || dicResult.Count <= 0) return;
                    if (dicResult["Success"] == false) return;

                    var data = dicResult["Data"];
                    if (data.Count <= 0) return;

                    Style style = this.FindResource("Link2") as Style;
                    int index = 1;

                    foreach (var item in data)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            WrapPanel panel = new WrapPanel();
                            TextBlock lbl = new TextBlock();
                            lbl.Tag = item["Id"].Value;
                            lbl.Style = style;
                            lbl.Text = index + "." + item["BT"].Value;
                            lbl.MouseLeftButtonDown += LblHelp_MouseLeftButtonDown;

                            panel.Margin = new Thickness(0, 0, 0, 5);
                            panel.Children.Add(lbl);
                            this.panelHelps.Children.Add(panel);
                        }));

                        //下一序列
                        index++;
                    }
                }
                catch (Exception ex) { }
            });
            thread.Start();
        }
        /// <summary>
        /// 加载帮助
        /// </summary>
        private void LoadHelps_delete()
        {
            Dictionary<long, string> dicHelps = new Dictionary<long, string>()
            {
                { 1,"单表操作之新建商品信息" },
                { 2,"双表操作之开入库单" },
                { 3,"用户、角色、权限管理" },
                { 4,"产品库存、银行账户盘点" },
                { 5,"打印数据、格式设置" },
            };

            Style style = this.FindResource("Link2") as Style;
            int index = 1;

            foreach (var kv in dicHelps)
            {
                WrapPanel panel = new WrapPanel();
                TextBlock lbl = new TextBlock();
                lbl.Tag = kv.Key;
                lbl.Style = style;
                lbl.Text = index + "." + kv.Value;
                lbl.MouseLeftButtonDown += LblHelp_MouseLeftButtonDown;

                panel.Margin = new Thickness(0, 0, 0, 5);
                panel.Children.Add(lbl);
                this.panelHelps.Children.Add(panel);

                //下一序列
                index++;
            }
        }
        /// <summary>
        /// 查看帮助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var id = (long)(sender as TextBlock).Tag;
            System.Diagnostics.Process.Start("http://erp.wsfly.com/moz/help?a=std&v=" + AppGlobal.GetSysConfigReturnString("System_AppVersion") + "&id=" + id);
        }
        #endregion
    }
}
