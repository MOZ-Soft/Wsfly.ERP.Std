using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Wsfly.ERP.Std.AppCode.Exts
{
    public class VisualTreePlus
    {
        /// <summary>
        /// 得到子控件
        /// 示例：
        /// TextBlock txt = VisualTreePlus.FindFirstVisualChild<TextBox>(itemsControl, "txtName");  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        //public static T FindFirstVisualChild<T>(DependencyObject obj, string childName) where T : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(obj, i);
        //        DependencyProperty dp = new DependencyProperty();
        //        if (child != null && child is T && child.GetValue(dp).ToString() == childName)
        //        {
        //            return (T)child;
        //        }
        //        else
        //        {
        //            T childOfChild = FindFirstVisualChild<T>(child, childName);
        //            if (childOfChild != null)
        //            {
        //                return childOfChild;
        //            }
        //        }
        //    }
        //    return null;
        //}
        /// <summary>
        /// 得到所有子控件
        /// 示例：
        /// DataVisualTreeHelper VTHelper = new DataVisualTreeHelper();  
        /// List<CheckBox> collection = VTHelper.GetChildObjects<CheckBox>(itemsControl, "")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                //指定集合的元素添加到List队尾
                childList.AddRange(GetChildObjects<T>(child, ""));
            }
            return childList;
        }
        /// <summary>
        /// 得到所有子控件
        /// 示例：
        /// DataVisualTreeHelper VTHelper = new DataVisualTreeHelper();  
        /// List<DependencyObject> collection = VTHelper.GetChildren(itemsControl)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<DependencyObject> GetChildren(DependencyObject obj)
        {
            DependencyObject child = null;
            List<DependencyObject> childList = new List<DependencyObject>();
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                //得到子控件
                child = VisualTreeHelper.GetChild(obj, i);

                //添加控件
                childList.Add(child);

                //递归 指定集合的元素添加到List队尾
                childList.AddRange(GetChildren(child));
            }
            return childList;
        }
    }
}
