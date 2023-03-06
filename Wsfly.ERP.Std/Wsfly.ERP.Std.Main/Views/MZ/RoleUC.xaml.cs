
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

namespace Wsfly.ERP.Std.Views.MZ
{
    /// <summary>
    /// RoleUC.xaml 的交互逻辑
    /// </summary>
    public partial class RoleUC : BaseUserControl
    {
        DataTable _dtRoles = null;
        DataTable _dtUserRoles = null;

        long _currentUserId = 0;

        /// <summary>
        /// 构造
        /// </summary>
        public RoleUC()
        {
            InitializeComponent();

            InitUCSize(-80, -80);

            this.Loaded += RoleUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoleUC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRoles();
            LoadDepartmentAndUsers();
            LoadUserRoles();

            this.btnSaveRole.Click += BtnSaveRole_Click;
            this.btnReloadUsers.Click += BtnReloadUsers_Click;
            this.btnReloadRoles.Click += BtnReloadRoles_Click;
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
        /// 重新加载用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadDepartmentAndUsers();
            LoadUserRoles();
        }

        /// <summary>
        /// 加载用户的角色
        /// </summary>
        private void LoadUserRoles()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_UserRoles",
                    };
                    //用户关联角色
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null)
                    {
                        //生成空表
                        BuildEmptyUserRoleTable();
                        return;
                    }

                    //用户角色表
                    _dtUserRoles = dt;
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "加载用户关联角色失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 角色权限版本
        /// </summary>
        private void LoadDepartmentAndUsers()
        {
            this.tvDepartments.Items.Clear();

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //所有部门
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_Departments",
                        Wheres = new List<Where>()
                        {
                            new Where("BMMC", null, WhereType.非空),
                        }
                    };
                    DataTable dtDepartments = SQLiteDao.GetTable(param);
                    if (dtDepartments == null || dtDepartments.Rows.Count <= 0) return;

                    //所有用户
                    param = new SQLParam()
                    {
                        TableName = "Sys_Users",
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="Id", CellValue=1, Type=WhereType.大于 }
                        }
                    };
                    DataTable dtUsers = SQLiteDao.GetTable(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //生成部门和用户
                        BuildDepartmentAndUsers(dtDepartments, dtUsers, 0);
                        return null;
                    }));
                }
                catch
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "加载部门及用户失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 生成部门和用户
        /// </summary>
        private void BuildDepartmentAndUsers(DataTable dtDepartments, DataTable dtUsers, long parentId)
        {
            //遍历所有部门
            foreach (DataRow row in dtDepartments.Rows)
            {
                //当前部门ID
                long id = DataType.Long(row["Id"], 0);

                //部门
                TreeViewItem deptItem = new TreeViewItem();
                deptItem.Tag = row;
                deptItem.Header = row["BMMC"].ToString();
                deptItem.IsExpanded = true;

                //部门用户
                if (dtUsers != null && dtUsers.Rows.Count > 0)
                {
                    //部门用户
                    AddDepartmentUsers(deptItem, row, dtUsers);
                }

                //添加为根节点
                this.tvDepartments.Items.Add(deptItem);
            }
        }

        /// <summary>
        /// 将用户分组到部门
        /// </summary>
        /// <param name="subItem"></param>
        /// <param name="dtUsers"></param>
        private void AddDepartmentUsers(TreeViewItem item, DataRow rowDepartment, DataTable dtUsers)
        {
            long departmentId = DataType.Long(rowDepartment["Id"], 0);

            DataRow[] rows = dtUsers.Select("[ParentId]=" + departmentId);

            if (rows != null && rows.Length > 0)
            {
                foreach (DataRow row in rows)
                {
                    string loginName = row["LoginName"].ToString();
                    string realName = row["RealName"].ToString();

                    string showName = string.IsNullOrWhiteSpace(realName) ? loginName : realName;

                    TreeViewItem subItem = new TreeViewItem();
                    subItem.Tag = row;
                    subItem.Header = "＠" + showName;
                    subItem.PreviewMouseLeftButtonDown += UserItem_PreviewMouseLeftButtonDown;

                    item.Items.Add(subItem);
                }
            }
        }

        /// <summary>
        /// 点击用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            DataRow rowUser = item.Tag as DataRow;

            _currentUserId = DataType.Long(rowUser["Id"], 0);
            this.lblChooseUser.Text = item.Header.ToString() + "的";

            DataRow[] rows = null;

            //获取用户拥有的角色
            if (_dtUserRoles != null)
            {
                rows = _dtUserRoles.Select("[UserId]=" + _currentUserId);
            }
            else
            {
                //生成空表
                BuildEmptyUserRoleTable();
            }

            for (int i = 0; i < this.listRoles.Items.Count; i++)
            {
                try
                {
                    DataRowView rowItem = this.listRoles.Items[i] as DataRowView;
                    ListViewItem roleItem = this.listRoles.ItemContainerGenerator.ContainerFromItem(rowItem) as ListViewItem;
                    CheckBox cb = this.GetChildObject<CheckBox>(roleItem);
                    Label lbl = this.GetChildObject<Label>(roleItem, "lbl");
                    lbl.Content = "";

                    long roleId = DataType.Long(rowItem["Id"], 0);

                    //是否有角色权限
                    if (rows != null && rows.Length > 0 && rows.Count(p => DataType.Long(p["RoleId"], -1) == roleId) > 0)
                    {
                        roleItem.IsSelected = true;
                        cb.IsChecked = true;
                    }
                    else
                    {
                        roleItem.IsSelected = false;
                        cb.IsChecked = false;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 增加一个下级节点
        /// </summary>
        /// <param name="item"></param>
        private void AddLoadingItem(TreeViewItem item)
        {
            TreeViewItem subItem = new TreeViewItem();
            subItem.Tag = "loading";
            subItem.Header = "请稍候，正在加载...";
            item.Items.Add(subItem);
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
                    SQLParam param = new SQLParam()
                    {

                        TableName = "Sys_Roles",
                    };
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0) return;

                    //默认移除第一个角色
                    dt.Rows.RemoveAt(0);

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
                            AppAlert.FormTips(gridMain, "删除角色成功！", AppCode.Enums.FormTipsType.Info);
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

        /// <summary>        
        /// 表全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTopSelectedAll_Checked(object sender, RoutedEventArgs e)
        {
            this.listRoles.SelectAll();

            for (int i = 0; i < this.listRoles.Items.Count; i++)
            {
                try
                {
                    DataRowView rowItem = this.listRoles.Items[i] as DataRowView;
                    ListViewItem roleItem = this.listRoles.ItemContainerGenerator.ContainerFromItem(rowItem) as ListViewItem;
                    CheckBox cb = this.GetChildObject<CheckBox>(roleItem);

                    cb.IsChecked = true;
                }
                catch { }
            }
        }

        /// <summary>
        /// 表全不选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTopSelectedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.listRoles.UnselectAll();

            for (int i = 0; i < this.listRoles.Items.Count; i++)
            {
                try
                {
                    DataRowView rowItem = this.listRoles.Items[i] as DataRowView;
                    ListViewItem roleItem = this.listRoles.ItemContainerGenerator.ContainerFromItem(rowItem) as ListViewItem;
                    CheckBox cb = this.GetChildObject<CheckBox>(roleItem);

                    cb.IsChecked = false;
                }
                catch { }
            }
        }

        /// <summary>        
        /// 选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0) return;

            ListViewItem item = this.GetParentObject<ListViewItem>(sender as CheckBox);
            item.IsSelected = true;

            long roleId = DataType.Long((item.Content as DataRowView)["Id"], -1);

            DataRow[] rows = null;

            //是否有用户角色
            if (_dtUserRoles != null)
            {
                rows = _dtUserRoles.Select("[UserId]=" + _currentUserId + " AND [RoleId]=" + roleId);
            }
            else
            {
                //生成空表
                BuildEmptyUserRoleTable();
            }

            if (rows == null || rows.Length <= 0)
            {
                //增加角色
                DataRow row = _dtUserRoles.NewRow();
                row["UserId"] = _currentUserId;
                row["RoleId"] = roleId;
                _dtUserRoles.Rows.Add(row);

                Label lbl = this.GetChildObject<Label>(item, "lbl");
                if (lbl != null) { lbl.Content = "正在添加..."; }

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        SQLParam param = new SQLParam()
                        {
                            TableName = "Sys_UserRoles",
                            OpreateCells = new List<KeyValue>()
                            {
                                new KeyValue("UserId", _currentUserId),
                                new KeyValue("RoleId", roleId)
                            }
                        };
                        long id = SQLiteDao.Insert(param);

                        if (id > 0)
                        {
                            row["Id"] = id;
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                if (lbl != null) { lbl.Content = "添加成功"; }
                                return null;
                            }));
                            return;
                        }
                    }
                    catch { }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (lbl != null) { lbl.Content = "添加失败"; }
                        return null;
                    }));
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }
        /// <summary>
        /// 生成用户角色空表
        /// </summary>
        private void BuildEmptyUserRoleTable()
        {
            //表
            _dtUserRoles = new DataTable();

            //编号列
            DataColumn colId = new DataColumn("Id", typeof(long));
            //用户编号列
            DataColumn colUserId = new DataColumn("UserId", typeof(long));
            //角色编号列
            DataColumn colRoleId = new DataColumn("RoleId", typeof(long));

            _dtUserRoles.Columns.Add(colId);
            _dtUserRoles.Columns.Add(colUserId);
            _dtUserRoles.Columns.Add(colRoleId);
        }

        /// <summary>
        /// 不选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentUserId <= 0) return;

            ListViewItem item = this.GetParentObject<ListViewItem>(sender as CheckBox);
            item.IsSelected = false;

            long roleId = DataType.Long((item.Content as DataRowView)["Id"], -1);

            DataRow[] rows = _dtUserRoles.Select("[UserId]=" + _currentUserId + " AND [RoleId]=" + roleId);

            if (rows != null && rows.Length > 0)
            {
                //取消角色
                _dtUserRoles.Rows.Remove(rows[0]);

                Label lbl = this.GetChildObject<Label>(item, "lbl");
                if (lbl != null) { lbl.Content = "正在取消..."; }

                System.Threading.Thread thread = new System.Threading.Thread(delegate ()
                {
                    try
                    {
                        SQLParam param = new SQLParam()
                        {
                            TableName = "Sys_UserRoles",
                            Wheres = new List<Where>()
                            {
                                 new Where() { CellName="UserId", CellValue = _currentUserId },
                                 new Where() { CellName="RoleId", CellValue = roleId },
                            }
                        };
                        bool flag = SQLiteDao.Delete(param);
                        if (flag)
                        {
                            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                            {
                                if (lbl != null) { lbl.Content = "取消成功"; }
                                return null;
                            }));
                            return;
                        }
                    }
                    catch { }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (lbl != null) { lbl.Content = "取消失败"; }
                        return null;
                    }));
                });
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
