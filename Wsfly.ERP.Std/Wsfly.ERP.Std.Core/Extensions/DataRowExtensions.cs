using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using Wsfly.ERP.Std.Core.Handler;

namespace Wsfly.ERP.Std.Core.Extensions
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// 得到值
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static object Get(this DataRow target, string columnName)
        {
            if (target == null) return null;
            if (!target.Table.Columns.Contains(columnName)) return null;
            return target[columnName];
        }

        /// <summary>
        /// 得到值：Id列值
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetId(this DataRow target)
        {
            var objVal = target.Get("Id");
            return DataType.Long(objVal, 0);
        }
        /// <summary>
        /// 得到值：ParentId列值
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetParentId(this DataRow target)
        {
            var objVal = target.Get("ParentId");
            return DataType.Long(objVal, 0);
        }

        /// <summary>
        /// 得到值-字符串
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetString(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            if (objVal == null) return string.Empty;
            return objVal.ToString();
        }
        /// <summary>
        /// 得到值-字符串转HTML
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetStringToHtml(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            if (objVal == null) return string.Empty;
            return StringHandler.StringToHtml(objVal.ToString());
        }
        /// <summary>
        /// 得到值-整数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static int GetInt(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Int(objVal, 0);
        }
        /// <summary>
        /// 得到值-长整数
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long GetLong(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Long(objVal, 0);
        }
        /// <summary>
        /// 得到值-Float
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static float GetFloat(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Float(objVal, 0);
        }
        /// <summary>
        /// 得到值-Double
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static double GetDouble(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Double(objVal, 0);
        }
        /// <summary>
        /// 得到值-Double
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDoubleString(this DataRow target, string columnName, string format = "0.00")
        {
            var objVal = target.GetDouble(columnName);
            return objVal.ToString(format);
        }
        /// <summary>
        /// 得到值-Decimal
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static decimal GetDecimal(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Decimal(objVal, 0);
        }
        /// <summary>
        /// 得到值-Decimal
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDecimalString(this DataRow target, string columnName, string format = "0.00")
        {
            var val = target.GetDecimal(columnName);
            return val.ToString(format);
        }
        /// <summary>
        /// 得到值-布尔值
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool GetBool(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.Bool(objVal, false);
        }
        /// <summary>
        /// 得到值-日期
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DateTime GetDateTime(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            return DataType.DateTime(objVal, DateTime.Now);
        }
        /// <summary>
        /// 得到值-日期格式
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string GetDateTimeString(this DataRow target, string columnName, string format = "yyyy-MM-dd HH:mm:ss")
        {
            var objVal = target.Get(columnName);
            return DataType.DateTime(objVal, DateTime.Now).ToString(format);
        }

        /// <summary>
        /// 得到值-集合
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        //[DebuggerStepThrough]
        public static Dictionary<string, dynamic> GetDictionary(this DataRow target, string columnName)
        {
            var objVal = target.Get(columnName);
            if (objVal == null || string.IsNullOrWhiteSpace(objVal.ToString())) return null;

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(objVal.ToString());
            }
            catch (Exception ex) { }

            return null;
        }


        /// <summary>
        /// 转为集合
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Dictionary<string, dynamic> ToDictionary(this DataRow target)
        {
            if (target == null) return null;

            Dictionary<string, dynamic> dicRow = new Dictionary<string, dynamic>();
            foreach (DataColumn column in target.Table.Columns)
            {
                dicRow.Add(column.ColumnName, target[column.ColumnName]);
            }

            return dicRow;
        }

        /// <summary>
        /// 数据行转类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T ToModel<T>(this DataRow target)
        {
            //初始
            T obj = default(T);

            //实例化
            obj = System.Activator.CreateInstance<T>();

            //是否有数据
            if (target == null) return obj;

            //得到所有属性
            PropertyInfo[] pis = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (pis == null) return obj;

            foreach (PropertyInfo pi in pis)
            {
                try
                {
                    //是否行包含属性名列
                    if (!target.Table.Columns.Contains(pi.Name)) continue;
                    //设置值
                    pi.SetValue(obj, target[pi.Name], null);
                }
                catch { }
            }

            return obj;
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool IsDBNull(this DataRow target, string columnName)
        {
            return target == null ||
                !target.Table.Columns.Contains(columnName) ||
                target[columnName] is DBNull ||
                target[columnName] == null;
        }
        /// <summary>
        /// 是否为空 或 空字符串
        /// </summary>
        /// <param name="target"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this DataRow target, string columnName)
        {
            bool flag = target.IsDBNull(columnName);
            if (flag) return true;

            string val = target.GetString(columnName);
            return string.IsNullOrWhiteSpace(val);
        }
        /// <summary>
        /// 克隆行
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static DataTable CloneRow(this DataRow target)
        {
            var dt = target.Table.Clone();
            dt.ImportRow(target);
            return dt;
        }
    }
}