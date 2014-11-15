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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HexLight.Util;

namespace HexLight.WpfControls
{
    /// <summary>
    /// Interaction logic for RadialSlider.xaml
    /// </summary>
    public partial class RadialSlider : UserControl
    {
        #region Dependency Properties

        public static DependencyProperty ThumbSizeProperty = DependencyProperty.Register("ThumbSize", typeof(Point), typeof(RadialSlider), new FrameworkPropertyMetadata(new Point(25, 15), FrameworkPropertyMetadataOptions.AffectsRender, OnThumbPropChanged));

        public static DependencyProperty ArcRadiusProperty = DependencyProperty.Register("ArcRadius", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsRender, OnArcPropChanged));
        public static DependencyProperty ArcStartAngleProperty = DependencyProperty.Register("ArcStartAngle", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnArcPropChanged));
        public static DependencyProperty ArcStopAngleProperty = DependencyProperty.Register("ArcStopAngle", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(360.0, FrameworkPropertyMetadataOptions.AffectsRender, OnArcPropChanged));
        public static DependencyProperty ArcOffsetProperty = DependencyProperty.Register("ArcOffset", typeof(Point), typeof(RadialSlider), new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnArcPropChanged));
        
        public static DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(0.0, OnValueChanged));
        public static DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(1.0, OnValueChanged));
        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static DependencyProperty ArcBrushProperty = DependencyProperty.Register("ArcBrush", typeof(Brush), typeof(RadialSlider), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty ArcStrokeThicknessProperty = DependencyProperty.Register("ArcStrokeThickness", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty ThumbOutlineProperty = DependencyProperty.Register("ThumbOutline", typeof(Brush), typeof(RadialSlider), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty ThumbStrokeThicknessProperty = DependencyProperty.Register("ThumbStrokeThickness", typeof(double), typeof(RadialSlider), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static void OnThumbPropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if one of the display metrics change
            RadialSlider ctl = (obj as RadialSlider);
            //ctl.UpdateThumb();
        }

        public static void OnArcPropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Force a re-render of the object if one of the display metrics change
            RadialSlider ctl = (obj as RadialSlider);
            //ctl.UpdateArc();
        }

        public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RadialSlider ctl = (obj as RadialSlider);
            ctl.InvalidateVisual();

            // Update the two-way bound value
            if (args.Property == RadialSlider.ValueProperty)
                ctl.Value = (double)args.NewValue;
        }

        #endregion

        #region Designer Properties

        [Description("Size of the thumb selector"), Category("Appearance")]
        public Point ThumbSize
        {
            get { return (Point)base.GetValue(ThumbSizeProperty); }
            set { base.SetValue(ThumbSizeProperty, value);  }
        }

        [Description("Radius of the arc that the slider follows"), Category("Appearance")]
        public double ArcRadius
        {
            get { return (double)base.GetValue(ArcRadiusProperty); }
            set { base.SetValue(ArcRadiusProperty, value); }
        }

        [Description("Start angle of the arc that the slider follows"), Category("Appearance")]
        public double ArcStartAngle
        {
            get { return (double)base.GetValue(ArcStartAngleProperty); }
            set { base.SetValue(ArcStartAngleProperty, value); }
        }

        [Description("Stop angle of the arc that the slider follows"), Category("Appearance")]
        public double ArcStopAngle
        {
            get { return (double)base.GetValue(ArcStopAngleProperty); }
            set { base.SetValue(ArcStopAngleProperty, value); }
        }

        [Description("Offset of the arc center. (0,0)=center of control"), Category("Appearance")]
        public Point ArcOffset
        {
            get { return (Point)base.GetValue(ArcOffsetProperty); }
            set { base.SetValue(ArcOffsetProperty, value); }
        }


        [Description("Minimum Value"), Category("Common")]
        public double Minimum
        {
            get { return (double)base.GetValue(MinimumProperty); }
            set { base.SetValue(MinimumProperty, value); }
        }

        [Description("Maximum Value"), Category("Common")]
        public double Maximum
        {
            get { return (double)base.GetValue(MaximumProperty); }
            set { base.SetValue(MaximumProperty, value); }
        }

        [Description("Value"), Category("Common")]
        public double Value
        {
            get { return (double)base.GetValue(ValueProperty); }
            set { base.SetValue(ValueProperty, value); }
        }

        [Description("Arc Stroke Brush"), Category("Brushes")]
        public Brush ArcBrush
        {
            get { return (Brush)base.GetValue(ArcBrushProperty); }
            set { base.SetValue(ArcBrushProperty, value); }
        }

        [Description("Arc Stroke Thickness"), Category("Appearance")]
        public double ArcStrokeThickness
        {
            get { return (double)base.GetValue(ArcStrokeThicknessProperty); }
            set { base.SetValue(ArcStrokeThicknessProperty, value); }
        }

        [Description("Thumb Stroke Brush"), Category("Brushes")]
        public Brush ThumbOutline
        {
            get { return (Brush)base.GetValue(ThumbOutlineProperty); }
            set { base.SetValue(ThumbOutlineProperty, value); }
        }

        [Description("Thumb Stroke Thickness"), Category("Appearance")]
        public double ThumbStrokeThickness
        {
            get { return (double)base.GetValue(ThumbStrokeThicknessProperty); }
            set { base.SetValue(ThumbStrokeThicknessProperty, value); }
        }

        #endregion

        public RadialSlider()
        {
            InitializeComponent();
        }


        #region Calculations

        private Point CalculateCenter(Point offset)
        {
            return new Point(
                this.ActualWidth / 2.0 - offset.X,
                this.ActualHeight / 2.0 - offset.Y
            );
        }

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext drawingContext)
        {
            this.UpdateSelector();
            this.UpdateArc();
            this.UpdateThumb();
            base.OnRender(drawingContext);
        }

        private void UpdateThumb()
        {
            selector.Width = ThumbSize.X;
            selector.Height = ThumbSize.Y;
        }

        private void UpdateArc()
        {
            Point center = CalculateCenter(this.ArcOffset);
            double radius = this.ArcRadius + (this.selector.ActualWidth / 2);
            arcPathFigure.StartPoint = CircularMath.PointFromAngle(this.ArcStartAngle, radius, center);
            arcPathSegment.Point = CircularMath.PointFromAngle(this.ArcStopAngle, radius, center);
            arcPathSegment.Size = new Size(radius, radius);
            arcPathSegment.IsLargeArc = (Math.Abs(this.ArcStopAngle - this.ArcStartAngle) > 180.0);
            arcPathSegment.SweepDirection = SweepDirection.Clockwise;
        }

        private void UpdateSelector()
        {
            double normValue = ((this.Value - this.Minimum) / (this.Maximum - this.Minimum));
            if (normValue < 0.0) normValue = 0.0;
            if (normValue > 1.0) normValue = 1.0;

            double angle = normValue * (this.ArcStopAngle - this.ArcStartAngle) + this.ArcStartAngle;
            double radius = this.ArcRadius +(this.selector.ActualWidth / 2);

            if (!double.IsNaN(angle))
            {
                Point valuePoint = CircularMath.PointFromAngle(angle, radius, CalculateCenter(this.ArcOffset));

                this.selector.Margin = new Thickness(valuePoint.X - this.selector.ActualWidth / 2, valuePoint.Y - this.selector.ActualHeight / 2, 0, 0);
                (this.selector.RenderTransform as RotateTransform).Angle = angle + 90.0;
            }
        }

        #endregion

        #region Dragging

        private bool isDragging = false;

        private void selector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            //StopAnimation();
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
                UpdateSelectorFromPoint(Mouse.GetPosition(this));
            }
        }



        private void UpdateSelectorFromPoint(Point point)
        {
            // Calculate angle from point (0..360)
            double angle = CircularMath.AngleFromPoint(point, CalculateCenter(this.ArcOffset));

            // Convert to a normalized value between 0.0 and 1.0
            double normAngle = CircularMath.NormMap(this.ArcStartAngle, this.ArcStopAngle, angle);

            // And update the value
            if (!double.IsNaN(normAngle))
            {
                double value = normAngle * (this.Maximum - this.Minimum) + this.Minimum;
                this.Value = value;

                UpdateSelector();
            }
        }
        #endregion
    }
}
