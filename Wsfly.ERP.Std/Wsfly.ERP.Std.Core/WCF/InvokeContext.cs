using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Wsfly.ERP.Std.Core.WCF
{
    public class InvokeContext
    {
        #region WCF服务工厂
        /// <summary>
        /// 根据URL创建服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static T CreateWCFServiceByURL<T>(string url)
        {
            return CreateWCFServiceByURL<T>(url, InvokeContextBinding.netTcpBinding);
        }
        /// <summary>
        /// 根据URL及绑定方式创建服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="bindingType"></param>
        /// <returns></returns>
        public static T CreateWCFServiceByURL<T>(string url, InvokeContextBinding bindingType)
        {
            if (string.IsNullOrEmpty(url)) throw new NotSupportedException("服务地址不能为空！");
            EndpointAddress address = new EndpointAddress(url);
            Binding binding = CreateBinding(bindingType);
            ChannelFactory<T> factory = new ChannelFactory<T>(binding, address);
            return factory.CreateChannel();
        }
        #endregion

        #region 创建传输协议
        /// <summary>
        /// 创建传输协议
        /// </summary>
        /// <param name="binding">传输协议名称</param>
        /// <returns></returns>
        private static Binding CreateBinding(InvokeContextBinding binding)
        {
            Binding bindinginstance = null;

            if (binding == InvokeContextBinding.basicHttpBinding)
            {
                BasicHttpBinding ws = new BasicHttpBinding();

                ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.netNamedPipeBinding)
            {
                NetNamedPipeBinding ws = new NetNamedPipeBinding();

                //ws.MaxReceivedMessageSize = 65535000;
                ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.netPeerTcpBinding)
            {
                NetPeerTcpBinding ws = new NetPeerTcpBinding();

                //ws.MaxReceivedMessageSize = 65535000;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.netTcpBinding)
            {
                NetTcpBinding ws = new NetTcpBinding();

                //ws.MaxReceivedMessageSize = 65535000;
                ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);
                ws.Security.Mode = SecurityMode.None;

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.wsDualHttpBinding)
            {
                WSDualHttpBinding ws = new WSDualHttpBinding();

                //ws.MaxReceivedMessageSize = 65535000;
                //ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.webHttpBinding)
            {
                //WebHttpBinding ws = new WebHttpBinding();
                //ws.MaxReceivedMessageSize = 65535000;
                //bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.wsFederationHttpBinding)
            {
                WSFederationHttpBinding ws = new WSFederationHttpBinding();

                //ws.MaxReceivedMessageSize = 65535000;
                //ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);

                bindinginstance = ws;
            }
            else if (binding == InvokeContextBinding.wsHttpBinding)
            {
                WSHttpBinding ws = new WSHttpBinding(SecurityMode.None);

                //ws.MaxReceivedMessageSize = 65535000;
                //ws.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.Windows;
                //ws.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Windows;
                //ws.MaxBufferSize = 2147483647;
                ws.MaxBufferPoolSize = 2147483647;
                ws.MaxReceivedMessageSize = 2147483647;
                ws.ReaderQuotas.MaxStringContentLength = 2147483647;
                ws.CloseTimeout = new TimeSpan(0, 10, 0);
                ws.OpenTimeout = new TimeSpan(0, 10, 0);
                ws.ReceiveTimeout = new TimeSpan(0, 10, 0);
                ws.SendTimeout = new TimeSpan(0, 10, 0);
                ws.Security.Mode = SecurityMode.None;

                bindinginstance = ws;
            }
            return bindinginstance;

        }
        #endregion
    }

    public enum InvokeContextBinding
    {
        basicHttpBinding,
        netNamedPipeBinding,
        netPeerTcpBinding,
        netTcpBinding,
        wsDualHttpBinding,
        webHttpBinding,
        wsFederationHttpBinding,
        wsHttpBinding
    }
}
