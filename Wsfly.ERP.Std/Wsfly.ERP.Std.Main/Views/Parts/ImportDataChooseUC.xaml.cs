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
    /// 导入数据到委托
    /// </summary>
    /// <param name="to"></param>
    public delegate void ChooseImportDataTo_Delegate(ImportDataTo to);

    /// <summary>
    /// ImportDataChooseUC.xaml 的交互逻辑
    /// </summary>
    public partial class ImportDataChooseUC : BaseUserControl
    {
        /// <summary>
        /// 选择导入数据到
        /// </summary>
        public event ChooseImportDataTo_Delegate ChooseImportDataTo_Event;

        /// <summary>
        /// 构造
        /// </summary>
        public ImportDataChooseUC()
        {
            InitializeComponent();

            this.Loaded += ImportDataChooseUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportDataChooseUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnZD.Click += BtnZD_Click;
            this.btnMX.Click += BtnMX_Click;

            this.btnClose.Click += BtnClose_Click;
        }
        /// <summary>
        /// 导入主单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZD_Click(object sender, RoutedEventArgs e)
        {
            //导入到主表
            if (ChooseImportDataTo_Event != null) ChooseImportDataTo_Event(ImportDataTo.主表);
            //移除
            RemoveUC();
        }
        /// <summary>
        /// 导入明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMX_Click(object sender, RoutedEventArgs e)
        {
            //导入到明细
            if (ChooseImportDataTo_Event != null) ChooseImportDataTo_Event(ImportDataTo.明细);
            //移除
            RemoveUC();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            RemoveUC();
        }
        /// <summary>
        /// 移除控件
        /// </summary>
        private void RemoveUC()
        {
            //移除控件
            (_ParentUC as Home.ListUC).gridMain.Children.Remove(this);
        }
    }
    /// <summary>
    /// 导入数据到
    /// </summary>
    public enum ImportDataTo
    {
        主表,
        明细
    }
}
