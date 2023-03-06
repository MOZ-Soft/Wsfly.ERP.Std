using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Wsfly.ERP.Std.Core.Base
{
    public class BaseUserControl : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// 是否活动控件
        /// </summary>
        public bool IsActive = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public BaseUserControl()
        {
            this.Background = Brushes.Transparent;
        }

        #region 委托
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
        public delegate object FlushClientBaseDelegate();
        /// <summary>
        /// 无参数代理
        /// </summary>
        /// <returns></returns>
        public delegate void FlushClientBaseDelegate_Void();

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
        /// 父级控件
        /// </summary>
        public BaseUserControl _ParentUC { get; set; }
        /// <summary>
        /// 顶级控件
        /// </summary>
        public BaseUserControl _TopUC { get; set; }
        /// <summary>
        /// 当前页编号
        /// </summary>
        public int _CurrentPageIndex { get; set; }
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
        #endregion       

        #region 数据分页实现

        #region 分页属性

        int _pageIndex = 1;
        int _pageIndexSub = 1;
        int _pageSize = 1000;
        int _pageCount = 0;
        /// <summary>
        /// 当前页索引
        /// </summary>
        public int _PageIndex { get { return _pageIndex; } set { _pageIndex = value; } }
        /// <summary>
        /// 当前页索引 子项
        /// </summary>
        public int _PageIndexSub { get { return _pageIndexSub; } set { _pageIndexSub = value; } }
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

        /// <summary>
        /// 分页事件委托
        /// </summary>
        /// <param name="sender"></param>
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

            //总页数
            int pageCount = TotalPageCount(_TotalCount, _PageSize);

            if (pageCount > 8)
            {
                int minIndex = _PageIndex - 4;
                int maxIndex = _PageIndex + 4;

                //最小页
                if (pageCount - _PageIndex < 4) minIndex = minIndex - 4 + (pageCount - _PageIndex);
                //最大页
                if (_PageIndex - 4 < 0) maxIndex = maxIndex + 4 - _PageIndex;

                //首页
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

                //尾页
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
        #endregion

        #region 公用函数
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

        #region 扩展函数
        /// <summary>
        /// 显示子页面
        /// </summary>
        /// <param name="pagePath"></param>
        public void ShowSubPage(string pagePath)
        {
            /*
                <!--子页面窗口-->
                <ScrollViewer x:Name="scrollMainFrame" Height="Auto" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Auto" Template="{DynamicResource ResourceKey=ScrollViewer}">
                    <ScrollViewer.Content>
                        <Frame x:Name="frameMain" Source="/Views/WsPage/HomePage.xaml" NavigationUIVisibility="Hidden"></Frame>
                    </ScrollViewer.Content>
                </ScrollViewer>
             */
            try
            {
                //是否有页面地址
                if (string.IsNullOrWhiteSpace(pagePath)) return;

                //Page页面
                Uri uri = new Uri("/Views/" + pagePath, System.UriKind.RelativeOrAbsolute);

                //查找框架
                System.Windows.Controls.Frame frameMain = this.FindName("frameMain") as System.Windows.Controls.Frame;

                //导航到页面
                frameMain.Navigate(uri);
            }
            catch { }
        }
        #endregion

        #region 查找控件
        /// <summary>
        /// 查找子控件，并返回一个List集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<T> GetChildObjects<T>(DependencyObject obj) where T : FrameworkElement
        {
            if (obj == null) return null;

            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child));
            }

            return childList;
        }
        /// <summary>
        /// 查找某子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T GetChildObject<T>(DependencyObject obj) where T : FrameworkElement
        {
            if (obj == null) return null;

            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child);
                    if (grandChild != null) return grandChild;
                }
            }
            return null;
        }
        /// <summary>
        /// 通过名称查找某子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            if (obj == null) return null;

            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null) return grandChild;
                }
            }
            return null;
        }
        /// <summary>
        /// 查找第一个类型父控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T GetParentObject<T>(DependencyObject obj) where T : FrameworkElement
        {
            if (obj == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T)
                {
                    return (T)parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
        /// <summary>
        /// 通过名称查找父控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            if (obj == null) return null;

            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
        #endregion
    }
}
