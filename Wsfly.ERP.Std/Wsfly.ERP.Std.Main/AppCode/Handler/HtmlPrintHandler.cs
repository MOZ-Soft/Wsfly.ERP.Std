using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// HTML模版打印处理
    /// </summary>
    public class HtmlPrintHandler
    {
        /// <summary>
        /// 递归替换值
        /// </summary>
        /// <param name="html"></param>
        /// <param name="ds"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static string RecursionProcessHtml(string html, DataSet ds, List<DataRow> rows, int xhIndex = 1)
        {
            //循环
            Regex regexFor = new Regex(@"\{MZXH\:(?<TableName>[^\}]*?)\}(((?<Nested>\{MZXH\:[^\}]*\})|\{end\}(?<-Nested>)|.*?)*)\{end\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            //正则匹配集合
            MatchCollection mcFor = regexFor.Matches(html);

            //是否有匹配
            if (mcFor != null && mcFor.Count > 0)
            {
                //遍历匹配
                foreach (Match mFor in mcFor)
                {
                    DataTable dt = null;

                    //表名
                    string tableName = mFor.Groups["TableName"].Value;

                    //数据表
                    if (string.IsNullOrWhiteSpace(tableName)) dt = ds.Tables[0];
                    else if (!string.IsNullOrWhiteSpace(tableName) && ds.Tables.Contains(tableName)) dt = ds.Tables[tableName];
                    else continue;

                    //内部HTML
                    string innerHtml = mFor.Groups[1].Value;
                    string newInnerHtml = "";
                    if (string.IsNullOrWhiteSpace(innerHtml)) continue;

                    foreach (DataRow row in dt.Rows)
                    {
                        //添加行
                        rows.Add(row);

                        //递归处理循环
                        newInnerHtml += RecursionProcessHtml(innerHtml, ds, rows, xhIndex);

                        xhIndex++;

                        //移除行
                        rows.Remove(row);
                    }

                    //替换值
                    newInnerHtml = HtmlReplaceVals(newInnerHtml, ds, rows, xhIndex);

                    //替换内容
                    html = html.Replace(mFor.Value, newInnerHtml);

                    //循环索引
                    xhIndex = 1;
                }
            }

            //替换值
            html = HtmlReplaceVals(html, ds, rows, xhIndex);

            return html;
        }
        /// <summary>
        /// 替换HTML值
        /// </summary>
        /// <param name="html"></param>
        /// <param name="ds"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        private static string HtmlReplaceVals(string html, DataSet ds, List<DataRow> rows, int xhIndex)
        {
            Regex regexVal = new Regex(@"\{(?<TableName>[^\}\:]*?)\.(?<CellName>[^\}]*?)\}");
            Regex regexDate = new Regex(@"\{Date\:(?<TableName>[^\}]*?)\.(?<CellName>[^\s]*?)\s*format=['""](?<Format>.*?)['""]\}");
            Regex regexDouble = new Regex(@"\{Double\:(?<TableName>[^\}]*?)\.(?<CellName>[^\s]*?)\s*format=['""](?<Format>.*?)['""]\}");
            Regex regexImage = new Regex(@"\{Image\:(?<TableName>[^\}\:]*?)\.(?<CellName>[^\}]*?)\}");

            //正则匹配集合
            MatchCollection mcVals = regexVal.Matches(html);
            MatchCollection mcDates = regexDate.Matches(html);
            MatchCollection mcDouble = regexDouble.Matches(html);
            MatchCollection mcImages = regexImage.Matches(html);

            if (mcDates != null && mcDates.Count > 0)
            {
                foreach (Match m in mcDates)
                {
                    try
                    {
                        string tableName = m.Groups["TableName"].Value;
                        string cellName = m.Groups["CellName"].Value;
                        string format = m.Groups["Format"].Value;

                        var objVal = new object();

                        //得到值
                        objVal = HtmlGetValue(ds, rows, tableName, cellName);
                        if (objVal == null) continue;

                        //格式化
                        DateTime dtime = DataType.DateTime(objVal, DateTime.Now);
                        string valStr = dtime.ToString(format);

                        //替换值
                        html = html.Replace(m.Value, valStr.ToString());
                    }
                    catch { }
                }
            }
            if (mcDouble != null && mcDouble.Count > 0)
            {
                foreach (Match m in mcDouble)
                {
                    try
                    {
                        string tableName = m.Groups["TableName"].Value;
                        string cellName = m.Groups["CellName"].Value;
                        string format = m.Groups["Format"].Value;

                        //得到值
                        var objVal = HtmlGetValue(ds, rows, tableName, cellName);
                        if (objVal == null) continue;

                        //格式化
                        double dVal = DataType.Double(objVal, 0);
                        string valStr = dVal.ToString(format);

                        //替换值
                        html = html.Replace(m.Value, valStr.ToString());
                    }
                    catch { }
                }
            }
            if (mcImages != null && mcImages.Count > 0)
            {
                foreach (Match m in mcImages)
                {
                    try
                    {
                        string tableName = m.Groups["TableName"].Value;
                        string cellName = m.Groups["CellName"].Value;

                        //得到值
                        var objVal = HtmlGetValue(ds, rows, tableName, cellName);
                        if (objVal == null) continue;

                        //路径
                        string valStr = objVal.ToString();

                        //替换值
                        html = html.Replace(m.Value, @"file:\\\" + valStr.ToString());
                    }
                    catch { }
                }
            }
            if (mcVals != null && mcVals.Count > 0)
            {
                foreach (Match m in mcVals)
                {
                    try
                    {
                        string tableName = m.Groups["TableName"].Value;
                        string cellName = m.Groups["CellName"].Value;

                        var objVal = new object();

                        if (cellName.Equals("XHIndex"))
                        {
                            //循环索引
                            objVal = xhIndex;
                        }
                        else
                        {
                            //得到值
                            objVal = HtmlGetValue(ds, rows, tableName, cellName);
                            if (objVal == null) continue;

                            objVal = objVal.ToString().Replace("\r\n", "<br />").Replace("\r", "<br />").Replace("\n", "<br />");
                        }

                        //替换值
                        html = html.Replace(m.Value, objVal.ToString());
                    }
                    catch { }
                }
            }

            return html;
        }
        /// <summary>
        /// 得到值
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="rows"></param>
        /// <param name="tableName"></param>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private static object HtmlGetValue(DataSet ds, List<DataRow> rows, string tableName, string cellName)
        {
            //没有列名
            if (string.IsNullOrWhiteSpace(cellName)) return string.Empty;

            //是否有行循环
            if (rows != null && rows.Count > 0)
            {
                //所有行遍历
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    //是否有表名
                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        //表名符合 且 有此列
                        if (rows[i].Table.TableName.Equals(tableName) && rows[i].Table.Columns.Contains(cellName))
                        {
                            //返回值
                            return rows[i][cellName];
                        }
                    }
                    else
                    {
                        //无表名 且 行有此列
                        if (rows[i].Table.Columns.Contains(cellName))
                        {
                            //返回值
                            return rows[i][cellName];
                        }
                    }
                }
            }

            //没有行 或 未能获取值
            if (!string.IsNullOrWhiteSpace(tableName) && ds.Tables.Contains(tableName))
            {
                //得到数据表
                DataTable dt = ds.Tables[tableName];

                //是否包含列
                if (!dt.Columns.Contains(cellName)) return string.Empty;

                //返回第一行的值
                return dt.Rows[0][cellName];
            }
            else
            {
                //遍历所有表
                foreach (DataTable dt in ds.Tables)
                {
                    //是否包含列
                    if (!dt.Columns.Contains(cellName)) continue;

                    //返回第一行的值
                    return dt.Rows[0][cellName];
                }
            }

            return string.Empty;
        }
    }
}
