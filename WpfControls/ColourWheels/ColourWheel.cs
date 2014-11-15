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
using HexLight.Colour;

namespace HexLight.WpfControls.ColourWheels
{
    public abstract class ColourWheel : Canvas
    {
        WriteableBitmap bitmap;

        #region Dependency Properties
        public static DependencyProperty InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(HSVWheel), new UIPropertyMetadata((double)0.0, OnPropertyChanged));

        public static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            HSVWheel ctl = (obj as HSVWheel);
            ctl.InvalidateVisual();
        }

        #endregion

        #region Designer Properties

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

        /// <summary>
        /// The function used to draw the pixels in the colour wheel.
        /// </summary>
        protected RGBStruct ColourFunction(double r, double theta)
        {
            RGBColor rgb = ColourMapping(r, theta, 1.0);
            return new RGBStruct(rgb.Rb, rgb.Gb, rgb.Bb, 255);
        }

        /// <summary>
        /// The colour mapping between Rad/Theta and RGB
        /// </summary>
        /// <param name="r">Radius/Saturation, between 0 and 1</param>
        /// <param name="theta">Angle/Hue, between 0 and 360</param>
        /// <returns>The RGB colour</returns>
        public virtual RGBColor ColourMapping(double radius, double theta, double value)
        {
            return new RGBColor(1.0f, 1.0f, 1.0f);
        }

        public virtual Point InverseColourMapping(RGBColor rgb)
        {
            return new Point(0, 0);
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
                            RGBStruct c = ColourFunction(pr / outer_radius,  ((pa + Math.PI) * 180.0 / Math.PI));

                            // Anti-aliasing
                            // This works by adjusting the alpha to the alias error between the outer radius (which is integer) 
                            // and the computed radius, pr (which is float).
                            double aadelta = pr - (outer_radius - 1.0);
                            if (aadelta >= 0.0)
                                c.a = (byte)(255 - aadelta * 255);

                            color_data = c.ToARGB32();
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
