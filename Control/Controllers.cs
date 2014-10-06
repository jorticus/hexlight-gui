using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    }
}
