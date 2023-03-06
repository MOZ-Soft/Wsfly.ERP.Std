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

using Wsfly.ERP.Std.AppCode.Base;


namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// AccountUC.xaml 的交互逻辑
    /// </summary>
    public partial class AccountUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public AccountUC()
        {
            InitializeComponent();

            this.Loaded += AccountUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AccountUC_Loaded(object sender, RoutedEventArgs e)
        {
            InitUCSize();
            
        }
    }
}
