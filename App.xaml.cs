using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using HexLight.Control;
using HexLight.Util;
using HexLight.Util.ColorTypes;
using HexLight.Properties;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace HexLight
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

        public PopupDial popupDial;
        public ControlWindow controlWindow;

        private WinForms.NotifyIcon trayIcon;

        private static Timer updateTimer;
        private const double UPDATE_INTERVAL = 1000.0 / 60.0;

        public App() : base()
        {
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // If exception occurred in another thread
            if (e.Exception is System.Reflection.TargetInvocationException)
                ShowExceptionDialog(e.Exception.InnerException);
            else
            {
                ShowExceptionDialog(e.Exception);
                e.Handled = true;
            }
        }

        public void ShowExceptionDialog(Exception ex)
        {
            CriticalError("Unhandled Error", ex);
        }
        public void CriticalError(string message, Exception ex = null)
        {
            string msg = message;
            if (ex != null)
            {
                msg += "\n\nException Details:\n" + ex.Message;
                msg += "\n\n" + ex.StackTrace.Split('\n')[0].Trim();
            }


            MessageBox.Show(msg, "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /*RenderIcon.RenderToPng(16, 16, "16px.png", 0.65);
            RenderIcon.RenderToPng(32, 32, "32px.png", 0.55);
            RenderIcon.RenderToPng(48, 48, "48px.png", 0.55);
            RenderIcon.RenderToPng(256, 256, "256px.png", 0.45);*/

            try
            {
                // Load settings
                switch (Settings.Default.Protocol.ToLower())
                {
                    case "tcp":
                        controller = new NetController(Settings.Default.ServerAddress, Settings.Default.ServerPort);
                        break;

                    case "arduino":
                        controller = new ArduinoController(Settings.Default.ComPort, Settings.Default.ComBaud);
                        break;

                    case "hexlight-serial":
                        controller = new HexControllerSerial(Settings.Default.ComPort, Settings.Default.ComBaud);
                        break;

                    case "hexlight-hid":
                    case "hexlight":
                        controller = new HexControllerHID(Settings.Default.DeviceID);
                        break;

                    default:
                        throw new Exception(String.Format("Unknown protocol {0}", Settings.Default.Protocol));

                }
            }
            catch (Exception ex)
            {
                CriticalError("Could not connect to the device. Is it plugged in?", ex);
                Shutdown(1);
                return;
            }

            //controller = new ArduinoController("COM1");
            //controller = new NetController("visc");
            controller.Color = Colors.Black;
            controller.Brightness = 0.0f;
            //controller.FadeTo(ColorTemperature.Hot, 0.2);
            
            viewModel = new ViewModel();

            controlWindow = new ControlWindow(viewModel);
            popupDial = new PopupDial(viewModel);

            updateTimer = new Timer(UPDATE_INTERVAL);
            updateTimer.Elapsed += updateTimer_Elapsed;
            updateTimer.Enabled = true;

            //PopupDial fm = new PopupDial();
            //fm.ShowDialog();

            ///// Tray Icon & Tray Menu /////

            trayIcon = new WinForms.NotifyIcon();
            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            trayIcon.Click += trayIcon_Click;
            trayIcon.Visible = true;

            trayIcon.ContextMenu = new WinForms.ContextMenu(new[] {
                new WinForms.MenuItem("Control Panel", trayIcon_ControlPanel_Click),
                new WinForms.MenuItem("Exit", trayIcon_Exit_Click),
            });
        }

        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
#if DEBUG
                (sender as Timer).Stop();
#endif

                    controller.Color = color;
                    controller.Brightness = brightness;
#if DEBUG
                (sender as Timer).Start();
#endif
                }
                catch (Exception ex)
                {
                    // Pass the exception to the main thread
                    Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action<Exception>((exc) => { throw exc; }), ex);

                    (sender as Timer).Stop(); // Ensure the timer is stopped
                }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (updateTimer != null)
                updateTimer.Enabled = false;

            if (controller != null)
            {
                controller.Color = Colors.Black;
                controller.Brightness = 0.0f;
            }
        }


        #region Tray Menu


        void trayIcon_ControlPanel_Click(object sender, EventArgs e)
        {
            controlWindow.Show();
            if (popupDial.IsVisible)
                popupDial.Hide(); // Prevent dial from appearing
        }

        void trayIcon_Exit_Click(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            this.Shutdown(0);
        }

        void trayIcon_Click(object sender, EventArgs e)
        {
            if (popupDial.IsVisible)
                popupDial.FadeOut();
            else
                popupDial.FadeIn();
        }

        #endregion
    }
}
