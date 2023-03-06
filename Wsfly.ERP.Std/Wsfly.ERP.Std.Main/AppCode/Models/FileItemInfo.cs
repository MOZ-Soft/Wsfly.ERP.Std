using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    public class FileItemInfo
    {
        /// <summary>
        /// 系统生成编号 - 不用处理
        /// </summary>
        public string _SysId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 父级编号
        /// </summary>
        public long ParentId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 原路径
        /// </summary>
        public string OrgPath { get; set; }
        /// <summary>
        /// 文件后缀
        /// </summary>
        public string FileExt { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 是否文件夹
        /// </summary>
        public bool IsFloder { get; set; }
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 下载保存目录
        /// </summary>
        public string SaveFloderPath { get; set; }
    }
}
