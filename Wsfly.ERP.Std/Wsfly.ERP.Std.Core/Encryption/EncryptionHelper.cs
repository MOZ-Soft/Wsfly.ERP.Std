using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Wsfly.ERP.Std.Core.Encryption
{
    public class EncryptionHelper
    {
        /// <summary>
        /// 得到哈希值
        /// </summary>
        /// <param name="TextToHash"></param>
        /// <returns></returns>
        public static string HashText(string TextToHash)
        {
            if (string.IsNullOrEmpty(TextToHash))
                return "";

            SHA1CryptoServiceProvider Sha1;

            byte[] bytVAlue;
            byte[] bytHash;

            Sha1 = new SHA1CryptoServiceProvider();

            bytVAlue = Encoding.UTF8.GetBytes(TextToHash);
            bytHash = Sha1.ComputeHash(bytVAlue);

            Sha1.Clear();

            return Convert.ToBase64String(bytVAlue);
        }

        /// <summary>
        /// 得到MD5值
        /// </summary>
        /// <param name="TextToMD5"></param>
        /// <returns></returns>
        public static string MD5Text(string TextToMD5)
        {
            if (string.IsNullOrEmpty(TextToMD5))
                return "";

            MD5CryptoServiceProvider md5;
            byte[] bytValue;
            byte[] bytHash;
            //创建新的加密服务提供程序对象
            md5 = new MD5CryptoServiceProvider();

            //将原始字符串转换成字节数组
            bytValue = Encoding.UTF8.GetBytes(TextToMD5);

            //计算散列，并返回一个字节数组
            bytHash = md5.ComputeHash(bytValue);

            md5.Clear();

            // 返回散列值的 Base64 编码字符串
            return Convert.ToBase64String(bytHash);
        }

        /// <summary>
        /// 生成Key值
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="defaultValue"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateKey(string key, string defaultKey, int length)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (key.Length > length)
                {
                    ///加密Key大于32位则只取length位
                    key = key.Substring(0, length);
                }
                else if (key.Length < length)
                {
                    ///加密Key小于length位则由默认Key填充
                    key += defaultKey.Substring(0, (length - key.Length));
                }
            }
            else
            {
                ///默认Key加密
                key = defaultKey;
            }

            return key;
        }
    }
}
