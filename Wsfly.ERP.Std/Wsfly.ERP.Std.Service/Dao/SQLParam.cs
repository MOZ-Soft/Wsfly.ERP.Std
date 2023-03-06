using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wsfly.ERP.Std.Service.Dao
{
    #region SQLParam
    /// <summary>
    /// SQL参数
    /// </summary>
    [Serializable]
    public class SQLParam
    {
        /// <summary>
        /// 操作
        /// </summary>
        public Actions Action { get; set; }

        /// <summary>
        /// 数据表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 操作的列
        /// </summary>
        public List<KeyValue> OpreateCells { get; set; }


        /// <summary>
        /// 条件Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 条件Id字符串 如：1,2,3,4,5
        /// </summary>
        public string Ids { get; set; }
        /// <summary>
        /// 条件列表
        /// </summary>
        public List<Where> Wheres { get; set; }
        /// <summary>
        /// 条件SQL
        /// </summary>
        public string WhereSQL { get; set; }
        /// <summary>
        /// 排序列表
        /// </summary>
        public List<OrderBy> OrderBys { get; set; }
        /// <summary>
        /// 排序SQL
        /// </summary>
        public string OrderSQL { get; set; }

        /// <summary>
        /// 前N行
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// 第N页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 分页尺码
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 不分页
        /// </summary>
        public bool DonotPaging { get; set; }

        /// <summary>
        /// 要执行的SQL
        /// </summary>
        public string ExecSQL { get; set; }


        /// <summary>
        /// 返回Id
        /// </summary>
        public long ReturnId { get; set; }
        /// <summary>
        /// 返回执行的SQL
        /// </summary>
        public string ReturnSql { get; set; }
        /// <summary>
        /// 返回总记录数量
        /// </summary>
        public long ReturnCount { get; set; }


        /// <summary>
        /// 主表名称
        /// </summary>
        public string MainTableName { get; set; }
        /// <summary>
        /// 从表名称
        /// </summary>
        public string SubTableName { get; set; }
        /// <summary>
        /// 三表名称
        /// </summary>
        public string ThreeTableName { get; set; }

        /// <summary>
        /// 行索引
        /// </summary>
        public int RowIndex { get; set; }
        /// <summary>
        /// 原行ID
        /// </summary>
        public long _ORGID { get; set; }

        /// <summary>
        /// 获取要更新的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetKeyValue(string key)
        {
            KeyValue kv = OpreateCells.Find(p => p.Key.Equals(key));
            if (kv == null) return null;

            return kv.Value;
        }
    }
    /// <summary>
    /// SQL操作
    /// </summary>
    public enum Actions
    {
        /// <summary>
        /// 执行Insert操作
        /// </summary>
        添加,
        /// <summary>
        /// 执行Update操作
        /// </summary>
        修改,
        /// <summary>
        /// 执行Delete操作
        /// </summary>
        删除,
        /// <summary>
        /// 执行SQL操作
        /// </summary>
        SQL
    }
    #endregion

    #region KeyValue
    /// <summary>
    /// 参数名&参数值
    /// </summary>
    [Serializable]
    public class KeyValue
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public KeyValue(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
    #endregion

    #region Where
    /// <summary>
    /// 查询条件
    /// </summary>
    [Serializable]
    public class Where
    {
        /// <summary>
        /// 查询列
        /// </summary>
        public string CellName { get; set; }
        /// <summary>
        /// 查询值
        /// </summary>
        public object CellValue { get; set; }
        /// <summary>
        /// 查询类型
        /// </summary>
        public WhereType Type { get; set; }
        /// <summary>
        /// 条件并列
        /// </summary>
        public ParallelType Parallel { get; set; }
        /// <summary>
        /// 操作符
        /// </summary>
        public string TypeSQL
        {
            get
            {
                switch (Type)
                {
                    case WhereType.大于: return ">";
                    case WhereType.大于等于: return ">=";
                    case WhereType.小于: return "<";
                    case WhereType.小于等于: return "<=";
                    case WhereType.不等于: return "<>";
                    case WhereType.模糊查询:
                    case WhereType.模糊前:
                    case WhereType.模糊后: return "like";
                    case WhereType.包含: return "in";
                    case WhereType.不包含: return "not in";
                    default: return "=";
                }
            }
        }
        /// <summary>
        /// 条件SQL
        /// </summary>
        public string WhereSQL
        {
            get
            {
                if (Type == WhereType.包含) return "[" + CellName + "] " + TypeSQL + " ({0}) ";
                else if (Type == WhereType.不包含) return "[" + CellName + "] " + TypeSQL + " ({0}) ";
                else if (Type == WhereType.空) return "[" + CellName + "] is null ";
                else if (Type == WhereType.非空) return "[" + CellName + "] is not null ";

                return "[" + CellName + "] " + TypeSQL + " @" + CellName;
            }
        }

        /// <summary>
        /// 默认构造
        /// </summary>
        public Where() { }
        /// <summary>
        /// 带参构造
        /// </summary>
        /// <param name="name">列名</param>
        /// <param name="value">查询值</param>
        /// <param name="whereType">条件类型</param>
        /// <param name="parallelType">并列条件类型</param>
        public Where(string name, object value, WhereType whereType = WhereType.相等, ParallelType parallelType = ParallelType.And)
        {
            CellName = name;
            CellValue = value;
            Type = whereType;
            Parallel = parallelType;
        }
    }

    /// <summary>
    /// 条件类型
    /// </summary>
    public enum WhereType
    {
        相等,
        大于,
        大于等于,
        小于,
        小于等于,
        不等于,
        模糊查询,
        模糊前,
        模糊后,
        包含,
        不包含,
        空,
        非空,
        左括号,
        右括号
    }

    /// <summary>
    /// 条件并列
    /// </summary>
    public enum ParallelType
    {
        And,
        Or
    }
    #endregion

    #region OrderBy
    /// <summary>
    /// 排序类型
    /// </summary>
    [Serializable]
    public class OrderBy
    {
        /// <summary>
        /// 排序列
        /// </summary>
        public string CellName { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// 排序SQL
        /// </summary>
        public string TypeSQL
        {
            get
            {
                if (Type == OrderType.顺序) return "asc";
                return "desc";
            }
        }

        /// <summary>
        /// 构造
        /// </summary>
        public OrderBy()
        {

        }
        /// <summary>
        /// 排序条件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public OrderBy(string name, OrderType type)
        {
            CellName = name;
            Type = type;
        }
    }
    /// <summary>
    /// 排序类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 顺序排序
        /// </summary>
        顺序 = 0,
        /// <summary>
        /// 倒序排序
        /// </summary>
        倒序 = 1,
        /// <summary>
        /// 随机排序
        /// ORDER BY NEWID() 
        /// </summary>
        随机 = 2
    }
    #endregion
}
