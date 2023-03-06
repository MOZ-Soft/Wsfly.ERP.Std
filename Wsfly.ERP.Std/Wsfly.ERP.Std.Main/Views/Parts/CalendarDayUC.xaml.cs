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
    /// CalendarDayUC.xaml 的交互逻辑
    /// </summary>
    public partial class CalendarDayUC : UserControl
    {
        /// <summary>
        /// 当前日期
        /// </summary>
        public DateTime DTDay = DateTime.Now;
        /// <summary>
        /// 是否当前月份
        /// </summary>
        public bool IsNotCurrentMonth = false;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChoosed = false;

        /// <summary>
        /// 日历
        /// </summary>
        CalendarUC _calendarUC = null;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        public CalendarDayUC(CalendarUC calendarUC, DateTime dtDay, bool isNotCurrentMonth = false)
        {
            //当前日期
            DTDay = dtDay;
            IsNotCurrentMonth = isNotCurrentMonth;
            _calendarUC = calendarUC;

            //构造
            InitializeComponent();

            //日期
            this.lblDay.Text = dtDay.Day.ToString();
            this.borderDay.MouseEnter += BorderDay_MouseEnter;
            this.borderDay.MouseLeave += BorderDay_MouseLeave;
            this.borderDay.MouseLeftButtonDown += BorderDay_MouseLeftButtonDown;

            //设置标签颜色
            SetDayNormal();
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderDay_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsChoosed) return;
            this.borderDay.Background = AppGlobal.ColorToBrush("#88FFFFFF");
        }
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderDay_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsChoosed) return;
            this.borderDay.Background = AppGlobal.ColorToBrush("#AA0090ff");
        }
        /// <summary>
        /// 选择日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BorderDay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //设置当前日期选中
            SetDayChoosed();
        }

        /// <summary>
        /// 清除选择
        /// </summary>
        public void ClearChoosed()
        {
            IsChoosed = false;

            Style styleBorder = this.FindResource("borderCalendarDay") as Style;
            Style styleLabel = this.FindResource("lblCalendarDayText") as Style;

            this.borderDay.Background = AppGlobal.ColorToBrush("#88FFFFFF");

            this.borderDay.Style = styleBorder;
            this.lblDay.Style = styleLabel;

            //设置标签颜色
            SetDayNormal();
        }
        /// <summary>
        /// 设置当前日期正常
        /// </summary>
        private void SetDayNormal()
        {
            if (DTDay.Date == DateTime.Now.Date)
            {
                //今天
                this.lblDay.Foreground = Brushes.OrangeRed;
            }
            else if (IsNotCurrentMonth)
            {
                //其它月份
                this.lblDay.Foreground = Brushes.LightGray;
            }
            else
            {
                this.lblDay.Foreground = Brushes.Gray;
            }
        }
        /// <summary>
        /// 设置当前日期选中
        /// </summary>
        public void SetDayChoosed()
        {
            IsChoosed = true;
            this.lblDay.Foreground = Brushes.White;

            Style styleBorder = this.FindResource("borderCalendarDay_Current") as Style;
            Style styleLabel = this.FindResource("lblCalendarDayText_Current") as Style;

            this.borderDay.Style = styleBorder;
            this.lblDay.Style = styleLabel;

            //设置选中日期
            _calendarUC.ChooseDay(DTDay);
        }
        /// <summary>
        /// 是否有工作项
        /// </summary>
        /// <param name="hasNotes"></param>
        public void HasNotes(bool hasNotes, bool isOver = false)
        {
            if (hasNotes)
            {
                this.borderNotes.Visibility = Visibility.Visible;

                if (isOver)
                {
                    this.borderNotes.Background = Brushes.LightGray;
                }
                else
                {
                    if (DTDay.Date < DateTime.Now.Date)
                    {
                        this.borderNotes.Background = Brushes.Red;
                    }
                    else if (DTDay.Date == DateTime.Now.Date)
                    {
                        this.borderNotes.Background = Brushes.OrangeRed;
                    }
                    else
                    {
                        this.borderNotes.Background = Brushes.Green;
                    }
                }
            }
            else
            {
                this.borderNotes.Visibility = Visibility.Hidden;
            }
        }
    }
}
