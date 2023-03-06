using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Wsfly.ERP.Std.Core.Base
{
    /// <summary>
    /// WPF窗体基类
    /// </summary>
    public class BaseWindow : System.Windows.Window
    {
        #region 委托
        /// <summary>
        /// 代理
        /// FlushClient fc = new FlushClient(Function);
        /// this.Dispatcher.Invoke(fc, new object[] { param });
        /// </summary>
        public delegate object FlushClientBase(object obj);
        /// <summary>
        /// 代理
        /// this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
        /// {
        ///     //ToDoing
        /// }));
        /// </summary>
        public delegate void FlushClientBaseDelegate();

        /// <summary>
        /// WsflyBase事件
        /// </summary>
        protected event WsflyBaseEventHandler WsflyBaseEvent;
        /// <summary>
        /// WsflyBase委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        protected delegate object WsflyBaseEventHandler(object sender, object parameter);
        #endregion

        #region 属性
        /// <summary>
        /// 父窗体
        /// </summary>
        public BaseWindow _ParentWindow { get; set; }
        /// <summary>
        /// 根目录
        /// </summary>
        public string _RootPath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 是否设计模式
        /// </summary>
        public bool IsDesignMode
        {
            get { return System.ComponentModel.DesignerProperties.GetIsInDesignMode(this); }
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public BaseWindow()
        {
            this.Background = Brushes.White;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
    }
}
