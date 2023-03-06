using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Data;
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
using Wsfly.ERP.Std.AppCode.Base;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ChooseIconWin.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseFontIconWin : BaseUserControl
    {
        string _defaultIcon = "";

        string[] _defaultColors = {
            "#000000", "#ffffff", "#7f8c8d", "#95a5a6", "#bdc3c7", "#ecf0f1", "#2c3e50", "#34495e", "#c0392b", "#e74c3c",
            "#d35400", "#e67e22", "#f39c12", "#f1c40f", "#8e44ad", "#9b59b6", "#2980b9", "#3498db", "#27ae60", "#2ecc71",
            "#16a085", "#1abc9c", "#e84393","#fd79a8","#3742fa","#5352ed","#1e90ff","#70a1ff","#2ed573","#7bed9f",
            "#ff4757","#ff6b81","#ff6348","#ff7f50","#ffa502","#eccc68","#BDC581","#9AECDB","#58B19F","#EAB543",
            "#1B9CFC","#25CCF7","#6D214F","#B33771","#F97F51","#FEA47F","#182C61"
        };

        /// <summary>
        /// 构造
        /// </summary>
        public ChooseFontIconWin(string value)
        {
            _defaultIcon = value;

            InitializeComponent();

            this.Loaded += ChooseFontIconWin_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseFontIconWin_Loaded(object sender, RoutedEventArgs e)
        {
            //初始大小
            InitSize();
            _ParentWindow.SizeChanged += _ParentWindow_SizeChanged;

            this.txtKeywords.KeyUp += TxtKeywords_KeyUp;
            this.txtColor.KeyUp += TxtColor_KeyUp;
            this.btnOk.Click += BtnOk_Click;

            InitIcons();

            InitColors();

            try
            {
                //默认的图标
                if (_defaultIcon.StartsWith(":"))
                {
                    string packType = _defaultIcon.Split(':')[1];
                    string iconStr = _defaultIcon.Split(':')[2];
                    string colorStr = _defaultIcon.Split(':')[3];

                    this.txtColor.Text = colorStr;

                    try
                    {
                        switch (packType)
                        {
                            case "PackIconMaterial":
                                PackIconMaterial icon = new PackIconMaterial();
                                icon.Width = 28;
                                icon.Height = 28;
                                icon.Kind = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), iconStr);

                                this.panelChooseIcon.Children.Clear();
                                this.panelChooseIcon.Children.Add(icon);
                                break;
                        }

                        _choosePackStr = packType;
                        _chooseIconStr = iconStr;
                    }
                    catch { }

                    ChangeColor();
                }
            }
            catch { }
        }

        /// <summary>
        /// 初始颜色
        /// </summary>
        private void InitColors()
        {
            foreach (string colorStr in _defaultColors)
            {
                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(colorStr);
                    Brush brush = new SolidColorBrush(color);

                    WrapPanel btnColor = new WrapPanel();
                    btnColor.Width = 20;
                    btnColor.Height = 20;
                    btnColor.Background = brush;
                    btnColor.Margin = new Thickness(5);
                    btnColor.Tag = colorStr;
                    btnColor.MouseDown += BtnColor_MouseDown;
                    btnColor.VerticalAlignment = VerticalAlignment.Top;

                    this.panelColors.Children.Add(btnColor);
                }
                catch (Exception ex) { }
            }
        }

        /// <summary>
        /// 初始图标
        /// </summary>
        private void InitIcons()
        {
            var enumType = typeof(PackIconMaterialKind);
            var packType = typeof(PackIconMaterial);

            //string htmlIcons = "";

            foreach (var item in Enum.GetValues(enumType))
            {
                PackIconMaterial icon = new PackIconMaterial();
                icon.Kind = (PackIconMaterialKind)Enum.Parse(enumType, item.ToString());
                icon.FontSize = 32;
                icon.Width = 32;
                icon.Height = 32;

                //htmlIcons += "<i class='material-icons'>" + item.ToString().ToLower()+"</i>\n";

                Button btn = new Button();
                btn.Width = 48;
                btn.Height = 48;
                btn.Margin = new Thickness(10);
                btn.Content = icon;
                btn.ToolTip = item.ToString();
                btn.Tag = item.ToString();
                btn.Click += Btn_Click;
                btn.MouseDoubleClick += Btn_MouseDoubleClick;

                this.panelItems.Children.Add(btn);
            }
        }

        string _choosePackStr = "PackIconMaterial";
        string _chooseIconStr = "None";

        /// <summary>
        /// 选择的图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (sender as Button);
                string iconStr = btn.Tag.ToString();

                _chooseIconStr = iconStr;

                var enumType = typeof(PackIconMaterialKind);

                Brush brush = Brushes.Black;

                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(this.txtColor.Text.Trim());
                    brush = new SolidColorBrush(color);
                }
                catch { }

                PackIconMaterial icon = new PackIconMaterial();
                icon.Width = 28;
                icon.Height = 28;
                icon.Foreground = brush;
                icon.Kind = (PackIconMaterialKind)Enum.Parse(enumType, iconStr);

                this.panelChooseIcon.Children.Clear();
                this.panelChooseIcon.Children.Add(icon);
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtKeywords_KeyUp(object sender, KeyEventArgs e)
        {
            string txt = this.txtKeywords.Text.Trim().ToLower();

            foreach (var item in this.panelItems.Children)
            {
                Button btn = item as Button;
                string name = btn.Tag.ToString().ToLower();

                if (!name.Contains(txt))
                {
                    btn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btn.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 输入颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtColor_KeyUp(object sender, KeyEventArgs e)
        {
            ChangeColor();
        }

        /// <summary>
        /// 改变颜色
        /// </summary>
        private void ChangeColor()
        {
            try
            {
                Brush brush = Brushes.Black;

                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(this.txtColor.Text.Trim());
                    brush = new SolidColorBrush(color);
                }
                catch (Exception ex) { }

                this.panelColor.Background = brush;

                if (this.panelChooseIcon.Children != null)
                {
                    PackIconMaterial icon = this.panelChooseIcon.Children[0] as PackIconMaterial;
                    icon.Foreground = brush;
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 选择颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColor_MouseDown(object sender, RoutedEventArgs e)
        {
            WrapPanel btn = (sender as WrapPanel);
            string colorStr = btn.Tag.ToString();

            this.txtColor.Text = colorStr;

            ChangeColor();
        }

        /// <summary>
        /// 选择图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button btn = (sender as Button);
            string icon = btn.Tag.ToString();

            _chooseIconStr = icon;

            ChooseCallback();
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_chooseIconStr))
            {
                _chooseIconStr = "None";
            }

            ChooseCallback();
        }
        /// <summary>
        /// 选择回调
        /// </summary>
        private void ChooseCallback()
        {
            string color = this.txtColor.Text.Trim();
            try
            {
                ColorConverter.ConvertFromString(color);
            }
            catch (Exception ex) { color = "#000000"; }

            string result = ":" + _choosePackStr + ":" + _chooseIconStr + ":" + color;

            //回调
            (_ParentWindow as Components.PopWindow).CallBack(result);
        }

        /// <summary>
        /// 窗体大小改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitSize();
        }
        /// <summary>
        /// 初始窗体大小
        /// </summary>
        private void InitSize()
        {
            this.scrollMain.Width = _ParentWindow.Width - 40;
            this.scrollMain.Height = _ParentWindow.Height - 70 - 70 - 50;

            this.panelColors.Width = _ParentWindow.Width - 40 - 180 - 250 - 120;
        }
    }
}
