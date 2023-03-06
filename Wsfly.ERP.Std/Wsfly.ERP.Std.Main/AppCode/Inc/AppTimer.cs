using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 秒表回调委托
/// </summary>
/// <param name="runIndex"></param>
public delegate void SencodsRun_Deletage(long runIndex);
public delegate void MinuteRun_Deletage(long runIndex);
public delegate void HourRun_Deletage(long runIndex);


/// <summary>
/// 程序定时器
/// </summary>
public class AppTimer
{
    /// <summary>
    /// 每秒执行事件
    /// </summary>
    public static event SencodsRun_Deletage SecondsRun_Event = null;
    /// <summary>
    /// 每分钟执行事件
    /// </summary>
    public static event MinuteRun_Deletage MinuteRun_Event = null;
    /// <summary>
    /// 每小时执行事件
    /// </summary>
    public static event HourRun_Deletage HourRun_Event = null;

    /// <summary>
    /// 定时器
    /// </summary>
    System.Timers.Timer _timerSeconds = null;
    /// <summary>
    /// 执行次数
    /// </summary>
    static long _timerIndex = 0;
    /// <summary>
    /// 是否启动
    /// </summary>
    static bool _isStart = false;


    /// <summary>
    /// 构造
    /// </summary>
    private AppTimer()
    {
        ///实例化定时器
        _timerSeconds = new System.Timers.Timer();
        _timerSeconds.Interval = 1000;
        _timerSeconds.Enabled = true;
        _timerSeconds.Elapsed += _timerSeconds_Elapsed;
        _timerSeconds.Start();
    }
    /// <summary>
    /// 启动
    /// </summary>
    public static void Start()
    {
        if (!_isStart)
        {
            new AppTimer();
        }
    }
    /// <summary>
    /// 每秒执行一次
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _timerSeconds_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        //执行次数加1
        _timerIndex++;

        //是否有注册每秒事件
        if (SecondsRun_Event != null)
        {
            //回调事件
            SecondsRun_Event(_timerIndex);
        }

        //是否有注册每分钟事件
        if (MinuteRun_Event != null && (_timerIndex % 60 == 0))
        {
            //回调
            MinuteRun_Event(_timerIndex);
        }

        //是否有注册每小时事件
        if (HourRun_Event != null && (_timerIndex % 3600 == 0))
        {
            //回调
            HourRun_Event(_timerIndex);
        }
    }
}

