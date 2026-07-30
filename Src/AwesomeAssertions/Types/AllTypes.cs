using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AwesomeAssertions.Types;

/// <summary>
/// Static class that allows for a 'fluent' selection of the types from an <see cref="Assembly"/>.
/// </summary>
/// <example>
/// AllTypes.From(myAssembly)<br />
/// .ThatImplement&lt;ISomeInterface&gt;<br />
/// .Should()<br />
/// .BeDecoratedWith&lt;SomeAttribute&gt;()
/// </example>
public static class AllTypes
{
    /// <summary>
    /// Returns a <see cref="TypeSelector"/> for selecting the types that are visible outside the
    /// specified <paramref name="assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly from which to select the types.</param>
    [RequiresUnreferencedCode("Assembly.GetTypes requires unreferenced code")]
    public static TypeSelector From(Assembly assembly)
    {
#pragma warning disable IL2026 // Types() requires unreferenced code, but caller is marked with [RequiresUnreferencedCode]
        return assembly.Types();
#pragma warning restore IL2026
    }
}
