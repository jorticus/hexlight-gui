using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using RGB.Util.ColorTypes;

namespace RGB.WpfControls
{
    public class HSVWheel : Canvas
    {
        WriteableBitmap bitmap;

        #region Dependency Properties
        public static DependencyProperty SegmentCountProperty = DependencyProperty.Register("SegmentCount", typeof(uint), typeof(HSVWheel), new UIPropertyMetadata((uint)0, OnPropertyChanged));
        public static DependencyProperty DialQualityProperty = DependencyProperty.Register("DialQuality", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)4.0, OnPropertyChanged));
        public static DependencyProperty OuterSaturationProperty = DependencyProperty.Register("OuterSaturation", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)1.0, OnPropertyChanged));
        public static DependencyProperty InnerGradientProperty = DependencyProperty.Register("InnerGradient", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)0.15, OnPropertyChanged));
        public static DependencyProperty OuterGradientProperty = DependencyProperty.Register("OuterGradient", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)1.0, OnPropertyChanged));
        public static DependencyProperty InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)0.0, OnPropertyChanged));

        public static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            HSVWheel ctl = (obj as HSVWheel);
            ctl.InvalidateVisual();
        }

        #endregion

        #region Designer Properties

        [Description("The number of colour segments to use. Set to 0 to automatically determine based on SegmentQuality"), Category("Appearance")]
        public uint SegmentCount
        {
            get { return (uint)base.GetValue(SegmentCountProperty); }
            set { base.SetValue(SegmentCountProperty, value); }
        }

        [Description("If SegmentCount is 0, automatically calculate the number of segments required using this factor. Lower values produce more segments, and should be between 2.0 - 5.0"), Category("Appearance")]
        public double DialQuality
        {
            get { return (double)base.GetValue(DialQualityProperty); }
            set { base.SetValue(DialQualityProperty, value); }
        }

        [Description("The maximum saturation around the outer edge of the wheel. Must be between 0.0 to 1.0"), Category("Appearance")]
        public double OuterSaturation
        {
            get { return (double)base.GetValue(OuterSaturationProperty); }
            set { base.SetValue(OuterSaturationProperty, value); }
        }

        [Description("The endstop for the inner gradient, with 0.0 being in the middle and 1.0 being on the outer edge."), Category("Appearance")]
        public double InnerGradient
        {
            get { return (double)base.GetValue(InnerGradientProperty); }
            set { base.SetValue(InnerGradientProperty, value); }
        }

        [Description("The endstop for the outer gradient, with 0.0 being in the middle and 1.0 being on the outer edge."), Category("Appearance")]
        public double OuterGradient
        {
            get { return (double)base.GetValue(OuterGradientProperty); }
            set { base.SetValue(OuterGradientProperty, value); }
        }

        [Description("The radius of the inner circle, as a percentage of the outer circle radius. Must be positive but less than 1.0"), Category("Appearance")]
        public double InnerRadius
        {
            get { return (double)base.GetValue(InnerRadiusProperty); }
            set { base.SetValue(InnerRadiusProperty, value); }
        }

        #endregion

        #region Other Properties

        public double ActualOuterRadius { get; private set; }
        public double ActualInnerRadius { get { return ActualOuterRadius * InnerRadius; } }

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            //this.Effect = new BlurEffect() { Radius = 5 };
            DrawHsvDial(dc);
        }

        protected struct rgbastruct
        {
            public byte r, g, b, a;

            public rgbastruct(byte r, byte g, byte b, byte a)
            {
                this.a = a; this.r = r; this.g = g; this.b = b;
            }
        }

        protected virtual rgbastruct ColourFunction(double r, double theta)
        {
            HSVColor hsv = new HSVColor((float)((theta + Math.PI) * 180.0 / Math.PI), (float)r, 1.0f);
            RGBColor rgb = hsv.ToRGB();

            return new rgbastruct(rgb.Rb, rgb.Gb, rgb.Bb, 255);
        }

        protected void DrawHsvDial(DrawingContext drawingContext)
        {
            float cx = (float)this.ActualWidth / 2.0f;
            float cy = (float)this.ActualHeight / 2.0f;

            float outer_radius = (float)Math.Min(cx, cy);
            ActualOuterRadius = outer_radius;

            //double outer_circumference = 2.0 * Math.PI * outer_radius;

            int bmp_width = (int)this.ActualWidth;
            int bmp_height = (int)this.ActualHeight;

            if (bmp_width <= 0 || bmp_height <= 0)
                return;

            bitmap = new WriteableBitmap(bmp_width, bmp_height, 96.0, 96.0, PixelFormats.Bgra32, null);

            bitmap.Lock();
            unsafe
            {
                int pBackBuffer = (int)bitmap.BackBuffer;

                for (int y = 0; y < bmp_height; y++) {
                    for (int x = 0; x < bmp_width; x++) {
                        int color_data = 0;
                        //double inner_radius = radius * InnerRadius;

                        // Convert xy to normalized polar co-ordinates
                        double dx = x - cx;
                        double dy = y - cy;
                        double pr = Math.Sqrt(dx * dx + dy * dy);

                        // Only draw stuff within the circle
                        if (pr <= outer_radius)
                        {
                            // Compute the colour for the given pixel using polar co-ordinates
                            double pa = Math.Atan2(dx, dy);
                            rgbastruct c = ColourFunction(pr / outer_radius, pa);

                            // Anti-aliasing
                            // This works by adjusting the alpha to the alias error between the outer radius (which is integer) 
                            // and the computed radius, pr (which is float).
                            double aadelta = pr - (outer_radius - 1.0);
                            if (aadelta >= 0.0)
                                c.a = (byte)(255 - aadelta * 255);

                            color_data = (c.a << 24) | (c.r << 16) | (c.g << 8) | (c.b << 0);
                        }

                        *((int*)pBackBuffer) = color_data;
                        pBackBuffer += 4;
                    }
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, bmp_width, bmp_height)); // I like to get dirty
            bitmap.Unlock();

            
            drawingContext.DrawImage(bitmap, new Rect(this.RenderSize));

        }

        #endregion
    }
}
