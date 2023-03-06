using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace Wsfly.ERP.Std.Core.Extensions
{
    /// <summary>
    /// 扩展列表
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// 复制列表 - 浅复制
        /// 新对象和原对象指向的地址是一致的，修改值也是一起改变；
        /// </summary>
        /// <returns></returns>
        public static List<T> Copy<T>(this List<T> target)
        {
            if (target == null) return null;

            var newList = new List<T>();
            target.ForEach(p => newList.Add(p));
            return newList;
        }
        /// <summary>
        /// 克隆列表 - 深复制
        /// 修改新对象字段的时候是不会影响到原始对象中对应字段的内容；
        /// </summary>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> target)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, target);
                stream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(stream) as List<T>;
            }
        }
    }
}