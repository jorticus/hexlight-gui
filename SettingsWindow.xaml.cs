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
        
        public SettingsWindow()
        {
            InitializeComponent();

            var controllers = Controllers.ListControllers();
            var activeControllerName = HexLight.Properties.Settings.Default.Controller;

            // Populate the listbox
            cbDrivers.Items.Clear();
            cbDrivers.Items.Add(ControllerItem.NullController);
            int idx = 0;
            foreach (var controller in controllers)
            {
                idx++;
                cbDrivers.Items.Add(new ControllerItem(controller));
                if (controller.FullName == activeControllerName)
                    cbDrivers.SelectedIndex = idx;
            }

            //TODO: Use ValueConverters and Bindings ?
            //TODO: Save the selected value
            //TODO: Provide a way for plugins to provide their own property UI?
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
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
