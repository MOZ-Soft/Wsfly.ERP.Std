using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Windows.Forms;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    public class Constant
    {
        /// <summary>
        /// 储存数据库连接字符串
        /// </summary>
        private static Dictionary<string, string> _ConnectionStrings = null;
        /// <summary>
        /// 锁对象
        /// </summary>
        private static object lockObject = new object();

        /// <summary>
        /// 单实例模式
        /// </summary>
        private Constant()
        {
            ///加载数据库连接
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Cnf\\Wsfly.ConnectionStrings.config";
            ///去掉空路径
            path = path.Replace("//", "/");
            ///加载配置
            XmlDocument _xmlConnectionStrings = new XmlDocument();
            _xmlConnectionStrings.Load(path);
            //所有配置
            XmlNodeList xnl = _xmlConnectionStrings.DocumentElement.ChildNodes;
            //没有配置
            if (xnl == null || xnl.Count <= 0) return;

            ///清空配置
            _ConnectionStrings = new Dictionary<string, string>();

            //添加到网站配置
            foreach (XmlNode node in xnl)
            {
                string key = node.Attributes["name"].Value;
                string value = node.Attributes["connectionString"].Value;
                bool encode = DataType.Bool(node.Attributes["encode"].Value, true);

                if (!string.IsNullOrEmpty(value) && value.IndexOf("|DataDirectory|") >= 0)
                {
                    value = value.Replace("|DataDirectory|", AppDomain.CurrentDomain.BaseDirectory);
                }

                ///添加数据库连接到库
                _ConnectionStrings.Add(key, value);
            }
        }
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetConnectionString(string name)
        {
            lock (lockObject)
            {
                if (_ConnectionStrings == null || _ConnectionStrings.Count <= 0 || !_ConnectionStrings.ContainsKey(name)) new Constant();
                if (!_ConnectionStrings.ContainsKey(name)) return null;
                return _ConnectionStrings[name];
            }
        }


        #region 扩展数据库连接
        /// <summary>
        /// 获取默认数据库连接
        /// </summary>
        public static string GetDefaultConnectionString
        {
            get { return GetConnectionString("DefaultConnectionString"); }
        }
        #endregion


    }
}
