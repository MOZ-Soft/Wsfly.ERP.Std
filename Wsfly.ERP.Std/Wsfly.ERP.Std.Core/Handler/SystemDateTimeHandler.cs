using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 设置/获取系统时间
    /// </summary>
    public class SystemDateTimeHandler
    {
        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <param name="st"></param>
        [DllImportAttribute("kernel32.dll")]
        public static extern void GetLocalTime(SystemTime st);

        /// <summary>
        /// 设置系统时间
        /// </summary>
        /// <param name="st"></param>
        [DllImportAttribute("kernel32.dll")]
        public static extern void SetLocalTime(SystemTime st);
    }

    /// <summary>
    /// 系统时间
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public class SystemTime
    {
        public ushort vYear;
        public ushort vMonth;
        public ushort vDayOfWeek;
        public ushort vDay;
        public ushort vHour;
        public ushort vMinute;
        public ushort vSecond;
    }
}
