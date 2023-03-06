using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Net;

namespace Wsfly.ERP.Std.Core.Base
{
    public class BasePage : System.Windows.Controls.Page
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public BasePage()
        {
            //背景色
            this.Background = Brushes.White;
            //加载
            this.Loaded += new System.Windows.RoutedEventHandler(BasePage_Loaded);
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BasePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //查找框架
            System.Windows.Controls.Frame frameMain = this.FindName("frameMain") as System.Windows.Controls.Frame;
            if (frameMain != null)
            {
                frameMain.Navigated += new System.Windows.Navigation.NavigatedEventHandler(frameMain_Navigated);
            }
        }

        #region 委托
        /// <summary>
        /// 代理
        /// FlushClient fc = new FlushClient(Function);
        /// this.Invoke(fc, new object[] { param });
        /// </summary>
        public delegate object FlushClientBase(object obj);
        public delegate void FlushClientBaseDelegate();

        /// <summary>
        /// WsflyBase事件
        /// </summary>
        protected event WsflyBaseEventHandler WsflyBaseEvent;
        /// <summary>
        /// WsflyBase委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        protected delegate object WsflyBaseEventHandler(object sender, object parameter);
        #endregion

        #region 属性
        /// <summary>
        /// 父窗体
        /// </summary>
        public BaseWindow _ParentWindow { get; set; }
        /// <summary>
        /// 父页面
        /// </summary>
        public BasePage _ParentPage { get; set; }
        /// <summary>
        /// 根目录
        /// </summary>
        public string _RootPath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 是否设计模式
        /// </summary>
        public bool IsDesignMode
        {
            get { return System.ComponentModel.DesignerProperties.GetIsInDesignMode(this); }
        }

        #region 分页属性

        int _pageIndex = 1;
        int _pageSize = 2000;
        int _pageCount = 0;
        /// <summary>
        /// 当前页索引
        /// </summary>
        public int _PageIndex { get { return _pageIndex; } set { _pageIndex = value; } }
        /// <summary>
        /// 分页大小
        /// </summary>
        public int _PageSize { get { return _pageSize; } set { _pageSize = value; } }
        /// <summary>
        /// 总页数
        /// </summary>
        public int _PageCount { get { return _pageCount; } set { _pageCount = value; } }
        /// <summary>
        /// 总记录数
        /// </summary>
        public long _TotalCount;
        #endregion
        #endregion       

        #region 公用函数
        /// <summary>
        /// 打开浏览器
        /// </summary>
        /// <param name="url"></param>
        public void OpenBrowser(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            System.Diagnostics.Process.Start(url);
        }
        #endregion

        #region 分页函数
        /// <summary>
        /// 分页事件委托
        /// </summary>
        /// <param name="pageIndex"></param>
        public delegate void PagingDelegate(object sender, int pageIndex);
        /// <summary>
        /// 分页点击事件
        /// </summary>
        public event PagingDelegate PagingEvent;
        /// <summary>
        /// 生成分页按钮
        /// </summary>
        /// <param name="panelPages">存放分页按钮的地方</param>
        public void BuildPagingButtons(System.Windows.Controls.WrapPanel panelPages)
        {
            panelPages.Children.Clear();

            if (_TotalCount <= 0) return;
            if (_TotalCount <= _PageSize)
            {
                //生成分页按钮
                BuildPagingButton(panelPages, 1);
                return;
            }

            ///总页数
            int pageCount = TotalPageCount(_TotalCount, _PageSize);

            if (pageCount > 8)
            {
                int minIndex = _PageIndex - 4;
                int maxIndex = _PageIndex + 4;

                ///最小页
                if (pageCount - _PageIndex < 4) minIndex = minIndex - 4 + (pageCount - _PageIndex);
                ///最大页
                if (_PageIndex - 4 < 0) maxIndex = maxIndex + 4 - _PageIndex;

                ///首页
                if (minIndex > 1)
                {
                    BuildPagingButton(panelPages, 1);
                    BuildPagingButton(panelPages, 0);
                }

                for (int i = minIndex; i <= maxIndex; i++)
                {
                    //最小第一页
                    if (i < 1) continue;
                    //已经最大页
                    if (i > pageCount) break;
                    //生成分页按钮
                    BuildPagingButton(panelPages, i);
                }

                ///尾页
                if (maxIndex < pageCount)
                {
                    BuildPagingButton(panelPages, 0);
                    BuildPagingButton(panelPages, pageCount);
                }
            }
            else
            {
                for (int i = 1; i <= pageCount; i++)
                {
                    //生成分页按钮
                    BuildPagingButton(panelPages, i);
                }
            }
        }
        /// <summary>
        /// 生成分页按钮
        /// </summary>
        /// <param name="panelPages"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        void BuildPagingButton(System.Windows.Controls.WrapPanel panelPages, int pageIndex)
        {
            panelPages.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            panelPages.Margin = new System.Windows.Thickness(0, 10, 0, 10);

            System.Windows.Controls.TextBlock lblPage = new System.Windows.Controls.TextBlock();

            if (pageIndex <= 0)
            {
                lblPage.Text = "..";
                lblPage.Padding = new System.Windows.Thickness(2, 3, 2, 3);
                lblPage.Foreground = Brushes.Gray;
                lblPage.FontSize = 18;
                lblPage.Margin = new System.Windows.Thickness(1, 0, 1, 0);
            }
            else
            {
                lblPage.Tag = pageIndex;
                lblPage.Text = pageIndex.ToString();
                lblPage.Padding = new System.Windows.Thickness(10, 3, 10, 3);
                lblPage.Background = this.FindResource("MainLightBrush") as Brush;
                lblPage.Foreground = Brushes.White;
                lblPage.FontSize = 18;
                lblPage.Margin = new System.Windows.Thickness(1, 0, 1, 0);

                if (pageIndex == _PageIndex)
                {
                    //当前页
                    lblPage.Background = this.FindResource("MainBrush") as Brush;
                }
                else
                {
                    //手势
                    lblPage.Cursor = System.Windows.Input.Cursors.Hand;

                    //点击分页
                    lblPage.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(delegate (object sender, System.Windows.Input.MouseButtonEventArgs e)
                    {
                        if (PagingEvent != null)
                        {
                            //当前分页
                            int index = (int)(sender as System.Windows.Controls.TextBlock).Tag;
                            _PageIndex = index;

                            //分页事件
                            PagingEvent(sender, index);
                        }
                    });
                }
            }

            panelPages.Children.Add(lblPage);
        }
        /// <summary>
        /// 清除分页按钮
        /// </summary>
        /// <param name="panelPages"></param>
        public void ClearPagingButtons(System.Windows.Controls.WrapPanel panelPages)
        {
            panelPages.Children.Clear();
        }
        /// <summary>
        /// 计算总页数
        /// </summary>
        /// <param name="totalCount">总记录数</param>
        /// <param name="pageSize">页码</param>
        /// <returns>页数</returns>
        public int TotalPageCount(long totalCount, int pageSize)
        {
            if (totalCount <= 0) return 0;
            return Convert.ToInt32((totalCount + pageSize - 1) / pageSize);
        }
        #endregion

        #region 浏览器导航
        /// <summary>
        /// 跳转页面
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="title">标题</param>
        public void Navigate(string path, string title = null)
        {
            //生成Uri
            Uri uri = AppHandler.BuildUri(path);

            //导航到地址
            System.Windows.Navigation.NavigationService.GetNavigationService(this).Navigate(uri);

            //设置标题
            if (!string.IsNullOrWhiteSpace(title)) SetTitle(title);
        }
        /// <summary>
        /// 跳转页面
        /// </summary>
        /// <param name="element"></param>
        /// <param name="title"></param>
        public void Navigate(System.Windows.UIElement element, string title = null)
        {
            //导航到内容
            System.Windows.Navigation.NavigationService.GetNavigationService(this).Navigate(element);

            //设置标题
            if (!string.IsNullOrWhiteSpace(title)) SetTitle(title);
        }
        /// <summary>
        /// 前进
        /// </summary>
        public void GoForward()
        {
            System.Windows.Navigation.NavigationService.GetNavigationService(this).GoForward();
        }
        /// <summary>
        /// 后退
        /// </summary>
        public void GoBack()
        {
            System.Windows.Navigation.NavigationService.GetNavigationService(this).GoBack();
        }
        /// <summary>
        /// 窗口标题
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            this.WindowTitle = title;

            if (_ParentWindow == null) return;

            _ParentWindow.Title = title;
        }
        #endregion

        #region 窗体事件
        /// <summary>
        /// 关闭窗体
        /// </summary>
        public void Close()
        {
            //是否有父窗体
            if (_ParentWindow == null) return;

            //关闭窗体
            _ParentWindow.Close();
        }
        /// <summary>
        /// 隐藏窗体
        /// </summary>
        public void Hide()
        {
            //是否有父窗体
            if (_ParentWindow == null) return;

            //隐藏窗体
            _ParentWindow.Hide();
        }
        /// <summary>
        /// 显示窗体
        /// </summary>
        public void Show()
        {
            //是否有父窗体
            if (_ParentWindow == null) return;

            //显示窗体
            _ParentWindow.Show();
        }
        #endregion

        #region 扩展函数
        /// <summary>
        /// 显示子页面
        /// </summary>
        /// <param name="pagePath"></param>
        public virtual void ShowSubPage(string pagePath)
        {
            /*
                <!--子页面窗口-->
                <ScrollViewer x:Name="scrollMainFrame" Height="Auto" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto" Template="{DynamicResource ResourceKey=ScrollViewer}">
                    <ScrollViewer.Content>
                        <Frame x:Name="frameMain" Source="Views/WsPage/HomePage.xaml" NavigationUIVisibility="Hidden"></Frame>
                    </ScrollViewer.Content>
                </ScrollViewer>
             */
            try
            {
                //是否有页面地址
                if (string.IsNullOrWhiteSpace(pagePath)) return;

                //生成Uri
                Uri uri = AppHandler.BuildUri(pagePath);

                //查找框架
                System.Windows.Controls.Frame frameMain = this.FindName("frameMain") as System.Windows.Controls.Frame;

                //是否有框架
                if (frameMain == null) return;

                //完成事件
                //frameMain.Navigated -= new System.Windows.Navigation.NavigatedEventHandler(frameMain_Navigated);
                //frameMain.Navigated += new System.Windows.Navigation.NavigatedEventHandler(frameMain_Navigated);

                //导航到页面
                frameMain.Navigate(uri);
            }
            catch { }
        }
        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frameMain_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSubPageParent();
        }
        /// <summary>
        /// 设置父级对象
        /// </summary>
        public void SetSubPageParent()
        {
            //查找框架
            System.Windows.Controls.Frame frameMain = this.FindName("frameMain") as System.Windows.Controls.Frame;

            //是否有框架
            if (frameMain == null) return;

            if (frameMain.Content is BasePage)
            {
                (frameMain.Content as BasePage)._ParentPage = this;

                if (_ParentWindow != null)
                {
                    (frameMain.Content as BasePage)._ParentWindow = _ParentWindow;
                    (frameMain.Content as BasePage).SetSubPageParent();
                }
            }
        }
        #endregion
    }
}
