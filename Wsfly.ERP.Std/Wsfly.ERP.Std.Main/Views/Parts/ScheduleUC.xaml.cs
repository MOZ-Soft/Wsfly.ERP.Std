using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ScheduleUC.xaml 的交互逻辑
    /// </summary>
    public partial class ScheduleUC : BaseUserControl
    {
        /// <summary>
        /// 工作计划
        /// </summary>
        DataTable _dtGZJH = null;
        /// <summary>
        /// 工作计划索引
        /// </summary>
        int _workplanIndex = 0;
        /// <summary>
        /// 选择日期
        /// </summary>
        DateTime _chooseDate = DateTime.Now;
        /// <summary>
        /// 统计年份
        /// </summary>
        int _tjYear = DateTime.Now.Year;

        /// <summary>
        /// 初始
        /// </summary>
        public ScheduleUC()
        {
            InitializeComponent();

            this.Loaded += ScheduleUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.lblYear.Text = DateTime.Now.Year + "年";
            this.lblMonth.Text = DateTime.Now.Month + "月";

            this.btnToday.Click += BtnToday_Click;
            this.btnPrevMonth.Click += BtnPrevMonth_Click;
            this.btnNextMonth.Click += BtnNextMonth_Click;

            this.btnAddWorkplan.Click += BtnAddWorkplan_Click;
            this.btnSave.Click += BtnSave_Click;
            this.btnCancel.Click += BtnCancel_Click;
            this.btnChooseFile.Click += BtnChooseFile_Click;

            this.ucCalendar.ChooseDate_Event += UcCalendar_ChooseDate_Event;

            this.lblScheduleTitle_List.Text = "工作事项";
            this.lblScheduleTitle_Edit.Text = "添加工作计划";

            this.cbShowYQX.Checked += CbShowYQX_Checked;
            this.cbShowYQX.Unchecked += CbShowYQX_Checked;

            //加载工作计划
            LoadWorkplans();

            //加载年统计
            LoadTJYear();

            //绑定鼠标控制滚动
            BindMouseControlScrollEvent();
            this.scrollWorkplans.ScrollChanged += ScrollWorkplans_ScrollChanged;
        }

        /// <summary>
        /// 重新加载工作计划
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbShowYQX_Checked(object sender, RoutedEventArgs e)
        {
            //加载工作计划
            LoadWorkplans();
        }

        /// <summary>
        /// 加载年统计
        /// </summary>
        private void LoadTJYear()
        {
            this.lblTJYear.Text = _tjYear + "年统计";

            string sql = @"
select '计划总数' TJX,count(*) SL from [Sys_Workplans] where [JHWCRQ]>=@BeginDate and [JHWCRQ]<@EndDate
union all
select '进行中' TJX,count(*) SL from [Sys_Workplans] where [JHWCRQ]>=@BeginDate and [JHWCRQ]<@EndDate and [ZT]='未开始'
union all
select '已完成' TJX,count(*) SL from [Sys_Workplans] where [JHWCRQ]>=@BeginDate and [JHWCRQ]<@EndDate and [ZT]='已完成'
union all
select '已取消' TJX,count(*) SL from [Sys_Workplans] where [JHWCRQ]>=@BeginDate and [JHWCRQ]<@EndDate and [ZT]='已取消'
union all
select '异常' TJX,count(*) SL from [Sys_Workplans] where [JHWCRQ]>=@BeginDate and [JHWCRQ]<@EndDate and [ZT]='未开始' and [JHWCRQ]<date('now','localtime')";

            SQLiteParameter[] ps = new SQLiteParameter[]
            {
                new SQLiteParameter() { ParameterName="@BeginDate", Value=_tjYear+"-01-01" },
                new SQLiteParameter() { ParameterName="@EndDate", Value=(_tjYear+1)+"-01-01" },
            };
            DataTable dt = SQLiteDao.GetTableBySQL(sql, ps);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string TJX = row.GetString("TJX");
                    string SL = row.GetString("SL");

                    switch (TJX)
                    {
                        case "计划总数": this.lblJHSL.Text = SL; break;
                        case "进行中": this.lblJHJXZ.Text = SL; break;
                        case "已完成": this.lblJHYWC.Text = SL; break;
                        case "已取消": this.lblJHYQX.Text = SL; break;
                        case "异常": this.lblJHYC.Text = SL; break;
                    }
                }
            }
        }

        #region 鼠标移动滚动内容
        /// <summary>
        /// 是否鼠标点下
        /// </summary>
        bool _isMouseDown = false;
        /// <summary>
        /// 鼠标位置
        /// </summary>
        double _mouseY = 0;
        /// <summary>
        /// 距离顶部位置
        /// </summary>
        double _marginTop = 0;

        /// <summary>
        /// 绑定鼠标控件滚动事件
        /// </summary>
        private void BindMouseControlScrollEvent()
        {
            //滚动条移动
            this.borderScrollInner.MouseLeftButtonDown += BorderScrollInner_MouseLeftButtonDown;
            this.MouseMove += UC_MouseMove;
            this.MouseUp += UC_MouseUp;
            this.MouseLeave += UC_MouseLeave;
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;
            _isMouseDown = false;
        }

        /// <summary>
        /// 鼠标放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isMouseDown) return;
            _isMouseDown = false;
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UC_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            double outerHeight = this.borderScrollOuter.ActualHeight - 2;
            double rY = e.GetPosition(this).Y;
            double moveY = rY - _mouseY;

            double top = (_marginTop + moveY);
            if (top <= 0) top = 0;
            else if (top > (outerHeight - 100)) top = outerHeight - 100D;

            this.borderScrollInner.Margin = new Thickness(0, top, 0, 0);

            //相对比例
            double bl = top / (outerHeight - 100D);
            double sh = this.scrollWorkplans.ScrollableHeight;
            this.scrollWorkplans.ScrollToVerticalOffset(sh * bl);
        }

        /// <summary>
        /// 鼠标点下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderScrollInner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _mouseY = e.GetPosition(this).Y;
            _marginTop = this.borderScrollInner.Margin.Top;
        }
        /// <summary>
        /// 滚动区域变化发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollWorkplans_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //由鼠标控件时
            if (_isMouseDown) return;

            double scrollableHeight = this.scrollWorkplans.ScrollableHeight;
            if (scrollableHeight <= 0)
            {
                this.borderScrollInner.Visibility = Visibility.Collapsed;
                return;
            }

            this.borderScrollInner.Visibility = Visibility.Visible;

            double verticalOffset = this.scrollWorkplans.VerticalOffset;
            double outerHeight = this.borderScrollOuter.ActualHeight;
            double marginTop = (outerHeight - 100) * (verticalOffset / scrollableHeight);

            this.borderScrollInner.Margin = new Thickness(0, marginTop, 0, 0);
        }
        #endregion

        /// <summary>
        /// 选择日期
        /// </summary>
        /// <param name="dt"></param>
        private void UcCalendar_ChooseDate_Event(DateTime dt)
        {
            _workplanIndex = 1;
            this.panelWorkplans.Children.Clear();

            if (dt.Year != _chooseDate.Year || dt.Month != _chooseDate.Month)
            {
                //重新选择月份
                SetChooseDate(dt);
                return;
            }

            //当前选择的日期
            _chooseDate = dt;

            if (_dtGZJH == null || _dtGZJH.Rows.Count <= 0) return;

            //工作计划
            DataRow[] rowsGZRZ = null;

            rowsGZRZ = _dtGZJH.Select("[JHWCRQ]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [JHWCRQ]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "' ");
            rowsGZRZ = rowsGZRZ.OrderBy(p => p["ZT"]).ToArray();

            if (rowsGZRZ != null && rowsGZRZ.Length > 0)
            {
                bool showYQX = cbShowYQX.IsChecked.Value;

                //遍历所有工作计划
                foreach (DataRow row in rowsGZRZ)
                {
                    if (!showYQX)
                    {
                        string zt = row.GetString("ZT");
                        if (zt == "已取消") continue;
                    }

                    ScheduleItemUC uc = new ScheduleItemUC(this, row, _workplanIndex++);
                    this.panelWorkplans.Children.Add(uc);
                }
            }

            //重新加载年统计
            if (dt.Year != _tjYear)
            {
                _tjYear = dt.Year;
                LoadTJYear();
            }
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            Parts.FileManagerUC uc = new Parts.FileManagerUC();
            Components.PopWindow win = new Components.PopWindow(uc, "选择文件");
            uc._ParentWindow = win;
            win.CallBack_Event += Win_CallBack_Event;
            win.Show();
        }

        /// <summary>
        /// 选择文件回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void Win_CallBack_Event(Components.PopWindow win, object param)
        {
            string path = (param as AppCode.Models.FileItemInfo).FilePath;
            path = AppGlobal.UploadFile(path);

            this.lblWorkAttachment.Tag = path;
            this.lblWorkAttachment.Text = System.IO.Path.GetFileName(path);
            win.Close();
        }

        /// <summary>
        /// 添加工作事项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddWorkplan_Click(object sender, RoutedEventArgs e)
        {
            this.borderNewWork.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 取消工作事项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.borderNewWork.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 保存工作事项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string content = this.txtContent.Text.Trim();
            string attachment = this.lblWorkAttachment.Tag == null ? "" : this.lblWorkAttachment.Tag.ToString();

            if (string.IsNullOrWhiteSpace(content))
            {
                AppAlert.FormTips(gridMain, "请输入工作事项内容！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_Workplans",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("GZNR", content),
                            new KeyValue("FJ", attachment),
                            new KeyValue("ZT", "未开始"),
                            new KeyValue("JHWCRQ", _chooseDate.ToString("yyyy-MM-dd HH:mm:ss")),
                            new KeyValue("ZXRID", AppGlobal.UserInfo.UserId),
                            new KeyValue("ZXR", AppGlobal.UserInfo.UserName),
                            new KeyValue("CreateUserId", AppGlobal.UserInfo.UserId),
                            new KeyValue("CreateDate", DateTime.Now),
                        }
                    };

                    //插入
                    long id = SQLiteDao.Insert(param);

                    if (id > 0) SQLiteDao.ExecuteSQL("update [Sys_Workplans] set [DataIndex]=[Id] where [Id]=" + id);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (id > 0)
                        {
                            //清空
                            this.txtContent.Text = "";
                            this.lblWorkAttachment.Tag = null;
                            this.lblWorkAttachment.Text = "";
                            this.borderNewWork.Visibility = Visibility.Collapsed;

                            //加载计划统计
                            LoadTJYear();

                            if (_dtGZJH == null)
                            {
                                //加载工作计划
                                LoadWorkplans();
                                return null;
                            }

                            //新计划
                            DataRow rowRZ = _dtGZJH.NewRow();
                            rowRZ["Id"] = id;
                            rowRZ["GZNR"] = content;
                            rowRZ["FJ"] = attachment;
                            rowRZ["ZT"] = "未完成";
                            rowRZ["JHWCRQ"] = _chooseDate.ToString("yyyy-MM-dd HH:mm:ss");
                            rowRZ["SJWCRQ"] = System.DBNull.Value;
                            rowRZ["ZXRID"] = AppGlobal.UserInfo.UserId;
                            rowRZ["ZXR"] = AppGlobal.UserInfo.UserName;
                            rowRZ["CreateDate"] = DateTime.Now;

                            //添加到工作计划
                            _dtGZJH.Rows.Add(rowRZ);

                            //添加工作项
                            ScheduleItemUC uc = new ScheduleItemUC(this, rowRZ, _workplanIndex++);
                            this.panelWorkplans.Children.Insert(0, uc);

                            //有新事项
                            ucCalendar.SetDayHasNote(_chooseDate, true);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "添加失败！", AppCode.Enums.FormTipsType.Info);
                        }
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "添加失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }

                HideLoading();
            });
            thread.Start();
        }

        /// <summary>
        /// 移除工作项
        /// </summary>
        /// <param name="scheduleItemUC"></param>
        public void RemoveItem(ScheduleItemUC uc)
        {
            if (cbShowYQX.IsChecked.Value)
            {
                uc.SetCancel();
                return;
            }

            this.panelWorkplans.Children.Remove(uc);
        }

        /// <summary>
        /// 今天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            SetChooseDate(DateTime.Now);
        }

        /// <summary>
        /// 下一月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = ucCalendar.ChooseDate.AddMonths(1);
            SetChooseDate(dt);
        }

        /// <summary>
        /// 上一月
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrevMonth_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = ucCalendar.ChooseDate.AddMonths(-1);
            SetChooseDate(dt);
        }

        /// <summary>
        /// 加载工作计划
        /// </summary>
        private void LoadWorkplans()
        {
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    DateTime dtBegin = new DateTime(_chooseDate.Year, _chooseDate.Month, 1);
                    DateTime dtEnd = dtBegin.AddMonths(1);

                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_Workplans",
                        Wheres = new List<Where>()
                        {
                            new Where("JHWCRQ", dtBegin, WhereType.大于等于),
                            new Where("JHWCRQ", dtEnd, WhereType.小于等于),
                            new Where("ZXRID", AppGlobal.UserInfo.UserId)
                        }
                    };

                    //查询工作计划
                    _dtGZJH = SQLiteDao.GetTable(param);

                    //初始日历
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        ucCalendar._dtGZJH = _dtGZJH;
                        ucCalendar.RenewCalendarNodes();

                        //选择日期
                        UcCalendar_ChooseDate_Event(_chooseDate);
                        return null;
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHandler.AddException(ex, "加载工作计划异常...");
                }

                //隐藏等待
                HideLoading();
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 设置选择日期
        /// </summary>
        /// <param name="dt"></param>
        private void SetChooseDate(DateTime dt)
        {
            this.lblYear.Text = dt.Year + "年";
            this.lblMonth.Text = dt.Month + "月";

            //当前日期
            _chooseDate = dt;

            //初始日历
            ucCalendar.InitCalendar(dt);

            //工作计划
            LoadWorkplans();
        }

        /// <summary>
        /// 设置日历节点状态
        /// </summary>
        /// <param name="state"></param>
        public void SetCalendarNodeState(AppCode.Enums.WorkPlanProjectDetailState state, DataRow rowPlan)
        {
            LoadTJYear();

            if (state == AppCode.Enums.WorkPlanProjectDetailState.已取消)
            {
                //移除
                rowPlan.Table.Rows.Remove(rowPlan);
            }

            ucCalendar.RenewDayWorkplanState(_chooseDate);
        }
    }
}
