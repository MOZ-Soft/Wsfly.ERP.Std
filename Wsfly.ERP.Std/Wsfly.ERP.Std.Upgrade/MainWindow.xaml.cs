using Wsfly.ERP.Std.Upgrade.AppCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace Wsfly.ERP.Std.Upgrade
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 升级地址
        /// </summary>
        private string _upgradeUrl = "http://upgrade.wsfly.com/MOZERP-Std/App.xml";
        /// <summary>
        /// XML本地信息
        /// </summary>
        XmlDocument _xmlAppInfo = null;

        /// <summary>
        /// 背景索引
        /// </summary>
        int _bgIndex = 1;
        /// <summary>
        /// 背景定时索引
        /// </summary>
        int _bgTimerIndex = 0;
        /// <summary>
        /// 资源命名空间
        /// </summary>
        string _resNamespace = "Wsfly.ERP.Std.Upgrade.Resources.";
        /// <summary>
        /// 配置文件路径
        /// </summary>
        string _configPath = AppDomain.CurrentDomain.BaseDirectory + "Cnf\\Wsfly.App.config";

        /// <summary>
        /// 程序名称
        /// </summary>
        string _appName = "MOZ-ERP";
        /// <summary>
        /// 当前版本号
        /// </summary>
        string _currentVersion = string.Empty;

        /// <summary>
        /// 文件下载计时器
        /// </summary>
        Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// 新版本号
        /// </summary>
        string _NewVersion = string.Empty;

        /// <summary>
        /// 升级忽略的配置
        /// </summary>
        ///AppCode.UpgradeFilterConfig _upgradeFilterConfig = null;

        /// <summary>
        /// 带参建代理
        /// FlushClient fc = new FlushClient(Function);
        /// this.Invoke(fc, new object[] { param });
        /// </summary>
        public delegate object FlushClientBase(object obj);
        /// <summary>
        /// 无参数代理
        /// </summary>
        /// <returns></returns>
        public delegate void FlushClientBaseDelegate();

        //过滤的文件 不要覆盖
        private string[] _filterFiles =
        {
            "DonotUpgrade.config",
            "Wsfly.App.config",
            "Upgrade.exe",

            "AppLog\\.*",
            "AppData\\.*",
            "AppFiles\\.*",
            "Cnf\\.*",
            "en-US\\.*",
            "zh-CN\\.*",
            "x64\\.*",
            "x86\\.*",

            "Antlr3.Runtime.dll",
            "BouncyCastle.Crypto.dll",
            "EntityFramework.dll",
            "EntityFramework.SqlServer.dll",
            "HtmlAgilityPack.dll",
            "ICSharpCode.SharpZipLib.dll",
            "JSCaller.dll",
            "LiveCharts.dll",
            "LiveCharts.Wpf.dll",
            "MahApps.Metro.IconPacks.dll",
            "Microsoft.mshtml.dll",
            "NCalc.dll",
            "Newtonsoft.Json.dll",
            "ThoughtWorks.QRCode.dll",
            "zxing.dll",

            "System.Data.SQLite.dll",
            "System.Data.SQLite.EF6.dll",
            "System.Data.SQLite.Linq.dll",

            "Smith.WPF.HtmlEditor.dll",
            "Smith.WPF.HtmlEditor.resources.dll",
            "Smith.WPF.HtmlEditor.Demo.resources.dll",

            "NPOI .Net40\\.*",
            "NPOI.dll",
            "NPOI.OOXML.dll",
            "NPOI.OpenXml4Net.dll",
            "NPOI.OpenXmlFormats.dll",
            
            //"FastReport.Net\\.*",
            //"FastReport.Bars.dll",
            //"FastReport.dll",
            //"FastReport.Editor.dll",
        };

        /// <summary>
        /// 构造
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            InitUI();

            this.Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// 初始界面
        /// </summary>
        private void InitUI()
        {
            //加载资源
            this.bg.Background = ResourceHandler.GetImageBrush(_resNamespace + "Banner1.png");
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //如果有不升级的配置文件，则删除
                string donotupgradePath = AppDomain.CurrentDomain.BaseDirectory + "DonotUpgrade.config";
                if (System.IO.File.Exists(donotupgradePath)) System.IO.File.Delete(donotupgradePath);
            }
            catch { }

            //鼠标移动
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;

            //定时器
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            //当前版本
            if (File.Exists(_configPath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_configPath);
                _currentVersion = xmlDoc.DocumentElement["Version"].InnerText;
                this.lblVersion.Text = _currentVersion;
            }

            try
            {
                Thread threadUpgrade = new Thread(delegate ()
                {
                    int tryTimes = 0;
                    while (true)
                    {
                        try
                        {
                            //检查App是否有新版本
                            CheckApp();
                        }
                        catch (Exception ex)
                        {
                            tryTimes++;
                            if (tryTimes >= 3) throw ex;
                        }
                    }
                });
                threadUpgrade.IsBackground = true;
                threadUpgrade.Start();
            }
            catch (Exception ex)
            {
                SetState("升级更新失败，请关闭后重试！");
            }
        }

        /// <summary>
        /// 得到本地ImageSouce
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ImageSource GetLocalImageSource(string path)
        {
            //不存在文件
            if (!File.Exists(path)) return null;

            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.UriSource = new Uri(path);
            bitmapSource.EndInit();
            bitmapSource.Freeze();

            return bitmapSource;
        }

        /// <summary>
        /// 定时更新背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //设置动态
                string sl = this.lblWaiting.Text;
                if (sl.Length >= 3) sl = "";
                sl += ".";

                //省略号
                this.lblWaiting.Text = sl;
                this.lblWaiting2.Text = sl;

                //切换背景
                _bgTimerIndex++;

                if (_bgTimerIndex == 5)
                {
                    _bgTimerIndex = 1;

                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.8)));
                    DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.8)));

                    _bgIndex++;
                    if (_bgIndex >= 5) _bgIndex = 1;

                    if (bg2.Opacity == 0)
                    {
                        bg2.Background = ResourceHandler.GetImageBrush(_resNamespace + "Banner" + _bgIndex + ".png");

                        bg.BeginAnimation(Border.OpacityProperty, fadeOutAnimation);
                        bg2.BeginAnimation(Border.OpacityProperty, fadeInAnimation);
                    }
                    else
                    {
                        bg.Background = ResourceHandler.GetImageBrush(_resNamespace + "Banner" + _bgIndex + ".png");

                        bg.BeginAnimation(Border.OpacityProperty, fadeInAnimation);
                        bg2.BeginAnimation(Border.OpacityProperty, fadeOutAnimation);
                    }
                }
            }));
        }
        /// <summary>
        /// 鼠标点下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch { }
        }

        #region 下载更新
        /// <summary>
        /// 获取最新版本
        /// </summary>
        void CheckApp()
        {
            _xmlAppInfo = GetXML(_upgradeUrl);

            if (_xmlAppInfo == null)
            {
                SetState("没有新版本！");
                ExitUpgrade();
                return;
            }

            //当前版本
            string currentVersion = _currentVersion;

            //版本格式 1.0.01
            Regex regex = new Regex(@"\d+\.\d\.\d{2}");

            //最新版本
            string newVersion = _xmlAppInfo.DocumentElement["ApplicationVersion"].InnerText;
            //是否符合版本格式
            if (!regex.IsMatch(newVersion))
            {
                CurrentIsNew(currentVersion);
                return;
            }

            //版本号对比
            string currentVersionNumber = currentVersion.Replace(".", "");
            string newVersionNumber = newVersion.Replace(".", "");

            //没有新的版本
            if (Int(newVersionNumber, 0) <= Int(currentVersionNumber, 0))
            {
                CurrentIsNew(currentVersion);
                return;
            }

            try
            {
                //查询进程
                Process[] proc = Process.GetProcessesByName(_appName);
                //关闭原有应用程序的所有进程 
                foreach (Process pro in proc)
                {
                    pro.Kill();
                }
            }
            catch { }

            try
            {
                XmlElement xmlFiles = _xmlAppInfo.DocumentElement["ApplicationFiles"];
                double size = Double(_xmlAppInfo.DocumentElement["ApplicationFileSize"].InnerText, 0);
                string readme = _xmlAppInfo.DocumentElement["Readme"].InnerText;

                //是否有得到新的版本信息
                if (xmlFiles.ChildNodes == null || xmlFiles.ChildNodes.Count <= 0 || size <= 0)
                {
                    CurrentIsNew(currentVersion);
                    return;
                }

                //检测到有最新版
                HasNewVersion(newVersion, size);

                //新版本号
                _NewVersion = newVersion;
                //下载新版本
                DownloadApplication(xmlFiles, size);
            }
            catch
            {
                CurrentIsNew(currentVersion);
                return;
            }
        }


        /// <summary>
        /// 下载新版本
        /// MIME
        /// .upgrade
        /// application/otcstream
        /// </summary>
        /// <param name="xmlFiles">文件列表</param>
        /// <param name="size">文件大小</param>
        void DownloadApplication(XmlElement xmlFiles, double size)
        {
            //关闭主程序及服务
            CloseAppAndService();

            try
            {
                //网络路径
                string netClientPath = _upgradeUrl.Substring(0, _upgradeUrl.LastIndexOf('/') + 1);
                //保存路径
                string savePath = AppDomain.CurrentDomain.BaseDirectory;

                //如果超过20分钟未下载完，则退出升级
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 20, 0);
                timer.Tick += Timer_Tick;
                timer.Start();

                //开始计时
                _stopwatch.Start();

                //总下载字节
                double totalDownloadedByte = 0;

                foreach (XmlNode nodeFile in xmlFiles.ChildNodes)
                {
                    //没有文件名
                    if (string.IsNullOrWhiteSpace(nodeFile.InnerText)) continue;

                    //文件路径
                    string path = netClientPath + nodeFile.InnerText;
                    string newFileName = nodeFile.InnerText;
                    double fileSize = 0;
                    newFileName = newFileName.Trim('/').StartsWith("files_") ? newFileName.Substring(newFileName.IndexOf('\\') + 1) : newFileName;
                    newFileName = newFileName.Replace(".upgrade", "");

                    try
                    {
                        //文件大小
                        fileSize = double.Parse(nodeFile.Attributes["Size"].InnerText);
                    }
                    catch { }

                    try
                    {
                        bool isFilter = false;
                        //是否需要过滤的文件名
                        foreach (string pattern in _filterFiles)
                        {
                            //过滤的文件
                            if (Regex.IsMatch(newFileName, pattern, RegexOptions.IgnoreCase))
                            {
                                isFilter = true;
                                break;
                            }
                        }

                        //是否被过滤
                        if (isFilter)
                        {
                            totalDownloadedByte += fileSize;
                            SetProgress(totalDownloadedByte, size);
                            continue;
                        }
                    }
                    catch (Exception ex) { }

                    //保存文件路径
                    string saveFilePath = savePath + newFileName;
                    //保存文件夹
                    string saveDir = System.IO.Path.GetDirectoryName(saveFilePath);
                    if (!System.IO.Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                    try
                    {
                        //开始下载
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(path);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        long totalBytes = response.ContentLength;

                        Stream st = response.GetResponseStream();

                        //写入文件
                        System.IO.Stream so = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);

                        byte[] by = new byte[1024];
                        int osize = st.Read(by, 0, (int)by.Length);

                        //循环字节下载
                        while (osize > 0)
                        {
                            totalDownloadedByte = osize + totalDownloadedByte;

                            so.Write(by, 0, osize);
                            SetProgress(totalDownloadedByte, size);

                            osize = st.Read(by, 0, (int)by.Length);
                        }

                        //关闭写入文件
                        so.Close();

                        //关闭数据流
                        st.Close();
                        //关闭请求
                        response.Close();
                    }
                    catch (Exception) { }
                }

                //停止计时
                _stopwatch.Stop();

                //显示状态
                SetState("更新成功！");

                //写入新版本号
                if (File.Exists(_configPath))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(_configPath);
                    xmlDoc.DocumentElement.SelectSingleNode(@"descendant::Version").InnerXml = _NewVersion;
                    xmlDoc.Save(_configPath);
                }

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //退出更新
                    ExitUpgrade();
                }));
            }
            catch (Exception ex)
            {
                //显示状态
                SetState("下载更新失败了！");
            }
        }
        /// <summary>
        /// 退出升级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //退出升级
                ExitUpgrade();
            }));
        }
        /// <summary>
        /// 退出更新
        /// </summary>
        private void ExitUpgrade()
        {
            try
            {
                //打开主程序
                string path = AppDomain.CurrentDomain.BaseDirectory + _appName + ".exe";
                Process.Start(path);

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //当前程序退出
                    //App.Current.Shutdown(0);
                    //Application.Current.Shutdown(0);
                    Environment.Exit(0);
                }));
            }
            catch
            {
                SetState("返回主程序失败");

                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    //当前程序退出
                    App.Current.Shutdown(0);
                }));
            }
        }
        /// <summary>
        /// 显示检测到最新版程序
        /// </summary>
        /// <param name="newVersion"></param>
        void HasNewVersion(string newVersion, double size)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblNewVersion.Text = newVersion;
                this.lblTotalSize.Text = CalculateSize(size);

                this.panelWaiting.Visibility = Visibility.Collapsed;
                this.panelProgressBar.Visibility = Visibility.Visible;
            }));
        }
        /// <summary>
        /// 当前为最新版本
        /// </summary>
        /// <param name="version"></param>
        void CurrentIsNew(string version)
        {
            SetCurrentVersion(version);
            SetState("当前为最新版本！");

            //退出更新
            ExitUpgrade();
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="state"></param>
        void SetState(string state)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblState.Text = state;
                this.lblWaiting.Visibility = Visibility.Collapsed;
            }));
        }
        /// <summary>
        /// 设置最新版本
        /// </summary>
        /// <param name="version"></param>
        void SetCurrentVersion(string version)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.lblNewVersion.Text = version;
            }));
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="currentDownloaded"></param>
        /// <param name="totalSize"></param>
        void SetProgress(double currentDownloaded, double totalSize)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //已下载大小
                this.lblDownloadSize.Text = CalculateSize(currentDownloaded);

                this.lblProgress.Text = (currentDownloaded / totalSize).ToString("0%");
                this.progressBar.Value = (currentDownloaded / totalSize) * 100;
            }));
        }
        /// <summary>
        /// 整型 Int32
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int Int(object value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch { return defaultValue; }
        }
        /// <summary>
        /// 转换为Double
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double Double(object value, double defaultValue)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch { return defaultValue; }
        }
        /// <summary>
        /// 计算大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static string CalculateSize(double size)
        {
            string unit = "B";

            if (size <= 1024) { }
            else if (size <= 1024 * 1024)
            {
                //总大小在10MB以内
                size = size / 1024;
                unit = "KB";
            }
            else if (size <= 1024 * 1024 * 1024)
            {
                size = size / 1024 / 1024;
                unit = "MB";
            }
            else
            {
                size = size / 1024 / 1024 / 1024;
                unit = "G";
            }

            return size.ToString("f") + " " + unit;
        }
        /// <summary>
        /// 关闭主程序及服务
        /// </summary>
        private void CloseAppAndService()
        {
            try
            {
                //检查进程是否存在，存在则关闭
                Process[] ps = Process.GetProcessesByName(_appName);
                if (ps.Length > 0)
                {
                    //遍历进程
                    foreach (Process p in ps)
                    {
                        //结束进程
                        p.Kill();
                    }
                }
            }
            catch { }
        }
        #endregion

        #region 辅助程序
        /// <summary>
        /// 通过url请求XML数据
        /// 使用：【HttpWebRequest】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <returns>返回XmlDocument</returns>
        public static XmlDocument GetXML(string url)
        {
            const string sAccept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            const string sUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            const string sContentType = "application/x-www-form-urlencoded";

            XmlDocument xml = new XmlDocument();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;

                HttpWebResponse WebResponse = (HttpWebResponse)webRequest.GetResponse();
                Stream respStream = WebResponse.GetResponseStream();

                xml.Load(respStream);

                respStream.Dispose();
                respStream.Close();

                WebResponse.Close();
            }
            catch (Exception)
            {
                return null;
            }

            return xml;
        }
        #endregion
    }
}
