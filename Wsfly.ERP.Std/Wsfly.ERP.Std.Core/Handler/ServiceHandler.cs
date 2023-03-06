using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Collections;
using System.Configuration.Install;
using System.Collections.Specialized;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// Windows服务助手
    /// </summary>
    public class ServiceHandler
    {
        /// <summary>
        /// 注册服务（注册完就启动，已经存在的服务直接启动。）
        /// </summary>
        /// <param name="strServiceName">服务名称</param>
        /// <param name="strServiceInstallPath">服务安装程序完整路径（.exe程序完整路径）</param>
        public static void Register(string strServiceName, string strServiceInstallPath)
        {
            IDictionary mySavedState = new Hashtable();

            try
            {
                ServiceController service = new ServiceController(strServiceName);

                //服务已经存在则卸载
                if (ServiceIsExisted(strServiceName))
                {
                    //StopService(strServiceName);
                    UnInstallService(strServiceName, strServiceInstallPath);
                }
                service.Refresh();
                //注册服务
                AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();

                mySavedState.Clear();
                myAssemblyInstaller.Path = strServiceInstallPath;
                myAssemblyInstaller.UseNewContext = true;
                myAssemblyInstaller.Install(mySavedState);
                myAssemblyInstaller.Commit(mySavedState);
                myAssemblyInstaller.Dispose();

                service.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("注册服务时出错：" + ex.Message);
            }
        }

        /// <summary>
        /// 卸载服务
        /// </summary>
        /// <param name="strServiceName">服务名称</param>
        /// <param name="strServiceInstallPath">服务安装程序完整路径（.exe程序完整路径）</param>
        public static void UnInstallService(string strServiceName, string strServiceInstallPath)
        {
            try
            {
                if (ServiceIsExisted(strServiceName))
                {
                    //UnInstall Service
                    AssemblyInstaller myAssemblyInstaller = new AssemblyInstaller();
                    myAssemblyInstaller.UseNewContext = true;
                    myAssemblyInstaller.Path = strServiceInstallPath;
                    myAssemblyInstaller.Uninstall(null);
                    myAssemblyInstaller.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("卸载服务时出错：" + ex.Message);
            }
        }


        /// <summary>
        /// 判断服务是否存在
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <returns></returns>
        public static bool ServiceIsExisted(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName == serviceName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 启动服务（启动存在的服务，30秒后启动失败报错）
        /// </summary>
        /// <param name="serviceName">服务名</param>
        public static void StartService(string serviceName)
        {
            if (ServiceIsExisted(serviceName))
            {
                ServiceController service = new ServiceController(serviceName);
                if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    for (int i = 0; i < 30; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            break;
                        }
                        if (i == 29)
                        {
                            throw new Exception("服务" + serviceName + "启动失败！");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 停止服务（停止存在的服务，30秒后停止失败报错）
        /// </summary>
        /// <param name="serviceName"></param>
        public static void StopService(string serviceName)
        {
            if (ServiceIsExisted(serviceName))
            {
                ServiceController service = new ServiceController(serviceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    for (int i = 0; i < 60; i++)
                    {
                        service.Refresh();
                        System.Threading.Thread.Sleep(1000);

                        if (service.Status == ServiceControllerStatus.Stopped)
                        {
                            break;
                        }

                        if (i == 29)
                        {
                            throw new Exception("服务" + serviceName + "停止失败！");
                        }
                    }
                }
            }
        }

        /// <summary>   
        /// 获取服务启动类型 
        /// 2：自动启动
        /// 3：手动启动
        /// 4：禁用
        /// </summary>   
        /// <param name="serviceName"></param>           
        /// <returns></returns>   
        public static int GetServiceStartType(string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                return Convert.ToInt32(servicesName.GetValue("Start"));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>   
        /// 修改服务的启动项 2为自动,3为手动           
        /// </summary>   
        /// <param name="startType"></param>           
        /// <param name="serviceName"></param>           
        /// <returns></returns>   
        public static bool ChangeServiceStartType(int startType, string serviceName)
        {
            try
            {
                RegistryKey regist = Registry.LocalMachine;
                RegistryKey sysReg = regist.OpenSubKey("SYSTEM");
                RegistryKey currentControlSet = sysReg.OpenSubKey("CurrentControlSet");
                RegistryKey services = currentControlSet.OpenSubKey("Services");
                RegistryKey servicesName = services.OpenSubKey(serviceName, true);
                servicesName.SetValue("Start", startType);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断某个Windows服务是否启动
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceStart(string serviceName)
        {
            ServiceController psc = new ServiceController(serviceName);
            bool bStartStatus = false;
            try
            {
                if (!psc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    bStartStatus = true;
                }
                return bStartStatus;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

