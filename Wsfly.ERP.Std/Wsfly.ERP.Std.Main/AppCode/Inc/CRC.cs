using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Inc
{
    #region CRC-16
    /// <summary>   
    /// CRC-16校验算法
    /// CRC-16/X25
    /// </summary>   
    public class CRC16
    {
        /// <summary>
        /// 获取Hex校验数据
        /// </summary>
        /// <param name="hexDataStr"></param>
        /// <returns></returns>
        public static byte[] GetHexCRC16(string hexDataStr, ref string sendHex)
        {
            //没有内容
            if (string.IsNullOrWhiteSpace(hexDataStr)) return null;

            //去掉空格
            sendHex = hexDataStr.Replace(" ", "").Trim();
            //转换为十六进制byte[]
            byte[] datas = HexToByte(sendHex);
            //生成校验码
            string crcCode = CRC16.GetCRC16Code(datas);
            //连接校验码 ■
            sendHex = sendHex + "FF25A0" + crcCode;
            //转换为十六进制byte[]
            return HexToByte(sendHex);
        }
        /// <summary>
        /// 校验Hex数据
        /// </summary>
        /// <returns></returns>
        public static string CheckHexCRC16(byte[] bytes, ref string receiveHex)
        {
            //没有Byte数据
            if (bytes == null || bytes.Length <= 0) return null;

            //收到的Hex
            receiveHex = "";
            //遍历Byte[]
            foreach (byte b in bytes)
            {
                //转换为16进制
                receiveHex += b.ToString("X2");
            }
            //Hex数据
            AppLog.WriteDebugLog("Hex数据：" + receiveHex);
            //校验Hex数据
            return CheckHexCRC16(receiveHex);
        }
        /// <summary>
        /// 校验Hex数据
        /// </summary>
        /// <returns></returns>
        public static string CheckHexCRC16(string hexDataStr)
        {
            //没有内容
            if (string.IsNullOrWhiteSpace(hexDataStr)) return null;
            if (!hexDataStr.Contains("FF25A0"))
            {
                AppLog.WriteDebugLog("未收到协议数据：FF25A0");
                return null;
            }

            //字符串分组
            string receiveData = hexDataStr.Replace("FF25A0", "■");
            string[] receiveDataArray = receiveData.Split('■');

            //收到的校验码
            string crc16Code = receiveDataArray[receiveDataArray.Length - 1];

            //收到的有效数据
            string dataHex = hexDataStr.Substring(0, hexDataStr.Length - ("FF25A0" + crc16Code).Length);
            dataHex = dataHex.Replace(" ", "").Trim();

            //判断校验码是否正确
            byte[] checkBytes = HexToByte(dataHex.Trim());
            string checkCRC16Code = CRC16.GetCRC16Code(checkBytes);
            if (!crc16Code.Equals(checkCRC16Code)) return null;
            return dataHex;
        }
        /// <summary>
        /// 转换十六进制字符串到字节数组
        /// </summary>
        /// <param name="msg">待转换字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] HexToByte(string msg)
        {
            //移除空格
            msg = msg.Replace(" ", "");

            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }
        /// <summary>
        /// 字符串转CRC-16校验数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetCRC16(string str)
        {
            //字符串转二进制
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            //校验码
            string code = GetCRC16Code(bytes);
            //分隔符
            string data = str + "■" + code;
            AppLog.WriteDebugLog("CRC校验后的数据：" + data);
            //转为二进制
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
        /// <summary>
        /// 验证CRC-16数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CheckCRC16(byte[] data)
        {
            //转换为字符串
            string str = System.Text.Encoding.UTF8.GetString(data);
            string[] strArray = str.Split('■');
            if (strArray.Length < 2) return string.Empty;

            //得到传递的值及校验码
            string val = strArray[0];
            string code = strArray[1];
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;

            //重新获取校验值
            byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(val);
            string checkCode = GetCRC16Code(valBytes);
            if (checkCode.Equals(code)) return val;

            return string.Empty;
        }
        /// <summary>
        /// 获取CRC-16的校验值
        /// </summary>
        /// <param name="crcString"></param>
        /// <returns></returns>
        public static string GetCRC16Code(byte[] creBytes)
        {
            // 开始crc16校验码计算  
            CRC16Util crc16 = new CRC16Util();
            crc16.Reset();
            crc16.Update(creBytes);
            int crc = crc16.GetCRCValue();
            // 16进制的CRC码  
            string crcCode = Convert.ToString(crc, 16).ToUpper();
            // 补足到4位  
            if (crcCode.Length < 4)
            {
                crcCode = crcCode.PadLeft(4, '0');
            }
            return crcCode;
        }
        /// <summary>
        /// 获取CRC-16的校验值
        /// </summary>
        /// <param name="crcString"></param>
        /// <returns></returns>
        public static string GetCRC16Code(string crcString)
        {
            // 转换成字节数组  
            byte[] creBytes = HexString2Bytes(crcString);

            // 开始crc16校验码计算  
            CRC16Util crc16 = new CRC16Util();
            crc16.Reset();
            crc16.Update(creBytes);
            int crc = crc16.GetCRCValue();
            // 16进制的CRC码  
            String crcCode = Convert.ToString(crc, 16).ToUpper();
            // 补足到4位  
            if (crcCode.Length < 4)
            {
                // crcCode = StringUtil.lefgPadding(crcCode, '0', 4);  
                crcCode = crcCode.PadLeft(4, '0');
            }
            return crcCode;
        }
        /// <summary>
        /// 十六进制转字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static String RealHexToStr(String str)
        {
            String hText = "0123456789ABCDEF";
            StringBuilder bin = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                bin.Append(hText[str[i] / 16]).Append(hText[str[i] % 16]).Append(' ');
            }
            return bin.ToString();
        }
        /// <summary>
        /// 十六进制字符串转换成字节数组
        /// </summary>
        /// <param name="hexstr"></param>
        /// <returns></returns>
        private static byte[] HexString2Bytes(string hexstr)
        {
            byte[] b = new byte[hexstr.Length / 2];
            int j = 0;
            for (int i = 0; i < b.Length; i++)
            {
                char c0 = hexstr[j++];
                char c1 = hexstr[j++];
                b[i] = (byte)((Parse(c0) << 4) | Parse(c1));
            }
            return b;
        }
        /// <summary>
        /// 16进制char转换成整型
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static int Parse(char c)
        {
            if (c >= 'a')
                return (c - 'a' + 10) & 0x0f;
            if (c >= 'A')
                return (c - 'A' + 10) & 0x0f;
            return (c - '0') & 0x0f;
        }
        /// <summary>
        /// 字节数组转为十六进制字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
    }
    /// <summary>
    /// CRC-16辅助
    /// </summary>
    internal class CRC16Util
    {
        private int value = 0xffff;
        private static int[] CRC16_TABLE = {
                0x0000, 0x1189, 0x2312, 0x329b, 0x4624, 0x57ad, 0x6536, 0x74bf,
                0x8c48, 0x9dc1, 0xaf5a, 0xbed3, 0xca6c, 0xdbe5, 0xe97e, 0xf8f7,
                0x1081, 0x0108, 0x3393, 0x221a, 0x56a5, 0x472c, 0x75b7, 0x643e,
                0x9cc9, 0x8d40, 0xbfdb, 0xae52, 0xdaed, 0xcb64, 0xf9ff, 0xe876,
                0x2102, 0x308b, 0x0210, 0x1399, 0x6726, 0x76af, 0x4434, 0x55bd,
                0xad4a, 0xbcc3, 0x8e58, 0x9fd1, 0xeb6e, 0xfae7, 0xc87c, 0xd9f5,
                0x3183, 0x200a, 0x1291, 0x0318, 0x77a7, 0x662e, 0x54b5, 0x453c,
                0xbdcb, 0xac42, 0x9ed9, 0x8f50, 0xfbef, 0xea66, 0xd8fd, 0xc974,
                0x4204, 0x538d, 0x6116, 0x709f, 0x0420, 0x15a9, 0x2732, 0x36bb,
                0xce4c, 0xdfc5, 0xed5e, 0xfcd7, 0x8868, 0x99e1, 0xab7a, 0xbaf3,
                0x5285, 0x430c, 0x7197, 0x601e, 0x14a1, 0x0528, 0x37b3, 0x263a,
                0xdecd, 0xcf44, 0xfddf, 0xec56, 0x98e9, 0x8960, 0xbbfb, 0xaa72,
                0x6306, 0x728f, 0x4014, 0x519d, 0x2522, 0x34ab, 0x0630, 0x17b9,
                0xef4e, 0xfec7, 0xcc5c, 0xddd5, 0xa96a, 0xb8e3, 0x8a78, 0x9bf1,
                0x7387, 0x620e, 0x5095, 0x411c, 0x35a3, 0x242a, 0x16b1, 0x0738,
                0xffcf, 0xee46, 0xdcdd, 0xcd54, 0xb9eb, 0xa862, 0x9af9, 0x8b70,
                0x8408, 0x9581, 0xa71a, 0xb693, 0xc22c, 0xd3a5, 0xe13e, 0xf0b7,
                0x0840, 0x19c9, 0x2b52, 0x3adb, 0x4e64, 0x5fed, 0x6d76, 0x7cff,
                0x9489, 0x8500, 0xb79b, 0xa612, 0xd2ad, 0xc324, 0xf1bf, 0xe036,
                0x18c1, 0x0948, 0x3bd3, 0x2a5a, 0x5ee5, 0x4f6c, 0x7df7, 0x6c7e,
                0xa50a, 0xb483, 0x8618, 0x9791, 0xe32e, 0xf2a7, 0xc03c, 0xd1b5,
                0x2942, 0x38cb, 0x0a50, 0x1bd9, 0x6f66, 0x7eef, 0x4c74, 0x5dfd,
                0xb58b, 0xa402, 0x9699, 0x8710, 0xf3af, 0xe226, 0xd0bd, 0xc134,
                0x39c3, 0x284a, 0x1ad1, 0x0b58, 0x7fe7, 0x6e6e, 0x5cf5, 0x4d7c,
                0xc60c, 0xd785, 0xe51e, 0xf497, 0x8028, 0x91a1, 0xa33a, 0xb2b3,
                0x4a44, 0x5bcd, 0x6956, 0x78df, 0x0c60, 0x1de9, 0x2f72, 0x3efb,
                0xd68d, 0xc704, 0xf59f, 0xe416, 0x90a9, 0x8120, 0xb3bb, 0xa232,
                0x5ac5, 0x4b4c, 0x79d7, 0x685e, 0x1ce1, 0x0d68, 0x3ff3, 0x2e7a,
                0xe70e, 0xf687, 0xc41c, 0xd595, 0xa12a, 0xb0a3, 0x8238, 0x93b1,
                0x6b46, 0x7acf, 0x4854, 0x59dd, 0x2d62, 0x3ceb, 0x0e70, 0x1ff9,
                0xf78f, 0xe606, 0xd49d, 0xc514, 0xb1ab, 0xa022, 0x92b9, 0x8330,
                0x7bc7, 0x6a4e, 0x58d5, 0x495c, 0x3de3, 0x2c6a, 0x1ef1, 0x0f78
            };

        /// <summary>
        /// 计算一个字节数组的CRC值 
        /// </summary>
        /// <param name="data"></param>
        public void Update(byte[] data)
        {
            //int fcs = 0xffff;  
            for (int i = 0; i < data.Length; i++)
            {
                // 1.value 右移8位(相当于除以256)  
                // 2.value与进来的数据进行异或运算后再与0xFF进行与运算  
                //    得到一个索引index，然后查找CRC16_TABLE表相应索引的数据  
                // 1和2得到的数据再进行异或运算。  
                value = (value >> 8) ^ CRC16_TABLE[(value ^ data[i]) & 0xff];
            }
            // 取反  
            //return ~fcs;  
        }
        /// <summary>
        /// 计算一个byte的CRC值 
        /// </summary>
        /// <param name="aByte"></param>
        public void Update(byte aByte)
        {
            value = (value >> 8) ^ CRC16_TABLE[(value ^ aByte) & 0xff];
        }
        /// <summary>
        /// 重新设定CRC初始值
        /// </summary>
        public void Reset()
        {
            value = 0xffff;
        }
        /// <summary>
        /// 获取计算好的CRC值
        /// </summary>
        /// <returns></returns>
        public int GetCRCValue()
        {
            return ~value & 0xffff;
        }
        /// <summary>  
        /// 生成FCS校验值  
        /// </summary>  
        /// <param name="ccc"></param>  
        /// <returns></returns>  
        private static byte[] MakeCRC16(byte[] ccc)
        {
            CRC16Util crc16 = new CRC16Util();
            crc16.Reset();
            crc16.Update(ccc);
            byte[] test = IntToByte(crc16.GetCRCValue());
            return test;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        private static int[] Copy(byte[] aa)
        {
            int[] cc = new int[aa.Length];
            for (int i = 0; i < aa.Length; i++)
            {
                cc[i] = aa[i];
            }
            return cc;
        }
        /// <summary>
        /// 整数转Byte[]
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static byte[] IntToByte(int i)
        {
            byte[] abyte0 = new byte[4];
            abyte0[0] = (byte)(0xff & i);
            abyte0[1] = (byte)((0xff00 & i) >> 8);
            abyte0[2] = (byte)((0xff0000 & i) >> 16);
            abyte0[3] = (byte)((0xff000000 & i) >> 24);
            return abyte0;
        }
        /// <summary>
        /// Hex转字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static String RealHexToStr(String str)
        {
            String hText = "0123456789ABCDEF";
            StringBuilder bin = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                bin.Append(hText[str[i] / 16]).Append(hText[str[i] % 16]).Append(' ');
            }
            return bin.ToString();
        }

    }
    #endregion

    #region CRC-32
    /// <summary>
    /// CRC-32位校验算法
    /// </summary>
    public class CRC32
    {
        static UInt32[] crcTable =
        {
          0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,
          0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,
          0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
          0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,
          0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,
          0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
          0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,
          0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,
          0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
          0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,
          0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,
          0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
          0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,
          0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,
          0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
          0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,
          0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,
          0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
          0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,
          0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,
          0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
          0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,
          0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,
          0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
          0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,
          0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,
          0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
          0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,
          0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,
          0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
          0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,
          0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
        };
        /// <summary>
        /// 字符串转CRC-32校验数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetCRC32(string str)
        {
            //字符串转二进制
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            //校验码
            uint code = GetCRC32Code(bytes);
            //分隔符
            string data = str + "■" + code;
            //转为二进制
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
        /// <summary>
        /// 验证CRC-32数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string CheckCRC32(byte[] data)
        {
            //转换为字符串
            string str = System.Text.Encoding.UTF8.GetString(data);
            string[] strArray = str.Split('■');
            if (strArray.Length < 2) return string.Empty;

            //得到传递的值及校验码
            string val = strArray[0];
            uint code = DataType.Uint(strArray[1], 0);
            if (code <= 0) return string.Empty;

            //重新获取校验值
            byte[] valBytes = System.Text.Encoding.UTF8.GetBytes(val);
            uint checkCode = GetCRC32Code(valBytes);
            if (checkCode.Equals(code)) return val;

            return string.Empty;
        }
        /// <summary>
        /// 获取CRC32位校验值
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static uint GetCRC32Code(byte[] bytes)
        {
            uint iCount = (uint)bytes.Length;
            uint crc = 0xFFFFFFFF;

            for (uint i = 0; i < iCount; i++)
            {
                crc = (crc << 8) ^ crcTable[(crc >> 24) ^ bytes[i]];
            }

            return crc;
        }
    }
    #endregion
}

