using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace HexLight.WinAPI
{
    public class Glass
    {
        [StructLayout(LayoutKind.Sequential)]
        protected struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size 
            public int cxRightWidth;     // width of right border that retains its size 
            public int cyTopHeight;      // height of top border that retains its size 
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

        [DllImport("DwmApi.dll")]
        protected static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        public static void ExtendFrameIntoClientArea(Window window, Thickness margins)
        {
            try
            {
                IntPtr windowPtr = new WindowInteropHelper(window).Handle;
                HwndSource windowSrc = HwndSource.FromHwnd(windowPtr);
                windowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(windowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;

                // Set Margins
                MARGINS _margins = new MARGINS();

                // Extend glass frame into client area 
                // Note that the default desktop Dpi is 96dpi. The  margins are 
                // adjusted for the system Dpi.
                _margins.cxLeftWidth = Convert.ToInt32(margins.Left * (DesktopDpiX / 96));
                _margins.cxRightWidth = Convert.ToInt32(margins.Right * (DesktopDpiX / 96));
                _margins.cyTopHeight = Convert.ToInt32(margins.Top * (DesktopDpiX / 96));
                _margins.cyBottomHeight = Convert.ToInt32(margins.Bottom * (DesktopDpiX / 96));

                int hr = DwmExtendFrameIntoClientArea(windowSrc.Handle, ref _margins);
                if (hr != 0)
                    throw new Exception("DwmExtendFrameIntoClientArea failed");
            } 
            catch (Exception ex)
            {
                throw new Exception("Could not extend aero glass", innerException:ex);
            }
                
        }
    }
}
