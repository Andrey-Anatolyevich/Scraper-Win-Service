using System.IO;
using System.Reflection;

namespace ParserCore
{
    internal class Utils
    {
        /// <summary>
        /// Получить путь до папки, в которой загружена текущая сборка.
        /// </summary>
        /// <returns></returns>
        public string GetAssemblyDirectory()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string currentAssemblyDir = Path.GetDirectoryName(currentAssembly.Location);
            return currentAssemblyDir;
        }
    }
}