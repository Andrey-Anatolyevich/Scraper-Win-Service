using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ParserCore
{
    /// <summary>
    /// Load plugins from path
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class UniversalPluginLoader<T>
    {
        public static List<T> LoadPlugins(string pluginsLocation)
        {
            if (string.IsNullOrWhiteSpace(pluginsLocation))
                throw new ArgumentException("pluginsLocation");
            if (!Directory.Exists(pluginsLocation))
                throw new Exception(string.Format("Directory [{0}] not exists.", pluginsLocation));

            //Array or file names
            ICollection<Assembly> assemblies = GetAssembliesFromListOfFiles(pluginsLocation);

            // check type of assembly
            Type pluginType = typeof(T);
            ICollection<Type> pluginTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null)
                    continue;

                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception ex)
                {
                    string message = string.Format("Exception while trying to access types (.GetTypes()) from assembly: {0}", assembly.GetName());
                    if (ex is ReflectionTypeLoadException)
                    {
                        Exception[] loaderExceptions = ((ReflectionTypeLoadException)ex).LoaderExceptions;
                        foreach (Exception innerEx in loaderExceptions)
                            message += Environment.NewLine + innerEx.ToString();
                    }
                    throw new Exception(message, ex);
                }

                foreach (Type type in types)
                {
                    // if not interface and not abstract class
                    if (!type.IsInterface && !type.IsAbstract)
                    {
                        // if inherited from target interface
                        if (type.GetInterface(pluginType.FullName) != null)
                        {
                            pluginTypes.Add(type);
                        }
                    }
                }
            }

            // create instance of each type and return collection
            List<T> plugins = new List<T>(pluginTypes.Count);
            foreach (Type type in pluginTypes)
            {
                T plugin = (T)Activator.CreateInstance(type);
                plugins.Add(plugin);
            }

            return plugins;
        }

        /// <summary>
        /// Get list of assemblies from the location
        /// </summary>
        /// <param name="pluginsLocation"></param>
        /// <returns></returns>
        private static ICollection<Assembly> GetAssembliesFromListOfFiles(string pluginsLocation)
        {
            if (string.IsNullOrEmpty(pluginsLocation))
                throw new ArgumentException("pluginsLocation is null or empty");
            if (!Directory.Exists(pluginsLocation))
                throw new DirectoryNotFoundException(string.Format("Directory [{0}] is not found", pluginsLocation));

            string[] dllFileNames = Directory.GetFiles(pluginsLocation, "*.dll");

            // load assemblies where possible
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                try
                {
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }
                catch { }
            }

            return assemblies;
        }
    }
}