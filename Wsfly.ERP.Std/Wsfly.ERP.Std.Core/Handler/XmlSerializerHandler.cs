using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class XmlSerializerHandler<T>
    {
        /// <summary>
        /// 转换为XML文件
        /// </summary>
        /// <param name="objInfo"></param>
        /// <param name="path"></param>
        public static bool ToXml(T objInfo, string path)
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(objInfo.GetType());
                TextWriter writer = new StreamWriter(path);
                serializer.Serialize(writer, objInfo);
                writer.Close();

                return true;
            }
            catch(Exception) { }

            return false;
        }
        /// <summary>
        /// 转换为对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T ToObject(string path)
        {
            if (!File.Exists(path)) return default(T);

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                FileStream stream = new FileStream(path, FileMode.Open);
                T obj = (T)serializer.Deserialize(stream);
                stream.Close();
                return obj;
            }
            catch(Exception ex) { }

            return default(T);
        }

        /// <summary> 
        /// 序列化对象 
        /// </summary> 
        /// <typeparam name="T">对象</typeparam> 
        /// <param name="t">对象</param> 
        /// <returns></returns> 
        public static string Serialize(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xz = new XmlSerializer(typeof(T));
                xz.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        /// <summary> 
        /// 反序列化为对象 
        /// </summary> 
        /// <param name="type">对象类型</param> 
        /// <param name="s">对象序列化后的Xml字符串</param> 
        /// <returns></returns> 
        public static T Deserialize(string xmlStr)
        {
            using (StringReader sr = new StringReader(xmlStr))
            {
                XmlSerializer xz = new XmlSerializer(typeof(T));
                return (T)xz.Deserialize(sr);
            }
        }
    }
}
