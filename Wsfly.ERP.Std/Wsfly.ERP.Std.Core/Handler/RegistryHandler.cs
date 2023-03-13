using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 注册表辅助
    /// </summary>
    public class RegistryHandler
    {
        /// <summary>
        /// 创建注册表项
        /// 计算机\HKEY_LOCAL_MACHINE\SOFTWARE
        /// </summary>
        /// <param name="regName"></param>
        public static RegistryKey RegistItem(string regName)
        {
            //注册表
            RegistryKey reg = Registry.LocalMachine;
            //得到系统软件目录
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);
            //是否已经存在注册表项
            if (IsRegeditItemExist(regName)) return software.OpenSubKey(regName, true);
            //创建注册表项
            return software.CreateSubKey(regName);
        }

        #region 操作子项
        /// <summary>
        /// 创建子项
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        public static void CreateSubItem(string regName, string subKey)
        {
            //注册表
            RegistryKey reg = Registry.LocalMachine;
            //得到系统软件目录
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);
            //不存在则创建注册表项
            if (!IsRegeditItemExist(regName)) software.CreateSubKey(regName);
            //得到注册表项
            RegistryKey currentReg = software.OpenSubKey(regName, true);
            //创建子项
            if (!IsExistSubKey(currentReg, subKey)) currentReg.CreateSubKey(subKey);
        }
        /// <summary>
        /// 删除子项
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        public static void DeleteSubItem(string regName, string subKey)
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) return;
            RegistryKey currentReg = software.OpenSubKey(regName, true);

            if (!IsExistSubKey(currentReg, regName)) return;
            currentReg.DeleteSubKey(subKey, true);
        }
        #endregion

        #region 根目录下 操作键值
        /// <summary>
        /// 写入键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetValue(string regName, string name, string value)
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) software.CreateSubKey(regName);
            RegistryKey currentReg = software.OpenSubKey(regName, true);
            currentReg.SetValue(name, value);
        }
        /// <summary>
        /// 删除键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        public static void DeleteValue(string regName, string name)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) return;
            RegistryKey currentReg = software.OpenSubKey(regName, true);
            currentReg.DeleteValue(name);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        public static object GetValue(string regName, string name)
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) return null;
            RegistryKey currentReg = software.OpenSubKey(regName, true);
            return currentReg.GetValue(name);
        }
        /// <summary>
        /// 获取键值 整数类型
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue_Int(string regName, string name)
        {
            try
            {
                var objResult = GetValue(regName, name);
                return Convert.ToInt32(objResult);
            }
            catch { }

            return 0;
        }
        /// <summary>
        /// 获取键值 字符串
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue_String(string regName, string name)
        {
            try
            {
                var objResult = GetValue(regName, name);
                return objResult.ToString();
            }
            catch { }

            return string.Empty;
        }
        /// <summary>
        /// 获取键值 Boolean
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetValue_Bool(string regName, string name)
        {
            try
            {
                var objResult = GetValue(regName, name);
                return Convert.ToBoolean(objResult);
            }
            catch { }

            return false;
        }
        #endregion

        #region 子目录下 操作键值
        /// <summary>
        /// 写入键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetValue(string regName, string subKey, string name, string value)
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) software.CreateSubKey(regName);
            RegistryKey currentReg = software.OpenSubKey(regName, true);

            if (!IsExistSubKey(currentReg, subKey)) currentReg.CreateSubKey(subKey);
            RegistryKey subKeyItem = currentReg.OpenSubKey(subKey, true);
            subKeyItem.SetValue(name, value);
        }
        /// <summary>
        /// 删除键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        public static void DeleteValue(string regName, string subKey, string name)
        {
            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) return;
            RegistryKey currentReg = software.OpenSubKey(regName, true);

            if (!IsExistSubKey(currentReg, subKey)) return;
            RegistryKey subKeyItem = currentReg.OpenSubKey(subKey, true);
            subKeyItem.DeleteValue(name);
        }

        /// <summary>
        /// 获取键值
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        public static object GetValue(string regName, string subKey, string name)
        {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey software = reg.OpenSubKey("SOFTWARE", true);

            if (!IsExistSubKey(software, regName)) return null;
            RegistryKey currentReg = software.OpenSubKey(regName, true);

            if (!IsExistSubKey(currentReg, subKey)) return null;
            RegistryKey subKeyItem = currentReg.OpenSubKey(subKey, true);
            return subKeyItem.GetValue(name);
        }
        /// <summary>
        /// 获取键值 整数类型
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue_Int(string regName, string subKey, string name)
        {
            try
            {
                var objResult = GetValue(regName, subKey, name);
                return Convert.ToInt32(objResult);
            }
            catch { }

            return 0;
        }
        /// <summary>
        /// 获取键值 字符串
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue_String(string regName, string subKey, string name)
        {
            try
            {
                var objResult = GetValue(regName, subKey, name);
                return objResult.ToString();
            }
            catch { }

            return string.Empty;
        }
        /// <summary>
        /// 获取键值 Boolean
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetValue_Bool(string regName, string subKey, string name)
        {
            try
            {
                var objResult = GetValue(regName, subKey, name);
                return Convert.ToBoolean(objResult);
            }
            catch { }

            return false;
        }
        #endregion

        #region 是否存在
        /// <summary>
        /// 是否存在子项
        /// </summary>
        /// <param name="currentReg"></param>
        /// <param name="subKey"></param>
        /// <returns></returns>
        public static bool IsExistSubKey(RegistryKey currentReg, string subKey)
        {
            string[] subkeyNames = currentReg.GetSubKeyNames();
            return subkeyNames.Contains(subKey);
        }
        /// <summary>
        /// 判断注册表项是否存在
        /// </summary>
        /// <param name="regName"></param>
        /// <returns></returns>
        public static bool IsRegeditItemExist(string regName)
        {
            string[] subkeyNames;
            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE");

            subkeyNames = software.GetSubKeyNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == regName)
                {
                    key.Close();
                    return true;
                }
            }
            key.Close();
            return false;
        }
        /// <summary>
        /// 判断该路径是否已经存在
        /// </summary>
        /// <param name="regName"></param>
        /// <param name="subKey"></param>
        /// <param name="tripleKey"></param>
        /// <returns></returns>
        private bool IsRegeditKeyExit(string regName, string subKey, string tripleKey)
        {
            string[] subKeyNames;

            RegistryKey key = Registry.LocalMachine;
            RegistryKey software = key.OpenSubKey("SOFTWARE", true);

            RegistryKey operateKey = software.OpenSubKey(regName, true);
            RegistryKey subKeyItem = operateKey.OpenSubKey(subKey, true);

            subKeyNames = subKeyItem.GetSubKeyNames();
            foreach (string keyName in subKeyNames)
            {
                if (keyName == tripleKey)
                {
                    key.Close();
                    return false;
                }
            }
            key.Close();
            return true;
        }
        #endregion
    }
}
