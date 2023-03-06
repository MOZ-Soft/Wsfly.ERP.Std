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
using System.Data;

using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.Views.Parts;
using Wsfly.ERP.Std.AppCode.Extensions;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Core.Handler;
using Wsfly.ERP.Std.Core.Extensions;
using Wsfly.ERP.Std.Service.Exts;

namespace Wsfly.ERP.Std.Views.Home
{
    /// <summary>
    /// LoginUC.xaml 的交互逻辑
    /// </summary>
    public partial class LoginUC : BaseUserControl
    {
        /// <summary>
        /// 构造
        /// </summary>
        public LoginUC()
        {
            InitializeComponent();
            this.Loaded += LoginUC_Loaded;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnLogin.Click += BtnLogin_Click;
            this.KeyDown += LoginUC_KeyDown;

            this.cbRemember.IsChecked = AppGlobal.LocalConfig.RememberPwd;
            this.txtUserName.Text = AppGlobal.LocalConfig.UserName;
            if (!string.IsNullOrWhiteSpace(AppGlobal.LocalConfig.UserPwd))
            {
                try
                {
                    //登陆密码解密
                    string key = AppGlobal.GetLocalKey();
                    string pwd = Core.Encryption.EncryptionDES.Decrypt(AppGlobal.LocalConfig.UserPwd, key);
                    if (!string.IsNullOrWhiteSpace(pwd))
                    {
                        //赋予密码
                        this.txtPassword.Password = pwd;
                        //自动登陆
                        Login();
                    }
                }
                catch { }
            }
            this.txtUserName.Focus();

            //首次登陆
            bool firstLogin = AppGlobal.GetSysConfigReturnBool("System_FirstLogin", false);
            if (firstLogin)
            {
                this.borderFirstLogin.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Reader
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            //初始窗口大小
            InitSize();
            AppData.MainWindow.SizeChanged += MainWindow_SizeChanged;
            AppData.MainWindow.StateChanged += MainWindow_StateChanged;
        }

        /// <summary>
        /// 窗口状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            //初始窗口大小
            InitSize();
        }

        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //初始窗口大小
            InitSize();
        }

        /// <summary>
        /// 窗口大小
        /// </summary>
        private void InitSize()
        {
            try
            {
                double width = AppData.MainWindow.WinWidth - 2;
                double height = AppData.MainWindow.WinHeight - 82;

                if (width > 0) this.Width = width;
                if (height > 0) this.Height = height;
            }
            catch { }
        }
        /// <summary>
        /// 回车调用登陆
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginUC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.txtUserName.IsFocused)
                {
                    this.txtPassword.Focus();
                    this.txtPassword.SelectAll();
                    return;
                }

                //登陆
                Login();
            }
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            //登陆
            Login();
        }

        /// <summary>
        /// 登陆
        /// </summary>
        private void Login()
        {
            //清空状态文本
            SetStatusText("");

            string un = this.txtUserName.Text.Trim();
            string pwd = this.txtPassword.Password.Trim();
            bool isRemember = this.cbRemember.IsChecked.Value;

            if (string.IsNullOrWhiteSpace(un))
            {
                AppAlert.FormTips(gridMain, "请输入帐号！", AppCode.Enums.FormTipsType.Info);
                return;
            }
            if (string.IsNullOrWhiteSpace(pwd))
            {
                AppAlert.FormTips(gridMain, "请输入密码！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            //显示加载层
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //登陆操作
                string msg = "请重试";
                //调用登陆
                System.Data.DataRow row = null;
                System.Data.DataRow rowDepartment = null;

                try
                {
                    //调用登陆
                    AppendStatusText("正在调用登陆...");
                    row = UserService.Login(un, pwd, ref msg);
                    rowDepartment = SQLiteDao.GetTableRow(row.GetParentId(), "Sys_Departments");
                }
                catch (Exception ex)
                {
                    //登陆失败
                    ErrorHandler.AddException(ex, "调用登陆异常，返回消息：" + msg);
                }

                if (row != null && row.Table.Columns != null && row.Table.Columns.Count > 0)
                {
                    //用户信息
                    UserInfo uInfo = new UserInfo()
                    {
                        UserId = DataType.Long(row["Id"], 0),
                        Number = row["Number"].ToString(),
                        LoginName = row["LoginName"].ToString(),
                        UserName = row["RealName"].ToString(),
                        Sex = row["Sex"].ToString(),
                        QQ = row["QQ"].ToString(),
                        WeiXin = row["WeiXin"].ToString(),
                        Telphone = row["Telphone"].ToString(),
                        Mobile = row["Mobile"].ToString(),
                        Head = row["Head"].ToString(),
                        CreateDate = DataType.DateTime(row["CreateDate"], DateTime.Now),
                        IsSupper = DataType.Bool(row["IsSupper"], false),
                        IsManager = DataType.Bool(row["IsManager"], false),
                        DepartmentId = DataType.Long(rowDepartment["Id"], 0),
                        DepartmentName = rowDepartment["BMMC"].ToString()
                    };

                    //用户数据
                    AppGlobal.UserDataRow = row;

                    //生日
                    if (row["Birthday"] is DBNull) uInfo.Birthday = null;
                    else uInfo.Birthday = DataType.DateTime(row["Birthday"], DateTime.Now);

                    if (uInfo == null || uInfo.UserId <= 0)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //登录失败
                            AppAlert.FormTips(gridMain, "登陆失败，请再次尝试！", AppCode.Enums.FormTipsType.Info);
                            SetStatusText("");
                            HideLoading();
                            return null;
                        }));
                        return;
                    }

                    try
                    {
                        //加载用户配置
                        SQLParam param = new SQLParam()
                        {
                            TableName = "Sys_UserConfigs",
                            Wheres = new List<Where>()
                            {
                                new Where() { CellName = "UserId", CellValue = uInfo.UserId }
                            }
                        };
                        //加载配置
                        DataRow rowConfig = SQLiteDao.GetTableRow(param);
                        //用户配置
                        AppGlobal.UserConfig = PropertyHandler.DataRowToModel<UserConfig>(rowConfig);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载用户配置失败");
                    }

                    try
                    {
                        //加载用户权限
                        string sql = @"select sra.*,sa.Code,md.TableId,md.TableName,md.ModuleName
                                               from [Sys_RoleAuthoritys] sra 
                                               left join [Sys_Actions] sa on sra.ActionId=sa.Id 
                                               left join [Sys_ModuleDetails] md on sra.[ModuleId]=md.Id
                                               where [RoleId] in (select [RoleId] from [Sys_UserRoles] where [UserId]=" + uInfo.UserId + ")";

                        DataTable dtAuthoritys = SQLiteDao.GetTableBySQL(sql);
                        if (dtAuthoritys != null && dtAuthoritys.Rows.Count > 0)
                        {
                            //用户权限
                            uInfo.UserAuthoritys = new List<UserAuthorityInfo>();

                            foreach (DataRow r in dtAuthoritys.Rows)
                            {
                                UserAuthorityInfo authority = new UserAuthorityInfo();
                                authority.RoleId = DataType.Long(r["RoleId"], -1);
                                authority.ModuleId = DataType.Long(r["ModuleId"], -1);
                                authority.ModuleName = r["ModuleName"].ToString();
                                authority.TableId = DataType.Long(r["TableId"], -1);
                                authority.TableName = r["TableName"].ToString();
                                authority.ActionId = DataType.Long(r["ActionId"], -1);
                                authority.Code = DataType.Int(r["Code"], -1);
                                uInfo.UserAuthoritys.Add(authority);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载用户权限失败");
                    }

                    try
                    {
                        //加载用户角色
                        string sql = "select * from [Sys_Roles] where [Id] in (select [RoleId] from [Sys_UserRoles] where [UserId]=" + uInfo.UserId + ")";
                        DataTable dtRoles = SQLiteDao.GetTableBySQL(sql);
                        if (dtRoles != null && dtRoles.Rows.Count > 0)
                        {
                            //用户角色
                            uInfo.UserRoles = new List<UserRoleInfo>();

                            foreach (DataRow r in dtRoles.Rows)
                            {
                                UserRoleInfo role = new UserRoleInfo();
                                role.RoleId = DataType.Long(r["Id"], 0);
                                role.RoleName = r["RoleName"].ToString();
                                uInfo.UserRoles.Add(role);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "加载用户角色失败");
                    }

                    if (uInfo == null || uInfo.UserId <= 0)
                    {
                        this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                        {
                            //登录失败
                            AppAlert.FormTips(gridMain, "登陆失败，未得到登陆信息！", AppCode.Enums.FormTipsType.Info);
                            SetStatusText("");
                            HideLoading();
                            return null;
                        }));
                        return;
                    }

                    #region 登陆日志
                    var dicRegisterInfo = AppGlobal.GetRegisterInfo();
                    if (dicRegisterInfo != null && dicRegisterInfo.ContainsKey("SerialNumber"))
                    {
                        System.Threading.Thread threadDLRZ = new System.Threading.Thread(delegate ()
                        {
                            try
                            {
                                //登陆登记
                                var apiResult = AppCode.API.XTAPI.Login(dicRegisterInfo, uInfo);
                                if (apiResult != null && apiResult.Success)
                                {
                                }
                            }
                            catch (Exception ex) { }

                            //保持用户在线
                            AppCode.API.XTAPI.KeepUserOnline();
                        });
                        threadDLRZ.Start();
                    }
                    #endregion

                    //登陆成功
                    AppGlobal.UserInfo = uInfo;

                    //记住帐号
                    AppGlobal.LocalConfig.UserName = un;

                    //首次登陆
                    bool firstLogin = AppGlobal.GetSysConfigReturnBool("System_FirstLogin", false);
                    if (firstLogin) AppGlobal.SetSysConfig("System_FirstLogin", "false");

                    if (isRemember)
                    {
                        //有记住密码
                        AppGlobal.LocalConfig.RememberPwd = true;
                        string key = AppGlobal.GetLocalKey();
                        AppGlobal.LocalConfig.UserPwd = Core.Encryption.EncryptionDES.Encrypt(pwd, key);
                    }
                    else
                    {
                        //不记住密码
                        AppGlobal.LocalConfig.RememberPwd = false;
                    }

                    //保存帐号
                    AppGlobal.LocalConfig.Save();

                    //登陆成功
                    AppData.MainWindow.Logined(this);
                }
                else
                {
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        //登录失败
                        AppAlert.FormTips(gridMain, "登陆失败，" + msg + "！", AppCode.Enums.FormTipsType.Info);
                        SetStatusText("");
                        HideLoading();
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="statusText"></param>
        public void SetStatusText(string statusText)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                if (!string.IsNullOrWhiteSpace(statusText)) statusText = "◆ " + statusText;
                this.lblStatus.Text = statusText;
                return false;
            }));
        }
        /// <summary>
        /// 追加状态
        /// </summary>
        /// <param name="statusText"></param>
        public void AppendStatusText(string statusText)
        {
            this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
            {
                if (!string.IsNullOrWhiteSpace(statusText)) statusText = "\n◆ " + statusText;
                this.lblStatus.Text += statusText;
                return false;
            }));
        }
    }
}
