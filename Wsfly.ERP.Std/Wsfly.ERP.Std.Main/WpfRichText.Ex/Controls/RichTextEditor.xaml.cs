using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wsfly.ERP.Std.AppCode.Models;

namespace WpfRichText.Ex.Controls
{
    /// <summary>
    /// Interaction logic for BindableRichTextbox.xaml
    /// </summary>
    public partial class RichTextEditor : UserControl
    {
        #region 委托
        /// <summary>
        /// 带参建代理
        /// FlushClient fc = new FlushClient(Function);
        /// this.Invoke(fc, new object[] { param });
        /// </summary>
        public delegate object FlushClientBase(object obj);
        /// <summary>
        /// 无参数代理
        /// </summary>
        /// <returns></returns>
        public delegate object FlushClientBaseDelegate();

        /// <summary>
        /// WsflyBase事件
        /// </summary>
        protected event WsflyBaseEventHandler WsflyBaseEvent;
        /// <summary>
        /// WsflyBase委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        protected delegate object WsflyBaseEventHandler(object sender, object parameter);
        #endregion


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(RichTextEditor), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 构造
        /// </summary>
        public RichTextEditor()
        {
            InitializeComponent();
            this.SizeChanged += RichTextEditor_SizeChanged;
            this.btnUploadImage.Click += BtnUploadImage_Click;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            string filepath = "";
            OpenFileDialog openfilejpg = new OpenFileDialog();
            openfilejpg.Filter = "jpg图片(*.jpg)|*.jpg|gif图片(*.gif)|*.gif";
            openfilejpg.FilterIndex = 0;
            openfilejpg.RestoreDirectory = true;
            openfilejpg.Multiselect = false;

            if (openfilejpg.ShowDialog() == true)
            {
                filepath = openfilejpg.FileName;
                filepath = AppGlobal.UploadFile(filepath);
                filepath = AppGlobal.GetUploadFilePath(filepath);

                Image img = new Image();
                BitmapImage bImg = new BitmapImage();
                img.IsEnabled = true;
                bImg.BeginInit();
                bImg.UriSource = new Uri(filepath, UriKind.Relative);
                bImg.EndInit();
                img.Source = bImg;
                img.Stretch = Stretch.Uniform;

                new InlineUIContainer(img, mainRTB.Selection.Start);
            }
        }

        /// <summary>
        /// 尺寸变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextEditor_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        /// <summary>
        /// 内容
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }
        /// <summary>
        /// 只读
        /// </summary>
        public void Readonly(bool flag = true)
        {
            if (flag)
            {
                this.Toolbar.Visibility = Visibility.Collapsed;
                this.mainRTB.Height = this.Height - 8;
                this.mainRTB.IsReadOnly = true;
            }
            else
            {
                this.Toolbar.Visibility = Visibility.Visible;
                this.mainRTB.Height = this.Height - 40;
                this.mainRTB.IsReadOnly = false;
            }
        }
    }
}
