#if R25 || R26
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace RevitAddinManager.Model;

class AssemblyLoadContext : System.Runtime.Loader.AssemblyLoadContext
{
    public AssemblyLoadContext() : base(isCollectible: true)
    {
    }
    private AssemblyDependencyResolver _resolver;

    public AssemblyLoadContext(string pluginPath): base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            if(assemblyPath.Contains("RevitAPI"))
            {
                return null;
            }
            var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read);
            return LoadFromStream(stream);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }

}
#endif