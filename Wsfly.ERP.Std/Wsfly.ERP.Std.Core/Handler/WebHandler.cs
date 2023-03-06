using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Threading;
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;


namespace Wsfly.ERP.Std.Core.Handler
{
    public class WebHandler
    {
        private readonly static int TIMEOUT = 15000;
        private CookieContainer _cookieCon = new CookieContainer();
        private CredentialCache _credentials = new CredentialCache();

        const string sAccept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
        const string sUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        const string sContentType = "application/x-www-form-urlencoded";
        const string sRequestEncoding = "ascii";
        const string sResponseEncoding = "utf-8";

        #region WEB请求文件数据
        /// <summary>
        /// 获取远程图片
        /// 可获取验证码图片
        /// </summary>
        public static System.Drawing.Image GetImgFile(string url)
        {
            string cookies = null;

            return GetImgFile(url, null, ref cookies);
        }
        /// <summary>
        /// 获取远程图片
        /// 可获取验证码图片
        /// </summary>
        public static System.Drawing.Image GetImgFile(string url, string server, ref string cookies)
        {
            try
            {
                System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (!string.IsNullOrEmpty(cookies))
                {
                    cookies = cookies.Trim(';');

                    CookieContainer cookieContainer = new CookieContainer();
                    cookieContainer.SetCookies(new Uri(server), cookies);
                    httpRequest.CookieContainer = cookieContainer;
                }

                httpRequest.Timeout = TIMEOUT;
                System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();

                cookies += ";" + webResponse.Headers.Get("Set-Cookie");
                cookies = cookies.Trim(';');

                System.Drawing.Image img = new System.Drawing.Bitmap(webResponse.GetResponseStream());

                return img;
            }
            catch { }

            return null;
        }
        /// <summary>
        /// 获取远程图片
        /// </summary>
        /// <param name="url">图片的URL</param>
        /// <param name="proxyServer">代理服务器</param>
        /// <returns>图片内容</returns>
        public byte[] GetImgFile(string url, string proxyServer)
        {
            WebResponse rsp = null;
            byte[] retBytes = null;

            try
            {
                Uri uri = new Uri(url);
                WebRequest webRequest = WebRequest.Create(uri);

                rsp = webRequest.GetResponse();
                Stream stream = rsp.GetResponseStream();

                if (!string.IsNullOrEmpty(proxyServer))
                {
                    webRequest.Proxy = new WebProxy(proxyServer);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    int b;
                    while ((b = stream.ReadByte()) != -1)
                    {
                        ms.WriteByte((byte)b);
                    }
                    retBytes = ms.ToArray();
                }
            }
            catch
            {
                retBytes = null;
            }
            finally
            {
                if (rsp != null)
                {
                    rsp.Close();
                }
            }
            return retBytes;
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="oriUri"></param>
        /// <param name="saveFileName"></param>
        /// <returns></returns>
        public static bool DownloadFile(string oriUri, string saveFileName)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadFile(oriUri, saveFileName);
                return true;

                Stream str = client.OpenRead(oriUri);

                byte[] mbyte = new byte[100000];
                int allmybyte = (int)mbyte.Length;
                int startmbyte = 0;

                while (allmybyte > 0)
                {
                    int m = str.Read(mbyte, startmbyte, allmybyte);
                    if (m == 0) break;
                    startmbyte += m;
                    allmybyte -= m;
                }

                FileStream fstr = new FileStream(saveFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fstr.Write(mbyte, 0, startmbyte);
                str.Close();
                fstr.Close();

                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }
        /// <summary>
        /// 得到下载文件Stream
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Stream DownloadFile(string url)
        {
            try
            {
                //请求
                System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                //超时时间
                httpRequest.Timeout = TIMEOUT;
                //回传
                System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();

                //得到Stream
                return webResponse.GetResponseStream();
            }
            catch (Exception ex) { }

            return null;
        }
        #endregion

        #region 提交数据【GET】
        /// <summary>
        /// 通过url请求数据
        /// 使用：【HttpWebRequest】
        /// 【无代理服务器】
        /// </summary>
        /// <param name="url">访问页面的URL</param>
        public static void AccessWeb(string url)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;

                HttpWebResponse WebResponse = (HttpWebResponse)webRequest.GetResponse();

                WebResponse.Close();

            }
            catch { }
        }
        /// <summary>
        /// 通过url请求XML数据
        /// 使用：【HttpWebRequest】
        /// 【无代理服务器】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <returns>返回XmlDocument</returns>
        public static XmlDocument GetXML(string url)
        {

            XmlDocument xml = new XmlDocument();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;


                HttpWebResponse WebResponse = (HttpWebResponse)webRequest.GetResponse();

                System.IO.Stream respStream = WebResponse.GetResponseStream();

                xml.Load(respStream);

                respStream.Dispose();
                respStream.Close();

                WebResponse.Close();
            }
            catch (System.Exception)
            {
                return null;
            }

            return xml;
        }
        /// <summary>
        /// 通过url请求数据
        /// 使用：【HttpWebRequest】
        /// 【无代理服务器】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <returns>返回页面代码</returns>
        public static string GetHtml(string url, Encoding encoding = null)
        {
            string html = "";

            try
            {
                if (encoding == null) encoding = Encoding.Default;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;


                HttpWebResponse WebResponse = (HttpWebResponse)webRequest.GetResponse();

                System.IO.Stream respStream = WebResponse.GetResponseStream();
                System.IO.StreamReader reader = new StreamReader(respStream, encoding);

                html = reader.ReadToEnd();

                respStream.Dispose();
                respStream.Close();

                reader.Dispose();
                reader.Close();

                WebResponse.Close();
            }
            catch (System.Exception ex)
            {
                html = ex.Message;
            }

            return html;
        }
        /// <summary>
        /// 通过url请求数据
        /// 【使用：WebRequest】
        /// 【无代理服务器】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <returns>返回页面代码</returns>
        public static string GetHtml2(string url)
        {
            string html = "";

            try
            {
                System.Net.WebRequest webRequest = System.Net.WebRequest.Create(url);
                System.Net.WebResponse webResponse = webRequest.GetResponse();
                System.IO.Stream respStream = webResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(respStream, System.Text.Encoding.Default);

                html = reader.ReadToEnd();

                respStream.Dispose();
                respStream.Close();

                reader.Dispose();
                reader.Close();

                webResponse.Close();
            }
            catch (System.Exception ex)
            {
                html = ex.Message;
            }

            return html;
        }
        /// <summary>
        /// 通过url请求数据
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <param name="proxyServer">代理服务器</param>
        /// <returns>返回页面代码</returns>
        public string GetHtml(string url, string proxyServer)
        {
            if (string.IsNullOrEmpty(proxyServer))
                return GetHtml(url);

            return GetHtml(url, proxyServer, "", "");
        }

        /// <summary>
        /// 通过url请求数据
        /// 【有代理服务器】
        /// </summary>
        /// <param name="url">被请求页面的url</param>
        /// <param name="proxyServer">代理服务器</param>
        /// <param name="un">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>返回页面代码</returns>
        public string GetHtml(string url, string proxyServer, string un, string pwd)
        {
            StringBuilder requestHtml = new StringBuilder("");
            HttpWebResponse webResponse = null;

            try
            {
                Uri uri = new Uri(url);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);

                if (!string.IsNullOrEmpty(proxyServer))
                {
                    WebProxy proxy = new WebProxy(proxyServer);

                    proxy.Credentials = new NetworkCredential(un, pwd);

                    webRequest.PreAuthenticate = true;
                    webRequest.Proxy = proxy;
                }
                else return null;

                webRequest.CookieContainer = this._cookieCon;
                webRequest.Headers.Add("Accept-Language: zh-cn");
                webRequest.AllowAutoRedirect = true;
                webRequest.Timeout = TIMEOUT;

                webResponse = (HttpWebResponse)webRequest.GetResponse();

                Stream rspStream = webResponse.GetResponseStream();
                StreamReader sr = new StreamReader(rspStream, System.Text.Encoding.Default);

                //获取数据
                Char[] read = new Char[256];
                int count = sr.Read(read, 0, 256);

                while (count > 0)
                {
                    requestHtml.Append(read, 0, count);
                    count = sr.Read(read, 0, 256);
                }
            }
            catch (Exception e)
            {
                requestHtml.Append(e.Message);
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                }
            }

            return requestHtml.ToString();
        }
        /// <summary>
        /// 获取HTML代码
        /// 可保存状态
        /// </summary>
        public static string GetHtmlWithState(string url, ref string cookies, System.Text.Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (encoding == null)
            {
                encoding = System.Text.Encoding.Default;
            }

            try
            {
                string server = url.Substring(0, url.IndexOf('/', 7));

                System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                if (!string.IsNullOrEmpty(cookies))
                {
                    cookies = cookies.Trim(';');

                    CookieContainer cookieContainer = new CookieContainer();
                    cookieContainer.SetCookies(new Uri(server), cookies);
                    httpRequest.CookieContainer = cookieContainer;
                }

                httpRequest.Timeout = TIMEOUT;
                System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)httpRequest.GetResponse();

                cookies += ";" + webResponse.Headers.Get("Set-Cookie");
                cookies = cookies.Trim(';');

                System.IO.Stream respStream = webResponse.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(respStream, encoding);

                string html = reader.ReadToEnd();

                respStream.Dispose();
                respStream.Close();

                reader.Dispose();
                reader.Close();

                webResponse.Close();

                return html;
            }
            catch { }

            return null;
        }

        #endregion

        #region 提交数据【POST】
        /// <summary>
        /// Post提交数据
        /// </summary>
        /// <param name="url">目标url</param>
        /// <param name="data">要post的数据</param>
        /// <returns>服务器响应</returns>
        public static string PostData(string url, string data, string cookies = null)
        {
            Encoding encoding = Encoding.UTF8;
            byte[] bytesToPost = encoding.GetBytes(data);
            return PostData(bytesToPost, url, cookies);
        }
        /// <summary>
        /// 通过URL提交数据[Post]
        /// </summary>
        /// <param name="data">要post的数据</param>
        /// <param name="url">目标url</param>
        /// <returns>服务器响应</returns>
        private static string PostData(byte[] data, string url, string cookies)
        {
            //创建httpWebRequest对象
            System.Net.WebRequest webRequest = System.Net.WebRequest.Create(url);
            System.Net.HttpWebRequest httpRequest = webRequest as System.Net.HttpWebRequest;
            if (!string.IsNullOrEmpty(cookies))
            {
                cookies = cookies.Trim(';');

                string domain = url.Substring(0, url.IndexOf("/", 8));
                CookieContainer cookieContainer = new CookieContainer();
                cookieContainer.SetCookies(new Uri(domain), cookies);
                httpRequest.CookieContainer = cookieContainer;
            }

            if (httpRequest == null)
            {
                throw new ApplicationException(string.Format("Invalid url string: {0}", url));
            }

            //填充httpWebRequest的基本信息
            httpRequest.UserAgent = sUserAgent;
            httpRequest.ContentType = sContentType;
            httpRequest.Method = "POST";

            //填充并发送要post的内容
            httpRequest.ContentLength = data.Length;
            Stream requestStream = httpRequest.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            //发送post请求到服务器并读取服务器返回信息
            Stream responseStream = null; ;
            try
            {
                responseStream = httpRequest.GetResponse().GetResponseStream();
            }
            catch (Exception e)
            {
                throw e;
            }
            //读取服务器返回信息
            string stringResponse = string.Empty;

            using (StreamReader responseReader = new StreamReader(responseStream, Encoding.GetEncoding(sResponseEncoding)))
            {
                stringResponse = responseReader.ReadToEnd();
            }

            responseStream.Close();

            return stringResponse;

        }

        /// <summary>
        /// 得到Cookies
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCookies(string url)
        {
            string cookies = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ContentType = sContentType;
                webRequest.Method = "get";
                webRequest.Accept = sAccept;
                webRequest.UserAgent = sUserAgent;

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                cookies = webResponse.Headers.Get("Set-Cookie");

                webResponse.Close();
            }
            catch { }

            return cookies;
        }
        #endregion

        #region PostXML
        /// <summary>
        /// PostXml
        /// </summary>
        /// <param name="url"></param>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static string PostXml(string url, string xmlData)
        {
            //加密
            xmlData = Encryption.EncryptionDES.Encrypt(xmlData);
            //文件数据
            byte[] fileData = Encoding.UTF8.GetBytes(xmlData);
            //Post提交
            return WebHandler.PostFile(url, fileData);
        }
        ///// <summary>
        ///// 得到上传的内容
        ///// </summary>
        ///// <returns></returns>
        //public static XmlDocument GetPostData()
        //{
        //    try
        //    {
        //        System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;

        //        //无提交的数据
        //        if (request.InputStream.Length == 0) return null;


        //        //上传的内容
        //        Stream stream = request.InputStream;
        //        if (stream.Length <= 0) return null;

        //        //得到上传内容
        //        byte[] bytes = new byte[stream.Length];
        //        stream.Read(bytes, 0, bytes.Length);
        //        stream.Seek(0, SeekOrigin.Begin);

        //        //得到字符内容
        //        string xmlData = Encoding.UTF8.GetString(bytes);

        //        //解密XML内容
        //        xmlData = Encryption.Encryption64.Decrypt(xmlData);

        //        //XML格式
        //        XmlDocument xml = new XmlDocument();
        //        xml.LoadXml(xmlData);

        //        return xml;
        //    }
        //    catch { }

        //    return null;
        //}
        #endregion

        #region 上传文件
        /// <summary>
        /// 获取文件Bytes
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static byte[] FileToBytes(string fileName)
        {
            //打开文件
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            try
            {
                //读取字节
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);

                //返回
                return buffur;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    //关闭资源
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 上传Stream文件
        /// </summary>
        /// <param name="serverDomain">服务器地址</param>
        /// <param name="stream"></param>
        /// <param name="ext"></param>
        /// <param name="domain"></param>
        /// <param name="thumbs"></param>
        /// <returns></returns>
        public static string UploadFile(string serverDomain, Stream stream, string ext, string domain, string thumbs = null)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return UploadFile(serverDomain, bytes, ext, domain, thumbs);
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="serverDomain">服务器地址</param>
        /// <param name="postFile">要上传的文件Byte[]数据</param>
        /// <param name="domain">当前网站域名</param>
        /// <param name="ext">文件后缀名</param>
        /// <param name="thumbs">缩略图片</param>
        /// <returns></returns>
        public static string UploadFile(string serverDomain, byte[] postFile, string ext, string domain, string thumbs = null)
        {
            string sig = "wsfly.com|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + domain;
            sig = EncryptionHandler.Encode(sig);
            sig = StringHandler.EncodeValue(sig);
            sig = sig.Replace("+", "-");
            string postUrl = serverDomain.Trim('/') + "/Sys/File/UploadFileByte.ashx?ext=" + ext + "&sig=" + sig;
            if (!string.IsNullOrWhiteSpace(thumbs))
            {
                //100x100,200x200,300x300
                postUrl += "&thumbs=" + thumbs.ToLower();
            }

            try
            {
                //新实例
                WebClient webclient = new WebClient();
                //上传文件
                byte[] buffer = webclient.UploadData(postUrl, "POST", postFile);
                //返回数据
                string resultData = Encoding.UTF8.GetString(buffer);
                //返回
                return resultData;
            }
            catch (Exception) { return ""; }
        }
        /// <summary>
        /// 上传图片并裁剪图片
        /// </summary>
        /// <param name="serverDomain">服务器域名</param>
        /// <param name="postFile">上传文件</param>
        /// <param name="ext">后缀</param>
        /// <param name="domain">保存目录</param>
        /// <param name="x1">选择区域 X起点坐标</param>
        /// <param name="y1">选择区域 Y起点坐标</param>
        /// <param name="x2">选择区域 X终点坐标</param>
        /// <param name="y2">选择区域 Y终点坐标</param>
        /// <param name="selectWidth">选择区域 宽度</param>
        /// <param name="selectHeight">选择区域 高度</param>
        /// <param name="imgZoomWidth">选择区域缩放宽度</param>
        /// <param name="needSize">需要缩小图片尺码 宽度x高度 如：100x100</param>
        /// <returns></returns>
        public static string UploadImageClipper(string serverDomain, byte[] postFile, string ext, string domain,
            int x1, int y1, int x2, int y2, int selectWidth, int selectHeight, int imgZoomWidth, string needSize)
        {
            string sig = "wsfly.com|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|" + domain;
            sig = EncryptionHandler.Encode(sig);
            sig = StringHandler.EncodeValue(sig);
            sig = sig.Replace("+", "-");
            string postUrl = serverDomain.Trim('/') + "/Sys/File/UploadFileByte.ashx?ext=" + ext + "&sig=" + sig;

            postUrl += "&clipper=1";
            postUrl += "&x1=" + x1;
            postUrl += "&y1=" + y1;
            postUrl += "&x2=" + x2;
            postUrl += "&y2=" + y2;
            postUrl += "&sw=" + selectWidth;
            postUrl += "&sh=" + selectHeight;
            postUrl += "&izw=" + imgZoomWidth;
            postUrl += "&ns=" + needSize;

            try
            {
                ///新实例
                WebClient webclient = new WebClient();
                ///上传文件
                byte[] buffer = webclient.UploadData(postUrl, "POST", postFile);
                ///返回数据
                string resultData = Encoding.UTF8.GetString(buffer);
                ///返回
                return resultData;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 提交文件数据
        /// </summary>
        /// <param name="url">目标url</param>
        /// <param name="data">要post的文件数据</param>
        /// <returns></returns>
        public static string PostFile(string url, byte[] data)
        {
            try
            {
                //新实例
                WebClient webclient = new WebClient();
                //上传文件
                byte[] buffer = webclient.UploadData(url, "POST", data);
                //返回数据
                string resultData = Encoding.UTF8.GetString(buffer);
                //返回
                return resultData;
            }
            catch (Exception ex) { return ""; }
        }
        #endregion

        #region 是否已经联网

        [DllImport("wininet")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        /// <summary>
        /// 检测本机是否联网
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedInternet()
        {
            int i = 0;

            if (InternetGetConnectedState(out i, 0))
            {
                //已联网
                return true;
            }
            else
            {
                //未联网
                return false;
            }
        }
        /// <summary>
        /// Ping IP地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool Ping(string ip)
        {
            System.Net.NetworkInformation.Ping ping;
            System.Net.NetworkInformation.PingReply res;
            ping = new System.Net.NetworkInformation.Ping();

            try
            {
                res = ping.Send(ip);
                if (res.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    return false;
                }

                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        #region 代理服务器
        /// <summary>
        /// 验证代理服务器
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <returns></returns>
        public static bool VerifyProxyServer(string server)
        {
            return VerifyProxyServer(server, null, null);
        }
        /// <summary>
        /// 验证代理服务器
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="un">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public static bool VerifyProxyServer(string server, string un, string pwd)
        {
            //分析IP、Port
            string ip = server.Split(':')[0];
            int port = DataType.Int(server.Split(':')[1], 80);

            ///代理
            WebProxy proxyObject = new WebProxy(ip, port);//str为IP地址 port为端口号
            if (!string.IsNullOrEmpty(un) && !string.IsNullOrEmpty(pwd))
            {
                proxyObject.Credentials = new NetworkCredential(un, pwd);
            }

            ///Web请求
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://www.wsfly.com/");
            HttpWebResponse webResponse = null;

            //设置代理 
            webRequest.Proxy = proxyObject;

            try
            {//服务器是否有响应
                webResponse = (HttpWebResponse)webRequest.GetResponse();
            }
            catch { return false; }

            ///内容与编码
            string content = string.Empty;
            Encoding code = Encoding.GetEncoding("UTF-8");

            ///得到返回内容
            using (StreamReader sr = new StreamReader(webResponse.GetResponseStream(), code))
            {
                if (sr != null)
                {
                    try
                    {
                        content = sr.ReadToEnd();
                        return true;
                    }
                    catch { return false; }
                    finally { sr.Close(); }
                }

                return false;
            }
        }
        #endregion
    }
}
