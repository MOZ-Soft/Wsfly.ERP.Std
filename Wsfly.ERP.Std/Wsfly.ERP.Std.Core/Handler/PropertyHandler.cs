using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// PropertyHandler
    /// </summary>
    public class PropertyHandler
    {
        /// <summary>
        /// 得到类的所有属性
        /// </summary>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertyInfoArray(Type type)
        {
            PropertyInfo[] props = null;

            try
            {
                object obj = Activator.CreateInstance(type);
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }
            catch { }

            return props;
        }
        /// <summary>
        /// 数据行转类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T DataRowToModel<T>(System.Data.DataRow row)
        {
            //初始
            T obj = default(T);

            //实例化
            obj = System.Activator.CreateInstance<T>();

            //是否有数据
            if (row == null) return obj;

            //得到所有属性
            PropertyInfo[] pis = GetPropertyInfoArray(typeof(T));
            if (pis == null) return obj;

            foreach (PropertyInfo pi in pis)
            {
                try
                {
                    //是否行包含属性名列
                    if (!row.Table.Columns.Contains(pi.Name)) continue;
                    //设置值
                    pi.SetValue(obj, row[pi.Name], null);
                }
                catch { }
            }

            return obj;
        }
        /// <summary>
        /// 将对象转换为数据行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static System.Data.DataRow ModelToDataRow<T>(T obj)
        {
            //是否有对象
            if (obj == null) return null;

            //得到所有属性
            PropertyInfo[] pis = GetPropertyInfoArray(typeof(T));
            if (pis == null) return null;

            //数据表
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.NewRow();

            foreach (PropertyInfo pi in pis)
            {
                try
                {
                    System.Data.DataColumn col = new System.Data.DataColumn();
                    col.ColumnName = pi.Name;
                    col.DataType = pi.PropertyType;

                    dt.Columns.Add(col);

                    dt.Rows[0][pi.Name] = pi.GetValue(obj, null);
                }
                catch { }
            }

            return dt.Rows[0];
        }
    }
}
