using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLight.Engine
{
    /// <summary>
    /// Use this attribute to decorate subclasses of HexEngine
    /// in order to provide a pretty name (displayed in the settings dialog)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EngineName : System.Attribute
    {
        public string Name { get; protected set; }

        public EngineName(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EngineLabel : System.Attribute
    {
        public string Label { get; protected set; }

        public EngineLabel(string label)
        {
            this.Label = label;
        }
    }
}
