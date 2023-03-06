using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class MemoryHandler
    {
        #region 占用内存情况
        /// <summary>
        /// 写CPU、内存日志
        /// </summary>
        public static void WriteMemoryLog()
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            CurrentProcess.Id.ToString();//PID

            string cpu = ((Double)(CurrentProcess.TotalProcessorTime.TotalMilliseconds - CurrentProcess.UserProcessorTime.TotalMilliseconds)).ToString();//CPU
            string memory = (CurrentProcess.WorkingSet64 / 1024 / 1024).ToString() + "M (" + (CurrentProcess.WorkingSet64 / 1024).ToString() + "KB)";//占用内存
            string threadCount = CurrentProcess.Threads.Count.ToString();//线程 

            AppLog.WriteDebugLog("CPU:" + cpu + "  Memory:" + memory + "  ThreadCount:" + threadCount);
        }
        #endregion
    }
}
