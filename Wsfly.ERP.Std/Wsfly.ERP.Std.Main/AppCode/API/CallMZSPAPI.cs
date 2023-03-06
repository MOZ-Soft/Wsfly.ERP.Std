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
    /// 调用官方API
    /// </summary>
    public class CallMZSPAPI
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
        private static string MZAPIDomain = "http://erp.wsfly.com/";
        /// <summary>
        /// MZAPI-APPID
        /// </summary>
        private static string MZAPIAPPID = "100001";
        /// <summary>
        /// MZAPI-APPKEY
        /// </summary>
        private static string MZAPIAPPKEY = "<RSAKeyValue><Modulus>0akU3hJBwul1Ez4KLuSPIqZHhkIOlqTVVagwA/ewpxCcf2jK7Zx/eug/pfzzxjyPyXcIXwv7er4bLGOcqtfL9MnznJ7WppEQfZRWH4mG/bd3gcfOwiZ6tm83PX+9eaP/x0mrqC6utXtRYGIsB0GtRjyOS2HPDmeV0JJ9VS3xVn0=</Modulus><Exponent>AQAB</Exponent><P>+VaZOgzlVMUSO5s0mP/JK1LMbR4vhh5cKNRv2cyTOsVmPoY8wC8T9a0eJL1DE9cbZzW8HEHOeh02TMPxf+TSmQ==</P><Q>10MZK+a6bPBwulbd+psQePamrU+N9w0tldzrsxZv7NFEd3yZVdjLtz+HTH4WQPws/wT+3a3ZgB5R5Nu6GFB1hQ==</Q><DP>KDi8Bw1FgWM5CbyDw5qfjQmSSJfx+qSzITMDyBKkPXrSf4uQCUCO67a9ghe11mGA3ilg6v4CnNhRhhilwIfdIQ==</DP><DQ>QBXqzYYgZERk2yT3ax91FP4heyFfG3jh5GbkCOoaIj/fCU+f+s3TQFf6eMxk5a3t23JqSibyxNDCAsdjrM9vXQ==</DQ><InverseQ>NrBGvctds3JzX138F61gcb7J1l7o4yTB5lpFrYPtKdH4pqFYAsPR6oASZGFskbDbw1bt7NxQmnCnFoY/pO6gZA==</InverseQ><D>gI2j9eZL3C85UesgY1BRU3WRNYkbWNMI29ScXFJrRoSw8I5YnwjuQXZWUHihIFPR9j2+LEk0kTYvgzAwx7A5pWmu8CahTXOm3TRxNGE1wYjhHK7CZhFKMqSp8rjtqujtqk//m5Lx+PbsXJOPf5VStInOAyUtnag/S1HZmDnvbQE=</D></RSAKeyValue>";


        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="data"></param>
        /// <param name="security"></param>
        /// <returns></returns>
        public static ReceiveJson Post(string apiName, Dictionary<string, object> data, string security = "none")
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
        public static ReceiveJson Post(string apiName, string data, string security = "none")
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
                if (security.Equals("rsa")) sign = Encrypt(data + "," + ts, security, privateKey);
                else sign = Encrypt(privateKey + "," + ts, security, privateKey, true);

                //数据加密
                if (!string.IsNullOrWhiteSpace(data)) data = Encrypt(data, security, privateKey);

                //要提交的Json数据
                PostJson postJson = new PostJson()
                {
                    data = data,
                    appid = appid,
                    security = security,
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
        private static string Encrypt(string source, string security, string privateKey, bool isSign = false)
        {
            if (security.Equals("rsa"))
            {
                return APIEncrypt.RSAEncrypt(source, privateKey);
            }
            else if (security.Equals("des"))
            {
                return APIEncrypt.DESEncrypt(source, privateKey);
            }
            else if (security.Equals("aes"))
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
        private static string Decrypt(string source, string security, string privateKey)
        {
            if (security.Equals("rsa"))
            {
                return APIEncrypt.RSADecrypt(source, privateKey);
            }
            else if (security.Equals("des"))
            {
                return APIEncrypt.DESDecrypt(source, privateKey);
            }
            else if (security.Equals("aes"))
            {
                return APIEncrypt.AESDecrypt(source, privateKey);
            }

            return source;
        }
    }
}
