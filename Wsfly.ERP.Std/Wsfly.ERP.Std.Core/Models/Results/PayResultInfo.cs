using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wsfly.ERP.Std.Core.Models.Results
{
    /// <summary>
    /// 返回生成支付结果
    /// </summary>
    [Serializable]
    public class PayResultInfo
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 支付宝支付地址
        /// </summary>
        public string AlipayPayUrl { get; set; }
        /// <summary>
        /// 微信支付地址
        /// </summary>
        public string WeiXinPayUrl { get; set; }
    }
}