using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Net;

namespace Wsfly.ERP.Std.AppCode.Base
{
    public class BaseWindow : Core.Base.BaseWindow
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public BaseWindow()
        {
            this.Title = "MOZ-ERP";
            this.Background = Brushes.White;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Icon = Core.Handler.ImageBrushHandler.GetIconImageSouce(Properties.Resources.favicon);
        }

        #region 属性

        #endregion

        #region 公用函数
        /// <summary>
        /// 移动窗体
        /// </summary>
        public void MoveTheWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) { try { DragMove(); } catch { } }
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

        #region 提示层
        /// <summary>
        /// 等待View控件
        /// </summary>
        Views.Components.LoadingView _viewLoading = null;
        /// <summary>
        /// 显示等待
        /// </summary>
        /// <param name="grid"></param>
        public void ShowLoading(System.Windows.Controls.Grid grid)
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

                return;
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
                    return;
                }));
            }
        }
        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="msg"></param>
        public void ShowTips(System.Windows.Controls.Grid grid, string msg, Enums.FormTipsType type = Enums.FormTipsType.Info, int seconds = 3)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                //提示
                AppAlert.FormTips(grid, msg, type, true, seconds);
                return;
            }));
        }
        #endregion
    }
}
