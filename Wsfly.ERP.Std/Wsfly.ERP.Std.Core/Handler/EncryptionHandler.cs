using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 
    /// </summary>
    public class EncryptionHandler
    {
        /// <summary>
        /// 8位加密、解密Key字符
        /// </summary>
        static string _DEFAULTKEY = "~Wsfly!?";

        /// <summary>   
        /// 字符串加密 
        /// </summary>   
        /// <param name="data">要加密的字符串</param>
        /// <returns></returns>
        public static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data)) return "";

            byte[] byKey = ASCIIEncoding.ASCII.GetBytes(_DEFAULTKEY);
            byte[] byIV = ASCIIEncoding.ASCII.GetBytes(_DEFAULTKEY);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }
        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return "";

            byte[] byKey = ASCIIEncoding.ASCII.GetBytes(_DEFAULTKEY);
            byte[] byIV = ASCIIEncoding.ASCII.GetBytes(_DEFAULTKEY);

            byte[] byEnc;

            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
    }
}
