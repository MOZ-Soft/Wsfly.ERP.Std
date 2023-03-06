using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.API
{
    public class APIEncrypt
    {
        #region RSA加密、解密
        /**
            说明：
            公钥加密->私钥解密
            私钥加密->私钥解密
        */

        /// <summary>         
        /// 生成公钥&私钥         
        /// </summary>         
        /// <param name="publicKey">公钥</param>         
        /// <param name="privateKey">私钥</param>         
        public static void RSAGenerateKeys(out string publicKey, out string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ToXmlString(false);
            privateKey = rsa.ToXmlString(true);
        }
        /*
        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="source">要加密的内容</param>
        /// <param name="publicKey">公钥或私密</param>
        /// <returns></returns>
        public static string Encrypt(string source, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string plaintext = source;
            rsa.FromXmlString(publicKey);
            byte[] cipherbytes;
            byte[] byteEn = rsa.Encrypt(Encoding.UTF8.GetBytes("w"), false);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(plaintext), false);

            StringBuilder sbString = new StringBuilder();
            for (int i = 0; i < cipherbytes.Length; i++)
            {
                sbString.Append(cipherbytes[i] + ",");
            }

            return sbString.ToString();
        }
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="source">要解密的内容</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string Decrypt(string source, string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);

            byte[] byteEn = rsa.Encrypt(Encoding.UTF8.GetBytes("w"), false);
            string[] sBytes = source.Split(',');

            for (int j = 0; j < sBytes.Length; j++)
            {
                if (sBytes[j] != "")
                {
                    byteEn[j] = Byte.Parse(sBytes[j]);
                }
            }

            byte[] plaintbytes = rsa.Decrypt(byteEn, false);
            return Encoding.UTF8.GetString(plaintbytes);
        }
        */
        /// <summary>
        /// 加密
        /// </summary>
        public static string RSAEncrypt(string source, string publicKey)
        {
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);

            //CspParameters cspParams = new CspParameters();
            //cspParams.KeyContainerName = KeyContainerName;
            //RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
            RSAalg.FromXmlString(publicKey);

            int keySize = RSAalg.KeySize / 8;
            int bufferSize = keySize - 11;
            byte[] buffer = new byte[bufferSize];
            MemoryStream msInput = new MemoryStream(sourceBytes);
            MemoryStream msOutput = new MemoryStream();
            int readLen = msInput.Read(buffer, 0, bufferSize);

            while (readLen > 0)
            {
                byte[] dataToEnc = new byte[readLen];
                Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                byte[] encData = RSAalg.Encrypt(dataToEnc, false);
                msOutput.Write(encData, 0, encData.Length);
                readLen = msInput.Read(buffer, 0, bufferSize);
            }

            msInput.Close();
            byte[] result = msOutput.ToArray();
            msOutput.Close();
            RSAalg.Clear();

            StringBuilder sbString = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sbString.Append(result[i] + ",");
            }
            return sbString.ToString().Trim(',');
        }
        /// <summary>
        /// 解密
        /// </summary>
        public static string RSADecrypt(string source, string privateKey)
        {
            string[] sBytes = source.Trim(',').Split(',');
            byte[] cryptBytes = new byte[sBytes.Length];
            for (int j = 0; j < sBytes.Length; j++)
            {
                if (sBytes[j] != "")
                {
                    cryptBytes[j] = Byte.Parse(sBytes[j]);
                }
            }

            //CspParameters cspParams = new CspParameters();
            //cspParams.KeyContainerName = KeyContainerName;
            //RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
            RSAalg.FromXmlString(privateKey);

            int keySize = RSAalg.KeySize / 8;
            int bufferSize = keySize;
            byte[] buffer = new byte[bufferSize];
            MemoryStream msInput = new MemoryStream(cryptBytes);
            MemoryStream msOutput = new MemoryStream();
            int readLen = msInput.Read(buffer, 0, bufferSize);

            while (readLen > 0)
            {
                byte[] dataToDec = new byte[readLen];
                Array.Copy(buffer, 0, dataToDec, 0, readLen);
                byte[] decData = RSAalg.Decrypt(dataToDec, false);
                msOutput.Write(decData, 0, decData.Length);
                readLen = msInput.Read(buffer, 0, bufferSize);
            }

            msInput.Close();
            byte[] result = msOutput.ToArray();
            msOutput.Close();
            RSAalg.Clear();

            return Encoding.UTF8.GetString(result);
        }
        #endregion

        #region DES加密、解密
        /// <summary>   
        /// DES 64位密钥 (8字符)加密 
        /// </summary>   
        /// <param name="data">要加密的字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string data, string key)
        {
            if (string.IsNullOrEmpty(data)) return "";

            key = GenerateKey(key, _DEFAULTKEY, 8);

            byte[] byKey = ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = ASCIIEncoding.ASCII.GetBytes(key);

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
        /// DES 64位密钥 (8字符)解密
        /// </summary>
        /// <param name="data">要解密的字符串</param>   
        /// <returns></returns>   
        public static string DESDecrypt(string data, string key)
        {
            if (string.IsNullOrEmpty(data)) return "";

            key = GenerateKey(key, _DEFAULTKEY, 8);

            byte[] byKey = ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = ASCIIEncoding.ASCII.GetBytes(key);

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

        #region AES加密、解密
        /// <summary>
        /// AES加密 64位密钥 (32字符)加密
        /// </summary>
        /// <param name="toEncrypt">加密字符串</param>
        /// <returns></returns>
        public static string AESEncrypt(string toEncrypt, string key)
        {
            if (string.IsNullOrEmpty(toEncrypt)) return "";

            key = GenerateKey(key, _DEFAULTKEY, 32);

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
        /// AES解密 64位密钥 (32字符)解密
        /// </summary>
        /// <param name="toDecrypt">解密字符串</param>
        /// <returns></returns>
        public static string AESDecrypt(string toDecrypt, string key)
        {
            if (string.IsNullOrEmpty(toDecrypt)) return "";

            key = GenerateKey(key, _DEFAULTKEY, 32);

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
        #endregion

        #region MD5加密
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
            StringBuilder tmp = new StringBuilder();
            foreach (byte i in result) tmp.Append(i.ToString("x"));
            return tmp.ToString();
        }
        #endregion

        /// <summary>
        /// DES、AES默认密钥
        /// </summary>
        static string _DEFAULTKEY = "Wsfly!%(@m_?=#_Wsflyo)0P8n?_v6$%";
        /// <summary>
        /// 生成Key值
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="defaultValue"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string GenerateKey(string key, string defaultKey, int length)
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