using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Web;
using System.IO;
using System.Security.Cryptography;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 验证码
    /// </summary>
    public class VerifyCodeHandler
    {
        #region 属性

        #region 验证码长度(默认5个验证码的长度)

        int length = 5;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        #endregion

        #region 验证码字体大小(默认24像素)

        int fontSize = 24;
        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        #endregion

        #region 边框补(默认4像素)

        int padding = 4;
        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }
        #endregion

        #region 自定义背景色(默认白色)

        Color backgroundColor = Color.White;
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }
        #endregion

        #region 自定义字体数组

        string[] fonts = { "Arial", "Georgia", "Rosewood Std", "Ravie" };
        public string[] Fonts
        {
            get { return fonts; }
            set { fonts = value; }
        }
        #endregion

        #region 自定义随机码字符串序列(使用逗号分隔)

        string _code1 = "2,3,4,5,6,7,8,9";//没有0,1
        string _code2 = "a,b,c,d,e,f,g,h,i,j,k,m,n,p,q,r,s,t,u,v,w,x,y,z";//没有l,o
        string _code3 = "A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,V,W,X,Y,Z";//没有I,O
        string _codeSerial = null;

        /// <summary>
        /// 随机验证码序列 (默认：数字大小写字母)
        /// </summary>
        public string CodeSerial
        {
            get
            {
                if (!string.IsNullOrEmpty(_codeSerial))
                {
                    return _codeSerial;
                }
                else
                {
                    switch (this.VerifyCodeHardType)
                    {
                        case VerifyCodeHardType.数字: return _code1;
                        case VerifyCodeHardType.小写字母: return _code2;
                        case VerifyCodeHardType.大写字母: return _code3;
                        case VerifyCodeHardType.大小写字母: return _code2 + "," + _code3;
                        case VerifyCodeHardType.数字小写字母: return _code1 + "," + _code2;
                        case VerifyCodeHardType.数字大写字母: return _code1 + "," + _code3;
                        case VerifyCodeHardType.数字大小写字母:
                        default: return _code1 + "," + _code2 + "," + _code3;
                    }
                }
            }
            set { _codeSerial = value; }
        }
        #endregion

        #region 自定义难度
        VerifyCodeHardType _verifyCodeHardType = VerifyCodeHardType.数字大小写字母;
        /// <summary>
        /// 验证码难度
        /// </summary>
        public VerifyCodeHardType VerifyCodeHardType
        {
            get { return _verifyCodeHardType; }
            set { _verifyCodeHardType = value; }
        }
        #endregion

        #endregion

        #region 方法

        #region 生成校验码图片
        /// <summary>
        /// 生成检验码图片
        /// </summary>
        /// <returns></returns>
        public Bitmap BuildVerifyCode(string sessionKey, out string code)
        {
            ///生成验证码
            code = GetVerifyCode();
            return BuildVerifyCodeImage(code);
        }
        /// <summary>
        /// 生成检验码图片
        /// </summary>
        /// <returns></returns>
        public Bitmap BuildVerifyCode(int len, string sessionKey, out string code)
        {
            ///生成验证码
            code = GetVerifyCode(len);
            return BuildVerifyCodeImage(code);
        }
        /// <summary>
        /// 生成校验码图片
        /// </summary>
        /// <param name="code">已经生成的随机码</param>
        /// <returns>位图</returns>
        private Bitmap BuildVerifyCodeImage(string code)
        {
            //随机种子
            Random rand = new Random(GetNewSeed());
            //左侧空白宽度
            int spaceWidth = rand.Next(20, 60);

            int fSize = FontSize;
            int fWidth = fSize + Padding;

            int imageWidth = (int)(code.Length * fWidth) + 4 + Padding * 2;
            int imageHeight = fSize * 2 + Padding;

            //生成图片
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(imageWidth, imageHeight);

            //GDI生成图片
            Graphics g = Graphics.FromImage(image);
            //设置文本输出质量
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //清空背景
            g.Clear(BackgroundColor);

            float top = 0, top1 = 1, top2 = 1;
            float left = spaceWidth;

            int n1 = (imageHeight - FontSize - Padding * 2);
            int n2 = n1 / 4;

            top1 = n2;
            top2 = n2 * 2;

            //字体样式——大小、字体
            Font font;
            //字体颜色——蓝色
            Brush brush = new System.Drawing.SolidBrush(Color.Blue);
            //钢笔
            Pen pen = new Pen(brush, 1);

            /* 取消曲线
            //定义曲线转折点 
            int count = 10;
            Point[] points = new Point[count];
            for (int i = 0; i < count; i++)
            {
                points[i].X = (imageWidth / (count - 1)) * i;
                points[i].Y = imageHeight / 2 + rand.Next(-5, 9);
            }
            
            //绘制曲线
            g.DrawCurve(pen, points, 0.5F);
            */

            //随机字体和颜色的验证码字符
            int findex, fsindex;
            for (int i = 0; i < code.Length; i++)
            {
                findex = rand.Next(Fonts.Length - 1);
                fsindex = rand.Next(5);

                //字体样式
                System.Drawing.FontStyle fontStyle;
                switch (fsindex)
                {
                    case 2: fontStyle = FontStyle.Italic; break;
                    case 3: fontStyle = FontStyle.Bold; break;
                    default: fontStyle = FontStyle.Regular; break;
                }

                //字符字体
                font = new System.Drawing.Font(Fonts[findex], fSize, fontStyle);
                //Size sz = TextRenderer.MeasureText(measureString, font);

                //验证码字符
                string codeText = code.Substring(i, 1);
                //SizeF fontSize = g.MeasureString(codeText,font);
                Size fontSize = System.Windows.Forms.TextRenderer.MeasureText(codeText, font);

                //字符定位
                if (i % 2 == 1) { top = top2; }
                else { top = top1; }

                //画字符
                g.DrawString(codeText, font, brush, left, top);

                //左侧距离
                left += fontSize.Width / 2 - 3;
            }

            //释放资源
            g.Dispose();

            //扭曲图片
            image = TwistImage(image, true, 3, 2);

            //返回图片
            return image;
        }

        private const double PI = 3.1415926535897932384626433832795;
        private const double PI2 = 6.283185307179586476925286766559;
        /// <summary>
        /// 正弦曲线Wave扭曲图片
        /// image = TwistImage(image, true, 3, 5);
        /// </summary>
        /// <param name="srcBmp"></param>
        /// <param name="bXDir"></param>
        /// <param name="nMultValue">波形的幅度倍数</param>
        /// <param name="dPhase">波形的起始相位，取值区间[0-2*PI)</param>
        /// <returns></returns>
        public System.Drawing.Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            System.Drawing.Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            // 将位图背景填充为白色
            System.Drawing.Graphics graph = System.Drawing.Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(BackgroundColor), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();

            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bXDir ? (PI2 * (double)j) / dBaseAxisLen : (PI2 * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    // 取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    System.Drawing.Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                     && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }

            return destBmp;
        }
        #endregion

        #region 生成随机字符码
        /// <summary>
        /// 生成随机字符码
        /// </summary>
        /// <param name="codeLen">随机码长度</param>
        /// <returns>产生的随机码</returns>
        public string GetVerifyCode(int len)
        {
            if (len <= 0) len = Length;
            string[] arr = CodeSerial.Split(',');
            string code = "";
            int randValue = -1;

            //unchecked((int)DateTime.Now.Ticks)
            Random rand = new Random(GetNewSeed());

            for (int i = 0; i < len; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                code += arr[randValue];
            }

            return code;
        }
        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <returns></returns>
        public string GetVerifyCode()
        {
            return GetVerifyCode(this.Length);
        }
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
        #endregion

        #endregion
    }
    /// <summary>
    /// 验证码-简化版
    /// </summary>
    public class VerifyCodeSimpleHandler
    {
        #region 属性

        static int _length = 5;
        static int _fontSize = 18;
        static int _padding = 4;
        static Color _backgroundColor = Color.White;
        //static string[] _fonts = { "Arial", "Georgia", "Rosewood Std", "Ravie", "Courier" };
        //系统自带字体：Arial、Courier、MS Serif、Tahoma、Verdana、Comic Sans、MS Sans Serif、Symbol、Time New Roman、Impact、Lucida Console、Marlett、Modem、Samall Fonts、Webdings、Windings、Roman、MS-DOS CP437、script、SIMHEI、SimSun&NSimSun、Palatino、Linotype、Microsoft、MS。
        static string[] _fonts = { "Arial", "Tahoma", "Verdana", "Time New Roman", "Lucida Console" };
        static string[] _codeSerial = {
                                          "1,2,3,4,5,6,7,8,9",
                                          "a,b,c,d,e,f,g,h,i,j,k,m,n,p,q,r,s,t,u,v,w,x,y,z",
                                          "A,B,C,D,E,F,G,H,I,J,K,L,M,N,P,Q,R,S,T,U,V,W,X,Y,Z"
                                      };

        static string _customerCode;
        static VerifyCodeHardType _level = VerifyCodeHardType.数字大写字母;


        /// <summary>
        /// 验证码长度(默认5个验证码的长度)
        /// </summary>
        public static int Length
        {
            get { return _length; }
            set { _length = value; }
        }
        /// <summary>
        /// 验证码字体大小(默认18像素)
        /// </summary>
        public static int FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }
        /// <summary>
        /// 边框补(默认2像素)
        /// </summary>
        public static int Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }
        /// <summary>
        /// 自定义背景色(默认白色)
        /// </summary>
        public static Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
        /// <summary>
        /// 设置背景颜色（#CCCCCC）
        /// </summary>
        public static string SetBackgroundColor
        {
            set { _backgroundColor = ColorTranslator.FromHtml(value); }
        }
        /// <summary>
        /// 自定义字体数组
        /// </summary>
        public static string[] Fonts
        {
            get { return _fonts; }
            set { _fonts = value; }
        }
        /// <summary>
        /// 用户自定验证字符串(使用逗号分隔)
        /// </summary>
        public static string CustomerCode
        {
            get { return _customerCode; }
            set { _customerCode = value; }
        }
        /// <summary>
        /// 随机码字符串序列(使用逗号分隔)
        /// </summary>
        static string GetCodeSerial
        {
            get
            {
                ///如果有自定验证码，则返回自定验证码
                if (!string.IsNullOrEmpty(CustomerCode)) return CustomerCode;

                string codeSerial = _codeSerial[0];

                switch (_level)
                {
                    case VerifyCodeHardType.小写字母:
                        codeSerial = _codeSerial[1];
                        break;
                    case VerifyCodeHardType.大写字母:
                        codeSerial = _codeSerial[2];
                        break;
                    case VerifyCodeHardType.大小写字母:
                        codeSerial = _codeSerial[1] + "," + _codeSerial[2];
                        break;
                    case VerifyCodeHardType.数字小写字母:
                        codeSerial = _codeSerial[0] + "," + _codeSerial[1];
                        break;
                    case VerifyCodeHardType.数字大写字母:
                        codeSerial = _codeSerial[0] + "," + _codeSerial[2];
                        break;
                    case VerifyCodeHardType.数字大小写字母:
                        codeSerial = _codeSerial[0] + "," + _codeSerial[1] + "," + _codeSerial[2];
                        break;
                    default:
                        break;
                }

                return codeSerial;
            }
        }
        /// <summary>
        /// 验证码难度（默认简单）
        /// </summary>
        public static VerifyCodeHardType VerifyCodeHardType
        {
            get { return _level; }
            set { _level = value; }
        }

        #endregion

        #region 实现
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="key">保存到Session的Key</param>
        /// <returns></returns>
        public static byte[] CreateVerifyCode(ref string buildCode)
        {
            //生成验证码
            string code = BuildString();
            //生成图片
            Image img = BuildImage(code);

            //返回验证码
            buildCode = code;

            //将验证码图片转换为流
            using (img)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();//内存流
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);//将图片保存到流

                return ms.ToArray();//返回byte[]
            }
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <returns>返回随机生成的验证码</returns>
        public static string BuildString(int length = 5)
        {
            if (length == 0)
            {
                length = Length;
            }

            string[] arr = GetCodeSerial.Split(',');

            string code = "";

            int randValue = -1;

            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));

            for (int i = 0; i < length; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);

                code += arr[randValue];
            }

            return code;
        }
        /// <summary>
        /// 生成校验码图片
        /// </summary>
        /// <param name="code">已经生成的随机码</param>
        /// <returns>位图</returns>
        public static Image BuildImage(string code)
        {
            int fSize = FontSize;
            int fWidth = fSize + Padding;

            int imageWidth = (int)(code.Length * fWidth) + 4 + Padding * 2 + 10;
            int imageHeight = fSize * 2 + Padding;

            //创建一个图片
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(imageWidth, imageHeight);
            //GDI
            Graphics g = Graphics.FromImage(image);
            //填充背景色
            g.Clear(BackgroundColor);
            //随机
            Random rand = new Random();

            float left = rand.Next(10, 50), top = 0, top1 = 1, top2 = 1;

            int n1 = (imageHeight - FontSize - Padding * 2);
            int n2 = n1 / 4;
            top1 = n2;
            top2 = n2 * 2;

            Font font;
            Brush brush;

            int findex, fsindex;

            //随机字体和颜色的验证码字符
            for (int i = 0; i < code.Length; i++)
            {
                string codeText = code.Substring(i, 1);

                findex = rand.Next(Fonts.Length - 1);
                fsindex = rand.Next(5);

                font = new System.Drawing.Font(Fonts[findex], fSize, FontStyle.Regular);
                brush = new System.Drawing.SolidBrush(Color.Blue);
                SizeF fontSize = g.MeasureString(codeText, font);

                if (i % 2 == 1)
                {
                    top = top2;
                }
                else
                {
                    top = top1;
                }
                //写验证码
                g.DrawString(codeText, font, brush, left, top);
                //左侧坐标
                left += fontSize.Width / 2;
            }

            //画一个边框
            //g.DrawRectangle(new Pen(Color.Black, 0), 0, 0, image.Width - 1, image.Height - 1);
            //释放资源
            g.Dispose();
            //返回图片
            return image;
        }
        #endregion
    }
    /// <summary>
    /// 验证码困难
    /// </summary>
    public enum VerifyCodeHardType
    {
        数字,
        小写字母,
        大写字母,
        大小写字母,
        数字小写字母,
        数字大写字母,
        数字大小写字母
    }
}
