using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using HexLight.Control;

namespace HexLight.Control
{

    public static class Controllers
    {
        /// <summary>
        /// Returns a list of all .NET assemblies in the current executing directory
        /// </summary>
        private static IEnumerable<Assembly> ListAssemblies(string path = null)
        {
            if (path == null)
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            List<Assembly> assemblies = new List<Assembly>();
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(dll));
                }
                catch (FileLoadException) { }           // Assembly has already been loaded
                catch (BadImageFormatException) { }     // Not a .NET assembly
            }

            return assemblies;
        }

        /// <summary>
        /// Return a list of all detected driver plugins
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> ListControllers()
        {
            var controllers = new List<KeyValuePair<string, Type>>();

            // Find out which ones subclass RGBController
            return from assy in ListAssemblies()
                    from type in assy.ExportedTypes
                    where type.IsSubclassOf(typeof(RGBController))
                        && !type.IsAbstract
                    select type;
        }

        /// <summary>
        /// Get a friendly human-readable name for the specified controller type
        /// </summary>
        public static string GetControllerName(Type controller)
        {
            if (controller != null)
            {
                var attr = controller.GetCustomAttribute<HexLight.Control.ControllerName>();
                if (attr == null)
                    throw new Exception("[ControllerName] attribute not specified for controller class");
                string name = attr.Name;

                if (controller.Assembly != Assembly.GetExecutingAssembly())
                    name += " (" + controller.Assembly.ManifestModule.Name + ")";

                return name;
            }
            return "Null Controller";
        }

        /// <summary>
        /// Get the appropriate type for the settings class
        /// </summary>
        public static Type GetControllerSettingsType(Type controller)
        {
            if (controller != null)
            {
                var attr = controller.GetCustomAttribute<HexLight.Control.ControllerSettingsType>();
                if (attr == null)
                    throw new Exception("[ControllerSettingsType] attribute not specified for controller class");
                return attr.Type;

            }
            return null;
        }

        /// <summary>
        /// Get controller type by GUID (type.guid)
        /// </summary>
        public static Type GetControllerByGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            var controllers = ListControllers();

            return (from controller in controllers
                    where controller.GUID == guid
                    select controller).FirstOrDefault();
        }

        /// <summary>
        /// Get controller type by full class name (type.FullName)
        ///   eg. "HexLight.Control.RGBController"
        /// </summary>
        public static Type GetControllerByName(string name)
        {
            // eg. 'HexLight.Control.RGBController'

            if (name == null || name == "")
                return null;

            var controllers = ListControllers();

            return (from controller in controllers
                    where controller.FullName == name
                    select controller).FirstOrDefault();
        }

        public static ControllerSettings GetControllerSettings(ApplicationSettingsBase settings, Type controllerType)
        {
            if (controllerType == null)
                return null;

            string propKey = controllerType.FullName;
            Type settingsType = Controllers.GetControllerSettingsType(controllerType);

            if (settingsType == null)
                return null;

            // Add entry for custom settings property (required for it to be accessable)
            var prop = new SettingsProperty(propKey);
            prop.DefaultValue = null;
            prop.IsReadOnly = false;
            prop.PropertyType = settingsType;  // Must match the actual type being used for it to serialize properly
            prop.Provider = settings.Providers["LocalFileSettingsProvider"];
            prop.Attributes.Add(typeof(System.Configuration.UserScopedSettingAttribute), new System.Configuration.UserScopedSettingAttribute());
            prop.SerializeAs = SettingsSerializeAs.Xml;
            prop.ThrowOnErrorSerializing = true;
            prop.ThrowOnErrorDeserializing = true;
            try
            {
                settings.Properties.Add(prop);
                settings.Reload();
            }
            catch (System.ArgumentException) { } // Ignore if it's already been added

            // Load the settings for the specific controller
            var conf = settings[propKey];

            if (conf == null)
            {
                // Set defaults
                conf = (ControllerSettings)Activator.CreateInstance(settingsType);
                settings[propKey] = conf;
                settings.Save();
            }

            return (ControllerSettings)conf;
        }

        /// <summary>
        /// Load controller-specific settings and instantiate the controller
        /// </summary>
        /// <param name="settings">The application settings that stores the controller-specific config</param>
        /// <param name="controllerType">The type of the class of controller to instantiate</param>
        /// <returns>The instantiated controller</returns>
        public static RGBController LoadController(ApplicationSettingsBase settings, Type controllerType)
        {
            if (controllerType != null)
            {
                var controllerSettings = GetControllerSettings(settings, controllerType);

                // Try and instantiate the controller with the current config
                try
                {
                    var controller = (RGBController)Activator.CreateInstance(controllerType,
                        new object[] { controllerSettings }
                    );
                    controllerSettings.Controller = controller; // Back-reference
                    return controller;
                }
                catch (TargetInvocationException ex) { throw ex.InnerException; }

            }
            return null;
        }
    }

    public class ControllerItem
    {
        public Type Controller { get; protected set; }
        public static ControllerItem NullController = new ControllerItem(null);

        public ControllerItem() { Controller = null; }

        public ControllerItem(Type controller)
        {
            this.Controller = controller;
        }

        public override string ToString()
        {
            return Controllers.GetControllerName(Controller);
        }
    }
}
