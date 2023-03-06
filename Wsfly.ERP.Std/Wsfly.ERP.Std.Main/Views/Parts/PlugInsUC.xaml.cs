using ICSharpCode.SharpZipLib.Zip;
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
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// PlugInsUC.xaml 的交互逻辑
    /// </summary>
    public partial class PlugInsUC : BaseUserControl
    {
        /// <summary>
        /// 所有插件
        /// </summary>
        DataTable _dtAllPlugs = null;
        /// <summary>
        /// 插件数据地址
        /// </summary>
        string _pathPlugData = AppDomain.CurrentDomain.BaseDirectory + "Plug-In/Wsfly.Plugs.db";
        /// <summary>
        /// 插件集合
        /// </summary>
        Dictionary<long, Dictionary<string, string>> _dicPlugs = new Dictionary<long, Dictionary<string, string>>();

        /// <summary>
        /// 构造
        /// </summary>
        public PlugInsUC()
        {
            InitializeComponent();

            this.Loaded += PlugInsUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlugInsUC_Loaded(object sender, RoutedEventArgs e)
        {
            //加载插件
            LoadPlugs();

            //初始窗口大小
            InitSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }
        /// <summary>
        /// 窗口大小发生改变
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
            double width = AppData.MainWindow.WinWidth;
            double height = AppData.MainWindow.WinHeight;

            this.tabMain.Height = height - 102;
        }
        /// <summary>
        /// 加载插件
        /// </summary>
        private void LoadPlugs()
        {
            //是否存在插件数据文件
            if (File.Exists(_pathPlugData))
            {
                try
                {
                    string plugData = Core.Handler.FileHandler.ReadFile(_pathPlugData);
                    if (!string.IsNullOrWhiteSpace(plugData))
                    {
                        _dicPlugs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, string>>>(plugData);
                    }
                }
                catch { }
            }

            //显示加载
            ShowLoading(gridMain);

            System.Threading.Thread threadAllPlugs = new System.Threading.Thread(delegate ()
            {
                //从服务器加载所有插件
                LoadAllPlugs();

                //插件
                if (_dtAllPlugs != null && _dtAllPlugs.Rows.Count > 0)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //显示所有插件
                        ShowAllPlugs();

                        //显示已经安装的插件
                        ShowInstalledPlugs();

                        //隐藏加载
                        HideLoading();
                        return null;
                    }));
                }
            });
            threadAllPlugs.IsBackground = true;
            threadAllPlugs.Start();


            //从本地加载已安装插件
        }
        /// <summary>
        /// 加载所有插件
        /// </summary>
        /// <param name="keywords"></param>
        private void LoadAllPlugs(string keywords = "")
        {
            try
            {
                //加载插件
                AppCode.API.ReceiveJson jsonAllPlugs = AppCode.API.CallMZSPAPI.Post("GetPlugIns", new Dictionary<string, object>() {
                    { "MC", keywords },
                    { "PageSize", 20 },
                    { "PageIndex", 1 },
                });

                //是否成功
                if (!jsonAllPlugs.success) return;

                //序列化为数据表
                _dtAllPlugs = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(jsonAllPlugs.data);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 显示所有插件
        /// </summary>
        private void ShowAllPlugs()
        {
            //遍历所有插件
            foreach (DataRow row in _dtAllPlugs.Rows)
            {
                Border border = new Border();
                StackPanel panel = new StackPanel();
                TextBlock lblTitle = new TextBlock();
                TextBlock lblDescribe = new TextBlock();

                border.Background = Brushes.AliceBlue;
                border.Padding = new Thickness(10);
                border.BorderBrush = Brushes.Bisque;
                border.BorderThickness = new Thickness(0, 0, 0, 1);

                lblTitle.Foreground = Brushes.Black;
                lblTitle.FontWeight = FontWeights.Bold;

                lblDescribe.Foreground = Brushes.Gray;
                lblDescribe.FontWeight = FontWeights.Normal;

                lblTitle.Text = row["MC"].ToString() + " v" + row["BB"].ToString();
                lblDescribe.Text = row["MS"].ToString();

                panel.Children.Add(lblTitle);
                panel.Children.Add(lblDescribe);

                bool isInstalled = false;
                long id = DataType.Long(row["Id"], 0);
                if (_dicPlugs != null && _dicPlugs.Count > 0)
                {
                    if (_dicPlugs.ContainsKey(id))
                    {
                        isInstalled = true;
                    }
                }

                if (!isInstalled)
                {
                    //如果是未安装的插件 则显示安装
                    Button btnDownload = new Button();
                    btnDownload.Content = "安装";
                    btnDownload.Style = this.FindResource("btn") as Style;
                    btnDownload.Tag = row["Id"];
                    btnDownload.HorizontalAlignment = HorizontalAlignment.Left;
                    btnDownload.Click += BtnDownload_Click;
                    panel.Children.Add(btnDownload);
                }
                else
                {
                    //已经安装
                    lblTitle.Text = "【已安装】" + lblTitle.Text;
                }

                border.Child = panel;

                this.panelAllPlugIns.Children.Add(border);
            }
        }
        /// <summary>
        /// 显示已安装插件
        /// </summary>
        private void ShowInstalledPlugs()
        {
            //是否有插件
            if (_dicPlugs == null || _dicPlugs.Count < 0) return;

            //遍历所有插件
            foreach (KeyValuePair<long, Dictionary<string, string>> kv in _dicPlugs)
            {
                Border border = new Border();
                StackPanel panel = new StackPanel();
                TextBlock lblTitle = new TextBlock();
                TextBlock lblDescribe = new TextBlock();

                border.Background = Brushes.AliceBlue;
                border.Padding = new Thickness(10);
                border.BorderBrush = Brushes.Bisque;
                border.BorderThickness = new Thickness(0, 0, 0, 1);

                lblTitle.Foreground = Brushes.Black;
                lblTitle.FontWeight = FontWeights.Bold;

                lblDescribe.Foreground = Brushes.Gray;
                lblDescribe.FontWeight = FontWeights.Normal;

                lblTitle.Text = kv.Value["Name"] + " v" + kv.Value["Version"];
                lblDescribe.Text = kv.Value["Describe"];

                panel.Children.Add(lblTitle);
                panel.Children.Add(lblDescribe);

                lblTitle.Text = "【已安装】" + lblTitle.Text;

                border.Child = panel;

                this.panelInstallPlugIns.Children.Add(border);
            }
        }
        /// <summary>
        /// 下载插件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            Button btnDownload = sender as Button;

            try
            {
                //下载文件
                long id = DataType.Long(btnDownload.Tag, 0);
                if (id <= 0) return;

                //查询插件
                DataRow[] rowPlugs = _dtAllPlugs.Select("[Id]=" + id);
                if (rowPlugs == null || rowPlugs.Length <= 0) return;

                //文件路径
                string url = rowPlugs[0]["CJ"].ToString();
                if (string.IsNullOrWhiteSpace(url)) return;
                url = "http://erp.wsfly.com/API/GetFile?url=" + url;

                string version = rowPlugs[0]["BB"].ToString();
                string describe = rowPlugs[0]["MS"].ToString();

                btnDownload.IsEnabled = false;
                btnDownload.Content = "正在下载...";

                //插件名称
                string plugName = rowPlugs[0]["MC"].ToString();

                //文件名称
                string fileName = System.IO.Path.GetFileName(url);

                System.Threading.Thread threadDownload = new System.Threading.Thread(delegate ()
                {
                    //保存路径
                    string savePath = AppDomain.CurrentDomain.BaseDirectory + "Plug-In\\" + fileName;
                    string plugInPath = AppDomain.CurrentDomain.BaseDirectory + "Plug-In\\";

                    //下载文件并保存
                    bool flag = Wsfly.ERP.Std.Core.Handler.WebHandler.DownloadFile(url, savePath);
                    if (flag)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            try
                            {
                                btnDownload.Content = "正在安装...";
                            }
                            catch (Exception ex) { }
                            return null;
                        }));
                        
                        //解压插件
                        UnZip(savePath, plugInPath);

                        //保存插件
                        _dicPlugs.Add(id, new Dictionary<string, string>()
                        {
                            { "Name", plugName },
                            { "Describe", describe },
                            { "Directory", fileName },
                            { "Version", version },
                        });
                        SavePlugs();

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            try
                            {
                                btnDownload.Content = "已安装";
                            }
                            catch (Exception ex) { }
                            return null;
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            try
                            {
                                btnDownload.Content = "安装";
                                btnDownload.IsEnabled = true;
                            }
                            catch (Exception ex) { }
                            return null;
                        }));
                    }
                });
                threadDownload.IsBackground = true;
                threadDownload.Start();
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "下载插件异常");
            }
        }
        /// <summary>
        /// 保存插件
        /// </summary>
        private void SavePlugs()
        {
            if (_dicPlugs == null || _dicPlugs.Count < 0) return;

            try
            {
                string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(_dicPlugs);
                Core.Handler.FileHandler.WriteFile(_pathPlugData, jsonData);
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "保存插件数据异常");
            }
        }
        /// <summary>
        /// ZIP:解压一个zip文件
        /// </summary>
        /// <param name="zipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="targetDirectory">解压到的目录</param>
        /// <param name="password">解压密码</param>
        /// <param name="overWrite">是否覆盖已存在的文件</param>
        public void UnZip(string zipFile, string targetDirectory, string password = "", bool overWrite = true)
        {
            //如果解压到的目录不存在，则创建
            if (!System.IO.Directory.Exists(targetDirectory)) System.IO.Directory.CreateDirectory(targetDirectory);

            //目录结尾
            if (!targetDirectory.EndsWith("\\")) { targetDirectory = targetDirectory + "\\"; }

            using (ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(zipFile)))
            {
                //是否有密码
                if (!string.IsNullOrWhiteSpace(password)) zipfiles.Password = password;

                ZipEntry theEntry;

                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "") directoryName = System.IO.Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = System.IO.Path.GetFileName(pathToZip);

                    if (!System.IO.Directory.Exists(targetDirectory + directoryName)) System.IO.Directory.CreateDirectory(targetDirectory + directoryName);

                    if (fileName != "")
                    {
                        if ((System.IO.File.Exists(targetDirectory + directoryName + fileName) && overWrite) || (!System.IO.File.Exists(targetDirectory + directoryName + fileName)))
                        {
                            using (System.IO.FileStream streamWriter = System.IO.File.Create(targetDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zipfiles.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                zipfiles.Close();
            }
        }
    }
}
