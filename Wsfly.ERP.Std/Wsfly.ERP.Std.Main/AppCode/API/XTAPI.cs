using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wsfly.ERP.Std.AppCode.Handler;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.AppCode.API
{
    public class XTAPI
    {
        /// <summary>
        /// API域名
        /// </summary>
        private static string _APIDomain = "http://erp.wsing.cn/";

        /// <summary>
        /// 服务注册
        /// </summary>
        /// <param name="serialNo"></param>
        /// <param name="qymc"></param>
        /// <param name="lxdh"></param>
        /// <returns></returns>
        public static APIResult Regist(string qymc, string lxdh)
        {
            try
            {
                string xlh = Guid.NewGuid().ToString().ToUpper();
                string token = GetServerToken(xlh);

                string appName = AppGlobal.GetSysConfigReturnString("System_AppName");
                string appVersion = AppGlobal.GetSysConfigReturnString("System_AppVersion");

                //提交注册
                Dictionary<string, string> dicData = new Dictionary<string, string>()
                {
                    { "XLH",  xlh},
                    { "QYMC", qymc},
                    { "LXDH", lxdh},
                    { "TOKEN", token },
                    { "SJKMC", "Wsfly.DC.ERP(Sqlite)" },
                    { "XTBH", "Wsfly.ERP.Std"},
                    { "AppName", appName},
                    { "AppVersion", appVersion}
                };
                string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);

                //提交数据
                System.Net.WebClient wc = new System.Net.WebClient();
                var resultBytes = wc.UploadData(_APIDomain + "API/XT_Regist", Encoding.UTF8.GetBytes(dataJson));
                string resultJson = Encoding.UTF8.GetString(resultBytes);
                APIResult apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(resultJson);
                if (apiResult != null && apiResult.Success)
                {
                    Dictionary<string, string> dicRegister = new Dictionary<string, string>();
                    dicRegister.Add("SerialNumber", xlh);
                    dicRegister.Add("Name", qymc);
                    dicRegister.Add("Telphone", lxdh);
                    dicRegister.Add("RegDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    bool flag = AppGlobal.SetRegisterInfo(dicRegister);
                    if (flag)
                    {
                        return new APIResult()
                        {
                            Success = true
                        };
                    }
                    else
                    {
                        return new APIResult()
                        {
                            Success = false,
                            Message = "提交注册资料成功，但更新本地配置失败！"
                        };
                    }
                }
            }
            catch (Exception ex) { }

            return new APIResult()
            {
                Success = false,
                Message = "注册失败！"
            };
        }
        /// <summary>
        /// 登陆登记
        /// </summary>
        /// <param name="dicRegisterInfo"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static APIResult Login(Dictionary<string, string> dicRegisterInfo, UserInfo userInfo)
        {
            try
            {
                if (dicRegisterInfo == null || !dicRegisterInfo.ContainsKey("SerialNumber")) return null;

                string xlh = dicRegisterInfo["SerialNumber"];
                string qymc = dicRegisterInfo["Name"];
                if (string.IsNullOrWhiteSpace(xlh)) return null;

                Dictionary<string, object> dicData = new Dictionary<string, object>()
                {
                    { "XLH", xlh },
                    { "QYMC", qymc },
                    { "YHMC", userInfo.UserName },
                    { "YHID", userInfo.Id },
                    { "KHDLX", "PC" },
                };
                string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);

                //提交数据
                System.Net.WebClient wc = new System.Net.WebClient();
                var resultBytes = wc.UploadData(_APIDomain + "API/XT_Login", Encoding.UTF8.GetBytes(dataJson));
                string resultJson = Encoding.UTF8.GetString(resultBytes);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(resultJson);
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 验证序列号
        /// </summary>
        /// <returns></returns>
        public static APIResult CheckXLH()
        {
            try
            {
                //注册信息
                var dicRegisterInfo = AppGlobal.GetRegisterInfo();
                if (dicRegisterInfo != null && dicRegisterInfo.ContainsKey("SerialNumber"))
                {
                    //序列号
                    string xlh = dicRegisterInfo["SerialNumber"];
                    if (!string.IsNullOrWhiteSpace(xlh))
                    {
                        //TOKEN
                        string token = GetServerToken(xlh);
                        if (string.IsNullOrWhiteSpace(token)) return null;

                        Dictionary<string, object> dicData = new Dictionary<string, object>()
                        {
                            { "XLH", xlh },
                            { "TOKEN", token },
                        };
                        string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);

                        //提交数据
                        System.Net.WebClient wc = new System.Net.WebClient();
                        var resultBytes = wc.UploadData(_APIDomain + "API/XT_CheckXLHAndToken", Encoding.UTF8.GetBytes(dataJson));
                        string resultJson = Encoding.UTF8.GetString(resultBytes);
                        var apiResult = Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(resultJson);
                        if (apiResult != null && apiResult.Success)
                        {
                            AppGlobal._SFZC = true;

                            try { AppGlobal._FWDQRQ = Convert.ToDateTime(apiResult.Data); }
                            catch { }
                        }

                        return apiResult;
                    }
                }
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 服务端在线
        /// </summary>
        /// <returns></returns>
        public static void ServerOnline()
        {
            //线程设置终端在线
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                //注册信息
                var dicRegisterInfo = AppGlobal.GetRegisterInfo();
                if (dicRegisterInfo != null && dicRegisterInfo.ContainsKey("SerialNumber"))
                {
                    //序列号
                    string xlh = dicRegisterInfo["SerialNumber"];
                    if (!string.IsNullOrWhiteSpace(xlh))
                    {
                        try
                        {
                            //TOKEN
                            string token = GetServerToken(xlh);
                            if (string.IsNullOrWhiteSpace(token)) return;

                            Dictionary<string, object> dicData = new Dictionary<string, object>()
                            {
                                { "XLH", xlh },
                                { "TOKEN", token },
                            };
                            string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);

                            //提交数据
                            System.Net.WebClient wc = new System.Net.WebClient();
                            var resultBytes = wc.UploadData(_APIDomain + "API/XT_CheckServerXLH", Encoding.UTF8.GetBytes(dataJson));
                            string resultJson = Encoding.UTF8.GetString(resultBytes);
                        }
                        catch (Exception ex) { }
                    }
                }
            });
            thread.Start();
        }
        /// <summary>
        /// 保持用户在线状态
        /// </summary>
        public static void KeepUserOnline()
        {
            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    if (AppGlobal.UserInfo == null) return;
                    if (AppGlobal.GetRegisterInfo() == null) return;

                    var dicRegisterInfo = AppGlobal.GetRegisterInfo();
                    if (dicRegisterInfo == null || !dicRegisterInfo.ContainsKey("SerialNumber")) return;

                    string xlh = dicRegisterInfo["SerialNumber"];
                    string qymc = dicRegisterInfo["Name"];

                    if (string.IsNullOrWhiteSpace(xlh)) return;

                    try
                    {
                        Dictionary<string, object> dicData = new Dictionary<string, object>()
                        {
                            { "XLH", xlh },
                            { "QYMC", qymc },
                            { "YHMC", AppGlobal.UserInfo.UserName },
                            { "YHID", AppGlobal.UserInfo.Id },
                            { "KHDLX", "PC" },
                        };
                        string dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);

                        //提交数据
                        System.Net.WebClient wc = new System.Net.WebClient();
                        var resultBytes = wc.UploadData(_APIDomain + "API/XT_Heartbeat", Encoding.UTF8.GetBytes(dataJson));
                        string resultJson = Encoding.UTF8.GetString(resultBytes);
                    }
                    catch (Exception ex) { }
                }
                catch (Exception ex) { }
            });
            thread.Start();
        }

        /// <summary>
        /// 获取服务器Token
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static string GetServerToken(string serialNo)
        {
            try
            {
                string cpuId = PCHandler.GetPCID("Win32_Processor");
                string boardId = PCHandler.GetPCID("Win32_BaseBoard");

                string toEncrypt = cpuId + "||" + boardId + "||" + serialNo;
                return AppCode.API.APIEncrypt.AESEncrypt(toEncrypt, serialNo);
            }
            catch (Exception ex) { }

            return string.Empty;
        }
    }

    /// <summary>
    /// API返回结果
    /// </summary>
    [Serializable]
    public class APIResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; }
    }
}
