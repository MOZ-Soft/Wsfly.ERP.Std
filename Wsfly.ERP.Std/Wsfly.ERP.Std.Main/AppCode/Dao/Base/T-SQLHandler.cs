using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace Wsfly.ERP.Std.AppCode.Dao.Base
{
    /// <summary>
    /// ����SQL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TSQLHandler<T>
    {
        #region ����
        private static string _tableName;//����
        private static string _tableColumns;//����
        /// <summary>
        /// ����
        /// </summary>
        public static string _TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    T _default = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
                    //�����ݱ�������ȡ����
                    object tbName = typeof(T).GetProperty("_TableName").GetValue(_default, null);
                    if (tbName == null) return "δ�������ݱ������";
                    _tableName = tbName.ToString().Trim('[').Trim(']');
                }

                return _tableName;
            }
        }
        /// <summary>
        /// ����
        /// </summary>
        public static string _TableColumns
        {
            get
            {
                if (string.IsNullOrEmpty(_tableColumns))
                {
                    T _default = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
                    object tbColumns = typeof(T).GetProperty("_TableColumns").GetValue(_default, null);
                    if (tbColumns == null) return "δ�������ݱ��е�����";
                    _tableColumns = tbColumns.ToString();
                }
                return _tableColumns;
            }
        }
        #endregion


        /// <summary>
        /// �õ���ѯ����SQL
        /// </summary>
        /// <returns></returns>
        public static string GetSelectSql(string where)
        {
            ///��ѯSQL
            string sql = "SELECT * FROM " + _TableName;

            ///��������� ����WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///����SQL
            return sql;
        }
        /// <summary>
        /// �õ���������SQL
        /// </summary>
        /// <returns></returns>
        public static string GetInsertSql()
        {
            ///ȥ�����е�ǰ��[]
            string cells = _TableColumns;
            cells = cells.Trim('[').Trim(']');

            ///����@CellNameģʽ
            cells = "@" + cells.Replace("],[", ",@");

            ///����T-SQL
            string sql = "INSERT INTO [" + _TableName + "](" + _TableColumns + ") VALUES(" + cells + ")";

            ///����
            return sql;
        }
        /// <summary>
        /// �õ���������SQL
        /// </summary>
        /// <returns></returns>
        public static string GetUpdateSql(string where)
        {
            ///SQL
            string sql = "UPDATE [" + _TableName + "] SET ";

            ///���зֳ�����
            string[] cls = _TableColumns.Split(',');

            ///ѭ����װT-SQL [CellName]=@CellName,
            for (int i = 0; i < cls.Length; i++)
            {
                sql += cls[i] + "=@" + cls[i].Trim('[').Trim(']') + ",";
            }

            ///ȥ�����ġ�����
            sql = sql.Trim(',');

            ///��������� ����WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///����T-SQL
            return sql;
        }
        /// <summary>
        /// �õ�ɾ������SQL
        /// </summary>
        /// <returns></returns>
        public static string GetDeleteSql(string where)
        {
            ///ɾ��SQL
            string sql = "DELETE FROM [" + _TableName + "]";

            ///��������� ����WHERE
            if (!string.IsNullOrEmpty(where)) sql += " WHERE " + where;

            ///����SQL
            return sql;
        }


        /// <summary>
        /// ����ת��Ϊ����SQL
        /// �磺
        /// ������[ID],[Name],[Remark]
        /// ���أ�@ID,@Name,@Remark
        /// </summary>
        /// <returns></returns>
        public static string GetParameterSql()
        {
            return "@" + _TableColumns.Replace("],[", ",@").Trim('[').Trim(']');
        }
        /// <summary>
        /// �õ�SQL�Ĳ�����ӳ��
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static SQLiteParameter[] GetParameters(T obj)
        {
            Transform<T> transform = new Transform<T>();
            return transform.ObjectToParameters(obj, _TableColumns);
        }
    }
}
