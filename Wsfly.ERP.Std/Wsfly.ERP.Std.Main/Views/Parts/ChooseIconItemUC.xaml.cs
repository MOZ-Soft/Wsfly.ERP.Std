
using Wsfly.Client.PC.AppCode.Base;
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

namespace Wsfly.Client.PC.Views.Parts
{
    /// <summary>
    /// ChooseIconItemUC.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseIconItemUC : BaseUserControl
    {
        /// <summary>
        /// 图标
        /// </summary>
        public DataRow _IconRow = null;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool _IsChoose = false;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="row"></param>
        public ChooseIconItemUC(DataRow row)
        {
            _IconRow = row;

            InitializeComponent();

            string icon = row["Icon"].ToString();
            string type = row["Type"].ToString();

            imgIcon.Source = AppGlobal.GetImageSource(icon);
            imgIcon.UseLayoutRounding = true;
            imgIcon.SnapsToDevicePixels = true;

            lblType.Text = type;
        }
        /// <summary>
        /// 标记未选中
        /// </summary>
        internal void UnChoose()
        {
            border.Style = this.FindResource("borderItem") as Style;
            border.BorderBrush = Brushes.LightGray;
            _IsChoose = false;
        }
        /// <summary>
        /// 标记选中
        /// </summary>
        internal void SetChoosed()
        {
            border.Style = this.FindResource("borderItem_Choosed") as Style;
            border.BorderBrush = Brushes.OrangeRed;
            _IsChoose = true;
        }
    }
}
