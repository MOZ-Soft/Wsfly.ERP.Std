
using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.AppCode.Models;
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
using Wsfly.ERP.Std.Service.Dao;
using MahApps.Metro.IconPacks;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// UploadFileUC.xaml 的交互逻辑
    /// </summary>
    public partial class FileManagerUC : BaseUserControl
    {
        /// <summary>
        /// 当前路径
        /// </summary>
        string _path = null;
        /// <summary>
        /// 选择的文件列表
        /// </summary>
        List<FileItemInfo> _files = new List<FileItemInfo>();
        /// <summary>
        /// 当前选择文件
        /// </summary>
        FileItemInfo _currentChooseFile = null;
        /// <summary>
        /// 当前显示目录
        /// </summary>
        DirItemInfo _currentShowDir = null;
        /// <summary>
        /// 文件夹级别
        /// </summary>
        List<DirItemInfo> _Dirs = new List<DirItemInfo>();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="path"></param>
        public FileManagerUC(string path = null)
        {
            //如果没有窗体则是主窗体
            if (this._ParentWindow == null)
            {
                this._ParentWindow = AppData.MainWindow;
            }

            _path = path;

            InitializeComponent();

            this.Loaded += UploadFileUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadFileUC_Loaded(object sender, RoutedEventArgs e)
        {
            //第一级
            DirItemInfo dirInfo = new DirItemInfo() { Id = 0, ParentId = 0, Name = "根目录" };
            _Dirs.Add(dirInfo);
            _currentShowDir = dirInfo;

            //加载操作
            LoadActions();

            //加载数据
            OpenDir();

            //初始窗口大小
            InitSize();
            this._ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            //点击事件
            this.btnDownload.Click += BtnDownload_Click;
            this.btnOK.Click += BtnOK_Click;

            this.AllowDrop = true;
            this.Drop += FileManagerUC_Drop;
        }

        /// <summary>
        /// 拖入文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileManagerUC_Drop(object sender, DragEventArgs e)
        {
            string[] files = null;

            //得到拖入文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = ((string[])e.Data.GetData(DataFormats.FileDrop));
            }

            //拖入的文件
            if (files != null && files.Length > 0)
            {
                //添加到待上传列表
                UploadFiels(files.ToList());
            }
        }


        /// <summary>
        /// 上级窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }

        /// <summary>
        /// 初始大小
        /// </summary>
        private void InitSize()
        {
            double parentWidth = this._ParentWindow.ActualWidth;
            double parentHeight = this._ParentWindow.ActualHeight;

            double reduceWidth = 30;
            double reduceHeight = 156;

            if (this._ParentWindow is MainWindow)
            {
                //主窗口
                reduceWidth = 104;
                reduceHeight = 188;

                this.gridRow_Bottom.Height = new GridLength(50);
                this.btnOK.Visibility = Visibility.Collapsed;
            }

            //reduceHeight += 20;

            this.scrollMainFrame.Width = parentWidth - reduceWidth;
            this.scrollMainFrame.Height = parentHeight - reduceHeight;

            this.panelMain.Width = this.scrollMainFrame.Width - 20;
        }

        /// <summary>
        /// 加载操作
        /// </summary>
        private void LoadActions()
        {
            List<FileActionInfo> actions = new List<FileActionInfo>()
            {
                new FileActionInfo() { Icon = "Plus", Name="添加", Code="add" },
                new FileActionInfo() { Icon = "Close", Name="删除", Code="delete" },
                new FileActionInfo() { Icon = "FolderPlus", Name="创建", Code="folder" },
                new FileActionInfo() { Icon = "Autorenew", Name="刷新", Code="reload" },
            };

            Style btnNull = this.FindResource("btnNull") as Style;

            foreach (FileActionInfo act in actions)
            {
                WrapPanel panel = new WrapPanel();

                //图标
                PackIconMaterial pi = new PackIconMaterial();
                pi.Width = 12;
                pi.Height = 12;
                pi.Foreground = Brushes.Black;
                pi.VerticalAlignment = VerticalAlignment.Center;
                pi.HorizontalAlignment = HorizontalAlignment.Center;
                pi.Kind = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), act.Icon);

                //标签
                TextBlock lbl = new TextBlock();
                lbl.Text = act.Name;
                lbl.FontSize = 14;
                lbl.VerticalAlignment = VerticalAlignment.Center;

                panel.Children.Add(pi);
                panel.Children.Add(lbl);

                //按钮
                Button btn = new Button();
                btn.Style = btnNull;
                btn.Tag = act;
                btn.Cursor = Cursors.Hand;
                btn.Margin = new Thickness(10, 0, 0, 0);
                btn.Content = panel;
                btn.Click += BtnAction_Click;

                this.panelActions.Children.Add(btn);
            }
            this.panelActions.HorizontalAlignment = HorizontalAlignment.Right;
        }

        /// <summary>
        /// 点击操作按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            FileActionInfo act = (sender as Button).Tag as FileActionInfo;
            if (act == null) return;

            switch (act.Code)
            {
                case "add":
                    //添加文件
                    List<string> files = Core.Handler.UploadFileHandler.ChooseFilesDialog();
                    UploadFiels(files);
                    break;
                case "delete":
                    //删除文件夹或文件
                    if (_files == null || _files.Count <= 0) return;
                    DeleteFiles();
                    break;
                case "folder":
                    //创建文件夹
                    BuildFolder();
                    break;
                case "reload":
                    //刷新
                    OpenDir();
                    break;

            }
        }

        /// <summary>
        /// 生成文件夹
        /// </summary>
        private void BuildFolder()
        {
            Parts.FileItemUC uc = new FileItemUC(true, false, _currentShowDir.Id);
            uc.MouseDown += UCFileItem_MouseDown;
            uc.MouseDoubleClick += UCFileItem_MouseDoubleClick;
            this.panelMain.Children.Add(uc);
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCFileItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FileItemUC item = (sender as FileItemUC);
            FileItemInfo fileInfo = item._FileInfo;

            //正在编辑名称
            if (item._IsEditName) return;

            if (item._IsChoosed)
            {
                _files.Remove(fileInfo);
                this.lblChooseFile.Text = "";
            }
            else
            {
                _files.Add(fileInfo);
                this.lblChooseFile.Text = fileInfo.FileName;
            }

            item.SetChoose();

            this.btnOK.IsEnabled = false;
            this.btnDownload.IsEnabled = false;

            if (fileInfo.IsFloder)
            {
                //选择目录
                this.btnOK.IsEnabled = false;
                this.btnDownload.IsEnabled = true;
            }
            else
            {
                //选择文件
                this.btnOK.IsEnabled = true;
                this.btnDownload.IsEnabled = true;

                //当前选择文件
                _currentChooseFile = fileInfo;

                string fileExt = fileInfo.FileExt.ToLower();
                string[] imgExts = { ".jpg", ".png", ".gif", ".jpeg" };
                if (imgExts.Contains(fileExt.ToLower()))
                {
                    if (item._IsLoading || item._IsLoaded) return;

                    System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                    {
                        try
                        {
                            //加载图片
                            //string path = AppGlobal.DownloadTempFile(fileInfo.FilePath);
                            item._IsLoading = false;
                            item._IsLoaded = true;

                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                //item.LoadedPic(path);
                                return null;
                            }));
                        }
                        catch (Exception ex) { }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// 双击文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCFileItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileItemUC item = (sender as FileItemUC);
            FileItemInfo fileInfo = item._FileInfo;

            //正在编辑名称
            if (item._IsEditName) return;

            if (fileInfo.IsFloder)
            {
                //目录信息
                DirItemInfo dirInfo = new DirItemInfo() { Id = fileInfo.Id, ParentId = fileInfo.ParentId, Name = fileInfo.FileName };

                //上级
                _Dirs.Add(dirInfo);
                //显示目录
                _currentShowDir = dirInfo;
                //打开目录
                OpenDir();
            }
            else
            {
                try
                {
                }
                catch { }
            }
        }

        /// <summary>
        /// 生成路径
        /// </summary>
        private void BuildNavs()
        {
            //是否有路径
            if (_Dirs == null || _Dirs.Count <= 0) return;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                this.panelNavs.Children.Clear();
                int navCount = 0;

                foreach (DirItemInfo info in _Dirs)
                {
                    TextBlock lblNav = new TextBlock();
                    lblNav.Text = Core.Handler.StringHandler.SubStringsByBytes(info.Name, 12) + (info.Name.Equals("根目录") ? "：" : " > ");
                    lblNav.ToolTip = info.Name;
                    lblNav.Foreground = Brushes.Gray;
                    lblNav.Cursor = Cursors.Hand;
                    lblNav.Tag = info;
                    lblNav.MouseLeftButtonDown += LblNav_MouseLeftButtonDown;

                    this.panelNavs.Children.Add(lblNav);

                    //导航级数
                    navCount++;

                    //超过10级 不显示
                    if (navCount > 10)
                    {
                        TextBlock lblNavLast = new TextBlock();
                        lblNavLast.Text = "...";
                        lblNavLast.Foreground = Brushes.Gray;
                        this.panelNavs.Children.Add(lblNavLast);
                        break;
                    }
                }

                return null;
            }));
        }

        /// <summary>
        /// 点击导航
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblNav_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DirItemInfo info = (sender as TextBlock).Tag as DirItemInfo;

            if (info.Equals(_currentShowDir)) return;

            int count = _Dirs.Count - 1;

            for (int i = count; i >= 0; i--)
            {
                if (!_Dirs[i].Equals(info))
                {
                    _Dirs.RemoveAt(i);
                    continue;
                }
                break;
            }

            _currentShowDir = info;
            OpenDir();
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        private void OpenDir()
        {
            SQLParam param = new SQLParam()
            {
                PageIndex = 1,
                PageSize = 999,
                TableName = "Sys_Files",
                OrderBys = new List<OrderBy>()
                {
                    new OrderBy() { CellName = "FileExt", Type = OrderType.顺序 },
                    new OrderBy() { CellName = "FileName", Type = OrderType.顺序 },
                    new OrderBy() { CellName = "Id", Type = OrderType.顺序 }
                },
                Wheres = new List<Where>()
                {
                    new Where() { CellName="IsDelete", CellValue=false },
                    new Where() { CellName="ParentId", CellValue=_currentShowDir.Id },
                    new Where() { CellName="UserId", CellValue=AppGlobal.UserInfo.UserId }
                }
            };

            _currentChooseFile = null;
            _files = new List<FileItemInfo>();
            this.panelMain.Children.Clear();

            //生成路径
            BuildNavs();

            //显示等等
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    DataTable dt = SQLiteDao.GetTable(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (_currentShowDir.Id > 0)
                        {
                            FileItemUC ucParentDir = new FileItemUC(false, true);
                            ucParentDir.MouseDown += UcParentDir_MouseDown;
                            this.panelMain.Children.Add(ucParentDir);
                        }

                        if (dt == null || dt.Rows.Count <= 0)
                        {
                            HideLoading();
                            return null;
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            FileItemUC uc = new FileItemUC(row);
                            uc.MouseDown += UCFileItem_MouseDown;
                            uc.MouseDoubleClick += UCFileItem_MouseDoubleClick;
                            this.panelMain.Children.Add(uc);
                        }

                        HideLoading();

                        return null;
                    }));
                }
                catch
                {
                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 返回上级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcParentDir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //移除当前级
            _Dirs.RemoveAt(_Dirs.Count - 1);
            //设置当前级
            _currentShowDir = _Dirs[_Dirs.Count - 1];
            //打开目录
            OpenDir();
        }

        /// <summary>
        /// 删除文件 标记删除
        /// </summary>
        private void DeleteFiles()
        {
            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    string ids = "";

                    foreach (FileItemInfo info in _files)
                    {
                        ids += info.Id > 0 ? info.Id + "," : "";
                    }

                    ids = ids.Trim(',');

                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_Files",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("IsDelete", 1)
                        },
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="Id", CellValue=ids, Type= WhereType.包含 }
                        }
                    };

                    bool flag = SQLiteDao.Update(param);

                    if (flag)
                    {
                        //清除选择
                        if (_files.Contains(_currentChooseFile)) _currentChooseFile = null;

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            List<FileItemUC> items = new List<FileItemUC>();

                            foreach (FileItemUC uc in this.panelMain.Children)
                            {
                                if (_files.Contains(uc._FileInfo))
                                {
                                    items.Add(uc);
                                }
                            }

                            if (items != null && items.Count > 0)
                            {
                                foreach (FileItemUC uc in items)
                                {
                                    this.panelMain.Children.Remove(uc);
                                }
                            }

                            HideLoading();

                            //清除
                            _files = new List<FileItemInfo>();

                            return null;
                        }));
                    }
                    else
                    {
                        HideLoading();
                    }
                }
                catch { }
            });
            thread.Start();
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_currentChooseFile == null)
            {
                AppAlert.FormTips(gridMain, "请选择要返回的文件！", AppCode.Enums.FormTipsType.Error);
                return;
            }

            //回调
            (_ParentWindow as Views.Components.PopWindow).CallBack(_currentChooseFile);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        private void UploadFiels(List<string> files)
        {
            //是否有文件
            if (files == null || files.Count <= 0) return;

            foreach (string path in files)
            {
                //文件项信息
                FileItemInfo fileInfo = new FileItemInfo()
                {
                    Id = 0,
                    ParentId = _currentShowDir.Id,
                    OrgPath = path,
                    FileName = "",
                    FileExt = "",
                    FileSize = 0,
                    IsFloder = false,
                    UserId = AppGlobal.UserInfo.UserId
                };

                if (File.Exists(path))
                {
                    //是文件
                    FileInfo fInfo = new FileInfo(path);
                    fileInfo.FileSize = fInfo.Length;
                    fileInfo.FileExt = fInfo.Extension.ToLower();
                    fileInfo.FileName = fInfo.Name;
                }
                else if (Directory.Exists(path))
                {
                    //是文件夹
                    DirectoryInfo dInfo = new DirectoryInfo(path);
                    fileInfo.IsFloder = true;
                    fileInfo.FileName = dInfo.Name;
                }
                else
                {
                    //都不是
                    continue;
                }

                try
                {
                    string fileName = DateTime.Now.ToFileTime() + fileInfo.FileExt;

                    string saveDir = AppDomain.CurrentDomain.BaseDirectory + "AppFiles\\";
                    if (!System.IO.Directory.Exists(saveDir)) System.IO.Directory.CreateDirectory(saveDir);

                    string saveFileName = saveDir + fileName;
                    System.IO.File.Copy(path, saveFileName);

                    fileInfo.FilePath = "AppFiles\\" + fileName;

                    //保存到数据库
                    long id = SQLiteDao.Insert(new SQLParam()
                    {
                        TableName = "Sys_Files",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ParentId", fileInfo.ParentId),
                            new KeyValue("FileName", fileInfo.FileName),
                            new KeyValue("FilePath", fileInfo.FilePath),
                            new KeyValue("FileExt", fileInfo.FileExt),
                            new KeyValue("FileSize", fileInfo.FileSize),
                            new KeyValue("UserId", AppGlobal.UserInfo.Id),
                            new KeyValue("IsDelete", false),
                            new KeyValue("CreateDate", DateTime.Now),
                        }
                    });

                    if (id > 0)
                    {
                        UploadFileSuccess(fileInfo);
                        return;
                    }

                    AppAlert.FormTips(gridMain, "上传文件[" + fileInfo.FileName + "]失败！");
                }
                catch (Exception ex)
                {
                    AppAlert.FormTips(gridMain, "上传文件[" + fileInfo.FileName + "]失败！");
                }
            }
        }

        /// <summary>
        /// 上传成功
        /// </summary>
        /// <param name="fileInfo"></param>
        private void UploadFileSuccess(FileItemInfo fileInfo)
        {
            if (fileInfo.ParentId != _currentShowDir.Id) return;



            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                FileItemUC uc = new FileItemUC(fileInfo);
                uc.MouseDown += UCFileItem_MouseDown;
                uc.MouseDoubleClick += UCFileItem_MouseDoubleClick;
                this.panelMain.Children.Add(uc);

                return null;
            }));
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_files == null || _files.Count <= 0)
            {
                AppAlert.FormTips(gridMain, "请选择要另存为的文件！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            //下载文件 保存位置
            string path = Core.Handler.UploadFileHandler.ChooseFolderDialog();
            if (string.IsNullOrWhiteSpace(path)) return;

            string focusFileName = "";

            //下载文件列表
            foreach (FileItemInfo file in _files)
            {
                try
                {
                    file.SaveFloderPath = path;
                    file.UserId = AppGlobal.UserInfo.UserId;

                    //原文件是否存在
                    string filename = AppDomain.CurrentDomain.BaseDirectory + file.FilePath;
                    if (!System.IO.File.Exists(filename))
                    {
                        AppAlert.FormTips(gridMain, "原文件[" + file.FileName + "]不存在！");
                        continue;
                    }

                    //复制文件
                    string toFileName = Core.Handler.FileHandler.BuildNotRepeatFilePath(path, System.IO.Path.GetFileNameWithoutExtension(file.FileName), file.FileExt);
                    System.IO.File.Copy(filename, toFileName);

                    focusFileName = toFileName;
                }
                catch (Exception ex) { }
            }

            if (!string.IsNullOrWhiteSpace(focusFileName))
            {
                System.Diagnostics.Process.Start("Explorer", "/select," + focusFileName);
            }
        }
    }

}
