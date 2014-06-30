using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace HexLight
{
    public enum ExceptionSeverity { Error, Critical, Unhandled }
    


    public partial class ExceptionDialog : Window
    {
        public enum ModalResult { Ignore, Abort, Debug }
        public ModalResult modalResult = ModalResult.Ignore;

        public ExceptionDialog()
        {
            InitializeComponent();
        }

        public static bool ShowException(string message, Exception ex = null, ExceptionSeverity severity = ExceptionSeverity.Error)
        {
            var fm = new ExceptionDialog();
            fm.messageLabel.Text = message;

            if (ex != null)
            {
                fm.messageLabel.Text += "\n\n" + ex.Message;

                fm.detailsLabel.Text = "Exception Details:\n\n" + ex.ToString();
            }

            fm.Title = "HexLight Controller";
            switch (severity)
            {
                case ExceptionSeverity.Critical:
                    fm.Title += " - Critical Error";
                    break;
                case ExceptionSeverity.Error:
                    fm.Title += " - Error";
                    break;
                case ExceptionSeverity.Unhandled:
                    fm.Title += " - Unhandled Exception";
                    break;
            }

            // Hide Ignore/Debug buttons if error is critical, since we cannot recover from it.
            if (severity == ExceptionSeverity.Critical)
            {
                fm.btnIgnore.Visibility = Visibility.Hidden;
                fm.btnDebug.Visibility = Visibility.Hidden;
            }

            fm.ShowDialog();

            // Shutdown the app if required
            if (severity == ExceptionSeverity.Critical || fm.modalResult == ModalResult.Abort)
            {
                Application.Current.Shutdown(1);
                return true;
            }

            if (fm.modalResult == ModalResult.Debug)
                Debugger.Launch();

            return (fm.modalResult == ModalResult.Ignore);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            modalResult = ModalResult.Abort;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            modalResult = ModalResult.Ignore;
            Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            modalResult = ModalResult.Debug;
            Close();
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MinWidth = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = System.Windows.SizeToContent.Height;
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            detailsScroller.Visibility = System.Windows.Visibility.Visible;
            btnShowDetails.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
