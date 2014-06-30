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
    public class TimerException : Exception
    {
        public Timer Timer { get; private set; }
        public TimerException(string message, Exception innerException, Timer sender)
            : base(message, innerException)
        {
            Timer = sender;
        }
    }

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
            try
            {
                Exception ex = e.Exception;

                // If exception occurred in another thread
                if (e.Exception is System.Reflection.TargetInvocationException)
                    ex = e.Exception.InnerException;

                ExceptionDialog.ShowException("Unhandled Exception", e.Exception.InnerException, ExceptionSeverity.Unhandled);

                // Re-enable timer if Ignoring the exception
                if (ex is TimerException && (ex as TimerException).Timer.Enabled == false)
                    (ex as TimerException).Timer.Start();

                e.Handled = true;
            }
            catch (Exception ex)
            {
                // Fail-safe
                Shutdown(1);
                return;
            }
        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /*RenderIcon.RenderToPng(16, 16, "16px.png", 0.65);
            RenderIcon.RenderToPng(32, 32, "32px.png", 0.55);
            RenderIcon.RenderToPng(48, 48, "48px.png", 0.55);
            RenderIcon.RenderToPng(256, 256, "256px.png", 0.45);*/
            //RenderIcon.RenderToPng(32, 32, "hsv.png", 0);

            try
            {
                // Load settings
                switch (Settings.Default.Protocol)
                {
                    case DeviceProtocol.SimpleTCP:
                        controller = new SimpleTcpController(Settings.Default.ServerAddress, Settings.Default.ServerPort);
                        break;

                    case DeviceProtocol.SimpleSerial:
                        controller = new SimpleSerialController(Settings.Default.ComPort, Settings.Default.ComBaud);
                        break;

                    case DeviceProtocol.HexLightSerial:
                        controller = new HexControllerSerial(Settings.Default.ComPort, Settings.Default.ComBaud);
                        break;

                    case DeviceProtocol.HexLightUSB:
                        controller = new HexControllerHID(Settings.Default.DeviceID);
                        break;
                    
                    default:
                        throw new Exception(String.Format("Unknown device protocol {0}", Settings.Default.Protocol));
                }

            }
            catch (Exception ex)
            {
                // Show the settings dialog if an exception occurrs, 
                // to allow the user to re-configure if necessary.
                var fm = new SettingsWindow();
                ExceptionDialog.ShowException("Could not connect to the device. Check application settings and try again", ex, severity: ExceptionSeverity.Warning);
                fm.ShowDialog();
                Shutdown(1);
                return;
            }

            controller.Color = Colors.Black;
            controller.Brightness = 0.0f;
            //controller.FadeTo(ColorTemperature.Hot, 0.2);
            
            viewModel = new ViewModel();

            controlWindow = new ControlWindow(viewModel);
            popupDial = new PopupDial(viewModel);

            #if DEBUG
                controlWindow.Show();
            #endif

            updateTimer = new Timer(UPDATE_INTERVAL);
            updateTimer.Elapsed += updateTimer_Elapsed;
            updateTimer.Enabled = true;

            ///// Tray Icon & Tray Menu /////

            trayIcon = new WinForms.NotifyIcon();
            trayIcon.Text = "HexLight Controller";
            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            trayIcon.Click += trayIcon_Click;
            trayIcon.Visible = true;

            trayIcon.ContextMenu = new WinForms.ContextMenu(new[] {
                new WinForms.MenuItem("Control Panel", trayIcon_ControlPanel_Click),
                new WinForms.MenuItem("Settings", trayIcon_Settings_Click),
                new WinForms.MenuItem("Exit", trayIcon_Exit_Click),
            });
        }

        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
#if DEBUG
            (sender as Timer).Stop();
#endif
            try
            {
                controller.Color = color;
                controller.Brightness = brightness;
            }
            catch (Exception ex)
            {
                (sender as Timer).Stop();
                // Pass the exception to the main thread
                Application.Current.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action<Exception>((exc) => { throw new TimerException("Exception in Timer Thread", exc, sender as Timer); }), ex);
            }
#if DEBUG
            (sender as Timer).Start();
#endif
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (updateTimer != null)
                updateTimer.Enabled = false;

            try
            {
                if (controller != null)
                {
                    controller.Color = Colors.Black;
                    controller.Brightness = 0.0f;
                }
            }
            catch (Exception ex) { } // Don't worry about exceptions while closing down
        }


        #region Tray Menu


        void trayIcon_ControlPanel_Click(object sender, EventArgs e)
        {
            controlWindow.Show();
            if (popupDial.IsVisible)
                popupDial.Hide(); // Prevent dial from appearing
        }

        void trayIcon_Settings_Click(object sender, EventArgs e)
        {
            if (popupDial.IsVisible)
                popupDial.Hide(); // Prevent dial from appearing

            var fm = new SettingsWindow();
            fm.ShowDialog();
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
