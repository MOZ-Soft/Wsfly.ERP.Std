using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 实现JS可以访问C#函数
    /// --关联方法：this.webBrowser1.ObjectForScripting = new ObjectForScriptingHandler(this);
    /// JS调用方法：var returnValue = window.external.Invok("Login", "SetQQLogin", '{ "param1":"p1","param2":"p2"}');
    /// 窗口调用JS：this.webBrowser1.InvokeScript("js方法名", new object[] { "hello" });
    /// </summary>
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ObjectForScriptingHandler
    {
        //存放webBrowser的容器
        object _objContainer;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="main"></param>
        public ObjectForScriptingHandler(object objContainer)
        {
            _objContainer = objContainer;
        }
        /// <summary>
        /// 执行调用函数
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="function"></param>
        /// <param name="param"></param>
        public object Invok(string handler, string function, string param)
        {
            try
            {
                //类型全名
                string fullName = "Wsfly.Main.AppCode.Apps." + handler + "Handler";
                //当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();
                //实例化类
                object instance = assembly.CreateInstance(fullName);
                //是否存在类
                if (instance == null) return null;
                //得到类型
                Type type = instance.GetType();
                //是否可以得到
                if (type == null) return null;

                ///得到方法
                MethodInfo methodInfo = type.GetMethod(function);
                ///参数列表
                //ParameterInfo[] ps = methodInfo.GetParameters();
                ///参数
                ///第一个参数为 容器 如：window,page,view等
                ///第二个参数为 Js回传的参数
                object[] ps = { _objContainer, param };
                ///执行方法
                return methodInfo.Invoke(instance, ps);
            }
            catch (Exception ex)
            {
                return "error:" + ex.Message;
            }
        }
    }
}
