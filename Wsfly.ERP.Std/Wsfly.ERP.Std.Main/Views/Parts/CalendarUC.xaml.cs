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

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// CalendarUC.xaml 的交互逻辑
    /// </summary>
    public partial class CalendarUC : UserControl
    {
        /// <summary>
        /// 年
        /// </summary>
        public int Year { get { return ChooseDate.Year; } }
        /// <summary>
        /// 月
        /// </summary>
        public int Month { get { return ChooseDate.Month; } }
        /// <summary>
        /// 日
        /// </summary>
        public int Day { get { return ChooseDate.Day; } }
        /// <summary>
        /// 选择日期
        /// </summary>
        public DateTime ChooseDate { get; set; }

        /// <summary>
        /// 日期列表
        /// </summary>
        List<CalendarDayUC> CalendarDays = new List<CalendarDayUC>();

        /// <summary>
        /// 选择日期委托
        /// </summary>
        /// <param name="dt"></param>
        public delegate void ChooseDate_Delegate(DateTime dt);
        /// <summary>
        /// 选择日期事件
        /// </summary>
        public event ChooseDate_Delegate ChooseDate_Event;

        /// <summary>
        /// 项目管理版本
        /// </summary>
        public string _projectVersion = "V1";
        /// <summary>
        /// 是否工作日志
        /// </summary>
        public bool _isGZRZ = false;
        /// <summary>
        /// 计划完成时间列名
        /// </summary>
        string _jhwcsjlm = "PlanCompletionDate";
        /// <summary>
        /// 状态列名
        /// </summary>
        string _ztlm = "State";

        /// <summary>
        /// 项目任务
        /// </summary>
        public DataTable _dtXMRW = null;
        /// <summary>
        /// 工作日志
        /// </summary>
        public DataTable _dtGZJH = null;

        /// <summary>
        /// 初始
        /// </summary>
        public CalendarUC()
        {
            //构造
            InitializeComponent();

            //初始日历
            InitCalendar(DateTime.Now);

            this.Loaded += CalendarUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalendarUC_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// 初始日历
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        public void InitCalendar(DateTime initDate)
        {
            if (this.gridCalendar.Children.Count > 7)
            {
                //清空子项
                this.gridCalendar.Children.RemoveRange(7, this.gridCalendar.Children.Count - 7);
            }

            //当前选择日期
            ChooseDate = initDate;
            CalendarDays.Clear();

            int year = initDate.Year;
            int month = initDate.Month;
            int day = initDate.Day;

            DateTime prevMonth = new DateTime(year, month, 1).AddDays(-1);
            DateTime currentMonth = new DateTime(year, month, 1);
            DateTime nextMonth = new DateTime(year, month, 1).AddMonths(2).AddDays(-1);

            int prevMonthDays = prevMonth.Day;
            int currentMonthDays = currentMonth.AddMonths(1).AddDays(-1).Day;
            int nextMonthDays = nextMonth.Day;

            int index = 0;
            DateTime[] dts = new DateTime[42];

            //本月第一天是星期几
            int dayOfCurrentMonthFirst = (int)new DateTime(year, month, 1).DayOfWeek;
            //上月
            for (int i = dayOfCurrentMonthFirst - 1; i >= 0; i--)
            {
                dts[index] = prevMonth.AddDays(-i);
                index++;
            }
            //本月
            for (int i = 0; i < currentMonthDays; i++)
            {
                dts[index] = currentMonth.AddDays(i);
                index++;
            }
            //下月
            for (int i = 0; i < (42 - dayOfCurrentMonthFirst - currentMonthDays); i++)
            {
                dts[index] = currentMonth.AddMonths(1).AddDays(i);
                index++;
            }

            index = 0;
            //行
            for (int i = 1; i < 7; i++)
            {
                //列
                for (int j = 0; j < 7; j++)
                {
                    //日期
                    DateTime dt = dts[index];

                    //是否在选择的本月
                    bool isNotCurrentMonth = true;
                    if (dt.Year == year && dt.Month == month) isNotCurrentMonth = false;

                    //创建控件
                    CalendarDayUC uc = new CalendarDayUC(this, dt, isNotCurrentMonth);
                    Grid.SetRow(uc, i);
                    Grid.SetColumn(uc, j);

                    //当前选中日期
                    if (ChooseDate.Date == dt.Date)
                    {
                        uc.SetDayChoosed();
                    }

                    this.gridCalendar.Children.Add(uc);
                    CalendarDays.Add(uc);

                    //索引
                    index++;
                }
            }
        }
        /// <summary>
        /// 清空日历节点
        /// </summary>
        public void ClearCalendarNodes()
        {
            foreach (CalendarDayUC uc in CalendarDays)
            {
                uc.HasNotes(false, false);
            }
        }
        /// <summary>
        /// 刷新日历节点
        /// </summary>
        public void RenewCalendarNodes()
        {
            foreach (CalendarDayUC uc in CalendarDays)
            {
                //日期
                DateTime dt = uc.DTDay;

                //是否有项目任务
                if (_dtXMRW != null && _dtXMRW.Rows.Count > 0)
                {
                    if (_projectVersion.Equals("V2"))
                    {
                        //项目任务
                        _jhwcsjlm = "JSSJ";
                        _ztlm = "ZT";
                    }

                    DataRow[] drs = _dtXMRW.Select("[" + _jhwcsjlm + "]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "'");
                    if (drs != null && drs.Length > 0)
                    {
                        string select = "[" + _jhwcsjlm + "]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "' and [" + _ztlm + "]='未完成'";
                        if (_projectVersion.Equals("V2"))
                        {
                            select = "[" + _jhwcsjlm + "]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "' and ([" + _ztlm + "]='未开始' or [" + _ztlm + "]='已开始')";
                        }

                        //未完成的任务
                        DataRow[] notOverRows = _dtXMRW.Select(select);
                        //是否有未完成的
                        bool isOver = (notOverRows != null && notOverRows.Length > 0) ? false : true;
                        //标记有任务
                        uc.HasNotes(true, isOver);
                    }
                }

                //是否有工作日志
                if (_dtGZJH != null && _dtGZJH.Rows.Count > 0)
                {
                    _jhwcsjlm = "JHWCRQ";
                    _ztlm = "ZT";

                    DataRow[] drs = _dtGZJH.Select("[" + _jhwcsjlm + "]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "'");
                    if (drs != null && drs.Length > 0)
                    {
                        string select = select = "[" + _jhwcsjlm + "]>='" + dt.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dt.ToString("yyyy-MM-dd 23:59:59") + "' and ([" + _ztlm + "]='未开始' or [" + _ztlm + "]='已开始')"; ;                        
                        //未完成的日志
                        DataRow[] notOverRows = _dtGZJH.Select(select);
                        //是否有未完成的
                        bool isOver = (notOverRows != null && notOverRows.Length > 0) ? false : true;
                        //标记有日志
                        uc.HasNotes(true, isOver);
                    }
                }
            }
        }

        /// <summary>
        /// 设置当前日期选中
        /// </summary>
        /// <param name="dtDay"></param>
        internal void ChooseDay(DateTime dtDay)
        {
            foreach (CalendarDayUC uc in CalendarDays)
            {
                if (uc.IsChoosed && uc.DTDay != dtDay)
                {
                    uc.ClearChoosed();
                }
            }

            try
            {
                //选择日期
                if (ChooseDate_Event != null)
                {
                    //回调选择日期
                    ChooseDate_Event(dtDay);
                }
            }
            catch { }

            //当前选择日期
            ChooseDate = dtDay;
        }
        /// <summary>
        /// 获取日期天UC
        /// </summary>
        /// <param name="dtDay"></param>
        /// <returns></returns>
        private CalendarDayUC GetDayUC(DateTime dtDay)
        {
            foreach (CalendarDayUC uc in CalendarDays)
            {
                if (uc.DTDay.Date == dtDay.Date)
                {
                    return uc;
                }
            }

            return null;
        }
        /// <summary>
        /// 设置日期是否有工作项
        /// </summary>
        /// <param name="dtDay"></param>
        /// <param name="hasNote"></param>
        public void SetDayHasNote(DateTime dtDay, bool hasNote = true)
        {
            CalendarDayUC uc = GetDayUC(dtDay);
            if (uc == null) return;

            uc.HasNotes(hasNote, false);
        }

        /// <summary>
        /// 刷新日期的任务状态
        /// </summary>
        /// <param name="dtDay"></param>
        public void RenewDayWorkplanState(DateTime dtDay)
        {
            CalendarDayUC uc = GetDayUC(dtDay);
            if (uc == null) return;

            //是否有项目任务
            if (_dtXMRW != null && _dtXMRW.Rows.Count > 0)
            {
                if (_projectVersion.Equals("V2"))
                {
                    //项目任务
                    _jhwcsjlm = "JSSJ";
                    _ztlm = "ZT";
                }

                DataRow[] drs = _dtXMRW.Select("[" + _jhwcsjlm + "]>='" + dtDay.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dtDay.ToString("yyyy-MM-dd 23:59:59") + "'");
                if (drs != null && drs.Length > 0)
                {
                    //搜索条件
                    string select = "[" + _jhwcsjlm + "]>='" + dtDay.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dtDay.ToString("yyyy-MM-dd 23:59:59") + "' and [" + _ztlm + "]='未完成'";
                    if (_projectVersion.Equals("V2"))
                    {
                        select = "[" + _jhwcsjlm + "]>='" + dtDay.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dtDay.ToString("yyyy-MM-dd 23:59:59") + "' and ([" + _ztlm + "]='未开始' or [" + _ztlm + "]='已开始')";
                    }

                    //未完成的任务
                    DataRow[] notOverRows = _dtXMRW.Select(select);
                    //是否有未完成的
                    bool isOver = (notOverRows != null && notOverRows.Length > 0) ? false : true;
                    //标记有任务
                    uc.HasNotes(true, isOver);
                }
            }

            //是否有工作日志
            if (_dtGZJH != null && _dtGZJH.Rows.Count > 0)
            {
                _jhwcsjlm = "JHWCRQ";
                _ztlm = "ZT";

                DataRow[] drs = _dtGZJH.Select("[" + _jhwcsjlm + "]>='" + dtDay.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dtDay.ToString("yyyy-MM-dd 23:59:59") + "'");
                if (drs != null && drs.Length > 0)
                {
                    string select = select = "[" + _jhwcsjlm + "]>='" + dtDay.ToString("yyyy-MM-dd 00:00:00") + "' and [" + _jhwcsjlm + "]<='" + dtDay.ToString("yyyy-MM-dd 23:59:59") + "' and ([" + _ztlm + "]='未开始' or [" + _ztlm + "]='已开始')";
                    //未完成的日志
                    DataRow[] notOverRows = _dtGZJH.Select(select);
                    //是否有未完成的
                    bool isOver = (notOverRows != null && notOverRows.Length > 0) ? false : true;
                    //标记有日志
                    uc.HasNotes(true, isOver);
                }
            }
        }
    }
}
