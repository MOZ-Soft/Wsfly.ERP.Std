using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Models.Sys
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 用户编号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 登陆帐号
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Head { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string Telphone { get; set; }
        /// <summary>
        /// 微信号
        /// </summary>
        public string WeiXin { get; set; }
        /// <summary>
        /// QQ号码
        /// </summary>
        public string QQ { get; set; }
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 用户标识
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
        public long DepartmentId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        public long BMID { get { return DepartmentId; } }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string BMMC { get { return DepartmentName; } }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long Id { get { return UserId; } }
        /// <summary>
        /// 姓名
        /// </summary>
        public string XM { get { return UserName; } }
        /// <summary>
        /// 手机
        /// </summary>
        public string SJ { get { return Mobile; } }

        /// <summary>
        /// 限制时间登陆
        /// </summary>
        public bool XZSJDL { get; set; }
        /// <summary>
        /// 可登陆时间
        /// </summary>
        public int KDLSJ { get; set; }
        /// <summary>
        /// 限制时间操作
        /// </summary>
        public bool XZSJCZ { get; set; }
        /// <summary>
        /// 可操作时间
        /// </summary>
        public int KCZSJ { get; set; }
        /// <summary>
        /// 限制PC登陆
        /// </summary>
        public bool XZPCDL { get; set; }
        /// <summary>
        /// 绑定PC编码
        /// </summary>
        public string BDPCBM { get; set; }
        

        /// <summary>
        /// 是否实名认证
        /// </summary>
        public bool IsRNA { get; set; }
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSupper { get; set; }
        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsManager { get; set; }
        /// <summary>
        /// 用户权限
        /// </summary>
        public List<UserAuthorityInfo> UserAuthoritys { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public List<UserRoleInfo> UserRoles { get; set; }
        ///// <summary>
        ///// 用户数据表
        ///// </summary>
        //public System.Data.DataTable UserDataTable { get; set; }
        ///// <summary>
        ///// 用户数据行
        ///// </summary>
        //public System.Data.DataRow UserDataRow { get { return UserDataTable == null ? null : UserDataTable.Rows[0]; } }

        

        /// <summary>
        /// 用户信息Json
        /// </summary>
        public string UserConfig { get; set; }

        #region 判断角色
        /// <summary>
        /// 是否项目总监
        /// </summary>
        public bool IsXMZJ
        {
            get
            {
                if (UserRoles == null || UserRoles.Count <= 0) return false;
                return UserRoles.Exists(p => p.RoleName.Equals("项目总监"));
            }
        }
        /// <summary>
        /// 是否项目经理
        /// </summary>
        public bool IsXMJL
        {
            get
            {
                if (UserRoles == null || UserRoles.Count <= 0) return false;
                return UserRoles.Exists(p => p.RoleName.Equals("项目经理"));
            }
        }
        /// <summary>
        /// 是否任务经理
        /// </summary>
        public bool IsRWJL
        {
            get
            {
                if (UserRoles == null || UserRoles.Count <= 0) return false;
                return UserRoles.Exists(p => p.RoleName.Equals("任务经理"));
            }
        }
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSupperAdmin
        {
            get
            {
                if (IsSupper) return true;
                if (UserRoles == null || UserRoles.Count <= 0) return false;
                return UserRoles.Exists(p => p.RoleName.Equals("超级管理员"));
            }
        }
        /// <summary>
        /// 是否总经理
        /// </summary>
        public bool IsZJL
        {
            get
            {
                if (UserRoles == null || UserRoles.Count <= 0) return false;
                return UserRoles.Exists(p => p.RoleName.Equals("总经理"));
            }
        }
        /// <summary>
        /// 是否拥有角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool HasRole(string role)
        {
            if (UserRoles == null || UserRoles.Count <= 0) return false;
            return UserRoles.Exists(p => p.RoleName.Equals(role));
        }
        #endregion
    }
}
