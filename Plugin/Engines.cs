using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using HexLight.Plugin;

namespace HexLight.Engine
{

    public static class Engines
    {
        public static IEnumerable<Type> ListEngines()
        {
            return Plugins.ListPlugins(typeof(HexEngine));
        }


        //public static string GetEngineName(Type engine)
        //{
        //    var attr = engine.GetCustomAttribute<HexLight.Engine.EngineName>();
        //    if (attr == null)
        //        throw new Exception("[EngineName] attribute not specified for engine class");
        //    string name = attr.Name;

        //    if (engine.Assembly != Assembly.GetExecutingAssembly())
        //        name += " (" + engine.Assembly.ManifestModule.Name + ")";

        //    return name;
        //}

        public static string GetEngineName(Type engine)
        {
            var attr = engine.GetCustomAttribute<HexLight.Engine.EngineName>();
            if (attr == null)
                return engine.Name;
            return attr.Name;
        }

        public static Type GetEngineByGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            var engines = ListEngines();

            return (from engine in engines
                    where engine.GUID == guid
                    select engine).FirstOrDefault();
        }

        public static HexEngine LoadEngine(Type engineType)
        {
            try
            {
                var engine = (HexEngine)Activator.CreateInstance(engineType);
                return engine;
            }
            catch (TargetInvocationException ex) { throw ex.InnerException; }
        }
    }
}
