
using Wsfly.ERP.Std.AppCode.Base;
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

namespace Wsfly.ERP.Std.Views.Controls
{
    /// <summary>
    /// AreaUC.xaml 的交互逻辑
    /// </summary>
    public partial class AreaUC : BaseUserControl
    {
        /// <summary>
        /// 是否第一次加载
        /// </summary>
        bool _isFirstLoad = true;

        /// <summary>
        /// 是否需要地址
        /// </summary>
        public bool NeedAddress = false;

        /// <summary>
        /// 省份
        /// </summary>
        string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// 选择城市 临时
        /// </summary>
        string TempSelectCity { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string Text
        {
            get
            {
                DataRowView rowProvince = this.ddlProvince.SelectedItem as DataRowView;
                DataRowView rowCity = this.ddlCity.SelectedItem as DataRowView;

                if (rowProvince == null) return null;
                if (rowCity == null) return rowProvince["ShortName"].ToString();

                //省份城市
                string provinceCity = rowProvince["ShortName"] + " " + rowCity["ShortName"];

                //不需要显示地址
                if (!NeedAddress) return provinceCity;

                //返回地区地址
                return provinceCity + " " + this.txtAddress.Text.Trim().Replace(" ", "");
            }
        }

        /// <summary>
        /// 内容
        /// </summary>
        public string BindText
        {
            get { return (string)GetValue(BindTextProperty); }
            set { SetValue(BindTextProperty, value); }
        }
        /// <summary>
        /// 内容依赖属性
        /// </summary>
        public static readonly DependencyProperty BindTextProperty = DependencyProperty.Register("BindText", typeof(string), typeof(DropDownTable), new PropertyMetadata(""));

        /// <summary>
        /// 构造
        /// </summary>
        public AreaUC()
        {
            InitializeComponent();


            this.Loaded += AreaUC_Loaded;
            //选择省份事件
            this.ddlProvince.SelectionChanged += DdlProvince_SelectionChanged;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AreaUC_Loaded(object sender, RoutedEventArgs e)
        {
            //默认不显示地址
            this.txtAddress.Visibility = Visibility.Collapsed;

            if (NeedAddress)
            {
                //需要地址
                this.txtAddress.Visibility = Visibility.Visible;
            }

            try
            {
                //初始值
                if (!string.IsNullOrWhiteSpace(BindText))
                {
                    //有初始值
                    Province = BindText.Split(' ')[0];
                    City = BindText.Split(' ')[1];
                    this.txtAddress.Text = BindText.Split(' ')[2];
                }
                else
                {
                    Province = "广东";
                    City = "中山";
                }
            }
            catch { }

            //加载省份
            LoadProvince();
        }

        /// <summary>
        /// 加载省份
        /// </summary>
        private void LoadProvince()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {                        
                        TableName = "Sys_Areas",
                        Wheres = new List<Where>()
                        {
                             new Where() { CellName="SJID", CellValue=0 },
                             new Where() { CellName="IsStop", CellValue=0 },
                        }
                    };

                    //得到省份
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0) return;

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.ddlProvince.DisplayMemberPath = "ShortName";
                        this.ddlProvince.ItemsSource = dt.AsDataView();

                        if (_isFirstLoad)
                        {
                            try
                            {
                                DataRow row = dt.Select("[ShortName]='" + Province + "'")[0];
                                int cityIndex = dt.Rows.IndexOf(row);
                                this.ddlProvince.SelectedIndex = cityIndex;
                            }
                            catch { }
                        }
                        return null;
                    }));
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 加载城市
        /// </summary>
        private void LoadCity()
        {
            if (this.ddlProvince.SelectedItem == null) return;

            DataRowView row = this.ddlProvince.SelectedItem as DataRowView;
            long id = DataType.Long(row["Id"], 0);

            if (id <= 0) return;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        
                        TableName = "Sys_Areas",
                        Wheres = new List<Where>()
                        {
                             new Where() { CellName="SJID", CellValue=id },
                             new Where() { CellName="IsStop", CellValue=0 },
                        }
                    };

                    //得到省份
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0) return;

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.ddlCity.DisplayMemberPath = "ShortName";
                        this.ddlCity.ItemsSource = dt.AsDataView();

                        if (_isFirstLoad)
                        {
                            //首次加载
                            try
                            {
                                DataRow rowCity = dt.Select("[ShortName]='" + City + "'")[0];
                                int cityIndex = dt.Rows.IndexOf(rowCity);
                                this.ddlCity.SelectedIndex = cityIndex;
                            }
                            catch { }

                            _isFirstLoad = false;
                        }
                        else if (!string.IsNullOrWhiteSpace(TempSelectCity))
                        {
                            //临时选择
                            try
                            {
                                DataRow rowCity = dt.Select("[ShortName]='" + TempSelectCity + "'")[0];
                                int cityIndex = dt.Rows.IndexOf(rowCity);
                                this.ddlCity.SelectedIndex = cityIndex;
                                TempSelectCity = "";
                            }
                            catch { }
                        }
                        else
                        {
                            this.ddlCity.SelectedIndex = 0;
                        }
                        return null;
                    }));
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 选择的省份
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DdlProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCity();
        }
        
        /// <summary>
        /// 设置选项
        /// </summary>
        /// <param name="province"></param>
        /// <param name="city"></param>
        public void SetSelect(string province, string city)
        {
            try
            {
                DataView dv = this.ddlProvince.ItemsSource as DataView;
                DataRow row = dv.Table.Select("[ShortName]='" + province + "'")[0];
                int cityIndex = dv.Table.Rows.IndexOf(row);
                this.ddlProvince.SelectedIndex = cityIndex;

                TempSelectCity = city;
            }
            catch { }
        }
    }
}
