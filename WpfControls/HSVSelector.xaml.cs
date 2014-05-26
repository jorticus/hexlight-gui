using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RGB.Util.ColorTypes;

namespace RGB.WpfControls
{
    /// <summary>
    /// Interaction logic for HSVSelector.xaml
    /// </summary>
    public partial class HSVSelector : UserControl
    {

        #region Dependency Properties

        public static DependencyProperty ThumbSizeProperty = DependencyProperty.Register("ThumbSize", typeof(double), typeof(HSVSelector), new FrameworkPropertyMetadata(0.0, OnThumbSizeChanged));
        public static DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(HSVSelector), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));
        public static DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(double), typeof(HSVSelector), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public static void OnThumbSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if the thumb size changes
            HSVSelector ctl = (obj as HSVSelector);

            ctl.UpdateThumbSize();
            ctl.InvalidateMeasure();    // Updating the thumbsize changes the dial radius
            ctl.UpdateSelector();       // Also need to re-calculate the thumb position
            ctl.InvalidateVisual();     // And then re-paint everything
        }

        public static void OnColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if a visual-related property changes
            HSVSelector ctl = (obj as HSVSelector);
            ctl.InvalidateVisual();

            // Force an update of the property binding, because animations don't update the property itself.
            //ctl.SetValue(args.Property, args.NewValue);
            if (args.Property == HSVSelector.HueProperty)
                ctl.Hue = (double)args.NewValue;

            if (args.Property == HSVSelector.SaturationProperty)
                ctl.Saturation = (double)args.NewValue;
        }

        #endregion

        #region Designer Properties

        [Description("Radius of the thumb selector"), Category("Appearance")]
        public double ThumbSize
        {
            get { return (double)base.GetValue(ThumbSizeProperty); }
            set { base.SetValue(ThumbSizeProperty, value); UpdateThumbSize(); }
        }

        [Description("Hue, 0.0 to 360.0 degrees"), Category("Common")]
        public double Hue
        {
            get { return (double)base.GetValue(HueProperty); }
            set { base.SetValue(HueProperty, value); }
        }
        [Description("Saturation, 0.0 (white) to 1.0 (saturated)"), Category("Common")]
        public double Saturation
        {
            get { return (double)base.GetValue(SaturationProperty); }
            set { base.SetValue(SaturationProperty, value); }
        }

        #endregion

        #region Other Properties

        private HSVColor hsv;
        public HSVColor Color { get { return hsv; } }

        #endregion

        private bool isDragging = false;
      
        public HSVSelector()
        {
            InitializeComponent();
        }

        #region Rendering

        protected override void OnRender(DrawingContext drawingContext)
        {
            this.UpdateSelector();
            base.OnRender(drawingContext);
        }

        private void UpdateThumbSize()
        {
            selector.Width = ThumbSize;
            selector.Height = ThumbSize;
            wheel.Margin = new Thickness(ThumbSize / 2);
            //wheel.Margin = new Thickness(0);
        }

        private void UpdateSelector()
        {
            double cx = this.ActualWidth / 2.0;
            double cy = this.ActualHeight / 2.0;

            double radius = (wheel.ActualOuterRadius - wheel.ActualInnerRadius) * this.Saturation + wheel.ActualInnerRadius;

            // Snap to middle of wheel when inside InnerRadius
            if (radius < wheel.ActualInnerRadius + float.Epsilon)
                radius = 0.0;

            double angle = this.Hue + 180.0f;

            double x = radius * Math.Sin(angle * Math.PI / 180.0);
            double y = radius * Math.Cos(angle * Math.PI / 180.0);

            double mx = cx + x - this.selector.ActualWidth / 2;
            double my = cy + y - this.selector.ActualHeight / 2;

            hsv.hue = (float)Hue;
            hsv.sat = (float)Saturation;
            hsv.value = 1.0f;

            this.selector.Margin = new Thickness(mx, my, 0, 0);
            this.selector.Fill = new SolidColorBrush(hsv.ToRGB());
            //this.selector.StrokeThickness = this.ThumbSize / 5.0;
        }


        #endregion

        #region Dragging

        private void wheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Mouse.Capture(this.selector as UIElement);
            //isDragging = true;

            AnimateTo(Mouse.GetPosition(this));
        }

        private void selector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            StopAnimation();
            Mouse.Capture((sender as UIElement));
        }

        private void selector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            isDragging = false;
        }

        private void selector_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calculate Hue and Saturation from the mouse position
                UpdateSelectorFromPoint(Mouse.GetPosition(this));
            }
        }

        private void UpdateSelectorFromPoint(Point point)
        {
            this.Hue = CalculateHue(point);
            this.Saturation = CalculateSaturation(point);
            UpdateSelector();
        }

        private double CalculateHue(Point point)
        {
            double cx = this.ActualWidth / 2;
            double cy = this.ActualHeight / 2;

            double dx = point.X - cx;
            double dy = point.Y - cy;

            double angle = Math.Atan2(dx, dy) / Math.PI * 180.0;

            // Hue is offset by 180 degrees, so red appears at the top
            double hue = angle - 180.0;
            if (hue < 0) hue += 360.0;

            return hue;
        }

        private double CalculateSaturation(Point point)
        {
            double cx = this.ActualWidth / 2;
            double cy = this.ActualHeight / 2;

            double dx = point.X - cx;
            double dy = point.Y - cy;

            double dist = Math.Sqrt(dx * dx + dy * dy);

            // Saturation is defined between OuterRadius and InnerRadius (not OuterGradient and InnerGradient)
            float sat = (float)((Math.Min(dist, wheel.ActualOuterRadius) - wheel.ActualInnerRadius) / (wheel.ActualOuterRadius - wheel.ActualInnerRadius));

            return sat;
        }

        #endregion

        #region Animation

        public double AnimationSpeed = 0.5;
        public double AnimationEasePower = 2.0;

        private Storyboard sb = null;

        private static DependencyProperty HSAnimationProperty = DependencyProperty.Register("HSAnimation", typeof(Point), typeof(HSVSelector), new FrameworkPropertyMetadata(OnHSPropertyAnimated));

        private Point HSAnimation
        {
            get { return (Point)base.GetValue(HSAnimationProperty); }
            set { base.SetValue(HSAnimationProperty, value); }
        }

        private static void OnHSPropertyAnimated(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            HSVSelector ctl = (obj as HSVSelector);

            ctl.Hue = (args.NewValue as Point?).Value.X;
            ctl.Saturation = (args.NewValue as Point?).Value.Y;

            ctl.InvalidateVisual();
        }

        private void AnimateTo(Point point)
        {
            //TODO: Might be able to shift this stuff into the XAML
            // by using databindings for the From and To fields.
            // This would allow the style to be overridden later on.

            //TODO: Animate through white

            //TODO: Handle discontinuity at hue=0=360 degrees

            //NOTE: Must use PointAnimation so both hue and saturation get
            // updated at the same time. I tried using two seaprate DoubleAnimations
            // but the second one would not update the property.

            Point from = new Point(this.Hue, this.Saturation);
            Point to = new Point(CalculateHue(point), CalculateSaturation(point));
            Duration duration = new Duration(TimeSpan.FromSeconds(AnimationSpeed));

            sb = new Storyboard();
            sb.Duration = duration;

            var hs_animation = new PointAnimation(from, to, duration);
            hs_animation.EasingFunction = new PowerEase() { Power = AnimationEasePower };
            hs_animation.FillBehavior = FillBehavior.Stop;

            sb.Children.Add(hs_animation);
            Storyboard.SetTarget(hs_animation, this);
            Storyboard.SetTargetProperty(hs_animation, new PropertyPath(HSVSelector.HSAnimationProperty));

            sb.FillBehavior = FillBehavior.Stop;

            sb.Completed += (o, e) =>
            {
                // Set the final colour position
                this.HSAnimation = to;
            };

            // Using a storyboard like this allows us to stop it.
            sb.Begin();
        }

        private void StopAnimation()
        {
            if (sb != null)
                sb.Stop();
        }

        #endregion
    }
}
