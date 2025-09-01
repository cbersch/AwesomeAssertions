using System;
using System.Collections;

namespace AwesomeAssertions.Formatting;

/// <summary>
/// Holds a <see cref="Type"/> which should be formatted in short notation, i.e. without any namespaces.
/// </summary>
/// <param name="value">The value to format</param>
/// <param name="relevantIndex">The relevant index</param>
internal sealed class EnumerableValue(IEnumerable value, int relevantIndex)
{
    public IEnumerable Value { get; } = value;

    public int RelevantIndex { get; } = relevantIndex;

    public void Deconstruct(out IEnumerable value, out int relevantIndex)
    {
        value = Value;
        relevantIndex = RelevantIndex;
    }
}
