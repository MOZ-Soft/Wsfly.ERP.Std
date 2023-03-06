using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Wsfly.ERP.Std.Core.Encryption
{
    public class EncryptionAES
    {
        /// <summary>
        /// ƒ¨»œ√‹‘ø
        /// </summary>
        static string _DEFAULTKEY = "Wsfly!%(@m_?=#_Wsflyo)0P8n?_v6$%";

        /// <summary>
        /// AESº”√‹ 64Œª√‹‘ø (32◊÷∑˚)º”√‹
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt)
        {
            return EnCode(toEncrypt, _DEFAULTKEY);
        }
        /// <summary>
        /// AESº”√‹ 64Œª√‹‘ø (32◊÷∑˚)º”√‹
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt,string key)
        {
            return EnCode(toEncrypt, key);
        }
        /// <summary>
        /// AESº”√‹ 64Œª√‹‘ø (32◊÷∑˚)º”√‹
        /// </summary>
        /// <param name="toEncrypt">º”√‹◊÷∑˚¥Æ</param>
        /// <returns></returns>
        private static string EnCode(string toEncrypt, string key)
        {
            if (string.IsNullOrEmpty(toEncrypt)) return "";

            key = EncryptionHelper.GenerateKey(key, _DEFAULTKEY, 32);

            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();//using System.Security.Cryptography;   
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;//using System.Security.Cryptography;   
            rDel.Padding = PaddingMode.PKCS7;//using System.Security.Cryptography;

            ICryptoTransform cTransform = rDel.CreateEncryptor();//using System.Security.Cryptography;   
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        

        



        /// <summary>
        /// AESΩ‚√‹ 64Œª√‹‘ø (32◊÷∑˚)Ω‚√‹
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string Decrypt(string toDecrypt)
        {
            return DeCode(toDecrypt, _DEFAULTKEY);
        }
        /// <summary>
        /// AESΩ‚√‹ 64Œª√‹‘ø (32◊÷∑˚)Ω‚√‹
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <returns></returns>
        public static string Decrypt(string toDecrypt,string key)
        {
            return DeCode(toDecrypt, key);
        }
        /// <summary>
        /// AESΩ‚√‹ 64Œª√‹‘ø (32◊÷∑˚)Ω‚√‹
        /// </summary>
        /// <param name="toDecrypt">Ω‚√‹◊÷∑˚¥Æ</param>
        /// <returns></returns>
        private static string DeCode(string toDecrypt, string key)
        {
            if (string.IsNullOrEmpty(toDecrypt)) return "";

            key = EncryptionHelper.GenerateKey(key, _DEFAULTKEY, 32);

            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = null;

            try
            {
                toEncryptArray = Convert.FromBase64String(toDecrypt);
            }
            catch { return null; }

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}
