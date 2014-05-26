using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using RGB.Util.ColorTypes;

namespace RGB
{
    public class ViewModel : INotifyPropertyChanged
    {
        private RGBColor rgbColor;
        private HSVColor hsvColor;
        private ColorTemperature temperature;
        private float brightness;
        
        private App application { get { return (App.Current as App); } }

        public ViewModel()
        {
            // Set default colour to white, full brightness
            rgbColor = new RGBColor(1.0f, 1.0f, 1.0f);
            hsvColor = new HSVColor(0.0f, 0.0f, 1.0f);
            temperature = ColorTemperature.Neutral;
            brightness = 1.0f;
            ColorChanged();
        }

        #region Model Properties

        public float Red
        {
            get { return rgbColor.r; }
            set
            {
                rgbColor.r = value;

                this.RgbChanged();
                this.ColorChanged();
            }
        }
        public float Green
        {
            get { return rgbColor.g; }
            set
            {
                rgbColor.g = value;

                this.RgbChanged();
                this.ColorChanged();
            }
        }
        public float Blue
        {
            get { return rgbColor.b; }
            set
            {
                rgbColor.b = value;

                this.RgbChanged();
                this.ColorChanged();
            }
        }

        public float Hue
        {
            get { return hsvColor.hue; }
            set
            {
                hsvColor.hue = value;
                hsvColor.value = 1.0f;

                this.HsvChanged();
                this.ColorChanged();
            }
        }

        public float Saturation
        {
            get { return hsvColor.sat; }
            set
            {
                hsvColor.sat = value;
                hsvColor.value = 1.0f;

                this.HsvChanged();
                this.ColorChanged();
            }
        }
        /*public float Value
        {
            get { return hsvColor.value; }
            set
            {
                //if (value != hsvColor.value)
                {
                    this.HsvChanged();
                    this.ColorChanged();
                }
                hsvColor.value = value;
            }
        }*/

        public float Temperature
        {
            get { return temperature.k; }
            set
            {
                temperature.k = value;

                this.TemperatureChanged();
                this.ColorChanged();
            }
        }

        public float Brightness
        {
            get { return brightness; }
            set
            {
                brightness = value;
                this.ColorChanged();
            }
        }


        public Color SystemColor { get { return application.color; } }
        public Color HsvColor { get { return new HSVColor(hsvColor.hue, 1.0f, 1.0f).ToRGB(); } }

        #endregion

        #region Property Change Event Handlers

        public event PropertyChangedEventHandler PropertyChanged;

        private void RgbChanged()
        {
            hsvColor = rgbColor.ToHSV();
            //application.color = rgbColor;
        }

        private void HsvChanged()
        {
            rgbColor = hsvColor.ToRGB();
            //application.color = rgbColor;
        }

        private void TemperatureChanged()
        {
            rgbColor = temperature.ToRGB();
            //this.RgbChanged();
        }

        private void ColorChanged()
        {
            // Apply brightness to the colour.
            // Note that RGB/HSV colour spaces can specify the brightness as well.
            // It is up to the end-user to limit user input, if necessary.
            application.color = rgbColor * brightness;

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Red"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("Green"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("Blue"));

                this.PropertyChanged(this, new PropertyChangedEventArgs("Hue"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("Saturation"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("Value"));

                this.PropertyChanged(this, new PropertyChangedEventArgs("SystemColor"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("HsvColor"));

                this.PropertyChanged(this, new PropertyChangedEventArgs("Brightness"));
            }
        }

        #endregion
    }
}
