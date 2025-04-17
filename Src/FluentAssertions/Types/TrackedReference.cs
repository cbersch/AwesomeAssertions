using System.Collections.Generic;
using FluentAssertions.Common;

namespace FluentAssertions.Types;

internal sealed class TrackedReference
{
    private readonly List<string> referencePaths = [];

    public TrackedReference(string targetAssemblyName, string referencePath)
    {
        Guard.ThrowIfArgumentIsNullOrEmpty(targetAssemblyName);
        TargetAssemblyName = targetAssemblyName;

        if (!string.IsNullOrEmpty(referencePath))
        {
            referencePaths.Add(referencePath);
        }
    }

    public void Add(string referenceName)
    {
        Guard.ThrowIfArgumentIsNullOrEmpty(referenceName);
        referencePaths.Add(referenceName);
    }

    public IEnumerable<string> ReferencePaths => referencePaths;

    public string TargetAssemblyName { get; }
}
