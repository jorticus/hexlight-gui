using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HexLight.Plugin
{
    public static class Plugins
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
        /// Return a list of all detected plugins (Managed .dlls), descended from the specified base_class.
        /// </summary>
        /// <param name="base_class">The base class that the plugins must descend from</param>
        /// <param name="path">Optional file path to search for plugins</param>
        /// <returns>An enumerable of plugins</returns>
        public static IEnumerable<Type> ListPlugins(Type base_class, string path = null)
        {
            var controllers = new List<KeyValuePair<string, Type>>();

            // Find out which ones subclass the specified type
            return from assy in Plugins.ListAssemblies(path)
                   from type in assy.ExportedTypes
                   where type.IsSubclassOf(base_class)
                       && !type.IsAbstract // Ignore the abstract base class
                   select type;
        }

        /// <summary>
        /// Get the external assembly filename for the specified type,
        /// or null if the type is internal to the application
        /// </summary>
        /// <param name="engine">Type to query</param>
        /// <returns>An assembly name (eg. "Assembly.dll"), or null</returns>
        public static string GetAssemblyName(Type engine)
        {
            if (engine.Assembly != Assembly.GetExecutingAssembly())
                return engine.Assembly.ManifestModule.Name;
            return null;
        }
    }
}
