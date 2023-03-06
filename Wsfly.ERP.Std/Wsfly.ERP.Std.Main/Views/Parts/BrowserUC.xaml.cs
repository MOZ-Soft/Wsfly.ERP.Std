
using Wsfly.ERP.Std.AppCode.Base;
using Wsfly.ERP.Std.AppCode.Ctls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// BrowserUC.xaml 的交互逻辑
    /// </summary>
    public partial class BrowserUC : BaseUserControl
    {
        string _url = null;
        public bool _SendFile = false;
        public string _UploadFileDomain = "";

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="url"></param>
        public BrowserUC(string url)
        {
            _url = url;
            InitializeComponent();

            this.Loaded += BrowserUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserUC_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //阻止错误
                SuppressScriptErrors(this.browser);
                //加载页面
                this.browser.Source = new Uri(_url);
            }
            catch { }

            //允许上传文件
            if (_SendFile && !string.IsNullOrWhiteSpace(_UploadFileDomain))
            {
                this.KeyDown += BrowserUC_KeyDown;
            }
        }

        /// <summary>
        /// 剪贴板的内容上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserUC_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                //上传剪贴板的内容到服务器
                string path = UploadClipboardToServer();
                path = path.Trim('|');

                //文件名称
                string name = System.IO.Path.GetFileName(path);
                //将文件发送给Web页面
                SendFileToWeb(path, name);
            }
        }

        /// <summary>
        /// 阻止错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_Error(object sender, System.Windows.Forms.HtmlElementErrorEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 阻止JS错误
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="hide"></param>
        void SuppressScriptErrors(WebBrowser webBrowser, bool hide = true)
        {
            webBrowser.Navigating += (s, e) =>
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fiComWebBrowser == null)
                    return;

                object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
                if (objComWebBrowser == null)
                    return;

                objComWebBrowser.GetType().InvokeMember("Silent", System.Reflection.BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            };
        }

        /// <summary>
        /// 将内容发送给页面
        /// </summary>
        /// <param name="path"></param>
        void SendFileToWeb(string path, string name)
        {
            object[] objects =
            {
                path,
                name
            };
            browser.InvokeScript("ClientUploadFile", objects);
        }

        #region 上传文件到服务器
        /// <summary>
        /// 上传剪贴板内的文件到服务器
        /// </summary>
        /// <returns></returns>
        public string UploadClipboardToServer()
        {
            IDataObject data = System.Windows.Clipboard.GetDataObject();
            object dataFiles = System.Windows.Clipboard.GetData(DataFormats.FileDrop);

            string serverDomain = _UploadFileDomain;
            string result = string.Empty;

            if (dataFiles != null)
            {
                string[] files = (string[])dataFiles;
                foreach (string path in files)
                {
                    //剪贴板的内容为字符 判断是否为路径
                    if (File.Exists(path))
                    {
                        try
                        {
                            //文件Byte
                            byte[] bytes = Core.Handler.WebHandler.FileToBytes(path);
                            string ext = System.IO.Path.GetExtension(path);

                            //上传文件到服务器
                            string sPath = UploadFileToServer(serverDomain, bytes, ext);
                            result += sPath + "|";
                        }
                        catch { }
                    }
                }
            }
            else if (data.GetDataPresent(typeof(System.Drawing.Bitmap)))
            {
                //剪贴板内是图片对象
                var img = (System.Drawing.Bitmap)data.GetData(typeof(System.Drawing.Bitmap));
                if (img == null) return string.Empty;
                result = UploadFileToServer(serverDomain, Bitmap2Byte(img), ".png");
            }

            return result;
        }
        /// <summary>
        /// 图片转Bytes
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] Bitmap2Byte(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
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
        public static string UploadFileToServer(string serverDomain, byte[] postFile, string ext)
        {
            string sig = "wsfly.com|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "|mzerp.client.adv";
            sig = Core.Encryption.EncryptionDES.Encrypt(sig, "!Ke=Quan");
            sig = Core.Handler.StringHandler.EncodeValue(sig);
            sig = sig.Replace("+", "-");

            //上传地址
            string postUrl = serverDomain.Trim('/') + "/Home/FileUpload?ext=" + ext + "&sig=" + sig;

            try
            {
                //新实例
                WebClient webclient = new WebClient();
                //上传文件
                byte[] buffer = webclient.UploadData(postUrl, "POST", postFile);
                //返回数据
                string resultData = Encoding.UTF8.GetString(buffer);

                if (!string.IsNullOrWhiteSpace(resultData))
                {
                    Dictionary<string, object> dicResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(resultData);
                    if (DataType.Bool(dicResult["Success"], false))
                    {
                        //返回路径
                        return dicResult["Data"].ToString();
                    }
                }

                //返回
                return resultData;
            }
            catch (Exception) { return ""; }
        }
        #endregion
    }
}
