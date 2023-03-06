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
using System.Data;
using Microsoft.Win32;

using MahApps.Metro.IconPacks;

using Wsfly.ERP.Std.Views.Home;
using Wsfly.ERP.Std.AppCode.Handler;
using Wsfly.ERP.Std.Views.Parts;
using Wsfly.ERP.Std.Views.MZ;
using Wsfly.ERP.Std.AppCode.Extensions;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Core.Extensions;


namespace Wsfly.ERP.Std
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : AppCode.Base.BaseWindow
    {
        #region 变量、属性
        /// <summary>
        /// 当前活动页面
        /// </summary>
        public Core.Base.BaseUserControl _CurrentActivePage = null;
        /// <summary>
        /// 检查版本更新
        /// </summary>
        AppCode.Handler.UpgradeHandler _upgradeHandler = null;
        /// <summary>
        /// 是否已经检查是否有新版本
        /// </summary>
        bool _isCheckedNewVersion = false;
        /// <summary>
        /// 是否系统窗体大小改变
        /// </summary>
        bool _isWinStateChange = false;
        /// <summary>
        /// 是否显示登陆
        /// </summary>
        bool _isShowLogin = true;
        /// <summary>
        /// 系统DPI
        /// </summary>
        public float _SystemDPI = 100;

        /// <summary>
        /// 显示刷新表配置
        /// </summary>
        public bool ShowRenewTC
        {
            set
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    this.imgRenewTC.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }));
            }
        }
        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WinWidth
        {
            get
            {
                double width = this.ActualWidth;
                if (double.IsNaN(width)) width = this.Width;
                return width;
            }
        }
        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WinHeight
        {
            get
            {
                double height = this.ActualHeight;
                if (double.IsNaN(height)) height = this.Height;
                return height;
            }
        }

        /// <summary>
        /// 首页
        /// </summary>
        IndexUC _indexUC = null;
        #endregion

        #region 初始与加载
        /// <summary>
        /// 构造
        /// </summary>
        public MainWindow()
        {
            try
            {
                //本地配置
                AppConfig localConfig = AppConfig.Load();
                AppGlobal.LocalConfig = localConfig;
                AppGlobal.LocalConfig.Save();
            }
            catch (Exception ex)
            {
                //新实例化本地配置
            }

            this.WindowState = WindowState.Maximized;

            //避免覆盖任务栏
            Core.Handler.WindowHandler.RepairWindowBehavior(this);

            //预加载窗体
            LoadingWindow loadingWin = new LoadingWindow();
            loadingWin.Show();

            //隐藏主程序
            this.Hide();

            //加载信息窗口
            System.Threading.Thread threadLoadConfigs = new System.Threading.Thread(delegate ()
            {
                //预加载
                PreLoading(loadingWin);
            });
            threadLoadConfigs.IsBackground = true;
            threadLoadConfigs.Start();
        }

        /// <summary>
        /// 初始界面
        /// </summary>
        private void InitUI()
        {
            //初始界面
            InitializeComponent();
            //初始窗口大小80%
            FirstInitWindowSize();

            //是否全屏
            if (AppGlobal.LocalConfig.IsFullScreen)
            {
                this.WindowState = WindowState.Maximized;
                _isMaxWin = true;
            }

            //初始消息管道
            InitPIPE();

            try
            {
                this.borderMain.Background = Core.Handler.ImageBrushHandler.GetImageBrush(Properties.Resources.bg);
            }
            catch { }

            string appName = AppGlobal.GetSysConfigReturnString("System_AppName");
            this.Title = appName;

            #region 右下角注册
            System.Threading.Thread threadRegister = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool showRegister = true;
                    try
                    {
                        var apiResult = AppCode.API.XTAPI.CheckXLH();
                        if (apiResult != null && apiResult.Success)
                        {
                            var dicRegisterInfo = AppGlobal.GetRegisterInfo();

                            showRegister = false;
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.lblRegister.Text = "授权给：" + dicRegisterInfo["Name"];
                                this.lblRegister.Foreground = Brushes.Black;
                                this.lblRegister.Cursor = Cursors.Arrow;
                            }));
                        }
                    }
                    catch (Exception ex) { }

                    if (showRegister)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.lblRegister.Text = "免费授权认证";
                            this.lblRegister.Foreground = Brushes.Blue;
                            this.lblRegister.Cursor = Cursors.Hand;
                            this.lblRegister.MouseDown += LblRegister_MouseDown;
                        }));
                    }
                }
                catch { }
            });
            threadRegister.Start();
            #endregion

            #region 右下角快捷功能

            this.imgRenewTC.MouseLeftButtonDown += imgRenewTC_MouseLeftButtonDown;
            this.imgService.MouseLeftButtonDown += ImgService_MouseLeftButtonDown;
            this.imgFiles.MouseLeftButtonDown += ImgFiles_MouseLeftButtonDown;
            this.imgSettings.MouseLeftButtonDown += ImgSettings_MouseLeftButtonDown;

            this.lblLogout.MouseLeftButtonDown += LblLogout_MouseLeftButtonDown;
            #endregion

            #region 托盘设置
            //设置托盘的各个属性
            AppGlobal._NotifyIcon = new System.Windows.Forms.NotifyIcon();
            AppGlobal._NotifyIcon.Text = appName;
            AppGlobal._NotifyIcon.Icon = Core.Handler.ImageHandler.ConvertToIcon(Properties.Resources.AppLogo);
            AppGlobal._NotifyIcon.Visible = true;
            AppGlobal._NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseDoubleClick);

            //帮助中心菜单项
            System.Windows.Forms.MenuItem tsmiUpgrade = new System.Windows.Forms.MenuItem("检查更新");
            tsmiUpgrade.Click += new EventHandler(tsmiUpgrade_Click);

            //帮助中心菜单项
            System.Windows.Forms.MenuItem tsmiHelp = new System.Windows.Forms.MenuItem("帮助中心");
            tsmiHelp.Click += new EventHandler(tsmiHelp_Click);

            //设置菜单项
            System.Windows.Forms.MenuItem tsmiSettings = new System.Windows.Forms.MenuItem("系统设置");
            tsmiSettings.Click += new EventHandler(tsmiSettings_Click);

            //退出菜单项
            System.Windows.Forms.MenuItem tsmiExit = new System.Windows.Forms.MenuItem("退出系统");
            tsmiExit.Click += new EventHandler(tsmiExit_Click);

            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { tsmiUpgrade, tsmiHelp, tsmiSettings, tsmiExit };
            AppGlobal._NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            #endregion

            //加载事件
            this.Loaded += MainWindow_Loaded;
            //关闭事件
            this.Closing += MainWindow_Closing;
            //状态事件
            this.StateChanged += MainWindow_StateChanged;
            //窗口大小改变
            this.SizeChanged += MainWindow_SizeChanged;
            this.scrollNavs.PreviewMouseLeftButtonDown += ScrollNavs_PreviewMouseLeftButtonDown;

            //绑定事件
            BingEvents();

            //主窗口
            AppData.MainWindow = this;

            //是否有优先打开窗口
            if (AppGlobal.LocalConfig != null && !string.IsNullOrWhiteSpace(AppGlobal.LocalConfig.FirstRunFullWindow))
            {
                //显示独立窗口
                ShowIndependentWin(AppGlobal.LocalConfig.FirstRunFullWindow);
            }

            //显示登陆页面
            ShowLogin();
        }

        /// <summary>
        /// 显示独立窗口
        /// </summary>
        private void ShowIndependentWin(string name)
        {
            try
            {
                //打开窗口
                if (!string.IsNullOrWhiteSpace(name))
                {
                    //.exe结尾
                    if (name.ToLower().EndsWith(".exe"))
                    {
                        System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + name);
                        return;
                    }

                    //窗口名称
                    string winName = "Wsfly.ERP.Std.Views.Independents." + name;

                    //当前程序集
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var instance = assembly.CreateInstance(winName);

                    //成功创建实例
                    if (instance != null)
                    {
                        //打开窗口窗口
                        Window win = (Window)instance;
                        win.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
                        {
                            if (e.Key == Key.Escape)
                            {
                                //按ESC关闭
                                win.Close();
                            }
                        });
                        win.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "显示独立窗口异常");
            }
        }
        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //设置窗口大小
            SetWindowSize();
        }

        /// <summary>
        /// 设置窗口大小
        /// </summary>
        private void SetWindowSize()
        {
            try
            {
                double borderWidth = 1 * 2;
                double leftWidth = this.gridColumnLeft.Width.Value;
                double topBarHeight = this.gridRowTop.Height.Value;
                double bottomBarHeight = this.gridRowTop.Height.Value;
                double topBarActionWidth = 150;
                double navMargin = 10 * 2;
                double topLeftWidth = this.panelLeft.Width;
                double navLeftWidth = 80;

                //左侧宽度
                if (leftWidth == 0) leftWidth = 80;
                //显示登陆
                if (_isShowLogin)
                {
                    leftWidth = 0;
                }

                double winWidth = this.Width;
                double winHeight = this.Height;

                if (winWidth < this.ActualWidth) winWidth = this.ActualWidth;
                if (winHeight < this.ActualHeight) winHeight = this.ActualHeight;

                this.scrollMainFrame.Width = winWidth - borderWidth - leftWidth;
                this.scrollMainFrame.Height = winHeight - borderWidth - topBarHeight - bottomBarHeight;
                this.panelNavs.Width = winWidth - borderWidth - navLeftWidth - topBarActionWidth - navMargin;
                this.scrollNavs.Width = this.panelNavs.Width;
                this.scrollLeftMenus.Height = winHeight - borderWidth - topBarHeight - 40;

                if (this.scrollNavs.ExtentHeight > 40)
                {
                    this.panelTopScroll.Visibility = Visibility.Visible;
                }
                else
                {
                    this.panelTopScroll.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "窗口大小改变异常");
            }
        }

        /// <summary>
        /// 获取DPI
        /// </summary>
        /// <returns></returns>
        public float GetLogPiex()
        {
            try
            {
                return Core.Handler.PrimaryScreen.DpiX;
            }
            catch { }

            float returnValue = 88;
            try
            {
                RegistryKey key = Registry.CurrentUser;
                RegistryKey pixeKey = key.OpenSubKey("Control Panel\\Desktop");
                if (pixeKey != null)
                {
                    var pixels = pixeKey.GetValue("LogPixels");
                    if (pixels != null)
                    {
                        returnValue = float.Parse(pixels.ToString());
                    }
                    pixeKey.Close();
                }
                else
                {
                    pixeKey = key.OpenSubKey("Control Panel\\Desktop\\WindowMetrics");
                    if (pixeKey != null)
                    {
                        var pixels = pixeKey.GetValue("AppliedDPI");
                        if (pixels != null)
                        {
                            returnValue = float.Parse(pixels.ToString());
                        }
                        pixeKey.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                returnValue = 96;
            }
            return returnValue;
        }

        /// <summary>
        /// 第一次初始窗口大小
        /// </summary>
        private void FirstInitWindowSize()
        {
            try
            {
                //设置窗口大小/窗口初始坐标
                //占用屏幕 80%
                //double screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                //double screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

                //DPI
                _SystemDPI = GetLogPiex();

                //Size primarySize = Screen.PrimaryScreen.Bounds.Size;
                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                double width = screenWidth * 0.8;
                double height = screenHeight * 0.8;

                if (width < 1000) width = 1000;
                if (height < 640) height = 640;

                this.Width = width;
                this.Height = height;

                this.Left = (screenWidth - width) / 2;
                this.Top = (screenHeight - height) / 2;

                //this.scrollMainFrame.Width = width - 82;
                //this.scrollMainFrame.Height = height - 82;

                //this.panelNavs.Width = width - 250;
                //this.scrollNavs.Width = this.panelNavs.Width;

                double borderWidth = 1 * 2;
                double leftWidth = this.gridColumnLeft.Width.Value;
                double topBarHeight = this.gridRowTop.Height.Value;
                double bottomBarHeight = this.gridRowTop.Height.Value;
                double topBarActionWidth = 150; //最小化、最大化、关闭
                double navMargin = 10 * 2;      //Margin
                double topLeftWidth = this.panelLeft.Width;

                //左侧宽度
                if (leftWidth == 0) leftWidth = 80;
                //显示登陆
                if (_isShowLogin) leftWidth = 0;

                this.scrollMainFrame.Width = this.Width - borderWidth - leftWidth;
                this.scrollMainFrame.Height = this.Height - borderWidth - topBarHeight - bottomBarHeight;
                this.panelNavs.Width = this.Width - borderWidth - leftWidth - topBarActionWidth - navMargin;
                this.scrollNavs.Width = this.panelNavs.Width;
                this.scrollLeftMenus.Height = this.Height - borderWidth - topBarHeight - 40;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "初始窗口大小异常");
            }
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            //调用退出程序
            ExitApplication();

            //回收垃圾
            AppGlobal.GCCollect();
        }

        /// <summary>
        /// 显示登陆窗口
        /// </summary>
        private void ShowLogin()
        {
            //显示登陆
            _isShowLogin = true;

            //回收垃圾
            AppGlobal.GCCollect();

            //窗口大小改变
            SetWindowSize();

            //登陆页面
            LoginUC uc = new LoginUC();
            AddPage((long)AppModules.用户登陆, uc, "用户登陆", true, false);
        }
        #endregion

        #region 界面事件
        RegisterUC _registerUC = null;
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblRegister_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowRegister();
        }
        /// <summary>
        /// 显示注册
        /// </summary>
        public void ShowRegister()
        {
            if (AppGlobal.UserInfo == null) return;

            _registerUC = new RegisterUC();
            _registerUC.Width = 500;
            _registerUC.Height = 300;

            Views.Components.PageWindow win = new Views.Components.PageWindow(_registerUC, "系统授权认证");
            _registerUC._ParentWindow = win;
            win.Width = 520;
            win.Height = 350;
            win.ShowDialog();
        }
        #endregion

        #region 退出程序
        /// <summary>
        /// 退出应用程序 判断是否真实退出
        /// </summary>
        public void ExitApplication()
        {
            try
            {
                if (!_NotifyExitApp && AppGlobal.UserConfig != null && !AppGlobal.UserConfig.IsDirctExit)
                {
                    //不直接退出
                    this.Hide();

                    //显示到托盘
                    ShowNotifyTips("最小化到托盘，双击显示！");
                }
                else
                {
                    //弹出提示
                    Views.Components.ExitTipsWindow win = new Views.Components.ExitTipsWindow();
                    win._ParentWindow = this;
                    win.ShowMaxButton = false;
                    win.ShowDialog();
                }
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 退出程序
        /// </summary>
        public void ExitApp()
        {
            try
            {
                //是否需要关闭消息管道
                if (pipeServer != null) pipeServer.Close();
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "关闭消息管道异常");
            }

            try
            {
                //托盘不显示
                AppGlobal._NotifyIcon.Visible = false;
                AppGlobal._NotifyIcon.Dispose();
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "托盘不显示异常");
            }

            try
            {
                //用户退出
                Logout();
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "用户退出登陆异常");
            }
            finally
            {
                //关闭程序
                Application.Current.Shutdown(-1);
            }

            try
            {
                //强制关闭程序
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "强制关闭程序异常");
            }
        }
        /// <summary>
        /// 用户退出
        /// </summary>
        public void Logout(bool hiddenWin = true)
        {
            try
            {
                //没有登陆
                if (AppGlobal.UserInfo == null) return;

                //身份Key
                string token = AppGlobal.UserInfo.Token;

                //清空用户
                AppGlobal.UserInfo = null;

                //是否隐藏窗体
                if (hiddenWin) this.Hide();
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "调用退出服务异常2");
            }
        }


        /// <summary>
        /// 注销并显示登陆
        /// </summary>
        public void LogoutAndLogin(bool callLogout = true)
        {
            this.lblLogout.Visibility = Visibility.Collapsed;
            this.lblUserName.Text = "您好，请登陆";

            this.gridMain.ColumnDefinitions[0].Width = new GridLength(0);
            this.panelBottomCT.Margin = new Thickness(0);

            //清除密码
            AppGlobal.LocalConfig.RememberPwd = false;
            AppGlobal.LocalConfig.UserPwd = "";
            AppGlobal.LocalConfig.Save();

            //用户退出
            if (callLogout) Logout(false);

            //不显示右下角快捷
            this.imgSettings.Visibility = Visibility.Collapsed;
            //this.imgService.Visibility = Visibility.Collapsed;
            this.imgFiles.Visibility = Visibility.Collapsed;
            this.imgRenewTC.Visibility = Visibility.Collapsed;

            //取消每分钟事件
            AppTimer.MinuteRun_Event -= AppTimer_MinuteRun_Event;

            //清除所有菜单
            this.panelMenus.Children.Clear();

            //移除所有页面
            RemoveAllPages();

            //显示登陆页面
            ShowLogin();
        }
        #endregion

        #region 登陆成功
        /// <summary>
        /// 登陆成功
        /// </summary>
        public void Logined(LoginUC uc)
        {
            //不是显示登陆
            _isShowLogin = false;

            try
            {
                //每秒事件
                AppTimer.SecondsRun_Event += AppTimer_SecondsRun_Event;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "调用保持与服务端的连接失败2");
            }

            try
            {
                //每分钟事件
                AppTimer.MinuteRun_Event += AppTimer_MinuteRun_Event;
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "调用保持与服务端的连接失败2");
            }

            //===========================================================
            //加载表配置
            System.Threading.Thread threadLoadTableConfigs = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //加载所有表配置
                    ShowStatus("正在加载表配置...", 60000);
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate () { ShowLoading(this.gridMain); }));

                    AppGlobal.LoadAllTableConfigs();
                    ShowStatus("表配置加载完成！", 3000);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.imgRenewTC.Visibility = Visibility.Collapsed;
                    }));
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "加载表配置异常");

                    ShowStatus("加载表配置异常");
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.imgRenewTC.Visibility = Visibility.Visible;
                    }));
                }
                finally
                {
                    //隐藏等待
                    HideLoading();
                }
            });
            threadLoadTableConfigs.IsBackground = true;
            threadLoadTableConfigs.Start();

            //===========================================================
            //加载所有用户
            System.Threading.Thread threadLoadUsers = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //加载所有用户
                    AppGlobal.LoadAllUsers();
                }
                catch (Exception ex)
                {
                    ShowStatus("加载系统用户异常");
                }
            });
            threadLoadUsers.IsBackground = true;
            threadLoadUsers.Start();


            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    try
                    {
                        //显示左侧栏
                        this.gridMain.ColumnDefinitions[0].Width = new GridLength(80);
                        //底部左侧右移
                        this.panelBottomCT.Margin = new Thickness(80, 0, 0, 0);

                        //移除登陆页
                        RemovePage(uc._CurrentPageIndex);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "绑定上传、下载事件；显示左栏；移除登陆页面异常");
                    }

                    try
                    {
                        //加载主页
                        _indexUC = new IndexUC();
                        AddPage((long)AppModules.导航, _indexUC, "导航", true, false);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载主页异常");
                    }

                    try
                    {
                        //加载右下角快捷方式
                        this.imgService.Visibility = Visibility.Visible;
                        this.imgFiles.Visibility = Visibility.Visible;
                        this.imgSettings.Visibility = Visibility.Visible;

                        //程序名称
                        AppGlobal._NotifyIcon.Text = AppGlobal.GetSysConfigReturnString("System_AppName");
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载右下角快捷方式异常");
                    }

                    try
                    {
                        //用户名
                        if (AppGlobal.UserInfo != null && !string.IsNullOrWhiteSpace(AppGlobal.UserInfo.UserName))
                        {
                            this.lblUserName.Text = "您好，" + AppGlobal.UserInfo.UserName;
                        }
                        else
                        {
                            this.lblUserName.Text = "您好，登陆成功！";
                        }

                        this.lblUserName.Cursor = Cursors.Hand;
                        this.lblLogout.Visibility = Visibility.Visible;
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载用户信息失败");

                        try
                        {
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(AppGlobal.UserInfo);
                            AppLog.WriteDebugLog(json);
                        }
                        catch { }
                    }

                    //加载菜单
                    LoadMenus();

                    //改变窗口大小
                    SetWindowSize();

                    //是否有优先打开窗口
                    if (AppGlobal.LocalConfig != null && !string.IsNullOrWhiteSpace(AppGlobal.LocalConfig.LoginRunFullWindow))
                    {
                        //显示独立窗口
                        ShowIndependentWin(AppGlobal.LocalConfig.LoginRunFullWindow);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "登陆成功事件！");
                }
            }));
        }


        /// <summary>
        /// 每秒事件
        /// </summary>
        /// <param name="runIndex"></param>
        private void AppTimer_SecondsRun_Event(long runIndex)
        {
            //当前时间
            string dtNowTime = DateTime.Now.ToString("HH:mm:ss");

            //工作总结提醒
            string project_WorkSummaryTime = AppGlobal.GetSysConfigReturnString("Project_WorkSummaryTime");
            if (!string.IsNullOrWhiteSpace(project_WorkSummaryTime) && dtNowTime.Equals(project_WorkSummaryTime))
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    try
                    {
                        AppAlert.Tips("又到工作总结时间，请录入您今日的工作总结！", "工作报时");
                    }
                    catch { }
                }));
            }
        }

        /// <summary>
        /// 每分钟事件
        /// </summary>
        /// <param name="runIndex"></param>
        private void AppTimer_MinuteRun_Event(long runIndex)
        {
            if (runIndex % 600 == 0)
            {
                //保持用户XT在线状态
                AppCode.API.XTAPI.KeepUserOnline();
            }
        }

        #endregion

        #region 预加载、加载配置
        /// <summary>
        /// 预加载
        /// </summary>
        /// <param name="loadingWin"></param>
        private void PreLoading(LoadingWindow loadingWin)
        {
            //PC唯一ID
            string pcid = AppGlobal._PCID;

            //版本号
            loadingWin.ShowVersion(AppGlobal.LocalConfig.Version);

            /* 2022-10-21 取消自动检查版本
             //检查版本更新
            _upgradeHandler = new AppCode.Handler.UpgradeHandler();
            _upgradeHandler.CallBack_Event += UpgradeHandler_CallBack_Event;
            _upgradeHandler.Start();

            //预加载显示消息
            loadingWin.ShowMessage("check new version");

            //只有检查是否有新版本后 才继续加载
            int stopCount = 0;
            while (!_isCheckedNewVersion)
            {
                //暂停0.5秒
                System.Threading.Thread.Sleep(500);
                stopCount++;

                //超过1秒未检查到 则跳过
                if (stopCount > 2) break;
            }
            */

            //检查是否有新的升级程序
            System.Threading.Thread threadCheckUpgradeExe = new System.Threading.Thread(delegate ()
            {
                try { UpgradeHandler.CheckUpgrade(); }
                catch { }
            });
            threadCheckUpgradeExe.Start();

            //预加载显示消息
            loadingWin.ShowMessage("set system datetime");

            try
            {
                //设置本地时间格式
                AppGlobal.SetSystemDateTimeFormat();
            }
            catch (Exception ex) { }


            try
            {
                //清理软件缓存
                AppGlobal.CleanCaches();
            }
            catch { }

            //预加载显示消息
            loadingWin.ShowMessage("loading configs");

            //加载表所有操作动作
            LoadTableOperateActions();

            //加载图片资源
            LoadImageSources();

            //加载配置
            LoadConfigs(loadingWin);
        }
        /// <summary>
        /// 加载表所有操作动作
        /// </summary>
        private void LoadTableOperateActions()
        {
            System.Threading.Thread threadLoadTableOperateAction = new System.Threading.Thread(delegate ()
            {
                int i = 0;
                while (i < 3)
                {
                    try
                    {
                        //查询所有操作
                        AppGlobal.TableOperateActions = SQLiteDao.GetTable(new Service.Dao.SQLParam()
                        {
                            TableName = AppGlobal.SysTableName_Actions
                        });
                        break;
                    }
                    catch (Exception ex)
                    {
                        i++;
                    }
                }
            });
            threadLoadTableOperateAction.IsBackground = true;
            threadLoadTableOperateAction.Start();
        }
        /// <summary>
        /// 加载图片资源
        /// </summary>
        private void LoadImageSources()
        {
            try
            {
                //加载提示图标
                AppGlobal.FormTips_Error = ImageBrushHandler.GetImageSource(Properties.Resources.Tips_Error);
                AppGlobal.FormTips_Right = ImageBrushHandler.GetImageSource(Properties.Resources.Tips_Right);
                AppGlobal.FormTips_Warning = ImageBrushHandler.GetImageSource(Properties.Resources.Tips_Warning);
                AppGlobal.FormTips_Info = ImageBrushHandler.GetImageSource(Properties.Resources.Tips_Info);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 版本检查回调
        /// </summary>
        /// <param name="hasNewVersion"></param>
        /// <param name="newVersion"></param>
        private void UpgradeHandler_CallBack_Event(bool hasNewVersion, string newVersion, bool autoUpgrade)
        {
            //没有新版本
            if (!hasNewVersion) return;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //提示
                    AppAlert.Alert("检测到新版本V" + newVersion + "，需要立即升级！", "升级提示", AppCode.Enums.AlertWindowButton.Ok, false);
                    //自动升级，不提示
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Upgrade.exe";
                    System.Diagnostics.Process.Start(path);
                    //当前程序退出
                    App.Current.Shutdown(-1);
                    return;
                }
                catch (Exception ex)
                {
                    //已经检查 并且有新版本，但是启动更新失败
                    _isCheckedNewVersion = true;
                    ErrorHandler.AddException(ex, "有新版本，但是启动更新失败");
                }
            }));
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadConfigs(LoadingWindow loadingWin)
        {
            try
            {
                //加载系统配置
                LoadSysConfigs();
            }
            catch { }

            try
            {
                //加载数据表默认列
                LoadDefaultCells();
            }
            catch { }

            try
            {
                string appName = AppGlobal.GetSysConfigReturnString("System_AppName");

                //在桌面建立快捷方式
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (string.IsNullOrWhiteSpace(desktopPath)) desktopPath = Core.Handler.ShortcutHandler.GetDeskDir();
                if (!string.IsNullOrWhiteSpace(desktopPath))
                {
                    string lnkPath = desktopPath + "\\" + appName + ".lnk";
                    if (!System.IO.File.Exists(lnkPath))
                    {
                        string targetPath = AppDomain.CurrentDomain.BaseDirectory + "MOZ-ERP.exe";
                        Core.Handler.ShortcutHandler.CreateShortcut(lnkPath, targetPath, desktopPath, appName);
                    }
                }
            }
            catch { }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //取消预加载窗口定时事件绑定
                    AppTimer.SecondsRun_Event -= loadingWin.Loading_SecondsRun_Event;

                    //关闭预加载窗口
                    loadingWin.Close();

                    //加载配置成功，初始主界面
                    InitUI();

                    //显示主窗口
                    this.Show();
                }
                catch { }
            }));
        }
        /// <summary>
        /// 加载数据表默认列
        /// </summary>
        public void LoadDefaultCells()
        {
            try
            {
                //加载默认表配置
                int loadCount = 0;
                while (true)
                {
                    try
                    {
                        //查询默认列
                        DataTable dt = SQLiteDao.GetTable(new SQLParam()
                        {
                            TableName = AppGlobal.SysTableName_TableDefaultCells,
                            OrderBys = new List<OrderBy>()
                            {
                                new OrderBy("Order", OrderType.顺序)
                            }
                        });

                        //是否加载成功
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //设置默认列表
                            AppGlobal._TableDefaultCells = dt;
                            break;
                        }
                    }
                    catch { }

                    //加载次数
                    loadCount++;
                    if (loadCount > 3) break;
                }
            }
            catch { }
        }
        #endregion

        #region 加载框架数据-菜单
        /// <summary>
        /// 加载菜单
        /// </summary>
        private void LoadMenus()
        {
            TextBlock lblLoading = new TextBlock();
            lblLoading.Text = "加载菜单";
            lblLoading.HorizontalAlignment = HorizontalAlignment.Center;
            lblLoading.Foreground = Brushes.Gray;
            lblLoading.Margin = new Thickness(0, 20, 0, 20);

            this.panelMenus.Children.Clear();
            this.panelMenus.Children.Add(lblLoading);

            //线程加载菜单
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        Top = 0,
                        Wheres = new List<Where>(),
                        OrderBys = new List<OrderBy>(),
                        TableName = AppGlobal.SysTableName_Modules
                    };

                    //查询条件
                    param.Wheres.Add(new Where()
                    {
                        CellName = "IsShow",
                        CellValue = "1",
                        Type = 0
                    });

                    //排序方式
                    param.OrderBys.Add(new OrderBy("Order", OrderType.顺序));

                    //不断加载菜单 直到加载成功
                    while (true)
                    {
                        try
                        {
                            //主模块
                            param.TableName = AppGlobal.SysTableName_Modules;
                            DataTable dtMenus = SQLiteDao.GetTable(param);
                            //子模块
                            param.TableName = AppGlobal.SysTableName_ModuleDetails;
                            DataTable dtSubMenus = SQLiteDao.GetTable(param);

                            //菜单
                            AppGlobal.Menus = dtMenus;
                            AppGlobal.SubMenus = dtSubMenus;

                            if (dtMenus != null && dtMenus.Rows.Count > 0)
                            {
                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    try
                                    {
                                        this.panelMenus.Children.Clear();

                                        for (int i = 0; i < dtMenus.Rows.Count; i++)
                                        {
                                            try
                                            {
                                                //创建左侧菜单项
                                                BuildLeftMenuItem(dtMenus.Rows[i], dtSubMenus);
                                            }
                                            catch (Exception ex) { }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorHandler.AddException(ex, "创建左右菜单项异常");
                                    }
                                }));
                            }

                            break;
                        }
                        catch (Exception ex)
                        {
                            //暂停1秒
                            System.Threading.Thread.Sleep(1000);
                            ErrorHandler.AddException(ex, "加载菜单异常！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "加载菜单2异常！");
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 创建左侧菜单项
        /// </summary>
        /// <param name="rowMenu"></param>
        /// <param name="dtSubMenus"></param>
        private void BuildLeftMenuItem(DataRow rowMenu, DataTable dtSubMenus)
        {
            //是否显示主菜单
            bool isShow = false;
            //子菜单
            DataRow[] subMenus = dtSubMenus.Select("[ParentId]=" + rowMenu["Id"] + " and [IsPC]=1");

            if (subMenus.Length > 0)
            {
                //有子级菜单
                foreach (DataRow row in subMenus)
                {
                    if (AppGlobal.HasAuthority(row.GetId()))
                    {
                        //只要有一个模块有权限则都需要显示主菜单
                        isShow = true;
                        break;
                    }
                }
            }

            //不显示菜单
            if (!isShow) return;

            Button btn = new Button();
            StackPanel panel = new StackPanel();
            TextBlock lblName = new TextBlock();

            string moduleName = rowMenu.GetString("ModuleName");

            //文字
            lblName.Text = Core.Handler.StringHandler.SubStringsByBytes(moduleName, 8, ".");
            lblName.ToolTip = moduleName;
            lblName.TextAlignment = TextAlignment.Center;

            //图标
            string icon = rowMenu.GetString("Icon");
            if (icon.StartsWith(":"))
            {
                string packType = icon.Split(':')[1];
                string iconStr = icon.Split(':')[2];
                string colorStr = icon.Split(':')[3];
                Brush brush = Brushes.Black;

                try
                {
                    brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
                }
                catch (Exception ex) { }

                switch (packType)
                {
                    case "PackIconMaterial":
                        PackIconMaterial pi = new PackIconMaterial();
                        pi.Width = 36;
                        pi.Height = 36;
                        pi.Foreground = brush;
                        pi.Margin = new Thickness(0, 0, 0, 5);
                        pi.VerticalAlignment = VerticalAlignment.Center;
                        pi.HorizontalAlignment = HorizontalAlignment.Center;
                        pi.Kind = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), iconStr);

                        panel.Children.Add(pi);
                        break;
                }
            }

            //添加到Panel
            panel.Tag = lblName.Text;
            panel.Children.Add(lblName);

            //按钮样式
            btn.Tag = rowMenu.GetId();
            btn.Style = this.FindResource("btnMenu") as Style;
            btn.Content = panel;
            btn.Click += Btn_Click;

            //添加到UI界面
            this.panelMenus.Children.Add(btn);

            //间隔线
            Border borderSpaceLine = new Border();
            borderSpaceLine.Style = this.FindResource("borderLeftSpace") as Style;
            this.panelMenus.Children.Add(borderSpaceLine);
        }
        /// <summary>
        /// 点击主菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            long moduleId = DataType.Long((sender as Button).Tag, 0);
            var name = ((sender as Button).Content as StackPanel).Tag;

            if (moduleId <= 0) return;

            //打开子页面
            SubNavsUC uc = new SubNavsUC(moduleId);
            AddPage(10000 - moduleId, uc, name.ToString());
        }

        /// <summary>
        /// 重新加载菜单
        /// </summary>
        public void ReloadMenus()
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    //加载菜单
                    LoadMenus();

                    //循环所有页面
                    foreach (UIElement uc in this.panelPages.Children)
                    {
                        if (uc is SubNavsUC)
                        {
                            //重新加载菜单
                            (uc as SubNavsUC).LoadMenus();
                        }
                    }
                }
                catch { }
            }));
        }
        #endregion

        #region 系统设置
        /// <summary>
        /// 加载系统配置
        /// </summary>
        private void LoadSysConfigs()
        {
            //加载系统配置
            AppGlobal.LoadSysConfigs();

            //设置服务端在线
            AppCode.API.XTAPI.ServerOnline();
        }
        #endregion

        #region 托盘事件
        /// <summary>
        /// 托盘直接退出
        /// </summary>
        bool _NotifyExitApp = false;
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.Focus();

            if (_isMaxWin)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }

            try
            {
                //任务栏图标闪烁
                WindowExtensions.FlashWindow(this, 4, 500);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsmiUpgrade_Click(object sender, EventArgs e)
        {
            _upgradeHandler.Start();
        }
        /// <summary>
        /// 帮助中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsmiHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }
        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsmiSettings_Click(object sender, EventArgs e)
        {
            ShowSettingPage();
        }
        /// <summary>
        /// 退出系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tsmiExit_Click(object sender, EventArgs e)
        {
            _NotifyExitApp = true;
            //退出
            ExitApp();
        }
        /// <summary>
        /// 托盘提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowNotifyTips(string message, int timeout = 3000, string title = "温馨提示", System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info)
        {
            AppGlobal._NotifyIcon.ShowBalloonTip(timeout, title, message, icon);
        }
        /// <summary>
        /// 显示帮助中心
        /// </summary>
        public void ShowHelp()
        {
            //打开帮助页面
            System.Diagnostics.Process.Start("http://erp.wsfly.com/moz/help?a=std&v=" + AppGlobal.GetSysConfigReturnString("System_AppVersion"));
        }
        /// <summary>
        /// 获取内网IP
        /// </summary>
        /// <returns></returns>
        private string GetInternalIP()
        {
            System.Net.IPHostEntry host;
            string localIP = "";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        #endregion

        #region 状态栏
        System.Timers.Timer _timerStatusClear = null;
        /// <summary>
        /// 显示状态栏文本
        /// </summary>
        /// <param name="text"></param>
        public void ShowStatus(string text, int interval = 0)
        {
            if (_timerStatusClear != null)
            {
                _timerStatusClear.Stop();
                _timerStatusClear.Close();
                _timerStatusClear = null;
            }

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblStatusText.Text = text;

                try
                {
                    if (interval > 0)
                    {
                        _timerStatusClear = new System.Timers.Timer(interval);
                        _timerStatusClear.Elapsed += _timerStatusClear_Elapsed;
                        _timerStatusClear.Start();
                    }
                }
                catch { }
            }));
        }
        /// <summary>
        /// 定时清空状态文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerStatusClear_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblStatusText.Text = "";
            }));

            try
            {
                _timerStatusClear.Stop();
                _timerStatusClear.Close();
                _timerStatusClear = null;
            }
            catch { }
        }
        #endregion

        #region 绑定事件与窗体事件
        /// <summary>
        /// 是否最大化窗口
        /// </summary>
        bool _isMaxWin = false;

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BingEvents()
        {
            this.panelTop.MouseLeftButtonDown += PanelTop_MouseLeftButtonDown;
            //this.scrollNavs.PreviewMouseLeftButtonDown += ScrollNavs_PreviewMouseLeftButtonDown;

            this.borderMin.MouseEnter += BorderMin_MouseEnter;
            this.borderMin.MouseLeave += BorderMin_MouseLeave;
            this.borderMin.MouseLeftButtonDown += BorderMin_MouseLeftButtonDown;

            this.borderMax.MouseEnter += BorderMin_MouseEnter;
            this.borderMax.MouseLeave += BorderMin_MouseLeave;
            this.borderMax.MouseLeftButtonDown += BorderMax_MouseLeftButtonDown;

            this.borderClose.MouseEnter += BorderMin_MouseEnter;
            this.borderClose.MouseLeave += BorderMin_MouseLeave;
            this.borderClose.MouseLeftButtonDown += BorderClose_MouseLeftButtonDown;

            this.navToTop.Click += ToTop_Click;
            this.navToBottom.Click += NavToBottom_Click;

            this.btnToTop.MouseEnter += BtnToTop_MouseEnter;
            this.btnToTop.MouseLeave += BtnToTop_MouseLeave;
            this.btnToTop.MouseLeftButtonDown += BtnToTop_MouseLeftButtonDown;

            this.btnToBottom.MouseEnter += BtnToTop_MouseEnter;
            this.btnToBottom.MouseLeave += BtnToTop_MouseLeave;
            this.btnToBottom.MouseLeftButtonDown += BtnToBottom_MouseLeftButtonDown;

            //注册热键
            this.KeyDown += MainWindow_KeyDown;
        }

        /// <summary>
        /// 鼠标点击向下移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToBottom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double toTop = this.scrollLeftMenus.VerticalOffset + 82;
            if (toTop < 0) return;
            this.scrollLeftMenus.ScrollToVerticalOffset(toTop);

            if (this.scrollLeftMenus.Height < this.panelMenus.ActualHeight) this.btnToTop.Opacity = 0.3;
        }

        /// <summary>
        /// 鼠标点击向上移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToTop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double toTop = this.scrollLeftMenus.VerticalOffset - 82;
            if (toTop < 0) toTop = 0;
            this.scrollLeftMenus.ScrollToVerticalOffset(toTop);

            if (this.scrollLeftMenus.Height < this.panelMenus.ActualHeight) this.btnToBottom.Opacity = 0.3;
        }

        /// <summary>
        /// 鼠标离开上下移动左右菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToTop_MouseLeave(object sender, MouseEventArgs e)
        {
            var btn = (sender as Border);
            btn.Opacity = 1;
            btn.BorderBrush = Brushes.White;
        }

        /// <summary>
        /// 鼠标进入上下移动左右菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToTop_MouseEnter(object sender, MouseEventArgs e)
        {
            var btn = (sender as Border);
            btn.Opacity = 1;
            btn.BorderBrush = Brushes.Black;
        }

        /// <summary>
        /// 导航栏向下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavToBottom_Click(object sender, RoutedEventArgs e)
        {
            double toTop = this.scrollNavs.VerticalOffset + 40;
            if (toTop < 0) return;
            this.scrollNavs.ScrollToVerticalOffset(toTop);
        }

        /// <summary>
        /// 导航栏向上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToTop_Click(object sender, RoutedEventArgs e)
        {
            double toTop = this.scrollNavs.VerticalOffset - 40;
            if (toTop < 0) return;
            this.scrollNavs.ScrollToVerticalOffset(toTop);
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                ShowHelp();
            }
            else if (e.Key == Key.F9)
            {
                FileManagerUC uc = new FileManagerUC();
                AddPage(AppModules.我的文件, uc, "我的文件");
            }
            else if (e.Key == Key.F11)
            {
                SettingUC uc = new SettingUC();
                AddPage(AppModules.个人设置, uc, "个人设置");
            }
            else if (e.Key == Key.F12)
            {
                SysSettingsUC uc = new SysSettingsUC();
                AddPage(AppModules.系统设置, uc, "系统设置");
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExitApplication();
        }

        /// <summary>
        /// 窗体状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            _isWinStateChange = false;

            if (this.Width <= 800) return;
            if (this.Height <= 600) return;

            WindowStateChange();
        }

        /// <summary>
        /// 最大化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderMax_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isWinStateChange = true;
            WindowStateChange();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderMin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 鼠标离开最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderMin_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (UIElement li in ((sender as Border).Child as Canvas).Children)
            {
                if (li is Line)
                {
                    (li as Line).Stroke = this.FindResource("lineWinActionBrush") as Brush;
                }
                else if (li is Rectangle)
                {
                    (li as Rectangle).Stroke = this.FindResource("lineWinActionBrush") as Brush;
                }
            }
        }

        /// <summary>
        /// 鼠标进入最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderMin_MouseEnter(object sender, MouseEventArgs e)
        {
            foreach (UIElement li in ((sender as Border).Child as Canvas).Children)
            {
                if (li is Line)
                {
                    (li as Line).Stroke = this.FindResource("MainBrush") as Brush;
                }
                else if (li is Rectangle)
                {
                    (li as Rectangle).Stroke = this.FindResource("MainBrush") as Brush;
                }
            }
        }
        /// <summary>
        /// 点击导航栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollNavs_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }
        /// <summary>
        /// 鼠标点下，移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PanelTop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount >= 2)
                {
                    if (_isClickTab)
                    {
                        //双击的是标签 不放大缩小
                        _isClickTab = false;
                        return;
                    }

                    //窗口大小调整
                    WindowStateChange();
                }
                else
                {
                    this.DragMove();
                }
            }
            catch { }
        }
        /// <summary>
        /// 窗口状态改变
        /// </summary>
        private void WindowStateChange()
        {
            if (_isWinStateChange)
            {
                if (this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                    _isMaxWin = true;
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    _isMaxWin = false;
                }
            }
            else
            {
                //最大化
                _isMaxWin = this.WindowState == WindowState.Maximized;

                AppGlobal.LocalConfig.IsFullScreen = _isMaxWin;
                AppGlobal.LocalConfig.Save();
            }

            if (this.scrollNavs.ExtentHeight > 40)
            {
                this.panelTopScroll.Visibility = Visibility.Visible;
            }
            else
            {
                this.panelTopScroll.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblService_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).Foreground = Brushes.Gray;
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblService_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as TextBlock).Foreground = Brushes.Black;
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblLogout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LogoutAndLogin();
        }

        /// <summary>
        /// 刷新表配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgRenewTC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.imgRenewTC.Visibility = Visibility.Collapsed;
            ShowStatus("正在刷新表配置...");

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //加载表配置
                    AppGlobal.LoadAllTableConfigs();
                    ShowStatus("刷新表配置完成", 5000);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.imgRenewTC.Visibility = Visibility.Collapsed;
                    }));
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "加载表配置异常");

                    ShowStatus("加载表配置异常");
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.imgRenewTC.Visibility = Visibility.Visible;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }


        /// <summary>
        /// 服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgService_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ServiceUC uc = new ServiceUC();
            AddPage(AppModules.售后服务, uc, "客服");
        }

        /// <summary>
        /// 文件管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgFiles_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Views.Parts.FileManagerUC uc = new Views.Parts.FileManagerUC();
            AddPage(AppModules.我的文件, uc, "文件管理");
        }

        /// <summary>
        /// 帮助中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowHelp();
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowSettingPage();
        }
        /// <summary>
        /// 显示设置页面
        /// </summary>
        private void ShowSettingPage()
        {
            SysSettingsUC uc = new SysSettingsUC();
            AddPage(AppModules.系统设置, uc, "系统设置");
        }
        #endregion

        #region Tab页面操作
        /// <summary>
        /// 总页数
        /// </summary>
        int _tabCount = 0;
        /// <summary>
        /// 当前页索引
        /// </summary>
        int _currentTabIndex = 0;
        /// <summary>
        /// 锁添加页面
        /// </summary>
        object _objLockAddPage = new object();
        /// <summary>
        /// 是否点击Tab
        /// </summary>
        bool _isClickTab = false;

        /// <summary>
        /// 添加页面
        /// </summary>
        /// <param name="module"></param>
        /// <param name="uc"></param>
        /// <param name="title"></param>
        public void AddPage(AppModules module, AppCode.Base.BaseUserControl uc, string title = null)
        {
            AddPage((long)module, uc, title);
        }
        /// <summary>
        /// 添加页面
        /// </summary>
        /// <param name="page">要添加的页面</param>
        /// <param name="title">Tab标签名称</param>
        /// <param name="addTab">是否添加顶部Tab</param>
        /// <param name="canClose">是否可以关闭</param>
        public void AddPage(long moduleId, AppCode.Base.BaseUserControl uc, string title = null, bool addTab = true, bool canClose = true)
        {
            lock (_objLockAddPage)
            {
                //标题
                uc.Title = title;

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //是否已经存在页面
                    if (this.panelNavs.Children != null && this.panelNavs.Children.Count > 0 && moduleId != 0)
                    {
                        foreach (Border b in this.panelNavs.Children)
                        {
                            AppCode.Models.TabInfo t = b.Tag as AppCode.Models.TabInfo;
                            if (t.ModuleId == moduleId && t.Title == title)
                            {
                                //已经存在页面直接显示
                                ShowPage(t.Index);
                                return;
                            }
                        }
                    }

                    //隐藏所有页面
                    HideAllPage();

                    //标签数量
                    _tabCount++;
                    _currentTabIndex = _tabCount;

                    //当前页的编号
                    uc._CurrentPageIndex = _currentTabIndex;

                    //添加页面
                    uc.Name = "MZERP_page_" + _currentTabIndex;
                    uc.IsActive = true;
                    _CurrentActivePage = uc;

                    this.panelPages.RegisterName(uc.Name, uc);
                    this.panelPages.Children.Add(uc);

                    //激活窗口
                    uc.ActivateUC();
                    uc.UCPageIndex = _currentTabIndex;

                    //是否需要Tab
                    if (addTab)
                    {
                        if (string.IsNullOrWhiteSpace(title)) title = "Tab-" + _currentTabIndex;

                        //Tab信息
                        AppCode.Models.TabInfo tabInfo = new AppCode.Models.TabInfo()
                        {
                            Index = _currentTabIndex,
                            ModuleId = moduleId,
                            Title = title
                        };

                        //Tag标签容器
                        Border borderTab = new Border();
                        borderTab.Tag = tabInfo;
                        borderTab.Name = "MZERP_tab_" + _currentTabIndex;
                        borderTab.Style = this.FindResource("borderTab_Current") as Style;
                        borderTab.MouseLeftButtonDown += BorderTab_MouseLeftButtonDown;
                        borderTab.PreviewMouseLeftButtonDown += BorderTab_PreviewMouseLeftButtonDown;

                        //存放名称及操作按钮
                        WrapPanel panel = new WrapPanel();
                        panel.VerticalAlignment = VerticalAlignment.Center;

                        //标题
                        TextBlock lblText = new TextBlock();
                        lblText.ToolTip = title;
                        lblText.Text = Core.Handler.StringHandler.SubStringsByBytes(title, 20);
                        lblText.Style = this.FindResource("lblTabTitle_Current") as Style;

                        //添加标签名称
                        panel.Children.Add(lblText);

                        if (canClose)
                        {
                            //关闭按钮
                            Canvas canvasClose = new Canvas();
                            canvasClose.Tag = _currentTabIndex;
                            canvasClose.Width = 8;
                            canvasClose.Height = 8;
                            canvasClose.Margin = new Thickness(5, 0, 0, 0);
                            canvasClose.Background = Brushes.Black;
                            canvasClose.PreviewMouseLeftButtonDown += CanvasClose_PreviewMouseLeftButtonDown;
                            canvasClose.MouseEnter += CanvasClose_MouseEnter;
                            canvasClose.MouseLeave += CanvasClose_MouseLeave;
                            Line line1 = new Line()
                            {
                                X1 = 0,
                                Y1 = 0,
                                X2 = 8,
                                Y2 = 8,
                                StrokeThickness = 1,
                                Stroke = Brushes.White
                            };
                            Line line2 = new Line()
                            {
                                X1 = 8,
                                Y1 = 0,
                                X2 = 0,
                                Y2 = 8,
                                StrokeThickness = 1,
                                Stroke = Brushes.White
                            };
                            canvasClose.Children.Add(line1);
                            canvasClose.Children.Add(line2);

                            //添加关闭按钮
                            panel.Children.Add(canvasClose);
                        }

                        borderTab.Child = panel;

                        this.panelNavs.RegisterName(borderTab.Name, borderTab);
                        this.panelNavs.Children.Add(borderTab);
                        this.scrollNavs.ScrollToEnd();

                        if (this.scrollNavs.ExtentHeight > 40)
                        {
                            this.panelTopScroll.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.panelTopScroll.Visibility = Visibility.Collapsed;
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas canvas = (sender as Canvas);
            foreach (Line line in canvas.Children)
            {
                line.Stroke = Brushes.White;
            }

            canvas.Background = Brushes.Black;
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Canvas canvas = (sender as Canvas);
            foreach (Line line in canvas.Children)
            {
                line.Stroke = Brushes.Red;
            }

            canvas.Background = Brushes.Transparent; //((canvas.Parent as WrapPanel).Parent as Border).Background;
        }

        /// <summary>
        /// 关闭标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //页索引
            int ucIndex = DataType.Int((sender as Canvas).Tag, 0);
            if (ucIndex <= 0) return;

            //移除页面
            RemovePage(ucIndex);
        }

        /// <summary>
        /// 移除页面
        /// </summary>
        /// <param name="index"></param>
        public void RemovePage(int index)
        {
            //上一页面
            int prevIndex = 0;
            //是否移除
            bool isRemove = false;

            //循环所有页面
            for (int i = 0; i < this.panelPages.Children.Count; i++)
            {
                //得到页面
                UIElement p = this.panelPages.Children[i];

                //得到控件
                AppCode.Base.BaseUserControl uc = (p as AppCode.Base.BaseUserControl);
                int ucIndex = uc._CurrentPageIndex;
                if (ucIndex <= 0) continue;

                if (isRemove)
                {
                    //已经移除 显示下一页面
                    ShowPage(ucIndex);
                    break;
                }

                if (ucIndex.Equals(index))
                {
                    try
                    {
                        //执行移除事件
                        uc.CloseUC();
                    }
                    catch { }

                    //移除页面
                    this.panelPages.Children.Remove(p);

                    //移除Tab
                    object objTab = this.panelNavs.FindName("MZERP_tab_" + ucIndex);
                    if (objTab != null)
                    {
                        this.panelNavs.Children.Remove(objTab as UIElement);
                    }

                    //标记已经移除
                    isRemove = true;

                    if (_currentTabIndex != index)
                    {
                        //当前页面不是移除的页面 不做处理
                        break;
                    }
                }
                else
                {
                    //上一页面
                    prevIndex = ucIndex;
                }
            }

            if (prevIndex > 0 && isRemove)
            {
                //显示上一页面
                ShowPage(prevIndex);
            }
        }
        /// <summary>
        /// 移除页面并打开新的页面
        /// </summary>
        /// <param name="index"></param>
        /// <param name="uc"></param>
        /// <param name="moduleId"></param>
        /// <param name="title"></param>
        public void RemovePageAndOpenNewPage(int index, AppCode.Base.BaseUserControl uc, long moduleId, string title)
        {
            //移除页面
            RemovePage(index);

            //添加新的页面
            this.AddPage(moduleId, uc, title);
        }
        /// <summary>
        /// 移除所有页面
        /// </summary>
        public void RemoveAllPages()
        {
            //清除Tab标签
            this.panelNavs.Children.Clear();
            //清除所有页面
            this.panelPages.Children.Clear();

            _indexUC = null;
        }
        /// <summary>
        /// 显示页面
        /// </summary>
        /// <param name="index"></param>
        public void ShowPage(int index)
        {
            //隐藏所有页面
            HideAllPage();

            try
            {
                Core.Base.BaseUserControl uc = (this.panelPages.FindName("MZERP_page_" + index) as Core.Base.BaseUserControl);
                uc.Visibility = Visibility.Visible;
                uc.IsActive = true;
                if (uc is AppCode.Base.BaseUserControl) (uc as AppCode.Base.BaseUserControl).ActivateUC();
                _CurrentActivePage = uc;
                Border borderTab = (this.panelNavs.FindName("MZERP_tab_" + index) as Border);

                if (borderTab != null)
                {
                    borderTab.Style = this.FindResource("borderTab_Current") as Style;
                    ((borderTab.Child as WrapPanel).Children[0] as TextBlock).Style = this.FindResource("lblTabTitle_Current") as Style;
                }
            }
            catch (Exception ex)
            {
                //显示错误页面
            }
        }
        /// <summary>
        /// 隐藏所有页面
        /// </summary>
        private void HideAllPage()
        {
            if (this.panelPages.Children != null && this.panelPages.Children.Count > 0)
            {
                //隐藏所有页面
                foreach (UIElement p in this.panelPages.Children)
                {
                    Core.Base.BaseUserControl uc = p as Core.Base.BaseUserControl;
                    uc.Visibility = Visibility.Collapsed;
                    uc.IsActive = false;

                    //冻结窗口
                    if (uc == _CurrentActivePage && uc is AppCode.Base.BaseUserControl)
                    {
                        (uc as AppCode.Base.BaseUserControl).ActivateUC();
                    }
                }
            }

            if (this.panelNavs.Children != null && this.panelNavs.Children.Count > 0)
            {
                //所有Tab设置为不活动
                foreach (UIElement p in this.panelNavs.Children)
                {
                    (p as Border).Style = this.FindResource("borderTab") as Style;
                    (((p as Border).Child as WrapPanel).Children[0] as TextBlock).Style = this.FindResource("lblTabTitle") as Style;
                }
            }
        }
        /// <summary>
        /// 点击Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderTab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ClickTab(sender,e);
        }
        /// <summary>
        ///  点击Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderTab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickTab(sender, e);
        }
        /// <summary>
        /// 点击标签
        /// </summary>
        private void ClickTab(object sender, MouseButtonEventArgs e)
        {
            Border borderTab = (sender as Border);
            AppCode.Models.TabInfo tabInfo = borderTab.Tag as AppCode.Models.TabInfo;

            if (tabInfo.Index <= 0) return;

            try
            {
                if (e.ClickCount >= 2)
                {
                    //第一个页面不能关闭
                    if (tabInfo.ModuleId == (int)AppModules.导航 ||
                        tabInfo.ModuleId == (int)AppModules.用户登陆) return;

                    //移除页面
                    RemovePage(tabInfo.Index);
                    return;
                }
            }
            catch { }

            //查看Tab是否存在 移除页面时会触发事件
            if (!this.panelNavs.Children.Contains(borderTab)) return;

            //显示页面
            ShowPage(tabInfo.Index);
        }
        #endregion

        #region 子菜单事件
        /// <summary>
        /// 点击子菜单
        /// </summary>
        /// <param name="menu"></param>
        public void ClickMenu(DataRow rowMenu)
        {
            if (rowMenu == null) return;

            long tableId = rowMenu.GetLong("TableId");
            long moduleId = rowMenu.GetId();
            string moduleName = rowMenu.GetString("ModuleName");
            string code = string.Empty;
            string url = string.Empty;

            //添加一次模块点击次数
            AddModuleClickCount(moduleId);

            #region 独立页面

            try
            {
                //模块编码
                code = rowMenu["ModuleCode"].ToString();
            }
            catch { }

            try
            {
                //地址
                url = rowMenu["Url"].ToString();
            }
            catch { }

            //独立指定页面
            if (!string.IsNullOrWhiteSpace(code) && code.StartsWith("DL_"))
            {
                code = code.Substring(3);
                string dirName = code.Split('_')[0];
                string ucName = code.Substring(dirName.Length + 1);

                try
                {
                    //反射加载页面
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    //反射创建实例
                    dynamic ucDynamic = assembly.CreateInstance("Wsfly.ERP.Std.Views." + dirName + "." + ucName);
                    //是否存在实例
                    if (ucDynamic == null) return;
                    //基类是否基础控件
                    if (ucDynamic is AppCode.Base.BaseUserControl)
                    {
                        //添加独立指定页面
                        AppData.MainWindow.AddPage(moduleId, ucDynamic as AppCode.Base.BaseUserControl, moduleName);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "加载独立指定页面异常");
                }

                return;
            }
            else if (!string.IsNullOrWhiteSpace(code) && code.StartsWith("Independents_"))
            {
                //独立窗口程序
                string winName = code.Split('_')[1];
                ShowIndependentWin(winName);
                return;
            }
            #endregion

            //打开程序
            if (!string.IsNullOrWhiteSpace(url))
            {
                string ext = System.IO.Path.GetExtension(url);
                if (!string.IsNullOrWhiteSpace(ext) && ext.ToString().Equals(".exe"))
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + url.Trim('/');
                    System.Diagnostics.Process.Start(path);
                    return;
                }
            }

            //打开网址
            if (!string.IsNullOrWhiteSpace(url))
            {
                //加载浏览器
                BrowserUC ucBrowser = new BrowserUC(url);

                //弹出窗口
                Views.Components.PageWindow win = new Views.Components.PageWindow(ucBrowser, moduleName);
                ucBrowser._ParentWindow = win;

                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

                double width = screenWidth * 0.8;
                double height = screenHeight * 0.8;

                if (width < 1000) width = 1000;
                if (height < 640) height = 640;

                //默认大小
                win.Width = width;
                win.Height = height;

                //可最大化
                win._ShowMaxBtn = true;

                //显示
                win.Show();
                return;
            }

            //是否已经加载成功表配置
            if (!AppGlobal.TableConfigsLoadSuccess)
            {
                AppAlert.FormTips(gridMain, "请稍候尝试，正在努力加载数据表！");
                return;
            }

            //表配置
            TableInfo tableInfo = AppGlobal.GetTableConfig(tableId);
            if (tableInfo != null)
            {
            }

            //模块编号
            List<long> moduleIds = new List<long>();
            AppGlobal.GetModuleId(tableId, ref moduleId, ref moduleIds);

            //打开列表页面
            ListUC uc = new ListUC(tableId, moduleId, moduleIds, moduleName);
            AddPage(moduleId, uc, moduleName);
        }
        /// <summary>
        /// 添加模块点击次数
        /// </summary>
        /// <param name="moduleId"></param>
        private void AddModuleClickCount(long moduleId)
        {
            //添加一次点击次数
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    long userId = AppGlobal.UserInfo.Id;

                    string tbName = "Sys_UserQuickActions";

                    string sql = "select count(*) SL from " + tbName + " where [UserId]=" + userId + " and [ModuleDetailId]=" + moduleId;

                    DataRow rowResult = SQLiteDao.GetTableRowBySql(sql);

                    int sl = rowResult.GetInt("SL");

                    if (sl > 0)
                    {
                        //已经存在
                        sql = "update [" + tbName + "] set [Count]=[Count]+1 where [UserId]=" + userId + " and [ModuleDetailId]=" + moduleId;
                    }
                    else
                    {
                        //不存在
                        sql = "insert into [" + tbName + "]([UserId],[ModuleDetailId],[Count]) values(" + userId + "," + moduleId + ",1)";
                    }

                    SQLiteDao.ExecuteSQL(sql);
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "添加模块点击次数异常");
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 用户事件
        /// <summary>
        /// 保存用户配置
        /// </summary>
        public void SaveUserConfig()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //得到所有属性
                    System.Reflection.PropertyInfo[] pis = Core.Handler.PropertyHandler.GetPropertyInfoArray(typeof(Core.Models.Sys.UserConfig));

                    SQLParam param = new SQLParam()
                    {
                        Action = Actions.修改,
                        TableName = AppGlobal.SysTableName_UserConfigs,
                        OpreateCells = new List<KeyValue>(),
                        Wheres = new List<Where>()
                        {
                             new Where("UserId", AppGlobal.UserInfo.UserId)
                        }
                    };

                    foreach (System.Reflection.PropertyInfo pi in pis)
                    {
                        //属性名称
                        string name = pi.Name;

                        //是否用户编号
                        if (name.ToUpper().Equals("USERID")) continue;

                        //得到值
                        object obj = pi.GetValue(AppGlobal.UserConfig, null);

                        //添加到要修改的值
                        param.OpreateCells.Add(new KeyValue(name, obj));
                    }

                    //更新
                    bool flag = SQLiteDao.Update(param);

                    if (!flag)
                    {
                        //添加
                        param.Action = Actions.添加;
                        param.OpreateCells.Add(new KeyValue("UserId", AppGlobal.UserInfo.UserId));
                        SQLiteDao.Insert(param);
                    }
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region 验证用户
        /// <summary>
        /// 本地Key
        /// </summary>
        /// <returns></returns>
        private string GetLocalPCKey()
        {
            string cpuId = PCHandler.GetPCID("Win32_Processor");
            string boardId = PCHandler.GetPCID("Win32_BaseBoard");

            string key = cpuId + boardId;

            return Core.Encryption.EncryptionDES.Encrypt(key, "CpuBoard");
        }
        #endregion

        #region 拖动窗体大小
        private const int WM_NCHITTEST = 0x0084;//
        private readonly int agWidth = 12;      //拐角宽度
        private readonly int bThickness = 5;    //边框宽度
        private Point mousePoint = new Point(); //鼠标坐标
        private const int WM_GETMINMAXINFO = 0x0024;//大小变化

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            System.Windows.Interop.HwndSource hwndSource = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }
        }
        public enum HitTest : int
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
        }
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                switch (msg)
                {
                    case WM_NCHITTEST:
                        {
                            this.mousePoint.X = (lParam.ToInt32() & 0xFFFF);
                            this.mousePoint.Y = (lParam.ToInt32() >> 16);

                            //标记消息已经处理
                            //handled = true;

                            //this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            //{
                            //    //double screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                            //    //double screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                            //}));

                            if (_SystemDPI == 96)
                            {
                                #region 测试鼠标位置
                                // 窗口右下角
                                if (this.ActualWidth + this.Left - this.mousePoint.X <= this.agWidth && this.ActualHeight + this.Top - this.mousePoint.Y <= this.agWidth)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                                }
                                // 窗口右侧
                                else if (this.ActualWidth + this.Left - this.mousePoint.X <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTRIGHT);
                                }
                                // 窗口下方
                                else if (this.ActualHeight + this.Top - this.mousePoint.Y <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOM);
                                }
                                else
                                {
                                    handled = false;
                                    break;
                                }
                                #endregion
                            }
                            else if (_SystemDPI == 120)
                            {
                                //屏幕放大125%
                                #region 测试鼠标位置

                                double mouseX = this.mousePoint.X * 0.8;
                                double mouseY = this.mousePoint.Y * 0.8;

                                // 窗口右下角
                                if (this.ActualWidth + this.Left - mouseX <= this.agWidth && this.ActualHeight + this.Top - mouseY <= this.agWidth)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                                }
                                // 窗口右侧
                                else if (this.ActualWidth + this.Left - mouseX <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTRIGHT);
                                }
                                // 窗口下方
                                else if (this.ActualHeight + this.Top - mouseY <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOM);
                                }
                                else
                                {
                                    handled = false;
                                    break;
                                }
                                #endregion
                            }
                            else if (_SystemDPI == 144)
                            {
                                //屏幕放大150%
                                #region 测试鼠标位置

                                double mouseX = this.mousePoint.X * 0.6666666666666667;
                                double mouseY = this.mousePoint.Y * 0.6666666666666667;

                                // 窗口右下角
                                if (this.ActualWidth + this.Left - mouseX <= this.agWidth && this.ActualHeight + this.Top - mouseY <= this.agWidth)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                                }
                                // 窗口右侧
                                else if (this.ActualWidth + this.Left - mouseX <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTRIGHT);
                                }
                                // 窗口下方
                                else if (this.ActualHeight + this.Top - mouseY <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOM);
                                }
                                else
                                {
                                    handled = false;
                                    break;
                                }
                                #endregion
                            }
                            else if (_SystemDPI == 192)
                            {
                                //屏幕放大200%
                                #region 测试鼠标位置

                                double mouseX = this.mousePoint.X * 0.5;
                                double mouseY = this.mousePoint.Y * 0.5;

                                // 窗口右下角
                                if (this.ActualWidth + this.Left - mouseX <= this.agWidth && this.ActualHeight + this.Top - mouseY <= this.agWidth)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                                }
                                // 窗口右侧
                                else if (this.ActualWidth + this.Left - mouseX <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTRIGHT);
                                }
                                // 窗口下方
                                else if (this.ActualHeight + this.Top - mouseY <= this.bThickness)
                                {
                                    handled = true;
                                    return new IntPtr((int)HitTest.HTBOTTOM);
                                }
                                else
                                {
                                    handled = false;
                                    break;
                                }
                                #endregion
                            }
                            else
                            {
                                handled = false;
                                break;
                            }
                        }

                }

                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
            }

            return IntPtr.Zero;
        }
        #endregion

        #region 通信管道
        System.IO.Pipes.NamedPipeServerStream pipeServer = null;
        private void InitPIPE()
        {
            return;

            pipeServer = new System.IO.Pipes.NamedPipeServerStream("MZERPPIPE", System.IO.Pipes.PipeDirection.InOut, 20, System.IO.Pipes.PipeTransmissionMode.Message, System.IO.Pipes.PipeOptions.Asynchronous);

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {

                    pipeServer.BeginWaitForConnection((o) =>
                    {
                        //处理消息
                        ProcessPIPEMSG(o);
                    }, pipeServer);
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "消息管道异常");
                }
            });
        }
        /// <summary>
        /// 处理PIPE消息
        /// </summary>
        /// <param name="o"></param>
        private void ProcessPIPEMSG(IAsyncResult o)
        {
            System.IO.Pipes.NamedPipeServerStream server = (System.IO.Pipes.NamedPipeServerStream)o.AsyncState;

            try
            {
                server.EndWaitForConnection(o);
                System.IO.StreamReader sr = new System.IO.StreamReader(server);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(server);
                string result = null;
                string clientName = server.GetImpersonationUserName();
                while (true)
                {
                    result = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(result)) continue;

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, result, AppCode.Enums.FormTipsType.Info);
                        return;
                    }));

                    if (result.Equals("SHOWMZERPDISPLY"))
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            this.Show();
                            if (_isMaxWin)
                            {
                                this.WindowState = WindowState.Maximized;
                            }
                            else
                            {
                                this.WindowState = WindowState.Normal;
                            }
                            return;
                        }));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "获取管道消息异常");
            }
            finally
            {
                if (server != null)
                {
                    server.Disconnect();
                    server.Dispose();
                    server.Close();
                }
            }
        }
        #endregion

        #region 引导
        /// <summary>
        /// 重新加载引导
        /// </summary>
        public void ReloadGuide()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (_indexUC != null)
                {
                    _indexUC.LoadGuideNav();
                }
            }));
        }
        /// <summary>
        /// 注册成功
        /// </summary>
        public void Registed()
        {
            try
            {
                //注册信息
                var dicRegisterInfo = AppGlobal.GetRegisterInfo();
                if (dicRegisterInfo != null)
                {
                    this.lblRegister.Text = "授权给：" + dicRegisterInfo["Name"];
                    this.lblRegister.Foreground = Brushes.Black;
                    this.lblRegister.Cursor = Cursors.Arrow;

                    this.lblRegister.MouseDown -= LblRegister_MouseDown;

                    _registerUC._ParentWindow.Close();
                    AppAlert.FormTips(gridMain, "授权认证成功！", AppCode.Enums.FormTipsType.Right);
                }
            }
            catch (Exception ex) { }
        }
        #endregion
    }
}
