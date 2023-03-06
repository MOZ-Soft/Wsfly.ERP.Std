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

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// LoadingMask.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingView : UserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public LoadingView()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="grid"></param>
        public void Show(Grid grid)
        {
            grid.Children.Add(this);

            Grid.SetRow(this, 0);
            Grid.SetColumn(this, 0);

            if (grid.RowDefinitions.Count > 1)
            {
                Grid.SetRowSpan(this, grid.RowDefinitions.Count);
            }

            if (grid.ColumnDefinitions.Count > 1)
            {
                Grid.SetColumnSpan(this, grid.ColumnDefinitions.Count);
            }
        }
    }
}
