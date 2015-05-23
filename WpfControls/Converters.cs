using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HexLight
{
    /// <summary>
    /// See http://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    /// <summary>
    /// Convert a boolean to numeric value, specified by TrueValue and FalseValue
    /// </summary>
    public class BooleanToNumericConverter : IValueConverter
    {
        public double TrueValue { get; set; }
        public double FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool cond = (bool)value;
            return cond ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // TODO - Probably not a good idea to compare doubles
            double val = (double)value;

            if (val == TrueValue)
                return true;
            if (val == FalseValue)
                return false;

            return Binding.DoNothing;
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return String.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Cannot convert back
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Turns a linear-scaled slider (0.0 to 1.0) into a logarithmic value,
    /// defined by Minimum and Maximum.
    /// The logarithm is in base 10, so for best results Minimum & Maximum should be a power of 10.
    /// </summary>
    public class LogarithmicConverter : IValueConverter
    {
        public double Maximum
        {
            get { return linMax; }
            set { linMax = value; logMax = Math.Log10(value); }
        }
        public double Minimum
        {
            get { return linMin; }
            set { linMin = value; logMin = Math.Log10(value); }
        }

        // Cached values
        private double linMin;
        private double linMax;
        private double logMin;
        private double logMax;

        public object Convert(object _value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double scale = (logMax - logMin);
            double value = (double)_value;

            return (Math.Log10(value) - logMin) / scale;
        }

        public object ConvertBack(object _value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double scale = (logMax - logMin);
            double value = (double)_value;

            return Math.Pow(10.0, logMin + scale * value);
        }
    }
}
