using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// Ping扩展
    /// </summary>
    public class PingHandler
    {
        private const int TIME_OUT = 100;
        private const int PACKET_SIZE = 512;
        private const int TRY_TIMES = 2;
        private static Regex _reg = new Regex(@"平均\s*\=\s*(?<value>[\d]+?)ms", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public delegate void Pinged_Delegate(string result);
        public event Pinged_Delegate Pinged_Event;

        /// <summary>
        /// 启动测试
        /// </summary>
        /// <param name="strCommandline"></param>
        /// <param name="packetSize"></param>
        /// <returns></returns>
        private float LaunchPing(string strCommandLine, int packetSize)
        {
            Process proc = new Process();
            proc.StartInfo.Arguments = strCommandLine;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "ping.exe";
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            //启动进程
            proc.Start();
            //处理结果
            string strBuffer = proc.StandardOutput.ReadToEnd();
            //关闭进程
            proc.Close();

            //处理结果
            return ParseResult(strBuffer, packetSize);
        }
        /// <summary>
        /// 处理结果
        /// </summary>
        /// <param name="strBuffer"></param>
        /// <param name="packetSize"></param>
        /// <returns></returns>
        private float ParseResult(string strBuffer, int packetSize)
        {
            //Ping结果
            if (Pinged_Event != null) Pinged_Event(strBuffer);

            if (strBuffer.Length < 1) return 0.0F;

            //匹配平均速度
            MatchCollection mc = _reg.Matches(strBuffer);
            if (mc == null || mc.Count < 1 || mc[0].Groups == null) return 0.0F;

            //获取平均时间
            int avg;
            if (!int.TryParse(mc[0].Groups[1].Value, out avg)) return 0.0F;
            if (avg <= 0) return 1024.0F;

            //计算  平均速度(kbps/s)=包大小/平均时间(毫秒)/1000/1024
            //1000 将毫秒转为秒
            //1024 将b转为kb
            return (float)packetSize / avg * 1000 / 1024;
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="strHost">主机名或ip</param>
        /// <returns>kbps/s</returns>
        public float Test(string strHost)
        {
            return LaunchPing(string.Format("{0} -n {1} -l {2} -w {3}", strHost, TRY_TIMES, PACKET_SIZE, TIME_OUT), PACKET_SIZE);
        }
        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="strHost">主机名或ip</param>
        /// <param name="packetSize">发送测试包大小</param>
        /// <param name="timeOut">超时</param>
        /// <param name="tryTimes">测试次数</param>
        /// <returns>kbps/s</returns>
        public float Test(string strHost, int packetSize, int timeOut, int tryTimes)
        {
            return LaunchPing(string.Format("{0} -n {1} -l {2} -w {3}", strHost, tryTimes, packetSize, timeOut), packetSize);
        }
    }
}