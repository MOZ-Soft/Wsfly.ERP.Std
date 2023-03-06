using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 磁盘Handler
    /// </summary>
    public class DiskHandler
    {
        ///  <summary> 
        /// 获取指定驱动器的空间总大小(单位为MB) 
        ///  </summary> 
        ///  <param name="dickName">只需输入代表驱动器的字母即可 （大写）</param> 
        ///  <returns> </returns> 
        public static long GetHardDiskSpace(string dickName)
        {
            long totalSize = new long();
            dickName = dickName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == dickName)
                {
                    totalSize = drive.TotalSize / (1024 * 1024);
					break;
                }
            }
            return totalSize;
        }

        ///  <summary> 
        /// 获取指定驱动器的剩余空间总大小(单位为MB) 
        ///  </summary> 
        ///  <param name="dickName">只需输入代表驱动器的字母即可 </param> 
        ///  <returns> </returns> 
        public static long GetHardDiskFreeSpace(string dickName)
        {
            long freeSpace = new long();
            dickName = dickName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == dickName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024);
					break;
                }
            }
            return freeSpace;
        }

    }
}
