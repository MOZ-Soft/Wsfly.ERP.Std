using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Wsfly.ERP.Std.Core.Encryption
{
    /// <summary>
    /// DES����
    /// </summary>
    public class EncryptionDES
    {
        const string _DEFAULTKEY = "!Wsfly@9";//8���ַ�

        #region EnCode ����
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string value)
        {
            return EnCode(value, _DEFAULTKEY);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string value, string key)
        {
            return EnCode(value, key);
        }

        /// <summary>   
        /// EnCode 64λ��Կ (8�ַ�)���� 
        /// </summary>   
        /// <param name="str">Ҫ���ܵ��ַ���</param>
        /// <returns></returns>
        private static string EnCode(string data,string key)
        {
            if (string.IsNullOrEmpty(data)) return "";

            key = EncryptionHelper.GenerateKey(key, _DEFAULTKEY, 8);

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

        #endregion

        #region DeCode ����

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string value)
        {
            return DeCode(value, _DEFAULTKEY);
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string value, string key)
        {
            return DeCode(value, key);
        }

        /// <summary>   
        /// DeCode 64λ��Կ (8�ַ�)����
        /// </summary>   
        /// <param name="str">Ҫ���ܵ��ַ���</param>   
        /// <returns></returns>   
        private static string DeCode(string data,string key)
        {
            if (string.IsNullOrEmpty(data)) return "";

            key = EncryptionHelper.GenerateKey(key, _DEFAULTKEY, 8);

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

        #endregion
    }
}
