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
using System.Windows.Shapes;

namespace Wsfly.ERP.Std.Views.Components
{
    /// <summary>
    /// ActivateUserWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ActivateUserWindow : BaseWindow
    {
        string _localPCKey = string.Empty;
        string _orderNumber = string.Empty;
        FormTipsView _tipsView = null;

        /// <summary>
        /// 构造
        /// </summary>
        public ActivateUserWindow(string localPCKey)
        {
            _localPCKey = localPCKey;

            InitializeComponent();

            this.Loaded += ActivateUserWindow_Loaded;
        }
        /// <summary>
        /// 激活用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActivateUserWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
