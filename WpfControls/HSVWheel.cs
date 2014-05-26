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
using RGB.Util.ColorTypes;

namespace RGB.WpfControls
{
    public class HSVWheel : Canvas
    {
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

        protected void DrawHsvDial(DrawingContext drawingContext)
        {
            //TODO: Optimise the following code by shifting calculations out and only call when properties are changed
            HSVColor hsv = new HSVColor(0.0f, (float)OuterSaturation, 1.0f);

            double cx = this.ActualWidth / 2.0;
            double cy = this.ActualHeight / 2.0;

            double outer_radius = Math.Min(cx, cy);
            double inner_radius = outer_radius * InnerRadius;
            ActualOuterRadius = outer_radius;

            double outer_circumference = 2.0 * Math.PI * outer_radius;

            // Automatically determine the number of segments to use, based on the circumference and quality factor
            uint num_segments = SegmentCount;
            if (num_segments == 0)
                num_segments = (uint)Math.Round(outer_circumference / DialQuality);

            //double line_thickness = Math.Ceiling(outer_circumference / (double)num_segments);

            // Determine the angle of each segment
            double angle_delta = 360.0 / (double)num_segments;
            double angle = -angle_delta/2;

            // Draw a white inner circle to prevent the center from being transparent
            if (inner_radius > 0.0)
                drawingContext.DrawEllipse(Brushes.White, null, new Point(cx, cy), inner_radius+1, inner_radius+1);

            Point? last_point = null;
            for (int i = 0; i < num_segments+1; i++)
            {
                hsv.hue = (float)angle + 180.0f;
                double x = Math.Sin(angle_delta * Math.PI / 180.0);
                double y = Math.Cos(angle_delta * Math.PI / 180.0);

                Point pt1 = new Point(cx + x * inner_radius, cy + y * inner_radius);
                Point pt2 = new Point(cx + x * outer_radius, cy + y * outer_radius);

                //Point pt2b = new Point(cx + Math.Sin(-1 * Math.PI / 180.0) * outer_radius, cy + Math.Cos(-angle_delta * Math.PI / 180.0) * outer_radius);

                // Create a brush for this slice, with the required HSV colour
                // NOTE: The gradient is an approximation to the Value channel of HSV, 
                // so the resulting dial doesn't quite match a real HSV dial
                LinearGradientBrush brush = new LinearGradientBrush(Colors.White, hsv.ToRGB(), new Point(0.49, InnerGradient), new Point(0.5, OuterGradient));
                
                //brush.RelativeTransform = new RotateTransform(-angle+angle_delta/2, 0.5, 0.5);
                //RadialGradientBrush brush = new RadialGradientBrush(Colors.White, hsv.ToRGB());
                //brush.Center = new Point(0.5, 0.9);
                //brush.GradientOrigin = new Point(0.9, 0.5);

                Pen pen = new Pen(brush, 1.0); // A thin pen fixes segment appearance
                //drawingContext.DrawLine(pen, pt1, pt2);
  
                if (last_point.HasValue)
                {
                    
                    // Create a wedge slice
                    var figure = new PathFigure(pt1, new[] { 
                        new LineSegment(last_point.Value, true),
                        new LineSegment(pt2, true),
                    }, true);
                    var geometry = new PathGeometry(new[] { figure });

                    //geometry.Transform = new RotateTransform(-angle, this.ActualWidth / 2.0, this.ActualHeight/2.0);
                    //brush.Transform = geometry.Transform;// new RotateTransform(30, pt2.X, pt2.Y);
                    
                    // Transform each wedge by the required angle. 
                    // This avoids having to mess around with gradient transforms, which is an absolute nightmare
                    drawingContext.PushTransform(new RotateTransform(-angle, cx, cy));
                    drawingContext.DrawGeometry(brush, pen, geometry);
                    drawingContext.Pop();
                }
                //last_point = pt2;
                last_point = new Point(cx, cy + outer_radius);

                angle += angle_delta;
            }
        }

        #endregion
    }
}
