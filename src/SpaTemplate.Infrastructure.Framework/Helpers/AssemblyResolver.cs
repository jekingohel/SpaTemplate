using System;
using System.Linq;
using System.Reflection;

namespace SpaTemplate.Infrastructure.Framework
{
    public static class AssemblyResolver
    {
        public static void RedirectAssembly()
        {
            var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName)
                .Select(a => a.FullName).ToList();

            Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
            {
                var requestedAssembly = new AssemblyName(args.Name);
                foreach (var asmName in list)
                    if (asmName.StartsWith(requestedAssembly.Name + ","))
                        return Assembly.Load(asmName);
                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;
        }
    }
}