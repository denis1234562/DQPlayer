using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DQPlayer.Helpers.CustomControls
{
    public class OutlinedLabel : FrameworkElement
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                InvalidateVisual();
            }
        }

        public Brush TextColor
        {
            get => (Brush)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }    

        public Brush OutlineBrush
        {
            get => (Brush)GetValue(OutlineBrushProperty);
            set => SetValue(OutlineBrushProperty, value);
        }

        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(Brush), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty OutlineBrushProperty =
            DependencyProperty.Register(nameof(OutlineBrush), typeof(Brush), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(OutlinedLabel),
                new PropertyMetadata(null));

        private Size _startSize;

        public OutlinedLabel()
        {
            Text = string.Empty;
            FontFamily = new FontFamily("Seago UI");
            TextColor = Brushes.Black;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_startSize == default(Size))
            {
                _startSize = new Size(ActualWidth, ActualHeight);
            }

            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            var pen = new Pen(OutlineBrush ?? TextColor, (ActualHeight / _startSize.Height) * Thickness);
            drawingContext.DrawGeometry(TextColor, pen, BuildGeometryOfLabel());
        }

        private Geometry BuildGeometryOfLabel()
        {
            var emSize = (72 / 128d) * ActualHeight;
            var formattedText = new FormattedText(
                Text, new CultureInfo("en-us"), FlowDirection, new Typeface(FontFamily.Source), emSize, Brushes.Black)
            {
                TextAlignment = TextAlignment
            };
            formattedText.SetFontWeight(FontWeight);

            var origin = new Point(ActualWidth / 2, (ActualHeight - formattedText.Height) / 2);

            return formattedText.BuildGeometry(origin);
        }
    }
}
