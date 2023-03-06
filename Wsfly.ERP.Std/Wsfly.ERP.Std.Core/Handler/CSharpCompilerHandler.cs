using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// C#动态编译代码
    /// </summary>
    public class CSharpCompilerHandler
    {
        /// <summary>
        /// 编译
        /// </summary>
        /// <param name="code"></param>
        public static Assembly Compiler(string code)
        {
            //代码生成器
            CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider();

            //编译的参数
            CompilerParameters compilerParameters = new CompilerParameters();
            //compilerParameters.ReferencedAssemblies.AddRange();
            compilerParameters.CompilerOptions = "/t:library";
            compilerParameters.GenerateInMemory = true;

            //开始编译
            CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(compilerParameters, code);
            if (compilerResults.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("编译错误：");
                foreach (CompilerError error in compilerResults.Errors)
                {
                    sb.AppendLine(error.ErrorText);
                }
                throw new Exception(sb.ToString());
            }

            //得到程序集
            return compilerResults.CompiledAssembly;

            //调用示例
            //Assembly assembly = compilerResults.CompiledAssembly;
            //Type type = assembly.GetType("ExpressionCalculate");
            //MethodInfo method = type.GetMethod("Calculate");
            //return method.Invoke(null, null);
        }
        /// <summary>
        /// 编译2
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Assembly Compiler2(string code)
        {
            // 1.CSharpCodePrivoder
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

            // 2.ICodeComplier
            //ICodeCompiler objICodeCompiler = objCSharpCodePrivoder.CreateCompiler();

            // 3.CompilerParameters
            CompilerParameters objCompilerParameters = new CompilerParameters();
            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            // 4.CompilerResults
            CompilerResults cr = objCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, code);

            if (cr.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("编译错误：");

                foreach (CompilerError error in cr.Errors)
                {
                    sb.AppendLine(error.ErrorText);
                }

                throw new Exception(sb.ToString());
            }
            else
            {
                // 通过反射，调用方法
                return cr.CompiledAssembly;

                //Assembly objAssembly = cr.CompiledAssembly;
                //object objHelloWorld = objAssembly.CreateInstance("DynamicCodeGenerate.HelloWorld");
                //MethodInfo objMI = objHelloWorld.GetType().GetMethod("OutPut");
                //objMI.Invoke(objHelloWorld, null);
            }
        }
        /// <summary>
        /// 包装代码
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string WrapExpression(string expression)
        {
            string code = @"
                using System;

                class ExpressionCalculate
                {
                    public static DateTime dt_start = Convert.ToDateTime(""{start_dt}"");
                    public static DateTime dt_end = Convert.ToDateTime(""{end_dt}"");
                    public static DateTime dt_now = DateTime.Now;

                    public static object Calculate()
                    {
                        return {0};
                    }
                }
            ";

            return code.Replace("{0}", expression);
        }
    }
}
