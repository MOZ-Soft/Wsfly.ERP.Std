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
using Wsfly.ERP.Std.AppCode.Extensions;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// SubNavsUC.xaml 的交互逻辑
    /// </summary>
    public partial class SubNavsUC : BaseUserControl
    {
        /// <summary>
        /// 模块编号
        /// </summary>
        private long _moduleId = 0;
        /// <summary>
        /// 是否有流程
        /// </summary>
        bool _hasFLow = false;
        /// <summary>
        /// 默认导航图标宽度
        /// </summary>
        int _navItemWidth = 80;
        /// <summary>
        /// 默认导航图标高度
        /// </summary>
        int _navItemHeight = 100;
        /// <summary>
        /// 默认导航放大倍数
        /// </summary>
        double _navIconZoom = 1;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="parentId"></param>
        public SubNavsUC(long moduleId)
        {
            _moduleId = moduleId;

            //图标放大
            if (AppGlobal.LocalConfig.NavIconZoom > 1 && AppGlobal.LocalConfig.NavIconZoom <= 3)
            {
                _navIconZoom = AppGlobal.LocalConfig.NavIconZoom;
                _navItemWidth = Convert.ToInt32(_navItemWidth * _navIconZoom);
                _navItemHeight = Convert.ToInt32(_navItemHeight * _navIconZoom);
            }
            
            InitializeComponent();

            this.Loaded += SubNavsUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubNavsUC_Loaded(object sender, RoutedEventArgs e)
        {
            //初始页面大小
            InitUCSize();

            this.canvasNavs.Width = this.Width - 20;
            this.canvasNavs.HorizontalAlignment = HorizontalAlignment.Left;
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;

            //加载菜单
            LoadMenus();
        }
        
        /// <summary>
        /// 加载菜单
        /// </summary>
        public void LoadMenus()
        {
            //清空菜单
            this.canvasNavs.Children.Clear();

            //子菜单
            DataTable dtSubMenus = AppGlobal.SubMenus;

            //样式
            Style btnStyle = this.FindResource("btnSubMenu") as Style;

            double cWidth = double.IsNaN(canvasNavs.Width) ? canvasNavs.ActualWidth : canvasNavs.Width;
            int itemColnumMaxCount = Convert.ToInt32((cWidth) / _navItemWidth) - 2;
            int itemColnumIndex = 0;
            int itemRowIndex = 0;

            bool hasFlow = false;
            bool hasUserModulePoint = false;

            //独立模块
            List<SubNavItemUC> ucIndependentMenus = new List<SubNavItemUC>();
            //有流程的模块
            List<SubNavItemUC> ucFlowMenus = new List<SubNavItemUC>();

            //线程加载图标
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //所有子模块
                DataRow[] rowSubMenus = dtSubMenus.Select("[ParentId]=" + _moduleId);
                DataRow[] rowSubMenuHasChildren = dtSubMenus.Select("[ParentId]=" + _moduleId + " and [ParentFlowId]>0");
                foreach (DataRow row in rowSubMenuHasChildren)
                {
                    long parentFlowId = row.GetLong("ParentFlowId");
                    var parentFlowRows = dtSubMenus.Select("[ParentId]=" + _moduleId + " and [Id]=" + parentFlowId);
                    if (parentFlowRows != null && parentFlowRows.Length > 0)
                    {
                        _hasFLow = true;
                        break;
                    }
                }

                foreach (DataRow row in rowSubMenus)
                {
                    long id = row.GetId();
                    long pid = row.GetLong("ParentId");

                    long parentFlowId = row.GetLong("ParentFlowId");

                    //是否PC菜单
                    if (!row.GetBool("IsPC")) continue;

                    //是否有权限
                    bool hasAuthority = AppGlobal.HasAuthority(id);
                    if (!_hasFLow && !hasAuthority) continue;

                    //画流程图所需参数
                    var parentFlowRows = dtSubMenus.Select("[ParentId]=" + _moduleId + " and [Id]=" + parentFlowId);
                    var subFlowRows = dtSubMenus.Select("[ParentId]=" + _moduleId + " and [ParentFlowId]=" + id);

                    bool hasParentFlow = parentFlowRows == null || parentFlowRows.Length < 1 ? false : true;
                    bool hasSubFlow = subFlowRows == null || subFlowRows.Length < 1 ? false : true;

                    double x = itemColnumIndex * _navItemWidth;
                    double y = itemRowIndex * _navItemHeight;
                    
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        //菜单按钮
                        SubNavItemUC btn = new SubNavItemUC(row, _navIconZoom);
                        btn.Width = _navItemWidth;
                        btn.Height = _navItemHeight;
                        btn.Name = "menu_" + id;
                        btn.HasSubFlow = hasSubFlow;
                        btn.IsRoot = !hasParentFlow;
                        btn.HasAuthority = hasAuthority;

                        if (hasAuthority)
                        {
                            btn.PreviewMouseUp += Btn_MouseUp;

                            btn.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            btn.Opacity = 0.5;
                            btn.Cursor = Cursors.No;
                        }

                        Canvas.SetLeft(btn, x);
                        Canvas.SetTop(btn, y);

                        //添加到UI界面
                        this.canvasNavs.Children.Add(btn);

                        if (hasParentFlow || hasSubFlow)
                        {
                            //有流程
                            ucFlowMenus.Add(btn);
                            hasFlow = true;
                        }
                        else
                        {
                            //无程序
                            ucIndependentMenus.Add(btn);
                        }
                    }));

                    itemColnumIndex++;
                    if (itemColnumIndex > itemColnumMaxCount)
                    {
                        itemColnumIndex = 0;
                        itemRowIndex++;
                    }
                }

                //处理流程
                if (hasFlow && !hasUserModulePoint)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (UIElement ele in this.canvasNavs.Children)
                        {
                            ele.Visibility = Visibility.Collapsed;
                        }
                    }));

                    //布置独立菜单图标
                    itemColnumIndex = 0;
                    itemRowIndex = 0;


                    if (ucIndependentMenus != null && ucIndependentMenus.Count > 0)
                    {
                        foreach (SubNavItemUC btn in ucIndependentMenus)
                        {
                            if (!btn.HasAuthority) continue;

                            double x = itemColnumIndex * _navItemWidth;
                            double y = itemRowIndex * _navItemHeight;

                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                Canvas.SetLeft(btn, x);
                                Canvas.SetTop(btn, y);

                                btn.Visibility = Visibility.Visible;
                            }));

                            itemColnumIndex++;
                            if (itemColnumIndex > itemColnumMaxCount)
                            {
                                itemColnumIndex = 0;
                                itemRowIndex++;
                            }
                        }

                        itemRowIndex++;
                    }


                    int orgItemRowIndex = 0;

                    //布置流程菜单图标
                    foreach (SubNavItemUC btn in ucFlowMenus)
                    {
                        if (!btn.IsRoot) continue;

                        double x = 0 * _navItemWidth;
                        double y = itemRowIndex * _navItemHeight;

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            Canvas.SetLeft(btn, x);
                            Canvas.SetTop(btn, y);
                        }));

                        orgItemRowIndex = itemRowIndex;

                        //下级流程
                        BuildSubFlow(ref itemRowIndex, 2, btn, ucFlowMenus);

                        itemRowIndex++;

                        //没有下级菜单
                        if (btn.SubNavs.Count <= 0)
                        {
                            //没有权限
                            if (!btn.HasAuthority)
                            {
                                //配置有下级菜单 但下级菜单没有权限且本功能也没权限时
                                if (ucFlowMenus.Exists(p => p.__ParentFlowId == btn.__ModuleId))
                                {
                                    itemRowIndex = orgItemRowIndex;
                                }
                                else
                                {
                                    itemRowIndex--;
                                }
                                continue;
                            }
                        }
                        else
                        {
                            //有下级菜单
                            if (!btn.HasAuthority)
                            {
                                //当前菜单没有权限
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    btn.IsEnabled = false;
                                    btn.Opacity = 0.2;
                                    btn.Cursor = Cursors.No;
                                    btn.Visibility = Visibility.Visible;
                                }));
                                continue;
                            }
                        }


                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            btn.Visibility = Visibility.Visible;
                        }));
                    }
                }
            });
            thread.Start();
        }
        /// <summary>
        /// 生成下级流程
        /// </summary>
        /// <param name="btnParent"></param>
        private void BuildSubFlow(ref int rowIndex, int colIndex, SubNavItemUC btnParent, List<SubNavItemUC> ucFlowMenus)
        {
            List<SubNavItemUC> subFlowBtns = ucFlowMenus.FindAll(p => p.__ParentFlowId == btnParent.__ModuleId);
            if (subFlowBtns == null || subFlowBtns.Count < 1) return;

            foreach (SubNavItemUC btn in subFlowBtns)
            {
                double x = colIndex * _navItemWidth;
                double y = rowIndex * _navItemHeight;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    Canvas.SetLeft(btn, x);
                    Canvas.SetTop(btn, y);
                }));

                //下级流程
                BuildSubFlow(ref rowIndex, colIndex + 2, btn, ucFlowMenus);

                rowIndex++;

                //没有下级菜单
                if (btn.SubNavs.Count <= 0)
                {
                    //没有权限
                    if (!btn.HasAuthority) continue;
                }

                //添加到子菜单项
                btnParent.SubNavs.Add(btn);

                this.Dispatcher.Invoke(new Action(() =>
                {
                    btn.Visibility = Visibility.Visible;

                    //线条
                    var line = new Line();
                    line.Stroke = Brushes.LightGray;
                    line.StrokeThickness = 1;
                    line.X1 = Canvas.GetLeft(btnParent) + _navItemWidth;
                    line.Y1 = Canvas.GetTop(btnParent) + (_navItemHeight / 2);
                    line.X2 = x;
                    line.Y2 = y + (_navItemHeight / 2);
                    this.canvasNavs.Children.Add(line);
                }));
            }

            rowIndex--;
        }
        /// <summary>
        /// 鼠标放开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //点击的按钮
            SubNavItemUC btn = sender as SubNavItemUC;

            try
            {
                //释放鼠标捕获
                btn.ReleaseMouseCapture();
            }
            catch { }

            //打开模块
            OpenModule(btn);
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.canvasNavs.Width = this.Width - 20;
        }

        /// <summary>
        /// 点击菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenModule((sender as SubNavItemUC));
        }
        /// <summary>
        /// 进入菜单
        /// </summary>
        /// <param name="btn"></param>
        private void OpenModule(SubNavItemUC btn)
        {
            if (btn.MenuInfo == null) return;

            //点击菜单
            AppData.MainWindow.ClickMenu(btn.MenuInfo);
        }
    }
}
