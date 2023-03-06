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
                return ExecuteScript(js);
            }
            catch(Exception ex) { }

            return null;
        }
        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="jsExpression">参数体</param>
        /// <returns></returns>
        public static dynamic ExecuteScript(string jsExpression)
        {
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JavaScript";

            //JavaScript代码的字符串
            //scriptControl.AddCode(jsCode);

            try
            {
                return scriptControl.Eval(jsExpression).ToString();
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }

            return null;
        }
    }
}
