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
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// EditUC.xaml 的交互逻辑
    /// </summary>
    public partial class EditUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public EditUC(TableInfo tableConfig)
        {
            InitializeComponent();

            this.Loaded += EditUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditUC_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
