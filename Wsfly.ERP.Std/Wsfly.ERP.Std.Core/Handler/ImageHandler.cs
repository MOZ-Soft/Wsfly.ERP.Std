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
    /// ͼƬ����
    /// </summary>
    public class ImageHandler
    {
        #region �ü�ͼƬ
        /*
        /// <summary>
        /// �ü�ͼƬ
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
                throw new Exception("!�Ҳ���ͼƬ�ļ���");
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
        /// �ü�ͼƬ
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
        /// ��ȡͼ���е�һ����
        /// </summary>
        /// <param name="sourceImage">ԭͼ��</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
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

        #region ����ͼ

        #region �Կ�Ȼ��ߡ��߶���������ͼ
        /// <summary>
        /// Ϊԭʼͼ����������ͼ
        /// [�Կ���ж�]
        /// </summary>
        /// <param name="sourceImage">ԭʼͼ��</param>
        /// <param name="widthOfThumbnailImage">����ͼ�Ŀ��(����)</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
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
        /// Ϊԭʼͼ����������ͼ
        /// [�Ը߶��ж�]
        /// </summary>
        /// <param name="sourceImage">ԭʼͼ��</param>
        /// <param name="heightOfThumbnailImage">����ͼ�ĸ߶�(����)</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
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

        #region ���ɹ̶���С������ͼ������ɫ���
        /// <summary>
        /// ͼ����������ͼ[WsFly.com]
        /// </summary>
        /// <param name="sourceImage">ԭʼͼ��</param>
        /// <param name="width">����ͼ�Ŀ��(����)</param>
        /// <param name="height">����ͼ�ĸ߶�(����)</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
        public static System.Drawing.Image GetThumbnailImage(System.Drawing.Image sourceImage, int width, int height)
        {
            return GetThumbnailImage(sourceImage, width, height, Color.White);
        }
        /// <summary>
        /// ͼ����������ͼ[WsFly.com]
        /// </summary>
        /// <param name="path">ԭʼͼ���ַ</param>
        /// <param name="width">����ͼ�Ŀ��(����)</param>
        /// <param name="height">����ͼ�ĸ߶�(����)</param>
        /// <returns>����ͼ��ַ</returns>
        public static string CreateThumbnailImage(string path, int width, int height)
        {
            ///�����ļ�·�� 
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + "." + DateTime.Now.ToString("ssffff") + ".thumb" + ext;

            ///�õ�����
            System.Drawing.Image imgThumb = System.Drawing.Image.FromFile(path);

            ///����ͼƬ
            using (imgThumb)
            {
                ///�õ�����ͼ
                System.Drawing.Image newImage = GetThumbnailImage(imgThumb, width, height, Color.White);
                ///����ͼƬ
                SaveImage(newImage, path_save);
                ///�ͷ�ͼƬ����
                newImage.Dispose();
                ///����ͼƬ·��
                return path_save;
            }
        }
        /// <summary>
        /// ͼ����������ͼ[WsFly.com]
        /// �����ˮӡ�����ˮӡ
        /// </summary>
        /// <param name="path">ԭʼͼ���ַ</param>
        /// <param name="width">����ͼ�Ŀ��(����)</param>
        /// <param name="height">����ͼ�ĸ߶�(����)</param>
        /// <returns>����ͼ��ַ</returns>
        public static string CreateThumbnailImage(string path, string watermark, int width, int height, bool relativePath = false)
        {
            return CreateThumbnailImage(path, watermark, width, height, null);
        }
        /// <summary>
        /// ͼ����������ͼ[WsFly.com]
        /// �����ˮӡ�����ˮӡ
        /// </summary>
        /// <param name="path">ԭʼͼ���ַ</param>
        /// <param name="width">����ͼ�Ŀ��(����)</param>
        /// <param name="height">����ͼ�ĸ߶�(����)</param>
        /// <returns>����ͼ��ַ</returns>
        public static string CreateThumbnailImage(string path, string watermark, int width, int height, string endExtension)
        {

            ///�ļ���׺
            if (string.IsNullOrEmpty(endExtension)) endExtension = ".thumb";
            if (!endExtension.StartsWith(".")) endExtension = "." + endExtension;

            ///�����ļ�·��
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + "." + DateTime.Now.ToString("ssffff") + endExtension + ext;

            ///�õ�����
            System.Drawing.Image imgThumb = System.Drawing.Image.FromFile(path);

            ///����ͼƬ
            using (imgThumb)
            {
                ///�õ�����ͼ
                System.Drawing.Image newImage = GetThumbnailImage(imgThumb, width, height, Color.White);

                if (!string.IsNullOrEmpty(watermark))
                {///����ˮӡ

                    if (File.Exists(watermark))
                    {
                        ///����ͼƬˮӡ
                        newImage = WatermarkImage(newImage, watermark, 0.3f, Position.Bottom_Right);
                    }
                    else
                    {
                        ///��������ˮӡ
                        newImage = WatermarkWord(newImage, watermark, Color.Red, Position.Bottom_Right);
                    }
                }

                ///����ͼƬ
                SaveImage(newImage, path_save);
                ///�ͷ�ͼƬ����
                newImage.Dispose();
                ///���ر���·��
                return path_save;
            }
        }


        /// <summary>
        /// Ϊԭʼͼ����������ͼ
        /// [�Կ�Ⱥ͸߶��ж�]
        /// </summary>
        /// <param name="sourceImage">Ҫ��������ͼImage����</param>
        /// <param name="maxWidth">����ͼ�Ŀ��(����)</param>
        /// <param name="maxHeight">����ͼ�ĸ߶�(����)</param>
        /// <param name="backgroudColor">����ͼ������ɫ</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
        public static System.Drawing.Image GetThumbnailImage(System.Drawing.Image sourceImage, int maxWidth, int maxHeight, Color backgroudColor)
        {
            int imgWidth = maxWidth;
            int imgHeight = maxHeight;

            int width = sourceImage.Width;
            int height = sourceImage.Height;

            ///���ͼƬ��С��Ҫ���ɵ�����ͼ��С��ȣ�����Դ�ļ�
            if (imgWidth >= width && imgHeight >= height) return sourceImage;

            //using (sourceImage)
            //{
            try
            {
                System.Drawing.Image imgThumb;//BMPͼƬ
                System.Drawing.Graphics g;//����

                #region �Կ��ж�

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

                #region �Ը��ж�

                rate = (float)maxHeight / height;

                if (rate >= 1)
                {//ʵ�ʸ߶�С����Ҫ�߶�
                    maxHeight = height;
                }
                else
                {//ʵ�ʸ߶ȴ�����Ҫ�߶�
                    maxWidth = (int)(rate * maxWidth);
                    maxHeight = (int)(rate * height);
                }

                #endregion

                //����ͼƬ��С:����ͼ�����δ�С
                Size size = new Size(maxWidth, maxHeight);
                //��ͼ��ʼλ��:��ͼƬΪ����
                Point pint = new Point(((imgWidth / 2) - (maxWidth / 2)), ((imgHeight / 2) - (maxHeight / 2)));

                //ȷ��BMPͼƬ��С:����ͼҪ���С
                imgThumb = new System.Drawing.Bitmap(imgWidth, imgHeight);

                //����ͼ��
                g = System.Drawing.Graphics.FromImage(imgThumb);
                //��ջ������Ա���ɫ���
                g.Clear(backgroudColor);
                //���ø�������ֵ��
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //��ָ��λ�ò��Ұ�ָ����С����ԭͼƬ��ָ������
                g.DrawImage(sourceImage, new Rectangle(pint, size), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel);
                //�ͷ���Դ
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

        #region ������ʽ��������ͼ
        ///   <summary>   
        ///   Ϊԭʼͼ����������ͼ
        ///   [�Կ���ж�]
        ///   [ͼ��������]
        ///   </summary>   
        ///   <param name="sourceImageStream">ԭʼͼ��������</param>   
        ///   <param name="widthOfThumbnailImage">����ͼ�Ŀ��(����)</param>   
        ///   <returns>byte[]���͵�����ͼ</returns>   
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
        /// ��������ͼ
        /// </summary>
        /// <param name="originalImagePath">Դͼ·��������·����</param>
        /// <param name="thumbnailPath">����ͼ·��������·����</param>
        /// <param name="width">����ͼ���</param>
        /// <param name="height">����ͼ�߶�</param>
        /// <param name="mode">��������ͼ�ķ�ʽ</param>    
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
                    case "HW"://ָ���߿����ţ����ܱ��Σ�                
                        break;
                    case "W"://ָ�����߰�����                    
                        toheight = originalImage.Height * width / originalImage.Width;
                        break;
                    case "H"://ָ���ߣ�������
                        towidth = originalImage.Width * height / originalImage.Height;
                        break;
                    case "Cut"://ָ���߿�ü��������Σ�                
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

                //�½�һ��bmpͼƬ
                System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

                //�½�һ������
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

                //���ø�������ֵ��
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                //���ø�����,���ٶȳ���ƽ���̶�
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //��ջ�������͸������ɫ���
                g.Clear(System.Drawing.Color.Transparent);

                //��ָ��λ�ò��Ұ�ָ����С����ԭͼƬ��ָ������
                g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, towidth, toheight),
                    new System.Drawing.Rectangle(x, y, ow, oh),
                    System.Drawing.GraphicsUnit.Pixel);

                try
                {
                    //��jpg��ʽ��������ͼ
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

        #region ͼƬ����
        /// <summary>
        /// ��ȡͼ���е�һ����
        /// </summary>
        /// <param name="sourceImage">ԭͼ��</param>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        /// <returns>System.Drawing.Image���͵�����ͼ</returns>
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

        #region ����ͼƬ[ˮӡ]
        /// <summary>
        /// ���ˮӡ
        /// </summary>
        /// <param name="path">Ҫ���ˮӡ��ͼƬ��ַ</param>
        /// <param name="watermark">ˮӡ���ݣ�ͼƬ��ַ/ˮӡ���֣�</param>
        /// <param name="position">ˮӡλ��</param>
        /// <param name="isImage">�Ƿ�ˮӡͼƬ</param>
        public static string Watermark(string path, string watermark, Position position, bool isImage)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (string.IsNullOrEmpty(watermark)) return null;

            try
            {
                if (isImage)
                {//ͼƬˮӡ
                    path = WatermarkImage(path, watermark, 0.3f, position);
                }
                else
                {//����ˮӡ
                    path = WatermarkWord(path, watermark, Color.Red, position);
                }

                return path;
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// ������ˮӡ
        /// </summary>
        /// <param name="path">�ļ�·��</param>
        /// <param name="word">ˮӡ����</param>
        /// <param name="fontColor">������ɫ</param>
        /// <param name="p">����λ��</param>
        /// <returns>ͼ��·��</returns>
        public static string WatermarkWord(string path, string word, Color fontColor, Position p)
        {
            string ext = System.IO.Path.GetExtension(path).ToLower();
            string path_save = path.Substring(0, path.LastIndexOf(".")) + ".markText" + ext;

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(word) || !File.Exists(path)) return null;

            #region ��������

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

                #region ˮӡλ��

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
        /// ������ˮӡ
        /// </summary>
        /// <param name="image">ͼ�����</param>
        /// <param name="word">ˮӡ����</param>
        /// <param name="fontColor">������ɫ</param>
        /// <param name="p">����λ��</param>
        /// <returns>ͼ�����</returns>
        public static System.Drawing.Image WatermarkWord(System.Drawing.Image image, string word, Color fontColor, Position p)
        {
            if (string.IsNullOrEmpty(word)) return image;

            #region ��������

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

                #region ˮӡλ��

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
        /// ��ͼƬˮӡ
        /// </summary>
        /// <param name="path">�ļ�·��</param>
        /// <param name="path_watermark">ˮӡ·��</param>
        /// <param name="tempAlpha">͸����</param>
        /// <param name="p">λ��</param>
        /// <returns>ͼ��·��</returns>
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


                //����͸���ȣ���ֵ������ȡĬ��ֵ
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

                #region ˮӡλ��

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

                //�����ˮӡ�����ͼƬ
                SaveImage(b, path_save);

                b.Dispose();
                g.Dispose();

                image.Dispose();

                return path_save;
            }
        }
        /// <summary>
        /// ��ͼƬˮӡ
        /// </summary>
        /// <param name="image">ͼ�����</param>
        /// <param name="path_watermark">ˮӡ·��</param>
        /// <param name="tempAlpha">͸����</param>
        /// <param name="p">λ��</param>
        /// <returns>ͼ�����</returns>
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


                //����͸���ȣ���ֵ������ȡĬ��ֵ
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

                #region ˮӡλ��

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

        #region ѹ��ͼƬ
        /// <summary>
        /// ѹ��ͼƬ
        /// </summary>
        /// <param name="img">ͼ��</param>
        /// <param name="quality">����</param>
        /// <returns></returns>
        public static Image CompressionPic(System.Drawing.Image img, int quality)
        {
            ImageFormat tFormat = img.RawFormat;
            //���´���Ϊ����ͼƬʱ������ѹ������
            EncoderParameters ep = new EncoderParameters();
            //����ѹ���ı���1-100
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

                //������ͼƬ
                using (MemoryStream imgStream = new MemoryStream())
                {
                    if (imgCodeInfo != null)
                    {
                        //���浽�ڴ���
                        img.Save(imgStream, imgCodeInfo, ep);
                        //������ͼƬ
                        return System.Drawing.Image.FromStream(imgStream);
                    }
                    else
                    {
                        //����һ�����ƶ���
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
        /// ѹ��ͼƬ
        /// </summary>
        /// <param name="img">ͼƬ</param>
        /// <param name="savePath">����·��</param> 
        /// <param name="quality">����</param>
        /// <returns></returns>
        public static bool CompressionPic(System.Drawing.Image img, string savePath, int quality)
        {
            ImageFormat tFormat = img.RawFormat;
            //���´���Ϊ����ͼƬʱ������ѹ������
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = quality;//����ѹ���ı���1-100
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
        /// JPEGͼƬѹ�� 
        /// </summary> 
        /// <param name="originalPath">ͼƬԭ·��</param> 
        /// <param name="savePath">����·��</param> 
        /// <param name="quality">ͼƬ����</param> 
        /// <returns></returns> 
        public static bool CompressionPic(string originalPath, string savePath, int quality)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromFile(originalPath);
            ImageFormat tFormat = iSource.RawFormat;
            //���´���Ϊ����ͼƬʱ������ѹ������ 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = quality;//����ѹ���ı���1-100 
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
                    //�����µ�ͼƬ
                    iSource.Save(savePath, imgCodeInfo, ep);
                }
                else
                {
                    //����ͼƬ
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

        #region ����
        /// <summary>
        /// ����ͼƬ
        /// </summary>
        /// <param name="img"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        public static void SaveImage(System.Drawing.Image img, string path)
        {
            ///��׺����
            string ext = System.IO.Path.GetExtension(path).ToLower();
            ///ͼƬ�����ʽ
            System.Drawing.Imaging.ImageFormat imgFormat = ImageFormat.Jpeg;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            #region ͼƬ��ʽ
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

            ///����ͼ��
            img.Save(path, imgFormat);
        }
        /// <summary>
        /// �ж��ļ��Ƿ���ͼƬ
        /// </summary>
        /// <param name="filePath">�ļ��ľ���·��</param>
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

                //255216��jpg;7173��gif;6677��BMP,13780��PNG;7790��exe,8297��rar 
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

        #region ����ת��
        /// <summary>
        /// �ֽ���תΪͼƬ
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
        /// ��תΪͼƬ
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static Image Stream2Image(Stream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);
            return Image.FromStream(ms);
        }
        /// <summary>
        /// ͼƬתΪ��
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
        /// ͼƬתΪ�ֽ���
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
        /// �õ�Iconͼ��
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
        /// ת��ImageΪIcon
        /// </summary>
        /// <param name="image">Ҫת��Ϊͼ���Image����</param>
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
                        //дͼ��ͷ��
                        bin.Write((short)0);           //0-1����
                        bin.Write((short)1);           //2-3�ļ����͡�1=ͼ��, 2=���
                        bin.Write((short)1);           //4-5ͼ��������ͼ����԰������ͼ��

                        bin.Write((byte)image.Width);  //6ͼ����
                        bin.Write((byte)image.Height); //7ͼ��߶�
                        bin.Write((byte)0);            //8��ɫ����������λ��>=8����0��������Ȼ�ģ��ﵽ8bpp����ɫ��������256��byte������ʾ��
                        bin.Write((byte)0);            //9����������Ϊ0
                        bin.Write((short)0);           //10-11��ɫ��
                        bin.Write((short)32);         //12-13λ��
                        bin.Write((int)msImg.Length);  //14-17λͼ���ݴ�С
                        bin.Write(22);                   //18-21λͼ������ʼ�ֽ�

                        //дͼ������
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
        /// ���ַ���תͼƬ
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
    /// λ��
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
