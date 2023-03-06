using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Inc
{
    /// <summary>
    /// 收银系统配置
    /// </summary>
    public class CashierConfig
    {
        /// <summary>
        /// 保存锁
        /// </summary>
        private static readonly object _lockSaveConfig = new object();
        /// <summary>
        /// 操作锁
        /// </summary>
        private static readonly object _lockOperateConfig = new object();
        /// <summary>
        /// 配置文件名称
        /// </summary>
        private static readonly string _configFileName = "Cnf/Wsfly.App.Cashier.config";
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static readonly string _configPath = AppDomain.CurrentDomain.BaseDirectory + _configFileName;
        /// <summary>
        /// Parameters
        /// </summary>
        private static Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        private static bool _isEncrypt = true;



        /// <summary>
        /// 构造
        /// </summary>
        private CashierConfig()
        {
        }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (Parameters == null)
            {
                //加载本地配置
                Load();
            }

            //是否存在参数配置
            if (Parameters == null) return null;

            //是否存在配置值
            if (Parameters.ContainsKey(key)) return Parameters[key];

            return null;
        }
        /// <summary>
        /// 获取配置值-字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            object result = Get(key);
            if (result == null) return null;

            return result.ToString();
        }
        /// <summary>
        /// 获取配置值-整数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetInt(string key)
        {
            object result = Get(key);
            if (result == null) return 0;

            return DataType.Int(result, 0);
        }
        /// <summary>
        /// 获取配置值-长整数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long GetLong(string key)
        {
            object result = Get(key);
            if (result == null) return 0;

            return DataType.Long(result, 0);
        }
        /// <summary>
        /// 获取配置值-Boolean
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBoolean(string key)
        {
            object result = Get(key);
            if (result == null) return false;

            return DataType.Bool(result, false);
        }
        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, object value, bool autoSave = false)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            lock (_lockOperateConfig)
            {
                if (Parameters == null)
                {
                    //加载本地配置
                    Load();
                }

                //是否存在参数配置
                if (Parameters == null) Parameters = new Dictionary<string, object>();

                //是否已经有配置键
                if (Parameters.ContainsKey(key)) Parameters[key] = value;
                //添加新配置键值
                else Parameters.Add(key, value);

                //是否自动保存配置
                if (autoSave) Save();
            }
        }
        /// <summary>
        /// 删除键值
        /// </summary>
        /// <param name="key"></param>
        public static void Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            lock (_lockOperateConfig)
            {
                //是否存在参数配置
                if (Parameters == null) return;

                //是否已经有配置键
                if (Parameters.ContainsKey(key)) Parameters.Remove(key);
            }
        }


        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        private static void Load()
        {
            bool isLoaded = false;

            try
            {
                //文件是否存在
                if (File.Exists(_configPath))
                {
                    //读取配置文件
                    string jsonData = File.ReadAllText(_configPath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        //写入备份文件
                        File.WriteAllText(_configPath + ".bak", jsonData, Encoding.UTF8);
                        //是否解密
                        if (_isEncrypt) jsonData = Core.Encryption.EncryptionAES.Decrypt(jsonData);
                        //转换为键值集合
                        Parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
                        //加载成功
                        isLoaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "加载配置文件失败：" + _configFileName);
            }

            try
            {
                //未加载成功且备份文件存在 尝试从备份文件读取配置
                if (!isLoaded && File.Exists(_configPath + ".bak"))
                {
                    string jsonData = File.ReadAllText(_configPath + ".bak", Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        //是否解密
                        if (_isEncrypt) jsonData = Core.Encryption.EncryptionAES.Decrypt(jsonData);
                        //转换为键值集合
                        Parameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "加载配置备份文件失败：" + _configFileName);
            }

            try
            {
                //是否有参数
                if (Parameters == null || Parameters.Count <= 0)
                {
                    //添加一个参数示例
                    Parameters = new Dictionary<string, object>();
                    Parameters.Add("PoweredBy", "wsfly.com");

                    //保存新文件
                    Save();

                    AppLog.WriteDebugLog("缺少配置文件：" + _configFileName);
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteBugLog(ex, "没有配置文件或内容，初始默认配置失败：" + _configFileName);
            }
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save()
        {
            lock (_lockSaveConfig)
            {
                try
                {
                    //保存配置
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(Parameters);
                    //是否需要加密
                    if (_isEncrypt) jsonData = Core.Encryption.EncryptionAES.Encrypt(jsonData);
                    //写入配置文件
                    System.IO.File.WriteAllText(_configPath, jsonData, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "保存配置文件失败：" + _configFileName);
                }
            }
        }
    }
}