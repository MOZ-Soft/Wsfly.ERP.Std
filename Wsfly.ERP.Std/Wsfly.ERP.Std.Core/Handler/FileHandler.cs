using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace Wsfly.ERP.Std.Core.Handler
{
    public class FileHandler : IDisposable
    {
        private bool _alreadyDispose = false;

        #region 构造函数

        public FileHandler()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        ~FileHandler()
        {
            Dispose(); ;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDispose) return;

            _alreadyDispose = true;
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 取得文件名
        /****************************************
           * 函数名称：GetFileName
           * 功能说明：取得文件名
           * 参    数：文件路径
           * 调用示列：
           *             string path = "aaa.aspx";        
           *             string s = Kernel.GetFileName(path);         
         *****************************************/
        /// <summary>
        /// 取得文件名
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns>.gif|.html格式</returns>
        public static string GetFileName(string path)
        {
            path = path.Replace("\\", "/");

            if (path.LastIndexOf("/") >= 0)
                return path.Substring(path.LastIndexOf("/"));

            return path;
        }
        /// <summary>
        /// 取得文件名称[无后缀]
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutPostfix(string path)
        {
            path = path.Replace("\\", "/");

            if (path.IndexOf("/") >= 0)
            {
                path = path.Substring(path.LastIndexOf("/") + 1);

                if (path.LastIndexOf(".") > 0)
                    path = path.Substring(0, path.LastIndexOf("."));
            }

            return path;
        }
        #endregion

        #region 取得文件后缀名
        /****************************************
           * 函数名称：GetPostfix
           * 功能说明：取得文件后缀名
           * 参    数：filename:文件名称
           * 调用示列：
           *             string filename = "aaa.aspx";        
           *             string s = Kernel.GetPostfixStr(filename);         
         *****************************************/
        /// <summary>
        /// 取后缀名
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>.gif|.html格式</returns>
        public static string GetPostfix(string filename)
        {
            if (filename.LastIndexOf(".") < 0) return "";

            int start = filename.LastIndexOf(".");
            int length = filename.Length;
            string postfix = filename.Substring(start, length - start);
            return postfix;
        }
        #endregion

        #region 取得文件大小

        /// <summary>
        /// 得到文件大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetFileSize(double size)
        {
            string[] strs = {
                                "B",
                                "KB",
                                "MB"
                            };

            for (int i = 0; i < 3; i++)
            {
                if (size < 1024.00)
                    return size.ToString("f") + strs[i];
                else
                    size = System.Math.Round(size / 1024, 2);
            }

            return size + "G";
        }
        /// <summary>
        /// 取得文件大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileSize(string path)
        {
            double size = 0.00;

            if (File.Exists(path))
            {//文件
                FileInfo file = new FileInfo(path);

                size = file.Length;
            }
            else if (Directory.Exists(path))
            {//文件夹
                size = GetDirectorySize(path);
            }
            else
            {
                return "0 字节";
            }

            string[] strs = {
                                "字节",
                                "KB",
                                "MB"
                            };

            for (int i = 0; i < 3; i++)
            {
                if (size < 1024.00)
                    return size + strs[i];
                else
                    size = System.Math.Round(size / 1024, 2);
            }

            return size + "G";
        }

        #endregion

        #region 取得文件类型

        /// <summary>
        /// 取得文件类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileType(string fileName)
        {
            FileInfomation fileInfo = new FileInfomation();  //初始化FileInfomation结构

            //调用GetFileInfo函数，最后一个参数说明获取的是文件类型(SHGFI_TYPENAME)
            int res = GetFileInfo(fileName, (int)FileAttributeFlags.FILE_ATTRIBUTE_NORMAL, ref fileInfo, Marshal.SizeOf(fileInfo), (int)GetFileInfoFlags.SHGFI_TYPENAME);

            return fileInfo.szTypeName;
        }

        //在shell32.dll导入函数SHGetFileInfo
        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
        private static extern int GetFileInfo(string pszPath, int dwFileAttributes, ref FileInfomation psfi, int cbFileInfo, int uFlags);

        //定义SHFILEINFO结构(名字随便起，这里用FileInfomation)
        [StructLayout(LayoutKind.Sequential)]
        private struct FileInfomation
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        //定义文件属性标识
        private enum FileAttributeFlags : int
        {
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_DEVICE = 0x00000040,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
            FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
            FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
            FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000
        }

        //定义获取资源标识
        private enum GetFileInfoFlags : int
        {
            SHGFI_ICON = 0x000000100,     // get icon
            SHGFI_DISPLAYNAME = 0x000000200,     // get display name
            SHGFI_TYPENAME = 0x000000400,     // get type name
            SHGFI_ATTRIBUTES = 0x000000800,     // get attributes
            SHGFI_ICONLOCATION = 0x000001000,     // get icon location
            SHGFI_EXETYPE = 0x000002000,     // return exe type
            SHGFI_SYSICONINDEX = 0x000004000,     // get system icon index
            SHGFI_LINKOVERLAY = 0x000008000,     // put a link overlay on icon
            SHGFI_SELECTED = 0x000010000,     // show icon in selected state
            SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
            SHGFI_LARGEICON = 0x000000000,     // get large icon
            SHGFI_SMALLICON = 0x000000001,     // get small icon
            SHGFI_OPENICON = 0x000000002,     // get open icon
            SHGFI_SHELLICONSIZE = 0x000000004,     // get shell size icon
            SHGFI_PIDL = 0x000000008,     // pszPath is a pidl
            SHGFI_USEFILEATTRIBUTES = 0x000000010,     // use passed dwFileAttribute
            SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040      // Get the index of the overlay
        }

        #endregion

        #region 写文件
        /****************************************
           * 函数名称：WriteFile
           * 功能说明：写文件,会覆盖掉以前的内容
           * 参     数：Path:文件路径,Strings:文本内容
           * 调用示列：
           *             string Path = Server.MapPath("Default2.aspx");       
           *             string Strings = "这是我写的内容啊";
           *             Kernel.WriteFile(Path,Strings);
         *****************************************/
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">文件内容</param>
        /// <param name="charset">编码格式</param>
        public static void WriteFile(string path, string content, string charset = "GB2312")
        {
            Encoding encoding = Encoding.Default;

            try
            {
                if (!string.IsNullOrEmpty(charset))
                {
                    encoding = Encoding.GetEncoding(charset);
                }
            }
            catch { encoding = Encoding.Default; }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            if (!File.Exists(path))
            {
                FileStream f = File.Create(path);
                f.Close();
            }

            StreamWriter f2 = new StreamWriter(path, false, encoding);
            f2.Write(content);

            f2.Close();
            f2.Dispose();
        }
        #endregion

        #region 读文件
        /****************************************
           * 函数名称：ReadFile
           * 功能说明：读取文本内容
           * 参     数：Path:文件路径
           * 调用示列：
           *             string Path = Server.MapPath("Default2.aspx");       
           *             string s = Kernel.ReadFile(Path);
         *****************************************/
        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="charset">编码格式</param>
        /// <returns></returns>
        public static string ReadFile(string path, string charset = "GB2312")
        {
            string s = null;

            if (File.Exists(path))
            {
                Encoding encoding = Encoding.Default;

                try
                {
                    if (!string.IsNullOrEmpty(charset))
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch { encoding = Encoding.Default; }

                StreamReader f2 = new StreamReader(path, encoding);
                s = f2.ReadToEnd();
                f2.Close();
                f2.Dispose();
            }

            return s;
        }
        /// <summary>
        /// 读取文件的最后一行
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="charset">编码格式</param>
        /// <returns></returns>
        public static string ReadLastLine(string path, string charset = "GB2312")
        {
            string s = null;

            if (File.Exists(path))
            {
                Encoding encoding = Encoding.Default;

                try
                {
                    if (!string.IsNullOrEmpty(charset))
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch { encoding = Encoding.Default; }

                StreamReader f2 = new StreamReader(path, encoding);
                while (!f2.EndOfStream)
                {
                    s = f2.ReadLine();
                }
                f2.Close();
                f2.Dispose();
            }

            return s;
        }
        #endregion

        #region 追加文件
        /****************************************
           * 函数名称：FileAdd
           * 功能说明：追加文件内容
           * 参     数：Path:文件路径,strings:内容
           * 调用示列：
           *             string Path = Server.MapPath("Default2.aspx");     
           *             string Strings = "新追加内容";
           *             Kernel.FileAdd(Path, Strings);
         *****************************************/
        /// <summary>
        /// 追加文件
        /// </summary>
        /// <param name="Path">文件路径</param>
        /// <param name="strings">内容</param>
        public static void FileAdd(string path, string strings)
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            }

            StreamWriter sw = new StreamWriter(path, true, Encoding.GetEncoding("GB2312"));//File.AppendText(path)
            sw.Write(strings);
            sw.Flush();
            sw.Close();
        }
        #endregion

        #region 拷贝文件
        /****************************************
           * 函数名称：FileCoppy
           * 功能说明：拷贝文件
           * 参     数：OrignFile:原始文件,NewFile:新文件路径
           * 调用示列：
           *             string orignFile = Server.MapPath("Default2.aspx");     
           *             string NewFile = Server.MapPath("Default3.aspx");
           *             Kernel.FileCoppy(OrignFile, NewFile);
         *****************************************/
        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="OrignFile">原始文件</param>
        /// <param name="NewFile">新文件路径</param>
        public static void FileCoppy(string orignFile, string newFile)
        {
            if (!File.Exists(orignFile)) return;

            File.Copy(orignFile, newFile, true);
        }

        #endregion

        #region 删除文件
        /****************************************
           * 函数名称：FileDel
           * 功能说明：删除文件
           * 参     数：Path:文件路径
           * 调用示列：
           *             string Path = Server.MapPath("Default3.aspx");    
           *             Kernel.FileDel(Path);
         *****************************************/
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="Path">路径</param>
        public static void FileDel(string path)
        {
            if (!File.Exists(path)) return;
            File.Delete(path);
        }
        #endregion

        #region 移动文件
        /****************************************
           * 函数名称：FileMove
           * 功能说明：移动文件
           * 参     数：OrignFile:原始路径,NewFile:新文件路径
           * 调用示列：
           *             string orignFile = Server.MapPath("../说明.txt");    
           *             string NewFile = Server.MapPath("../../说明.txt");
           *             Kernel.FileMove(OrignFile, NewFile);
         *****************************************/
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="OrignFile">原始路径</param>
        /// <param name="NewFile">新路径</param>
        public static void FileMove(string orignFile, string newFile)
        {
            if (!File.Exists(orignFile)) return;

            File.Move(orignFile, newFile);
        }
        #endregion
        
        #region 在当前目录下创建目录
        /****************************************
           * 函数名称：FolderCreate
           * 功能说明：在当前目录下创建目录
           * 参     数：OrignFolder:当前目录,NewFloder:新目录
           * 调用示列：
           *             string orignFolder = Server.MapPath("test/");    
           *             string NewFloder = "new";
           *             Kernel.FolderCreate(OrignFolder, NewFloder); 
         *****************************************/
        /// <summary>
        /// 在当前目录下创建目录
        /// </summary>
        /// <param name="OrignFolder">当前目录</param>
        /// <param name="NewFloder">新目录</param>
        public static void CreateDirectory(string orignFolder, string NewFloder)
        {
            Directory.SetCurrentDirectory(orignFolder);
            Directory.CreateDirectory(NewFloder);
        }
        #endregion

        #region 递归删除文件夹目录及文件
        /****************************************
           * 函数名称：DeleteFolder
           * 功能说明：递归删除文件夹目录及文件
           * 参     数：dir:文件夹路径
           * 调用示列：
           *             string dir = Server.MapPath("test/");  
           *             Kernel.DeleteFolder(dir);       
         *****************************************/
        /// <summary>
        /// 递归删除文件夹目录及文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                        File.Delete(d); //直接删除其中的文件 
                    else
                        DeleteFolder(d); //递归删除子文件夹 
                }

                Directory.Delete(dir); //删除已空文件夹 
            }

        }

        #endregion

        #region 递归获取文件目录及文件
        /// <summary>
        /// 递归获取所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<string> GetFiles(string dir)
        {
            List<string> files = new List<string>();

            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        files.Add(d);
                    }
                    else
                    {
                        //递归
                        List<string> subFiles = GetFiles(d);
                        if (subFiles != null && subFiles.Count > 0) files.AddRange(subFiles);
                    }
                }
            }

            return files;
        }
        /// <summary>
        /// 递归获取所有文件夹及文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<string> GetDirAndFiles(string dir)
        {
            List<string> files = new List<string>();

            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    files.Add(d);

                    if (Directory.Exists(d))
                    {
                        //递归目录
                        List<string> subFiles = GetDirAndFiles(d);
                        if (subFiles != null && subFiles.Count > 0) files.AddRange(subFiles);
                    }
                }
            }

            return files;
        }
        #endregion

        #region 获取文件夹的大小

        public static long GetDirectorySize(string path)
        {
            ///如果文件夹不存在
            if (!Directory.Exists(path)) { return -1; }

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            long sumSize = 0;

            foreach (FileSystemInfo fsInfo in dirInfo.GetFileSystemInfos())
            {
                if (fsInfo.Attributes.ToString().ToLower() == "directory")
                {
                    sumSize += GetDirectorySize(fsInfo.FullName);
                }
                else
                {
                    FileInfo fiInfo = new FileInfo(fsInfo.FullName);
                    sumSize += fiInfo.Length;
                }
            }
            return sumSize;
        }

        #endregion

        #region 将指定文件夹下面的所有内容copy到目标文件夹下面 果目标文件夹为只读属性就会报错。
        /****************************************
           * 函数名称：CopyDir
           * 功能说明：将指定文件夹下面的所有内容copy到目标文件夹下面 果目标文件夹为只读属性就会报错。
           * 参     数：srcPath:原始路径,aimPath:目标文件夹
           * 调用示列：
           *             string srcPath = Server.MapPath("test/");  
           *             string aimPath = Server.MapPath("test1/");
           *             Kernel.CopyDir(srcPath,aimPath);   
         *****************************************/
        /// <summary>
        /// 指定文件夹下面的所有内容copy到目标文件夹下面
        /// </summary>
        /// <param name="srcPath">原始路径</param>
        /// <param name="aimPath">目标文件夹</param>
        public static void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加之
                if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                    aimPath += Path.DirectorySeparatorChar;
                // 判断目标目录是否存在如果不存在则新建之
                if (!Directory.Exists(aimPath))
                    Directory.CreateDirectory(aimPath);
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                //如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                //string[] fileList = Directory.GetFiles(srcPath);
                string[] fileList = Directory.GetFileSystemEntries(srcPath);
                //遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    //先当作目录处理如果存在这个目录就递归Copy该目录下面的文件

                    if (Directory.Exists(file))
                        CopyDir(file, aimPath + Path.GetFileName(file));
                    //否则直接Copy文件
                    else
                        File.Copy(file, aimPath + Path.GetFileName(file), true);
                }

            }
            catch (Exception ee)
            {
                throw new Exception(ee.ToString());
            }
        }


        #endregion

        #region 图片文件转换
        /// <summary>
        /// 将图片Image转换成Byte[]
        /// </summary>
        /// <param name="Image">image对象</param>
        /// <param name="imageFormat">后缀名</param>
        /// <returns></returns>
        public static byte[] ImageToBytes(System.Drawing.Image Image, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if (Image == null) return null;
            byte[] data = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (System.Drawing.Bitmap Bitmap = new System.Drawing.Bitmap(Image))
                {
                    Bitmap.Save(ms, imageFormat);
                    ms.Position = 0;
                    data = new byte[ms.Length];
                    ms.Read(data, 0, Convert.ToInt32(ms.Length));
                    ms.Flush();
                }
            }

            return data;
        }
        /// <summary>
        /// Image转换成Bitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap ImageToBitmap(System.Drawing.Image image)
        {
            //System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)image;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image);
            return bmp;
        }
        /// <summary>
        /// byte[]转换成Image
        /// </summary>
        /// <param name="bytes">二进制图片流</param>
        /// <returns>Image</returns>
        public static System.Drawing.Image BytesToImage(byte[] bytes)
        {
            if (bytes == null) return null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
                ms.Flush();
                return returnImage;
            }
        }
        /// <summary>
        /// byte[]转换成Bitmap
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap BytesToBitmap(byte[] Bytes)
        {
            MemoryStream stream = null;

            try
            {
                stream = new MemoryStream(Bytes);
                return new System.Drawing.Bitmap((System.Drawing.Image)new System.Drawing.Bitmap(stream));
            }
            catch (Exception) { }
            finally
            {
                stream.Close();
            }

            return null;
        }
        /// <summary>
        /// Bitmap转换成Image
        /// </summary>
        /// <param name="Bi"></param>
        /// <returns></returns>
        public static System.Drawing.Image BitmapToImage(System.Drawing.Bitmap bitmap)
        {
            System.Drawing.Image img = bitmap;
            return img;
        }
        /// <summary>
        /// Bitmap转换为byte[]
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] BitmapToBytes(System.Drawing.Bitmap bitmap, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, imageFormat);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                stream.Flush();
                return data;
            }
        }
        #endregion

        #region Stream/byte[]/file转换
        /// <summary>
        /// Stream转换为byte[]
        /// </summary>
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        /// <summary>
        /// byte[]转换为Stream
        /// </summary>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        /// <summary>
        /// Stream 写入文件
        /// </summary>
        public static void StreamToFile(Stream stream, string fileName)
        {
            // 把 Stream 转换成 byte[]
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);

            // 把 byte[] 写入文件
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
                bw.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 从文件读取Stream
        /// </summary>
        public static Stream FileToStream(string fileName)
        {
            // 打开文件
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // 读取文件的 byte[]
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                fileStream.Close();
                // 把 byte[] 转换成 Stream
                Stream stream = new MemoryStream(bytes);
                return stream;
            }
        }
        /// <summary>
        /// 从文件读取Bytes
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] FileToBytes(string fileName)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffur = new byte[fs.Length];
                    fs.Read(buffur, 0, (int)fs.Length);
                    return buffur;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 生成路径
        /// <summary>
        /// 生成不重复的临时文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static string BuildNotRepeatTempFilePath(string fileName, string fileExt)
        {
            string tempDirName = AppDomain.CurrentDomain.BaseDirectory;
            tempDirName = tempDirName + "AppData\\_Temp\\";

            return BuildNotRepeatFilePath(tempDirName, fileName, fileExt);
        }
        /// <summary>
        /// 生成不重复的文件路径
        /// </summary>
        /// <param name="saveDir"></param>
        /// <param name="fileName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static string BuildNotRepeatFilePath(string saveDir, string fileName, string fileExt)
        {
            //是否需要创建文件夹
            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                //自动生成 长时间文件名
                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            }
            else
            {
                //过滤特殊字符
                //\ / : * ? " < > |
                fileName = fileName.Replace("\\", "");
                fileName = fileName.Replace("/", "");
                fileName = fileName.Replace(":", "");
                fileName = fileName.Replace("?", "");
                fileName = fileName.Replace("\"", "");
                fileName = fileName.Replace("<", "");
                fileName = fileName.Replace(">", "");
                fileName = fileName.Replace("|", "");
            }

            //保存目录
            saveDir = saveDir.Trim('\\') + "\\";

            //保存路径
            string filePath = saveDir + fileName + fileExt;

            //文件索引
            int index = 1;

            //是否存在文件
            while (File.Exists(filePath))
            {
                //如果文件存在 则重命名
                filePath = saveDir + fileName + "(" + index + ")" + fileExt;

                //下个文件索引
                index++;
            }

            //返回路径
            return filePath;
        }
        #endregion

        #region 文件权限
        /// <summary>
        /// 获取目录权限
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<FileSystemAccessRule> GetDirectorySecurity(string fileName)
        {
            List<FileSystemAccessRule> rules = new List<FileSystemAccessRule>();

            //列出目标目录所具有的权限
            DirectorySecurity sec = Directory.GetAccessControl(fileName, AccessControlSections.All);
            foreach (FileSystemAccessRule rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                //rule.IdentityReference.Value
                //rule.FileSystemRights

                rules.Add(rule);
            }

            return rules;
        }
        /// <summary>
        /// 添加目录权限
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="account"></param>
        /// <param name="userRights">FCWR</param>
        public static bool AddDirectorySecurity(string fileName, string account, FileSystemRights userRights)
        {
            try
            {
                bool ok;
                DirectoryInfo dInfo = new DirectoryInfo(fileName);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                FileSystemAccessRule accessRule = new FileSystemAccessRule(account, userRights, iFlags, PropagationFlags.None, AccessControlType.Allow);
                dSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out ok);

                dInfo.SetAccessControl(dSecurity);
                return true;
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion
    }
}
