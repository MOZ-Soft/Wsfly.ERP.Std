using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace Wsfly.ERP.Std.AppCode.Ctls
{
    public class FormatedText:StackPanel
    {
        public static readonly DependencyProperty StretchSizeProperty;
        public static readonly DependencyProperty TextOrientationProperty;

        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty FillProperty;
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty FontStyleProperty;
        public static readonly DependencyProperty FontWeightProperty;
        public static readonly DependencyProperty FontStretchProperty;
        

        static FormatedText()
        {
            PropertyMetadata stretchMeta = new PropertyMetadata(0.0, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata orienMeta = new PropertyMetadata(System.Windows.Controls.Orientation.Horizontal, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).Orientation = (Orientation)e.NewValue;
                (d as FormatedText).CreateText();
            });


            PropertyMetadata textMeta = new PropertyMetadata("", (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText(e.NewValue as string);
            });
            PropertyMetadata fillMeta = new PropertyMetadata(Brushes.Black, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata strokMeta = new PropertyMetadata(Brushes.Black, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata thickMeta = new PropertyMetadata(0.1, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });

            PropertyMetadata fontsizeMeta = new PropertyMetadata(20.0, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata fontfamilyMeta = new PropertyMetadata(new FontFamily("方正大黑简体"), (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata fontstyleMeta = new PropertyMetadata(new FontStyle(), (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata fontWeightMeta = new PropertyMetadata(new FontWeight(), (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });
            PropertyMetadata fontStretchMeta = new PropertyMetadata(new FontStretch(), (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as FormatedText).CreateText();
            });

            StretchSizeProperty = DependencyProperty.Register("StretchSize", typeof(double), typeof(FormatedText), stretchMeta);
            TextOrientationProperty = DependencyProperty.Register("TextOrientation", typeof(Orientation), typeof(FormatedText), orienMeta);


            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FormatedText), textMeta);
            FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(FormatedText), fillMeta);
            StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(FormatedText), strokMeta);
            StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(FormatedText), thickMeta);


            FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(FormatedText), fontsizeMeta);
            FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(FormatedText), fontfamilyMeta);
            FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(FormatedText), fontstyleMeta);
            FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(FormatedText), fontWeightMeta);
            FontStretchProperty = DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(FormatedText), fontStretchMeta);
        }

        private void CreateText()
        {
            CreateText(Text);
        }


        private void CreateText(string newStr)
        {
            this.Children.Clear();
            if (newStr == null)
                return;
            this.Orientation = TextOrientation;

            for (int i = 0; i < newStr.Length; i++)
            {
                if (i < newStr.Length - 1)
                    addChar(newStr[i], false);
                else
                    addChar(newStr[i], true);
            }
        }

        /// <summary>
        /// 添加一个字符
        /// </summary>
        /// <param name="c"></param>
        private void addChar(char c, bool ignore)
        {
            StrokeableLabel label = new StrokeableLabel();
            label.Text = c + "";
            label.Fill = this.Fill;
            label.Stroke = this.Stroke;
            label.StrokeThickness = this.StrokeThickness;
            label.FontSize = this.FontSize;
            label.FontFamily = this.FontFamily;
            label.FontStyle = this.FontStyle;
            label.FontWeight = this.FontWeight;
            label.FontStretch = this.FontStretch;
            if (!ignore)
                switch (Orientation)
                {
                    case System.Windows.Controls.Orientation.Horizontal:
                        label.Margin = new Thickness(0, 0, StretchSize, 0);
                        break;
                    case System.Windows.Controls.Orientation.Vertical:
                        label.Margin = new Thickness(0, 0, 0, StretchSize);
                        break;
                }
            label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.Children.Add(label);
        }

        /// <summary>
        /// 设置文字方向
        /// </summary>
        public System.Windows.Controls.Orientation TextOrientation
        {
            get { return (Orientation)GetValue(TextOrientationProperty); }
            set { SetValue(TextOrientationProperty, value); }
        }

        /// <summary>
        /// 字符间距
        /// </summary>
        public double StretchSize
        {
            get
            {
                return (double)GetValue(StretchSizeProperty);
            }
            set
            {
                SetValue(StretchSizeProperty, value);
            }
        }

        /// <summary>
        /// 是否显示原样压盖
        /// </summary>
        public bool ShowOrigoal
        {
            set;
            get;
        }

        /// <summary>
        /// 字符串格式化对象的填充花刷
        /// </summary>
        public Brush Fill { get { return GetValue(FillProperty) as Brush; } set { SetValue(FillProperty, value); } }

        /// <summary>
        /// 边缘画刷
        /// </summary>
        public Brush Stroke { get { return GetValue(StrokeProperty) as Brush; } set { SetValue(StrokeProperty, value); } }

        /// <summary>
        /// 边缘宽度
        /// </summary>
        public double StrokeThickness { get { return (double)GetValue(StrokeThicknessProperty); } set { SetValue(StrokeThicknessProperty, value); } }

        /// <summary>
        /// 显示的文字
        /// </summary>
        public string Text
        {
            get
            {
                return GetValue(TextProperty) as string;
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return GetValue(FontFamilyProperty) as FontFamily; }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }
    }
}
