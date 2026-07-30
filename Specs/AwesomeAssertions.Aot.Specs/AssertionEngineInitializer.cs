using AwesomeAssertions.Aot.Specs;
using AwesomeAssertions.Extensibility;

[assembly: AssertionEngineInitializer(
    typeof(XunitV3TestFrameworkInitializer),
    nameof(XunitV3TestFrameworkInitializer.Initialize))]

namespace AwesomeAssertions.Aot.Specs;

/// <summary>
/// Registers <see cref="XunitV3TestFramework"/> as the active test framework before the first assertion runs,
/// so AwesomeAssertions does not need to rely on its own (reflection-based) xUnit v3 detection.
/// </summary>
internal static class XunitV3TestFrameworkInitializer
{
    public static void Initialize()
    {
        AssertionEngine.TestFramework = new XunitV3TestFramework();
    }
}
