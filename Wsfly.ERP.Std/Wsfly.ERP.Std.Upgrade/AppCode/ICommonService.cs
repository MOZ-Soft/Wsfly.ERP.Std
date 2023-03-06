using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Wsfly.ERP.Std.Upgrade.AppCode
{
    /// <summary>
    /// ICommonService
    /// </summary>
    [ServiceContract]
    public interface ICommonService
    {
        /// <summary>
        /// 获取客户端版号
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetClientVersion();
        /// <summary>
        /// 获取客户端新版本配置信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetClientAppXml();
        /// <summary>
        /// 获取客户端升级文件二进制数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="position"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        [OperationContract]
        byte[] GetClientUpgradeFile(string fileName, long position, out long totalCount);
    }
}
