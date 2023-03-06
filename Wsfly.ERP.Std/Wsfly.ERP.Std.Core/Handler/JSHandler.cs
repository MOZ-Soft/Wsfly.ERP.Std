using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// Javascript 助手
    /// </summary>
    public class JSHandler
    {
        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        public static object Eval(string js)
        {
            try
            {
                JSCaller caller = new JSCaller();
                return caller.Eval(js);
            }
            catch(Exception ex) { }

            return null;
        }
    }
}
