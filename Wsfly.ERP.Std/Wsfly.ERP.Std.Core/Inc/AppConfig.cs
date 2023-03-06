using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Wsfly.ERP.Std.Core.Models;
using Wsfly.ERP.Std.Core.Handler;

namespace Wsfly.ERP.Std.Core
{
    /// <summary>
    /// 程序基础配置
    /// </summary>
    [Serializable]
    public partial class AppBaesConfig
    {
        #region 属性
        /// <summary>
        /// 保存目录
        /// </summary>
        public static string _DirName = AppDomain.CurrentDomain.BaseDirectory + "Cnf\\";
        /// <summary>
        /// 配置文件
        /// </summary>
        public static string _ConfigPath = _DirName + "Wsfly.App.config";
        /// <summary>
        /// 配置备份文件
        /// </summary>
        public static string _ConfigBakPath = _DirName + "Wsfly.App.config.bak";

        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsFullScreen { get; set; }
        /// <summary>
        /// 是否调试
        /// </summary>
        public bool IsDebug { get; set; }
        
        /// <summary>
        /// 更新地址
        /// </summary>
        public string UpgradeUrl { get; set; }
        /// <summary>
        /// 程序版本号
        /// </summary>
        public string Version { get; set; }
        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        public AppBaesConfig()
        {
            IsFullScreen = true;
            IsDebug = false;
            UpgradeUrl = "http://upgrade.wsfly.com/MZERP-Client/App.xml";
            Version = "1.0.0";
        }

        /// <summary>
        /// 初始化 加载配置
        /// </summary>
        /// <param name="path"></param>
        public AppBaesConfig(string path)
        {
            try
            {
                //初始默认值
                AppBaesConfig config = AppBaesConfig.LoadBaseConfig(path);

                //得到值
                this.IsFullScreen = config.IsFullScreen;
                this.IsDebug = config.IsDebug;
                this.Version = config.Version;
            }
            catch { }
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        public static AppBaesConfig LoadBaseConfig(string path = null)
        {
            //初始默认值
            AppBaesConfig config = new AppBaesConfig();

            //没有路径
            if (string.IsNullOrWhiteSpace(path)) path = _ConfigPath;

            //文件是否存在
            if (File.Exists(path))
            {
                XmlHandler xmlHandler = new XmlHandler(path);

                config.IsFullScreen = DataType.Bool(xmlHandler.GetNodeText("IsFullScreen"), false);
                config.IsDebug = DataType.Bool(xmlHandler.GetNodeText("IsDebug"), false);                                
                config.Version = xmlHandler.GetNodeText("Version");
            }

            //返回配置
            return config;
        }
        #endregion
    }
}
