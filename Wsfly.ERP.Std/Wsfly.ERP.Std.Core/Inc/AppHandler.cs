using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Drawing;


using ThoughtWorks.QRCode.Codec;

using Wsfly.ERP.Std.Core.Handler;
using Wsfly.ERP.Std.Core.Encryption;
using System.Management;

namespace Wsfly.ERP.Std.Core
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AppHandler
    {
        /// <summary>
        /// 得到对应字符串类型
        /// 如：bool 得到 System.Boolean
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetTypeByString(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;

            if (type.IndexOf(',') > 0) return Type.GetType("System.String");

            try
            {
                switch (type.ToLower())
                {
                    case "bool":
                        return Type.GetType("System.Boolean", true, true);
                    case "byte":
                        return Type.GetType("System.Byte", true, true);
                    case "sbyte":
                        return Type.GetType("System.SByte", true, true);
                    case "char":
                        return Type.GetType("System.Char", true, true);
                    case "decimal":
                        return Type.GetType("System.Decimal", true, true);
                    case "double":
                        return Type.GetType("System.Double", true, true);
                    case "float":
                        return Type.GetType("System.Single", true, true);
                    case "int":
                        return Type.GetType("System.Int32", true, true);
                    case "uint":
                        return Type.GetType("System.UInt32", true, true);
                    case "long":
                        return Type.GetType("System.Int64", true, true);
                    case "ulong":
                        return Type.GetType("System.UInt64", true, true);
                    case "object":
                        return Type.GetType("System.Object", true, true);
                    case "short":
                        return Type.GetType("System.Int16", true, true);
                    case "ushort":
                        return Type.GetType("System.UInt16", true, true);
                    case "string":
                        return Type.GetType("System.String", true, true);
                    case "date":
                    case "datetime":
                        return Type.GetType("System.DateTime", true, true);
                    case "guid":
                        return Type.GetType("System.Guid", true, true);
                    default:
                        return Type.GetType(type, true, true);
                }
            }
            catch { }

            return Type.GetType("System.String", true, true);
        }
        /// <summary>
        /// 得到网络图片，如果本地不存在，则下载到本地
        /// </summary>
        /// <param name="pic"></param>
        /// <returns></returns>
        public static string GetNetPic(string pic, string thumbSize = "240x240", string dirName = "Imgs")
        {
            //缩略图
            if (!string.IsNullOrWhiteSpace(thumbSize)) thumbSize = "_" + thumbSize;
            //文件夹
            if (!string.IsNullOrWhiteSpace(dirName)) dirName = dirName + "\\";

            //本地图片
            string fileName = Path.GetFileName(pic);
            string fileExt = pic.Substring(pic.LastIndexOf('.'));
            string localPath = AppDomain.CurrentDomain.BaseDirectory + "Downloads\\" + dirName + fileName + thumbSize + fileExt;

            //是否存在文件
            if (File.Exists(localPath)) return localPath;

            //是否存在目录
            if (!Directory.Exists(localPath)) Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            using (Image image = WebHandler.GetImgFile(pic))
            {
                //下载图片并保存
                image.Save(localPath);
            }

            return localPath;
        }
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Bitmap BuildEWM(string url)
        {
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 6;

            //将字符串生成二维码图片
            return qrCodeEncoder.Encode(url, Encoding.Default);
        }
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static void BuildEWM(string content, string path, int scale = 6)
        {
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = scale;

            //将字符串生成二维码图片
            var bitmap = qrCodeEncoder.Encode(content, Encoding.Default);
            bitmap.Save(path);
            bitmap.Dispose();
        }
        /// <summary>
        /// 生成地址
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Uri BuildUri(string path)
        {
            //生成默认地址
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);

            //是否以下面的字符串开头
            bool flag = Regex.IsMatch(path, @"^((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp|pack)://).*", RegexOptions.IgnoreCase);

            if (!flag)
            {
                //生成本地Uri
                path = path.StartsWith("/Views/") ? path.Substring(7) : path;
                path = path.Trim().Trim('/');
                uri = new Uri("/Views/" + path, System.UriKind.RelativeOrAbsolute);
            }

            return uri;
        }
        /// <summary>
        /// 显示帐号
        /// 如：13813800000 显示为：138****0000
        /// 如：lyp@qq.com 显示为：l**@qq.com
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string ShowLoginAccount(string account)
        {
            string newAccount = string.Empty;

            bool isMobile = false;

            isMobile = RegexHandler.IsMobile(account);

            if (isMobile) account = "0" + account;

            if (account.IndexOf('@') > 0)
            {
                string naccount = account.Split('@')[0];
                string mailHost = account.Split('@')[1];

                if (naccount.Length > 3)
                {
                    int len = naccount.Length / 3;
                    naccount = naccount.Substring(0, len) + GetStarCharString(naccount.Length - len * 2) + naccount.Substring(naccount.Length - len);
                }
                else
                {
                    naccount = naccount.Substring(0, 1) + GetStarCharString(naccount.Length - 1);
                }

                newAccount = naccount + "@" + mailHost;
            }
            else
            {
                int len = account.Length / 3;

                newAccount = account.Substring(0, len) + GetStarCharString(account.Length - len * 2) + account.Substring(account.Length - len);
            }

            if (isMobile) newAccount = newAccount.Substring(1);

            return newAccount;
        }
        /// <summary>
        /// 得到星号字符串
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        static string GetStarCharString(int len)
        {
            string str = "";
            for (int i = 0; i < len; i++) { str += "*"; }
            return str;
        }
        /// <summary>
        /// 显示日期
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ShowDate(DateTime? date)
        {
            if (!date.HasValue) return "";

            return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 计算总页数
        /// </summary>
        /// <param name="totalCount"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static int CalculatePageCount(int totalCount, int pageSize)
        {
            if (totalCount <= 0) return 0;

            return (totalCount + pageSize - 1) / pageSize;
        }

        #region 应用程序
        /// <summary>
        /// 设置应用程序开机自动运行
        /// </summary>
        /// <param name="fileName">应用程序的文件名</param>
        /// <param name="isAutoRun">是否自动运行，为false时，取消自动运行</param>
        /// <exception cref="System.Exception">设置不成功时抛出异常</exception>
        public static void SetAutoRun(string fileName, bool isAutoRun = true)
        {
            RegistryKey reg = null;

            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    throw new Exception("该文件不存在!");
                }

                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (reg == null)
                {
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                }
                if (isAutoRun)
                {
                    reg.SetValue(name, fileName);
                }
                else
                {
                    reg.SetValue(name, false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (reg != null) reg.Close();
            }
        }
        /// <summary>
        /// 重新启动当前程序
        /// </summary>
        public void ReStartApp()
        {
            //当前路径
            string path = Assembly.GetExecutingAssembly().Location;
            //新启动程序
            System.Diagnostics.Process.Start(path);
            //退出程序
            Environment.Exit(0);
        }
        #endregion

        #region 嵌入资源
        /// <summary>
        /// 得到资源文件
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static Stream GetResource(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            try
            {
                ///资源名称
                string resourceName = "Wsfly.Resources.Default." + name;
                string path = AppDomain.CurrentDomain.BaseDirectory + "Wsfly.Resources.dll";
                Assembly assembly = Assembly.LoadFile(path);
                ///读取资源
                Stream stream = assembly.GetManifestResourceStream(resourceName);
                ///返回资源
                return stream;
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 得到资源字符串
        /// </summary>
        /// <returns></returns>
        public static string GetResourceText(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Stream stream = GetResource(name);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        /// <summary>
        /// 得到资源图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Drawing.Image GetResourceImage(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            ///得到图片流
            Stream stream = GetResource("Images." + name);
            ///流是否为空
            if (stream == null) return null;
            ///转换为图片对象
            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            ///释放资源
            stream.Close();
            ///返回图片
            return img;
        }
        /// <summary>
        /// 得到 WPF ImageBrush
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Windows.Media.ImageBrush GetResourceImageBrush(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Stream stream = GetResource("Images." + name);
            if (stream == null) return null;

            System.Windows.Media.ImageSourceConverter imageSourceConverter = new System.Windows.Media.ImageSourceConverter();
            System.Windows.Media.ImageBrush brush = new System.Windows.Media.ImageBrush();
            brush.ImageSource = (System.Windows.Media.ImageSource)imageSourceConverter.ConvertFrom(stream); ;

            return brush;
        }
        /// <summary>
        /// 得到WPF ImageSource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Windows.Media.ImageSource GetResourceImageSource(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Stream stream = GetResource("Images." + name);
            if (stream == null) return null;

            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        /// <summary>
        /// 得到ICON图标
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Drawing.Icon GetResourceIcon(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            ///得到图片流
            Stream stream = GetResource("Icons." + name);
            ///流是否为空
            if (stream == null) return null;
            ///得到ICON
            System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(new System.Drawing.Bitmap(stream).GetHicon());
            ///释放资源
            stream.Close();
            ///返回图片
            return icon;
        }
        /// <summary>
        /// 得到声音文件
        /// </summary>
        /// <param name="name"></param>
        public static Stream GetResourceSound(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return GetResource("Sounds." + name);
        }
        #endregion

        #region 语音播放
        /// <summary>
        /// 播放声音
        /// </summary>
        public void PlaySound(string msg)
        {
            try
            {
                VoiceHandler.PlaySound(msg);
            }
            catch { }
        }
        /// <summary>
        /// 播放声音
        /// </summary>
        public void PlaySound(Stream stream)
        {
            try
            {
                //播放声音
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
                player.Play();
            }
            catch { }
        }
        #endregion

        #region 获取PC信息
        /// <summary>
        /// 获取主机信息
        /// CPU ：Win32_Processor
        /// 主板：Win32_BaseBoard
        /// 硬盘：Win32_DiskDrive
        /// </summary>
        /// <returns></returns>
        public static string GetPCID(string idName)
        {
            try
            {
                ManagementClass mc = new ManagementClass(idName);
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (idName.Equals("Win32_Processor"))
                    {
                        //CPU
                        return mo.Properties["ProcessorId"].Value.ToString();
                    }
                    else if (idName.Equals("Win32_BaseBoard"))
                    {
                        //主板
                        return mo.Properties["SerialNumber"].Value.ToString();
                    }
                    else if (idName.Equals("Win32_DiskDrive"))
                    {
                        //硬盘
                        return mo.Properties["SerialNumber"].Value.ToString();
                    }
                }
            }
            catch { }

            return null;
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="serverDomain">域名</param>
        /// <param name="filePath">路径</param>
        /// <param name="dir">目录</param>
        /// <returns></returns>
        public static string UploadFile(string serverDomain, string filePath, string dir)
        {
            try
            {
                //获取文件Bytes
                byte[] bytes = WebHandler.FileToBytes(filePath);
                string ext = FileHandler.GetPostfix(filePath);

                string url = WebHandler.UploadFile(serverDomain, bytes, ext, "erp." + dir);
                if (!string.IsNullOrWhiteSpace(url)) return url;
            }
            catch { }

            return string.Empty;
        }
        #endregion

        #region 本地信息
        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            try
            {
                //得到主机名
                string hostName = System.Net.Dns.GetHostName();

                try
                {
                    //本地IP  *过时*
                    string localIP = System.Net.Dns.Resolve(hostName).AddressList[0].ToString();
                    if (!string.IsNullOrWhiteSpace(localIP)) return localIP;
                }
                catch { }

                //解析主机名
                System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);

                //遍历IP地址列表
                for (int i = 0; i < ipEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipEntry.AddressList[i].ToString();
                    }
                }
                return "0.0.0.1";
            }
            catch (Exception ex)
            {
                return "0.0.0.0";
            }
        }
        #endregion
    }
}