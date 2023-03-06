using MahApps.Metro.IconPacks;
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
using Wsfly.ERP.Std.Core.Handler;
using Wsfly.ERP.Std.AppCode.Extensions;
using Wsfly.ERP.Std.Core.Extensions;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// SubNavItem.xaml 的交互逻辑
    /// </summary>
    public partial class SubNavItemUC : UserControl
    {
        /// <summary>
        /// Id
        /// </summary>
        public long __ModuleId { get; set; }
        /// <summary>
        /// 模块名称
        /// </summary>
        public string __ModuleName { get; set; }
        /// <summary>
        /// 上级流程ID
        /// </summary>
        public long __ParentFlowId { get; set; }
        /// <summary>
        /// 菜单信息
        /// </summary>
        public DataRow MenuInfo { get; set; }
        /// <summary>
        /// 是否流程定位
        /// </summary>
        public bool IsFlowPoint { get; set; }
        /// <summary>
        /// 是否有子流程
        /// </summary>
        public bool HasSubFlow { get; set; }
        /// <summary>
        /// 是否根节点
        /// </summary>
        public bool IsRoot { get; set; }
        /// <summary>
        /// 是否有权限
        /// </summary>
        public bool HasAuthority { get; set; }
        /// <summary>
        /// 子菜单项
        /// </summary>
        public List<SubNavItemUC> SubNavs { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="rowMenu"></param>
        public SubNavItemUC(DataRow rowMenu, double zoom = 1)
        {
            InitializeComponent();

            this.SubNavs = new List<SubNavItemUC>();

            this.MenuInfo = rowMenu;
            this.__ModuleId = rowMenu.GetId();
            this.__ModuleName = rowMenu.GetString("ModuleName");
            this.__ParentFlowId = rowMenu.GetLong("ParentFlowId");

            string name = rowMenu["ModuleName"].ToString();
            string icon = rowMenu["Icon"].ToString();
            
            this.lblName.Text = StringHandler.SubStringsByBytes(name, 20);

            this.panelIcon.Height = 56 * zoom;
            this.lblName.Height = 34 * zoom;
            this.lblName.FontSize = this.lblName.FontSize * zoom;

            if (icon.StartsWith(":"))
            {
                string packType = icon.Split(':')[1];
                string iconStr = icon.Split(':')[2];
                string colorStr = icon.Split(':')[3];
                Brush brush = Brushes.Black;

                try
                {
                    brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
                }
                catch (Exception ex) { }

                switch (packType)
                {
                    case "PackIconMaterial":
                        PackIconMaterial pi = new PackIconMaterial();
                        pi.Width = 32 * zoom;
                        pi.Height = 32 * zoom;
                        pi.Foreground = brush;
                        pi.Margin = new Thickness(0, (12 * zoom), 0, 0);
                        pi.VerticalAlignment = VerticalAlignment.Center;
                        pi.HorizontalAlignment = HorizontalAlignment.Center;
                        pi.Kind = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), iconStr);

                        this.panelIcon.Children.Clear();
                        this.panelIcon.Children.Add(pi);
                        break;
                }
            }

            this.MouseEnter += SubNavItem_MouseEnter;
            this.MouseLeave += SubNavItem_MouseLeave;
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubNavItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!HasAuthority) return;

            this.borderMenu.BorderBrush = Brushes.Transparent;
            this.borderMenu.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            this.lblName.FontWeight = FontWeights.Normal;
        }
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubNavItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!HasAuthority) return;

            this.borderMenu.BorderBrush = Brushes.LightGray;
            this.borderMenu.Background = new SolidColorBrush(Color.FromArgb(125, 255, 255, 255));
            this.lblName.FontWeight = FontWeights.Bold;
        }
    }
}
