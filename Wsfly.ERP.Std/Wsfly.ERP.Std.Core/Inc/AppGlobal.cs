using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Wsfly.ERP.Std.Core.Encryption;
using Wsfly.ERP.Std.Core.Handler;
using Wsfly.ERP.Std.Core.Models.Sys;

/// <summary>
/// 全局函数
/// </summary>
public partial class AppBaseGlobal
{
    /// <summary>
    /// 用户配置
    /// </summary>
    public static UserConfig UserConfig { get; set; }
    /// <summary>
    /// 用户信息
    /// </summary>
    public static UserInfo UserInfo { get; set; }
    /// <summary>
    /// 用户信息
    /// </summary>
    public static System.Data.DataRow UserDataRow { get; set; }
    
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ipOrDomain">IP地址或域名</param>
    /// <returns>是否连接服务器成功</returns>
    public static bool ConnectServer(string ipOrDomain)
    {
        System.Net.NetworkInformation.Ping ping;
        System.Net.NetworkInformation.PingReply res;

        //是否可以Ping到服务器
        ping = new System.Net.NetworkInformation.Ping();

        try
        {
            //服务器IP或域名
            ipOrDomain = ipOrDomain.ToLower().Replace("http://", "").Trim('/');
            //发送Ping命令
            res = ping.Send(ipOrDomain);
            //结果状态：是否成功
            if (res.Status == System.Net.NetworkInformation.IPStatus.Success) return true;
        }
        catch (Exception) { }

        return false;
    }
    /// <summary>
    /// 计算总页数
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="pageSize">页码</param>
    /// <returns>页数</returns>
    public static int TotalPageCount(long totalCount, int pageSize)
    {
        if (totalCount <= 0) return 0;
        return Convert.ToInt32((totalCount + pageSize - 1) / pageSize);
    }
    /// <summary>
    /// 打开浏览器
    /// </summary>
    /// <param name="url"></param>
    public static void OpenBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        System.Diagnostics.Process.Start(url);
    }

    #region 生成传递Sign
    /// <summary>
    /// 生成Sign
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string BuildSign(string key)
    {
        //Sign格式：key|datetime
        //如：9a7ab98c-b24b-4df0-83fd-7ff513e04b7d|2016-05-05 12:00:00

        //是否有Key
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        //生成格式
        string sign = key + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //加密内容
        sign = EncryptionDES.Encrypt(sign);
        //传递Url编码
        sign = StringHandler.EncodeUrlChar(sign);
        //返回
        return sign;
    }
    /// <summary>
    /// 是否有效的Sign
    /// </summary>
    /// <param name="sign"></param>
    /// <returns></returns>
    public static bool IsValidSign(string sign, string realKey)
    {
        //Sign格式：key|datetime
        //如：9a7ab98c-b24b-4df0-83fd-7ff513e04b7d|2016-05-05 12:00:00

        try
        {
            //Url解码
            sign = StringHandler.DecodeUrlChar(sign);
            //解密内容
            sign = EncryptionDES.Decrypt(sign);
        }
        catch { }

        try
        {
            //解密为空
            if (string.IsNullOrWhiteSpace(sign)) return false;
            string[] paras = sign.Split('|');
            if (paras.Length < 2) return false;

            string key = paras[0];
            string dt = paras[1];

            //是否有值
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (string.IsNullOrWhiteSpace(dt)) return false;

            //判断时间是否超过10分钟
            DateTime date = DataType.DateTime(dt, DateTime.Now.AddDays(-1));
            if ((DateTime.Now - date).TotalMinutes > 10) return false;

            //Key是否正确
            if (key.Equals(realKey)) return true;
        }
        catch { }

        return false;
    }
    #endregion
}

