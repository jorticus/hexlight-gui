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
using HexLight.Colour;
using HexLight.Util;

namespace HexLight.WpfControls
{
    /// <summary>
    /// Interaction logic for WheelSelector.xaml
    /// </summary>
    public partial class WheelSelector : UserControl
    {
        //NOTE: Theta is equivalent to Hue, and Rad is equivalent to Saturation, if an HSV colour model is being used.
        // This is done in case other colour models are being used.

        #region Dependency Properties

        public static DependencyProperty ThumbSizeProperty = DependencyProperty.Register("ThumbSize", typeof(double), typeof(WheelSelector), new FrameworkPropertyMetadata(0.0, OnThumbSizeChanged));
        public static DependencyProperty ThetaProperty = DependencyProperty.Register("Theta", typeof(double), typeof(WheelSelector), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));
        public static DependencyProperty RadProperty = DependencyProperty.Register("Rad", typeof(double), typeof(WheelSelector), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));
        public static DependencyProperty RGBValueProperty = DependencyProperty.Register("RGBValue", typeof(RGBColor), typeof(WheelSelector), new FrameworkPropertyMetadata(new RGBColor(1.0f, 1.0f, 1.0f), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public static void OnThumbSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if the thumb size changes
            WheelSelector ctl = (obj as WheelSelector);

            ctl.UpdateThumbSize();
            ctl.InvalidateMeasure();    // Updating the thumbsize changes the dial radius
            ctl.UpdateSelector();       // Also need to re-calculate the thumb position
            ctl.InvalidateVisual();     // And then re-paint everything
        }

        private static bool propUpdateFlag = false;

        public static void OnColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            // Force a re-render of the object if a visual-related property changes
            WheelSelector ctl = (obj as WheelSelector);
            ctl.InvalidateVisual();

            //TODO: Figure out how to handle circular dependencies properly.
            // Currently I'm using propUpdateFlag to prevent infinite loops here.
            // Ideally there should be a way to update the local values without calling 
            // this callback again (but still notifying other users)

            if (!propUpdateFlag)
            {
                propUpdateFlag = true;

                if (args.Property == WheelSelector.ThetaProperty || args.Property == WheelSelector.RadProperty)
                    ctl.RGBValue = ctl.wheel.ColourMapping(ctl.Rad, CircularMath.Mod(ctl.Theta), 1.0);

                if (args.Property == WheelSelector.RGBValueProperty)
                {
                    var rgb = (RGBColor)args.NewValue;
                    var pt = ctl.wheel.InverseColourMapping(rgb);
                    ctl.SetCurrentValue(ThetaProperty, pt.X);
                    ctl.SetCurrentValue(RadProperty, pt.Y);
                }

                propUpdateFlag = false;
            }
        }

        #endregion

        #region Designer Properties

        [Description("Radius of the thumb selector"), Category("Appearance")]
        public double ThumbSize
        {
            get { return (double)base.GetValue(ThumbSizeProperty); }
            set { base.SetValue(ThumbSizeProperty, value); UpdateThumbSize(); }
        }

        [Description("Theta, an angle, 0.0 to 360.0 degrees"), Category("Common")]
        public double Theta
        {
            get { return (double)base.GetValue(ThetaProperty); }
            set { base.SetValue(ThetaProperty, CircularMath.Mod(value)); }
        }
        [Description("Rad, radial distance, 0.0 to 1.0"), Category("Common")]
        public double Rad
        {
            get { return (double)base.GetValue(RadProperty); }
            set { base.SetValue(RadProperty, value); }
        }

        [Description("Currently selected colour"), Category("Common")]
        public RGBColor RGBValue
        {
            get { return (RGBColor)base.GetValue(RGBValueProperty); }
            set { base.SetValue(RGBValueProperty, value); }
        }

        #endregion

        #region Other Properties

        private HSVColor hsv;
        private RGBColor rgbValue;
        //public HSVColor Color { get { return hsv; } }


        public Type WheelClass { 
            get { return wheelClass; }
            set { wheelClass = value; InstantiateWheel(); }
        }

        protected Type wheelClass = typeof(ColourWheels.HSVWheel);
        protected ColourWheels.ColourWheel wheel;

        #endregion

        private bool isDragging = false;
      
        public WheelSelector()
        {
            InitializeComponent();

            if (wheel == null)
                InstantiateWheel();
        }

        protected void InstantiateWheel()
        {
            if (wheel != null)
                this.grid.Children.Remove(wheel);

            if (wheelClass != null)
            {
                wheel = (ColourWheels.ColourWheel)Activator.CreateInstance(WheelClass);
                wheel.Name = "wheel";
                wheel.Margin = new Thickness(10.0);
                wheel.MouseDown += wheel_MouseDown;
                wheel.CacheMode = new BitmapCache();
                Canvas.SetZIndex(wheel, -1);
                this.grid.Children.Add(wheel);
            }
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
            if (!double.IsNaN(this.Theta) && !double.IsNaN(this.Rad))
            {
                double cx = this.ActualWidth / 2.0;
                double cy = this.ActualHeight / 2.0;

                double radius = (wheel.ActualOuterRadius - wheel.ActualInnerRadius) * this.Rad + wheel.ActualInnerRadius;

                // Snap to middle of wheel when inside InnerRadius
                if (radius < wheel.ActualInnerRadius + float.Epsilon)
                    radius = 0.0;

                double angle = this.Theta + 180.0f;

                double x = radius * Math.Sin(angle * Math.PI / 180.0);
                double y = radius * Math.Cos(angle * Math.PI / 180.0);

                double mx = cx + x - this.selector.ActualWidth / 2;
                double my = cy + y - this.selector.ActualHeight / 2;

                hsv.hue = (float)Theta;
                hsv.sat = (float)Rad;
                hsv.value = 1.0f;

                this.selector.Margin = new Thickness(mx, my, 0, 0);
                this.selector.Fill = new SolidColorBrush(RGBValue);
                //this.selector.StrokeThickness = this.ThumbSize / 5.0;
            }
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
                // Calculate Theta and Rad from the mouse position
                UpdateSelectorFromPoint(Mouse.GetPosition(this));
            }
        }

        private void UpdateSelectorFromPoint(Point point)
        {
            this.Theta = CalculateTheta(point);
            this.Rad = CalculateR(point);
            //this.RGBValue = wheel.ColourMapping(this.Rad, this.Theta, 1.0);
            UpdateSelector();
        }

        private double CalculateTheta(Point point)
        {
            double cx = this.ActualWidth / 2;
            double cy = this.ActualHeight / 2;

            double dx = point.X - cx;
            double dy = point.Y - cy;

            double angle = Math.Atan2(dx, dy) / Math.PI * 180.0;

            // Theta is offset by 180 degrees, so red appears at the top
            return CircularMath.Mod(angle - 180.0);
        }

        private double CalculateR(Point point)
        {
            double cx = this.ActualWidth / 2;
            double cy = this.ActualHeight / 2;

            double dx = point.X - cx;
            double dy = point.Y - cy;

            double dist = Math.Sqrt(dx * dx + dy * dy);

            return Math.Min(dist, wheel.ActualOuterRadius) / wheel.ActualOuterRadius;
            //return (float)((Math.Min(dist, wheel.ActualOuterRadius) - wheel.ActualInnerRadius) / (wheel.ActualOuterRadius - wheel.ActualInnerRadius));
        }

        #endregion

        #region Animation

        public double AnimationSpeed = 0.5;
        public double AnimationEasePower = 2.0;

        private Storyboard sb = null;

        private static DependencyProperty HSAnimationProperty = DependencyProperty.Register("HSAnimation", typeof(Point), typeof(WheelSelector), new FrameworkPropertyMetadata(OnHSPropertyAnimated));

        private Point HSAnimation
        {
            get { return (Point)base.GetValue(HSAnimationProperty); }
            set { base.SetValue(HSAnimationProperty, value); }
        }

        private static void OnHSPropertyAnimated(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            WheelSelector ctl = (obj as WheelSelector);

            ctl.Theta = (args.NewValue as Point?).Value.X;
            ctl.Rad = (args.NewValue as Point?).Value.Y;

            ctl.InvalidateVisual();
        }

        private void AnimateTo(Point point)
        {
            //TODO: Might be able to shift this stuff into the XAML
            // by using databindings for the From and To fields.
            // This would allow the style to be overridden later on.

            //NOTE: Must use PointAnimation so both hue and saturation get
            // updated at the same time. I tried using two seaprate DoubleAnimations
            // but the second one would not update the property.

            Point from = new Point(this.Theta, this.Rad);
            Point to = new Point(CalculateTheta(point), CalculateR(point));

            // The shortest path actually crosses the 360-0 discontinuity
            if (from.X - to.X > 180.0)
                to.X += 360.0;
            if (from.X - to.X < -180.0)
                to.X -= 360.0;

            Duration duration = new Duration(TimeSpan.FromSeconds(AnimationSpeed));

            sb = new Storyboard();
            sb.Duration = duration;

            var hs_animation = new PointAnimation(from, to, duration);
            hs_animation.EasingFunction = new PowerEase() { Power = AnimationEasePower };
            hs_animation.FillBehavior = FillBehavior.Stop;

            sb.Children.Add(hs_animation);
            Storyboard.SetTarget(hs_animation, this);
            Storyboard.SetTargetProperty(hs_animation, new PropertyPath(WheelSelector.HSAnimationProperty));

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
