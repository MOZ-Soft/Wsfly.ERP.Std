using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 图片操作
    /// </summary>
    public class ImageHandler
    {
        #region 裁剪图片
        /*
        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Image CutImage(System.Web.HttpContextBase context)
        {
            int x1 = DataType.Int(context.Request["x1"], 0);
            int y1 = DataType.Int(context.Request["y1"], 0);
            int x2 = DataType.Int(context.Request["x2"], 0);
            int y2 = DataType.Int(context.Request["y2"], 0);

            int selectWidth = DataType.Int(context.Request["w"], 0);
            int selectHeight = DataType.Int(context.Request["h"], 0);

            int imgZoomWidth = DataType.Int(context.Request["imgW"], 0);
            int imgZoomHeight = DataType.Int(context.Request["imgH"], 0);

            string imgUrl = context.Request["url"];

            if (x1 <= 0 && y1 <= 0 && x2 <= 0 && y2 <= 0)
            {
                x1 = 0;
                y1 = 0;
                x2 = 300;
                y2 = 300;
            }

            if (selectWidth <= 0 || selectHeight <= 0)
            {
                selectWidth = 300;
                selectHeight = 300;
            }

            imgUrl = context.Server.MapPath(imgUrl);

            if (!File.Exists(imgUrl))
            {
                throw new Exception("!找不到图片文件！");
            }

            Image imgHead = Image.FromFile(imgUrl);
            var imgWidth = imgHead.Width;
            var imgHeight = imgHead.Height;

            float realScale = 1;

            if (imgZoomWidth != imgWidth) realScale = imgWidth / (float)imgZoomWidth;

            x1 = DataType.Int(x1 * realScale, 0);
            y1 = DataType.Int(y1 * realScale, 0);
            x2 = DataType.Int(x2 * realScale, 300);
            y2 = DataType.Int(y2 * realScale, 300);

            selectWidth = DataType.Int(selectWidth * realScale, 300);
            selectHeight = DataType.Int(selectHeight * realScale, 300);

            Image imgThumb = CutImage(imgHead, x1, y1, selectWidth, selectHeight);

            return imgThumb;
        }
        */
        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Image CutImage2(Image imgPic, int x1, int y1, int x2, int y2, int selectWidth, int selectHeight, int imgZoomWidth, int imgZoomHeight)
        {
            if (x1 <= 0 && y1 <= 0 && x2 <= 0 && y2 <= 0)
            {
                x1 = 0;
                y1 = 0;
                x2 = 300;
                y2 = 300;
            }

            if (selectWidth <= 0 || selectHeight <= 0)
            {
                selectWidth = 300;
                selectHeight = 300;
            }

            var imgWidth = imgPic.Width;
            var imgHeight = imgPic.Height;

            float realScale = 1;

            if (imgZoomWidth != imgWidth) realScale = imgWidth / (float)imgZoomWidth;

            x1 = DataType.Int(x1 * realScale, 0);
            y1 = DataType.Int(y1 * realScale, 0);
            x2 = DataType.Int(x2 * realScale, 300);
            y2 = DataType.Int(y2 * realScale, 300);

            selectWidth = DataType.Int(selectWidth * realScale, 300);
            selectHeight = DataType.Int(selectHeight * realScale, 300);

            Image imgThumb = CutImage(imgPic, x1, y1, selectWidth, selectHeight);

            return imgThumb;
        }
        /// <summary>
        /// 截取图像中的一部份
        /// </summary>
        /// <param name="sourceImage">原图像</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        private static Image CutImage(System.Drawing.Image sourceImage, int x, int y, int width, int height)
        {
            using (sourceImage)
            {
                System.Drawing.Image imgThumb;
                System.Drawing.Graphics g;

                imgThumb = new System.Drawing.Bitmap(width, height);

                g = System.Drawing.Graphics.FromImage(imgThumb);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceImage, new Rectangle(0, 0, x + width, y + height), x, y, x + width, y + height, GraphicsUnit.Pixel);
                g.Dispose();

                return imgThumb;
            }
        }
        #endregion

        #region 缩略图

        #region 以宽度或者、高度生成缩略图
        /// <summary>
        /// 为原始图像生成缩略图
        /// [以宽度判断]
        /// </summary>
        /// <param name="sourceImage">原始图像</param>
        /// <param name="widthOfThumbnailImage">缩略图的宽度(像素)</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        public static System.Drawing.Image GetThumbnailImageByWidth(System.Drawing.Image sourceImage, int widthOfThumbnailImage)
        {

            using (sourceImage)
            {
                try
                {
                    System.Drawing.Image imgThumb;
                    System.Drawing.Graphics g;

                    int width = sourceImage.Width;
                    int height = sourceImage.Height;

                    int heightOfThumbnailImage;

                    float rate = (float)widthOfThumbnailImage / width;

                    if (rate >= 1)
                    {
                        widthOfThumbnailImage = width;
                        heightOfThumbnailImage = height;
                    }
                    else
                    {
                        heightOfThumbnailImage = (int)(rate * height);
                    }


                    imgThumb = new System.Drawing.Bitmap(widthOfThumbnailImage, heightOfThumbnailImage);

                    g = System.Drawing.Graphics.FromImage(imgThumb);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(sourceImage, new Rectangle(0, 0, widthOfThumbnailImage, heightOfThumbnailImage), 0, 0, width, height, GraphicsUnit.Pixel);
                    g.Dispose();

                    return imgThumb;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }
        /// <summary>
        /// 为原始图像生成缩略图
        /// [以高度判断]
        /// </summary>
        /// <param name="sourceImage">原始图像</param>
        /// <param name="heightOfThumbnailImage">缩略图的高度(像素)</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        public static System.Drawing.Image GetThumbnailImageByHeight(System.Drawing.Image sourceImage, int heightOfThumbnailImage)
        {

            using (sourceImage)
            {
                try
                {
                    System.Drawing.Image imgThumb;
                    System.Drawing.Graphics g;

                    int width = sourceImage.Width;
                    int height = sourceImage.Height;

                    int widthOfThumbnailImage;

                    float rate = (float)heightOfThumbnailImage / height;

                    if (rate >= 1)
                    {
                        widthOfThumbnailImage = width;
                        heightOfThumbnailImage = height;
                    }
                    else
                    {
                        widthOfThumbnailImage = (int)(rate * width);
                    }


                    imgThumb = new System.Drawing.Bitmap(widthOfThumbnailImage, heightOfThumbnailImage);

                    g = System.Drawing.Graphics.FromImage(imgThumb);

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(sourceImage, new Rectangle(0, 0, widthOfThumbnailImage, heightOfThumbnailImage), 0, 0, width, height, GraphicsUnit.Pixel);
                    g.Dispose();

                    return imgThumb;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }
        #endregion

        #region 生成固定大小的缩略图，背景色填充
        /// <summary>
        /// 图像生成缩略图[WsFly.com]
        /// </summary>
        /// <param name="sourceImage">原始图像</param>
        /// <param name="width">缩略图的宽度(像素)</param>
        /// <param name="height">缩略图的高度(像素)</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        public static System.Drawing.Image GetThumbnailImage(System.Drawing.Image sourceImage, int width, int height)
        {
            return GetThumbnailImage(sourceImage, width, height, Color.White);
        }
        /// <summary>
        /// 图像生成缩略图[WsFly.com]
        /// </summary>
        /// <param name="path">原始图像地址</param>
        /// <param name="width">缩略图的宽度(像素)</param>
        /// <param name="height">缩略图的高度(像素)</param>
        /// <returns>缩略图地址</returns>
        public static string CreateThumbnailImage(string path, int width, int height)
        {
            ///保存文件路径 
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + "." + DateTime.Now.ToString("ssffff") + ".thumb" + ext;

            ///得到对象
            System.Drawing.Image imgThumb = System.Drawing.Image.FromFile(path);

            ///载入图片
            using (imgThumb)
            {
                ///得到缩略图
                System.Drawing.Image newImage = GetThumbnailImage(imgThumb, width, height, Color.White);
                ///保存图片
                SaveImage(newImage, path_save);
                ///释放图片对象
                newImage.Dispose();
                ///返回图片路径
                return path_save;
            }
        }
        /// <summary>
        /// 图像生成缩略图[WsFly.com]
        /// 如果有水印则添加水印
        /// </summary>
        /// <param name="path">原始图像地址</param>
        /// <param name="width">缩略图的宽度(像素)</param>
        /// <param name="height">缩略图的高度(像素)</param>
        /// <returns>缩略图地址</returns>
        public static string CreateThumbnailImage(string path, string watermark, int width, int height, bool relativePath = false)
        {
            return CreateThumbnailImage(path, watermark, width, height, null);
        }
        /// <summary>
        /// 图像生成缩略图[WsFly.com]
        /// 如果有水印则添加水印
        /// </summary>
        /// <param name="path">原始图像地址</param>
        /// <param name="width">缩略图的宽度(像素)</param>
        /// <param name="height">缩略图的高度(像素)</param>
        /// <returns>缩略图地址</returns>
        public static string CreateThumbnailImage(string path, string watermark, int width, int height, string endExtension)
        {

            ///文件后缀
            if (string.IsNullOrEmpty(endExtension)) endExtension = ".thumb";
            if (!endExtension.StartsWith(".")) endExtension = "." + endExtension;

            ///保存文件路径
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + "." + DateTime.Now.ToString("ssffff") + endExtension + ext;

            ///得到对象
            System.Drawing.Image imgThumb = System.Drawing.Image.FromFile(path);

            ///载入图片
            using (imgThumb)
            {
                ///得到缩略图
                System.Drawing.Image newImage = GetThumbnailImage(imgThumb, width, height, Color.White);

                if (!string.IsNullOrEmpty(watermark))
                {///增加水印

                    if (File.Exists(watermark))
                    {
                        ///增加图片水印
                        newImage = WatermarkImage(newImage, watermark, 0.3f, Position.Bottom_Right);
                    }
                    else
                    {
                        ///增加文字水印
                        newImage = WatermarkWord(newImage, watermark, Color.Red, Position.Bottom_Right);
                    }
                }

                ///保存图片
                SaveImage(newImage, path_save);
                ///释放图片对象
                newImage.Dispose();
                ///返回保存路径
                return path_save;
            }
        }


        /// <summary>
        /// 为原始图像生成缩略图
        /// [以宽度和高度判断]
        /// </summary>
        /// <param name="sourceImage">要生成缩略图Image对象</param>
        /// <param name="maxWidth">缩略图的宽度(像素)</param>
        /// <param name="maxHeight">缩略图的高度(像素)</param>
        /// <param name="backgroudColor">缩略图背景颜色</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        public static System.Drawing.Image GetThumbnailImage(System.Drawing.Image sourceImage, int maxWidth, int maxHeight, Color backgroudColor)
        {
            int imgWidth = maxWidth;
            int imgHeight = maxHeight;

            int width = sourceImage.Width;
            int height = sourceImage.Height;

            ///如果图片大小与要生成的缩略图大小相等，返回源文件
            if (imgWidth >= width && imgHeight >= height) return sourceImage;

            //using (sourceImage)
            //{
            try
            {
                System.Drawing.Image imgThumb;//BMP图片
                System.Drawing.Graphics g;//画板

                #region 以宽判断

                float rate = (float)maxWidth / width;

                if (rate >= 1)
                {
                    maxWidth = width;
                }
                else
                {
                    height = (int)(rate * height);
                }

                #endregion

                #region 以高判断

                rate = (float)maxHeight / height;

                if (rate >= 1)
                {//实际高度小于需要高度
                    maxHeight = height;
                }
                else
                {//实际高度大于需要高度
                    maxWidth = (int)(rate * maxWidth);
                    maxHeight = (int)(rate * height);
                }

                #endregion

                //绘制图片大小:缩略图不变形大小
                Size size = new Size(maxWidth, maxHeight);
                //绘图开始位置:以图片为中心
                Point pint = new Point(((imgWidth / 2) - (maxWidth / 2)), ((imgHeight / 2) - (maxHeight / 2)));

                //确定BMP图片大小:缩略图要求大小
                imgThumb = new System.Drawing.Bitmap(imgWidth, imgHeight);

                //绘制图形
                g = System.Drawing.Graphics.FromImage(imgThumb);
                //清空画布并以背景色填充
                g.Clear(backgroudColor);
                //设置高质量插值法
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //在指定位置并且按指定大小绘制原图片的指定部分
                g.DrawImage(sourceImage, new Rectangle(pint, size), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel);
                //释放资源
                g.Dispose();

                return imgThumb;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //}

        }
        #endregion

        #region 其它方式生成缩略图
        ///   <summary>   
        ///   为原始图像生成缩略图
        ///   [以宽度判断]
        ///   [图像数据流]
        ///   </summary>   
        ///   <param name="sourceImageStream">原始图像数据流</param>   
        ///   <param name="widthOfThumbnailImage">缩略图的宽度(像素)</param>   
        ///   <returns>byte[]类型的缩略图</returns>   
        public static byte[] GetThumbnailImage(Stream sourceImageStream, int widthOfThumbnailImage)
        {

            using (sourceImageStream)
            {
                try
                {
                    byte[] Thumb;

                    sourceImageStream.Seek(0, 0);

                    System.Drawing.Image imgSource = new System.Drawing.Bitmap(sourceImageStream);
                    System.Drawing.Image imgThumb = GetThumbnailImageByWidth(imgSource, widthOfThumbnailImage);

                    Stream ThumbnailImageStream = new System.IO.MemoryStream();

                    imgThumb.Save(ThumbnailImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                    long ThumbnailImageFileLength = ThumbnailImageStream.Length;

                    Thumb = new byte[ThumbnailImageFileLength];

                    ThumbnailImageStream.Seek(0, 0);
                    ThumbnailImageStream.Read(Thumb, 0, (int)ThumbnailImageFileLength);

                    ThumbnailImageStream.Close();

                    imgSource.Dispose();
                    imgThumb.Dispose();
                    ThumbnailImageStream.Dispose();

                    return Thumb;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式</param>    
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {
            using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath))
            {

                int towidth = width;
                int toheight = height;

                int x = 0;
                int y = 0;
                int ow = originalImage.Width;
                int oh = originalImage.Height;

                switch (mode)
                {
                    case "HW"://指定高宽缩放（可能变形）                
                        break;
                    case "W"://指定宽，高按比例                    
                        toheight = originalImage.Height * width / originalImage.Width;
                        break;
                    case "H"://指定高，宽按比例
                        towidth = originalImage.Width * height / originalImage.Height;
                        break;
                    case "Cut"://指定高宽裁减（不变形）                
                        if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                        {
                            oh = originalImage.Height;
                            ow = originalImage.Height * towidth / toheight;
                            y = 0;
                            x = (originalImage.Width - ow) / 2;
                        }
                        else
                        {
                            ow = originalImage.Width;
                            oh = originalImage.Width * height / towidth;
                            x = 0;
                            y = (originalImage.Height - oh) / 2;
                        }
                        break;
                    default:
                        break;
                }

                //新建一个bmp图片
                System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

                //新建一个画板
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

                //设置高质量插值法
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                //设置高质量,低速度呈现平滑程度
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //清空画布并以透明背景色填充
                g.Clear(System.Drawing.Color.Transparent);

                //在指定位置并且按指定大小绘制原图片的指定部分
                g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);

                try
                {
                    //以jpg格式保存缩略图
                    bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch (System.Exception e)
                {
                    throw e;
                }
                finally
                {
                    originalImage.Dispose();
                    bitmap.Dispose();
                    g.Dispose();
                }
            }
        }
        #endregion

        #region 图片剪切
        /// <summary>
        /// 截取图像中的一部份
        /// </summary>
        /// <param name="sourceImage">原图像</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>System.Drawing.Image类型的缩略图</returns>
        public static Image InterceptionImgage(System.Drawing.Image sourceImage, int x, int y)
        {
            using (sourceImage)
            {
                System.Drawing.Image imgThumb;
                System.Drawing.Graphics g;

                imgThumb = new System.Drawing.Bitmap(x, y);

                g = System.Drawing.Graphics.FromImage(imgThumb);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceImage, new Rectangle(0, 0, x, y), 0, 0, x, y, GraphicsUnit.Pixel);
                g.Dispose();

                return imgThumb;
            }
        }
        #endregion

        #endregion

        #region 操作图片[水印]
        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="path">要添加水印的图片地址</param>
        /// <param name="watermark">水印内容（图片地址/水印文字）</param>
        /// <param name="position">水印位置</param>
        /// <param name="isImage">是否水印图片</param>
        public static string Watermark(string path, string watermark, Position position, bool isImage)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (string.IsNullOrEmpty(watermark)) return null;

            try
            {
                if (isImage)
                {//图片水印
                    path = WatermarkImage(path, watermark, 0.3f, position);
                }
                else
                {//文字水印
                    path = WatermarkWord(path, watermark, Color.Red, position);
                }

                return path;
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// 加文字水印
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="word">水印文字</param>
        /// <param name="fontColor">文字颜色</param>
        /// <param name="p">文字位置</param>
        /// <returns>图像路径</returns>
        public static string WatermarkWord(string path, string word, Color fontColor, Position p)
        {
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + ".markText" + ext;

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(word) || !File.Exists(path)) return null;

            #region 文字属性

            SolidBrush brush;
            Font font;
            SizeF fontSize;

            string fFamily = "Arial";
            int fSize = 16;
            FontStyle fStyle = FontStyle.Regular;

            #endregion

            using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
            {

                Graphics graphics = Graphics.FromImage(image);

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                brush = new SolidBrush(fontColor);
                font = new Font(fFamily, fSize, fStyle);
                fontSize = graphics.MeasureString(word, font);

                #region 水印位置

                float xpos = 10;
                float ypos = 10;

                switch (p)
                {
                    case Position.Top_Left:
                        break;
                    case Position.Top_Center:
                        xpos = ((image.Width / 2) - (fontSize.Width / 2));
                        break;
                    case Position.Top_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        break;


                    case Position.Middle_Left:
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;
                    case Position.Middle_Center:
                        xpos = ((image.Width / 2) - (fontSize.Width / 2));
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;
                    case Position.Middle_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;


                    case Position.Bottom_Left:
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;
                    case Position.Bottom_Center:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;
                    case Position.Bottom_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;

                    default: break;
                }

                #endregion

                graphics.DrawString(word, font, brush, xpos, ypos);

                graphics.Dispose();
                brush.Dispose();

                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                image.Dispose();

                System.Drawing.Image imageWithWater = System.Drawing.Image.FromStream(stream);

                imageWithWater.Save(path_save);
                imageWithWater.Dispose();

                stream.Dispose();

                return path_save;
            }
        }
        /// <summary>
        /// 加文字水印
        /// </summary>
        /// <param name="image">图像对象</param>
        /// <param name="word">水印文字</param>
        /// <param name="fontColor">文字颜色</param>
        /// <param name="p">文字位置</param>
        /// <returns>图像对象</returns>
        public static System.Drawing.Image WatermarkWord(System.Drawing.Image image, string word, Color fontColor, Position p)
        {
            if (string.IsNullOrEmpty(word)) return image;

            #region 文字属性

            SolidBrush brush;
            Font font;
            SizeF fontSize;

            string fFamily = "Arial";
            int fSize = 16;
            FontStyle fStyle = FontStyle.Regular;

            #endregion

            using (Graphics graphics = Graphics.FromImage(image))
            {

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                brush = new SolidBrush(fontColor);
                font = new Font(fFamily, fSize, fStyle);
                fontSize = graphics.MeasureString(word, font);

                #region 水印位置

                float xpos = 10;
                float ypos = 10;

                switch (p)
                {
                    case Position.Top_Left:
                        break;
                    case Position.Top_Center:
                        xpos = ((image.Width / 2) - (fontSize.Width / 2));
                        break;
                    case Position.Top_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        break;


                    case Position.Middle_Left:
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;
                    case Position.Middle_Center:
                        xpos = ((image.Width / 2) - (fontSize.Width / 2));
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;
                    case Position.Middle_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height / 2) - (fontSize.Height / 2));
                        break;


                    case Position.Bottom_Left:
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;
                    case Position.Bottom_Center:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;
                    case Position.Bottom_Right:
                        xpos = ((image.Width - fontSize.Width) - 10);
                        ypos = ((image.Height - fontSize.Height) - 10);
                        break;

                    default: break;
                }

                #endregion

                graphics.DrawString(word, font, brush, xpos, ypos);

                graphics.Dispose();
                brush.Dispose();

                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                image.Dispose();

                return System.Drawing.Image.FromStream(stream);
            }
        }

        /// <summary>
        /// 加图片水印
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="path_watermark">水印路径</param>
        /// <param name="tempAlpha">透明度</param>
        /// <param name="p">位置</param>
        /// <returns>图像路径</returns>
        public static string WatermarkImage(string path, string path_watermark, float tempAlpha, Position p)
        {
            if (string.IsNullOrEmpty(path_watermark) ||
                string.IsNullOrEmpty(path) ||
                tempAlpha <= 0) return path;

            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + ".markImage" + ext;

            using (Image image = Image.FromFile(path))
            {
                Bitmap b = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(b);

                g.Clear(Color.White);
                g.DrawImage(image, 0, 0, image.Width, image.Height);

                Image watermark = new Bitmap(path_watermark);

                System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                System.Drawing.Imaging.ColorMap colorMap = new System.Drawing.Imaging.ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                System.Drawing.Imaging.ColorMap[] remapTable = { colorMap };
                imageAttributes.SetRemapTable(remapTable, System.Drawing.Imaging.ColorAdjustType.Bitmap);


                //设置透明度，数值有误则取默认值
                if (tempAlpha < 0 || tempAlpha > 1)
                    tempAlpha = 0.3f;

                float[][] colorMatrixElements = {
                                                 new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 0.0f, 0.0f, tempAlpha, 0.0f},
                                                 new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                                            };

                System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
                imageAttributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                #region 水印位置

                int xpos = 10;
                int ypos = 10;

                switch (p)
                {
                    case Position.Top_Left:
                        break;
                    case Position.Top_Center:
                        xpos = ((image.Width / 2) - (watermark.Width / 2));
                        break;
                    case Position.Top_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        break;


                    case Position.Middle_Left:
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;
                    case Position.Middle_Center:
                        xpos = ((image.Width / 2) - (watermark.Width / 2));
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;
                    case Position.Middle_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;


                    case Position.Bottom_Left:
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;
                    case Position.Bottom_Center:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;
                    case Position.Bottom_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;

                    default: break;
                }

                #endregion

                g.DrawImage(watermark, new Rectangle(xpos, ypos, watermark.Width, watermark.Height), 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAttributes);

                watermark.Dispose();
                imageAttributes.Dispose();

                //保存加水印过后的图片
                SaveImage(b, path_save);

                b.Dispose();
                g.Dispose();

                image.Dispose();

                return path_save;
            }
        }
        /// <summary>
        /// 加图片水印
        /// </summary>
        /// <param name="image">图像对象</param>
        /// <param name="path_watermark">水印路径</param>
        /// <param name="tempAlpha">透明度</param>
        /// <param name="p">位置</param>
        /// <returns>图像对象</returns>
        public static System.Drawing.Image WatermarkImage(System.Drawing.Image image, string path_watermark, float tempAlpha, Position p)
        {
            if (string.IsNullOrEmpty(path_watermark) || tempAlpha <= 0) return image;

            using (image)
            {
                Bitmap b = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(b);

                g.Clear(Color.White);
                g.DrawImage(image, 0, 0, image.Width, image.Height);

                Image watermark = new Bitmap(path_watermark);

                System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                System.Drawing.Imaging.ColorMap colorMap = new System.Drawing.Imaging.ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                System.Drawing.Imaging.ColorMap[] remapTable = { colorMap };
                imageAttributes.SetRemapTable(remapTable, System.Drawing.Imaging.ColorAdjustType.Bitmap);


                //设置透明度，数值有误则取默认值
                if (tempAlpha < 0 || tempAlpha > 1)
                    tempAlpha = 0.3f;

                float[][] colorMatrixElements = {
                                                 new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                                 new float[] {0.0f, 0.0f, 0.0f, tempAlpha, 0.0f},
                                                 new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                                            };

                System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
                imageAttributes.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                #region 水印位置

                int xpos = 10;
                int ypos = 10;

                switch (p)
                {
                    case Position.Top_Left:
                        break;
                    case Position.Top_Center:
                        xpos = ((image.Width / 2) - (watermark.Width / 2));
                        break;
                    case Position.Top_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        break;


                    case Position.Middle_Left:
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;
                    case Position.Middle_Center:
                        xpos = ((image.Width / 2) - (watermark.Width / 2));
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;
                    case Position.Middle_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height / 2) - (watermark.Height / 2));
                        break;


                    case Position.Bottom_Left:
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;
                    case Position.Bottom_Center:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;
                    case Position.Bottom_Right:
                        xpos = ((image.Width - watermark.Width) - 10);
                        ypos = ((image.Height - watermark.Height) - 10);
                        break;

                    default: break;
                }

                #endregion

                g.DrawImage(watermark, new Rectangle(xpos, ypos, watermark.Width, watermark.Height), 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAttributes);

                watermark.Dispose();
                imageAttributes.Dispose();

                //b.Dispose();
                g.Dispose();
                image.Dispose();

                return b;
            }
        }

        #endregion

        #region 压缩图片
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="img">图像</param>
        /// <param name="quality">质量</param>
        /// <returns></returns>
        public static Image CompressionPic(System.Drawing.Image img, int quality)
        {
            ImageFormat tFormat = img.RawFormat;
            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            //设置压缩的比例1-100
            long[] qy = new long[1];
            qy[0] = quality;
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            try
            {
                ImageCodecInfo[] imgCodeInfos = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo imgCodeInfo = null;

                for (int x = 0; x < imgCodeInfos.Length; x++)
                {
                    if (imgCodeInfos[x].FormatDescription.Equals("JPEG"))
                    {
                        imgCodeInfo = imgCodeInfos[x];
                        break;
                    }
                }

                //新生成图片
                using (MemoryStream imgStream = new MemoryStream())
                {
                    if (imgCodeInfo != null)
                    {
                        //保存到内存流
                        img.Save(imgStream, imgCodeInfo, ep);
                        //返回新图片
                        return System.Drawing.Image.FromStream(imgStream);
                    }
                    else
                    {
                        //返回一个复制对象
                        return (Image)img.Clone();
                    }
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                img.Dispose();
                img.Dispose();
            }
        }
        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="savePath">保存路径</param> 
        /// <param name="quality">质量</param>
        /// <returns></returns>
        public static bool CompressionPic(System.Drawing.Image img, string savePath, int quality)
        {
            ImageFormat tFormat = img.RawFormat;
            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = quality;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            try
            {
                ImageCodecInfo[] imgCodeInfos = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo imgCodeInfo = null;
                for (int x = 0; x < imgCodeInfos.Length; x++)
                {
                    if (imgCodeInfos[x].FormatDescription.Equals("JPEG"))
                    {
                        imgCodeInfo = imgCodeInfos[x];
                        break;
                    }
                }

                if (imgCodeInfo != null)
                {
                    img.Save(savePath, imgCodeInfo, ep);
                }
                else
                {
                    img.Save(savePath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                img.Dispose();
                img.Dispose();
            }
        }
        /// <summary> 
        /// JPEG图片压缩 
        /// </summary> 
        /// <param name="originalPath">图片原路径</param> 
        /// <param name="savePath">保存路径</param> 
        /// <param name="quality">图片质量</param> 
        /// <returns></returns> 
        public static bool CompressionPic(string originalPath, string savePath, int quality)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(originalPath);
            ImageFormat tFormat = iSource.RawFormat;
            //以下代码为保存图片时，设置压缩质量 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = quality;//设置压缩的比例1-100 
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] imgCodeInfos = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo imgCodeInfo = null;
                for (int x = 0; x < imgCodeInfos.Length; x++)
                {
                    if (imgCodeInfos[x].FormatDescription.Equals("JPEG"))
                    {
                        imgCodeInfo = imgCodeInfos[x];
                        break;
                    }
                }
                if (imgCodeInfo != null)
                {
                    //保存新的图片
                    iSource.Save(savePath, imgCodeInfo, ep);
                }
                else
                {
                    //保存图片
                    iSource.Save(savePath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                iSource.Dispose();
            }
        }
        #endregion

        #region 其它
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        public static void SaveImage(System.Drawing.Image img, string path)
        {
            ///后缀名称
            string ext = System.IO.Path.GetExtension(path).ToLower();
            ///图片保存格式
            System.Drawing.Imaging.ImageFormat imgFormat = ImageFormat.Jpeg;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            #region 图片格式
            switch (ext)
            {
                case ".jpg":
                    imgFormat = ImageFormat.Jpeg;
                    break;
                case ".gif":
                    imgFormat = ImageFormat.Gif;
                    break;
                case ".png":
                    imgFormat = ImageFormat.Png;
                    break;
                case ".bmp":
                    imgFormat = ImageFormat.Bmp;
                    break;
                case ".emf":
                    imgFormat = ImageFormat.Emf;
                    break;
                case ".icon":
                    imgFormat = ImageFormat.Icon;
                    break;
                case ".tiff":
                    imgFormat = ImageFormat.Tiff;
                    break;
                case ".wmf":
                    imgFormat = ImageFormat.Wmf;
                    break;
            }
            #endregion

            ///保存图像
            img.Save(path, imgFormat);
        }
        /// <summary>
        /// 判断文件是否是图片
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <returns></returns>
        public static bool IsPicture(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);

                string fileClass;
                byte buffer;

                buffer = reader.ReadByte();
                fileClass = buffer.ToString();

                buffer = reader.ReadByte();
                fileClass += buffer.ToString();

                reader.Close();

                fs.Close();
                fs.Dispose();

                string[] strs = "".Split(',');

                //255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                if (fileClass == "255216") { return true; }//JPG
                else if (fileClass == "7173") { return true; }//GIF
                else if (fileClass == "6677") { return true; }//BMP
                else if (fileClass == "13780") { return true; }//PNG
                else { return false; }
            }
            catch
            {
                return false;
            }
        }

        public static bool ThumbnailCallback()
        {
            return false;
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// 字节流转为图片
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Image Byte2Image(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                ms.Position = 0;
                return Image.FromStream(ms);
            }
        }
        /// <summary>
        /// 流转为图片
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static Image Stream2Image(Stream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);
            return Image.FromStream(ms);
        }
        /// <summary>
        /// 图片转为流
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static MemoryStream Image2Stream(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);
            return ms;
        }
        /// <summary>
        /// 图片转为字节流
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] Image2Bytes(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Jpeg);

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                return bytes;
            }
        }
        #endregion

        #region Icon
        [DllImport("shell32.DLL", EntryPoint = "ExtractAssociatedIcon")]
        private static extern int ExtractAssociatedIconA(int hInst, string lpIconPath, ref int lpiIcon);
        /// <summary>
        /// 得到Icon图标
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Icon GetIcon(string path)
        {
            try
            {
                int refInt = 0;
                System.IntPtr iconHandle = new IntPtr(ExtractAssociatedIconA(0, path, ref refInt));
                return Icon.FromHandle(iconHandle);
            }
            catch (Exception ex) { }

            return null;
        }
        /// <summary>
        /// 转换Image为Icon
        /// </summary>
        /// <param name="image">要转换为图标的Image对象</param>
        public static Icon ConvertToIcon(Image image)
        {
            if (image == null) return null;

            try
            {
                using (MemoryStream msImg = new MemoryStream(), msIco = new MemoryStream())
                {
                    image.Save(msImg, ImageFormat.Png);

                    using (var bin = new BinaryWriter(msIco))
                    {
                        //写图标头部
                        bin.Write((short)0);           //0-1保留
                        bin.Write((short)1);           //2-3文件类型。1=图标, 2=光标
                        bin.Write((short)1);           //4-5图像数量（图标可以包含多个图像）

                        bin.Write((byte)image.Width);  //6图标宽度
                        bin.Write((byte)image.Height); //7图标高度
                        bin.Write((byte)0);            //8颜色数（若像素位深>=8，填0。这是显然的，达到8bpp的颜色数最少是256，byte不够表示）
                        bin.Write((byte)0);            //9保留。必须为0
                        bin.Write((short)0);           //10-11调色板
                        bin.Write((short)32);         //12-13位深
                        bin.Write((int)msImg.Length);  //14-17位图数据大小
                        bin.Write(22);                   //18-21位图数据起始字节

                        //写图像数据
                        bin.Write(msImg.ToArray());

                        bin.Flush();
                        bin.Seek(0, SeekOrigin.Begin);
                        return new Icon(msIco);
                    }
                }
            }
            catch (Exception ex) { }

            return null;
        }
        #endregion

        /// <summary>
        /// 将字符串转图片
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static System.Drawing.Image Base64StringToImage(string base64Str)
        {
            System.Drawing.Bitmap bitmap = null;
            System.Drawing.Image img = null;
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = Convert.FromBase64String(base64Str);
                ms.Write(buffer, 0, buffer.Length);
                try
                {
                    img = System.Drawing.Image.FromStream(ms);
                    if (img != null)
                    {
                        bitmap = new System.Drawing.Bitmap(img.Width, img.Height);
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            g.DrawImage(img, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        }
                    }
                }
                catch { }
            }
            return bitmap;
        }
    }
    /// <summary>
    /// 位置
    /// </summary>
    public enum Position
    {
        Top_Left,
        Top_Center,
        Top_Right,
        Middle_Left,
        Middle_Center,
        Middle_Right,
        Bottom_Left,
        Bottom_Center,
        Bottom_Right
    }
}
