using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.AppCode.Enums
{
    /// <summary>
    /// 列表快捷键
    /// </summary>
    [Serializable]
    public class ListAction
    {
        /// <summary>
        /// 操作事件编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 操作事件代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 操作图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 键值
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// 操作名称
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 操作枚举
        /// </summary>
        public ListActionEnum ActionEnum
        {
            get
            {
                //是否有值
                if (Code <= 0) return ListActionEnum.Null;

                try
                {
                    //枚举类型
                    return (ListActionEnum)Enum.Parse(typeof(ListActionEnum), Code.ToString());
                }
                catch { }

                //没有类型
                return ListActionEnum.Null;
            }
        }
        /// <summary>
        /// 快捷键
        /// </summary>
        public System.Windows.Input.Key? QuickKey
        {
            get
            {
                //没有快捷按键
                if (string.IsNullOrWhiteSpace(KeyName)) return null;

                try
                {
                    //枚举类型
                    return (System.Windows.Input.Key)Enum.Parse(typeof(System.Windows.Input.Key), KeyName.ToString());
                }
                catch { }

                return null;
            }
        }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }
    }
}
