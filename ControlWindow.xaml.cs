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
            switch (this.tabControl.SelectedIndex)
            {
                case 0:
                    application.mode = Mode.Manual;
                    break;

                case 1:
                    application.mode = Mode.Rowdz;
                    break;
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            Glass.ExtendFrameIntoClientArea(this, new Thickness(0, 50, 0, 0));
        }
    }
}