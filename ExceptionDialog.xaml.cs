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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace HexLight
{
    public enum ExceptionSeverity { Warning, Error, Critical, Unhandled }
    


    public partial class ExceptionDialog : Window
    {
        public enum ModalResult { Ignore, Abort, Ok, Debug }
        public ModalResult modalResult = ModalResult.Ignore;

        public ExceptionDialog()
        {
            InitializeComponent();
        }

        protected static string BuildExceptionDetails(Exception ex)
        {
            string message = ex.ToString();

            if (ex.InnerException != null)
            {
                message += "\n\n" + BuildExceptionDetails(ex.InnerException);
            }

            return message;
        }

        public static bool ShowException(string message, Exception ex = null, ExceptionSeverity severity = ExceptionSeverity.Error)
        {
            var fm = new ExceptionDialog();
            fm.messageLabel.Text = (message != null) ? message : "";

            if (ex != null)
            {
                if (message != null) fm.messageLabel.Text += "\n\nDetail:\n";
                fm.messageLabel.Text += ex.Message;

                fm.detailsLabel.Text = "Exception Details:\n\n" + BuildExceptionDetails(ex);
            }
            else
            {
                fm.detailsGrid.Visibility = Visibility.Hidden;
                fm.btnShowDetails.Visibility = Visibility.Hidden;
            }

            fm.Title = "HexLight Controller";
            switch (severity)
            {
                case ExceptionSeverity.Critical:
                    fm.btnIgnore.Visibility = Visibility.Collapsed;
                    fm.btnDebug.Visibility = Visibility.Collapsed;
                    fm.btnOk.Visibility = Visibility.Collapsed;
                    fm.btnAbort.IsCancel = true;
                    fm.btnAbort.IsDefault = true;
                    fm.Title += " - Critical Error";
                    break;
                case ExceptionSeverity.Error:
                    fm.btnOk.Visibility = Visibility.Collapsed;
                    fm.btnIgnore.IsCancel = true;
                    fm.btnIgnore.IsDefault = true;
                    fm.Title += " - Error";
                    break;
                case ExceptionSeverity.Unhandled:
                    fm.btnOk.Visibility = Visibility.Collapsed;
                    fm.btnIgnore.IsCancel = true;
                    fm.btnIgnore.IsDefault = true;
                    fm.Title += " - Unhandled Exception";
                    break;
                case ExceptionSeverity.Warning:
                    fm.btnDebug.Visibility = Visibility.Collapsed;
                    fm.btnAbort.Visibility = Visibility.Collapsed;
                    fm.btnIgnore.IsCancel = true;
                    fm.btnOk.IsDefault = true;
                    fm.Title += " - Error";
                    break;
            }

            fm.ShowDialog();

            // Shutdown the app if required
            if (severity == ExceptionSeverity.Critical || fm.modalResult == ModalResult.Abort)
            {
                Application.Current.Shutdown(1);
                return true;
            }

            if (fm.modalResult == ModalResult.Debug)
            {
                Debugger.Launch();
                Debugger.Break();

                // Welcome, you have been brought here from the [Debug] button.
                // Where to from here?
                // - Try stepping out to see what caused the exception
                // - Examin the stack traces to see what caused the exception
            }

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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            modalResult = ModalResult.Ok;
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
            //WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
            //WinForms.Screen screen = WinForms.Screen.FromHandle(windowInteropHelper.Handle);

            this.SizeToContent = System.Windows.SizeToContent.Manual;
            this.ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
            detailsScroller.Visibility = System.Windows.Visibility.Visible;
            btnShowDetails.Visibility = System.Windows.Visibility.Collapsed;

            this.Height += 200;
        }
    }
}
