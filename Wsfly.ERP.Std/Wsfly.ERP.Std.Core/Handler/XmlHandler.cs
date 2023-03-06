using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// XML文件操作
    /// </summary>
    public class XmlHandler
    {
        #region XML操作
        /// <summary>
        /// XML文件是否存在
        /// </summary>
        public bool HasFile = false;
        /// <summary>
        /// XML文件地址
        /// </summary>
        protected string strXmlFile;
        /// <summary>
        /// XML文档
        /// </summary>
        protected XmlDocument objXmlDoc = new XmlDocument();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="stream"></param>
        public XmlHandler()
        {
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="XmlFile"></param>
        public XmlHandler(string XmlFile)
        {
            try
            {
                if (!File.Exists(XmlFile))
                {
                    HasFile = false;
                    objXmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"gb2312\" ?><RootNode></RootNode>");
                }
                else
                {
                    HasFile = true;
                    objXmlDoc.Load(XmlFile);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            strXmlFile = XmlFile;
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="stream"></param>
        public XmlHandler(Stream stream)
        {
            try
            {
                objXmlDoc.Load(stream);
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// 加载XML
        /// </summary>
        /// <param name="xml"></param>
        public void Load(string xml)
        {
            try
            {
                objXmlDoc.LoadXml(xml);
            }
            catch (Exception ex) { throw ex; }
        }
        

        #region 获取节点或节点属性
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public XmlNode GetNode(string nodeName)
        {
            try
            {
                return objXmlDoc.DocumentElement.SelectSingleNode(@"descendant::" + nodeName);
            }
            catch { return null; }
        }
        /// <summary>
        /// 获取多个节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public XmlNodeList GetNodes(string nodeName)
        {
            try
            {
                return objXmlDoc.DocumentElement.SelectNodes(@"descendant::" + nodeName);
            }
            catch { return null; }
        }
        /// <summary>
        /// 获取所有节点
        /// </summary>
        /// <returns></returns>
        public XmlNodeList GetAllNodes()
        {
            try
            {
                return objXmlDoc.DocumentElement.ChildNodes;
            }
            catch { return null; }
        }

        /// <summary>
        /// 获取节点的所有属性
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public XmlAttributeCollection GetNodeAttrs(string nodeName)
        {
            try
            {
                return GetNode(nodeName).Attributes;
            }
            catch { return null; }
        }
        public XmlNode GetFirstNode() { return objXmlDoc.DocumentElement.FirstChild; }
        public XmlNode GetLastNode() { return objXmlDoc.DocumentElement.LastChild; }
        #endregion

        #region 获取节点内容或属性值
        /// <summary>
        /// 得到节点内容
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public string GetNodeText(string nodeName)
        {
            try
            {
                return GetNode(nodeName).InnerText;
            }
            catch { return null; }
        }
        /// <summary>
        /// 得到节点属性值
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="attrName"></param>
        /// <returns></returns>
        public string GetNodeAttrValue(string nodeName, string attrName)
        {
            try
            {
                return GetNode(nodeName).Attributes[attrName].Value;
            }
            catch { return null; }
        }
        #endregion

        /// <summary>
        /// 查找数据。返回一个DataView。
        /// </summary>
        /// <param name="XmlPathNode"></param>
        /// <returns></returns>
        public DataView GetData(string XmlPathNode)
        {
            try
            {
                DataSet ds = new DataSet();
                StringReader read = new StringReader(objXmlDoc.SelectSingleNode(XmlPathNode).OuterXml);
                ds.ReadXml(read);
                return ds.Tables[0].DefaultView;
            }
            catch { return null; }
        }

        #region 更新节点
        /// <summary>
        /// 更新节点内容。
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="Content">内容</param>
        public void SetNodeContent(string nodeName, string nodeContent)
        {
            if (HasFile)
            {
                if (GetNode(nodeName) != null)
                {
                    GetNode(nodeName).InnerXml = "<![CDATA[" + nodeContent + "]]>";
                    return;
                }
            }

            InsertElement(nodeName, nodeContent);
        }
        #endregion

        #region 删除节点
        /// <summary>
        /// 移除指定节点
        /// </summary>
        /// <param name="index"></param>
        public void Delete(int index)
        {
            objXmlDoc.DocumentElement.RemoveChild(objXmlDoc.DocumentElement.ChildNodes[index]);
        }
        /// <summary>
        /// 删除一个节点。 
        /// </summary>
        /// <param name="nodeName"></param>
        public void Delete(string nodeName)
        {
            objXmlDoc.DocumentElement.RemoveChild(GetNode(nodeName));
        }
        /// <summary>
        /// 删除一个节点。 
        /// </summary>
        /// <param name="node"></param>
        public void Delete(XmlNode node)
        {
            objXmlDoc.DocumentElement.RemoveChild(node);
        }
        /// <summary>
        /// 删除所有节点。 
        /// </summary>
        public void DeleteAllNodes()
        {
            objXmlDoc.DocumentElement.RemoveAll();
        }
        #endregion

        #region 插入节点
        /// <summary>
        /// 插入一节点和此节点的一子节点。 
        /// </summary>
        public void InsertNode(string nodeName, string subNodeName, string subNodeContent)
        {
            XmlNode objRootNode = objXmlDoc.DocumentElement;
            XmlElement objChildNode = objXmlDoc.CreateElement(nodeName);
            objRootNode.AppendChild(objChildNode);
            XmlElement objElement = objXmlDoc.CreateElement(subNodeName);
            objElement.InnerXml = "<![CDATA[" + subNodeContent + "]]>";
            objChildNode.AppendChild(objElement);
        }
        /// <summary>
        /// 插入一个节点，带一属性。 
        /// </summary>
        public void InsertElement(string nodeName, string nodeContent, string attrName, string attrValue)
        {
            XmlNode objNode = objXmlDoc.DocumentElement;
            XmlElement objElement = objXmlDoc.CreateElement(nodeName);
            objElement.SetAttribute(attrName, attrValue);
            objElement.InnerXml = "<![CDATA[" + nodeContent + "]]>";
            objNode.AppendChild(objElement);
        }
        /// <summary>
        /// 插入一个节点，带N属性。 
        /// </summary>
        public void InsertElement(string nodeName, string nodeContent, string[] attrName, string[] attrValue)
        {
            XmlNode objNode = objXmlDoc.DocumentElement;
            XmlElement objElement = objXmlDoc.CreateElement(nodeName);

            for (int i = 0; i < attrName.Length; i++)
            {
                objElement.SetAttribute(attrName[i], attrValue[i]);
            }

            objElement.InnerXml = "<![CDATA[" + nodeContent + "]]>";
            objNode.AppendChild(objElement);
        }
        /// <summary>
        /// 插入一个节点，不带属性。
        /// </summary>
        public void InsertElement(string nodeName, string nodeContent)
        {
            XmlNode objNode = objXmlDoc.DocumentElement;
            XmlElement objElement = objXmlDoc.CreateElement(nodeName);
            objElement.InnerXml = "<![CDATA[" + nodeContent + "]]>";
            objNode.AppendChild(objElement);
        }
        #endregion

        #region 插入或者更新节点
        /// <summary>
        /// 插入一个节点，不带属性。
        /// </summary>
        public void InsertElement(string nodeName, string nodeContent, bool append)
        {
            XmlNode objNode = objXmlDoc.DocumentElement;
            XmlElement objElement = objXmlDoc.CreateElement(nodeName);
            objElement.InnerXml = "<![CDATA[" + nodeContent + "]]>";

            if (append) objNode.AppendChild(objElement);
            else objNode.PrependChild(objElement);
        }
        /// <summary>
        /// 插入一节点，如果节点名称存在则更新节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="content"></param>
        public void AddOrUpdateNode(string nodeName, string nodeContent)
        {
            if (GetNode(nodeName) == null)
            {
                InsertElement(nodeName, nodeContent);
            }
            else
            {
                SetNodeContent(nodeName, nodeContent);
            }
        }
        /// <summary>
        /// 插入一节点，如果节点名称存在则更新节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="content"></param>
        public void AddOrUpdateNode(string nodeName, object nodeContent)
        {
            AddOrUpdateNode(nodeName, nodeContent.ToString());
        }
        /// <summary>
        /// 插入一个节点，带一属性。 
        /// </summary>
        public void AddOrUpdateNode(string nodeName, string nodeContent, string attrName, string attrValue)
        {
            if (GetNode(nodeName) == null)
            {
                InsertElement(nodeName, nodeContent, attrName, attrValue);
            }
            else
            {
                SetNodeContent(nodeName, nodeContent);
                SetAttribute(nodeName, attrName, attrValue);
            }
        }
        #endregion

        #region 节点属性操作
        /// <summary>
        /// 添加节点属性
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="attrName"></param>
        /// <param name="attrValue"></param>
        public void AddAttribute(string nodeName, string attrName, string attrValue)
        {
            XmlElement node = GetNode(nodeName) as XmlElement;

            if (node == null) return;

            node.SetAttribute(attrName, attrValue);
        }
        /// <summary>
        /// 更新节点属性
        /// 如果属性存在则 更新
        /// 否则 添加
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="attrName"></param>
        /// <param name="attrValue"></param>
        public void SetAttribute(string nodeName, string attrName, string attrValue)
        {
            bool hasAttribute = false;
            XmlNode node = GetNode(nodeName);

            if (node == null) return;

            XmlAttributeCollection collection = GetNode(nodeName).Attributes;

            if (collection != null)
            {
                foreach (XmlAttribute attr in collection)
                {
                    if (attr.Name.Equals(attrName))
                    {
                        node.Attributes[attrName].Value = attrValue;
                        hasAttribute = true;
                        break;
                    }
                }
            }

            if (!hasAttribute)
                AddAttribute(nodeName, attrName, attrValue);
        }
        /// <summary>
        /// 移除节点属性
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="attrName"></param>
        public void DeleteAttribute(string nodeName, string attrName)
        {
            XmlNode node = GetNode(nodeName);

            if (node == null) return;

            try
            {
                node.Attributes[attrName].RemoveAll();
            }
            catch { }
        }
        /// <summary>
        /// 移除节点的所有属性
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="attrName"></param>
        public void DeleteAllAttribute(string nodeName)
        {
            XmlNode node = GetNode(nodeName);

            if (node == null) return;

            node.Attributes.RemoveAll();
        }
        #endregion

        /// <summary>
        /// 保存文檔。
        /// </summary>
        public void Save()
        {
            try
            {
                objXmlDoc.Save(strXmlFile);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            objXmlDoc = null;
        }

        #endregion

        #region  XML 其它操作

        /// <summary>
        /// 判断节点是否有此属性
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool NodeHasAttribute(System.Xml.XmlNode node, string attributeName)
        {
            ///此节点没有属性
            if (node.Attributes.Count <= 0) return false;

            ///循环所有属性
            foreach (System.Xml.XmlAttribute attr in node.Attributes)
            {
                ///有此属性
                if (attr.Name.Equals(attributeName)) return true;
            }

            return false;
        }

        #endregion

        #region 将【DataSet|DataTable】转换为 【XML 字符串】

        /// <summary>
        /// 将DataTable对象转换成XML字符串
        /// </summary>
        /// <param name="dt">DataTable对象</param>
        /// <returns>XML字符串</returns>
        public static string CDataToXml(DataTable dt)
        {
            if (dt != null)
            {
                MemoryStream ms = null;
                XmlTextWriter XmlWt = null;

                try
                {
                    ms = new MemoryStream();
                    //根据ms实例化XmlWt
                    XmlWt = new XmlTextWriter(ms, Encoding.Unicode);
                    //获取ds中的数据

                    dt.WriteXml(XmlWt);
                    int count = (int)ms.Length;
                    byte[] temp = new byte[count];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(temp, 0, count);
                    //返回Unicode编码的文本

                    UnicodeEncoding ucode = new UnicodeEncoding();
                    string returnValue = ucode.GetString(temp).Trim();
                    return returnValue;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //释放资源
                    if (XmlWt != null)
                    {
                        XmlWt.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 将DataSet对象中指定的Table转换成XML字符串
        /// </summary>
        /// <param name="ds">DataSet对象</param>
        /// <param name="tableIndex">DataSet对象中的Table索引</param>
        /// <returns>XML字符串</returns>
        public static string CDataToXml(DataSet ds, int tableIndex)
        {
            if (tableIndex != -1)
            {
                return CDataToXml(ds.Tables[tableIndex]);
            }
            else
            {
                return CDataToXml(ds.Tables[0]);
            }
        }
        //// <summary>
        /// 将DataSet对象转换成XML字符串
        /// </summary>
        /// <param name="ds">DataSet对象</param>
        /// <returns>XML字符串</returns>
        public static string CDataToXml(DataSet ds)
        {
            return CDataToXml(ds, -1);
        }
        /// <summary>
        /// 将DataView对象转换成XML字符串
        /// </summary>
        /// <param name="dv">DataView对象</param>
        /// <returns>XML字符串</returns>
        public static string CDataToXml(DataView dv)
        {
            return CDataToXml(dv.Table);
        }

        #endregion

        #region 将【DataSet|DataTable】保存为 【XML 文件】

        /// <summary>
        /// 写入XML文件
        /// </summary>
        /// <param name="ds">数据源</param>
        /// <param name="path">保存文件的绝对路径</param>
        /// <returns></returns>
        public string WriteXmlToFile(DataSet ds, string path)
        {
            try
            {
                if (ds == null) { return "数据为空!"; }

                if (File.Exists(path)) { return "文件存在!"; }

                FileStream myFileStream = new FileStream(path, FileMode.Create);

                XmlTextWriter myXmlWriter = new XmlTextWriter(myFileStream, Encoding.Unicode);

                ds.WriteXml(myXmlWriter);

                myXmlWriter.Close();

                return null;
            }
            catch
            {
                return "写入文件失败";
            }
        }
        /// <summary>
        /// 将DataSet对象数据保存为XML文件
        /// </summary>
        /// <param name="dt">DataSet</param>
        /// <param name="xmlFilePath">XML文件路径</param>
        /// <returns>bool值</returns>
        public static bool CDataToXmlFile(DataTable dt, string xmlFilePath)
        {
            if ((dt != null) && (!string.IsNullOrEmpty(xmlFilePath)))
            {
                string path = xmlFilePath;
                MemoryStream ms = null;
                XmlTextWriter XmlWt = null;
                try
                {
                    ms = new MemoryStream();
                    //根据ms实例化XmlWt
                    XmlWt = new XmlTextWriter(ms, Encoding.Unicode);
                    //获取ds中的数据
                    dt.WriteXml(XmlWt);
                    int count = (int)ms.Length;
                    byte[] temp = new byte[count];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(temp, 0, count);
                    //返回Unicode编码的文本
                    UnicodeEncoding ucode = new UnicodeEncoding();
                    //写文件
                    StreamWriter sw = new StreamWriter(path);
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine(ucode.GetString(temp).Trim());
                    sw.Close();
                    return true;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //释放资源
                    if (XmlWt != null)
                    {
                        XmlWt.Close();
                        ms.Close();
                        ms.Dispose();
                    }
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 将DataSet对象中指定的Table转换成XML文件
        /// </summary>
        /// <param name="ds">DataSet对象</param>
        /// <param name="tableIndex">DataSet对象中的Table索引</param>
        /// <param name="xmlFilePath">xml文件路径</param>
        /// <returns>bool]值</returns>
        public static bool CDataToXmlFile(DataSet ds, int tableIndex, string xmlFilePath)
        {
            if (tableIndex != -1)
            {
                return CDataToXmlFile(ds.Tables[tableIndex], xmlFilePath);
            }
            else
            {
                return CDataToXmlFile(ds.Tables[0], xmlFilePath);
            }
        }
        /// <summary>
        /// 将DataSet对象转换成XML文件
        /// </summary>
        /// <param name="ds">DataSet对象</param>
        /// <param name="xmlFilePath">xml文件路径</param>
        /// <returns>bool]值</returns>
        public static bool CDataToXmlFile(DataSet ds, string xmlFilePath)
        {
            return CDataToXmlFile(ds, -1, xmlFilePath);
        }
        /// <summary>
        /// 将DataView对象转换成XML文件
        /// </summary>
        /// <param name="dv">DataView对象</param>
        /// <param name="xmlFilePath">xml文件路径</param>
        /// <returns>bool]值</returns>
        public static bool CDataToXmlFile(DataView dv, string xmlFilePath)
        {
            return CDataToXmlFile(dv.Table, xmlFilePath);
        }

        #endregion

        #region 将 【XML 字符串】 转换为 【DataSet | DataTable】
        /// <summary>
        /// 将Xml字符串转换成DataSet对象
        /// </summary>
        /// <param name="xmlStr">Xml内容字符串</param>
        /// <returns>DataSet对象</returns>
        public static DataSet CXmlToDataSet(string xmlStr)
        {
            if (!string.IsNullOrEmpty(xmlStr))
            {
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                try
                {
                    DataSet ds = new DataSet();
                    //读取字符串中的信息
                    StrStream = new StringReader(xmlStr);
                    //获取StrStream中的数据
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds获取Xmlrdr中的数据                
                    ds.ReadXml(Xmlrdr);
                    return ds;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    //释放资源
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 将Xml字符串转换成DataTable对象
        /// </summary>
        /// <param name="xmlStr">Xml字符串</param>
        /// <param name="tableIndex">Table表索引</param>
        /// <returns>DataTable对象</returns>
        public static DataTable CXmlToDatatTable(string xmlStr, int tableIndex)
        {
            return CXmlToDataSet(xmlStr).Tables[tableIndex];
        }
        /// <summary>
        /// 将Xml字符串转换成DataTable对象
        /// </summary>
        /// <param name="xmlStr">Xml字符串</param>
        /// <returns>DataTable对象</returns>
        public static DataTable CXmlToDatatTable(string xmlStr)
        {
            return CXmlToDataSet(xmlStr).Tables[0];
        }
        #endregion

        #region 将 读取【XML 文件】 转换为 【DataSet | DataTable】
        /// <summary>
        /// 读取Xml文件信息,并转换成DataSet对象
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="xmlFilePath">Xml文件地址</param>
        /// <returns>DataSet对象</returns>
        public static DataSet CXmlFileToDataSet(string xmlFilePath)
        {
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                string path = xmlFilePath;
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    //根据地址加载Xml文件
                    xmldoc.Load(path);

                    DataSet ds = new DataSet();
                    //读取文件中的字符流
                    StrStream = new StringReader(xmldoc.InnerXml);
                    //获取StrStream中的数据
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds获取Xmlrdr中的数据
                    ds.ReadXml(Xmlrdr);
                    return ds;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    //释放资源
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 读取Xml文件信息,并转换成DataTable对象
        /// </summary>
        /// <param name="xmlFilePath">xml文件路径</param>
        /// <param name="tableIndex">Table索引</param>
        /// <returns>DataTable对象</returns>
        public static DataTable CXmlToDataTable(string xmlFilePath, int tableIndex)
        {
            return CXmlFileToDataSet(xmlFilePath).Tables[tableIndex];
        }
        /// <summary>
        /// 读取Xml文件信息,并转换成DataTable对象
        /// </summary>
        /// <param name="xmlFilePath">xml文件路径</param>
        /// <returns>DataTable对象</returns>
        public static DataTable CXmlToDataTable(string xmlFilePath)
        {
            return CXmlFileToDataSet(xmlFilePath).Tables[0];
        }
        #endregion
    }
}
