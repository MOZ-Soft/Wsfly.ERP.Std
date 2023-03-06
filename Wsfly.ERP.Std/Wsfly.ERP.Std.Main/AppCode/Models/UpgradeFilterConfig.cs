using Wsfly.ERP.Std.Core.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wsfly.ERP.Std.AppCode.Modules
{
    /// <summary>
    /// 升级忽略文件配置
    /// </summary>
    public class UpgradeFilterConfig
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        public static string _ConfigPath = AppDomain.CurrentDomain.BaseDirectory + "UpgradeFilter.config";
        /// <summary>
        /// 忽略的文件
        /// </summary>
        public List<UpgradeFilterItem> FilterRegexs { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public UpgradeFilterConfig()
        {
            FilterRegexs = new List<UpgradeFilterItem>();
        }


        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public static UpgradeFilterConfig Load(string path = null)
        {
            //初始默认值
            UpgradeFilterConfig config = new UpgradeFilterConfig();

            //没有路径
            if (string.IsNullOrWhiteSpace(path)) path = _ConfigPath;

            //文件是否存在
            if (File.Exists(path))
            {
                config = XmlSerializerHandler<UpgradeFilterConfig>.ToObject(path);
            }

            //返回配置
            return config;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            try
            {
                //保存为XML格式
                XmlSerializerHandler<UpgradeFilterConfig>.ToXml(this, _ConfigPath);
            }
            catch { }
        }
    }
    /// <summary>
    /// 升级忽略的表达式
    /// </summary>
    public class UpgradeFilterItem
    {
        /// <summary>
        /// 忽略正则表格式
        /// </summary>
        public string FilterRegex { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public UpgradeFilterItem() { }
        /// <summary>
        /// 带参数构造
        /// </summary>
        /// <param name="regex"></param>
        public UpgradeFilterItem(string regex) { FilterRegex = regex; }
    }
}
