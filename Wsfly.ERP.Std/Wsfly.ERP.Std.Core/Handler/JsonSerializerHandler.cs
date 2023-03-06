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
    /// Json���л�
    /// </summary>
    public class JsonSerializerHandler
    {
        #region JsonTo
        /// <summary>
        /// Json�ַ���ת��ΪDictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        /// <summary>
        /// Json�ַ���ת��ΪDictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDictionaryObj(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        /// <summary>
        /// JsonתΪJObject
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
        /// �������л�Ϊjson
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            if (obj == null) return null;
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// ��DataTableת��ΪJson�ַ���
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt)
        {
            return DataTableToJson(dt, null);
        }
        /// <summary>
        /// ��DataTableת��ΪJson�ַ���
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToJson(DataTable dt, string attrNames)
        {
            return DataTableToJson(dt, attrNames);
        }
        /// <summary>
        /// ��������ת��ΪJson�ַ���
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string ToJson(DataRow row)
        {
            return DataRowToJson(row, null);
        }
        /// <summary>
        /// ��������ת��ΪJson�ַ���
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string ToJson(DataRow row, string attrNames)
        {
            return DataRowToJson(row, attrNames);
        }

        #region ����
        /// <summary>
        /// ��Dictionaryת��ΪJson�ַ���
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
        /// ��DataTableת��ΪJson�ַ���
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
        /// ��������ת��ΪJson�ַ���
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
    /// Json���л�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSerializerHandler<T> : JsonSerializerHandler
    {
        /// <summary>
        /// �������л�ΪJson�ַ���
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        /// <summary>
        /// ��Json�ַ������л�Ϊ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T> JsonTo(string json)
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
        /// <summary>
        /// ��ʵ���෵��Json�ַ���
        /// </summary>
        /// <returns></returns>
        public static string ToJson(T model, string attrNames)
        {
            return ObjectToJson(model, attrNames);
        }
        /// <summary>
        /// ����ʵ�����б�Json�ַ���
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(IList<T> list)
        {
            return JsonConvert.SerializeObject(list);
        }
        /// <summary>
        /// ����ʵ�����б�Json�ַ���
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(IList<T> list, string attrNames)
        {
            return ObjectsToJson(list, attrNames);
        }
        /// <summary>
        /// ��ʵ���෵��Json�ַ���
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
        /// ����ʵ�����б�Json�ַ���
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
