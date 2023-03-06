using System;
using System.Security.Permissions;
using System.Windows.Threading;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class DispatcherHelper
    {
        /// <summary>
        /// 标准WinForm处理消息队列
        /// System.Windows.Forms.Application.DoEvents()
        /// </summary>
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);

            try
            {
                Dispatcher.PushFrame(frame);
            }
            catch (InvalidOperationException)
            {
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private static object ExitFrames(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }
}
