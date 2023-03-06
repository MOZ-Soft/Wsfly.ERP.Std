using System;
using System.Collections.Generic;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// 返回JsonModel
    /// </summary>
    public class JsonResult
    {
        bool _success;
        string _message;
        object _data;
        string _action;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        /// <summary>
        /// 数据
        /// </summary>
        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
        /// <summary>
        /// 操作
        /// </summary>
        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }
    }
}
