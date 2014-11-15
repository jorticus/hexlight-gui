using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace HexLight.Control
{
    //TODO: Capabilities (Linear RGB, Perceptual RGB, XYZ, XYY, etc.)

    public abstract class ControllerSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Returns the XAML form to use for the settings model
        /// </summary>
        public abstract UserControl GetSettingsPage();

        /// <summary>
        /// Set to the current controller instance when instantiated
        /// through Controllers.LoadController()
        /// </summary>
        [XmlIgnore]
        public RGBController Controller;

        #region Property-Change Handlers

        /// <summary>
        /// Call when a connection-related property is changed,
        /// so the settings handler knows to re-load the entire controller to make 
        /// sure the new connection parameters are valid.
        /// </summary>
        /// <param name="name"></param>
        protected void ConnChanged(string name)
        {
            PropChanged(name);

            if (ConnectionPropertyChanged != null)
                ConnectionPropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Call when a regular property is changed (Used for XAML model binding)
        /// </summary>
        /// <param name="name"></param>
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
    /// Use this attribute to decorate subclasses of RGBController.
    /// specifies what class to use to use as the viewmodel for the settings form
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
