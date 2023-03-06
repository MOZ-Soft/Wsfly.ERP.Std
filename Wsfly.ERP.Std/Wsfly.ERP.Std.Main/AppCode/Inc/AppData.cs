using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// 程序数据缓存
/// </summary>
public class AppData
{
    /// <summary>
    /// 配置
    /// </summary>
    private static Dictionary<string, object> _appData = new Dictionary<string, object>();

    /// <summary>
    /// 私有
    /// </summary>
    private AppData()
    {

    }
    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="configs"></param>
    public static void Set(Dictionary<string, object> configs)
    {
        _appData = configs;
    }
    /// <summary>
    /// 添加配置
    /// </summary>
    public static void Set(string key, object value)
    {
        try
        {
            if (_appData == null) new AppData();
            if (_appData.ContainsKey(key))
            {
                _appData[key] = value;
                return;
            }

            _appData.Add(key, value);
        }
        catch { }
    }
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object Get(string key)
    {
        try
        {
            if (_appData == null) return null;
            if (!_appData.ContainsKey(key)) return null;

            return _appData[key];
        }
        catch { }

        return null;
    }
    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key)
    {
        return (string)Get(key);
    }
    /// <summary>
    /// 得到图片
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static System.Drawing.Image GetImage(string key)
    {
        return (System.Drawing.Image)Get(key);
    }
    /// <summary>
    /// 清空
    /// </summary>
    public static void Clear()
    {
        if (_appData == null) return;
        _appData.Clear();
    }

    #region 扩展
    /// <summary>
    /// 主要窗口
    /// </summary>
    public static Wsfly.ERP.Std.MainWindow MainWindow
    {
        get { return (Wsfly.ERP.Std.MainWindow)Get("_MZ-ERP_MainWindow"); }
        set { Set("_MZ-ERP_MainWindow", value); }
    }
    #endregion
}
