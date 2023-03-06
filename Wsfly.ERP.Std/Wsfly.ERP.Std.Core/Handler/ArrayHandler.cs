using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class ArrayHandler
    {
        #region 整型数组操作
        /// <summary>
        /// 合并数组
        /// </summary>
        /// <param name="First">第一个数组</param>
        /// <param name="Second">第二个数组</param>
        /// <returns>合并后的数组(第一个数组+第二个数组，长度为两个数组的长度)</returns>
        public static int[] MergerIntArray(int[] First, int[] Second)
        {
            int[] result = new int[First.Length + Second.Length];
            First.CopyTo(result, 0);
            Second.CopyTo(result, First.Length);
            return result;
        }

        /// <summary>
        /// 数组追加
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="str">字符串</param>
        /// <returns>合并后的数组(数组+字符串)</returns>
        public static int[] AppendIntArray(int[] Source, int Val)
        {
            int[] result = new int[Source.Length + 1];
            Source.CopyTo(result, 0);
            result[Source.Length] = Val;
            return result;
        }

        /// <summary>
        /// 从数组中截取一部分成新的数组
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="StartIndex">原数组的起始位置</param>
        /// <param name="EndIndex">原数组的截止位置</param>
        /// <returns></returns>
        public static int[] SplitIntArray(int[] Source, int StartIndex, int EndIndex)
        {
            try
            {
                int[] result = new int[EndIndex - StartIndex + 1];
                for (int i = 0; i <= EndIndex - StartIndex; i++) result[i] = Source[i + StartIndex];
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 不足长度的前面补0,超出长度的前面部分去掉
        /// </summary>
        /// <param name="First">要处理的数组</param>
        /// <param name="byteLen">数组长度</param>
        /// <returns></returns>
        public static int[] ProcessIntArray(int[] First, int byteLen)
        {
            int[] result;
            if (First.Length > byteLen)
            {
                result = new int[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = First[i + First.Length - byteLen];
                return result;
            }
            else
            {
                result = new int[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = 0;
                First.CopyTo(result, byteLen - First.Length);
                return result;
            }
        }
        #endregion

        #region 字符串数组操作
        /// <summary>
        /// 合并数组
        /// </summary>
        /// <param name="First">第一个数组</param>
        /// <param name="Second">第二个数组</param>
        /// <returns>合并后的数组(第一个数组+第二个数组，长度为两个数组的长度)</returns>
        public static string[] MergerStringArray(string[] First, string[] Second)
        {
            string[] result = new string[First.Length + Second.Length];
            First.CopyTo(result, 0);
            Second.CopyTo(result, First.Length);
            return result;
        }

        /// <summary>
        /// 数组追加
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="str">字符串</param>
        /// <returns>合并后的数组(数组+字符串)</returns>
        public static string[] AppendStringArray(string[] Source, string str)
        {
            string[] result = new string[Source.Length + 1];
            Source.CopyTo(result, 0);
            result[Source.Length] = str;
            return result;
        }

        /// <summary>
        /// 从数组中截取一部分成新的数组
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="StartIndex">原数组的起始位置</param>
        /// <param name="EndIndex">原数组的截止位置</param>
        /// <returns></returns>
        public static string[] SplitArray(string[] Source, int StartIndex, int EndIndex)
        {
            try
            {
                string[] result = new string[EndIndex - StartIndex + 1];
                for (int i = 0; i <= EndIndex - StartIndex; i++) result[i] = Source[i + StartIndex];
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 不足长度的前面补空格,超出长度的前面部分去掉
        /// </summary>
        /// <param name="First">要处理的数组</param>
        /// <param name="byteLen">数组长度</param>
        /// <returns></returns>
        public static string[] ProcessStringArray(string[] First, int byteLen)
        {
            string[] result;
            if (First.Length > byteLen)
            {
                result = new string[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = First[i + First.Length - byteLen];
                return result;
            }
            else
            {
                result = new string[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = " ";
                First.CopyTo(result, byteLen - First.Length);
                return result;
            }
        }
        #endregion
    }
}
