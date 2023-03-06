using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Wsfly.ERP.Std.Core.Handler
{
    /// <summary>
    /// 上传文件辅助 选择文件
    /// </summary>
    public class UploadFileHandler
    {
        /// <summary>
        /// 选择目录
        /// </summary>
        /// <returns></returns>
        public static string ChooseFolderDialog()
        {
            //选择文件夹
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowDialog();

            //返回选择
            return folderDlg.SelectedPath;
        }
        /// <summary>
        /// 显示选择文件
        /// </summary>
        /// <param name="filter">筛选文件类型 如：图像文件|*.jpg</param>
        /// <returns></returns>
        public static string ChooseFileDialog(string filter = "All Files(*.*)|*.*")
        {
            List<string> paths = ChooseFilesDialog(false, filter);

            if (paths != null && paths.Count > 0) return paths[0];

            return string.Empty;
        }
        /// <summary>
        /// 显示选择文件
        /// </summary>
        /// <param name="multiselect">是否可以多选</param>
        /// <param name="filter">筛选文件类型 如：图像文件|*.jpg</param>
        /// <returns></returns>
        public static List<string> ChooseFilesDialog(bool multiselect = true, string filter = "All Files(*.*)|*.*")
        {
            //图像Filetr
            //图像文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择文件";
            openFileDialog.Filter = filter;
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = multiselect;
            //openFileDialog.DefaultExt = "jpg";
            DialogResult result = openFileDialog.ShowDialog();

            //未选择文件
            if (result == DialogResult.Cancel) return null;
            
            //选择文件数组
            List<string> paths = new List<string>(openFileDialog.FileNames);

            //返回选择文件
            return paths;
        }
    }
}
