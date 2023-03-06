using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Wsfly.ERP.Std.AppCode.Handler;
using Wsfly.ERP.Std;
using Wsfly.ERP.Std.AppCode.Models;
using Wsfly.ERP.Std.Views.Controls;
using Wsfly.ERP.Std.Service.Dao;
using Wsfly.ERP.Std.Core.Models.Sys;
using Wsfly.ERP.Std.Service.Exts;
using Wsfly.ERP.Std.Core.Encryption;
using Wsfly.ERP.Std.Core.Handler;

public partial class AppGlobal : AppBaseGlobal
{
    #region 通用属性

    /// <summary>
    /// 托盘图标
    /// </summary>
    public static System.Windows.Forms.NotifyIcon _NotifyIcon = null;

    /// <summary>
    /// 回收系统垃圾
    /// </summary>
    public static void GCCollect()
    {
        try
        {
            GC.Collect();
            GC.SuppressFinalize(AppData.MainWindow);
            //GC.WaitForPendingFinalizers(); //导致程序无响应
            //GC.Collect();
        }
        catch { }
    }

    /// <summary>
    /// 是否注册
    /// </summary>
    public static bool _SFZC = false;
    /// <summary>
    /// 服务是否到期
    /// </summary>
    public static bool _FWDQ = false;
    /// <summary>
    /// 服务到期日期
    /// </summary>
    public static DateTime? _FWDQRQ = null;
    /// <summary>
    /// 首次启动日期
    /// </summary>
    public static DateTime? _FirstStartedDate = null;
    #endregion

    #region 终端唯一属性
    /// <summary>
    /// PC唯一ID
    /// </summary>
    private static string _pcid = string.Empty;
    /// <summary>
    /// CPU序列号
    /// </summary>
    private static string _cpuId = string.Empty;
    /// <summary>
    /// 主板序列号
    /// </summary>
    private static string _boardId = string.Empty;
    /// <summary>
    /// 锁
    /// </summary>
    private static object _lcokPCID = new object();
    /// <summary>
    /// PC唯一ID
    /// </summary>
    public static string _PCID
    {
        get
        {
            lock (_lcokPCID)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(_pcid)) return _pcid;

                    _cpuId = Wsfly.ERP.Std.Core.AppHandler.GetPCID("Win32_Processor");
                    _boardId = Wsfly.ERP.Std.Core.AppHandler.GetPCID("Win32_BaseBoard");

                    //加密
                    _pcid = Wsfly.ERP.Std.Core.Encryption.EncryptionAES.Encrypt(_cpuId + "|MZ|" + _boardId);

                    return _pcid;
                }
                catch (Exception ex) { }

                return "";
            }
        }
    }
    /// <summary>
    /// CPU序列号
    /// </summary>
    public static string _CPUID { get { return string.IsNullOrWhiteSpace(_PCID) ? "" : _cpuId; } }
    /// <summary>
    /// 主板序列号
    /// </summary>
    public static string _BOARDID { get { return string.IsNullOrWhiteSpace(_PCID) ? "" : _boardId; } }
    #endregion

    #region 计算公式
    private static string _jsgsType = null;
    /// <summary>
    /// 计算公式
    /// </summary>
    /// <param name="jsgs">公式 如：100*100</param>
    /// <param name="type">类型 如：typeof(float)</param>
    /// <param name="decimalDigits">小数位数</param>
    /// <returns></returns>
    public static object JSGS(string jsgs, Type type = null, int decimalDigits = 0)
    {
        //是否有计算公式类型
        if (string.IsNullOrWhiteSpace(_jsgsType))
        {
            _jsgsType = GetSysConfigReturnString("System_JSGS_Type");
            if (string.IsNullOrWhiteSpace(_jsgsType)) _jsgsType = "Default";
            _jsgsType = _jsgsType.ToLower();
        }
        //计算结果
        object dbValue = null;
        //JavaScript计算
        if (_jsgsType.Equals("javascript")) dbValue = Wsfly.ERP.Std.Core.Handler.JSHandler.Eval(jsgs);
        //SQLServer计算
        else if (_jsgsType.Equals("sqlserver")) dbValue = 0;
        //NCalc组件计算（建议）
        else if (_jsgsType.Equals("ncalc")) dbValue = new NCalc.Expression(jsgs).Evaluate();
        //Data.Compute计算
        else dbValue = new System.Data.DataTable().Compute(jsgs, "");

        Type[] types = { typeof(float), typeof(double), typeof(decimal) };
        if (type != null && types.Contains(type))
        {
            //是否数字、且有小数位数
            if (type != null && decimalDigits > 0)
            {
                if (type == typeof(float))
                {
                    //浮点小数
                    float fValue = DataType.Float(dbValue, 0);
                    dbValue = Math.Round(fValue, decimalDigits, MidpointRounding.AwayFromZero);
                }
                else if (type == typeof(double))
                {
                    //双精度浮点小数
                    double dValue = DataType.Double(dbValue, 0);
                    dbValue = Math.Round(dValue, decimalDigits, MidpointRounding.AwayFromZero);
                }
                else if (type == typeof(decimal))
                {
                    //decimal
                    decimal dValue = DataType.Decimal(dbValue, 0);
                    dbValue = Math.Round(dValue, decimalDigits, MidpointRounding.AwayFromZero);
                }
            }
        }

        //返回值
        return dbValue;
    }
    /// <summary>
    /// 计算公式
    /// </summary>
    /// <param name="jsgs">公式 如：100*100</param>
    /// <param name="typeString">类型 如：float</param>
    /// <param name="decimalDigits">小数位数</param>
    /// <returns></returns>
    public static object JSGS(string jsgs, string typeString, int decimalDigits = 0)
    {
        //列类型
        Type type = null;

        if (!string.IsNullOrWhiteSpace(typeString))
        {
            //是否包含此类型
            string[] types = { "float", "double", "decimal" };
            if (types.Contains(typeString.ToString()))
            {
                //列类型转换
                type = Wsfly.ERP.Std.Core.AppHandler.GetTypeByString(typeString);
            }
        }

        //执行计算
        return JSGS(jsgs, type, decimalDigits);
    }
    #endregion

    #region 程序资源图片
    /// <summary>
    /// 程序背景
    /// </summary>
    public static System.Windows.Media.ImageBrush _AppBackground { get; set; }
    /// <summary>
    /// 程序LOGO
    /// </summary>
    public static System.Windows.Media.ImageSource _AppLogo { get; set; }
    /// <summary>
    /// 程序ICO
    /// </summary>
    public static System.Windows.Media.ImageSource _AppICO { get; set; }
    #endregion

    #region 系统配置
    /// <summary>
    /// 系统配置
    /// </summary>
    public static DataTable SysConfigs { get; set; }
    /// <summary>
    /// 加载系统配置
    /// </summary>
    /// <returns></returns>
    public static bool LoadSysConfigs()
    {
        try
        {
            //系统配置
            AppGlobal.SysConfigs = SQLiteDao.GetTable("Sys_Configs");
            return true;
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "加载系统配置异常");
        }

        return false;
    }
    /// <summary>
    /// 设置系统设置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static bool SetSysConfig(string key, string value)
    {
        try
        {
            if (SysConfigs == null || SysConfigs.Rows.Count <= 0) return false;

            foreach (DataRow row in SysConfigs.Rows)
            {
                string cKey = row["Key"].ToString();
                if (string.IsNullOrWhiteSpace(cKey) || !cKey.Equals(key)) continue;

                row["Value"] = value;
                break;
            }

            //更新配置
            return SQLiteDao.Update(new SQLParam()
            {
                TableName = "Sys_Configs",
                OpreateCells = new List<KeyValue>()
                {
                    new KeyValue("Value", value)
                },
                Wheres = new List<Where>()
                {
                    new Where("Key", key)
                }
            });
        }
        catch (Exception ex) { }

        return false;
    }
    /// <summary>
    /// 得到系统配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object GetSysConfig(string key)
    {
        try
        {
            if (SysConfigs == null || SysConfigs.Rows.Count <= 0) return null;

            DataRow[] rows = SysConfigs.Select("[Key]='" + key + "'");
            if (rows == null || rows.Length <= 0) return null;

            return rows[0]["Value"];
        }
        catch { }

        return null;
    }
    /// <summary>
    /// 得到系统配置-返回字符串
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetSysConfigReturnString(string key)
    {
        try
        {
            object val = GetSysConfig(key);
            return val == null ? string.Empty : val.ToString();
        }
        catch { }

        return string.Empty;
    }
    /// <summary>
    /// 得到系统配置-返回Int
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int GetSysConfigReturnInt(string key, int defaultValue = 0)
    {
        try
        {
            object val = GetSysConfig(key);
            return DataType.Int(val, defaultValue);
        }
        catch { }

        return defaultValue;
    }
    /// <summary>
    /// 得到系统配置-返回Bool
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetSysConfigReturnBool(string key, bool defaultValue = false)
    {
        try
        {
            object val = GetSysConfig(key);
            return DataType.Bool(val, defaultValue);
        }
        catch { }

        return defaultValue;
    }
    /// <summary>
    /// 得到系统配置-返回Double
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static double GetSysConfigReturnDouble(string key, double defaultValue = 0)
    {
        try
        {
            object val = GetSysConfig(key);
            return DataType.Double(val, defaultValue);
        }
        catch { }

        return defaultValue;
    }
    /// <summary>
    /// 设置系统注册信息
    /// </summary>
    public static bool SetRegisterInfo(Dictionary<string, string> dicRegister)
    {
        try
        {
            string regData = Newtonsoft.Json.JsonConvert.SerializeObject(dicRegister);
            regData = Wsfly.ERP.Std.Core.Encryption.EncryptionAES.Encrypt(regData);
            return SetSysConfig("System_RegisterInfo", regData);
        }
        catch (Exception ex) { }

        return false;
    }
    /// <summary>
    /// 获取注册资料
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetRegisterInfo()
    {
        try
        {
            string registerInfos = AppGlobal.GetSysConfigReturnString("System_RegisterInfo");
            if (!string.IsNullOrWhiteSpace(registerInfos))
            {
                registerInfos = Wsfly.ERP.Std.Core.Encryption.EncryptionAES.Decrypt(registerInfos);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(registerInfos);
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// 启动先执行
    /// </summary>
    public static void FirstRun()
    {
        try
        {
            string regName = "Wsfly.ERP.Std";
            var registry = RegistryHandler.RegistItem(regName);

            if (!RegistryHandler.IsExistSubKey(registry, "IsInited"))
            {
                RegistryHandler.CreateSubItem(regName, "IsInited");
                RegistryHandler.SetValue(regName, "IsInited", "true");
            }

            if (!RegistryHandler.IsExistSubKey(registry, "FirstStartedDate"))
            {
                string kssjval = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                kssjval = EncryptionAES.Encrypt(kssjval);

                RegistryHandler.CreateSubItem(regName, "FirstStartedDate");
                RegistryHandler.SetValue(regName, "FirstStartedDate", kssjval);
            }
            else
            {
                string kssjval = RegistryHandler.GetValue_String(regName, "FirstStartedDate");
                kssjval = EncryptionAES.Decrypt(kssjval);

                _FirstStartedDate = DataType.DateTime(kssjval, DateTime.Now.AddYears(-1));
            }
        }
        catch (Exception ex) { }
    }
    #endregion

    #region 用户相关
    /// <summary>
    /// 获取所有用户列表
    /// </summary>
    /// <returns></returns>
    public static DataTable GetAllUsersByRoles(string keywords, int pageSize, int pageIndex)
    {
        return Wsfly.ERP.Std.Service.Dao.SQLiteDao.GetTable("Sys_Users");
    }
    /// <summary>
    /// 系统所有用户
    /// </summary>
    public static DataTable _MZUsers { get; set; }
    /// <summary>
    /// 加载所有系统用户
    /// </summary>
    public static void LoadAllUsers()
    {
        _MZUsers = Wsfly.ERP.Std.Service.Dao.SQLiteDao.GetTable("Sys_Users");
    }
    #endregion

    #region 过滤列名
    /// <summary>
    /// 下拉表过滤的列
    /// </summary>
    public static string[] DropDownTable_BuildFilterCells = { "ID", "CREATEDATE", "AUDITUSERID", "USERID", "CREATEUSERID", "MODEFYUSERID", "MODIFYDATE", "PASSWORD" };
    /// <summary>
    /// 编辑保存过滤的列
    /// </summary>
    public static string[] List_TableEditFilterCells = { "ISSELECTED", "ID", "PARENTID", "CREATEDATE", "ISAUDIT", "AUDITUSERID", "AUDITUSERNAME", "AUDITDATE", "MODIFYDATE", "MODIFYUSERID", "USERID", "USERNAME", "UPDATEDATE" };
    /// <summary>
    /// 回传行过滤的列
    /// </summary>
    public static string[] List_ReturnRowFilterCells = { "ISSELECTED", "ID", "PARENTID", "CREATEUSERID", "CREATEUSERNAME", "CREATEDATE", "ISAUDIT", "AUDITUSERID", "AUDITUSERNAME", "AUDITDATE", "TOTALMONEY", "TOTALCOUNT", "MODIFYDATE", "MODIFYUSERID", "USERID", "USERNAME", "UPDATEDATE", "ORDER", "REMARK", "ICON", "COUNT", "MZ_ISEDIT", "MZ_ISNEW", "WCSL", "WCJE", "ISSHOW", "ISLOCK", "SEARCHKEYWORDS", "CNNAME", "RQ", "DH", "BH", "LSH", "ZJE", "ZSL", "SYSBYYSL" };
    /// <summary>
    /// 粘贴行过滤的列
    /// </summary>
    public static string[] List_PasteFilterCells = { "ISSELECTED", "ID", "PARENTID", "CREATEUSERID", "CREATEUSERNAME", "CREATEDATE", "ISAUDIT", "AUDITUSERID", "AUDITUSERNAME", "AUDITDATE", "TOTALMONEY", "TOTALCOUNT", "MODIFYDATE", "MODIFYUSERID", "USERID", "USERNAME", "UPDATEDATE", "ORDER", "REMARK", "ICON", "COUNT", "MZ_ISEDIT", "MZ_ISNEW", "WCSL", "WCJE", "ISSHOW", "ISLOCK", "SEARCHKEYWORDS", "CNNAME", "RQ", "DH", "BH", "LSH", "ZJE", "ZSL", "SYSBYYSL" };
    /// <summary>
    /// 不可编辑的列
    /// </summary>
    public static string[] List_CannotEditCells = { "USERID", "USERNAME", "CREATEDATE", "ISAUDIT", "AUDITUSERID", "AUDITUSERNAME", "AUDITDATE", "CREATEUSERID", "CREATEUSERNAME" };
    /// <summary>
    /// 关键字过滤的列
    /// </summary>
    public static string[] FilterKeywordsCells = { "USERNAME" };
    /// <summary>
    /// 生成搜索关键字的后缀
    /// </summary>
    public static string[] BuildSearchKeywordsSuffix = { "BH", "DH", "MC", "NAME", "TITLE", "NUMBER", "BT", "KHMC", "KHBH", "SPMC", "SPBH", "DW", "YS", "GG", "XH" };
    /// <summary>
    /// 导入数据过滤的列
    /// </summary>
    public static string[] ImportDataFilterCells = { "ID", "MZ_ISEDIT", "MZ_ISNEW" };
    /// <summary>
    /// 需要创建表的类型
    /// </summary>
    public static string[] NeedBuildTableTypes = { "单表", "双表" };
    /// <summary>
    /// 编辑行的标识状态列名
    /// </summary>
    public static string DataGridEditStateCellName = "MZ_IsEdit";
    /// <summary>
    /// 新行的标识状态列名
    /// </summary>
    public static string DataGridNewStateCellName = "MZ_IsNew";
    #endregion

    #region 系统表名定义
    public static string SysTableStartWith = "Sys_";
    public static string SysTableName_Actions = "Sys_Actions";
    public static string SysTableName_Tables = "Sys_Tables";
    public static string SysTableName_TableCells = "Sys_TableCells";
    public static string SysTableName_TableDefaultCells = "Sys_TableDefaultCells";
    public static string SysTableName_Users = "Sys_Users";
    public static string SysTableName_UserConfigs = "Sys_UserConfigs";
    public static string SysTableName_UserTableConfigs = "Sys_UserTableConfigs";
    public static string SysTableName_Modules = "Sys_Modules";
    public static string SysTableName_ModuleDetails = "Sys_ModuleDetails";
    public static string SysTableName_PrintTemplates = "Sys_PrintTemplates";
    public static string SysTableName_TableActionEvents = "Sys_TableActionEvents";
    public static string SysTableName_TableMenus = "Sys_TableMenus";
    #endregion

    #region 复制放到内存
    /// <summary>
    /// 当前复制的数据
    /// </summary>
    public static CopyDataInfo RightKeyCopyData { get; set; }
    #endregion

    #region 表配置
    /// <summary>
    /// 视图表不显示的操作按钮
    /// </summary>
    public static int[] ViewTableNotShowActionCodes = { 1, 2, 3, 4, 5, 8, 9, 10, 12 };

    /// <summary>
    /// 数据表默认列
    /// </summary>
    public static DataTable _TableDefaultCells = null;

    /// <summary>
    /// 表操作动作
    /// </summary>
    public static DataTable TableOperateActions { get; set; }
    #endregion

    #region 本地配置
    /// <summary>
    /// 升级地址
    /// </summary>
    public static string UpgradeUrl = "http://upgrade.wsfly.com/MOZERP-Std/App.xml";
    /// <summary>
    /// API地址
    /// </summary>
    public static string APIUrl = "http://erp.wsfly.com/";

    /// <summary>
    /// 本地Key
    /// </summary>
    /// <returns></returns>
    public static string GetLocalKey()
    {
        string cpuId = Wsfly.ERP.Std.Core.Handler.PCHandler.GetPCID("Win32_Processor");
        string boardId = Wsfly.ERP.Std.Core.Handler.PCHandler.GetPCID("Win32_BaseBoard");
        string key = cpuId + boardId;
        return GetMD5(key);
    }

    /// <summary>
    /// 本地配置
    /// </summary>
    public static AppConfig LocalConfig { get; set; }
    #endregion

    #region 本地资源
    /// <summary>
    /// 加载本地资源图片
    /// </summary>
    /// <param name="dirName"></param>
    /// <param name="fileNameFormat"></param>
    /// <param name="maxCount"></param>
    public static List<Image> LoadResImages(string dirName, string fileNameFormat, int maxCount = 10)
    {
        List<Image> imgs = new List<Image>();
        string fileFormat = AppDomain.CurrentDomain.BaseDirectory + "Res\\" + dirName + "\\" + fileNameFormat;
        for (int i = 1; i <= maxCount; i++)
        {
            try
            {
                //文件路径
                string filePath = string.Format(fileFormat, i);
                if (!File.Exists(filePath)) continue;

                //读取文件为图片
                Image img = Image.FromFile(filePath);
                imgs.Add(img);
            }
            catch { }
        }

        return imgs;
    }
    #endregion

    #region 菜单
    /// <summary>
    /// 一级菜单
    /// </summary>
    public static DataTable Menus { get; set; }
    /// <summary>
    /// 二级菜单
    /// </summary>
    public static DataTable SubMenus { get; set; }
    #endregion

    #region 获取图片资源
    /// <summary>
    /// 是否图片路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsImage(string path)
    {
        //得到文件后缀名
        string ext = path.Substring(path.LastIndexOf('.'));
        if (string.IsNullOrWhiteSpace(ext)) return false;

        //图片后缀
        string[] imgExts = { ".jpg", ".jpeg", ".png", ".gif" };

        //是否图片
        if (imgExts.Contains(ext)) return true;
        return false;
    }

    #endregion

    #region 权限相关
    /// <summary>
    /// 根据表编号得到模块编号
    /// </summary>
    /// <param name="tableId"></param>
    /// <returns></returns>
    public static void GetModuleId(long tableId, ref long moduleId, ref List<long> moduleIds)
    {
        try
        {
            DataTable dt = SQLiteDao.GetTable(new SQLParam()
            {
                TableName = AppGlobal.SysTableName_ModuleDetails,
                Wheres = new List<Where>()
                {
                    new Where("TableId", tableId)
                }
            });

            if (dt != null && dt.Rows.Count > 0)
            {
                //模块ID列表
                moduleIds = new List<long>();

                //便利所有模块明细
                foreach (DataRow row in dt.Rows)
                {
                    long mId = DataType.Long(row["Id"], 0);
                    string mName = row["ModuleName"].ToString();
                    mName = mName.Replace("管理", "");

                    //添加到列表
                    moduleIds.Add(mId);
                    //第一个模块ID
                    if (moduleId <= 0) moduleId = mId;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "查询模块异常");
        }
    }

    /// <summary>
    /// 根据模块编号得到角色编号
    /// </summary>
    /// <param name="moduleId"></param>
    /// <returns></returns>
    public static long GetRoleId(long moduleId)
    {
        try
        {
            if (moduleId <= 0) return -1;

            if (AppGlobal.UserInfo.UserAuthoritys == null) return -1;

            UserAuthorityInfo info = AppGlobal.UserInfo.UserAuthoritys.Find(p => p.ModuleId == moduleId);
            if (info == null) return -1;

            UserRoleInfo role = AppGlobal.UserInfo.UserRoles.Find(p => p.RoleId.Equals(info.RoleId));
            if (role == null) return -1;

            return role.RoleId;
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "根据模块编号得到角色编号异常");
        }

        return -1;
    }
    /// <summary>
    /// 根据模块编号得到角色编号
    /// </summary>
    /// <param name="moduleIds"></param>
    /// <returns></returns>
    public static List<long> GetRoleId(List<long> moduleIds)
    {
        try
        {
            if (moduleIds == null || moduleIds.Count <= 0) return null;

            if (AppGlobal.UserInfo.UserAuthoritys == null) return null;

            List<long> roleIds = new List<long>();

            //遍历模块Id
            foreach (long moduleId in moduleIds)
            {
                //得到权限
                List<UserAuthorityInfo> infos = AppGlobal.UserInfo.UserAuthoritys.Where(p => p.ModuleId == moduleId).ToList();
                if (infos == null || infos.Count <= 0) continue;

                //遍历权限
                foreach (UserAuthorityInfo info in infos)
                {
                    //得到角色
                    IList<UserRoleInfo> roles = AppGlobal.UserInfo.UserRoles.Where(p => p.RoleId.Equals(info.RoleId)).ToList();
                    if (roles == null || roles.Count <= 0) continue;

                    //遍历角色
                    foreach (UserRoleInfo role in roles)
                    {
                        if (roleIds.Contains(role.RoleId)) continue;
                        roleIds.Add(role.RoleId);
                    }
                }
            }

            //是否有角色列表
            if (roleIds == null || roleIds.Count <= 0) return null;
            return roleIds;
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "根据模块编号得到角色编号异常");
        }

        return null;
    }

    /// <summary>
    /// 是否有权限
    /// </summary>
    /// <param name="moduleId">模块编号</param>
    /// <param name="actionId">操作编号</param>
    /// <returns>是否有权限</returns>
    public static bool HasAuthority(long moduleId, long actionId = 0)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;

        //没有模块编号
        if (moduleId <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        if (actionId <= 0)
        {
            //是否有模块编号
            return AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId) > 0;
        }
        else
        {
            //是否有模块及操作编号
            return AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId && p.ActionId == actionId) > 0;
        }
    }
    /// <summary>
    /// 根据Code判断
    /// </summary>
    /// <param name="moduleId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static bool HasAuthorityByCode(long moduleId, int code)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;

        //没有模块编号
        if (moduleId <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        if (code <= 0)
        {
            //是否有模块编号
            return AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId) > 0;
        }
        else
        {
            //是否有模块及操作编号
            return AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId && p.Code == code) > 0;
        }
    }
    /// <summary>
    /// 根据Code判断
    /// </summary>
    /// <param name="moduleIds"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static bool HasAuthorityByCode(List<long> moduleIds, int code)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;
        //没有模块编号
        if (moduleIds == null || moduleIds.Count <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        if (code <= 0)
        {
            //是否有模块编号
            foreach (long moduleId in moduleIds)
            {
                bool flag = AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId) > 0;
                if (flag) return true;
            }

            return false;
        }
        else
        {
            //是否有模块及操作编号
            foreach (long moduleId in moduleIds)
            {
                bool flag = AppGlobal.UserInfo.UserAuthoritys.Count(p => p.ModuleId == moduleId && p.Code == code) > 0;
                if (flag) return true;
            }

            return false;
        }
    }
    /// <summary>
    /// 是否有权限
    /// </summary>
    /// <param name="moduleId">模块编号</param>
    /// <param name="actionId">操作编号</param>
    /// <returns>是否有权限</returns>
    public static bool HasAuthority(long[] moduleIds)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;
        //没有模块编号
        if (moduleIds == null || moduleIds.Length <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        foreach (long moduleId in moduleIds)
        {
            //是否有模块编号
            bool flag = AppGlobal.UserInfo.UserAuthoritys.Exists(p => p.ModuleId == moduleId);
            if (flag) return true;
        }

        return false;
    }
    /// <summary>
    /// 是否有权限
    /// </summary>
    /// <param name="tableId"></param>
    /// <returns></returns>
    public static bool HasTableAuthority(long tableId)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;

        //查询模块
        DataTable dtModuleDetails = SQLiteDao.GetTable(new SQLParam()
        {
            TableName = "Sys_ModuleDetails",
            Wheres = new List<Where>()
            {
                new Where("TableId", tableId)
            }
        });
        if (dtModuleDetails == null || dtModuleDetails.Rows.Count <= 0)
        {
            return false;
        }
        else
        {
            //所有模块编号
            List<long> ids = new List<long>();
            foreach (DataRow row in dtModuleDetails.Rows)
            {
                long mid = DataType.Long(row["Id"], 0);
                if (mid <= 0) continue;

                ids.Add(mid);
            }

            //是否拥有权限
            return HasAuthority(ids.ToArray());
        }
    }
    /// <summary>
    /// 是否有权限
    /// </summary>
    /// <param name="tableId"></param>
    /// <returns></returns>
    public static bool HasTableAuthorityWithUser(long tableId)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;
        //没有表编号
        if (tableId <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        //是否有权限配置
        List<UserAuthorityInfo> listUserAuthoritys = AppGlobal.UserInfo.UserAuthoritys.Where(p => p.TableId == tableId).ToList();
        if (listUserAuthoritys == null || listUserAuthoritys.Count <= 0) return false;

        return true;
    }
    /// <summary>
    /// 是否有权限
    /// </summary>
    /// <param name="tableId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static bool HasTableAuthorityWithUser(long tableId, int code)
    {
        //超级管理员
        if (AppGlobal.UserInfo.IsSupperAdmin) return true;
        //没有表编号
        if (tableId <= 0) return false;
        //没有设置权限
        if (AppGlobal.UserInfo.UserAuthoritys == null || AppGlobal.UserInfo.UserAuthoritys.Count <= 0) return false;

        //是否有权限配置
        List<UserAuthorityInfo> listUserAuthoritys = AppGlobal.UserInfo.UserAuthoritys.Where(p => p.TableId.Equals(tableId) && p.Code.Equals(code)).ToList();
        if (listUserAuthoritys == null || listUserAuthoritys.Count <= 0) return false;

        return true;
    }
    #endregion

    #region 提示相关
    /// <summary>
    /// 提示声音
    /// </summary>
    public static void TipsSound()
    {
        //播放声音
        PlaySound(".Sounds.Tips.wav");
    }
    /// <summary>
    /// 提示声音
    /// </summary>
    public static void PlaySound(string path)
    {
        System.Threading.Thread thread = new System.Threading.Thread(delegate ()
        {
            try
            {
                //数据流
                Stream stream = null;

                if (path.StartsWith("http://"))
                {
                    //网络文件
                    Wsfly.ERP.Std.Core.Handler.ImageBrushHandler.GetNetImageSource(path);
                    stream = Wsfly.ERP.Std.Core.Handler.WebHandler.DownloadFile(path);
                }
                else if (path.StartsWith("//"))
                {
                    //相对路径
                    path = AppDomain.CurrentDomain.BaseDirectory + path.Substring(2).Replace("/", "\\");
                    if (File.Exists(path))
                    {
                        //读取文件流
                        stream = new FileStream(path, FileMode.Open);
                    }
                }
                else
                {
                    //本地文件
                    if (File.Exists(path))
                    {
                        //读取文件流
                        stream = new FileStream(path, FileMode.Open);
                    }
                }

                if (stream != null)
                {
                    //播放声音
                    Wsfly.ERP.Std.Core.Handler.VoiceHandler.PlayWav(stream);
                }
            }
            catch { }
        });
        thread.IsBackground = true;
        thread.Start();
    }
    #endregion

    #region 日期判断
    /// <summary>
    /// 今日是否工作日
    /// </summary>
    public static bool TodayIsWorkDay
    {
        get
        {
            DateTime dtNow = DateTime.Now;
            if (dtNow.DayOfWeek != System.DayOfWeek.Saturday && dtNow.DayOfWeek != System.DayOfWeek.Sunday) return true;
            return false;
        }
    }
    /// <summary>
    /// 得到月份的工作日天数
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static int GetMonthWorkDays(DateTime dt)
    {
        //天数
        int days = DateTime.DaysInMonth(dt.Year, dt.Month);

        //开始日期
        DateTime fromTime = new DateTime(dt.Year, dt.Month, 1);

        //工作日  
        int monthWorkDays = 0;

        for (int i = 0; i < days; i++)
        {
            //日期
            DateTime tempdt = fromTime.Date.AddDays(i);

            //是否工作日
            if (tempdt.DayOfWeek != System.DayOfWeek.Saturday && tempdt.DayOfWeek != System.DayOfWeek.Sunday)
            {
                //是工作日
                monthWorkDays++;
            }
        }

        return monthWorkDays;
    }
    /// <summary>
    /// 根据今日得到此前的工作日天数
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static int GetWorkDaysByEndDate(DateTime dt)
    {
        //天数
        int days = dt.Day;

        //开始日期
        DateTime fromTime = new DateTime(dt.Year, dt.Month, 1);

        //工作日  
        int monthWorkDays = 0;

        for (int i = 0; i < days; i++)
        {
            //日期
            DateTime tempdt = fromTime.Date.AddDays(i);

            //是否工作日
            if (tempdt.DayOfWeek != System.DayOfWeek.Saturday && tempdt.DayOfWeek != System.DayOfWeek.Sunday)
            {
                //是工作日
                monthWorkDays++;
            }
        }

        return monthWorkDays;
    }
    #endregion

    #region 数据转换
    /// <summary>
    /// 数据行转集合
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public static Dictionary<string, object> DataRowToDictionary(DataRow row)
    {
        if (row == null || row.Table.Columns == null || row.Table.Columns.Count <= 0) return null;

        try
        {
            Dictionary<string, object> dicJson = new Dictionary<string, object>();
            foreach (DataColumn col in row.Table.Columns)
            {
                //添加Dictionary
                dicJson[col.ColumnName] = row[col.ColumnName];
            }
            return dicJson;
        }
        catch { }

        return null;
    }
    /// <summary>
    /// 数据行转Json
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public static string DataRowToJson(DataRow row)
    {
        if (row == null || row.Table.Columns == null || row.Table.Columns.Count <= 0) return "";

        try
        {
            Dictionary<string, object> dicJson = new Dictionary<string, object>();
            foreach (DataColumn col in row.Table.Columns)
            {
                //添加Dictionary
                dicJson[col.ColumnName] = row[col.ColumnName];
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(dicJson);
        }
        catch { }

        return "";
    }
    #endregion

    #region 表维护配置
    /// <summary>
    /// 表配置列表
    /// </summary>
    static List<TableInfo> _listTableInfos = new List<TableInfo>();
    /// <summary>
    /// 获取所有表配置
    /// </summary>
    public static List<TableInfo> GetTableConfigs
    {
        get { return _listTableInfos; }
    }
    /// <summary>
    /// 表配置是否加载完成
    /// </summary>
    public static bool TableConfigsLoadSuccess { get { return (_listTableInfos != null && _listTableInfos.Count > 0); } }
    /// <summary>
    /// 加载终端表配置
    /// </summary>
    public static void LoadAllTableConfigs()
    {
        string msg = string.Empty;
        //加载终端表配置
        _listTableInfos = TableConfigsService.LoadClientTableInfos(AppGlobal.UserInfo.UserId);
    }
    /// <summary>
    /// 获取表配置ID
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static long GetTableId(string tableName)
    {
        //是否已经加载
        if (_listTableInfos != null && _listTableInfos.Count > 0)
        {
            TableInfo tableInfo = _listTableInfos.Find(p => p.TableName.Equals(tableName));
            return tableInfo == null ? 0 : tableInfo.Id;
        }
        return 0;
    }
    /// <summary>
    /// 获取表配置
    /// </summary>
    /// <param name="tableId"></param>
    /// <returns></returns>
    public static TableInfo GetTableConfig(long tableId)
    {
        TableInfo tableInfo = null;

        //是否已经加载
        if (_listTableInfos != null && _listTableInfos.Count > 0)
        {
            tableInfo = _listTableInfos.Find(p => p.Id == tableId);
        }

        return tableInfo;
    }
    /// <summary>
    /// 获取表配置
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static TableInfo GetTableConfig(string tableName)
    {
        TableInfo tableInfo = null;

        //是否已经加载
        if (_listTableInfos != null && _listTableInfos.Count > 0)
        {
            tableInfo = _listTableInfos.Find(p => p.TableName.Equals(tableName));
        }

        return tableInfo;
    }
    /// <summary>
    /// 获取子表配置
    /// </summary>
    /// <param name="parentTableId"></param>
    /// <returns></returns>
    public static TableInfo GetSubTableConfig(long parentTableId)
    {
        try
        {
            TableInfo tableInfo = null;
            //是否已经加载
            if (_listTableInfos != null && _listTableInfos.Count > 0)
            {
                tableInfo = _listTableInfos.Find(p => p.ParentId == parentTableId && p.Type.Equals("双表"));
                if (tableInfo == null) tableInfo = _listTableInfos.Find(p => p.ParentId == parentTableId && p.Type.Equals("单表"));
                if (tableInfo == null) tableInfo = _listTableInfos.Find(p => p.ParentId == parentTableId && p.Type.Equals("虚拟"));
            }
            //获取子表配置
            if (tableInfo != null) return tableInfo;
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "查询子表配置异常");
        }

        return null;
    }
    /// <summary>
    /// 获取三表配置
    /// </summary>
    /// <param name="parentTableId"></param>
    /// <returns></returns>
    public static TableInfo GetThreeTableConfig(long parentTableId)
    {
        try
        {
            TableInfo tableInfo = null;
            //是否已经加载
            if (_listTableInfos != null && _listTableInfos.Count > 0)
            {
                //获取表配置
                tableInfo = _listTableInfos.Find(p => p.ParentId == parentTableId && p.Type.Equals("三表"));
                if (tableInfo != null) return tableInfo;
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "查询三表配置异常");
        }

        return null;
    }
    /// <summary>
    /// 获取三表配置
    /// </summary>
    /// <param name="parentTableName"></param>
    /// <returns></returns>
    public static TableInfo GetThreeTableConfig(string parentTableName)
    {
        try
        {
            TableInfo tableInfo = null;
            //是否已经加载
            if (_listTableInfos != null && _listTableInfos.Count > 0)
            {
                //获取表配置
                tableInfo = _listTableInfos.Find(p => p.TableName == parentTableName && p.Type.Equals("双表"));
                if (tableInfo != null)
                {
                    return GetThreeTableConfig(tableInfo.Id);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "查询三表配置异常");
        }

        return null;
    }
    /// <summary>
    /// 设置表的排序及宽度
    /// </summary>
    /// <param name="tableId"></param>
    /// <param name="cellId"></param>
    /// <param name="order"></param>
    /// <param name="width"></param>
    public static void SetTableCellOrderAndWidth(long tableId, long cellId, int order, int width)
    {
        TableInfo tableInfo = GetTableConfig(tableId);
        if (tableInfo == null) return;
        CellInfo cellInfo = tableInfo.Cells.Find(p => p.Id == cellId);
        if (cellInfo == null) return;

        cellInfo.UserCellOrder = order;
        cellInfo.UserCellWidth = width;
    }
    /// <summary>
    /// 加载表配置、列配置
    /// </summary>
    /// <param name="tableId">表ID</param>
    /// <param name="loadAuditProcesses">是否加载审批流程</param>
    /// <returns></returns>
    private static TableInfo LoadTableConfigInfo(long tableId)
    {
        //表配置类
        TableInfo tableInfo = null;

        try
        {
            //加载表配置
            DataRow rowTable = SQLiteDao.GetTableRow(tableId, AppGlobal.SysTableName_Tables);
            //是否加载表成功
            if (rowTable == null || rowTable.Table.Columns == null || rowTable.Table.Columns.Count <= 0) return null;
            //表配置信息
            tableInfo = AppGlobal.DataRowToTableInfo(rowTable);

            //列配置初始化
            tableInfo.Cells = new List<CellInfo>();
            tableInfo.ViewCells = new List<string>();
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "加载表配置异常");
            return null;
        }

        try
        {
            //加载表列
            DataTable dt = SQLiteDao.GetTable(new SQLParam()
            {
                TableName = SysTableName_TableCells,
                Wheres = new List<Where>()
                {
                    new Where("ParentId", tableId)
                },
                OrderBys = new List<OrderBy>()
                {
                    new OrderBy("Order", OrderType.顺序),
                    new OrderBy("Id", OrderType.顺序),
                }
            });

            foreach (DataRow row in dt.Rows)
            {
                //列配置
                CellInfo cellInfo = AppGlobal.DataRowToCell(row);

                //添加列配置
                tableInfo.Cells.Add(cellInfo);
                //所有列名
                tableInfo.ViewCells.Add(cellInfo.CellName);
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.AddException(ex, "加载表列配置异常");
            return null;
        }

        return tableInfo;
    }
    /// <summary>
    /// 数据行表配置信息转表对象
    /// </summary>
    /// <param name="rowTable"></param>
    /// <returns></returns>
    public static TableInfo DataRowToTableInfo(DataRow rowTable)
    {
        //主表信息
        TableInfo tableInfo = new TableInfo();

        //得到所有属性
        System.Reflection.PropertyInfo[] propertys = typeof(TableInfo).GetProperties();

        //遍历属性
        foreach (System.Reflection.PropertyInfo property in propertys)
        {
            string pName = "";

            try
            {
                //属性名称
                pName = property.Name;

                //是否包含列
                if (rowTable.Table.Columns.Contains(pName))
                {
                    //空值
                    if (rowTable[pName] == DBNull.Value) continue;

                    //赋值
                    try
                    {
                        property.SetValue(tableInfo, Convert.ChangeType(rowTable[property.Name], property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.AddException(ex, "数据行表配置信息转表对象 赋值异常 [" + pName + "]");
                        property.SetValue(tableInfo, rowTable[pName], null);
                    }
                }
            }
            catch (Exception ex) { ErrorHandler.AddException(ex, "数据行表配置信息转表对象 反射异常 [" + pName + "]"); }
        }

        tableInfo.Cells = new List<CellInfo>();
        tableInfo.ViewCells = new List<string>();

        return tableInfo;
    }
    /// <summary>
    /// 数据行列配置转列配置对象
    /// </summary>
    public static CellInfo DataRowToCell(DataRow row)
    {
        //列配置信息
        CellInfo cellInfo = new CellInfo();

        //得到所有属性
        System.Reflection.PropertyInfo[] propertys = typeof(CellInfo).GetProperties();

        //遍历属性
        foreach (System.Reflection.PropertyInfo property in propertys)
        {
            string pName = "";

            try
            {
                //属性名称
                pName = property.Name;

                //是否包含列
                if (row.Table.Columns.Contains(pName))
                {
                    //空值
                    if (row[pName] == DBNull.Value) continue;

                    //赋值
                    try
                    {
                        property.SetValue(cellInfo, Convert.ChangeType(row[property.Name], property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        property.SetValue(cellInfo, row[pName], null);
                        ErrorHandler.AddException(ex, "数据行列配置转列配置对象 赋值异常 [" + pName + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.AddException(ex, "数据行列配置转列配置对象 反射异常 [" + pName + "]");
            }
        }

        //不显示关联主表列
        if (cellInfo.IsForeignKey) cellInfo.IsShow = false;
        //不显示关联主表列
        else if (cellInfo.CellName.Equals("ParentId")) cellInfo.IsShow = false;
        //关键字列不显示
        else if (cellInfo.CellName.Equals("SearchKeywords")) cellInfo.IsShow = false;

        return cellInfo;
    }
    #endregion

    #region FormTips图片
    /// <summary>
    /// 错误提示图片
    /// </summary>
    public static System.Windows.Media.ImageSource FormTips_Error { get; set; }
    /// <summary>
    /// 正确提示图片
    /// </summary>
    public static System.Windows.Media.ImageSource FormTips_Right { get; set; }
    /// <summary>
    /// 等待提示图片
    /// </summary>
    public static System.Windows.Media.ImageSource FormTips_Waiting { get; set; }
    /// <summary>
    /// 警告提示图片
    /// </summary>
    public static System.Windows.Media.ImageSource FormTips_Warning { get; set; }
    /// <summary>
    /// 消息提示图片
    /// </summary>
    public static System.Windows.Media.ImageSource FormTips_Info { get; set; }
    #endregion

    #region 判断
    /// <summary>
    /// 是否安装Excel
    /// </summary>
    /// <returns></returns>
    public static bool IsInstalledExcel()
    {
        Type type = Type.GetTypeFromProgID("Excel.Application");
        return type != null;
    }
    #endregion

    #region 计算文本显示
    /// <summary>
    /// 计算文本显示宽、高
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="fontFamily">The font family.</param>
    /// <param name="fontStyle">The font style.</param>
    /// <param name="fontWeight">The font weight.</param>
    /// <param name="fontStretch">The font stretch.</param>
    /// <param name="fontSize">Size of the font.</param>
    /// <returns></returns>
    public static MeasureSize MeasureTextSize(
        string text,
        System.Windows.Media.FontFamily fontFamily,
        System.Windows.FontStyle fontStyle,
        System.Windows.FontWeight fontWeight,
        System.Windows.FontStretch fontStretch,
        double fontSize)
    {
        System.Windows.Media.FormattedText ft = new System.Windows.Media.FormattedText(text,
                                             System.Globalization.CultureInfo.CurrentCulture,
                                             System.Windows.FlowDirection.LeftToRight,
                                             new System.Windows.Media.Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                             fontSize,
                                             System.Windows.Media.Brushes.Black);
        return new MeasureSize(ft.Width, ft.Height);
    }
    /// <summary>
    /// 计算文本显示宽、高
    /// </summary>
    public static MeasureSize MeasureText(string text,
        System.Windows.Media.FontFamily fontFamily,
        System.Windows.FontStyle fontStyle,
        System.Windows.FontWeight fontWeight,
        System.Windows.FontStretch fontStretch,
        double fontSize)
    {
        System.Windows.Media.Typeface typeface = new System.Windows.Media.Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
        System.Windows.Media.GlyphTypeface glyphTypeface;

        if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
        {
            return MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
        }

        double totalWidth = 0;
        double height = 0;

        for (int n = 0; n < text.Length; n++)
        {
            ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
            double width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
            double glyphHeight = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;

            if (glyphHeight > height)
            {
                height = glyphHeight;
            }
            totalWidth += width;
        }
        return new MeasureSize(totalWidth, height);
    }
    /// <summary>
    /// 显示文本尺寸
    /// </summary>
    public class MeasureSize
    {
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public MeasureSize() { }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public MeasureSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
    #endregion

    #region 系统时间设置
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [DllImport("kernel32.dll", EntryPoint = "GetSystemDefaultLCID")]
    public static extern int GetSystemDefaultLCID();
    /// <summary>
    /// 设置系统信息
    /// </summary>
    /// <param name="Locale"></param>
    /// <param name="LCType"></param>
    /// <param name="lpLCData"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", EntryPoint = "SetLocaleInfoA")]
    public static extern int SetLocaleInfo(int Locale, int LCType, string lpLCData);

    /// <summary>
    /// 长日期格式
    /// </summary>
    public const int LOCALE_SLONGDATE = 0x20;
    /// <summary>
    /// 短日期格式
    /// </summary>
    public const int LOCALE_SSHORTDATE = 0x1F;
    /// <summary>
    /// 时间格式
    /// </summary>
    public const int LOCALE_STIME = 0x1003;
    /// <summary>
    /// 设置系统日期格式
    /// </summary>
    public static void SetSystemDateTimeFormat()
    {
        try
        {
            int x = GetSystemDefaultLCID();
            SetLocaleInfo(x, LOCALE_STIME, "HH:mm:ss");          //时间格式
            SetLocaleInfo(x, LOCALE_SSHORTDATE, "yyyy-MM-dd");   //短日期格式  
            SetLocaleInfo(x, LOCALE_SLONGDATE, "yyyy-MM-dd");    //长日期格式 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    #endregion

    #region 辅助函数
    /// <summary>
    /// 获取IP
    /// </summary>
    /// <returns></returns>
    public static string GetIP()
    {
        try
        {
            string url = "http://erp.wsfly.com/Home/IP";
            string html = Wsfly.ERP.Std.Core.Handler.WebHandler.GetHtml(url, Encoding.UTF8);
            Dictionary<string, string> json = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(html);
            return json["IP"];
        }
        catch (Exception ex)
        {
        }

        return string.Empty;
    }

    /// <summary>
    /// 生成MD5
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetMD5(string data)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bytValue, bytHash;
        bytValue = Encoding.UTF8.GetBytes(data);
        bytHash = md5.ComputeHash(bytValue);
        md5.Clear();
        string sTemp = "";
        for (int i = 0; i < bytHash.Length; i++) sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
        return sTemp.ToLower();
    }
    /// <summary>  
    /// 获取时间戳  
    /// </summary>  
    /// <returns></returns>  
    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
    #endregion

    #region 清理缓存函数
    /// <summary>
    /// 清理缓存
    /// </summary>
    public static void CleanCaches()
    {
        System.Threading.Thread thread = new System.Threading.Thread(delegate ()
        {
            try
            {
                //清理缓存垃圾文件
                string rootDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] dirs =
                {
                    "AppData\\_Temp\\",
                    "AppData\\_Temp2\\"
                };

                //遍历要清理的目录
                foreach (string dir in dirs)
                {
                    //组合路径
                    string path = rootDir + dir;
                    //得到此路径下所有文件
                    List<string> files = Wsfly.ERP.Std.Core.Handler.FileHandler.GetFiles(path);
                    if (files != null && files.Count > 0)
                    {
                        //遍历文件列表
                        foreach (string fileName in files)
                        {
                            FileInfo fileInfo = new FileInfo(fileName);
                            if (fileInfo == null) continue;

                            DateTime dtLastAccessTime = fileInfo.LastAccessTime;
                            if ((DateTime.Now - dtLastAccessTime).TotalDays > 7)
                            {
                                try
                                {
                                    //删除文件
                                    fileInfo.Delete();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }
        });
        thread.IsBackground = true;
        thread.Start();
    }
    #endregion

    #region 颜色转笔刷
    /// <summary>
    /// HTML颜色转笔刷
    /// </summary>
    /// <param name="htmlColor"></param>
    /// <returns></returns>
    public static System.Windows.Media.Brush HtmlColorToBrush(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor)) return System.Windows.Media.Brushes.White;

        System.Windows.Media.BrushConverter brushConverter = new System.Windows.Media.BrushConverter();
        return (System.Windows.Media.Brush)brushConverter.ConvertFromString(htmlColor.Trim());
    }
    /// <summary>
    /// 将颜色转为笔刷
    /// </summary>
    /// <param name="htmlColor"></param>
    /// <returns></returns>
    public static System.Windows.Media.SolidColorBrush ColorToBrush(string htmlColor)
    {
        System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(htmlColor);
        return new System.Windows.Media.SolidColorBrush(color);
    }
    #endregion

    /// <summary>
    /// 得到图片
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static System.Windows.Media.ImageSource GetImageSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            return Wsfly.ERP.Std.AppCode.Handler.ImageBrushHandler.GetLocalImageSource(path);
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "加载图片异常");
        }

        return null;
    }
    /// <summary>
    /// 得到图片刷
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static System.Windows.Media.ImageBrush GetImageBrush(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            return Wsfly.ERP.Std.AppCode.Handler.ImageBrushHandler.GetLocalImageBrush(path);
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "加载图片异常");
        }

        return null;
    }
    /// <summary>
    /// 上传文件 将文件复制到程序目录：./AppFiles/
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>相对路径</returns>
    public static string UploadFile(string path)
    {
        try
        {
            string directoryName = "AppFiles\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            string fileName = DateTime.Now.ToFileTime() + System.IO.Path.GetExtension(path);
            string destFilePath = AppDomain.CurrentDomain.BaseDirectory + directoryName;
            if (!System.IO.Directory.Exists(destFilePath)) System.IO.Directory.CreateDirectory(destFilePath);
            destFilePath += fileName;
            System.IO.File.Copy(path, destFilePath);
            return directoryName + fileName;
        }
        catch (Exception ex) { }

        return string.Empty;
    }
    /// <summary>
    /// 获取上传文件的路径
    /// </summary>
    /// <param name="relativePath">相对路径</param>
    /// <returns>绝对路径</returns>
    public static string GetUploadFilePath(string relativePath)
    {
        return AppDomain.CurrentDomain.BaseDirectory + relativePath;
    }

}
