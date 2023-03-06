using System;
using System.Collections.Generic;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Models
{
    /// <summary>
    /// ����JsonModel
    /// </summary>
    public class JsonResult
    {
        bool _success;
        string _message;
        object _data;
        string _action;

        /// <summary>
        /// �Ƿ�ɹ�
        /// </summary>
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }
        /// <summary>
        /// ��Ϣ
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        /// <summary>
        /// ����
        /// </summary>
        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
        /// <summary>
        /// ����
        /// </summary>
        public string Action
        {
            get { return _action; }
            set { _action = value; }
        }
    }
}
