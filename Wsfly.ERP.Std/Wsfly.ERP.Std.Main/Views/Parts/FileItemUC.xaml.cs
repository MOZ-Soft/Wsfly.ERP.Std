
using Wsfly.ERP.Std.AppCode.Models;
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
using Wsfly.ERP.Std.AppCode.Handler;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// FileItemUC.xaml 的交互逻辑
    /// </summary>
    public partial class FileItemUC : UserControl
    {
        /// <summary>
        /// 是否新建文件夹
        /// </summary>
        public bool _IsNewFloder = false;
        /// <summary>
        /// 是否返回上一级
        /// </summary>
        public bool _IsReturnDir = false;
        /// <summary>
        /// 是否选择
        /// </summary>
        public bool _IsChoosed = false;
        /// <summary>
        /// 文件信息
        /// </summary>
        public FileItemInfo _FileInfo = null;
        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool _IsLoading = false;
        /// <summary>
        /// 是否已经加载
        /// </summary>
        public bool _IsLoaded = false;
        /// <summary>
        /// 锁添加文件夹
        /// </summary>
        private object _lockAddFloder = new object();
        /// <summary>
        /// 是否编辑名称
        /// </summary>
        public bool _IsEditName { get; set; }

        /// <summary>
        /// 新建文件夹构造
        /// </summary>
        /// <param name="isFloder"></param>
        public FileItemUC(bool isFloder = true, bool isReturn = false, long parentId = 0)
        {
            _IsNewFloder = isFloder;
            _IsReturnDir = isReturn;

            if (isFloder)
            {
                //新建文件夹
                _FileInfo = new FileItemInfo()
                {
                    FileName = "新建文件夹",
                    FilePath = "",
                    FileExt = "",
                    IsFloder = isFloder,
                    FileSize = 0,
                    ParentId = parentId,
                    Id = 0
                };
            }
            else if (isReturn)
            {
                //返回上级
                _FileInfo = new FileItemInfo()
                {
                    FileName = "返回上级",
                    FilePath = "",
                    FileExt = "",
                    IsFloder = isFloder,
                    FileSize = 0,
                    ParentId = 0,
                    Id = 0
                };
            }

            InitUI();
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileItemUC_Loaded(object sender, RoutedEventArgs e)
        {
            //聚焦
            if (_FileInfo.IsFloder && _IsNewFloder)
            {
                this.txtName.Focus();
            }
        }

        /// <summary>
        /// 大小改变
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }

        /// <summary>
        /// 显示文件项
        /// </summary>
        /// <param name="row"></param>
        public FileItemUC(DataRow row)
        {
            //文件信息
            _FileInfo = new FileItemInfo()
            {
                FileName = row["FileName"].ToString(),
                FilePath = row["FilePath"].ToString(),
                FileExt = row["FileExt"].ToString(),
                FileSize = DataType.Long(row["FileSize"].ToString(), 0),
                Id = DataType.Long(row["Id"].ToString(), 0),
                ParentId = DataType.Long(row["ParentId"].ToString(), 0)
            };

            //是否文件夹
            _FileInfo.IsFloder = string.IsNullOrWhiteSpace(_FileInfo.FileExt);

            InitUI();
        }
        /// <summary>
        /// 显示文件项
        /// </summary>
        /// <param name="row"></param>
        public FileItemUC(FileItemInfo file)
        {
            _FileInfo = file;

            InitUI();
        }

        /// <summary>
        /// 初始界面
        /// </summary>
        private void InitUI()
        {
            InitializeComponent();

            this.Width = 100;
            this.Height = 100;
            this.Margin = new Thickness(0, 0, 5, 5);

            if (_FileInfo.IsFloder)
            {
                if (_IsNewFloder)
                {
                    this.Focus();
                    this.lblName.Visibility = Visibility.Collapsed;
                    this.txtName.Visibility = Visibility.Visible;
                }
            }
            else
            {
                string name = _FileInfo.FileName;
                string ext = _FileInfo.FileExt;

                ext = ext.Trim('.').ToUpper();

                this.lblExt.Text = ext;
            }

            this.lblName.ToolTip = _FileInfo.FileName;
            this.lblName.Text = Core.Handler.StringHandler.SubStringsByBytes(_FileInfo.FileName, 12);

            this.txtName.Text = _FileInfo.FileName;
            this.txtName.SelectAll();
            this.txtName.KeyDown += TxtName_KeyDown;
                        
            this.MouseEnter += FileItemUC_MouseEnter;
            this.MouseLeave += FileItemUC_MouseLeave;

            if (_IsReturnDir)
            {
                //返回上一级
                this.lblExt.Visibility = Visibility.Collapsed;
                this.borderReturn.Visibility = Visibility.Visible;
            }
            else if (_FileInfo.IsFloder)
            {
                //文件夹
                this.lblExt.Visibility = Visibility.Collapsed;
                this.borderReturn.Visibility = Visibility.Collapsed;
                this.imgDir.Visibility = Visibility.Visible;
                this.imgDir.Source = ImageBrushHandler.GetImageSource(Properties.Resources.tag_Dir);

                this.lblName.MouseLeftButtonDown += LblName_MouseLeftButtonDown;
                this.txtName.LostFocus += TxtName_LostFocus;
            }
            else
            {
                this.lblName.MouseLeftButtonDown += LblName_MouseLeftButtonDown;
                this.txtName.LostFocus += TxtName_LostFocus;
            }

            this.Loaded += FileItemUC_Loaded;
        }

        /// <summary>
        /// 按下回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //保存
                SaveChange();
            }
        }

        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileItemUC_MouseLeave(object sender, MouseEventArgs e)
        {
            //已经选择
            if (_IsChoosed)
            {
                this.borderMain.BorderBrush = Brushes.SkyBlue;
                return;
            }

            this.borderMain.BorderBrush = Brushes.White;
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileItemUC_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_IsChoosed)
            {
                this.borderMain.BorderBrush = Brushes.Gray;
                return;
            }
            this.borderMain.BorderBrush = Brushes.SkyBlue;
        }

        /// <summary>
        /// 鼠标点击名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsEditName = true;
            this.lblName.Visibility = Visibility.Collapsed;
            this.txtName.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 编辑框失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtName_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveChange();
        }

        /// <summary>
        /// 更新文件名称
        /// </summary>
        private void UpdateFileName()
        {
            SQLParam param = new SQLParam()
            {
                
                Id = _FileInfo.Id,
                TableName = "Sys_Files",
                OpreateCells = new List<KeyValue>()
                {
                    new KeyValue("FileName", _FileInfo.FileName)
                }
            };

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool flag = SQLiteDao.Update(param);
                }
                catch { }
            });
            thread.Start();
        }

        /// <summary>
        /// 保存文件夹
        /// </summary>
        private void SaveFloder()
        {
            SQLParam param = new SQLParam()
            {
                
                TableName = "Sys_Files",
                OpreateCells = new List<KeyValue>()
                {
                    new KeyValue("ParentId", _FileInfo.ParentId),
                    new KeyValue("FileName", _FileInfo.FileName),
                    new KeyValue("FilePath", _FileInfo.FilePath),
                    new KeyValue("FileExt", _FileInfo.FileExt),
                    new KeyValue("FileSize", _FileInfo.FileSize),
                    new KeyValue("UserId", AppGlobal.UserInfo.UserId)
                }
            };

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                lock (_lockAddFloder)
                {
                    if (!_IsNewFloder) return;

                    try
                    {
                        long id = SQLiteDao.Insert(param);
                        _FileInfo.Id = id;
                        _IsNewFloder = false;
                    }
                    catch { }
                }
            });
            thread.Start();
        }

        /// <summary>
        /// 设置已选择
        /// </summary>
        public void SetChoose()
        {
            if (_IsChoosed)
            {
                this.tagChoosed.Visibility = Visibility.Hidden;
            }
            else
            {
                this.tagChoosed.Visibility = Visibility.Visible;
            }

            _IsChoosed = !_IsChoosed;

            if (this.txtName.Visibility == Visibility.Visible && _IsNewFloder)
            {
                SaveChange();
            }
            else if(!this.lblName.ToolTip.ToString().Trim().Equals(this.txtName.Text.Trim()))
            {
                SaveChange();
            }
        }
        /// <summary>
        /// 保存更改
        /// </summary>
        private void SaveChange()
        {
            _IsEditName = false;

            this.lblName.Visibility = Visibility.Visible;
            this.txtName.Visibility = Visibility.Collapsed;

            if (!_IsNewFloder)
            {
                //没有更改
                if (this.lblName.ToolTip.ToString().Trim().Equals(this.txtName.Text.Trim())) return;
            }

            //有更改
            _FileInfo.FileName = this.txtName.Text.Trim();
            this.lblName.ToolTip = _FileInfo.FileName;
            this.lblName.Text = Core.Handler.StringHandler.SubStringsByBytes(_FileInfo.FileName, 12);            

            if (_IsNewFloder)
            {
                //保存文件夹
                SaveFloder();
            }
            else
            {
                //更新名称
                UpdateFileName();
            }
        }

        /// <summary>
        /// 加载图片成功
        /// </summary>
        /// <param name="path"></param>
        internal void LoadedPic(string path)
        {
            this.imgPic.Visibility = Visibility.Visible;
            this.imgPic.Source = AppGlobal.GetImageSource(path);
        }
    }
}
