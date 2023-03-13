using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KeQuan.Services.Engines
{
    /// <summary>
    /// Http引擎
    /// </summary>
    public class HttpEngine
    {
        /// <summary>
        /// 端口
        /// </summary>
        static int _port = 8088;
        /// <summary>
        /// Http 监听器
        /// </summary>
        public static HttpListener _httpListener;

        /// <summary>
        /// 主线程
        /// </summary>
        static Thread _threadMain = null;
        /// <summary>
        /// 是否运行
        /// </summary>
        public static bool IsRuning = false;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            //是否已经启动
            if (IsRuning) return;
            IsRuning = true;


            //线程启动
            _threadMain = new Thread(delegate ()
            {
                try
                {
                    StartHttpListener();
                }
                catch (Exception ex)
                {
                    AppLog.WriteBugLog(ex, "启动API监听异常");
                }
            });
            _threadMain.IsBackground = true;
            _threadMain.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static void Stop()
        {
            if (IsRuning && _threadMain != null)
            {
                try
                {
                    //终止线程
                    _threadMain.Abort();
                }
                catch { }
                finally
                {
                    IsRuning = false;
                }
            }
        }

        /// <summary>
        /// 启动Http监听
        /// </summary>
        private static void StartHttpListener()
        {
            try
            {
                //已经有监听
                if (_httpListener != null)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                    _httpListener = null;
                }

                try
                {
                    AppLog.WriteDebugLog("初始化监听端口：" + _port, "http");

                    //HTTP 协议侦听器
                    _httpListener = new HttpListener();
                    _httpListener.Prefixes.Add("http://+:" + _port + "/");
                    _httpListener.Start();

                    //异步监听客户端请求，当客户端的网络请求到来时会自动执行Result委托
                    //该委托没有返回值，有一个IAsyncResult接口的参数，可通过该参数获取context对象
                    _httpListener.BeginGetContext(Result, null);
                    AppLog.WriteDebugLog("服务端初始化完成！", "http");
                }
                catch (Exception ex)
                {
                    AppLog.WriteDebugLog("服务端初始化（02）异常：" + ex.Message, "http");
                }
            }
            catch (Exception ex)
            {
                AppLog.WriteDebugLog("服务端初始化（01）异常：" + ex.Message, "http");
            }
        }
        /// <summary>
        /// 接收到网络请求
        /// </summary>
        /// <param name="ar"></param>
        private static void Result(IAsyncResult ar)
        {
            System.Threading.Thread threadHttp = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //继续异步监听
                    _httpListener.BeginGetContext(Result, null);

                    //获得context对象
                    var context = _httpListener.EndGetContext(ar);
                    var request = context.Request;
                    var response = context.Response;

                    AppLog.WriteDebugLog("接收到新请求：" + request.Url.ToString(), "httpapi");

                    ////如果是js的ajax请求，还可以设置跨域的ip地址与参数
                    //context.Response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
                    //context.Response.AppendHeader("Access-Control-Allow-Headers", "ID,PW");//后台跨域参数设置，通常设置为配置文件
                    //context.Response.AppendHeader("Access-Control-Allow-Method", "post");//后台跨域请求设置，通常设置为配置文件

                    context.Response.ContentType = "text/plain;charset=utf-8";//返回的ContentType类型为纯文本格式，编码为UTF-8
                    context.Response.AddHeader("Content-type", "text/plain;charset=utf-8");//添加响应头信息

                    //context.Response.ContentType = "application/json";  //返回Json

                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.StatusDescription = "200";
                    context.Response.StatusCode = 200;

                    //处理客户端发送的请求并返回处理信息
                    var result = HandleRequest(request, response);

                    try
                    {
                        //设置客户端返回信息的编码
                        var returnByteArr = Encoding.UTF8.GetBytes(result);
                        using (var stream = response.OutputStream)
                        {
                            //把处理信息返回到客户端
                            stream.Write(returnByteArr, 0, returnByteArr.Length);
                        }

                        AppLog.WriteDebugLog("服务端处理结束", "http");
                    }
                    catch (Exception ex)
                    {
                        AppLog.WriteDebugLog("服务端返回结果异常：" + ex.Message, "http");
                    }
                }
                catch (Exception ex)
                {
                    AppLog.WriteDebugLog("服务端处理请求（01）异常：" + ex.Message, "http");
                }
            });
            threadHttp.IsBackground = true;
            threadHttp.Start();
        }
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string id = request.QueryString["id"];

                return "接收到参数id：" + id;
            }
            catch (Exception ex)
            {
                AppLog.WriteDebugLog("服务端处理请求（02）异常：" + ex.Message, "http");
            }

            return "FAIL";
        }
    }
}
