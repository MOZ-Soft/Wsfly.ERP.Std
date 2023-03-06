using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace Wsfly.ERP.Std.AppCode.Ctls
{
    public class StrokeableLabel : Label
    {
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty FillProperty;
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        static StrokeableLabel()
        {
            PropertyMetadata textMeta = new PropertyMetadata("", (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as StrokeableLabel).InvalidateVisual();
            });
            PropertyMetadata fillMeta = new PropertyMetadata(Brushes.Black, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as StrokeableLabel).InvalidateVisual();
            });
            PropertyMetadata strokMeta = new PropertyMetadata(Brushes.Black, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as StrokeableLabel).InvalidateVisual();
            });
            PropertyMetadata thickMeta = new PropertyMetadata(0.1, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                (d as StrokeableLabel).InvalidateVisual();
            });

            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(StrokeableLabel), textMeta);
            FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(StrokeableLabel), fillMeta);
            StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(StrokeableLabel), strokMeta);
            StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(StrokeableLabel), thickMeta);
        }

        /// <summary>
        /// Create the outline geometry based on the formatted text.
        /// </summary>
        public void CreateText()
        {
            getformattedText(Text == null ? "没有值显示" : Text);
            if (ShowOrigoal) this.Content = Text;
        }

        PathGeometry pg = new PathGeometry();

        private void getformattedText(string str)
        {
            // Create the formatted text based on the properties set.
            FormattedText formattedText = new FormattedText(
                str,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(
                    FontFamily,
                    FontStyle,
                    FontWeight,
                    FontStretch),
                FontSize,
                System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text. 
                );

            if (ShowOrigoal)
            {
                this.Width = formattedText.Width + 10;
                this.Height = formattedText.Height + 10;
            }
            else
            {
                this.Width = formattedText.Width;
                this.Height = formattedText.Height;
            }
            // Build the geometry object that represents the text.
            //pg.AddGeometry(formattedText.BuildGeometry(new System.Windows.Point(5, 5)));

            if (ShowOrigoal)
                TextGeometry = formattedText.BuildGeometry(new System.Windows.Point(5, 5));
            else
                TextGeometry = formattedText.BuildGeometry(new System.Windows.Point(0, 0));

            // Build the geometry object that represents the text hightlight.
            if (Highlight == true)
            {
                TextHighLightGeometry = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));
            }
        }

        /// <summary>
        /// OnRender override draws the geometry of the text and optional highlight.
        /// </summary>
        /// <param name="drawingContext">Drawing context of the OutlineText control.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            CreateText();
            // Draw the outline based on the properties that are set.
            drawingContext.DrawGeometry(Fill, new System.Windows.Media.Pen(Stroke, StrokeThickness), TextGeometry);
            // Draw the text highlight based on the properties that are set.
            if (Highlight == true)
            {
                drawingContext.DrawGeometry(null, new System.Windows.Media.Pen(Stroke, StrokeThickness), TextHighLightGeometry);
            }
        }

        /// <summary>
        /// 字符串的格式化几何对象
        /// </summary>
        public Geometry TextGeometry { get; private set; }

        /// <summary>
        /// 是否高亮（暂时不支持）
        /// </summary>
        public bool Highlight { get; set; }

        /// <summary>
        /// 高亮几何对象
        /// </summary>
        public Geometry TextHighLightGeometry { get; private set; }

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
    }
}
