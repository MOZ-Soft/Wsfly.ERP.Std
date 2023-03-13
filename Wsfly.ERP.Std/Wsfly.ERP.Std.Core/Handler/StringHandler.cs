using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 字符串操作
    /// </summary>
    public class StringHandler
    {
        #region HTML
        /// <summary>
        /// 去掉字符串中的脚本
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveScripts(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            source = Regex.Replace(source, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            return source;
        }
        /// <summary>
        /// 去掉字符串中的IFRAME
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveIframe(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            source = Regex.Replace(source, @"<iframe[^>]*?>.*?</iframe>", "", RegexOptions.IgnoreCase);

            return source;
        }
        /// <summary>
        /// 去除HTML标记
        /// 根据<>标记 正则匹配
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveAllHtmlTag(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            return System.Text.RegularExpressions.Regex.Replace(source, "<[^>]*?>", "");
        }
        /// <summary>
        /// 去除HTML标记
        /// [WEB 技术调用]
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveHTMLAndEncode(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";

            //删除脚本
            source = Regex.Replace(source, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML
            source = Regex.Replace(source, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"-->", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"<!--.*", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            source = source.Replace("<", "");
            source = source.Replace(">", "");
            source = source.Replace("\r\n", "");
            source = source.Replace("\t", "");
            source = source.Replace("\r", "");
            source = source.Replace("\n", "");

            //source = HttpContext.Current.Server.HtmlEncode(source).Trim();

            return source;
        }
        /// <summary>
        /// 去除HTML标记
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveHTML(string source)
        {
            try
            {
                if (string.IsNullOrEmpty(source)) return "";

                string[] aryReg = {
                                @"<script[^>]*?>.*?</script>",
                                //@"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                                @"([\r\n])[\s]+",
                                @"&(quot|#34);",
                                @"&(amp|#38);",
                                @"&(lt|#60);",
                                @"&(gt|#62);",
                                @"&(nbsp|#160);",
                                @"&(iexcl|#161);",
                                @"&(cent|#162);",
                                @"&(pound|#163);",
                                @"&(copy|#169);",
                                @"&#(\d+);",
                                @"-->",
                                @"<!--.*\n"
                              };

                string[] aryRep = {
                                  "",
                                  //"",
                                  "",
                                  "\"",
                                  "&",
                                  "<",
                                  ">",
                                  "   ",
                                  "\xa1",  //chr(161),
                                  "\xa2",  //chr(162),
                                  "\xa3",  //chr(163),
                                  "\xa9",  //chr(169),
                                  "",
                                  "\r\n", ""
                              };

                string newReg = aryReg[0];
                string strOutput = source;

                for (int i = 0; i < aryReg.Length; i++)
                {
                    Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                    strOutput = regex.Replace(strOutput, aryRep[i]);
                }

                strOutput = strOutput.Replace("<", "");
                strOutput = strOutput.Replace(">", "");
                strOutput = strOutput.Replace("\r\n", "");
                strOutput = strOutput.Replace("\r", "");
                strOutput = strOutput.Replace("\n", "");
                strOutput = strOutput.Replace("\t", "");

                return strOutput;
            }
            catch (Exception ex) { }

            return source;
        }
        /// <summary>
        /// 去除DIV标记
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveDivTag(string source)
        {
            //删除脚本
            source = Regex.Replace(source, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除DIV
            source = Regex.Replace(source, @"<div[^>]*?>", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"</div>", "<br/>", RegexOptions.IgnoreCase);
            //删除P
            source = Regex.Replace(source, @"<p[^>]*?>", "", RegexOptions.IgnoreCase);
            source = Regex.Replace(source, @"</p>", "<br/>", RegexOptions.IgnoreCase);
            //删除IMG
            source = Regex.Replace(source, @"<img[^>]*?>", "", RegexOptions.IgnoreCase);
            //去掉两个BR
            source = Regex.Replace(source, @"<br[^>]*?><br[^>]*?>", "<br/>", RegexOptions.IgnoreCase);

            //source.Replace("<", "");
            //source.Replace(">", "");
            //source.Replace("\r\n", "");

            //source = HttpContext.Current.Server.HtmlEncode(source).Trim();

            return source;
        }
        /// <summary>
        /// 移除ViewState
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveViewState(string value)
        {
            if (value == null) return "";

            string pattern = @"<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" [^>]+/>";

            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase);

            reg.IsMatch(value);

            return reg.Replace(value, "");
        }
        /// <summary>
        /// 取出某个标记中的所有内容
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetTagContent(string source, string tag)
        {
            if (source == null) return "";

            string content = "";

            string strPattern = @"<" + tag + "[^>]*?>(?<Text>[^<]*)</" + tag + ">";

            MatchCollection Matches = Regex.Matches(source, strPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            foreach (Match NextMatch in Matches)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    content += "|$|";
                }

                content += NextMatch.Groups["Text"].Value.ToString();
            }

            return content;
        }
        /// <summary>
        /// 取出HTML文本中的图片地址
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetImgUrl(string source)
        {
            if (source == null) return "";

            string str = string.Empty;

            //string sPattern = @"^<img\s+[^>]*>";

            Regex r = new Regex(@"<img\s+[^>]*\s*src\s*=\s*([']?)(?<url>\S+)'?[^>]*>", RegexOptions.Compiled);

            Match m = r.Match(source.ToLower());

            if (m.Success)
            {
                str = m.Result("${url}");
            }
            return str;
        }

        /// <summary>
        /// HTML编码转换为字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string HtmlToString(string source)
        {
            if (source == null) return "";

            source = source.Trim();
            source = source.Replace("'", "''");
            //source = source.Replace(@"\\", @"\");
            //source = System.Web.HttpContext.Current.Server.HtmlEncode(source);

            return source;
        }
        /// <summary>
        /// 字符串转换为HTML编码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StringToHtml(string source)
        {
            if (source == null) return "";

            source = source.Trim();
            source = source.Replace("\r\n", "<br />");
            source = source.Replace("\n", "<br />");
            //source = System.Web.HttpContext.Current.Server.HtmlDecode(source);
            return source;
        }
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string ClearHTMLAndSubString(string source, int lenght)
        {
            source = StringToHtml(source);

            source = RemoveAllHtmlTag(source);

            source = SubStringsByBytes(source, lenght * 2);

            return source;
        }
        #endregion

        #region 过滤

        /// <summary>
        /// 过滤关键字
        /// 以"*"显示
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="keywords">关键字列表</param>
        /// <returns></returns>
        public static string FilterKeywords(string value, ArrayList keywords)
        {
            for (int i = 0; i < keywords.Count; i++)
            {
                if (keywords[i] == null || string.IsNullOrEmpty(keywords[i].ToString()))
                    continue;

                int len = keywords[i].ToString().Length;//关键字的长度

                string newStr = "";

                int k = 0;

                while (k < len)
                {
                    newStr += "*";

                    k++;
                }

                value = value.Replace(keywords[i].ToString(), "");
            }

            return value;
        }

        #endregion

        #region 字符串编码
        /// <summary>
        /// 字符串编码
        /// </summary>
        public static string StringEncode(string source, EncodeCharType encodeName)
        {
            string value = string.Empty;

            if (string.IsNullOrEmpty(source)) return null;

            if (!RegexHandler.HasCn(source)) return source;

            switch (encodeName)
            {
                case EncodeCharType.GBK:
                    value = ChineseToString(source, encodeName.ToString());
                    break;
                case EncodeCharType.GB2312:
                    value = ChineseToString(source, encodeName.ToString());
                    break;
                case EncodeCharType.Base64:
                    value = Base64Encode(source);
                    break;
                case EncodeCharType.Unicode:
                    value = GB2312ToUnicode(source);
                    break;
                default:
                    value = source;
                    break;
            }

            return value;
        }
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Base64Encode(string source)
        {
            byte[] bytes = Encoding.Default.GetBytes(source);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Base64Decode(string source)
        {
            byte[] outputb = Convert.FromBase64String(source);
            return Encoding.Default.GetString(outputb);
        }
        /// <summary>
        /// GB2312,GBK
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encodeName"></param>
        /// <returns></returns>
        private static string ChineseToString(string source, string encodeName)
        {
            string value = null;

            Byte[] encodedBytes = new byte[source.Length * 2];
            int iCount = Encoding.GetEncoding(encodeName).GetBytes(source, 0, source.Length, encodedBytes, 0);

            foreach (Byte b in encodedBytes)
            {
                string strValue = Convert.ToString(b, 16);
                strValue = strValue.ToUpper();
                value += "%" + strValue;
            }

            return value;
        }
        /// <summary>
        /// 对值进行编码
        /// 调用：Microsoft.JScript.GlobalObject.encodeURI
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeValue(string value)
        {
            //值进行编码
            if (!string.IsNullOrEmpty(value))
            {
                value = Microsoft.JScript.GlobalObject.encodeURI(value);
            }

            return value;
        }
        /// <summary>
        /// 对值进行解码
        /// 调用：Microsoft.JScript.GlobalObject.decodeURI
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeValue(string value)
        {
            //值进行解码
            if (!string.IsNullOrEmpty(value))
            {
                value = Microsoft.JScript.GlobalObject.decodeURI(value);
            }

            return value;
        }
        /// <summary>
        /// 加密Url字符
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EncodeUrlChar(string value)
        {
            /*
                十六进制值 
                1. +  URL 中+号表示空格 %2B 
                2. 空格 URL中的空格可以用+号或者编码 %20 
                3. /  分隔目录和子目录 %2F  
                4. ?  分隔实际的 URL 和参数 %3F  
                5. % 指定特殊字符 %25  
                6. # 表示书签 %23  
                7. & URL 中指定的参数间的分隔符 %26  
                8. = URL 中指定参数的值 %3D 
             */
            return value.Replace("+", "%2B").Replace(" ", "%20").Replace("/", "%2F").Replace("?", "%3F").Replace("%", "%25").Replace("#", "%23").Replace("&", "%26").Replace("=", "%3D");
        }
        /// <summary>
        /// 解密Url字符
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DecodeUrlChar(string value)
        {
            /*
                十六进制值 
                1. +  URL 中+号表示空格 %2B 
                2. 空格 URL中的空格可以用+号或者编码 %20 
                3. /  分隔目录和子目录 %2F  
                4. ?  分隔实际的 URL 和参数 %3F  
                5. % 指定特殊字符 %25  
                6. # 表示书签 %23  
                7. & URL 中指定的参数间的分隔符 %26  
                8. = URL 中指定参数的值 %3D 
             */
            return value.Replace("%2B", "+").Replace("%20", " ").Replace("%2F", "/").Replace("%3F", "?").Replace("%25", "%").Replace("%23", "#").Replace("%26", "&").Replace("%3D", "=");
        }
        /// <summary>
        /// 汉字转换为Unicode编码
        /// </summary>
        /// <param name="str">要编码的汉字字符串</param>
        /// <returns>Unicode编码的的字符串</returns>
        public static string GB2312ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2) r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') + bts[i].ToString("x").PadLeft(2, '0');
            return r;
        }
        /// <summary>
        /// 将Unicode编码转换为汉字字符串
        /// </summary>
        /// <param name="str">Unicode编码字符串</param>
        /// <returns>汉字字符串</returns>
        public static string UnicodeToGB2312(string str)
        {
            string r = str;
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (mc.Count >= 0)
            {
                r = string.Empty;
                byte[] bts = new byte[2];
                foreach (Match m in mc)
                {
                    bts[0] = (byte)int.Parse(m.Groups[2].Value, System.Globalization.NumberStyles.HexNumber);
                    bts[1] = (byte)int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    r += Encoding.Unicode.GetString(bts);
                }
            }
            return r;
        }
        /// <summary>
        /// 作用：将字符串内容转化为16进制数据编码，其逆过程是Decode
        /// 参数说明：
        /// strEncode 需要转化的原始字符串
        /// 转换的过程是直接把字符转换成Unicode字符,比如数字"3"-->0033,汉字"我"-->U+6211
        /// 函数decode的过程是encode的逆过程.
        /// </summary>
        /// <param name="strEncode"></param>
        /// <returns></returns>
        public static string HexEncode(string strEncode)
        {
            string strReturn = "";//  存储转换后的编码
            foreach (short shortx in strEncode.ToCharArray())
            {
                strReturn += shortx.ToString("X4");
            }
            return strReturn;
        }


        /// <summary>
        /// 作用：将16进制数据编码转化为字符串，是Encode的逆过程
        /// </summary>
        /// <param name="strDecode"></param>
        /// <returns></returns>
        public static string HexDecode(string strDecode)
        {
            string sResult = "";
            for (int i = 0; i < strDecode.Length / 4; i++)
            {
                sResult += (char)short.Parse(strDecode.Substring(i * 4, 4), global::System.Globalization.NumberStyles.HexNumber);
            }
            return sResult;
        }
        #endregion

        #region 字符串操作
        ///<summary>
        /// 计算文本长度，区分中英文字符，中文算两个长度，英文算一个长度
        /// </summary>
        /// <param name="value">要计算的文本</param>
        /// <returns>长度</returns>
        public static int GetLength(string value)
        {
            int len = 0;

            for (int i = 0; i < value.Length; i++)
            {
                byte[] byte_len = Encoding.Default.GetBytes(value.Substring(i, 1));

                if (byte_len.Length > 1)
                    len += 2; //如果长度大于1，是中文，占两个字节，+2
                else
                    len += 1; //如果长度等于1，是英文，占一个字节，+1
            }

            return len;
        }
        #endregion

        #region 截取字符串
        /// <summary>
        /// 截取字符串
        /// 如果长度超过指定长度则加[...]
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string SubStrings(string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return "...";

            if (value.Length > length)
            {
                return value.Substring(0, length) + "...";
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SubStringsWithNothing(string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            if (value.Length > length)
            {
                return value.Substring(0, length);
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 截取字符串 根据字节
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string SubStringsByBytes(string value, int length, string end = "...")
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            byte[] bytes = Encoding.Unicode.GetBytes(value);
            if (bytes.Length <= length) return value;

            int n = 0;
            int i = 0;

            for (; i < bytes.GetLength(0) && n < length; i++)
            {
                if (i % 2 == 0)
                {
                    n++;
                }
                else
                {
                    if (bytes[i] > 0)
                    {
                        n++;
                    }
                }
            }

            i = i + 1;

            if (i % 2 == 1)
            {
                i = i - 1;
            }

            string strReturn = System.Text.Encoding.Unicode.GetString(bytes, 0, i);

            return (strReturn.Length > 1 && bytes.Length > i) ? strReturn.Substring(0, strReturn.Length - 1) + end : strReturn;
        }
        /// <summary>
        /// 字符串转数组，根据字节分割
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string[] StringToArrayByBytes(string value, int maxLength)
        {
            if (String.IsNullOrEmpty(value)) return null;

            byte[] bytes = Encoding.Unicode.GetBytes(value);
            if (bytes.Length <= maxLength) return new string[] { value };

            int n = 0;
            int i = 0;
            int b = 0;

            //字符串列表
            List<string> list = new List<string>();

            for (; i < bytes.GetLength(0); i++)
            {
                if (i % 2 == 0)
                {
                    n++;
                }
                else
                {
                    if (bytes[i] > 0)
                    {
                        n++;
                    }
                }

                //切割到最大值或结尾
                if (n + 2 >= maxLength || i == bytes.Length - 1)
                {
                    //切割
                    i = i + 1;
                    if (i % 2 == 1) i = i - 1;

                    //得到切割的内容
                    string str = Encoding.Unicode.GetString(bytes, b, i - b);
                    list.Add(str);

                    //下次切割开始位置
                    b = i;
                    n = 0;
                }
            }

            //返回数组
            return list.ToArray();
        }
        /// <summary>
        /// 截取字符串 根据字节 去掉HMLT
        /// </summary>
        /// <returns></returns>
        public static string SubStringWithoutHTML(string value, int length)
        {
            value = RemoveAllHtmlTag(value);
            value = SubStringsByBytes(value, length);

            return value;
        }
        /// <summary>
        /// 得到字符串长度 根据字节
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetStringLength(string value)
        {
            if (String.IsNullOrEmpty(value)) return 0;
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            return bytes.Length;
        }
        #endregion

        #region 替换与转换

        /// <summary>
        /// 替换字符
        /// </summary>
        /// <param name="source">字符串</param>
        /// <param name="oldChar">要替换的字符</param>
        /// <param name="newChar">替换为的字符</param>
        /// <returns></returns>
        public static string Replace(string source, string oldChar, string newChar)
        {
            if (string.IsNullOrEmpty(source)) return null;

            return source.Replace(oldChar, newChar);
        }
        /// <summary>
        /// 转换字符串为ASCII
        /// </summary>
        /// <param name="source">字符串</param>
        /// <returns></returns>
        public static string StringToASCII(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            string str = "";

            for (int i = 0; i < source.Length; i++)
            {
                str += Convert.ToInt32(source[i]).ToString();
            }

            return str;
        }
        ///// <summary>
        ///// 简体转繁体
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static string SimpleToTraditional(string source)
        //{
        //    return Microsoft.VisualBasic.Strings.StrConv(source, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0);
        //}
        ///// <summary>
        ///// 繁体转简体
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public static string TraditionalToSimple(string source)
        //{
        //    return Microsoft.VisualBasic.Strings.StrConv(source, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0);
        //}

        #endregion

        #region 随机字符串,随机数

        private static string _LowerChar = "abcdefghijklmnopqrstuvwxyz";
        private static string _UpperChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string _NumberChar = "0123456789";


        /// <summary>
        /// 使用RNGCryptoServiceProvider 做种，可以在一秒内产生的随机数重复率非常
        /// 的低，对于以往使用时间做种的方法是个升级
        /// </summary>
        /// <returns></returns>
        public static int GetNewSeed()
        {
            byte[] rndBytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(rndBytes);
            return BitConverter.ToInt32(rndBytes, 0);
        }
        /// <summary>
        /// 取得指定范围内的随机数
        /// </summary>
        /// <param name="startNumber">下限数</param>
        /// <param name="endNumber">上限数</param>
        /// <returns>int</returns>
        public static int GetRandomNumber(int startNumber, int endNumber)
        {
            Random objRandom = new Random(GetNewSeed());
            int r = objRandom.Next(startNumber, endNumber);
            return r;
        }
        /// <summary>
        /// 获取指定 ASCII 范围内的随机字符串
        /// </summary>
        /// <param name="resultLength">结果字符串长度</param>
        /// <param name="startNumber"> 开始的ASCII值 如（33－125）中的 33</param>
        /// <param name="endNumber"> 结束的ASCII值 如（33－125）中的 125</param>
        /// <returns></returns>
        public static string GetRandomStringByASCII(int resultLength, int startNumber, int endNumber)
        {
            System.Random objRandom = new System.Random(GetNewSeed());
            string result = null;

            for (int i = 0; i < resultLength; i++)
            {
                result += (char)objRandom.Next(startNumber, endNumber);
            }
            return result;
        }
        /// <summary>
        /// 从指定字符串中抽取指定长度的随机字符串
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="resultLength">待获取随机字符串长度</param>
        /// <returns></returns>
        public static string GetRandomString(string source, int resultLength)
        {
            if (string.IsNullOrWhiteSpace(source)) source = _LowerChar + _UpperChar + _NumberChar;

            System.Random objRandom = new System.Random(GetNewSeed());
            string result = null;
            for (int i = 0; i < resultLength; i++)
            {
                result += source.Substring(objRandom.Next(0, source.Length - 1), 1);
            }
            return result;
        }
        /// <summary>
        /// 获取指定长度随机的数字字符串
        /// </summary>
        /// <param name="resultLength">待获取随机字符串长度</param>
        /// <returns></returns>
        public static string GetRandomNumberString(int resultLength)
        {
            return GetRandomString(_NumberChar, resultLength);
        }
        /// <summary>
        /// 获取指定长度随机的字母字符串（包含大小写字母）
        /// </summary>
        /// <param name="resultLength">待获取随机字符串长度</param>
        /// <returns></returns>
        public static string GetRandomLetterString(int resultLength)
        {
            return GetRandomString(_LowerChar + _UpperChar, resultLength);
        }
        /// <summary>
        /// 获取指定长度随机的字母＋数字混和字符串（包含大小写字母）
        /// </summary>
        /// <param name="resultLength"></param>
        /// <returns></returns>
        public static string GetRandomMixString(int resultLength)
        {
            return GetRandomString(_LowerChar + _UpperChar + _NumberChar, resultLength);
        }
        /// <summary>
        /// 得到GUID
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetGUID(GuidType type)
        {
            //32位字符串：xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //连字符分隔的32位字符串：xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
            //在大括号中、由连字符分隔的32位字符串：{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
            //在圆括号中、由连字符分隔的32位字符串：(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
            return System.Guid.NewGuid().ToString(type.ToString());
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// 编码类型
    /// </summary>
    public enum EncodeCharType
    {
        GBK = 0,
        GB2312 = 1,
        UTF8 = 2,
        Base64 = 3,
        Unicode = 4,
    }
    /// <summary>
    /// GUID类型
    /// </summary>
    public enum GuidType
    {
        /// <summary>
        /// 32位字符串：xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /// </summary>
        N,
        /// <summary>
        /// //连字符分隔的32位字符串：xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        /// </summary>
        D,
        /// <summary>
        /// //在大括号中、由连字符分隔的32位字符串：{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
        /// </summary>
        B,
        /// <summary>
        /// //在圆括号中、由连字符分隔的32位字符串：(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
        /// </summary>
        P
    }
    #endregion
}