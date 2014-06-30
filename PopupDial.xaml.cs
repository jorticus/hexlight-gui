using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using Hardcodet.Wpf.TaskbarNotification;
using HexLight.WinAPI;

namespace HexLight
{
    /// <summary>
    /// Interaction logic for PopupDial.xaml
    /// </summary>
    public partial class PopupDial : Window
    {
        // NOTE: HSV selector and brightness slider values are bound to application.viewModel

        public App application { get { return (App.Current as App); } }

        // Maybe I should implement form fade in/fade out in XAML?
        private Duration fadeInDuration = new Duration(TimeSpan.FromSeconds(0.1));
        private Duration fadeOutDuration = new Duration(TimeSpan.FromSeconds(0.3));
        private EasingFunctionBase fadeEasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

        public PopupDial(object viewModel = null)
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
            this.DataContext = viewModel;
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Allow dragging of the form
            /*if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                DragMove();*/
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //trayIcon.Site.
            //this.Hide();
            this.FadeOut();

            //TODO: Fix hide/show bug when clicking system tray
            //var pt = Mouse.GetPosition(null);
            //if (WinAPI.GetTrayRectangle().Contains(new System.Drawing.Point((int)pt.X, (int)pt.Y)));
            //    ignoreClick = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            var rect = WinAPI.WinAPI.GetTrayRectangle();
            this.Top = rect.Top - this.ActualHeight;
            this.Left = rect.Right - this.ActualWidth;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //trayIcon.Visible = false;
        }

        public void FadeIn()
        {
            this.Opacity = 0.0;
            this.Show();
            this.Activate();

            var anim = new DoubleAnimation(0.0, 1.0, fadeInDuration, FillBehavior.HoldEnd);
            anim.EasingFunction = fadeEasingFunction;

            this.BeginAnimation(PopupDial.OpacityProperty, anim);
        }

        public void FadeOut()
        {
            var anim = new DoubleAnimation(1.0, 0.0, fadeOutDuration, FillBehavior.HoldEnd);
            anim.EasingFunction = fadeEasingFunction;

            anim.Completed += (o, e) =>
            {
                this.Hide();
            };

            this.BeginAnimation(PopupDial.OpacityProperty, anim);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
