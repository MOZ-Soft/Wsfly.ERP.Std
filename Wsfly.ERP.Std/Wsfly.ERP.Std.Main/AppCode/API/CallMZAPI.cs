using Wsfly.ERP.Std.Core.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wsfly.ERP.Std.AppCode.API
{
    /// <summary>
    /// 调用API
    /// </summary>
    public class CallMZAPI
    {
        /// <summary>
        /// 提交委托
        /// </summary>
        /// <param name="postJosn"></param>
        public delegate void PostDelegate(string postJosn);
        /// <summary>
        /// 提交事件
        /// </summary>
        public static event PostDelegate PostEvent;

        /// <summary>
        /// MZAPI-APPDomain
        /// </summary>
        private static string MZAPIDomain { get { return AppGlobal.GetSysConfigReturnString("System_MZAPI_Domain"); } }
        /// <summary>
        /// MZAPI-APPID
        /// </summary>
        private static string MZAPIAPPID { get { return AppGlobal.GetSysConfigReturnString("System_MZAPI_APPID"); } }
        /// <summary>
        /// MZAPI-APPKEY
        /// </summary>
        private static string MZAPIAPPKEY { get { return AppGlobal.GetSysConfigReturnString("System_MZAPI_APIKEY"); } }


        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="data"></param>
        /// <param name="security"></param>
        /// <returns></returns>
        public static ReceiveJson Post(string apiName, Dictionary<string, object> data, Security security = Security.None)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            return Post(apiName, json, security);
        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="data"></param>
        /// <param name="security"></param>
        /// <returns></returns>
        public static ReceiveJson Post(string apiName, string data, Security security = Security.None)
        {
            try
            {
                //提交地址
                string postUrl = GetAPIUrl(apiName);
                string appid = MZAPIAPPID;
                string privateKey = MZAPIAPPKEY;

                if (string.IsNullOrWhiteSpace(postUrl) ||
                    string.IsNullOrWhiteSpace(postUrl) ||
                    string.IsNullOrWhiteSpace(postUrl))
                {
                    throw new Exception("MZAPI未正确配置！");
                }

                //时间戳
                string ts = GetTimeSpan();
                //签名
                string sign = "";
                if (security == Security.RSA) sign = Encrypt(data + "," + ts, security, privateKey);
                else sign = Encrypt(privateKey + "," + ts, security, privateKey, true);

                //数据加密
                if (!string.IsNullOrWhiteSpace(data)) data = Encrypt(data, security, privateKey);

                //要提交的Json数据
                PostJson postJson = new PostJson()
                {
                    data = data,
                    appid = appid,
                    security = security.ToString().ToLower(),
                    sign = sign,
                    timespan = ts
                };

                //要提交的Json字符串
                string postJsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(postJson);

                //提交数据
                if (PostEvent != null)
                {
                    try { PostEvent(postJsonStr); }
                    catch { }
                }

                //提交数据
                string resultData = PostJsonData(postUrl, postJsonStr);

                //解析并返回结果
                ReceiveJson receiveJson = Newtonsoft.Json.JsonConvert.DeserializeObject<ReceiveJson>(resultData);

                //收到的数据
                string receiveData = receiveJson.data;
                if (!string.IsNullOrWhiteSpace(receiveData))
                {
                    receiveData = Decrypt(receiveData, security, privateKey);
                    receiveJson.data = receiveData;
                }

                if (!string.IsNullOrWhiteSpace(receiveJson.sign))
                {
                    //验证签名
                    string receiveSign = "";
                    if (security.Equals("rsa"))
                    {
                        receiveSign = Decrypt(receiveJson.sign, security, privateKey);
                        if (!receiveSign.Equals(receiveData + "," + receiveJson.timespan))
                        {
                            return new ReceiveJson()
                            {
                                success = false,
                                data = receiveData,
                                message = "验证返回结果的签名失败！"
                            };
                        }
                    }
                    else
                    {
                        receiveSign = Encrypt(privateKey + "," + receiveJson.timespan, security, privateKey, true);
                        if (!receiveSign.Equals(receiveJson.sign))
                        {
                            return new ReceiveJson()
                            {
                                success = false,
                                data = resultData,
                                message = "验证返回结果的签名失败！"
                            };
                        }
                    }
                }

                //返回结果
                return receiveJson;
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "调用MZAPI异常");
            }

            return null;
        }
        /// <summary>
        /// 获取服务器文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Stream GetMZFile(string url)
        {
            try
            {
                url = MZAPIDomain.Trim('/') + "/API/GetFile?url=" + System.Web.HttpUtility.UrlEncode(url);
                return WebHandler.DownloadFile(url);
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetMZFilePath(string url)
        {
            try
            {
                url = MZAPIDomain.Trim('/') + "/API/GetFilePath?url=" + System.Web.HttpUtility.UrlEncode(url);
                string serverUrl = WebHandler.GetHtml(url, Encoding.UTF8);

                return Path.Combine(MZAPIDomain, serverUrl);
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static System.Drawing.Image GetMZImage(string url)
        {
            try
            {
                url = MZAPIDomain.Trim('/') + "/API/GetImage?url=" + System.Web.HttpUtility.UrlEncode(url);
                using (Stream stream = WebHandler.DownloadFile(url))
                {
                    return System.Drawing.Image.FromStream(stream);
                }
            }
            catch { }

            return null;
        }
        /// <summary>
        /// API地址
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        private static string GetAPIUrl(string apiName)
        {
            if (string.IsNullOrWhiteSpace(MZAPIDomain)) return string.Empty;
            return MZAPIDomain.Trim('/') + "/API/" + apiName;
        }
        /// <summary>
        /// 得到时间戳
        /// </summary>
        /// <returns></returns>
        private static string GetTimeSpan()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary> 
        /// 提交Json数据 
        /// </summary> 
        /// <returns></returns> 
        private static string PostJsonData(string url, string jsonData)
        {
            try
            {
                //要的数据 
                byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);
                //新实例 
                System.Net.WebClient webclient = new System.Net.WebClient();
                //上传 
                byte[] buffer = webclient.UploadData(url, "POST", postData);
                //返回数据 
                return System.Text.Encoding.UTF8.GetString(buffer);
            }
            catch (Exception ex) { throw ex; }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source"></param>
        /// <param name="security"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static string Encrypt(string source, Security security, string privateKey, bool isSign = false)
        {
            if (security == Security.RSA)
            {
                return APIEncrypt.RSAEncrypt(source, privateKey);
            }
            else if (security == Security.DES)
            {
                return APIEncrypt.DESEncrypt(source, privateKey);
            }
            else if (security == Security.AES)
            {
                return APIEncrypt.AESEncrypt(source, privateKey);
            }
            else if (isSign)
            {
                return APIEncrypt.MD5Encrypt(source);
            }

            return source;
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="source"></param>
        /// <param name="security"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private static string Decrypt(string source, Security security, string privateKey)
        {
            if (security == Security.RSA)
            {
                return APIEncrypt.RSADecrypt(source, privateKey);
            }
            else if (security == Security.DES)
            {
                return APIEncrypt.DESDecrypt(source, privateKey);
            }
            else if (security == Security.AES)
            {
                return APIEncrypt.AESDecrypt(source, privateKey);
            }

            return source;
        }
    }

    /// <summary>
    /// 提交JSON
    /// </summary>
    [Serializable]
    public class PostJson
    {
        /// <summary>
        /// 提交数据
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// 应用ID
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 安全模式
        /// rsa,des,aes,none
        /// </summary>
        public string security { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string timespan { get; set; }
    }
    /// <summary>
    /// 返回JSON
    /// </summary>
    [Serializable]
    public class ReceiveJson
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string timespan { get; set; }
    }

    /// <summary>
    /// 安全模式
    /// </summary>
    public enum Security
    {
        None,
        RSA,
        DES,
        AES
    }
}
