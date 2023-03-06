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
using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ScheduleItemUC.xaml 的交互逻辑
    /// </summary>
    public partial class ScheduleItemUC : BaseUserControl
    {
        int _index = 1;
        string ct1 = "";
        string ct2 = "";
        bool _isExplan = false;
        DataRow _row = null;
        ScheduleUC _ucParent = null;

        /// <summary>
        /// 初始
        /// </summary>
        /// <param name="row"></param>
        public ScheduleItemUC(ScheduleUC uc, DataRow row, int index)
        {
            _index = index;
            _row = row;
            _ucParent = uc;

            InitializeComponent();

            this.Loaded += ScheduleItemUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleItemUC_Loaded(object sender, RoutedEventArgs e)
        {
            InitCT(_row, _index);

            this.lblSuccess.MouseLeftButtonDown += LblSuccess_MouseLeftButtonDown;
            this.lblCancel.MouseLeftButtonDown += LblCancel_MouseLeftButtonDown;
            this.MouseEnter += ScheduleItemUC_MouseEnter;
            this.MouseLeave += ScheduleItemUC_MouseLeave;
        }

        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleItemUC_MouseEnter(object sender, MouseEventArgs e)
        {
            this.borderScheduleItem.BorderBrush = Brushes.LightGray;
        }
        /// <summary>
        /// 鼠标离开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleItemUC_MouseLeave(object sender, MouseEventArgs e)
        {
            this.borderScheduleItem.BorderBrush = Brushes.White;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblCancel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool? flagCancel = AppAlert.Alert("是否确定取消工作计划？", "请确认", AppCode.Enums.AlertWindowButton.OkCancel);
            if (flagCancel.HasValue && flagCancel.Value)
            {
                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        SQLParam param = new SQLParam()
                        {
                            TableName = "Sys_Workplans",
                            Id = DataType.Long(_row["Id"].ToString(), 0),
                            OpreateCells = new List<KeyValue>()
                            {
                                new KeyValue("ZT", "已取消"),
                                new KeyValue("SJWCRQ", null)
                            }
                        };

                        //更新状态
                        bool flag = SQLiteDao.Update(param);

                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            if (flag)
                            {
                                _row["ZT"] = "已取消";
                                _row["SJWCRQ"] = System.DBNull.Value;

                                _ucParent.SetCalendarNodeState(AppCode.Enums.WorkPlanProjectDetailState.已取消, _row);
                                _ucParent.RemoveItem(this);
                            }
                            else
                            {
                                AppAlert.FormTips(_ucParent.gridMain, "取消失败！", AppCode.Enums.FormTipsType.Info);
                            }
                            return null;
                        }));

                    }
                    catch { }
                });
                thread.Start();
            }
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblSuccess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_Workplans",
                        Id = DataType.Long(_row["Id"].ToString(), 0),
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("ZT", "已完成"),
                            new KeyValue("SJWCRQ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                    };

                    //更新状态
                    bool flag = SQLiteDao.Update(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            //成功
                            _row["ZT"] = "已完成";
                            _row["SJWCRQ"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            this.lblActualCompletionDate.Visibility = Visibility.Visible;

                            this.lblSuccess.Visibility = Visibility.Collapsed;
                            this.lblCancel.Visibility = Visibility.Collapsed;
                            this.canvasSuccess.Visibility = Visibility.Visible;

                            DateTime planCompletionDate = DataType.DateTime(_row["JHWCRQ"], DateTime.Now);

                            this.lblActualCompletionDate.Text = "实际：" + DateTime.Now.ToString("yyyy-MM-dd");
                            string totalDays = (DateTime.Now - planCompletionDate).TotalDays.ToString();
                            int days = DataType.Int(totalDays.Substring(0, totalDays.IndexOf('.')), 0);

                            if (days > 0)
                            {
                                this.lblMoreDays.Visibility = Visibility.Visible;
                                this.lblMoreDays.Text = "超" + days + "天";
                                this.lblMoreDays.Foreground = Brushes.Red;
                            }

                            _ucParent.SetCalendarNodeState(AppCode.Enums.WorkPlanProjectDetailState.已完成, _row);
                        }
                        else
                        {
                            AppAlert.FormTips(_ucParent.gridMain, "标记完成失败！", AppCode.Enums.FormTipsType.Info);
                        }
                        return null;
                    }));

                }
                catch { }
            });
            thread.Start();
        }

        /// <summary>
        /// 鼠标点击展开或收起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblWorkplan_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isExplan)
            {
                this.lblWorkplan.Text = _index + "." + ct1;
            }
            else
            {
                this.lblWorkplan.Text = _index + "." + ct2;
            }

            _isExplan = !_isExplan;
        }

        /// <summary>
        /// 初始内容
        /// </summary>
        private void InitCT(DataRow row, int index)
        {
            //是否显示
            this.lblActualCompletionDate.Visibility = Visibility.Collapsed;
            this.lblMoreDays.Visibility = Visibility.Collapsed;
            this.canvasSuccess.Visibility = Visibility.Collapsed;

            ct1 = "";

            //缩略内容
            ct1 = Core.Handler.StringHandler.RemoveAllHtmlTag(row["GZNR"].ToString());
            ct2 = ct1;
            ct1 = Core.Handler.StringHandler.SubStringsByBytes(ct1, 50);

            string fj = row["FJ"].ToString();
            string zt = row["ZT"].ToString();

            if (!ct1.Equals(ct2))
            {
                this.lblWorkplan.MouseLeftButtonDown += LblWorkplan_MouseLeftButtonDown;
                this.lblWorkplan.ToolTip = "展开/收起";
            }

            //工作附件
            this.panelAttachment.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrWhiteSpace(fj))
            {
                this.lblAttachment.Tag = fj;
                this.lblAttachment.Text = System.IO.Path.GetFileName(fj);
                this.panelAttachment.Visibility = Visibility.Visible;
                this.lblAttachment.MouseLeftButtonDown += LblAttachment_MouseLeftButtonDown;
            }

            //计划日期和完成日期
            DateTime planCompletionDate = DataType.DateTime(row["JHWCRQ"], DateTime.Now);
            DateTime actualCompletionDate = DataType.DateTime(row["SJWCRQ"], DateTime.Now);

            //计划完成时间的结束
            planCompletionDate = new DateTime(planCompletionDate.Year, planCompletionDate.Month, planCompletionDate.Day, 23, 59, 59);

            //工作内容
            this.lblWorkplan.Text = index + "." + ct1;
            this.lblPlanCompletionDate.Text = "计划：" + planCompletionDate.ToString("yyyy-MM-dd");

            double days = 0;
            if (zt == "已取消")
            {
                this.lblSuccess.Visibility = Visibility.Collapsed;
                this.lblCancel.Visibility = Visibility.Collapsed;
                this.canvasSuccess.Visibility = Visibility.Collapsed;
                this.lblPlanCancel.Visibility = Visibility.Visible;

                this.lblWorkplan.Foreground = Brushes.Gray;
            }
            else if (row["SJWCRQ"] != System.DBNull.Value && row["ZT"].ToString() == "已完成")
            {
                //有实际完成日期
                this.lblActualCompletionDate.Visibility = Visibility.Visible;

                this.lblSuccess.Visibility = Visibility.Collapsed;
                this.lblCancel.Visibility = Visibility.Collapsed;
                this.lblPlanCancel.Visibility = Visibility.Collapsed;

                this.canvasSuccess.Visibility = Visibility.Visible;

                this.lblActualCompletionDate.Text = "实际：" + actualCompletionDate.ToString("yyyy-MM-dd");
                days = (actualCompletionDate - planCompletionDate).TotalDays;
            }
            else if (DateTime.Now > planCompletionDate)
            {
                //当前日期大于计划日期
                days = (DateTime.Now - planCompletionDate).TotalDays;
            }

            if (days > 0)
            {
                //不够一天按一天算
                if (days < 1) days = 1;

                this.lblMoreDays.Visibility = Visibility.Visible;
                this.lblMoreDays.Text = "超" + Convert.ToInt32(days) + "天";
                this.lblMoreDays.Foreground = Brushes.Red;
            }
        }
        /// <summary>
        /// 点击附件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblAttachment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string path = (sender as TextBlock).Tag.ToString();
            path = AppGlobal.GetUploadFilePath(path);
            System.Diagnostics.Process.Start(path);
        }
        /// <summary>
        /// 设置已取消
        /// </summary>
        public void SetCancel()
        {
            this.lblSuccess.Visibility = Visibility.Collapsed;
            this.lblCancel.Visibility = Visibility.Collapsed;
            this.canvasSuccess.Visibility = Visibility.Collapsed;
            this.lblPlanCancel.Visibility = Visibility.Visible;

            this.lblWorkplan.Foreground = Brushes.Gray;
        }
    }
}
