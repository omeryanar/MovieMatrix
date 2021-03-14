using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Setup
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains(".resources"))
                return null;

            string resourceName = "Setup.Library." + new AssemblyName(args.Name).Name + ".dll";
            using (Stream stream = ExecutingAssembly.GetManifestResourceStream(resourceName))
            {
                byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);

                return Assembly.Load(assemblyData);
            }
        }
    }
}
