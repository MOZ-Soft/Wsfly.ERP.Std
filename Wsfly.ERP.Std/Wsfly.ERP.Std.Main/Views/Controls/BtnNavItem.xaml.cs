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
    /// btnNavItem.xaml 的交互逻辑
    /// </summary>
    public partial class BtnNavItem : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BtnNavItem), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(BtnNavItem), new PropertyMetadata(null));

        /// <summary>
        /// 文本
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set
            {
                SetValue(TextProperty, value);
                this.lblName.Text = Text;
            }
        }

        /// <summary>
        /// 图标
        /// </summary>
        public ImageSource Icon
        {
            get { return GetValue(IconProperty) as ImageSource; }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// 点击委托
        /// </summary>
        /// <param name="btnNav"></param>
        public delegate void ClickDelegate(BtnNavItem btnNav);
        /// <summary>
        /// 点击事件
        /// </summary>
        public event ClickDelegate ClickEvent;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable
        {
            get { return this.btnTag.IsEnabled; }
            set
            {
                if (value)
                {
                    //启用
                    this.lblName.Opacity = 1;
                }
                else
                {
                    //禁用
                    this.lblName.Opacity = 0.5;
                }

                this.btnTag.IsEnabled = value;
            }
        }

        /// <summary>
        /// 构造
        /// </summary>
        public BtnNavItem()
        {
            InitializeComponent();

            this.Loaded += BtnNavItem_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNavItem_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrWhiteSpace(Text)) this.lblName.Text = Text;
            //if (Icon != null) this.imgTag.Source = Icon;

            this.btnTag.Click += BtnTag_Click;
        }
        /// <summary>
        /// 点击按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTag_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEnable) return;

            if (ClickEvent != null)
            {
                ClickEvent(this);
            }
        }
    }
}
