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
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// QuerySettingUC.xaml 的交互逻辑
    /// </summary>
    public partial class QuerySettingUC : BaseUserControl
    {
        //表配置
        TableInfo _tableConfig = null;

        /// <summary>
        /// 构造
        /// </summary>
        public QuerySettingUC(TableInfo tableInfo)
        {
            _tableConfig = tableInfo;

            InitializeComponent();

            this.Loaded += QuerySettingUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuerySettingUC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCells();

            this.btnSave.Click += BtnSave_Click;
            this.btnClose.Click += BtnClose_Click;
        }

        /// <summary>
        /// 加载列
        /// </summary>
        private void LoadCells()
        {
            foreach (CellInfo cell in _tableConfig.Cells)
            {
                if (!cell.IsShow) continue;

                CheckBox cb = new CheckBox();
                cb.Tag = cell.Id;
                cb.Content = cell.CnName;
                cb.IsChecked = cell.IsQuery;
                cb.Width = 175;
                cb.Margin = new Thickness(0, 0, 0, 5);

                this.panelCells.Children.Add(cb);
            }
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            List<long> listQuerys = new List<long>();

            foreach (UIElement ele in this.panelCells.Children)
            {
                CheckBox cb = ele as CheckBox;
                if (cb.IsChecked.HasValue && cb.IsChecked.Value)
                {
                    listQuerys.Add(DataType.Long(cb.Tag, 0));
                }
            }

            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    string sql = "update [Sys_TableCells] set [IsQuery]=0 where [ParentId]=" + _tableConfig.Id;
                    if (listQuerys != null && listQuerys.Count > 0) sql += ";update [Sys_TableCells] set [IsQuery]=1 where [Id] in (" + string.Join(",", listQuerys.ToArray()) + ")";

                    bool flag = SQLiteDao.ExecuteSQL(sql);
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            AppAlert.FormTips(gridMain, "查询设置成功！");
                            (_ParentUC as Home.ListUC).SetQueryCells_Callback(listQuerys, this);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "保存查询失败！");
                        }
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "保存查询异常");
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "保存查询异常！");
                        return null;
                    }));
                }
                finally
                {
                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //移除控件
            (_ParentUC as Home.ListUC).gridMain.Children.Remove(this);
        }
    }
}
