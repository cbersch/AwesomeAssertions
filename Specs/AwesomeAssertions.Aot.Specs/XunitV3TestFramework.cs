using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions.Execution;
using Xunit.Sdk;

namespace AwesomeAssertions.Aot.Specs;

/// <summary>
/// An AOT-compatible <see cref="ITestFramework"/> implementation for xUnit.net v3.
/// </summary>
/// <remarks>
/// The built-in <c>XUnitTestFramework</c> shipped with AwesomeAssertions detects xUnit by probing for the
/// <c>xunit.v3.assert</c> assembly at run-time using <see cref="System.Reflection.Assembly.Load(System.Reflection.AssemblyName)"/>.
/// That approach is unreliable when running under the Microsoft.Testing.Platform in-process console runner and is not
/// guaranteed to work in a Native AOT published application. This implementation instead references
/// <see cref="XunitException"/> directly at compile time, so no reflection or assembly probing is required.
/// </remarks>
internal sealed class XunitV3TestFramework : ITestFramework
{
    public bool IsAvailable => true;

    [DoesNotReturn]
    public void Throw(string message) => throw new XunitException(message);
}
