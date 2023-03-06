using System;
using System.Collections.Generic;
using System.Text;


public partial class DataType
{
    #region 数字数据类型转换
    /// <summary>
    /// 整数 Int16
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static short Short(object value, short defaultValue)
    {
        try
        {
            if (value == null) return defaultValue;

            return Convert.ToInt16(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 整型 Int32
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static int Int(object value, int defaultValue)
    {
        try
        {
            if (value == null) return defaultValue;

            return Convert.ToInt32(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为 Int64
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static long Long(object value, long defaultValue)
    {
        try
        {
            if (value == null) return defaultValue;

            return Convert.ToInt64(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为Double
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static double Double(object value, double defaultValue)
    {
        try
        {
            return Convert.ToDouble(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为Float
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static float Float(object value, float defaultValue)
    {
        try
        {
            return float.Parse(value.ToString());
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为十进制
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static decimal Decimal(object value, decimal defaultValue)
    {
        try
        {
            return Convert.ToDecimal(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为Unit
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static uint Uint(object value, uint defaultValue)
    {
        try
        {
            return Convert.ToUInt32(value);
        }
        catch { return defaultValue; }
    }
    #endregion


    /// <summary>
    /// 转换为Boolean
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static bool Bool(object value, bool defaultValue)
    {
        try
        {
            if (value == null) return defaultValue;

            return Convert.ToBoolean(value);
        }
        catch { return defaultValue; }
    }
    /// <summary>
    /// 转换为日期
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static DateTime DateTime(object value, DateTime defaultValue)
    {
        try
        {
            return Convert.ToDateTime(value);
        }
        catch { return defaultValue; }
    }
}
