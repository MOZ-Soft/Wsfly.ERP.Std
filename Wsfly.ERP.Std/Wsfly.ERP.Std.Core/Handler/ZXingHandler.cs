using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 一维码、二维码助手
    /// </summary>
    public class ZXingHandler
    {
        #region 一维码
        /// <summary>
        /// 生成条形码 保存图片
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="format">码制 国内主要使用的是EAN商品条形码，可分为EAN-13（标准版）和EAN-8（缩短版）两种。</param>
        /// <param name="path">路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public static void BuildBarcode(string content, BarcodeFormat format, string path, int width = 240, int height = 130)
        {
            //得到图片
            using (Bitmap img = BuildBarcode(content, format, width, height))
            {
                try
                {
                    //保存目录
                    string dirName = System.IO.Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(dirName)) System.IO.Directory.CreateDirectory(dirName);

                    //保存条码
                    img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch { }
            }
        }
        /// <summary>
        /// 生成条形码
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="format">码制</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>图片</returns>
        public static Bitmap BuildBarcode(string content, BarcodeFormat format = BarcodeFormat.EAN_13, int width = 240, int height = 130)
        {
            //设置条形码规格
            EncodingOptions encodeOption = new EncodingOptions();
            encodeOption.Width = width;
            encodeOption.Height = height;

            //生成条形码图片
            ZXing.BarcodeWriter wr = new BarcodeWriter();
            wr.Options = encodeOption;
            wr.Format = format;
            Bitmap img = wr.Write(content);

            //返回图片
            return img;
        }
        /// <summary>
        /// 读取条码内容
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="formats">码制</param>
        /// <returns>内容</returns>
        public static string ReadBarcode(Bitmap img, List<BarcodeFormat> formats = null)
        {
            //码制
            if (formats == null || formats.Count <= 0)
            {
                formats = new List<BarcodeFormat>()
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8
                };
            }

            //设置读取条形码规格
            DecodingOptions decodeOption = new DecodingOptions();
            decodeOption.PossibleFormats = formats;

            //进行读取操作
            ZXing.BarcodeReader br = new BarcodeReader();
            br.Options = decodeOption;
            ZXing.Result rs = br.Decode(img);

            //返回内容
            if (rs != null) return rs.Text;
            return string.Empty;
        }
        #endregion

        #region 二维码
        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="path">保存路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="margin">边缘距离</param>
        /// <param name="characterSet">内容编码</param>
        /// <returns></returns>
        public static void BuildQRCode(string content, string path, int width = 200, int height = 200, int margin = 5, string characterSet = "UTF-8")
        {
            //生成二维码图片
            using (Bitmap img = BuildQRCode(content, width, height, margin, characterSet))
            {
                try
                {
                    //保存目录
                    string dirName = System.IO.Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(dirName)) System.IO.Directory.CreateDirectory(dirName);

                    //保存图片
                    img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch { }
            }
        }
        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="margin">边缘距离</param>
        /// <param name="characterSet">内容编码</param>
        /// <returns>二维码图片</returns>
        public static Bitmap BuildQRCode(string content, int width = 200, int height = 200, int margin = 5, string characterSet = "UTF-8")
        {
            //设置QR二维码的规格
            ZXing.QrCode.QrCodeEncodingOptions qrEncodeOption = new ZXing.QrCode.QrCodeEncodingOptions();
            qrEncodeOption.CharacterSet = characterSet;
            qrEncodeOption.Height = width;
            qrEncodeOption.Width = height;
            qrEncodeOption.Margin = margin;

            //生成QR二维码图片
            ZXing.BarcodeWriter wr = new BarcodeWriter();
            wr.Format = BarcodeFormat.QR_CODE;
            wr.Options = qrEncodeOption;
            Bitmap img = wr.Write(content);

            //返回图片
            return img;
        }
        /// <summary>
        /// 读取二维码内容
        /// </summary>
        /// <param name="img"></param>
        /// <param name="formats"></param>
        /// <returns></returns>
        public static string ReadQRCode(Bitmap img, List<BarcodeFormat> formats = null)
        {
            //码制
            if (formats == null || formats.Count <= 0)
            {
                formats = new List<BarcodeFormat>()
                {
                    BarcodeFormat.QR_CODE
                };
            }

            //设置读取二维码规格
            DecodingOptions decodeOption = new DecodingOptions();
            decodeOption.PossibleFormats = formats;

            //进行读取操作
            ZXing.BarcodeReader br = new BarcodeReader();
            br.Options = decodeOption;
            ZXing.Result rs = br.Decode(img);

            //返回内容
            if (rs != null) return rs.Text;
            return string.Empty;
        }
        #endregion
    }
}
