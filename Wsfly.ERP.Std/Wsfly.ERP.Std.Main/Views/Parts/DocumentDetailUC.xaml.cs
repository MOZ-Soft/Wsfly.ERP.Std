
using Wsfly.ERP.Std.AppCode.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// DocumentDetailUC.xaml 的交互逻辑
    /// </summary>
    public partial class DocumentDetailUC : BaseUserControl
    {
        System.Data.DataRow _row = null;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="row"></param>
        public DocumentDetailUC(System.Data.DataRow row)
        {
            _row = row;

            InitializeComponent();

            this.Loaded += NoticeDetailUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoticeDetailUC_Loaded(object sender, RoutedEventArgs e)
        {
            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            //隐藏滚动条
            (_ParentWindow as Components.PageWindow).scrollMainFrame.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            (_ParentWindow as Components.PageWindow).scrollMainFrame.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            //事件
            this.btnClose.Click += BtnClose_Click;
            this.btnReply.Click += BtnReply_Click;
            this.btnSaveReply.Click += BtnSaveReply_Click;
            this.btnCancelReply.Click += BtnCancelReply_Click;

            //赋值
            this.lblTitle.Text = _row["Title"].ToString();
            this.lblAuthor.Text = _row["CreateUserName"].ToString();
            this.lblCreateDate.Text = DataType.DateTime(_row["CreateDate"].ToString(), DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
            this.lblAttachment.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrWhiteSpace(_row["Attachment"].ToString()))
            {
                this.lblAttachment.Text = "附件：" + _row["Attachment"].ToString();
                this.lblAttachment.Visibility = Visibility.Visible;
                this.lblAttachment.Tag = _row["Attachment"].ToString();
                this.lblAttachment.MouseLeftButtonDown += LblAttachment_MouseLeftButtonDown;
            }

            //标记文本已读
            SetDocumentReaded();

            //显示等待
            ShowLoading(gridMain);
            
            AddViews();
        }

        /// <summary>
        /// 取消回复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancelReply_Click(object sender, RoutedEventArgs e)
        {
            this.borderReply.Visibility = Visibility.Collapsed;
            this.WindowsFormsHost.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 提交回复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveReply_Click(object sender, RoutedEventArgs e)
        {
            string reply = this.txtReply.Text.Trim();
            if (string.IsNullOrEmpty(reply))
            {
                AppAlert.FormTips(gridMain, "请输入回复内容！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            //换行
            reply = reply.Replace("\r\n", "<br />");

            //隐藏回复
            this.borderReply.Visibility = Visibility.Collapsed;

            //显示等待
            ShowLoading(gridMain);
        }

        /// <summary>
        /// 回复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReply_Click(object sender, RoutedEventArgs e)
        {
            this.borderReply.Visibility = Visibility.Visible;
            this.WindowsFormsHost.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 加载回复
        /// </summary>
        private void LoadingReply()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        
                        TableName = "Sys_DocumentReplys",
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="DocumentId", CellValue=DataType.Long(_row["Id"], 0) }
                        },
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy() { CellName="UserId" },
                            new OrderBy() { CellName="Id" }
                        }
                    };

                    //得到数据
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0) return;

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        long currentUserId = -1;
                        string htmlReplyUser = null;
                        string htmlReplys = "<div>";

                        foreach (DataRow row in dt.Rows)
                        {
                            long userId = DataType.Long(row["UserId"], 0);
                            if (userId <= 0) continue;

                            if (currentUserId == -1 || userId != currentUserId)
                            {
                                //添加另一项
                                currentUserId = userId;

                                //没有编号
                                if (currentUserId <= 0) continue;

                                htmlReplyUser = "<div style='border-bottom:solid 1px #ccc; padding:10px; font-weight:bold; font-size:12px;'><span style='font-size:10px;'>●</span> " + row["UserName"] + "</div>";
                                htmlReplys = "<div style=' margin-bottom:20px;'>";
                            }

                            string content = row["Content"].ToString();

                            string[] cts = Regex.Split(content, "<new>", RegexOptions.IgnoreCase);

                            foreach (string ct in cts)
                            {
                                htmlReplys += "<div style='border-bottom:solid 1px #eee; padding:10px 20px 10px 20px; margin-left:24px; color:#666; font-size:12px;'>" + ct + "</div>";
                            }

                            //得到文档
                            System.Windows.Forms.HtmlDocument doc = this.webBrowser.Document;
                            System.Windows.Forms.HtmlElement ele = doc.GetElementById("docReply");
                            System.Windows.Forms.HtmlElement newReply = doc.CreateElement("div");
                            newReply.InnerHtml = htmlReplyUser + htmlReplys + "</div>";
                            ele.AppendChild(newReply);
                        }

                        return null;
                    }));
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 标记文档已读
        /// </summary>
        private void SetDocumentReaded()
        {

        }

        /// <summary>
        /// 点击附件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblAttachment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //下载文件
            string path = (sender as TextBlock).Tag.ToString();
            if (string.IsNullOrWhiteSpace(path)) return;

            //保存目录
            string saveDir = Core.Handler.UploadFileHandler.ChooseFolderDialog();
            if (string.IsNullOrWhiteSpace(saveDir)) return;

            //设置状态
            this.lblAttachment.Text += "[正在下载...]";
            
        }

        /// <summary>
        /// 窗口大小改变
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
            this.webBrowser.Width = Convert.ToInt32(_ParentWindow.Width - 40);
            this.webBrowser.Height = Convert.ToInt32(_ParentWindow.Height - 230);

            this.WindowsFormsHost.Width = Convert.ToInt32(_ParentWindow.Width - 40);
            this.WindowsFormsHost.Height = Convert.ToInt32(_ParentWindow.Height - 230);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            (this._ParentWindow as Components.PageWindow).Close();
        }

        /// <summary>
        /// 添加访问
        /// </summary>
        private void AddViews()
        {
            long id = DataType.Long(_row["Id"].ToString(), 0);
            if (id <= 0) return;
            
        }
    }
}
