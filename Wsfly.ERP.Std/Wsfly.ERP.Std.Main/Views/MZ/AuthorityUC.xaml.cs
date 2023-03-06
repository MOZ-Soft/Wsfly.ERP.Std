using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
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
using Wsfly.ERP.Std.AppCode.Models;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Service.Exts;

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// AuthorityUC.xaml 的交互逻辑
    /// </summary>
    public partial class AuthorityUC : BaseUserControl
    {
        /// <summary>
        /// 是否用户操作
        /// </summary>
        bool _isUserOpera = true;
        /// <summary>
        /// 所有角色
        /// </summary>
        DataTable _dtRoles = null;
        /// <summary>
        /// 所有角色权限
        /// </summary>
        DataTable _dtRoleAuthoritys = null;
        /// <summary>
        /// 当前用户编号
        /// </summary>
        long _currentRoleId = 0;

        public object _lockOperateDTRoleAuthoritys = new object();
        public object _lockOperateDTRoleAuthoritys2 = new object();

        /// <summary>
        /// 构造
        /// </summary>
        public AuthorityUC()
        {
            InitializeComponent();

            InitUCSize(-80, -80);

            this.Loaded += AuthorityUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AuthorityUC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoles();
            LoadModules();
            LoadRoleAuthoritys();

            this.btnSaveRole.Click += BtnSaveRole_Click;
            this.btnReloadRoles.Click += BtnReloadRoles_Click;
            this.btnReloadAuthoritys.Click += BtnReloadAuthoritys_Click;
            this.btnSaveRoleAuthoritys.Click += BtnSaveRoleAuthoritys_Click;

            this.listRoles.SelectionChanged += ListRoles_SelectionChanged;
        }

        /// <summary>
        /// 保存角色权限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveRoleAuthoritys_Click(object sender, RoutedEventArgs e)
        {
            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    bool flag = UserService.SetRoleAuthoritys(_dtRoleAuthoritys);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            //提示
                            AppAlert.FormTips(gridMain, "保存角色权限成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            //提示
                            AppAlert.FormTips(gridMain, "保存角色权限失败！", AppCode.Enums.FormTipsType.Info);
                        }

                        HideLoading();

                        return null;
                    }));
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "保存角色权限失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));

                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 重新加载权限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadAuthoritys_Click(object sender, RoutedEventArgs e)
        {
            LoadModules();
            LoadRoleAuthoritys();
        }

        /// <summary>
        /// 重新加载角色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadRoles_Click(object sender, RoutedEventArgs e)
        {
            LoadRoles();
        }

        /// <summary>
        /// 加载角色
        /// </summary>
        private void LoadRoles()
        {
            this.listRoles.ItemsSource = null;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    DataTable dt = SQLiteDao.GetTable("Sys_Roles");
                    if (dt == null || dt.Rows.Count <= 0) return;

                    //默认移除第一个角色
                    dt.Rows.RemoveAt(0);

                    //角色表
                    _dtRoles = dt;

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.listRoles.ItemsSource = dt.DefaultView;
                        return null;
                    }));
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "加载角色失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 加载模块
        /// </summary>
        private void LoadModules()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //模块
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_Modules",
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy() { CellName="Order" }
                        }
                    };
                    DataTable dtModules = SQLiteDao.GetTable(param);
                    if (dtModules == null || dtModules.Rows.Count <= 0) return;

                    //模块明细
                    param = new SQLParam()
                    {

                        TableName = "Sys_ModuleDetails",
                        OrderBys = new List<OrderBy>()
                        {
                            new OrderBy() { CellName="Order" }
                        }
                    };
                    DataTable dtModuleDetails = SQLiteDao.GetTable(param);

                    //所有操作
                    param = new SQLParam()
                    {

                        TableName = "Sys_Actions",
                    };
                    DataTable dtActions = SQLiteDao.GetTable(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //显示模块及操作
                        ShowModulesAndAuthoritys(dtModules, dtModuleDetails, dtActions);
                        return null;
                    }));
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "加载模块及操作失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 加载角色权限
        /// </summary>
        private void LoadRoleAuthoritys()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_RoleAuthoritys",
                    };
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null)
                    {
                        dt = new DataTable();

                        DataColumn colId = new DataColumn("Id", typeof(long));
                        DataColumn colRoleId = new DataColumn("RoleId", typeof(long));
                        DataColumn colModuleId = new DataColumn("ModuleId", typeof(long));
                        DataColumn colActionId = new DataColumn("ActionId", typeof(long));

                        dt.Columns.Add(colId);
                        dt.Columns.Add(colRoleId);
                        dt.Columns.Add(colModuleId);
                        dt.Columns.Add(colActionId);
                    }

                    lock (_lockOperateDTRoleAuthoritys)
                    {
                        _dtRoleAuthoritys = dt;
                    }
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        this.btnSaveRoleAuthoritys.Visibility = Visibility.Collapsed;
                        this.lblRoleAuthorityTips.Visibility = Visibility.Visible;
                        AppAlert.FormTips(gridMain, "加载角色权限失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 显示模块及操作
        /// </summary>
        /// <param name="dtModules"></param>
        /// <param name="dtModuleDetails"></param>
        /// <param name="dtActions"></param>
        private void ShowModulesAndAuthoritys(DataTable dtModules, DataTable dtModuleDetails, DataTable dtActions)
        {
            this.tvAuthoritys.Items.Clear();

            //循环所有模块
            foreach (DataRow rowModule in dtModules.Rows)
            {
                //模块编号
                long moduleId = DataType.Long(rowModule["Id"], 0);
                string moduleName = rowModule["ModuleName"].ToString();

                CheckBox cbModule = new CheckBox();
                cbModule.Content = moduleName;
                cbModule.Checked += CbModule_Checked;
                cbModule.Unchecked += CbModule_Unchecked;

                TreeViewItem moduleItem = new TreeViewItem();
                moduleItem.Header = cbModule;
                moduleItem.IsExpanded = true;
                moduleItem.Tag = rowModule;

                if (dtModuleDetails != null && dtModuleDetails.Rows.Count > 0)
                {
                    //所有子模块
                    foreach (DataRow rowModuleDetail in dtModuleDetails.Rows)
                    {
                        //模块明细编号
                        long parentId = DataType.Long(rowModuleDetail["ParentId"], -1);
                        long moduleDetailId = DataType.Long(rowModuleDetail["Id"], 0);
                        string moduleDetailName = rowModuleDetail["ModuleName"].ToString();

                        //非子级
                        if (parentId != moduleId) continue;

                        CheckBox cbModuleDetail = new CheckBox();
                        cbModuleDetail.Content = moduleDetailName;
                        cbModuleDetail.Checked += CbModuleDetail_Checked;
                        cbModuleDetail.Unchecked += CbModuleDetail_Unchecked;

                        TreeViewItem moduleDetailItem = new TreeViewItem();
                        moduleDetailItem.Header = cbModuleDetail;
                        moduleDetailItem.IsExpanded = false;
                        moduleDetailItem.Tag = rowModuleDetail;

                        if (dtActions != null && dtActions.Rows.Count > 0)
                        {
                            //所有操作
                            foreach (DataRow rowAction in dtActions.Rows)
                            {
                                string actionName = rowAction["ActionName"].ToString();

                                CheckBox cbModuleAction = new CheckBox();
                                cbModuleAction.Content = actionName;
                                cbModuleAction.Checked += CbModuleAction_Checked;
                                cbModuleAction.Unchecked += CbModuleAction_Unchecked;
                                cbModuleAction.Tag = new AuthorityItem()
                                {
                                    ModuleRow = rowModule,
                                    ModuleDetailRow = rowModuleDetail,
                                    ActionRow = rowAction,
                                };

                                TreeViewItem moduleActionItem = new TreeViewItem();
                                moduleActionItem.Header = cbModuleAction;
                                moduleActionItem.IsExpanded = true;
                                moduleActionItem.Tag = rowAction;

                                moduleDetailItem.Items.Add(moduleActionItem);
                            }
                        }

                        moduleItem.Items.Add(moduleDetailItem);
                    }
                }

                this.tvAuthoritys.Items.Add(moduleItem);
            }
        }

        /// <summary>
        /// 清空所有选择
        /// </summary>
        private void ClearAllChoose()
        {
            foreach (TreeViewItem item in this.tvAuthoritys.Items)
            {
                (item.Header as CheckBox).IsChecked = false;

                foreach (TreeViewItem subItem in item.Items)
                {
                    (subItem.Header as CheckBox).IsChecked = false;

                    foreach (TreeViewItem subItem2 in subItem.Items)
                    {
                        (subItem2.Header as CheckBox).IsChecked = false;
                    }
                }
            }
        }

        /// <summary>
        /// 选择角色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //当前选择的角色
            DataRowView row = this.listRoles.SelectedItem as DataRowView;
            if (row == null) return;
            //当前角色编号
            _currentRoleId = DataType.Long(row["Id"].ToString(), 0);

            _isUserOpera = false;

            //得到所有模块
            foreach (TreeViewItem moduleItem in tvAuthoritys.Items)
            {
                bool isFullDetailsChecked = true;
                bool hasChooseDetail = false;
                (moduleItem.Header as CheckBox).IsChecked = false;

                //所有子模块
                foreach (TreeViewItem moduleDetailItem in moduleItem.Items)
                {
                    bool isFullActionsChecked = true;
                    bool hasChooseAction = false;
                    long moduleId = DataType.Long((moduleDetailItem.Tag as DataRow)["Id"], 0);

                    (moduleDetailItem.Header as CheckBox).IsChecked = false;

                    //所有子模块操作
                    foreach (TreeViewItem actionItem in moduleDetailItem.Items)
                    {
                        long actionId = DataType.Long((actionItem.Tag as DataRow)["Id"], 0);
                        (actionItem.Header as CheckBox).IsChecked = false;

                        lock (_lockOperateDTRoleAuthoritys)
                        {
                            if (_dtRoleAuthoritys != null)
                            {
                                DataRow[] rows = _dtRoleAuthoritys.Select("[RoleId]=" + _currentRoleId + " and [ModuleId]=" + moduleId + " and [ActionId]=" + actionId);
                                if (rows != null && rows.Length > 0)
                                {
                                    (actionItem.Header as CheckBox).IsChecked = true;
                                    hasChooseAction = true;
                                    hasChooseDetail = true;
                                }
                                else
                                {
                                    isFullActionsChecked = false;
                                }
                            }
                            else
                            {
                                isFullActionsChecked = false;
                            }

                        }
                    }


                    if (isFullActionsChecked)
                    {
                        //下级全部选中
                        (moduleDetailItem.Header as CheckBox).IsChecked = true;
                    }
                    else if (!hasChooseAction)
                    {
                        //下级没有任何选择
                        (moduleDetailItem.Header as CheckBox).IsChecked = null;
                        isFullDetailsChecked = false;
                    }
                    else
                    {
                        //下级未全部选中
                        isFullDetailsChecked = false;
                    }
                }

                if (isFullDetailsChecked)
                {
                    //下级全部选中
                    (moduleItem.Header as CheckBox).IsChecked = true;
                }
                else if (!hasChooseDetail)
                {
                    //下级没有任何选择
                    (moduleItem.Header as CheckBox).IsChecked = null;
                }
            }

            _isUserOpera = true;
        }

        /// <summary>
        /// 操作取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModuleAction_Unchecked(object sender, RoutedEventArgs e)
        {
            //不是用户操作
            if (!_isUserOpera) return;

            CheckBox cb = sender as CheckBox;
            AuthorityItem authorityItem = cb.Tag as AuthorityItem;

            DataRow rowModuleDetail = authorityItem.ModuleDetailRow;
            DataRow rowAction = authorityItem.ActionRow;

            long moduleId = DataType.Long(rowModuleDetail["Id"], 0);
            long actionId = DataType.Long(rowAction["Id"], 0);

            if (_currentRoleId <= 0 || moduleId <= 0 || actionId <= 0) return;

            lock (_lockOperateDTRoleAuthoritys)
            {
                //角色编号
                long roleId = _currentRoleId;

                //判断是否有权限
                DataRow[] rows = _dtRoleAuthoritys.Select("[RoleId]=" + roleId + " and [ModuleId]=" + moduleId + " and [ActionId]=" + actionId);
                if (rows == null || rows.Length <= 0) return;

                foreach (DataRow r in rows)
                {
                    //删除权限
                    _dtRoleAuthoritys.Rows.Remove(r);
                }
            }
        }

        /// <summary>
        /// 操作选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModuleAction_Checked(object sender, RoutedEventArgs e)
        {
            //不是用户操作
            if (!_isUserOpera) return;

            CheckBox cb = sender as CheckBox;
            AuthorityItem authorityItem = cb.Tag as AuthorityItem;

            DataRow rowModuleDetail = authorityItem.ModuleDetailRow;
            DataRow rowAction = authorityItem.ActionRow;

            long moduleId = DataType.Long(rowModuleDetail["Id"], 0);
            long actionId = DataType.Long(rowAction["Id"], 0);

            if (_currentRoleId <= 0 || moduleId <= 0 || actionId <= 0) return;

            lock (_lockOperateDTRoleAuthoritys)
            {
                //角色编号
                long roleId = _currentRoleId;

                //判断是否有权限
                DataRow[] rows = _dtRoleAuthoritys.Select("[RoleId]=" + roleId + " and [ModuleId]=" + moduleId + " and [ActionId]=" + actionId);
                if (rows != null && rows.Length > 0) return;

                //添加到新行
                DataRow newRow = _dtRoleAuthoritys.NewRow();
                newRow["RoleId"] = _currentRoleId;
                newRow["ModuleId"] = moduleId;
                newRow["ActionId"] = actionId;
                _dtRoleAuthoritys.Rows.Add(newRow);
            }
        }

        /// <summary>
        /// 模块明细取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModuleDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem item = this.GetParentObject<TreeViewItem>(cb);
            if (item == null) return;

            foreach (TreeViewItem subItem in item.Items)
            {
                CheckBox subCB = subItem.Header as CheckBox;
                subCB.IsChecked = false;
            }
        }

        /// <summary>
        /// 模块明细选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModuleDetail_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem item = this.GetParentObject<TreeViewItem>(cb);
            if (item == null) return;

            foreach (TreeViewItem subItem in item.Items)
            {
                CheckBox subCB = subItem.Header as CheckBox;
                subCB.IsChecked = true;
            }
        }

        /// <summary>
        /// 模块取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModule_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem item = this.GetParentObject<TreeViewItem>(cb);

            foreach (TreeViewItem subItem in item.Items)
            {
                CheckBox subCB = subItem.Header as CheckBox;
                subCB.IsChecked = false;
            }
        }

        /// <summary>
        /// 模块选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbModule_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TreeViewItem item = this.GetParentObject<TreeViewItem>(cb);

            foreach (TreeViewItem subItem in item.Items)
            {
                CheckBox subCB = subItem.Header as CheckBox;
                subCB.IsChecked = true;
            }
        }


        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveRole_Click(object sender, RoutedEventArgs e)
        {
            string roleName = this.txtRoleName.Text.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                AppAlert.FormTips(gridMain, "请输入角色名称！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_Roles",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("RoleName", roleName)
                        }
                    };
                    long id = SQLiteDao.Insert(param);


                    if (id > 0)
                    {
                        if (_dtRoles != null)
                        {
                            //添加一行
                            DataRow row = _dtRoles.NewRow();
                            row["Id"] = id;
                            row["RoleName"] = roleName;
                            _dtRoles.Rows.Add(row);
                        }
                        else
                        {
                            //查询行
                            DataRow row = SQLiteDao.GetTableRow(id, "");
                            _dtRoles.Rows.Add(row);
                        }
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (id <= 0)
                        {
                            AppAlert.FormTips(gridMain, "添加角色失败！", AppCode.Enums.FormTipsType.Info);
                        }
                        else
                        {
                            this.txtRoleName.Text = "";
                            AppAlert.FormTips(gridMain, "添加角色成功！", AppCode.Enums.FormTipsType.Right);
                            this.listRoles.ItemsSource = null;
                            this.listRoles.ItemsSource = _dtRoles.DefaultView;
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
        /// 删除角色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            bool? isDelete = AppAlert.Alert("是否确定删除角色？", "是否确定删除？", AppCode.Enums.AlertWindowButton.OkCancel);
            if (!isDelete.HasValue || !isDelete.Value) return;

            Button btn = (sender as Button);
            ListViewItem item = this.GetParentObject<ListViewItem>(btn);

            DataRowView row = item.Content as DataRowView;

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {

                        Id = DataType.Long(row["Id"], 0),
                        TableName = "Sys_Roles"
                    };
                    bool flag = SQLiteDao.Delete(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            _dtRoles.Rows.Remove(row.Row);
                            this.listRoles.ItemsSource = null;
                            this.listRoles.ItemsSource = _dtRoles.DefaultView;
                            AppAlert.FormTips(gridMain, "删除角色成功！", AppCode.Enums.FormTipsType.Right);

                            if (_currentRoleId == param.Id)
                            {
                                //清空所有选择
                                ClearAllChoose();
                            }
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "删除角色失败！", AppCode.Enums.FormTipsType.Info);
                        }

                        return null;
                    }));
                }
                catch { }
            });
            thread.IsBackground = true;
            thread.Start();
        }

    }
}
