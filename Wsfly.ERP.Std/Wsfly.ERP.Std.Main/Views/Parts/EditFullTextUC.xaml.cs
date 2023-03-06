
using Wsfly.ERP.Std.AppCode.Base;
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
using System.Reflection;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// EditFullTextUC.xaml 的交互逻辑
    /// </summary>
    public partial class EditFullTextUC : BaseUserControl
    {
        /// <summary>
        /// 是否简单编辑器
        /// </summary>
        bool _isSimpleEditor = true;
        /// <summary>
        /// 是否第一次加载完成
        /// </summary>
        bool _isFirstLoaded = true;
        /// <summary>
        /// HTML内容
        /// </summary>
        string _htmlContent = null;

        /// <summary>
        /// 图片列表
        /// </summary>
        List<KeyValue> _ImgSrcs = new List<KeyValue>();

        /// <summary>
        /// 构造
        /// </summary>
        public EditFullTextUC(string html, bool isSimpleEditor = true)
        {
            //是否简单编辑器
            _isSimpleEditor = isSimpleEditor;

            //编辑的HTML代码
            _htmlContent = html;

            //构造
            InitializeComponent();

            //加载事件
            this.Loaded += EditPopTextUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditPopTextUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnCancel.Click += BtnCancel_Click;
            this.btnOK.Click += BtnOK_Click;

            //窗口大小事件
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            if (_isSimpleEditor)
            {
                //简单编辑器
                this.htmlEditor.Text = _htmlContent;
                this.htmlEditor.Focus();
                this.htmlEditor.Visibility = Visibility.Visible;
                this.browserEditor.Visibility = Visibility.Collapsed;
            }
            else
            {
                //网页编辑器
                string path = AppDomain.CurrentDomain.BaseDirectory + "Plug-In/UMEditor/index.html";
                if (!System.IO.File.Exists(path))
                {
                    //不存在插件
                    AppAlert.Alert("非常抱歉，UMEditor插件不存在！");
                    this.btnOK.Visibility = Visibility.Collapsed;
                    return;
                }


                //测试浏览器版本
                //Windows7 WebBrowser IE版本为：IE7
                //path = AppDomain.CurrentDomain.BaseDirectory + "Plug-In/UMEditor/browser.html";
                //this.browserEditor.Navigate(path);
                //return;

                //隐藏富文本编辑器
                this.htmlEditor.Visibility = Visibility.Collapsed;
                this.borderEditors.Visibility = Visibility.Collapsed;

                //显示等待
                ShowLoading(gridMain);

                System.Threading.Thread threadLoading = new System.Threading.Thread(delegate ()
                {
                    //替换网络图片为本地
                    ReplaceHTML();

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //隐藏等待
                        HideLoading();

                        //加载插件
                        this.borderEditors.Visibility = Visibility.Visible;
                        this.browserEditor.Visibility = Visibility.Visible;

                        //拖入文件
                        this.AllowDrop = true;

                        if (_ParentWindow is Components.PageWindow)
                        {
                            (_ParentWindow as Components.PageWindow).AllowDrop = true;
                            (_ParentWindow as Components.PageWindow).Drop += EditFullTextUC_Win_Drop;
                            (_ParentWindow as Components.PageWindow).DragEnter += EditFullTextUC_DragEnter;
                            (_ParentWindow as Components.PageWindow).DragLeave += EditFullTextUC_DragLeave;
                        }

                        //加载编辑器
                        this.browserEditor.AllowDrop = false;
                        this.browserEditor.DragEnter += EditFullTextUC_DragEnter;
                        this.browserEditor.DragLeave += EditFullTextUC_DragLeave;
                        this.browserEditor.Navigating += BrowserEditor_Navigating;
                        this.browserEditor.LoadCompleted += BrowserEditor_LoadCompleted; ;
                        this.browserEditor.Navigate(path);
                        return null;
                    }));
                });
                threadLoading.IsBackground = true;
                threadLoading.Start();

            }
        }
        /// <summary>
        /// 拖入文件离开窗口时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditFullTextUC_DragLeave(object sender, DragEventArgs e)
        {
            this.borderEditors.Visibility = Visibility.Visible;
            this.panelDropImages.Visibility = Visibility.Collapsed;

        }
        /// <summary>
        /// 拖入文件进入窗口时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditFullTextUC_DragEnter(object sender, DragEventArgs e)
        {
            this.borderEditors.Visibility = Visibility.Collapsed;
            this.panelDropImages.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 拖入文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditFullTextUC_Win_Drop(object sender, DragEventArgs e)
        {
            DropFiles(e);
        }
        /// <summary>
        /// 插入的文件作处理
        /// </summary>
        /// <param name="e"></param>
        private void DropFiles(DragEventArgs e)
        {
            this.borderEditors.Visibility = Visibility.Visible;
            this.panelDropImages.Visibility = Visibility.Collapsed;

            string[] files = null;

            //得到拖入文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = ((string[])e.Data.GetData(DataFormats.FileDrop));
            }

            //拖入的文件
            if (files != null && files.Length > 0)
            {
                string html = "";
                string[] imgExts = { ".jpg", ".gif", ".png", ".jpeg" };

                //遍历所有文件
                foreach (string file in files)
                {
                    if (string.IsNullOrWhiteSpace(file)) continue;

                    //上传文件
                    string path = AppGlobal.UploadFile(file);
                    path = AppGlobal.GetUploadFilePath(file);

                    //文件后缀
                    string fileExt = System.IO.Path.GetExtension(file);
                    if (!string.IsNullOrWhiteSpace(fileExt))
                    {
                        if (imgExts.Contains(fileExt))
                        {
                            html += "<img src='file://" + path + "' />";
                            continue;
                        }
                        else
                        {
                            html += "<a href='file://" + path + "'>" + System.IO.Path.GetFileName(file) + "</a>";
                            continue;
                        }
                    }
                }

                //是否有HTML
                if (!string.IsNullOrWhiteSpace(html))
                {
                    this.browserEditor.InvokeScript("InsertHtml", html);
                }
            }
        }

        /// <summary>
        /// 开始导航
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserEditor_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            SetWebBrowserSilent(sender as WebBrowser, true);
        }

        /// <summary>
        /// 设置浏览器静默，不弹错误提示框
        /// </summary>
        /// <param name="webBrowser">要设置的WebBrowser控件浏览器</param>
        /// <param name="silent">是否静默</param>
        private void SetWebBrowserSilent(WebBrowser webBrowser, bool silent)
        {
            try
            {
                FieldInfo fi = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi != null)
                {
                    object browser = fi.GetValue(webBrowser);
                    if (browser != null) browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, browser, new object[] { silent });
                }
            }
            catch { }
        }

        /// <summary>
        /// 是否加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserEditor_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                if (_isFirstLoaded)
                {
                    this.browserEditor.InvokeScript("SetContent", _htmlContent);
                    _isFirstLoaded = false;
                }
            }
            catch { }
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
            this.Height = _ParentWindow.Height - 70;
            this.htmlEditor.Width = _ParentWindow.Width - 32;
            this.htmlEditor.Height = _ParentWindow.Height - 122;
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            string html = "";

            if (_isSimpleEditor)
            {
                //RichTextEditor 里 获取HTML
                html = this.htmlEditor.Text.Trim();
            }
            else
            {
                //UMEditor 里 获取HTML
                var getHtml = this.browserEditor.InvokeScript("GetContent");
                if (getHtml != null) html = getHtml.ToString();
            }

            if (!string.IsNullOrWhiteSpace(html))
            {
                //显示等待
                ShowLoading(gridMain);

                //原标题
                var orgTitle = "";
                if (_ParentWindow is Components.PageWindow)
                {
                    orgTitle = (_ParentWindow as Components.PageWindow).Title;
                    (_ParentWindow as Components.PageWindow).Title += "（正在回传...）";
                }

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);

                        //所有图像节点
                        HtmlAgilityPack.HtmlNodeCollection hncImgs = doc.DocumentNode.SelectNodes("//img");
                        if (hncImgs != null && hncImgs.Count > 0)
                        {
                            foreach (HtmlAgilityPack.HtmlNode node in hncImgs)
                            {
                                //路径
                                string src = node.Attributes["src"].Value;
                                if (node.Attributes.Contains("_src"))
                                {
                                    html = html.Replace("_src=\"" + node.Attributes["_src"].Value + "\"", "");
                                }

                                //是否本地图片
                                if (src.StartsWith(@"file:///"))
                                {

                                }
                            }
                        }

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            HideLoading();
                            if (_ParentWindow is Components.PageWindow)
                            {
                                (_ParentWindow as Components.PageWindow).Title = orgTitle;
                                (_ParentWindow as Components.PageWindow).CallBack(html);
                            }
                            else if (_ParentWindow is Components.PopWindow) (_ParentWindow as Components.PopWindow).CallBack(html);
                            return null;
                        }));
                    }
                    catch (Exception ex) { }
                });
                thread.IsBackground = true;
                thread.Start();
                return;
            }

            (_ParentWindow as Views.Components.PageWindow).CallBack(html);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            //(_ParentWindow as Views.Ctls.PageWindow).Close();
            if (_ParentWindow is Components.PageWindow) (_ParentWindow as Components.PageWindow).Close();
            else if (_ParentWindow is Components.PopWindow) (_ParentWindow as Components.PopWindow).Close();
        }

        /// <summary>
        /// 替换HTML
        /// </summary>
        /// <returns></returns>
        private void ReplaceHTML()
        {
            //加载HTML
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(_htmlContent);

            //所有图像节点
            HtmlAgilityPack.HtmlNodeCollection hncImgs = doc.DocumentNode.SelectNodes("//img");
            if (hncImgs != null && hncImgs.Count > 0)
            {
                foreach (HtmlAgilityPack.HtmlNode node in hncImgs)
                {
                    //路径
                    string src = node.Attributes["src"].Value;

                }
            }
        }
    }
}
