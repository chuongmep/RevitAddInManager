// Stand-ins for the Revit API contracts. AssemblyInspector matches interfaces by full name straight
// out of the IL, so these are enough to exercise the detection without referencing the Revit API.
namespace Autodesk.Revit.UI
{
    public interface IExternalCommand
    {
    }

    public interface IExternalApplication
    {
    }
}

namespace QuickMsiBuilder.Tests.Fixtures
{
    using Autodesk.Revit.UI;

    public class SampleCommand : IExternalCommand
    {
    }

    public class AnotherCommand : IExternalCommand
    {
    }

    public class SampleApplication : IExternalApplication
    {
    }

    public abstract class AbstractCommand : IExternalCommand
    {
    }

    internal class InternalCommand : IExternalCommand
    {
    }

    public class NotAnAddin
    {
    }

    public class Outer
    {
        public class NestedCommand : IExternalCommand
        {
        }
    }
}

namespace QuickMsiBuilder.Tests.Fixtures.Inherited
{
    using Autodesk.Revit.UI;

    // Interface sits on a shared base class - the pattern most add-ins use.
    public abstract class BaseCommand : IExternalCommand
    {
    }

    public class DerivedCommand : BaseCommand
    {
    }

    public class DeeplyDerivedCommand : DerivedCommand
    {
    }

    public abstract class BaseApplication : IExternalApplication
    {
    }

    public class DerivedApplication : BaseApplication
    {
    }

    // Interface inheriting the Revit one.
    public interface ICompanyCommand : IExternalCommand
    {
    }

    public class CommandViaInterfaceChain : ICompanyCommand
    {
    }
}
