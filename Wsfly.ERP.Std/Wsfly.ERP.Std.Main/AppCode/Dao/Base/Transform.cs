using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.Data.SQLite;


namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    /// <summary>
    /// 对象转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Transform<T>
    {
        #region 对象转换
        /// <summary>
        /// 将行转换为对象
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public T DataRowToObject(DataRow row)
        {
            ///返回结果
            T result = default(T);//(T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);

            //所有列
            DataColumnCollection dcc = row.Table.Columns;

            //新实例
            if (dcc.Count > 0) result = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);

            for (int i = 0; i < dcc.Count; i++)
            {
                try
                {
                    PropertyInfo propertyInfo = result.GetType().GetProperty(dcc[i].ColumnName);

                    if (propertyInfo != null)
                    {
                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            propertyInfo.SetValue(result, Enum.ToObject(propertyInfo.PropertyType, row[i]), null);
                        }
                        else
                        {
                            propertyInfo.SetValue(result, row[i], null);
                        }
                    }
                }
                catch { }
            }

            return result;
        }
        /// <summary>
        /// 将数据表转换为对象列表
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public IList<T> DataTableToList(DataTable dt)
        {
            ///是否有数据
            if (dt == null || dt.Rows.Count <= 0) return null;
            ///新列表
            IList<T> result = new List<T>();

            ///循环数据行
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    ///转换为对象
                    T obj = DataRowToObject(row);
                    ///添加到结果
                    result.Add(obj);
                }
                catch { }
            }

            return result;
        }
        #endregion

        #region 填充对象
        /// <summary>
        /// 从DataReader读取数据，并构造一个T类型的实体对象的集合
        /// 使用完后关闭DataReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IList<T> FillCollection(IDataReader reader)
        {
            using (reader)
            {
                IList<T> list = new List<T>();

                while (reader.Read())
                {
                    T obj = Activator.CreateInstance<T>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo property = typeof(T).GetProperty(reader.GetName(i));

                        try
                        {
                            if (reader.GetValue(i) != DBNull.Value)
                            {
                                try
                                {
                                    property.SetValue(obj, Convert.ChangeType(reader.GetValue(i), property.PropertyType), null);
                                }
                                catch { property.SetValue(obj, reader.GetValue(i), null); }
                            }
                        }
                        catch{continue; }
                    }

                    list.Add(obj);
                }

                return list;
            }
        }
        /// <summary>
        /// 从DataTable读取数据，并构造一个T类型的实体对象的集合
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public IList<T> FillCollection(DataTable dt)
        {
            IList<T> list = new List<T>();

            for (int rIndex = 0; rIndex < dt.Rows.Count; rIndex++)
            {
                T obj = Activator.CreateInstance<T>();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    PropertyInfo property = typeof(T).GetProperty(dt.Columns[i].ColumnName);

                    try
                    {
                        if (dt.Rows[rIndex][i] != DBNull.Value)
                        {
                            try
                            {
                                property.SetValue(obj, Convert.ChangeType(dt.Rows[rIndex][i], property.PropertyType), null);
                            }
                            catch { property.SetValue(obj, dt.Rows[rIndex][i], null); }
                        }
                    }
                    catch { continue; }
                }

                list.Add(obj);
            }

            return list;
        }
        #endregion

        #region 参数转换
        /// <summary>
        /// 将对象转换为参数数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public SQLiteParameter[] ObjectToParameters(T obj, string tableColumns)
        {
            string[] parameterStr = tableColumns.Replace("[", "").Replace("]", "").Split(',');

            SQLiteParameter[] ps = new SQLiteParameter[parameterStr.Length];

            for (int i = 0; i < parameterStr.Length; i++)
            {
                string cellName = parameterStr[i];

                if (string.IsNullOrEmpty(cellName)) continue;

                PropertyInfo proper = typeof(T).GetProperty(cellName);

                switch (proper.PropertyType.Name)
                {
                    case "String":
                        ps[i] = new SQLiteParameter(cellName, DbType.String);
                        break;
                    case "Int16":
                        ps[i] = new SQLiteParameter(cellName, DbType.Int16);
                        break;
                    case "Int32":
                        ps[i] = new SQLiteParameter(cellName, DbType.Int32);
                        break;
                    case "Int64":
                        ps[i] = new SQLiteParameter(cellName, DbType.Int64);
                        break;
                    case "DateTime":
                        ps[i] = new SQLiteParameter(cellName, DbType.DateTime);
                        break;
                    default:
                        ps[i] = new SQLiteParameter();
                        ps[i].ParameterName = cellName;
                        break;
                }

                ///获取值
                object value = proper.GetValue(obj, null);

                ///空值
                if (value == null || proper.Name.ToLower().Equals("id"))
                {
                    ps[i].Value = DBNull.Value;
                    continue;
                }

                ///插入值
                ps[i].Value = value;
            }

            return ps;
        }
        #endregion
    }
}
