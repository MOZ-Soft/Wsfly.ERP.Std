
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Wsfly.ERP.Std.Core.Models.Sys;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    public class NPOIHandler
    {
        /// <summary>
        /// 英文字符
        /// </summary>
        private static string[] ENCHAR = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ" ,
            "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ",
            "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ",
            "DA", "DB", "DC", "DD", "DE", "DF", "DG", "DH", "DI", "DJ", "DK", "DL", "DM", "DN", "DO", "DP", "DQ", "DR", "DS", "DT", "DU", "DV", "DW", "DX", "DY", "DZ",
            "EA", "EB", "EC", "ED", "EE", "EF", "EG", "EH", "EI", "EJ", "EK", "EL", "EM", "EN", "EO", "EP", "EQ", "ER", "ES", "ET", "EU", "EV", "EW", "EX", "EY", "EZ",
            "FA", "FB", "FC", "FD", "FE", "FF", "FG", "FH", "FI", "FJ", "FK", "FL", "FM", "FN", "FO", "FP", "FQ", "FR", "FS", "FT", "FU", "FV", "FW", "FX", "FY", "FZ",
            "GA", "GB", "GC", "GD", "GE", "GF", "GG", "GH", "GI", "GJ", "GK", "GL", "GM", "GN", "GO", "GP", "GQ", "GR", "GS", "GT", "GU", "GV", "GW", "GX", "GY", "GZ",
            "HA", "HB", "HC", "HD", "HE", "HF", "HG", "HH", "HI", "HJ", "HK", "HL", "HM", "HN", "HO", "HP", "HQ", "HR", "HS", "HT", "HU", "HV", "HW", "HX", "HY", "HZ"
            };

        /// <summary>
        /// 将DataTable导出到Excel
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="cells">列配置信息</param>
        public static void ExportExcel(DataTable dt, string fileName, List<CellInfo> cells)
        {
            //创建工作薄
            IWorkbook wb = new HSSFWorkbook();

            try
            {
                //边框样式
                IFont font = wb.CreateFont();
                font.IsBold = true;

                //标题栏格式
                ICellStyle haderStyle = wb.CreateCellStyle();
                haderStyle.BorderBottom = BorderStyle.Thin;
                haderStyle.BorderLeft = BorderStyle.Thin;
                haderStyle.BorderRight = BorderStyle.Thin;
                haderStyle.BorderTop = BorderStyle.Thin;
                haderStyle.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                haderStyle.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                haderStyle.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                haderStyle.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                //haderStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                haderStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                haderStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
                haderStyle.SetFont(font);
                haderStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                haderStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

                //单元格样式
                ICellStyle cellStyle = wb.CreateCellStyle();
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BottomBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                cellStyle.LeftBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                cellStyle.RightBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                cellStyle.TopBorderColor = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
                cellStyle.WrapText = true;

                //第一个表
                ISheet tempSheet = wb.CreateSheet();

                //标题栏
                tempSheet.CreateRow(0);
                tempSheet.CreateRow(1);

                int colIndex = 0;

                //所有列
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //创建列
                    string colName = dt.Columns[j].ColumnName;
                    if (colName.Equals("Id") || colName.Equals("MZ_IsEdit") || colName.Equals("MZ_IsNew")) continue;

                    //第一行英文列名
                    tempSheet.GetRow(0).CreateCell(colIndex).SetCellValue(colName);
                    tempSheet.GetRow(0).Height = 0;
                    tempSheet.GetRow(0).ZeroHeight = true;

                    //第二行中文列名
                    CellInfo cellInfo = cells.Find(p => p.CellName.Equals(colName));
                    if (cellInfo != null)
                    {
                        tempSheet.GetRow(1).HeightInPoints = 2 * tempSheet.DefaultRowHeight / 20;
                        NPOI.SS.UserModel.ICell excelCell = tempSheet.GetRow(1).CreateCell(colIndex);
                        excelCell.SetCellValue(cellInfo.CnName);
                        excelCell.CellStyle = haderStyle;
                    }

                    colIndex++;
                }

                //设置Excel的自动筛选
                NPOI.SS.Util.CellRangeAddress filterCell = NPOI.SS.Util.CellRangeAddress.ValueOf("A2:" + ENCHAR[dt.Columns.Count - 1] + "2");
                tempSheet.SetAutoFilter(filterCell);

                try
                {
                    //列自适应宽度 只对英文和数字有效
                    for (int cIndex = 0; cIndex < dt.Columns.Count; cIndex++)
                    {
                        tempSheet.AutoSizeColumn(cIndex);
                    }
                }
                catch (Exception ex) { }

                //从第3行开始写数据
                int startRow = 2;

                //循环写入Excel
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //创建一行
                    tempSheet.CreateRow(startRow + i);

                    //重置列索引
                    colIndex = 0;

                    //所有列
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        //得到列
                        NPOI.SS.UserModel.ICell excelCell = tempSheet.GetRow(startRow + i).CreateCell(colIndex);
                        excelCell.CellStyle = cellStyle;

                        //空值忽略
                        if (dt.Rows[i][j] == null || dt.Rows[i][j] == DBNull.Value)
                        {
                            colIndex++;
                            continue;
                        }

                        //列名
                        string colName = dt.Columns[j].ColumnName;
                        //是否需要过滤
                        if (colName.Equals("Id") || colName.Equals("MZ_IsEdit") || colName.Equals("MZ_IsNew")) continue;

                        //列配置
                        CellInfo cellInfo = cells.Find(p => p.CellName.Equals(colName));
                        if (cellInfo != null)
                        {
                            string valType = cellInfo.ValType.ToLower();

                            if (valType.Equals("date"))
                            {
                                excelCell.SetCellValue(DataType.DateTime(dt.Rows[i][j], DateTime.Now).ToString("yyyy-MM-dd"));
                            }
                            else if (valType.Equals("datetime"))
                            {
                                excelCell.SetCellValue(DataType.DateTime(dt.Rows[i][j], DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else if (valType.Equals("int") || valType.Equals("long") || valType.Equals("float") || valType.Equals("double") || valType.Equals("decimal"))
                            {
                                double value = DataType.Double(dt.Rows[i][j], 0);

                                if (valType.Equals("float") || valType.Equals("double") || valType.Equals("decimal"))
                                {
                                    if (cellInfo.DecimalDigits > 0) value = Math.Round(value, cellInfo.DecimalDigits);
                                }

                                excelCell.SetCellValue(value);
                            }
                            else if (valType.Equals("bool"))
                            {
                                excelCell.SetCellValue(DataType.Bool(dt.Rows[i][j], false) ? "√":"");
                            }
                            else
                            {
                                excelCell.SetCellValue(Convert.ToString(dt.Rows[i][j]));
                            }
                            colIndex++;
                            continue;
                        }

                        if (colName.Equals("DataIndex"))
                        {
                            excelCell.SetCellValue(DataType.Double(dt.Rows[i][j], 0));
                            colIndex++;
                            continue;
                        }

                        //创建列
                        excelCell.SetCellValue(Convert.ToString(dt.Rows[i][j]));

                        colIndex++;
                    }
                }

                try
                {
                    //获取当前列的宽度，然后对比本列的长度，取最大值
                    for (int columnNum = 0; columnNum <= dt.Columns.Count; columnNum++)
                    {
                        int columnWidth = tempSheet.GetColumnWidth(columnNum) / 256;
                        for (int rowNum = 1; rowNum <= tempSheet.LastRowNum; rowNum++)
                        {
                            IRow currentRow;
                            //当前行未被使用过
                            if (tempSheet.GetRow(rowNum) == null)
                            {
                                currentRow = tempSheet.CreateRow(rowNum);
                            }
                            else
                            {
                                currentRow = tempSheet.GetRow(rowNum);
                            }

                            if (currentRow.GetCell(columnNum) != null)
                            {
                                NPOI.SS.UserModel.ICell currentCell = currentRow.GetCell(columnNum);
                                int length = Encoding.UTF8.GetBytes(currentCell.ToString()).Length;
                                if (columnWidth < length)
                                {
                                    columnWidth = length;
                                }
                            }
                        }
                        tempSheet.SetColumnWidth(columnNum, columnWidth * 256);
                    }
                }
                catch { }

                //冻结
                tempSheet.CreateFreezePane(0, 2, 0, 2);

                //将文档写到指定位置
                using (FileStream file = new FileStream(fileName, FileMode.Create))
                {
                    wb.Write(file);
                    file.Close();
                    file.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 将Excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="fileName">Excel文件地址</param>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable ExcelImport(string fileName, string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;

            IWorkbook workbook = null;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                if (fileName.IndexOf(".xlsx") > 0)
                {
                    // 2007版本
                    workbook = new XSSFWorkbook(fs);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    // 2003版本
                    workbook = new HSSFWorkbook(fs);
                }

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null)
                    {
                        //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }

                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum;

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            NPOI.SS.UserModel.ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cell.CellType == NPOI.SS.UserModel.CellType.Formula)
                                {
                                    //如果是公式Cell 
                                    //则仅读取其Cell单元格的显示值 而不是读取公式
                                    cell.SetCellType(NPOI.SS.UserModel.CellType.String);
                                    cellValue = cell.StringCellValue;
                                }
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;

                        if (firstRow.ZeroHeight || firstRow.Height == 0)
                        {
                            startRow++;
                        }
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        bool hasValue = false;
                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null)
                            {
                                //同理，没有数据的单元格都默认是null
                                string cellVal = row.GetCell(j).ToString();
                                dataRow[j] = cellVal;
                                if (!string.IsNullOrWhiteSpace(cellVal) && !hasValue)
                                {
                                    hasValue = true;
                                }
                            }
                        }
                        if (!hasValue) continue;

                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// 读取EXCEL
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<List<List<string>>> ReadExcel(string fileName)
        {
            //打开Excel工作簿
            XSSFWorkbook hssfworkbook = null;
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    hssfworkbook = new XSSFWorkbook(file);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddException(e, string.Format("文件{0}打开失败，错误：{1}", new string[] { fileName, e.ToString() }));
                return null;
            }

            //循环Sheet页
            int sheetsCount = hssfworkbook.NumberOfSheets;
            List<List<List<string>>> workBookContent = new List<List<List<string>>>();
            for (int i = 0; i < sheetsCount; i++)
            {
                //Sheet索引从0开始
                ISheet sheet = hssfworkbook.GetSheetAt(i);
                //循环行
                List<List<string>> sheetContent = new List<List<string>>();
                int rowCount = sheet.PhysicalNumberOfRows;
                for (int j = 0; j < rowCount; j++)
                {
                    //Row（逻辑行）的索引从0开始
                    IRow row = sheet.GetRow(j);
                    //循环列（各行的列数可能不同）
                    List<string> rowContent = new List<string>();
                    int cellCount = row.PhysicalNumberOfCells;
                    for (int k = 0; k < cellCount; k++)
                    {
                        NPOI.SS.UserModel.ICell cell = row.Cells[k];
                        if (cell == null)
                        {
                            rowContent.Add("NIL");
                        }
                        else
                        {
                            string cellValue = cell.StringCellValue;
                            if (cell.CellType == NPOI.SS.UserModel.CellType.Formula)
                            {
                                //如果是公式Cell 
                                //则仅读取其Cell单元格的显示值 而不是读取公式
                                cell.SetCellType(NPOI.SS.UserModel.CellType.String);
                                cellValue = cell.StringCellValue;
                            }

                            rowContent.Add(cellValue);
                        }
                    }
                    //添加行到集合中
                    sheetContent.Add(rowContent);
                }
                //添加Sheet到集合中
                workBookContent.Add(sheetContent);
            }

            return workBookContent;
        }

        /// <summary>
        /// 读取EXCEL文本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadExcelText(string fileName)
        {
            //读取EXCEL
            List<List<List<string>>> excelContent = ReadExcel(fileName);
            if (excelContent == null) return string.Empty;

            string text = string.Empty;
            StringBuilder sbText = new StringBuilder();

            //循环处理WorkBook中的各Sheet页
            List<List<List<string>>>.Enumerator enumeratorWorkBook = excelContent.GetEnumerator();
            while (enumeratorWorkBook.MoveNext())
            {
                //循环处理当期Sheet页中的各行
                List<List<string>>.Enumerator enumeratorSheet = enumeratorWorkBook.Current.GetEnumerator();
                while (enumeratorSheet.MoveNext())
                {
                    string[] rowContent = enumeratorSheet.Current.ToArray();
                    sbText.Append(rowContent);
                }
            }

            text = sbText.ToString();
            return text;
        }

        /// <summary>
        /// 读取Word
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadWord(string fileName)
        {
            #region 打开文档
            XWPFDocument document = null;
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    document = new XWPFDocument(file);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddException(e, string.Format("文件{0}打开失败，错误：{1}", new string[] { fileName, e.ToString() }));
                return string.Empty;
            }
            #endregion

            return null;
        }
        /// <summary>
        /// 读取Word文本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadWordText(string fileName)
        {
            string text = string.Empty;
            StringBuilder sbText = new StringBuilder();

            #region 打开文档
            XWPFDocument document = null;
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    document = new XWPFDocument(file);
                }
            }
            catch (Exception e)
            {
                ErrorHandler.AddException(e, string.Format("文件{0}打开失败，错误：{1}", new string[] { fileName, e.ToString() }));
                return string.Empty;
            }
            #endregion

            #region 页眉、页脚
            //页眉
            if (document.HeaderList != null && document.HeaderList.Count > 0)
            {
                foreach (XWPFHeader xwpfHeader in document.HeaderList)
                {
                    sbText.AppendLine(string.Format("{0}", new string[] { xwpfHeader.Text }));
                }
            }

            //页脚
            if (document.FooterList != null && document.FooterList.Count > 0)
            {
                foreach (XWPFFooter xwpfFooter in document.FooterList)
                {
                    sbText.AppendLine(string.Format("{0}", new string[] { xwpfFooter.Text }));
                }
            }
            #endregion

            #region 表格
            if (document.Tables != null && document.Tables.Count > 0)
            {
                foreach (XWPFTable table in document.Tables)
                {
                    //循环表格行
                    foreach (XWPFTableRow row in table.Rows)
                    {
                        foreach (XWPFTableCell cell in row.GetTableCells())
                        {
                            sbText.Append(cell.GetText());
                        }
                    }
                }
            }
            #endregion

            #region 图片
            if (document.AllPictures != null && document.AllPictures.Count > 0)
            {
                foreach (XWPFPictureData pictureData in document.AllPictures)
                {
                    string picExtName = pictureData.SuggestFileExtension();
                    string picFileName = pictureData.FileName;
                    byte[] picFileContent = pictureData.Data;
                    string picTempName = ""; //AppGlobal.BuildNotRepeatTempFilePath(picFileName, picExtName);

                    using (FileStream fs = new FileStream(picTempName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(picFileContent, 0, picFileContent.Length);
                        fs.Close();
                    }
                    sbText.AppendLine(picTempName);
                }
            }
            #endregion

            //正文段落
            foreach (XWPFParagraph paragraph in document.Paragraphs)
            {
                sbText.AppendLine(paragraph.ParagraphText);
            }

            text = sbText.ToString();

            return text;
        }


        #region 辅助
        /// <summary>
        /// 获取字体样式
        /// </summary>
        /// <param name="workbook">Excel操作类</param>
        /// <param name="fontname">字体名</param>
        /// <param name="fontColor">字体颜色</param>
        /// <param name="fontsize">字体大小</param>
        /// <returns></returns>
        private static IFont GetFontStyle(HSSFWorkbook workbook, string fontFamily, NPOI.HSSF.Util.HSSFColor fontColor = null, int fontsize = 10)
        {
            IFont font = workbook.CreateFont();
            if (!string.IsNullOrEmpty(fontFamily))
            {
                font.FontName = fontFamily;
            }
            if (fontColor != null)
            {
                font.Color = fontColor.Indexed;
            }
            font.IsItalic = true;
            font.FontHeightInPoints = (short)fontsize;
            return font;
        }
        /// <summary>
        /// 合并单元格
        /// </summary>
        /// <param name="sheet">要合并单元格所在的sheet</param>
        /// <param name="rowstart">开始行的索引</param>
        /// <param name="rowend">结束行的索引</param>
        /// <param name="colstart">开始列的索引</param>
        /// <param name="colend">结束列的索引</param>
        private static void SetCellRangeAddress(ISheet sheet, int rowstart, int rowend, int colstart, int colend)
        {
            NPOI.SS.Util.CellRangeAddress cellRangeAddress = new NPOI.SS.Util.CellRangeAddress(rowstart, rowend, colstart, colend);
            sheet.AddMergedRegion(cellRangeAddress);
        }
        /// <summary>
        /// 得到颜色
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="SystemColour"></param>
        /// <returns></returns>
        private static short GetXLColour(HSSFWorkbook workbook, System.Drawing.Color SystemColour)
        {
            short s = 0;
            HSSFPalette XlPalette = workbook.GetCustomPalette();
            NPOI.HSSF.Util.HSSFColor XlColour = XlPalette.FindColor(SystemColour.R, SystemColour.G, SystemColour.B);
            if (XlColour == null)
            {
                if (NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE < 255)
                {
                    if (NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE < 64)
                    {
                        //NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE = 64;
                        //NPOI.HSSF.Record.PaletteRecord.STANDARD_PALETTE_SIZE += 1;
                        XlColour = XlPalette.AddColor(SystemColour.R, SystemColour.G, SystemColour.B);
                    }
                    else
                    {
                        XlColour = XlPalette.FindSimilarColor(SystemColour.R, SystemColour.G, SystemColour.B);
                    }

                    s = XlColour.Indexed;
                }

            }
            else
            {
                s = XlColour.Indexed;
            }

            return s;
        }
        #endregion
    }
}
