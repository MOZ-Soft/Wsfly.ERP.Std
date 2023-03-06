using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.Net;
using System.IO;
using Wsfly.ERP.Std.AppCode.Modules;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// 回调委托
    /// </summary>
    /// <param name="hasNewVersion">是否有新版本</param>
    /// <param name="newVersion">新版本号</param>
    public delegate void Callback_Delegate(bool hasNewVersion, string newVersion = null, bool autoUpgrade = false);

    /// <summary>
    /// 在线升级助手
    /// </summary>
    public class UpgradeHandler
    {
        /// <summary>
        /// 是否有新版本回调
        /// </summary>
        public event Callback_Delegate CallBack_Event;

        /// <summary>
        /// 升级忽略的配置
        /// </summary>
        private static UpgradeFilterConfig _upgradeFilterConfig = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UpgradeHandler()
        {

        }
        /// <summary>
        /// 检查更新
        /// </summary>
        public void Start()
        {
            try
            {
                Thread thread = new Thread(AppUpgrae);
                thread.IsBackground = true;
                thread.Start();
            }
            catch { }
        }
        /// <summary>
        /// 程序更新
        /// </summary>
        private void AppUpgrae()
        {
            try
            {
                //检查升级程序是否需要更新
                CheckUpgrade();
            }
            catch (Exception ex) { AppLog.WriteBugLog(ex, "检查升级程序是否需要更新异常"); }

            //不更新
            string notupgradePath = AppDomain.CurrentDomain.BaseDirectory + "DonotUpgrade.config";
            if (File.Exists(notupgradePath))
            {
                //ppLog.WriteDebugLog("客户端不需要更新！");
                return;
            }

            try
            {
                //检查主程序是否需要更新
                CheckApp();
            }
            catch (System.Threading.ThreadAbortException ex) { return; }
            catch (Exception ex) { AppLog.WriteBugLog(ex, "检查主程序是否需要更新异常"); }
        }
        /// <summary>
        /// 检查升级程序
        /// </summary>
        public static void CheckUpgrade()
        {
            /*
                <?xml version="1.0" encoding="utf-8" ?>
                <LastVersion  xmlns="http://www.wsfly.com/">
                    <ApplicationVersion>1.0.0.1</ApplicationVersion>
                    <ApplicationUrl>http://upgrade.wsfly.com/MOZERP-Std/_Upgrade/Upgrade.exe</ApplicationUrl>
                </LastVersion>
            */
            //升级地址
            string url = AppGlobal.UpgradeUrl;
            url = url.ToLower().Replace("/app.xml", "/_Upgrade/upgrade.xml");

            //升级文件配置
            XmlDocument xmlUpgradeInfo = GetXML(url);
            if (xmlUpgradeInfo == null) return;

            //升级程序
            string currentVersion = "1.0.0.0";
            string upgradePath = AppDomain.CurrentDomain.BaseDirectory + "Upgrade.exe";
            if (System.IO.File.Exists(upgradePath))
            {
                //程序信息
                System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(upgradePath);
                currentVersion = info.FileVersion;
            }

            //最新版本
            string newVersion = xmlUpgradeInfo.DocumentElement["ApplicationVersion"].InnerText;
            string path = xmlUpgradeInfo.DocumentElement["ApplicationUrl"].InnerText;

            //版本号对比
            string currentVersionNumber = currentVersion.Replace(".", "");
            string newVersionNumber = newVersion.Replace(".", "");

            //没有新的版本
            if (Int(newVersionNumber, 0) <= Int(currentVersionNumber, 0)) return;

            //有新的版本 下载升级程序
            //开始下载
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(path);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream st = response.GetResponseStream();

            //写入文件
            Stream so = new FileStream(upgradePath, System.IO.FileMode.Create);
            byte[] by = new byte[1024];
            int osize = st.Read(by, 0, (int)by.Length);
            //循环字节下载
            while (osize > 0)
            {
                so.Write(by, 0, osize);
                osize = st.Read(by, 0, (int)by.Length);
            }
            //关闭写入文件
            so.Close();
            //关闭数据流
            st.Close();
            //关闭请求
            response.Close();
        }
        /// <summary>
        /// 检测主程序
        /// </summary>
        private void CheckApp()
        {
            //程序XML信息
            string appXml = "";
            if (string.IsNullOrWhiteSpace(appXml))
            {
                //没有更新版本
                if (CallBack_Event != null) CallBack_Event(false);
                //结束线程
                //Thread.CurrentThread.Abort();
                return;
            }

            //加载XML
            XmlDocument xmlAppInfo = new XmlDocument();
            xmlAppInfo.LoadXml(appXml);

            if (xmlAppInfo == null)
            {
                //没有更新版本
                if (CallBack_Event != null) CallBack_Event(false);
                //结束线程
                //Thread.CurrentThread.Abort();
                return;
            }

            //当前版本
            string currentVersion = AppGlobal.LocalConfig.Version;

            //版本格式 1.0.01
            Regex regex = new Regex(@"\d+\.\d\.\d{2}");

            //是否自动升级
            bool autoUpgrade = false;

            //最新版本
            string newVersion = xmlAppInfo.DocumentElement["ApplicationVersion"].InnerText;
            //是否符合版本格式
            if (!regex.IsMatch(newVersion))
            {
                //没有更新版本
                if (CallBack_Event != null) CallBack_Event(false);
                //结束线程
                //Thread.CurrentThread.Abort();
                return;
            }

            //版本号对比
            string currentVersionNumber = currentVersion.Replace(".", "");
            string newVersionNumber = newVersion.Replace(".", "");

            //是否比当前版本新
            if (Int(newVersionNumber, 0) <= Int(currentVersionNumber, 0))
            {
                //没有更新版本
                if (CallBack_Event != null) CallBack_Event(false);
                //结束线程
                //Thread.CurrentThread.Abort();
                return;
            }

            try
            {
                //是否自动升级
                autoUpgrade = DataType.Bool(xmlAppInfo.DocumentElement["ApplicationAutoUpgrade"].InnerText, false);
            }
            catch { }
            
            //有新的版本 且 有事件
            if (CallBack_Event != null)
            {
                CallBack_Event(true, newVersion, autoUpgrade);
            }
        }
        
        /// <summary>
        /// 检查是否有新版本
        /// </summary>
        public static bool CheckUpgradeHasNewVersion(ref string newVersion)
        {
            //加载XML
            XmlDocument xmlAppInfo = GetXML(AppGlobal.UpgradeUrl);
            if (xmlAppInfo == null) return false;

            //当前版本
            string currentVersion = AppGlobal.LocalConfig.Version;
            
            //版本格式 1.0.01
            Regex regex = new Regex(@"\d+\.\d\.\d{2}");
            
            //最新版本
            newVersion = xmlAppInfo.DocumentElement["ApplicationVersion"].InnerText;
            //是否符合版本格式
            if (!regex.IsMatch(newVersion)) return false;

            //版本号对比
            string currentVersionNumber = currentVersion.Replace(".", "");
            string newVersionNumber = newVersion.Replace(".", "");

            //是否比当前版本新
            if (Int(newVersionNumber, 0) <= Int(currentVersionNumber, 0)) return false;

            return true;
        }

        #region 辅助函数
        /// <summary>
        /// 是否可以连接到服务器
        /// </summary>
        /// <returns></returns>
        public bool CanConnectService
        {
            get
            {
                System.Net.NetworkInformation.Ping ping;
                System.Net.NetworkInformation.PingReply res;

                ping = new System.Net.NetworkInformation.Ping();
                try
                {
                    //服务器名称
                    string serverName = "wsfly.com";
                    res = ping.Send(serverName);
                    if (res.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch (Exception) { }

                return false;
            }
        }
        /// <summary>
        /// 通过url请求XML数据
        /// 使用：【HttpWebRequest】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <returns>返回XmlDocument</returns>
        public static XmlDocument GetXML(string url)
        {
            const string sAccept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            const string sUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            const string sContentType = "application/x-www-form-urlencoded";

            XmlDocument xml = new XmlDocument();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;

                HttpWebResponse WebResponse = (HttpWebResponse)webRequest.GetResponse();
                System.IO.Stream respStream = WebResponse.GetResponseStream();

                xml.Load(respStream);

                respStream.Dispose();
                respStream.Close();

                WebResponse.Close();
            }
            catch (System.Exception)
            {
                return null;
            }

            return xml;
        }
        /// <summary>
        /// 整型 Int32
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int Int(object value, int defaultValue)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch { return defaultValue; }
        }
        /// <summary>
        /// 转换为Double
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double Double(object value, double defaultValue)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch { return defaultValue; }
        }
        #endregion
    }
}
