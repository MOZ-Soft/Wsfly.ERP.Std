using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// Json序列化
    /// </summary>
    public class JsonSerializerHandler
    {
        #region JsonTo
        /// <summary>
        /// Json字符串转换为Dictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        /// <summary>
        /// Json字符串转换为Dictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDictionaryObj(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        /// <summary>
        /// Json转为JObject
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JObject JsonToJObject(string json)
        {
            return (JObject)JsonConvert.DeserializeObject(json);
        }
        #endregion

        #region ToJson
        /// <summary>
        /// 对象序列化为json
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            if (obj == null) return null;
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 将DataTable转换为Json字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt)
        {
            return DataTableToJson(dt, null);
        }
        /// <summary>
        /// 将DataTable转换为Json字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt, string attrNames)
        {
            return DataTableToJson(dt, attrNames);
        }
        /// <summary>
        /// 将数据行转换为Json字符串
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string ToJson(DataRow row)
        {
            return DataRowToJson(row, null);
        }
        /// <summary>
        /// 将数据行转换为Json字符串
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string ToJson(DataRow row, string attrNames)
        {
            return DataRowToJson(row, attrNames);
        }

        #region 辅助
        /// <summary>
        /// 将Dictionary转移为Json字符串
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private static string DictionaryToJson(Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Count <= 0) return null;

            string json = "{";

            foreach (string key in dictionary.Keys)
            {
                json += "'" + key + "'" + ":'" + dictionary[key] + "',";
            }

            json = json.Trim(',') + "}";

            return json;
        }
        /// <summary>
        /// 将DataTable转换为Json字符串
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static string DataTableToJson(DataTable dt, string attrNames)
        {
            if (dt == null || dt.Rows.Count <= 0) return null;

            string json = "[";

            foreach (DataRow row in dt.Rows)
            {
                json = DataRowToJson(row, attrNames) + ",";
            }

            json = json.Trim(',') + "]";

            return json;
        }
        /// <summary>
        /// 将数据行转换为Json字符串
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static string DataRowToJson(DataRow row, string attrNames)
        {
            if (row == null || row.Table.Columns.Count <= 0) return null;

            string json = "{";

            attrNames = "," + attrNames.Trim(',') + ",";

            foreach (DataColumn col in row.Table.Columns)
            {
                if (!string.IsNullOrEmpty(attrNames))
                {
                    string colName = "," + col.ColumnName + ",";

                    if (!attrNames.Contains(colName)) continue;
                }

                json += "\"" + col.ColumnName + "\":\"" + row[col] + "\"";
            }

            json = json.Trim(',') + "}";

            return json;
        }
        #endregion

        #endregion
    }
    /// <summary>
    /// Json序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSerializerHandler<T> : JsonSerializerHandler
    {
        /// <summary>
        /// 对象序列化为Json字符串
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// 将Json字符串序列化为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> JsonTo(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
        /// <summary>
        /// 将实体类返回Json字符串
        /// </summary>
        /// <returns></returns>
        public static string ToJson(T model, string attrNames)
        {
            return ObjectToJson(model, attrNames);
        }
        /// <summary>
        /// 返回实体类列表Json字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(IList<T> list)
        {
            return JsonConvert.SerializeObject(list);
        }
        /// <summary>
        /// 返回实体类列表Json字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(IList<T> list, string attrNames)
        {
            return ObjectsToJson(list, attrNames);
        }
        /// <summary>
        /// 将实体类返回Json字符串
        /// </summary>
        /// <returns></returns>
        private static string ObjectToJson(T model, string attrNames)
        {
            if (model == null) return null;

            string json = "{";

            PropertyInfo[] properties = model.GetType().GetProperties();

            if (!string.IsNullOrEmpty(attrNames))
            {
                attrNames = "," + attrNames + ",";
            }

            foreach (PropertyInfo info in properties)
            {
                if (!string.IsNullOrEmpty(attrNames))
                {
                    if (!attrNames.Contains("," + info.Name + ",")) continue;
                }

                Type type = info.GetType();

                try
                {
                    json += "'" + info.Name + "'" + ":'" + info.GetValue(model, null) + "',";
                }
                catch { }
            }

            json = json.Trim(',') + "}";

            return json;
        }
        /// <summary>
        /// 返回实体类列表Json字符串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static string ObjectsToJson(IList<T> list, string attrNames)
        {
            if (list == null || list.Count <= 0) return null;

            string json = "[";

            foreach (T model in list)
            {
                json += ObjectToJson(model, attrNames) + ",";
            }

            json = json.Trim(',') + "]";

            return json;
        }
    }
}
/*
namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}
*/
