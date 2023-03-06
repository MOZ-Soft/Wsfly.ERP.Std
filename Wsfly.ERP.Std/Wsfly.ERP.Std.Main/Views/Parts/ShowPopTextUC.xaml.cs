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

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ShowPopTextUC.xaml 的交互逻辑
    /// </summary>
    public partial class ShowPopTextUC : BaseUserControl
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public ShowPopTextUC(string text)
        {
            Text = text;

            InitializeComponent();

            this.Loaded += ShowPopTextUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPopTextUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnClose.Click += BtnClose_Click;

            try
            {
                string showHTML = "";
                Dictionary<string, object> objJson = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Text);
                foreach (KeyValuePair<string, object> kv in objJson)
                {
                    showHTML += kv.Key + ":" + kv.Value + "\r\n";
                }

                Text = showHTML;
            }
            catch { }

            this.lblContent.Text = Text;

            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;
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
            this.Width = Convert.ToInt32(_ParentWindow.Width - 50);
            this.lblContent.MaxWidth = this.Width;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this._ParentWindow.Close();
        }
    }
}
