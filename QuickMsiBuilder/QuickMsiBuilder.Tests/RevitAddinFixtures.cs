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
