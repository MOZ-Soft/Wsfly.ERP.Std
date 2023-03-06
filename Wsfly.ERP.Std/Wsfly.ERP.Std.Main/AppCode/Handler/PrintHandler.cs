
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Wsfly.ERP.Std.Core.Handler;
using FastReport.Export.Image;

namespace Wsfly.ERP.Std.AppCode.Handler
{
    /// <summary>
    /// 打印助手
    /// </summary>
    public class PrintHandler
    {
        /// <summary>
        /// 获取打印图片列表
        /// </summary>
        /// <param name="dsPrintData">打印数据集</param>
        /// <param name="templatePath">模版代码</param>
        /// <param name="isShowPreview">是否显示打印预览</param>
        public static List<BitmapImage> GetPrintImage(DataSet dsPrintData, string code)
        {
            List<BitmapImage> images = new List<BitmapImage>();

            try
            {
                //中文操作界面
                //FastReport.Utils.Res.LoadLocale(AppDomain.CurrentDomain.BaseDirectory + @"FastReport.Net\Localization\Chinese (Simplified).frl");
                //打印Report
                FastReport.Report report = new FastReport.Report();
                //加载报表模版
                report.LoadFromString(code);
                //注册数据
                report.RegisterData(dsPrintData);
                report.Prepare();

                //临时目录
                string tempDir = AppDomain.CurrentDomain.BaseDirectory + "AppFiles\\_TempPrint\\";
                if (!System.IO.Directory.Exists(tempDir))
                {
                    System.IO.Directory.CreateDirectory(tempDir);
                }

                using (ImageExport image = new ImageExport())
                {
                    image.ImageFormat = ImageExportFormat.Jpeg;
                    image.JpegQuality = 100;
                    image.Resolution = 100;
                    image.HasMultipleFiles = true;
                    image.Export(report, tempDir + DateTime.Now.ToFileTime() + ".jpg");

                    foreach (string file in image.GeneratedFiles)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.UriSource = new Uri(file);
                        bitmapImage.EndInit();

                        images.Add(bitmapImage);
                    }

                    /*
                     using (MemoryStream stream = new MemoryStream())
                    {
                        image.Export(report, stream);

                        stream.Seek(0, SeekOrigin.Begin);

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();

                        images.Add(bitmapImage);
                    }
                     */

                }


                //释放资源
                report.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return images;
        }
        /// <summary>
        /// 获取打印Pdf
        /// </summary>
        /// <param name="dsPrintData">打印数据集</param>
        /// <param name="templatePath">模版代码</param>
        /// <param name="isShowPreview">是否显示打印预览</param>
        public static string GetPrintPdf(DataSet dsPrintData, string code)
        {
            List<BitmapImage> images = new List<BitmapImage>();

            try
            {
                //中文操作界面
                //FastReport.Utils.Res.LoadLocale(AppDomain.CurrentDomain.BaseDirectory + @"FastReport.Net\Localization\Chinese (Simplified).frl");
                //打印Report
                FastReport.Report report = new FastReport.Report();
                //加载报表模版
                report.LoadFromString(code);
                //注册数据
                report.RegisterData(dsPrintData);
                report.Prepare();

                //临时目录
                string tempDir = AppDomain.CurrentDomain.BaseDirectory + "AppFiles\\_TempPrint\\";
                if (!System.IO.Directory.Exists(tempDir))
                {
                    System.IO.Directory.CreateDirectory(tempDir);
                }
                string savePath = tempDir + DateTime.Now.ToFileTime() + ".pdf";

                //导出PDF
                FastReport.Export.PdfSimple.PDFSimpleExport pdf = new FastReport.Export.PdfSimple.PDFSimpleExport();
                report.Export(pdf, savePath);

                //释放资源
                report.Dispose();

                return savePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return string.Empty;
        }


        /// <summary>
        /// /删除打印临时文件
        /// </summary>
        public static void DeletePrintTempFile()
        {
            try
            {
                string tempDir = AppDomain.CurrentDomain.BaseDirectory + "AppFiles\\_TempPrint\\";
                if (!System.IO.Directory.Exists(tempDir)) return;

                FileHandler.DeleteFolder(tempDir);
            }
            catch (Exception ex) { }
        }
    }
}
