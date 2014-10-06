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

namespace HexLight.WpfControls
{
    /// <summary>
    /// Interaction logic for CIESelector.xaml
    /// </summary>
    public partial class CIESelector : UserControl
    {
        public CIESelector()
        {
            InitializeComponent();
        }

        
        #region Dependency Properties

        public static DependencyProperty ThumbSizeProperty = DependencyProperty.Register("ThumbSize", typeof(double), typeof(CIESelector), new FrameworkPropertyMetadata(0.0, OnThumbSizeChanged));
        public static DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(CIESelector), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));
        public static DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(CIESelector), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

        public static void OnThumbSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if the thumb size changes
            CIESelector ctl = (obj as CIESelector);

            ctl.UpdateThumbSize();
            ctl.InvalidateMeasure();    // Updating the thumbsize changes the dial radius
            ctl.UpdateSelector();       // Also need to re-calculate the thumb position
            ctl.InvalidateVisual();     // And then re-paint everything
        }

        public static void OnColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if a visual-related property changes
            CIESelector ctl = (obj as CIESelector);
            ctl.InvalidateVisual();

            // Force an update of the property binding, because animations don't update the property itself.
            //ctl.SetValue(args.Property, args.NewValue);
            if (args.Property == CIESelector.XProperty)
                ctl.X = (double)args.NewValue;

            if (args.Property == CIESelector.YProperty)
                ctl.Y = (double)args.NewValue;
        }

        #endregion

        #region Designer Properties

        [Description("Radius of the thumb selector"), Category("Appearance")]
        public double ThumbSize
        {
            get { return (double)base.GetValue(ThumbSizeProperty); }
            set { base.SetValue(ThumbSizeProperty, value); UpdateThumbSize(); }
        }

        [Description("CIE X"), Category("Common")]
        public double X
        {
            get { return (double)base.GetValue(XProperty); }
            set { base.SetValue(XProperty, value); }
        }
        [Description("CIE Y"), Category("Common")]
        public double Y
        {
            get { return (double)base.GetValue(YProperty); }
            set { base.SetValue(YProperty, value); }
        }

        #endregion

        private bool isDragging = false;

        #region Rendering

        protected override void OnRender(DrawingContext drawingContext)
        {
            this.UpdateSelector();
            base.OnRender(drawingContext);
        }

        private void UpdateThumbSize()
        {
            //selector.Width = ThumbSize;
            //selector.Height = ThumbSize;
            //wheel.Margin = new Thickness(ThumbSize / 2);
            //wheel.Margin = new Thickness(0);
        }

        private void UpdateSelector()
        {
            double x = this.X;
            double y = this.Y;

            if (!double.IsNaN(x) && !double.IsNaN(y))
            {
                CIEXYZColour xyz = (CIEXYZColour)new CIEXYYColor(x, y, 0.5);

                this.selector.Margin = new Thickness(
                    cieplot.Margin.Left + x * cieplot.ActualWidth - this.selector.ActualWidth / 2,
                    cieplot.Margin.Top + (1.0 - y) * cieplot.ActualHeight - this.selector.ActualHeight / 2,
                    0, 0);

                this.selector.Fill = new SolidColorBrush(xyz.ToRGB(cieplot.cieRgbDefinition, limitGamut: false));
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
                // Calculate Hue and Saturation from the mouse position
                UpdateSelectorFromPoint(Mouse.GetPosition(cieplot));
            }
        }

        private void UpdateSelectorFromPoint(Point point)
        {
            this.X = Math.Max(Math.Min(point.X / cieplot.ActualWidth, 1.0), 0.0);
            this.Y = Math.Max(Math.Min(1.0 - (point.Y / cieplot.ActualHeight), 1.0), 0.0);
            //this.Hue = CalculateHue(point);
            //this.Saturation = CalculateSaturation(point);
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

        #endregion

        #region Animation

        public double AnimationSpeed = 0.5;
        public double AnimationEasePower = 2.0;

        private Storyboard sb = null;

        private static DependencyProperty HSAnimationProperty = DependencyProperty.Register("HSAnimation", typeof(Point), typeof(CIESelector), new FrameworkPropertyMetadata(OnHSPropertyAnimated));

        private Point HSAnimation
        {
            get { return (Point)base.GetValue(HSAnimationProperty); }
            set { base.SetValue(HSAnimationProperty, value); }
        }

        private static void OnHSPropertyAnimated(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            CIESelector ctl = (obj as CIESelector);

            //ctl.Value.x = (args.NewValue as Point?).Value.X;
            //ctl.Value.y = (args.NewValue as Point?).Value.Y;

            ctl.InvalidateVisual();
        }

        private void AnimateTo(Point point)
        {
            /*
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
            sb.Begin();*/
        }

        private void StopAnimation()
        {
            if (sb != null)
                sb.Stop();
        }

        #endregion
    }
}
