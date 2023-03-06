using Wsfly.ERP.Std.AppCode.Base;
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

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// HelpDetailUC.xaml 的交互逻辑
    /// </summary>
    public partial class HelpDetailUC : BaseUserControl
    {
        /// <summary>
        /// 行
        /// </summary>
        DataRow _row = null;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="row"></param>
        public HelpDetailUC(DataRow row)
        {
            _row = row;

            InitializeComponent();

            this.Loaded += HelpDetailUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpDetailUC_Loaded(object sender, RoutedEventArgs e)
        {
            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            //隐藏滚动条
            (_ParentWindow as Components.PageWindow).scrollMainFrame.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            (_ParentWindow as Components.PageWindow).scrollMainFrame.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

            //事件
            this.btnClose.Click += BtnClose_Click;

            //赋值
            this.lblTitle.Text = _row["Title"].ToString();
            this.lblCreateDate.Text = DataType.DateTime(_row["CreateDate"].ToString(), DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");

            //显示等待
            ShowLoading(gridMain);
            
            AddViews();
        }

        /// <summary>
        /// 添加访问
        /// </summary>
        private void AddViews()
        {
            long id = DataType.Long(_row["Id"].ToString(), 0);
            if (id <= 0) return;
            
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
    }
}
