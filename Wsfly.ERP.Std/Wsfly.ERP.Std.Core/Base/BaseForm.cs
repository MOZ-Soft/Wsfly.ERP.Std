using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wsfly.ERP.Std.Core.Base
{
    /// <summary>
    /// Winform窗体基类
    /// </summary>
    public class BaseForm : Form
    {
        #region 委托
        /// <summary>
        /// 代理
        /// FlushClient fc = new FlushClient(Function);
        /// this.Invoke(fc, new object[] { param });
        /// </summary>
        public delegate object FlushClientBase(object obj);
        /// <summary>
        /// 代理
        /// this.Invoke(new FlushClientBaseDelegate(delegate ()
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
        public Form _ParentWindow { get; set; }
        /// <summary>
        /// 根目录
        /// </summary>
        public string _RootPath = Application.StartupPath;
        #endregion
    }
}
