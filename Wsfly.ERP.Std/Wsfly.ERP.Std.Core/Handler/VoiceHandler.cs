using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class VoiceHandler
    {
        const int SpFlags = 1;                          //SpeechVoiceSpeakFlags.SVSFlagsAsyn

        static object _spVoiceCls = null;               //保存朗读用的 SAPI.SpVoice 
        static object _oISpeechObjectTokens = null;     //保存 SAPI.ISpeechObjectTokens 就是系统有的语音引擎集合
        static int _tokensCount = 0;                    //语音引擎集合数
        public static DictionaryEntry[] _deTokens = null;   //语音模块
        static string _currentTokenName = null;             //当前语音模块名称

        /// <summary>
        /// 当前语音模块
        /// </summary>
        public static object CurrentToken
        {
            get
            {
                //未设置播放语音模块 使用默认
                if (string.IsNullOrWhiteSpace(_currentTokenName)) return _deTokens[0];

                //循环得到名称
                foreach (DictionaryEntry entry in _deTokens)
                {
                    if (entry.Key.ToString() == _currentTokenName)
                    {
                        return entry;
                    }
                }

                //返回默认
                return _deTokens[0];
            }
            set
            {
                //当前语音模块
                _currentTokenName = value.ToString();
            }
        }

        //是否初始化
        static bool isInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        private VoiceHandler()
        {
            InitSAPI();
            isInit = true;
        }
        /// <summary>
        /// 初始化 API
        /// </summary>
        public static void InitSAPI()
        {
            //如果已经初始化 则退出
            if (isInit) return;

            //创建语音对象朗读用
            _spVoiceCls = ComHandler.CreateComObject("SAPI.SpVoice");

            if (_spVoiceCls == null)
            {
                throw new Exception("您还未安装微软语音插件！");
            }
            else
            {
                //取得所有的 识别对象模块集合
                //取得SAPI.ISpeechObjectTokens
                _oISpeechObjectTokens = ComHandler.CallComMethod("GetVoices", _spVoiceCls);

                //识别对象集合 Count;
                object r = ComHandler.GetComPropery("Count", _oISpeechObjectTokens);
                if (r is int)
                {
                    //语言数量
                    _tokensCount = (int)r;

                    //有语言模块
                    if (_tokensCount > 0)
                    {
                        //取得全部语音识别对象模块，及名称，以被以后使用
                        _deTokens = new DictionaryEntry[_tokensCount];
                        for (int i = 0; i < _tokensCount; i++)
                        {
                            //从集合中取出单个 识别对象模块
                            object oSpObjectToken = ComHandler.CallComMethod("Item", _oISpeechObjectTokens, i); //返回 SAPI.SpObjectToken
                            //取名称
                            string name = ComHandler.CallComMethod("GetDescription", oSpObjectToken) as string;
                            //放到 DictionaryEntry 对象中，key 是 名称, value 是 识别对象模块
                            _deTokens[i] = new DictionaryEntry(name, oSpObjectToken);

                        }
                    }
                }
            }
        }
        /// <summary>
        /// 朗读声音，播放声音
        /// </summary>
        /// <param name="msg"></param>
        public static void PlaySound(string msg, int rate = 0, int volume = 100)
        {
            ///初始化
            if (!isInit) { new VoiceHandler(); }

            try
            {
                if (_spVoiceCls != null)
                {
                    //设置语言引擎
                    ComHandler.SetComProperty("Voice", _spVoiceCls, ((DictionaryEntry)CurrentToken).Value);
                    ComHandler.SetComProperty("Rate", _spVoiceCls, rate);
                    ComHandler.SetComProperty("Volume", _spVoiceCls, volume);
                    //调用Speak 函数，msg 是要播放的文本，1 是异步播放,因为是异步的 com 对象不立刻释放
                    ComHandler.CallComMethod("Speak", _spVoiceCls, msg, SpFlags);
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 播放音频文件
        /// </summary>
        /// <param name="stream"></param>
        public static void PlayWav(System.IO.Stream stream)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(stream);
            player.Play();
        }
        /// <summary>
        /// 保存声音文件
        /// </summary>
        /// <param name="msg">播放的语音</param>
        /// <param name="fileName">保存路径</param>
        /// <param name="rate">语速</param>
        /// <param name="volume">音量</param>
        public static void SaveSound(string msg, string fileName, int rate = 0, int volume = 100)
        {
            int SpFileMode = 3;
            object oSpFileStream = ComHandler.CreateComObject("SAPI.SpFileStream"); //创建 SAPI.SpFileStream
            object oSpVoice = ComHandler.CreateComObject("SAPI.SpVoice"); //创建 SAPI.SpVoice

            try
            {
                ComHandler.CallComMethod("Open", oSpFileStream, fileName, SpFileMode, false); //打开流
                ComHandler.SetComProperty("Voice", oSpVoice, ((DictionaryEntry)CurrentToken).Value); //设置 Voice 属性，让谁朗读
                ComHandler.SetComProperty("Rate", oSpVoice, rate);
                ComHandler.SetComProperty("Volume", oSpVoice, volume);
                ComHandler.SetComProperty("AudioOutputStream", oSpVoice, oSpFileStream); //设置流

                ComHandler.CallComMethod("Speak", oSpVoice, msg, SpFlags); //调用 Speak

                ComHandler.CallComMethod("WaitUntilDone", oSpVoice, Timeout.Infinite); //等
                ComHandler.CallComMethod("Close", oSpFileStream); //关闭流
            }
            finally
            {
                Marshal.ReleaseComObject(oSpVoice);
                Marshal.ReleaseComObject(oSpFileStream);
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public static void Dispose()
        {
            //释放com对象
            Marshal.ReleaseComObject(_spVoiceCls);
            //标记未初始化
            isInit = false;
        }
    }
}
