

#if R25
using System.Reflection;
using System.Runtime.Loader;

namespace RevitAddinManager.Model;

class AssemblyLoadContext : System.Runtime.Loader.AssemblyLoadContext
{
    public AssemblyLoadContext() : base(isCollectible: true)
    {

    }
}
#endif