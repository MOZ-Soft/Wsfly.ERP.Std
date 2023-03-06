using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 键盘操作
    /// </summary>
    public class KeybordHandler
    {
        /// <summary>
        /// 默认按键配置
        /// </summary>
        static Dictionary<string, int[]> _DEFAULTKEYS = null;

        /// <summary>
        /// 字符串转键盘编码
        /// </summary>
        /// <param name="text"></param>
        public static int[] StrToKeybord(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            if (_DEFAULTKEYS == null) new KeybordHandler();

            List<int> keys = new List<int>();

            foreach (char c in text)
            {
                int[] key = _DEFAULTKEYS["{" + c + "}"];

                foreach (int k in key)
                {
                    keys.Add(k);
                }
            }

            return keys.ToArray();
        }
        /// <summary>
        /// 键盘输入
        /// </summary>
        /// <param name="text"></param>
        public static void KeyboardInput(string text)
        {
            int[] keys = StrToKeybord(text);
            KeyboardDo(keys);
        }

        const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        const uint KEYEVENTF_KEYUP = 0x2;
        /// <summary>
        /// 键盘事件
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwExtraInfo"></param>
        [System.Runtime.InteropServices.DllImport("user32")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        /// <summary>
        /// 按钮状态
        /// </summary>
        /// <param name="nVirtKey"></param>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);
        /// <summary>
        /// 模拟键盘按下
        /// </summary>
        /// <param name="keys"></param>
        public static void KeyboardDo(int[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                int key = keys[i];

                if (key == 16 || key == 17 || key == 18)
                {
                    //Shift/Ctrl/Alt键时 组合键
                    keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    keybd_event((byte)keys[i + 1], 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    keybd_event((byte)keys[i + 1], 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                    keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

                    //跳到下一个
                    i++;
                }
                else
                {
                    //非组合键
                    keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }
            }
        }

        /// <summary>
        /// 构造
        /// </summary>
        private KeybordHandler()
        {
            _DEFAULTKEYS = new Dictionary<string, int[]>();
            _DEFAULTKEYS.Add("{VK_BACK}", new int[] { 8 });//退格键
            _DEFAULTKEYS.Add("{VK_TAB}", new int[] { 9 });//Tab键
            _DEFAULTKEYS.Add("{VK_RETURN}", new int[] { 13 });//回车键
            _DEFAULTKEYS.Add("{VK_SHIFT}", new int[] { 16 });//Shift键
            _DEFAULTKEYS.Add("{VK_CONTROL}", new int[] { 17 });//Ctrl键
            _DEFAULTKEYS.Add("{VK_MENU}", new int[] { 18 });//Alt键
            _DEFAULTKEYS.Add("{VK_PAUSE}", new int[] { 19 });//Pause Break键
            _DEFAULTKEYS.Add("{VK_CAPITAL}", new int[] { 20 });//Caps Lock键
            _DEFAULTKEYS.Add("{VK_ESC}", new int[] { 27 });//ESC键
            _DEFAULTKEYS.Add("{VK_SPACE}", new int[] { 32 });//空格键
            _DEFAULTKEYS.Add("{VK_PRIOR}", new int[] { 33 });//Page Up
            _DEFAULTKEYS.Add("{VK_NEXT}", new int[] { 34 });//Page Down
            _DEFAULTKEYS.Add("{VK_END}", new int[] { 35 });//End键
            _DEFAULTKEYS.Add("{VK_HOME}", new int[] { 36 });//Home键
            _DEFAULTKEYS.Add("{VK_LEFT}", new int[] { 37 });//方向键:←
            _DEFAULTKEYS.Add("{VK_UP}", new int[] { 38 });//方向键:↑
            _DEFAULTKEYS.Add("{VK_RIGHT}", new int[] { 39 });//方向键:→
            _DEFAULTKEYS.Add("{VK_DOWN}", new int[] { 40 });//方向键:↓
            _DEFAULTKEYS.Add("{VK_INSERT}", new int[] { 45 });//Insert键
            _DEFAULTKEYS.Add("{VK_DELETE}", new int[] { 46 });//Delete键
            _DEFAULTKEYS.Add("{VK_LWIN}", new int[] { 91 });//左徽标键
            _DEFAULTKEYS.Add("{VK_RWIN}", new int[] { 92 });//右徽标键
            _DEFAULTKEYS.Add("{VK_APPS}", new int[] { 93 });//鼠标右键快捷键

            //小键盘
            _DEFAULTKEYS.Add("{0}", new int[] { 96 });//小键盘0
            _DEFAULTKEYS.Add("{1}", new int[] { 97 });//小键盘1
            _DEFAULTKEYS.Add("{2}", new int[] { 98 });//小键盘2
            _DEFAULTKEYS.Add("{3}", new int[] { 99 });//小键盘3
            _DEFAULTKEYS.Add("{4}", new int[] { 100 });//小键盘4
            _DEFAULTKEYS.Add("{5}", new int[] { 101 });//小键盘5
            _DEFAULTKEYS.Add("{6}", new int[] { 102 });//小键盘6
            _DEFAULTKEYS.Add("{7}", new int[] { 103 });//小键盘7
            _DEFAULTKEYS.Add("{8}", new int[] { 104 });//小键盘8
            _DEFAULTKEYS.Add("{9}", new int[] { 105 });//小键盘9
            _DEFAULTKEYS.Add("{.}", new int[] { 110 });//小键盘.
            _DEFAULTKEYS.Add("{*}", new int[] { 106 });//小键盘*
            _DEFAULTKEYS.Add("{+}", new int[] { 107 });//小键盘+
            _DEFAULTKEYS.Add("{-}", new int[] { 109 });//小键盘-
            _DEFAULTKEYS.Add("{/}", new int[] { 111 });//小键盘/

            //功能键 F1-F12
            _DEFAULTKEYS.Add("{F1}", new int[] { 112 });//F1
            _DEFAULTKEYS.Add("{F2}", new int[] { 113 });//F2
            _DEFAULTKEYS.Add("{F3}", new int[] { 114 });//F3
            _DEFAULTKEYS.Add("{F4}", new int[] { 115 });//F4
            _DEFAULTKEYS.Add("{F5}", new int[] { 116 });//F5
            _DEFAULTKEYS.Add("{F6}", new int[] { 117 });//F6
            _DEFAULTKEYS.Add("{F7}", new int[] { 118 });//F7
            _DEFAULTKEYS.Add("{F8}", new int[] { 119 });//F8
            _DEFAULTKEYS.Add("{F9}", new int[] { 120 });//F9
            _DEFAULTKEYS.Add("{F10}", new int[] { 121 });//F10
            _DEFAULTKEYS.Add("{F11}", new int[] { 122 });//F11
            _DEFAULTKEYS.Add("{F12}", new int[] { 123 });//F12

            _DEFAULTKEYS.Add("{VK_NUMLOCK}", new int[] { 144 });//Num Lock键
            _DEFAULTKEYS.Add("{VK_SCROLL}", new int[] { 145 });//Scroll Lock键

            //字母
            _DEFAULTKEYS.Add("{a}", new int[] { 65 });
            _DEFAULTKEYS.Add("{b}", new int[] { 66 });
            _DEFAULTKEYS.Add("{c}", new int[] { 67 });
            _DEFAULTKEYS.Add("{d}", new int[] { 68 });
            _DEFAULTKEYS.Add("{e}", new int[] { 69 });
            _DEFAULTKEYS.Add("{f}", new int[] { 70 });
            _DEFAULTKEYS.Add("{g}", new int[] { 71 });
            _DEFAULTKEYS.Add("{h}", new int[] { 72 });
            _DEFAULTKEYS.Add("{i}", new int[] { 73 });
            _DEFAULTKEYS.Add("{j}", new int[] { 74 });
            _DEFAULTKEYS.Add("{k}", new int[] { 75 });
            _DEFAULTKEYS.Add("{l}", new int[] { 76 });
            _DEFAULTKEYS.Add("{m}", new int[] { 77 });
            _DEFAULTKEYS.Add("{n}", new int[] { 78 });
            _DEFAULTKEYS.Add("{o}", new int[] { 79 });
            _DEFAULTKEYS.Add("{p}", new int[] { 80 });
            _DEFAULTKEYS.Add("{q}", new int[] { 81 });
            _DEFAULTKEYS.Add("{r}", new int[] { 82 });
            _DEFAULTKEYS.Add("{s}", new int[] { 83 });
            _DEFAULTKEYS.Add("{t}", new int[] { 84 });
            _DEFAULTKEYS.Add("{u}", new int[] { 85 });
            _DEFAULTKEYS.Add("{v}", new int[] { 86 });
            _DEFAULTKEYS.Add("{w}", new int[] { 87 });
            _DEFAULTKEYS.Add("{x}", new int[] { 88 });
            _DEFAULTKEYS.Add("{y}", new int[] { 89 });
            _DEFAULTKEYS.Add("{z}", new int[] { 90 });

            _DEFAULTKEYS.Add("{A}", new int[] { 16, 65 });
            _DEFAULTKEYS.Add("{B}", new int[] { 16, 66 });
            _DEFAULTKEYS.Add("{C}", new int[] { 16, 67 });
            _DEFAULTKEYS.Add("{D}", new int[] { 16, 68 });
            _DEFAULTKEYS.Add("{E}", new int[] { 16, 69 });
            _DEFAULTKEYS.Add("{F}", new int[] { 16, 70 });
            _DEFAULTKEYS.Add("{G}", new int[] { 16, 71 });
            _DEFAULTKEYS.Add("{H}", new int[] { 16, 72 });
            _DEFAULTKEYS.Add("{I}", new int[] { 16, 73 });
            _DEFAULTKEYS.Add("{J}", new int[] { 16, 74 });
            _DEFAULTKEYS.Add("{K}", new int[] { 16, 75 });
            _DEFAULTKEYS.Add("{L}", new int[] { 16, 76 });
            _DEFAULTKEYS.Add("{M}", new int[] { 16, 77 });
            _DEFAULTKEYS.Add("{N}", new int[] { 16, 78 });
            _DEFAULTKEYS.Add("{O}", new int[] { 16, 79 });
            _DEFAULTKEYS.Add("{P}", new int[] { 16, 80 });
            _DEFAULTKEYS.Add("{Q}", new int[] { 16, 81 });
            _DEFAULTKEYS.Add("{R}", new int[] { 16, 82 });
            _DEFAULTKEYS.Add("{S}", new int[] { 16, 83 });
            _DEFAULTKEYS.Add("{T}", new int[] { 16, 84 });
            _DEFAULTKEYS.Add("{U}", new int[] { 16, 85 });
            _DEFAULTKEYS.Add("{V}", new int[] { 16, 86 });
            _DEFAULTKEYS.Add("{W}", new int[] { 16, 87 });
            _DEFAULTKEYS.Add("{X}", new int[] { 16, 88 });
            _DEFAULTKEYS.Add("{Y}", new int[] { 16, 89 });
            _DEFAULTKEYS.Add("{Z}", new int[] { 16, 90 });

            //符号
            _DEFAULTKEYS.Add("{;}", new int[] { 186 });
            _DEFAULTKEYS.Add("{=}", new int[] { 187 });
            _DEFAULTKEYS.Add("{,}", new int[] { 188 });
            //_DEFAULTKEYS.Add("{-}", new int[] { 189 });
            //_DEFAULTKEYS.Add("{.}", new int[] { 190 });
            //_DEFAULTKEYS.Add("{/}", new int[] { 191 });
            _DEFAULTKEYS.Add("{`}", new int[] { 192 });
            _DEFAULTKEYS.Add("{[}", new int[] { 219 });
            //_DEFAULTKEYS.Add("{/}", new int[] { 220 });
            _DEFAULTKEYS.Add("{]}", new int[] { 221 });
            _DEFAULTKEYS.Add("{'}", new int[] { 222 });

            _DEFAULTKEYS.Add("{:}", new int[] { 16, 186 });
            //_DEFAULTKEYS.Add("{+}", new int[] { 16, 187 });
            _DEFAULTKEYS.Add("{<}", new int[] { 16, 188 });
            _DEFAULTKEYS.Add("{_}", new int[] { 16, 189 });
            _DEFAULTKEYS.Add("{>}", new int[] { 16, 190 });
            _DEFAULTKEYS.Add("{?}", new int[] { 16, 191 });
            _DEFAULTKEYS.Add("{~}", new int[] { 16, 192 });
            _DEFAULTKEYS.Add("{{}", new int[] { 16, 219 });
            _DEFAULTKEYS.Add("{|}", new int[] { 16, 220 });
            _DEFAULTKEYS.Add("{}}", new int[] { 16, 221 });
            _DEFAULTKEYS.Add("{\"}", new int[] { 16, 222 });


            //数字上面的符号
            _DEFAULTKEYS.Add("{!}", new int[] { 16, 49 });
            _DEFAULTKEYS.Add("{@}", new int[] { 16, 50 });
            _DEFAULTKEYS.Add("{#}", new int[] { 16, 51 });
            _DEFAULTKEYS.Add("{$}", new int[] { 16, 52 });
            _DEFAULTKEYS.Add("{%}", new int[] { 16, 53 });
            _DEFAULTKEYS.Add("{^}", new int[] { 16, 54 });
            _DEFAULTKEYS.Add("{&}", new int[] { 16, 55 });
            //_DEFAULTKEYS.Add("{*}", new int[] { 16, 56 });
            _DEFAULTKEYS.Add("{(}", new int[] { 16, 57 });
            _DEFAULTKEYS.Add("{)}", new int[] { 16, 48 });
        }
    }
}


/*
    VK_BACK: 8, //退格键
    VK_TAB: 9, //TAB键
    VK_RETURN: 13, //回车键
    VK_SHIFT: 16, //Shift键
    VK_CONTROL: 17, //Ctrl键
    VK_MENU: 18, //Alt键
    VK_PAUSE: 19, //Pause Break键
    VK_CAPITAL: 20, //Caps Lock键
    VK_SPACE: 32, //空格键    
    VK_PRIOR: 33, //Page Up        
    VK_NEXT: 34, //Page Down        
    VK_END: 35, //End键
    VK_HOME: 36, //Home键    
    VK_LEFT: 37, //方向键:←
    VK_UP: 38, //方向键:↑
    VK_RIGHT: 39, //方向键:→
    VK_DOWN: 40, //方向键:↓
    VK_INSERT: 45, //Insert键
    VK_DELETE: 46, //Delete键
    //字母表     
    VK_A: 65,
    VK_B: 66,
    VK_C: 67,
    VK_D: 68,
    VK_E: 69,
    VK_F: 70,
    VK_G: 71,
    VK_H: 72,
    VK_I: 73,
    VK_J: 74,
    VK_K: 75,
    VK_L: 76,
    VK_M: 77,
    VK_N: 78,
    VK_O: 79,
    VK_P: 80,
    VK_Q: 81,
    VK_R: 82,
    VK_S: 83,
    VK_T: 84,
    VK_U: 85,
    VK_V: 86,
    VK_W: 87,
    VK_X: 88,
    VK_Y: 89,
    VK_Z: 90,   
    VK_LWIN: 91, //左徽标键
    VK_RWIN: 92, //右徽标键
    VK_APPS: 93, //鼠标右键快捷键
    VK_NUMPAD0: 96, //小键盘0
    VK_NUMPAD0: 97, //小键盘1
    VK_NUMPAD0: 98, //小键盘2
    VK_NUMPAD0: 99, //小键盘3
    VK_NUMPAD0: 100, //小键盘4
    VK_NUMPAD0: 101, //小键盘5
    VK_NUMPAD0: 102, //小键盘6
    VK_NUMPAD0: 103, //小键盘7
    VK_NUMPAD0: 104, //小键盘8
    VK_NUMPAD0: 105, //小键盘9
    VK_DECIMAL: 110, //小键盘.
    VK_MULTIPLY: 106, //小键盘*
    VK_MULTIPLY: 107, //小键盘+
    VK_SUBTRACT: 109, //小键盘-
    VK_DIVIDE: 111, //小键盘/
    VK_F1: 112, //F1键
    VK_F2: 113, //F2键
    VK_F3: 114, //F3键
    VK_F4: 115, //F4键
    VK_F5: 116, //F5键
    VK_F6: 117, //F6键
    VK_F7: 118, //F7键
    VK_F8: 119, //F8键
    VK_F9: 120, //F9键
    VK_F10: 121, //F10键
    VK_F11: 122, //F11键
    VK_F12: 123, //F12键
    VK_NUMLOCK: 144, //Num Lock键
    VK_SCROLL: 145,  //Scroll Lock键
 */