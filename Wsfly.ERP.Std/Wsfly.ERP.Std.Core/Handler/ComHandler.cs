using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class ComHandler
    {
        #region 调用com组件，功能通用函数
        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        /// <param name="vlaue"></param>
        public static void SetComProperty(string name, object o, object vlaue)
        {
            Type t = o.GetType();
            t.InvokeMember(name, BindingFlags.Instance | BindingFlags.SetProperty, null, o, new object[] { vlaue });
        }
        /// <summary>
        /// 取得属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object GetComPropery(string name, object o)
        {
            Type t = o.GetType();
            return t.InvokeMember(name, BindingFlags.Instance | BindingFlags.GetProperty, null, o, null);
        }
        /// <summary>
        /// 调用方法函授
        /// </summary>
        /// <param name="name"></param>
        /// <param name="o"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public static object CallComMethod(string name, object o, params object[] parms)
        {
            Type t = o.GetType();

            return t.InvokeMember(name, BindingFlags.Instance | BindingFlags.InvokeMethod, null, o, parms);
        }
        /// <summary>
        /// 创建 com 对象
        /// </summary>
        /// <param name="FromProgID"></param>
        /// <returns></returns>
        public static object CreateComObject(string FromProgID)
        {
            Type comType = Type.GetTypeFromProgID(FromProgID);
            object rVar = null;
            if (comType != null)
                rVar = System.Activator.CreateInstance(comType);

            return rVar;
        }
        #endregion
    }
}
