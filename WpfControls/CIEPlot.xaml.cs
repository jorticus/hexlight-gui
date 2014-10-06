using System;
using System.Collections.Generic;
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
using HexLight.Colour;

namespace HexLight.WpfControls
{
    /// <summary>
    /// Interaction logic for CIEPlot.xaml
    /// </summary>
    public partial class CIEPlot : UserControl
    {
        public CIERGBDefinition cieRgbDefinition = CIERGBDefinition.CIERGB;

        public CIEPlot()
        {
            InitializeComponent();
        }
    }


    public class CIEPlotCanvas : Canvas
    {
        public CIERGBDefinition cieRgbDefinition = CIERGBDefinition.CIERGB;

        WriteableBitmap bitmap;

        public static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            HSVWheel ctl = (obj as HSVWheel);
            ctl.InvalidateVisual();
        }

        #region Rendering

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            //this.Effect = new BlurEffect() { Radius = 5 };
            DrawPlot(dc);

            var brush = Brushes.Transparent;
            var pen = new Pen(Brushes.White, 2.0);

            var pt_red = xyz2pt(cieRgbDefinition.Red);
            var pt_green = xyz2pt(cieRgbDefinition.Green);
            var pt_blue = xyz2pt(cieRgbDefinition.Blue);

            dc.DrawLine(pen, pt_red, pt_green);
            dc.DrawLine(pen, pt_green, pt_blue);
            dc.DrawLine(pen, pt_blue, pt_red);

            dc.DrawEllipse(brush, pen, xyz2pt(cieRgbDefinition.White), 2, 2);
        }

        protected Point xyz2pt(CIEXYYColor xyy)
        {
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            return new Point(width * xyy.x, height * xyy.y);
        }

        protected virtual RGBStruct ColourFunction(double x, double y)
        {
            CIEXYYColor xyy = new CIEXYYColor(x, y, 1.0);

            CIEXYZColour xyz = (CIEXYZColour)xyy;

            RGBColor rgb = xyz.ToRGB(cieRgbDefinition, false);

            if (rgb.OutOfGamut)
                return new RGBStruct(rgb.Rb, rgb.Gb, rgb.Bb, 128); // Values are clamped automatically
            else
                return new RGBStruct(rgb.Rb, rgb.Gb, rgb.Bb, 255);
        }

        protected void DrawPlot(DrawingContext drawingContext)
        {
            int bmp_width = (int)this.ActualWidth;
            int bmp_height = (int)this.ActualHeight;

            if (bmp_width <= 0 || bmp_height <= 0)
                return;

            bitmap = new WriteableBitmap(bmp_width, bmp_height, 96.0, 96.0, PixelFormats.Bgra32, null);

            bitmap.Lock();
            unsafe
            {
                int pBackBuffer = (int)bitmap.BackBuffer;

                for (int y = 0; y < bmp_height; y++)
                {
                    for (int x = 0; x < bmp_width; x++)
                    {
                        int color_data = 0;
                        //double inner_radius = radius * InnerRadius;

                        // Convert xy to CIE xy
                        double cx = (double)x / (double)bmp_width;
                        double cy = 1.0 - (double)y / (double)bmp_height;
                        color_data = ColourFunction(cx, cy).ToARGB32();

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
