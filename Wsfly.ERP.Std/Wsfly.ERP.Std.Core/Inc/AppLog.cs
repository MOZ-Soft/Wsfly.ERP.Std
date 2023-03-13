using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


/// <summary>
/// 日志助手
/// </summary>
public partial class AppLog
{
    /// <summary>
    /// 根目录
    /// </summary>
    private static string _RootPath = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 写异常日志
    /// </summary>
    /// <param name="ex">异常信息</param>
    /// <param name="des">异常描述</param>
    public static void WriteBugLog(Exception ex, string des = "")
    {
        string log = "[异常]" + des;
        log += "\r\n【Message】" + ex.Message;
        log += "\r\n【Source】\r\n" + ex.Source;
        log += "\r\n【StackTrace】\r\n" + ex.StackTrace;

        //递归子异常
        RecursionInnerException(ex, ref log, 1);

        WriteSysLog(log, "error");
    }
    /// <summary>
    /// 递归列出子异常
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="log"></param>
    /// <param name="level"></param>
    private static void RecursionInnerException(Exception ex, ref string log, int level)
    {
        if (ex.InnerException != null)
        {
            log += "\r\n********************** InnerException " + level + " **********************";
            log += "\r\n【Message】" + ex.InnerException.Message;
            log += "\r\n【Source】\r\n" + ex.InnerException.Source;
            log += "\r\n【StackTrace】\r\n" + ex.InnerException.StackTrace;

            RecursionInnerException(ex.InnerException, ref log, level + 1);
        }
    }
    /// <summary>
    /// 写调试日志
    /// </summary>
    public static void WriteDebugLog(string describe, string fileType = "")
    {
        string log = "[调试]" + describe;
        string fileName = "debug";
        if (!string.IsNullOrWhiteSpace(fileType)) fileName = fileType + "." + fileName;

        WriteSysLog(log, fileName);
    }
    /// <summary>
    /// 写错误日志
    /// </summary>
    public static void WriteErrorLog(string describe, string fileType = "")
    {
        string log = "[异常]" + describe;
        string fileName = "error";
        if (!string.IsNullOrWhiteSpace(fileType)) fileName = fileType + "." + fileName;

        WriteSysLog(log, fileName);
    }
    /// <summary>
    /// 写系统日志
    /// </summary>
    public static void WriteSysLog(string type, string title, string describe)
    {
        string log = "[" + type + "]" + title + (string.IsNullOrEmpty(describe) ? "" : "\r\n[描述]" + describe);

        WriteSysLog(log);
    }
    /// <summary>
    /// 写系统日志
    /// </summary>
    private static void WriteSysLog(string log, string fileName = "sys")
    {
        try
        {
            string wsName = "Wsfly";
            string dirName = "AppLog";
            //文件
            string path = _RootPath + "\\" + dirName + "\\" + wsName + "." + fileName + ".log";

            //如果目录不存在则创建目录
            if (!Directory.Exists(_RootPath + "\\" + dirName + "\\")) Directory.CreateDirectory(_RootPath + "\\" + dirName + "\\");

            //处理内容
            log = "\r\n\r\n=======================================================================\r\n" + log;
            log += "\r\n[日期]" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff");

            //文件是否存在
            //如果文件大小超过10MB则备份
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                long maxLength = 10 * 1024 * 1024;
                if (file.Length >= maxLength)
                {
                    FileMove(path, path.Replace(wsName + "." + fileName + ".log", wsName + "." + fileName + ".bak[" + DateTime.Now.ToString("yyyyMMdd.HHmmss") + "].log"));
                }
            }

            //如果文件存在则追加
            if (File.Exists(path))
            {
                FileAdd(path, log);
            }
            else
            {
                WriteFile(path, log);
            }
        }
        catch { }
    }
    /// <summary>
    /// 写文件
    /// </summary>
    private static void WriteFile(string path, string content, string charset = null)
    {
        Encoding encoding = Encoding.UTF8;

        try
        {
            if (!string.IsNullOrEmpty(charset))
                encoding = Encoding.GetEncoding(charset);
        }
        catch { encoding = Encoding.Default; }

        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        if (!File.Exists(path))
        {
            FileStream f = File.Create(path);
            f.Close();
        }

        StreamWriter f2 = new StreamWriter(path, false, encoding);
        f2.Write(content);

        f2.Close();
        f2.Dispose();
    }
    /// <summary>
    /// 追加文件
    /// </summary>
    private static void FileAdd(string path, string strings)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
        sw.Write(strings);
        sw.Flush();
        sw.Close();
    }
    /// <summary>
    /// 移动文件
    /// </summary>
    private static void FileMove(string orignFile, string newFile)
    {
        if (!File.Exists(orignFile)) return;

        File.Move(orignFile, newFile);
    }
}
