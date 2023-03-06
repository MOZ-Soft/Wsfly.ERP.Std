using KeQuan.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace KeQuan.Client.PC.AppCode.Models
{
    /// <summary>
    /// 事务操作
    /// </summary>
    public class TranOperate
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public TranOperateType Type { get; set; }
        /// <summary>
        /// 操作的行
        /// </summary>
        public DataRowView Row { get; set; }
        /// <summary>
        /// 要插入的对像
        /// </summary>
        public TranInsertParam InsertParam { get; set; }
        /// <summary>
        /// 要更新的对像
        /// </summary>
        public TranUpdateParam UpdateParam { get; set; }
        /// <summary>
        /// 要删除的对像
        /// </summary>
        public TranDeleteParam DeleteParam { get; set; }

        /// <summary>
        /// 是否已经处理
        /// </summary>
        public bool IsProcess { get; set; }
        /// <summary>
        /// 需要保存的行
        /// </summary>
        public NeedSaveRow NeedSaveRow { get; set; }
    }
    /// <summary>
    /// 事务操作类型
    /// </summary>
    public enum TranOperateType
    {
        添加,
        更新,
        删除
    }
}
