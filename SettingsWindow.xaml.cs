using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using HexLight.Properties;
using HexLight.Control;
using System.Reflection;
using System.ComponentModel;

namespace HexLight
{
    public enum DeviceProtocol { HexLightUSB, HexLightSerial, SimpleSerial, SimpleTCP };

    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected ControllerSettings scratchSettings = null;

        #region Dynamic Properties

        protected App application { get { return (App.Current as App); } }

        protected Type ActiveControllerType;

        /// <summary>
        /// Returns the current active controller, 
        /// or replaces the current one (disposing if necessary)
        /// </summary>
        protected RGBController ActiveController {
            get
            {
                return application.controller;
            }
            set
            {
                if (application.controller != null)
                    application.controller.Dispose();
                application.controller = value;
            }
        }
        /*protected ControllerSettings ControllerSettings
        {
            get
            {
                return (ActiveController != null) ? ActiveController.Settings : scratchSettings;
            }
            set
            {
                if (ActiveController != null)
                    ActiveController.Settings = value;
                else
                    scratchSettings = value;
            }
        }*/

        #endregion

        protected ControllerSettings controllerSettings;
        protected bool controllerLoaded = false;
        
        /// <summary>
        /// Constructor, initialize the UI
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            var controllers = Controllers.ListControllers();
            var activeControllerName = HexLight.Properties.Settings.Default.Controller;

            // Populate the listbox
            cbDrivers.Items.Clear();

            cbDrivers.Items.Add(ControllerItem.NullController);
            if (activeControllerName == null || activeControllerName == "")
                cbDrivers.SelectedIndex = 0;

            foreach (var controllerType in controllers)
            {
                var item = new ControllerItem(controllerType);
                cbDrivers.Items.Add(item);
                if (controllerType.FullName == activeControllerName)
                    cbDrivers.SelectedItem = item;
            }

            //TODO: Use ValueConverters and Bindings ?
        }
        
        /// <summary>
        /// Attempt to load the given controller type,
        /// and show the appropriate settings page for it
        /// </summary>
        /// <param name="controllerType">The type of the class of the controller</param>
        protected void LoadController(Type controllerType)
        {
            // Try load the controller
            if (controllerType != null)
            {
                ActiveControllerType = controllerType;

                if (ActiveController != null)
                    ActiveController = null;    // Close any existing connections

                try
                {
                    ActiveController = Controllers.LoadController(Settings.Default, controllerType);
                    SetError(null);
                }
                catch (Exception ex)
                {
                    // Couldn't load the controller, show the error message and destroy the active controller
                    SetError(ex);
                    ActiveController = null;
                }
            }
            else
            {
                // Loading a null controller
                ActiveController = null;
                ActiveControllerType = null;
                SetError(null);
            }

            if (ActiveController != null)
            {
                // Controller loaded, use its settings
                LoadSettingsPage(ActiveController.Settings);
            }
            else
            {
                // Create a temporary scratch settings page until we can properly instantiate the controller
                scratchSettings = Controllers.GetControllerSettings(HexLight.Properties.Settings.Default, controllerType);
                LoadSettingsPage(scratchSettings);
            }
        }

        /// <summary>
        /// Load the settings page for the given controller settings object
        /// </summary>
        /// <param name="controllerSettings">The object containing controller-specific configurations</param>
        protected void LoadSettingsPage(ControllerSettings controllerSettings)
        {
            // Save the settings
            //TODO: This doesn't work!
            //if (ActiveControllerType != null)
            //    Settings.Default[ActiveControllerType.FullName] = (ActiveController != null) ? ActiveController.Settings : scratchSettings;

            // Hook a notification for connection properties, which cause a complete controller re-load when changed
            // (So the user can verify that we can still connect for the new values)
            if (controllerSettings != null)
                controllerSettings.ConnectionPropertyChanged += ControllerSettings_ConnectionPropertyChanged;

            // Remove existing property page
            contentGrid.Children.Clear();

            if (controllerSettings != null)
            {
                // Set new propery page
                var page = controllerSettings.GetSettingsPage();
                if (page != null)
                {
                    contentGrid.Children.Add(page);
                    lblNotLoaded.Visibility = Visibility.Hidden;

                    page.DataContext = controllerSettings;
                    return;
                }
            }

            lblNotLoaded.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Called when a connection property is changed in the currently active settings page
        /// (See ControllerBase.ControllerSettings)
        /// </summary>
        void ControllerSettings_ConnectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Re-load the controller if a connection property is changed
            LoadController(ActiveControllerType);
        }

        /// <summary>
        /// Update the UI to show any exceptions that may have occurred while validating inputs
        /// </summary>
        private void SetError(Exception ex)
        {
            if (ex != null)
            {
                lblError.Visibility = Visibility.Visible;
                lblError.Content = String.Format("Error: {0}", ex.Message);
            }
            else
            {
                lblError.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Close and save settings
        /// </summary>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Save controller driver
            string controllerName = (ActiveControllerType != null) ? ActiveControllerType.FullName : null;
            Settings.Default.Controller = controllerName;

            // Save controller-specific settings
            if (controllerName != null)
                Settings.Default[controllerName] = (ActiveController != null) ? ActiveController.Settings : scratchSettings;

            Settings.Default.Save();
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            DialogResult = false;
            Close();
        }

        private void cbDrivers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (cbDrivers.SelectedItem as ControllerItem);
            LoadController((item != null) ? item.Controller : null);
        }

    }

    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue,
                     StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool useValue = (bool)value;
            string targetValue = parameter.ToString();
            if (useValue)
                return Enum.Parse(targetType, targetValue);

            return null;
        }
    }   
}
