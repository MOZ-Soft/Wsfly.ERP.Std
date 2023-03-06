
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
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// PrintTemplateItemUC.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTemplateItemUC : UserControl
    {
        PrintTemplateListUC _ucParent = null;
        public DataRow _rowTemplate = null;

        /// <summary>
        /// 设置模版地址
        /// </summary>
        public string SetTemplatePath { set { _rowTemplate["TemplatePath"] = value; } }
        public string SetTemplateCode { set { _rowTemplate["TemplateCode"] = value; } }


        /// <summary>
        /// 模版项
        /// </summary>
        public PrintTemplateItemUC(DataRow row, PrintTemplateListUC ucParent)
        {
            if (row == null) return;
            if (ucParent == null) return;

            string tempName = row["TemplateName"].ToString();
            string tempPath = row["TemplatePath"].ToString();
            bool isDefault = DataType.Bool(row["IsDefault"].ToString(), false);

            _rowTemplate = row;
            _ucParent = ucParent;

            InitializeComponent();

            this.txtName.Text = tempName;

            if (isDefault)
            {
                this.lblIsDefault.Text = "（默认）";
                this.btnDefault.Content = "取消默认";
            }

            //模版后缀
            string fileExt = System.IO.Path.GetExtension(tempPath);
            if (fileExt.Equals(".doc") || fileExt.Equals(".docx") || fileExt.Equals(".html"))
            {
                //不可编辑的模版
                this.btnEdit.Visibility = Visibility.Collapsed;
            }

            this.txtName.TextChanged += TxtName_TextChanged;
            this.btnSaveName.Click += BtnSaveName_Click;

            this.btnEdit.Click += BtnEdit_Click;
            this.btnDelete.Click += BtnDelete_Click;
            this.btnDefault.Click += BtnDefault_Click;
            this.btnPrint.Click += BtnPrint_Click;

            //管理员可删除、编辑 打印模版
            if (AppGlobal.UserInfo.UserId != 1)
            {
                this.btnEdit.Visibility = Visibility.Collapsed;
                this.btnDelete.Visibility = Visibility.Collapsed;
            }

            //版权原因暂时无法调用编辑打印模版
            this.btnEdit.Visibility = Visibility.Collapsed;
            this.btnDelete.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveName_Click(object sender, RoutedEventArgs e)
        {
            this.btnSaveName.Visibility = Visibility.Collapsed;

            string orgName = _rowTemplate["TemplateName"].ToString();
            string text = this.txtName.Text.Trim();
            if (orgName.Equals(text)) return;

            long id = DataType.Long(_rowTemplate["Id"], 0);
            if (id <= 0) return;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {                        
                        Id = id,
                        TableName = "Sys_PrintTemplates",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("TemplateName", text)
                        }
                    };

                    //更新
                    bool flag = SQLiteDao.Update(param);

                    this.Dispatcher.Invoke((EventHandler)delegate
                    {
                        if (flag)
                        {
                            //更新名称成功
                            _rowTemplate["TemplateName"] = text;
                        }
                    });
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 显示保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.btnSaveName.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 设为默认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDefault_Click(object sender, RoutedEventArgs e)
        {
            bool nowIsDefault = DataType.Bool(_rowTemplate["IsDefault"].ToString(), false);
            _ucParent.SetDeafult(_rowTemplate, !nowIsDefault);
        }
        /// <summary>
        /// 删除模版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool? flag = AppAlert.Alert("是否确定删除打印模版？", "删除确认", AppCode.Enums.AlertWindowButton.OkCancel);
            if (flag.HasValue && flag.Value)
            {
                _ucParent.DeleteTemplate(_rowTemplate, this);
            }
        }
        /// <summary>
        /// 编辑模版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            _ucParent.EditTemplate(_rowTemplate, this);
        }

        /// <summary>
        /// 设置默认
        /// </summary>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public void SetDefault(bool isDefault)
        {
            string tempName = _rowTemplate["TemplateName"].ToString();
            bool nowIsDefault = DataType.Bool(_rowTemplate["IsDefault"].ToString(), false);

            //当前是否需要修改
            if (nowIsDefault == isDefault) return;

            //修改是否默认
            _rowTemplate["IsDefault"] = isDefault;

            //显示名称
            this.lblIsDefault.Text = "";
            this.btnDefault.Content = "默认";

            if (isDefault)
            {
                this.lblIsDefault.Text = "（默认）";
                this.btnDefault.Content = "取消默认";
            }
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            string code = _rowTemplate["TemplateCode"].ToString();
            int version = DataType.Int(_rowTemplate["Version"], 0);

            (_ucParent._ParentUC as Home.ListUC).ShowPrintView(code, version);
            (_ucParent._ParentUC as Home.ListUC).gridMain.Children.Remove(_ucParent);
        }
    }
}
