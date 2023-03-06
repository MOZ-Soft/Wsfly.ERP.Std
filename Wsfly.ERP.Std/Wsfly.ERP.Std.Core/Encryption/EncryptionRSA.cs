using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Wsfly.ERP.Std.Core.Encryption
{
    /// <summary>
    /// RSA加密解密
    /// 说明：
    /// 公钥加密->私钥解密
    /// 私钥加密->私钥解密
    /// </summary>
    public class EncryptionRSA
    {
        /// <summary>
        /// 生成公钥&私钥
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="privateKey">私钥</param>
        public static void GenerateKeys(out string publicKey, out string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            privateKey = rsa.ToXmlString(true);
            publicKey = rsa.ToXmlString(false);
        }

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

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source">要加密的内容</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static string RSAEncrypt2(string source, string publicKey)
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
        /// 解密
        /// </summary>
        /// <param name="source">要解密的内容</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string RSADecrypt2(string source, string privateKey)
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

        /*
            说明：
            加密
            1、将要加密的内容进行MD5加密 var a = GetMD5("123")
            2、将加密后的MD5进行签名 var b = SignatureFormatter(privateKey, a)
            3、RSA加密 var c = RSAEncrypt(publicKey, b)

            解密：
            1、RSA解密 var d = RSADecrypt(privateKey, c)
            2、签名验证 var e = SignatureDeformatter(publicKey, d)
        */
        /// <summary>
        /// 对原始数据进行MD5加密
        /// </summary>
        /// <param name="source">待加密数据</param>
        /// <returns>返回机密后的数据</returns>
        public static string GetMD5(string source)
        {
            HashAlgorithm algorithm = HashAlgorithm.Create("MD5");
            byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(source);
            byte[] inArray = algorithm.ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }
        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="source">待签名的字符串</param>
        /// <param name="strHashData">Hash描述</param>
        /// <returns></returns>
        public bool GetHash(string source, ref string strHashData)
        {
            try
            {
                //从字符串中取得Hash描述 
                byte[] Buffer;
                byte[] HashData;
                System.Security.Cryptography.HashAlgorithm MD5 = System.Security.Cryptography.HashAlgorithm.Create("MD5");
                Buffer = System.Text.Encoding.GetEncoding("GB2312").GetBytes(source);
                HashData = MD5.ComputeHash(Buffer);
                strHashData = Convert.ToBase64String(HashData);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 对MD5加密后的密文进行签名
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="source">MD5加密后的密文</param>
        /// <returns></returns>
        public static string SignatureFormatter(string privateKey, string source)
        {
            byte[] rgbHash = Convert.FromBase64String(source);
            RSACryptoServiceProvider key = new RSACryptoServiceProvider();
            key.FromXmlString(privateKey);
            RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(key);
            formatter.SetHashAlgorithm("MD5");
            byte[] inArray = formatter.CreateSignature(rgbHash);
            return Convert.ToBase64String(inArray);
        }
        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="source">MD5加密后的数据</param>
        /// <returns>RSA公钥加密后的数据</returns>
        public static string RSAEncrypt3(string publicKey, string source)
        {
            string str2;
            try
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(publicKey);
                byte[] bytes = new UTF8Encoding().GetBytes(source);
                str2 = Convert.ToBase64String(provider.Encrypt(bytes, false));
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privateKey">私钥</param>
        /// <param name="source">待解密的数据</param>
        /// <returns>解密后的结果</returns>
        public static string RSADecrypt3(string privateKey, string source)
        {
            string str2;
            try
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(privateKey);
                byte[] rgb = Convert.FromBase64String(source);
                byte[] buffer2 = provider.Decrypt(rgb, false);
                str2 = new UTF8Encoding().GetString(buffer2);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        
        /// <summary>
        /// 签名验证
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="strHashbyteDeformatter">待验证的用户名</param>
        /// <param name="deformatterData">注册码</param>
        /// <returns></returns>
        public static bool SignatureDeformatter(string publicKey, string strHashbyteDeformatter, string deformatterData)
        {
            try
            {
                byte[] rgbHash = Convert.FromBase64String(strHashbyteDeformatter);
                RSACryptoServiceProvider key = new RSACryptoServiceProvider();
                key.FromXmlString(publicKey);
                RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(key);
                deformatter.SetHashAlgorithm("MD5");
                byte[] rgbSignature = Convert.FromBase64String(deformatterData);
                if (deformatter.VerifySignature(rgbHash, rgbSignature)) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
