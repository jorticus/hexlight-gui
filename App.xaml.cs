using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Media;
using System.Configuration.Provider;
using WinForms = System.Windows.Forms;
using System.Reflection;

using HexLight.Control;
using HexLight.Engine;
using HexLight.Colour;
using HexLight.Properties;
using System.Windows.Controls;
using HexLight.Plugin;

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

    public enum Mode { Manual, Engine };

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public float brightness;
        public RGBColor color;
        public HSVColor hsvColor;

        public RGBController controller;

        public HexEngine currentEngine;
        public List<HexEngine> engines;
        public IEnumerable<Type> controllers;

        public ViewModel viewModel;

        public PopupDial popupDial;
        public ControlWindow controlWindow;

        private WinForms.NotifyIcon trayIcon;

        private static Timer updateTimer;
        private static Timer connectTimer;
        private const double UPDATE_INTERVAL = 1000.0 / 1000.0;
        private const double CONNECT_INTERVAL = 250.0;

        /// <summary>
        /// The current LED update mode
        /// Mode.Manual:
        ///     LEDs are updated through the ViewModel
        /// Mode.Engine:
        ///     LEDs are updated through the current engine
        /// </summary>
        public Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                viewModel.ManualControl = (value == Mode.Manual);
            }
        }
        private Mode _mode;

        /// <summary>
        /// Set the current engine to use when in Engine mode
        /// Set to null to disable the engine.
        /// Engines are automatically disabled/enabled when switching
        /// </summary>
        /// <param name="engine"></param>
        public void SetEngine(HexEngine engine)
        {
            // Switch engines
            if (engine != currentEngine)
            {
                if (currentEngine != null)
                    currentEngine.Disable();

                if (engine != null)
                    engine.Enable();
            }

            currentEngine = engine;
        }

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
                if (e.Exception is System.Reflection.TargetInvocationException && e.Exception.InnerException != null)
                    ex = e.Exception.InnerException;

                if (ex is TimerException)
                    ex = ex.InnerException;

                ExceptionDialog.ShowException(null, ex, ExceptionSeverity.Unhandled);

                // Re-enable timer if Ignoring the exception
                if (ex is TimerException && (ex as TimerException).Timer.Enabled == false)
                    (ex as TimerException).Timer.Start();

                e.Handled = true;
            }
            catch (Exception)
            {
                // Fail-safe
                Shutdown(1);
                return;
            }
        }

        #region Plugin Loading

        /// <summary>
        /// Load the controller with the current settings,
        /// and attempt to connect
        /// </summary>
        private void LoadController()
        {
            var settings = HexLight.Properties.Settings.Default;

            // Get the class of the controller to use
            var controllerName = settings.Controller;
            if (controllerName == null || controllerName == "")
                throw new ControllerConnectionException("No controller currently configured");

            var controllerType = Controllers.GetControllerByName(controllerName);
            if (controllerType == null)
                throw new ControllerConnectionException(String.Format("No controller found by name '{0}'", controllerName));

            // Instantiate the controller and load any custom settings for it
            controller = Controllers.LoadController(settings, controllerType);

            // Attempt to connect (to validate the current settings)
            controller.Connect();
        }

        /// <summary>
        /// Attempt to load the controller with the current settings,
        /// showing the settings dialog if can't load
        /// </summary>
        private void TryLoadController()
        {
            try
            {
                LoadController();
            }
            catch (ControllerConnectionException cex)
            {
                Exception ex = cex;
                // Show the settings dialog if an exception occurrs, 
                // to allow the user to re-configure if necessary.
                while (true)
                {
                    var fm = new SettingsWindow();
                    var result = ExceptionDialog.ShowException("Could not connect to the device. Check application settings and try again", ex, ExceptionSeverity.Warning);

                    // Ignore
                    if (result == ExceptionDialog.ModalResult.Ignore)
                        break; // Just start the application

                    bool? _ok = fm.ShowDialog();
                    bool ok = (_ok.HasValue && _ok.Value);

                    // Cancel, exit application
                    if (!ok)
                    {
                        Shutdown(1);
                        return;
                    }

                    // If settings have changed, we'll need to re-load the controller
                    try
                    {
                        LoadController();
                        break; // Settings are now valid, don't loop
                    }
                    catch (Exception _ex)
                    {
                        ex = _ex; // For next iteration of the loop
                    }
                }
            }
        }

        /// <summary>
        /// Try and load all engines present
        /// </summary>
        private void LoadEngines(ControlWindow controlWindow)
        {
            var engineTypes = Engines.ListEngines();

            // Load engines & populate tabs
            foreach (var engineType in engineTypes)
            {
                string name = Engines.GetEngineName(engineType);

                // Try to load the engine
                HexEngine engine = null;
                UserControl page = null;
                try
                {
                    engine = Engines.LoadEngine(engineType);
                    page = engine.GetControlPage();
                }
                catch (NotImplementedException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    string assyName = Plugins.GetAssemblyName(engineType);
                    if (assyName != null)
                        name += " (" + assyName + ")";

                    ExceptionDialog.ShowException(String.Format("Could not load {0}", name), ex, ExceptionSeverity.Error);
                    continue; // Ignore was pressed
                }

                // Add tab & page to the control window
                controlWindow.AddEngine(name, page, engine);
            }
        }

        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            TryLoadController();

            // Attach event handlers
            controller.ConnectNotification += OnControllerConnected;
            controller.DisconnectNotification += OnControllerDisconnected;

            viewModel = new ViewModel();

            controlWindow = new ControlWindow(viewModel);
            popupDial = new PopupDial(viewModel);

            LoadEngines(controlWindow);

            #if DEBUG
                controlWindow.Show();
            #endif

            updateTimer = new Timer(UPDATE_INTERVAL);
            updateTimer.Elapsed += updateTimer_Elapsed;
            updateTimer.Enabled = controller.Connected;

            connectTimer = new Timer(CONNECT_INTERVAL);
            connectTimer.Elapsed += connectTimer_Elapsed;
            connectTimer.Enabled = !controller.Connected;

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

        /// <summary>
        /// Update the controller
        /// </summary>
        private void Update()
        {
            switch (Mode)
            {
                case Mode.Manual:
                    
                    break;

                case Mode.Engine:
                    if (currentEngine != null)
                    {
                        viewModel.RGB = currentEngine.Update(UPDATE_INTERVAL);
                    }
                    break;
            }

            controller.Color = color;
            controller.Brightness = brightness;
        }

        #region Timers

        private void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
#if DEBUG
            (sender as Timer).Stop();
#endif
            try
            {
                if (controller != null && controller.Connected)
                {
                    Update();
                }
            }
            catch (ControllerConnectionException)
            {
                // The device has disconnected, ignore the exception and try to re-connect
                connectTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                (sender as Timer).Stop();
                // Pass the exception to the main thread
                Application.Current.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action<Exception>((exc) => { throw new TimerException("Exception in Timer Thread", exc, sender as Timer); }), ex);
                return; // Don't re-enable the timer
            }
#if DEBUG
            (sender as Timer).Start();
#endif
        }

        void connectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
#if DEBUG
            (sender as Timer).Stop();
#endif
            if (controller != null)
            {
                try
                {
                    if (!controller.Connected)
                        controller.Connect();
                    (sender as Timer).Stop(); // Safety
                    return;
                } 
                catch (ControllerConnectionException)
                {
                    // Ignore if we can't connect
                }
            }
#if DEBUG
            (sender as Timer).Start();
#endif
        }

        #endregion

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
            catch (Exception) { } // Don't worry about exceptions while closing down

            Shutdown();
        }

        #region Connection Management

        protected void OnControllerConnected(object sender, EventArgs e)
        {
            if (connectTimer != null && updateTimer != null)
            {
                connectTimer.Enabled = false;
                trayIcon.ShowBalloonTip(2000, "Connected", "RGB controller connected", WinForms.ToolTipIcon.Info);
                updateTimer.Enabled = true;
            }
        }

        protected void OnControllerDisconnected(object sender, EventArgs e)
        {
            if (connectTimer != null && updateTimer != null)
            {
                updateTimer.Enabled = false;
                trayIcon.ShowBalloonTip(2000, "Disconnected", "RGB controller disconnected", WinForms.ToolTipIcon.Error);
                // Connect timer may be enabled in the update timer
            }
        }

        #endregion

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
