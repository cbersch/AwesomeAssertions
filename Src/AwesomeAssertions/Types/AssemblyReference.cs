using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AwesomeAssertions.Types;

[DebuggerDisplay("{Name.Name} ({References.Count})")]
internal sealed class AssemblyReference(AssemblyName name, IReadOnlyList<AssemblyReference> references)
{
    public AssemblyName Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public IReadOnlyList<AssemblyReference> References { get; } = references ?? throw new ArgumentNullException(nameof(references));
}
