using System.Runtime.CompilerServices;
using AwesomeAssertions.Aot.Specs;
using AwesomeAssertions.Extensibility;

[assembly: AssertionEngineInitializer(
    typeof(XunitV3TestFrameworkInitializer),
    nameof(XunitV3TestFrameworkInitializer.Initialize))]

namespace AwesomeAssertions.Aot.Specs;

internal static class XunitV3TestFrameworkInitializer
{
    // [ModuleInitializer] guarantees execution in AOT; the assembly attribute handles JIT/non-AOT paths
    [ModuleInitializer]
    public static void Initialize()
    {
        AssertionEngine.TestFramework = new XunitV3TestFramework();
    }
}
