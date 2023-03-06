using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Json;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// Json���л�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSerializer<T>
    {
        #region 
        /// <summary>
        /// �������л�ΪJson�ַ���
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToJson(T obj)
        {
            TextWriter tw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(tw);
            (new JsonSerializer()).Serialize(writer, obj);

            return tw.ToString();
        }
        #endregion



        /// <summary>
        /// ��Json�ַ������л�Ϊ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IList<T> JsonTo(string json)
        {
            JsonReader reader = new JsonTextReader(new StringReader(json));
            IList<T> objs = (IList<T>)(new JsonSerializer()).Deserialize(reader, typeof(T));

            return objs;
        }
        /// <summary>
        /// ��Json�ַ������л�Ϊ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject(string json)
        {
            JsonReader reader = new JsonTextReader(new StringReader(json));
            return (T)(new JsonSerializer()).Deserialize(reader, typeof(T));
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
        public static string ToJson(List<T> list)
        {
            return ObjectsToJson(list, null);
        }
        /// <summary>
        /// ����ʵ�����б�Json�ַ���
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(List<T> list, string attrNames)
        {
            return ObjectsToJson(list, attrNames);
        }
        /// <summary>
        /// ����ʵ�����б�Json�ַ���
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(IList<T> list)
        {
            return ObjectsToJson((List<T>)list, null);
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