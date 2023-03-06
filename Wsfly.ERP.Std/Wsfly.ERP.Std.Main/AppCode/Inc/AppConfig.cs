using Wsfly.ERP.Std.Core.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// 本地配置
/// </summary>
public class AppConfig : Wsfly.ERP.Std.Core.AppBaesConfig
{
    /// <summary>
    /// 保存锁
    /// </summary>
    public static readonly object _lockSaveConfig = new object();
    /// <summary>
    /// 自动启动
    /// </summary>
    public bool AutoRun { get; set; }
    /// <summary>
    /// 用户帐号
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// 用户密码
    /// </summary>
    public string UserPwd { get; set; }
    /// <summary>
    /// 是否记住密码
    /// </summary>
    public bool RememberPwd { get; set; }
    /// <summary>
    /// 启动后第一时间打开的窗口
    /// </summary>
    public string FirstRunFullWindow { get; set; }
    /// <summary>
    /// 登陆后第一时间打开窗口
    /// </summary>
    public string LoginRunFullWindow { get; set; }
    /// <summary>
    /// 导航放大倍数
    /// </summary>
    public double NavIconZoom { get; set; }

    /// <summary>
    /// 构造
    /// </summary>
    public AppConfig()
    {
        UserName = "";
        UserPwd = "";
        RememberPwd = false;
        AutoRun = false;
        IsFullScreen = false;
        FirstRunFullWindow = "";
        LoginRunFullWindow = "";
        NavIconZoom = 1;
    }


    /// <summary>
    /// 加载配置
    /// </summary>
    /// <returns></returns>
    public static AppConfig Load()
    {
        //初始默认值
        AppConfig config = new AppConfig();

        //没有路径
        string path = _ConfigPath;

        try
        {
            //文件是否存在
            if (File.Exists(path))
            {
                //是否有内容
                long fileSize = new FileInfo(path).Length;
                if (fileSize == 0) return LoadBakupConfig(path);

                //读取配置文件
                string configText = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(configText.Trim())) return LoadBakupConfig(path);
                if (configText.StartsWith("\0\0\0\0") && configText.EndsWith("\0\0\0\0")) return LoadBakupConfig(path);

                //读取配置文件 并转为配置对象
                config = XmlSerializerHandler<AppConfig>.ToObject(path);
                //写入备份文件
                File.WriteAllText(_ConfigPath + ".bak", configText);

                //返回配置
                return config;
            }

            //尝试从配置备份文件加载
            return LoadBakupConfig(path);
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "加载配置文件/Cnf/Wsfly.App.config失败");
        }

        //返回配置
        return config;
    }
    /// <summary>
    /// 保存配置
    /// </summary>
    public void Save()
    {
        lock (_lockSaveConfig)
        {
            try
            {
                //创建目录
                if (!Directory.Exists(_DirName)) Directory.CreateDirectory(_DirName);

                //保存为XML格式
                XmlSerializerHandler<AppConfig>.ToXml(this, _ConfigPath);
            }
            catch { }
        }
    }
    /// <summary>
    /// 读取配置备份文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static AppConfig LoadBakupConfig(string path)
    {
        try
        {
            //查看是否有备份文件
            if (File.Exists(path + ".bak"))
            {
                string configText = File.ReadAllText(path + ".bak", Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(configText))
                {
                    //读取配置备份文件 并转为配置对象
                    return XmlSerializerHandler<AppConfig>.ToObject(path + ".bak");
                }
            }
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "从配置备份文件加载异常");
        }

        AppLog.WriteDebugLog("配置文件不存在或为空，已重置配置内容；若连接异常，请联系管理员修正配置！");
        return FixConfigs(path);
    }
    /// <summary>
    /// 修复配置并保存
    /// </summary>
    /// <param name="path"></param>
    private static AppConfig FixConfigs(string path)
    {
        AppConfig config = null;

        try
        {
            config = new AppConfig()
            {
                IsFullScreen = false,
                IsDebug = false,
                UpgradeUrl = "http://upgrade.wsfly.com/MZERP-Client/App.xml",
                Version = "1.0.0",
                AutoRun = false,
                UserName = "",
                UserPwd = "",
                RememberPwd = true,
                FirstRunFullWindow = "",
                LoginRunFullWindow = ""
            };

            //修复配置 并保存
            config.Save();
        }
        catch (Exception ex)
        {
            AppLog.WriteBugLog(ex, "修正配置并保存异常！");
        }

        return config;
    }
}
