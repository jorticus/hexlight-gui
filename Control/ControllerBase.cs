using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HexLight.Control
{
    //TODO: Capabilities (Linear RGB, Perceptual RGB, XYZ, XYY, etc.)

    public abstract class ControllerSettings : INotifyPropertyChanged
    {
        public abstract UserControl GetSettingsPage();

        #region Property-Change Handlers
        protected void ConnChanged(string name)
        {
            PropChanged(name);

            if (ConnectionPropertyChanged != null)
                ConnectionPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        protected void PropChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ConnectionPropertyChanged;
        #endregion
    }

    /// <summary>
    /// Use this attribute to decorate subclasses of RGBController
    /// in order to provide a pretty name (displayed in the settings dialog)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerName : System.Attribute
    {
        public string Name { get; protected set; }

        public ControllerName(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerSettingsType : System.Attribute
    {
        public Type Type { get; protected set; }

        public ControllerSettingsType(Type type)
        {
            this.Type = type;
        }
    }
}
