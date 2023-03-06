using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Wsfly.ERP.Std.Service.Models;

namespace Wsfly.ERP.Std.AppCode.Base
{
    /// <summary>
    /// 基础用户控件
    /// </summary>
    public class BaseUserControl : Core.Base.BaseUserControl
    {
        //激活
        public delegate void ActivateUC_Delegate();
        public event ActivateUC_Delegate ActivateUC_Event;

        //冻结
        public delegate void UnActivateUC_Delegate();
        public event UnActivateUC_Delegate UnActivateUC_Event;

        /// <summary>
        /// 页面标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 页面索引
        /// </summary>
        public int UCPageIndex { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public BaseUserControl()
        {
            this.Margin = new System.Windows.Thickness(10);
        }

        /// <summary>
        /// 退出操作
        /// </summary>
        public virtual void CloseUC()
        {

        }

        #region 激活/冻结窗口
        /// <summary>
        /// 激活窗口
        /// </summary>
        public void ActivateUC()
        {
            try
            {
                //是否有激活窗口事件
                if (ActivateUC_Event != null)
                {
                    //激活窗口
                    ActivateUC_Event();
                }
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 冻结窗口
        /// </summary>
        public void UnActivateUC()
        {
            try
            {
                //是否有冻结窗口事件
                if (UnActivateUC_Event != null)
                {
                    //冻结窗口
                    UnActivateUC_Event();
                }
            }
            catch (Exception ex) { }
        }
        #endregion

        #region 提示层
        /// <summary>
        /// 等待View控件
        /// </summary>
        Views.Components.LoadingView _viewLoading = null;
        /// <summary>
        /// 显示等待
        /// </summary>
        /// <param name="grid"></param>
        public void ShowLoading(Grid grid)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                try
                {
                    if (_viewLoading != null)
                    {
                        _viewLoading.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        _viewLoading = new Views.Components.LoadingView();
                        _viewLoading.Show(grid);
                    }
                }
                catch { }

                return null;
            }));
        }
        /// <summary>
        /// 隐藏等待
        /// </summary>
        public void HideLoading()
        {
            if (_viewLoading != null)
            {
                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                {
                    _viewLoading.Visibility = System.Windows.Visibility.Collapsed;
                    return null;
                }));
            }
        }
        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="msg"></param>
        public void ShowTips(Grid grid, string msg, Enums.FormTipsType type = Enums.FormTipsType.Info, int seconds = 3)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //提示
                AppAlert.FormTips(grid, msg, type, true, seconds);
                return null;
            }));
        }
        #endregion

        #region 设置页面大小
        /// <summary>
        /// 添加大小
        /// </summary>
        private int _ucAddWidth = 0;
        private int _ucAddHeight = 0;
        /// <summary>
        /// 页面大小改变
        /// </summary>
        public void InitUCSize(int addWidth = -80, int addHeight = -100)
        {
            _ucAddWidth = addWidth;
            _ucAddHeight = addHeight;

            //AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;

            SetUCSize();
        }
        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            SetUCSize();
        }
        /// <summary>
        /// 设置大小
        /// </summary>
        private void SetUCSize()
        {
            this.Width = AppData.MainWindow.WinWidth - 2 + _ucAddWidth;
            this.Height = AppData.MainWindow.WinHeight - 2 + _ucAddHeight;
        }
        #endregion

        #region 弹出窗口
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="element"></param>
        /// <param name="title"></param>
        /// <param name="callback"></param>
        /// <param name="showMaxBtn"></param>
        /// <returns></returns>
        public Views.Components.PopWindow PopWindow(System.Windows.UIElement element, string title, bool showMaxBtn = true)
        {
            Views.Components.PopWindow win = new Views.Components.PopWindow(element, title);
            win._ShowMaxBtn = showMaxBtn;
            return win;
        }
        #endregion

        #region 分页实现
        /// <summary>
        /// 点击翻页回调
        /// </summary>
        /// <param name="pageIndex">点击的页</param>
        /// <param name="pageCount">总页数</param>
        public delegate void ClickPage_Deletage(int pageIndex, int pageCount);

        /// <summary>
        /// 显示分页
        /// </summary>
        /// <param name="result">查询分页结果</param>
        /// <param name="panel">保存分页控件容器</param>
        public void ShowPages(PageResult result, Panel panel, ClickPage_Deletage clickEvent, int showPageCount = 8, bool showToPage = true)
        {
            if (result.PageCount <= 0) return;

            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //清空
                panel.Children.Clear();

                int half = showPageCount / 2;
                int begin = 1;
                int end = begin + showPageCount;

                if (result.PageIndex > half)
                {
                    begin = result.PageIndex - half;
                    end = result.PageIndex + half;
                }
                if (result.PageIndex > result.PageCount - half)
                {
                    begin = result.PageCount - showPageCount;
                    end = result.PageCount;
                }

                //如果起始页小于1则 起始为1
                if (begin < 1) begin = 1;

                //超过最大页面
                if (end > result.PageCount) end = result.PageCount;

                //样式
                Style defaultBorder = this.FindResource("borderPageIndex") as Style;
                Style currentBorder = this.FindResource("borderPageIndex_Current") as Style;

                Style defaultLabel = this.FindResource("lblPageIndex") as Style;
                Style currentLabel = this.FindResource("lblPageIndex_Current") as Style;

                //生成页按钮
                for (int i = begin; i <= end; i++)
                {
                    Style borderStyle = defaultBorder;
                    Style lblStyle = defaultLabel;

                    if (i == result.PageIndex)
                    {
                        borderStyle = currentBorder;
                        lblStyle = currentLabel;
                    }

                    Border border = new Border();
                    TextBlock lbl = new TextBlock();
                    lbl.Text = i.ToString();
                    lbl.Style = lblStyle;

                    border.Style = borderStyle;
                    border.Child = lbl;
                    border.Tag = i;

                    if (i != result.PageIndex)
                    {
                        border.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(delegate (object sender, System.Windows.Input.MouseButtonEventArgs e)
                        {
                            //是否有回调事件
                            if (clickEvent == null) return;

                            //当前页数
                            Border b = sender as Border;
                            int pageIndex = DataType.Int(b.Tag.ToString(), 1);

                            try
                            {
                                //回调
                                clickEvent(pageIndex, result.PageCount);
                            }
                            catch { }
                        });
                    }

                    panel.Children.Add(border);
                }

                //总记录数
                TextBlock lblTotalCount = new TextBlock();
                lblTotalCount.Text = "共" + result.TotalCount + "条 第 " + result.PageIndex + "/" + result.PageCount + " 页";
                lblTotalCount.Margin = new Thickness(20, 0, 20, 0);
                lblTotalCount.VerticalAlignment = VerticalAlignment.Center;
                lblTotalCount.Foreground = AppGlobal.ColorToBrush("#666666");
                panel.Children.Add(lblTotalCount);

                if (showToPage)
                {
                    TextBox txt = new TextBox();
                    txt.Style = this.FindResource("txtToPage") as Style;
                    txt.Width = 40;
                    txt.BorderThickness = new Thickness(1, 1, 0, 1);
                    txt.Text = result.PageIndex.ToString();
                    txt.GotFocus += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                    {
                        txt.Tag = txt.Text;
                        txt.Text = "";
                    });
                    txt.LostFocus += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                    {
                        string text = txt.Text;
                        txt.Text = string.IsNullOrWhiteSpace(text) ? txt.Tag.ToString() : text;
                    });

                    Button btn = new Button();
                    btn.Content = "＞";
                    btn.Padding = new Thickness(0);
                    btn.Margin = new Thickness(0);
                    btn.Style = this.FindResource("btnToPage") as Style;
                    btn.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
                    {
                        //是否有回调事件
                        if (clickEvent == null) return;

                        //当前页数
                        int pageIndex = DataType.Int(txt.Text.Trim(), 1);

                        try
                        {
                            //回调
                            clickEvent(pageIndex, result.PageCount);
                        }
                        catch { }
                    });

                    panel.Children.Add(txt);
                    panel.Children.Add(btn);
                }

                return null;
            }));
        }

        #endregion

        #region Html
        /// <summary>
        /// 生成HTML页面
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public string BuildHtmlPage(string title, string content, bool showReply = true)
        {
            string html = @"
<!DOCTYPE html PUBLIC "" -//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns = ""http://www.w3.org/1999/xhtml""> 
    <head>
        <title>" + title + @"</title>
    </head>
    <body>
        <div id='docContent' style='min-height:400px;'>
            " + content + @"
        </div>
";
            //是否需要回复
            if (showReply)
            {
                html += @"        
        <div style='background:#f9f9f9; color:#666; padding:10px; font-weight:bold; margin-top:30px'>回复</div>
        <div id='docReply'>
            
        </div>";
            }

            html += @"
    </body>
</html>";

            //返回
            return html;
        }
        #endregion
    }
}
