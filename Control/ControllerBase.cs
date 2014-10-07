using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLight.Control
{
    //TODO: Capabilities (Linear RGB, Perceptual RGB, XYZ, XYY, etc.)

    public abstract class ControllerSettings
    {

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
