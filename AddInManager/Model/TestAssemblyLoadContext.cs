using System.Reflection;
using System.Runtime.Loader;

namespace RevitAddinManager.Model;

class TestAssemblyLoadContext : AssemblyLoadContext
{
    public TestAssemblyLoadContext() : base(isCollectible: true)
    {

    }
}