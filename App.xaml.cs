using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using RGB.Control;
using RGB.Util;
using RGB.Util.ColorTypes;
using RGB.Properties;
using System.Windows.Media;

namespace RGB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public float brightness;
        public RGBController controller;
        public RGBColor color;
        public HSVColor hsvColor;

        public ViewModel viewModel;

        private static Timer updateTimer;
        private const double UPDATE_INTERVAL = 1000.0 / 60.0;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /*RenderIcon.RenderToPng(16, 16, "16px.png", 0.65);
            RenderIcon.RenderToPng(32, 32, "32px.png", 0.55);
            RenderIcon.RenderToPng(48, 48, "48px.png", 0.55);
            RenderIcon.RenderToPng(256, 256, "256px.png", 0.45);*/

            // Load settings
            switch (Settings.Default.Protocol.ToLower())
            {
                case "tcp":
                    controller = new NetController(Settings.Default.ServerAddress, Settings.Default.ServerPort);
                    break;

                case "serial":
                    controller = new ArduinoController(Settings.Default.ComPort, Settings.Default.ComBaud);
                    break;

                case "hexrgb":
                    controller = new HexController(Settings.Default.ComPort, Settings.Default.ComBaud);
                    break;

                default:
                    throw new Exception(String.Format("Unknown protocol {0}", Settings.Default.Protocol));

            }

            //controller = new ArduinoController("COM1");
            //controller = new NetController("visc");
            controller.Color = Colors.Black;
            controller.Brightness = 0.0f;
            //controller.FadeTo(ColorTemperature.Hot, 0.2);

            viewModel = new ViewModel();

            updateTimer = new Timer(UPDATE_INTERVAL);
            updateTimer.Elapsed += updateTimer_Elapsed;
            updateTimer.Enabled = true;

            //PopupDial fm = new PopupDial();
            //fm.ShowDialog();
        }

        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            controller.Color = color;
            controller.Brightness = brightness;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            updateTimer.Enabled = false;
            controller.Color = Colors.Black;
            controller.Brightness = 0.0f;
        }

    }
}
