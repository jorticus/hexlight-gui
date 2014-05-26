using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RGB.WpfControls;

namespace RGB
{
    public class RenderIcon
    {
        public static void RenderToPng(int w, int h, string filename, double thumb_size = 0.5)
        {
            var ctl = new HSVSelector();

            ctl.ThumbSize = w * thumb_size;
            ctl.Saturation = 0.0;
            
            ctl.Measure(new Size(w, h));
            ctl.Arrange(new Rect(new Size(w, h)));
            ctl.UpdateLayout();

            RenderTargetBitmap bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(ctl);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bitmap));
            using (Stream s = File.Create(filename))
            {
                png.Save(s);
            }
        }
    }
}
