using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// �������ݻ���
/// </summary>
public class AppData
{
    /// <summary>
    /// ����
    /// </summary>
    private static Dictionary<string, object> _appData = new Dictionary<string, object>();

    /// <summary>
    /// ˽��
    /// </summary>
    private AppData()
    {

    }
    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="configs"></param>
    public static void Set(Dictionary<string, object> configs)
    {
        _appData = configs;
    }
    /// <summary>
    /// �������
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
    /// ��ȡ����
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
    /// ��ȡ����
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key)
    {
        return (string)Get(key);
    }
    /// <summary>
    /// �õ�ͼƬ
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static System.Drawing.Image GetImage(string key)
    {
        return (System.Drawing.Image)Get(key);
    }
    /// <summary>
    /// ���
    /// </summary>
    public static void Clear()
    {
        if (_appData == null) return;
        _appData.Clear();
    }

    #region ��չ
    /// <summary>
    /// ��Ҫ����
    /// </summary>
    public static Wsfly.ERP.Std.MainWindow MainWindow
    {
        get { return (Wsfly.ERP.Std.MainWindow)Get("_MZ-ERP_MainWindow"); }
        set { Set("_MZ-ERP_MainWindow", value); }
    }
    #endregion
}
