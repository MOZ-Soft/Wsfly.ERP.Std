using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class DynamicCallHandler
    {
        /*
            示例：
            static void Main(string[] args)
            {
                //1. 动态加载C++ Dll
                int hModule = DynamicCallHandler.LoadLibrary(@"c:\CppDemo.dll");
                if (hModule == 0) return;

                //2. 读取函数指针
                IntPtr intPtr = DynamicCallHandler.GetProcAddress(hModule, "Add");

                //3. 将函数指针封装成委托
                Add addFunction = (Add)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(Add));

                //4. 测试
                Console.WriteLine(addFunction(1, 2));
                Console.Read();
            }
            
            /// <summary>
            /// 函数指针
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            delegate int Add(int a, int b);
        */

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        public static extern int LoadLibrary(

        [MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(int hModule,

        [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        public static extern bool FreeLibrary(int hModule);
        
    }
}
