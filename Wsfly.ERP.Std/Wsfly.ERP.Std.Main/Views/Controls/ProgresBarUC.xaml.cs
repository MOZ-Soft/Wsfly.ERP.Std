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

namespace Wsfly.ERP.Std.Views.Controls
{
    /// <summary>
    /// ProgresBarUC.xaml 的交互逻辑
    /// </summary>
    public partial class ProgresBarUC : BaseUserControl
    {
        /// <summary>
        /// 值
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxValue { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 比例
        /// </summary>
        public double BL { get; set; }
        /// <summary>
        /// 显示数字
        /// </summary>
        public bool ShowSZ { get; set; }
        
        /// <summary>
        /// 构造
        /// </summary>
        public ProgresBarUC(string title, double value, double maxValue = 100, bool showSZ = true)
        {
            Title = title;
            Value = value;
            MaxValue = maxValue;
            ShowSZ = showSZ;

            double bl = 0;
            if (MaxValue > 0) bl = Value / MaxValue;
            if (bl > 1) bl = 1D;
            BL = bl;

            InitializeComponent();

            this.Loaded += ProgresBarUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgresBarUC_Loaded(object sender, RoutedEventArgs e)
        {
            //百分比
            string blText = BL.ToString("0%");
            if (ShowSZ) blText += " (" + Value + "/" + MaxValue + ")";
            this.lblProgres.Text = blText;
            this.lblTitle.Text = Title;
        }
        /// <summary>
        /// 呈现界面
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double outerWidth = this.borderOuter.ActualWidth - 2;
            if (outerWidth <= 0) return;
            this.borderInner.Width = outerWidth * BL;
        }
        /// <summary>
        /// 设置标题颜色
        /// </summary>
        /// <param name="red"></param>
        public void SetTitleColor(SolidColorBrush brush)
        {
            this.lblTitle.Foreground = brush;
        }
    }
}
