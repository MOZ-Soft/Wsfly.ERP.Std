using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class SortHandler
    {
        /// <summary>
        /// 整型数组排序
        /// </summary>
        /// <param name="arr">整型数组</param>
        /// <param name="isMaxToMin">是否从大到小</param>
        public static void IntArraySort(int[] arr, bool isMaxToMin = true)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                int minIndex = i;

                for (int j = arr.Length - 1; j > i; j--)
                {
                    if (isMaxToMin)
                    {
                        //从大到小
                        if (arr[j] > arr[minIndex])
                        {
                            minIndex = j;
                        }
                    }
                    else
                    {
                        //从小到大
                        if (arr[j] < arr[minIndex])
                        {
                            minIndex = j;
                        }
                    }
                }
                convert(ref arr[i], ref arr[minIndex]);
            }
        }
        /// <summary>
        /// 比较两个数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void convert(ref int a, ref int b)
        {
            int temp = 0;
            temp = a;
            a = b;
            b = temp;
        }
    }
}
