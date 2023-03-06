using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary> 
    /// 各种正则表达式验证
    /// </summary> 
    public class RegexHandler
    {
        #region 验证

        #region 验证字符

        /// <summary>
        /// 验证字符是否在4至12之间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidByte(string value, int minSize, int maxSize)
        {
            return Regex.IsMatch(value, @"^[a-zA-Z0-9_]{" + minSize + "," + maxSize + "}$");
        }

        /// <summary>
        /// 验证后缀名
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidPostfix(string value)
        {
            return Regex.IsMatch(value, @"\.(?i:gif|jpg|png|bmp|icon)$",RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 没有特殊字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool NoneSpecialChar(string source)
        {
            return Regex.IsMatch(source, "^[a-zA-Z0-9]+$"); 
        }
        /// <summary>
        /// 是否颜色字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsColor(string source)
        {
            return Regex.IsMatch(source, "^#?([a-f]|[A-F]|[0-9]){3}(([a-f]|[A-F]|[0-9]){3})?$");
        }
        #endregion
        
        #region 验证数字
        /// <summary>
        /// 是否数字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNum(object source)
        {
            try
            {
                Regex regex = new Regex(@"^\d+$");

                if (regex.IsMatch(source.ToString()))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 是不是Int型的
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsInt(object source)
        {
            try
            {
                Regex regex = new Regex(@"^[+|-]{0,1}\d+$");

                if (regex.Match(source.ToString()).Success)
                {
                    if ((long.Parse(source.ToString()) > 0x7fffffffL) || (long.Parse(source.ToString()) < -2147483648L))
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 验证是否为小数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFloat(string value)
        {
            return Regex.IsMatch(value, @"^[+|-]?\d*\.?\d*$");
        }
        #endregion

        #region 验证邮箱
        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(string source)
        {
            return Regex.IsMatch(source, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", RegexOptions.IgnoreCase);
        }
        public static bool HasEmail(string source)
        {
            return Regex.IsMatch(source, @"[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证网址
        /// <summary>
        /// 验证网址
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsUrl(string source)
        {
            return Regex.IsMatch(source, @"^(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?$", RegexOptions.IgnoreCase);
        }
        public static bool HasUrl(string source)
        {
            return Regex.IsMatch(source, @"(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证日期
        /// <summary>
        /// 验证日期
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsDateTime(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"^2\d{3}-(?:0?[1-9]|1[0-2])-(?:0?[1-9]|[1-2]\d|3[0-1])\s(?:0?[1-9]|1\d|2[0-3]):(?:0?[1-9]|[1-5]\d):(?:0?[1-9]|[1-5]\d)$");
        }

        /// <summary>
        /// 判断是否是日期+时间格式
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsValidDateTime(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"^(19|20)\d{2}[/\s\-\.]*(0[1-9]|1[0-2]|[1-9])[/\s\-\.]*(0[1-9]|3[01]|[12][0-9]|[1-9])[\s] *(2[0-3]|[01]?\d)(:[0-5]\d){0,2}$");
        }
        /// <summary>
        /// 是否是日期格式
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsDate(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"^((((1[6-9]|[2-9]\d)\d{2})[-|/]+(0?[13578]|1[02])[-|/]+(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$");
        }
        /// <summary>
        /// 是否为时间格式
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsTime(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"^((20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d)$");
        }
        #endregion
        
        #region 验证电话
        /// <summary>
        /// 是不是中国电话，格式010-85849685/13813800000
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsTel(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"(^(\d{3,4}-)?\d{7,8}$)|(^1[3456789][0-9]{9}$)", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 验证手机号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsMobile(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            return Regex.IsMatch(source, @"^1[3456789][0-9]{9}$", RegexOptions.IgnoreCase);
        }
        #endregion
        
        #region 验证中文
        /**/
        /// <summary>
        /// 中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsCn(string source)
        {
            return Regex.IsMatch(source, @"^[\u4e00-\u9fa5]+$", RegexOptions.IgnoreCase);
        }
        public static bool HasCn(string source)
        {
            return Regex.IsMatch(source, @"[\u4e00-\u9fa5]+", RegexOptions.IgnoreCase);
        }
        #endregion

        #region 验证IP
        /// <summary>
        /// 验证IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsIP(string source)
        {
            return Regex.IsMatch(source, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$", RegexOptions.IgnoreCase);
        }
        public static bool HasIP(string source)
        {
            return Regex.IsMatch(source, @"(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])", RegexOptions.IgnoreCase);
        }
        public static bool IsValidIP(string value)
        {
            return Regex.IsMatch(value, @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$");
        }
        /// <summary>
        /// 是否端口号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsPort(string value)
        {
            string pattern = @"^((\d{0,4})|([1-5]\d{1,4})|(6[0-4]\d{1,3})|(65[0-4]\d{1,2})|(655[0-2]\d)|(6553[0-5]))$";

            return Regex.IsMatch(value, pattern);
        }
        #endregion

        #region 验证身份证
        /// <summary>
        /// 验证身份证是否有效
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool IsIDCard(string Id)
        {
            if (Id.Length == 18)
            {
                bool check = IsIDCard18(Id);
                return check;
            }
            else if (Id.Length == 15)
            {
                bool check = IsIDCard15(Id);
                return check;
            }
            else
            {
                return false;
            }
        }

        private static bool IsIDCard18(string Id)
        {
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        private static bool IsIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            return true;//符合15位身份证标准
        }

        /// <summary>
        /// 判断身份证是否合法
        /// </summary>
        /// <param name="str">身份证号码</param>
        /// <returns>bool</returns>
        public static bool IsValidID(string id)
        {
            bool error = true;

            string reg = @"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$";    //设置正则表达式        
            Match m = Regex.Match(id, reg);                         //判断并得到结果

            if (!m.Success)
            {//判断如果不符合正则表达式规则设置error为false;
                error = false; ;
            }

            return error;
        }
        #endregion

        #region 看字符串的长度是不是在限定数之间 一个中文为两个字符
        /**/
        /// <summary>
        /// 看字符串的长度是不是在限定数之间 一个中文为两个字符
        /// </summary>
        /// <param name="source">字符串</param>
        /// <param name="begin">大于等于</param>
        /// <param name="end">小于等于</param>
        /// <returns></returns>
        public static bool IsLengthStr(string source, int begin, int end)
        {
            int length = Regex.Replace(source, @"[^\x00-\xff]", "OK").Length;
            if ((length <= begin) && (length >= end))
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 邮政编码 6个数字
        /**/
        /// <summary>
        /// 邮政编码 6个数字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsPostCode(string source)
        {
            return Regex.IsMatch(source, @"^\d{6}$", RegexOptions.IgnoreCase);
        }
        #endregion
        
        #region 验证是不是正常字符 字母，数字，下划线的组合
        /**/
        /// <summary>
        /// 验证是不是正常字符 字母，数字，下划线的组合
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNormalChar(string source)
        {
            if (HasCn(source)) return false;

            return Regex.IsMatch(source, @"[\w\d_]+", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 是否有特殊符号
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool HasSpecialChar(string domain)
        {
            if (HasCn(domain)) return false;

            string pattern = "[`~!@#$%^&*()+=|{}':;',//\"\\[\\].<>/?~！@#￥%……&*（）——+|{}【】‘；：”“’。，、？]";
            Regex reg = new Regex(pattern);

            return reg.IsMatch(domain);
        }
        #endregion

        #region 验证域名
        /// <summary>
        /// 是否是英文域名
        /// 例：google.com
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool IsEnDomain(string domain)
        {
            string pattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{1,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?";

            Regex reg = new Regex(pattern);

            return reg.IsMatch(domain);
        }
        /// <summary>
        /// 是否中文域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool IsCnDomain(string domain)
        {
            string pattern = @"[\u4e00-\u9fa5][a-zA-Z0-9][-a-zA-Z0-9]{1,20}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?";

            Regex reg = new Regex(pattern);

            return reg.IsMatch(domain);
        }
        
        #endregion

        #endregion

        #region 获取

        /// <summary>
        /// 获取页面编码格式
        /// </summary>
        /// <param name="source"></param>
        public static string GetCharSet(string source)
        {
            Match reg = Regex.Match(source, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return reg.Groups[2].Value.Trim();
        }
        /// <summary>
        /// 获取域名中间的内容
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static string GetDomain(string domain)
        {
            Regex reg = new Regex(@"(www.)?(?<value>[a-zA-Z0-9][-a-zA-Z0-9]{1,62})", RegexOptions.IgnoreCase);

            domain = reg.Match(domain).Groups["value"].Value.ToString().Trim();

            return domain;
        }
        #endregion
    }

}
