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
using HexLight.Control;
using HexLight.Engine;
using HexLight.Colour;
using HexLight.WpfControls;
using HexLight.WinAPI;

namespace HexLight
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        public App application { get { return (App.Current as App); } }
        public RGBColor Color { get { return application.color; } set { application.color = value; } }


        public void AddEngine(string label, UserControl page = null, HexEngine engine = null)
        {
            var tab = new TabItem();

            var labelTemplate = this.colourTabLabel;
            
            var tabLabel = new Label() { 
                Content = label,
                FontSize = labelTemplate.FontSize,
                FontFamily = labelTemplate.FontFamily,
                Padding = labelTemplate.Padding,
                BorderThickness = labelTemplate.BorderThickness,
                BorderBrush = labelTemplate.BorderBrush
            };

            if (engine == null || page == null)
                tabLabel.Foreground = new SolidColorBrush(Colors.Red);

            tab.Header = tabLabel;

            tab.Content = page;
            tab.DataContext = engine; // Store a reference to the engine so we can activate it

            tabControl.Items.Add(tab);
        }

        public ControlWindow(object viewModel = null)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void HSVSelector_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            #if DEBUG
                Application.Current.Shutdown(0);
            #else
                e.Cancel = true;
                this.Hide();
            #endif
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            //if (!(application.controller is HexController))
            //    throw new Exception("Controller does not support this command");

            //(application.controller as HexController).EnableUsbAudio((sender as CheckBox).IsChecked.Value);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.tabControl.SelectedIndex;

            if (index < 0)
            {
                throw new Exception("Invalid tab index");
            }
            else if (index == 0)
            {
                // Use manual mode to update LEDs
                application.SetEngine(null);
                application.Mode = Mode.Manual;
            }
            else
            {
                // Use an engine to update LEDs
                // Each tab has its own associated engine object (stored in the DataContext)
                var item = (this.tabControl.SelectedItem as TabItem);
                var engine = (item.DataContext as HexEngine);

                application.SetEngine(engine);
                application.Mode = Mode.Engine;
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            Glass.ExtendFrameIntoClientArea(this, new Thickness(1, 50, 1, 1));
        }
    }
}