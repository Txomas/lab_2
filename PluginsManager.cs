using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using IArchivatorLib;

namespace lab_2
{
    public static class PluginsManager
    {
        private readonly static string pluginPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

        public static List<IArchivator> GetPlugins()
        {
            List<IArchivator> plugins = new List<IArchivator>();

            DirectoryInfo pluginDirectory = new DirectoryInfo(pluginPath);
            if (!pluginDirectory.Exists)
                pluginDirectory.Create();

            //берем из директории все файлы с расширением .dll      
            var pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
            foreach (var file in pluginFiles)
            {
                //загружаем сборку
                Assembly asm = Assembly.LoadFrom(file);
                //ищем типы, имплементирующие наш интерфейс IPlugin,
                //чтобы не захватить лишнего
                var types = asm.GetTypes().
                                Where(t => t.GetInterfaces().
                                Where(i => i.FullName == typeof(IArchivator).FullName).Any());
                
                //заполняем экземплярами полученных типов коллекцию плагинов
                foreach (var type in types)
                {
                    var plugin = asm.CreateInstance(type.FullName) as IArchivator;
                    plugins.Add(plugin);
                }
            }
            return plugins;
        }

        public static void LoadPlugin(string PluginFileName)
        {
            FileInfo PluginFile = new FileInfo(PluginFileName);
            PluginFile.MoveTo(pluginPath + '\\' + PluginFile.Name);
        }

        public static void DeletePlugin(string PluginFileName)
        {
            FileInfo PluginFile = new FileInfo(PluginFileName);
            PluginFile.Delete();
        }
    }
}