using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentAssertions.Types;

/// <summary>
/// Builds the full hierarchy of assemblies referenced by a given assembly
/// </summary>
/// <param name="assembly">Assembly to analyze</param>
internal sealed class ReferenceHierarchyBuilder(Assembly assembly)
{
    private readonly Assembly assembly = assembly;
    private readonly Dictionary<string, TrackedReference> references = [];

    public IEnumerable<TrackedReference> Execute()
    {
        GetAllDependentAssemblies(assembly, string.Empty);
        return references.Values;
    }

    private void GetAllDependentAssemblies(Assembly assembly, string currentHierarchyPath)
    {
        AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
        var unknownReferences = new List<AssemblyName>();
        foreach (AssemblyName reference in referencedAssemblies)
        {
            if (!TrackReference(reference.Name, currentHierarchyPath))
            {
                unknownReferences.Add(reference);
            }
        }

        foreach (AssemblyName unknownReference in unknownReferences)
        {
            if (TryLoadAssembly(unknownReference, out Assembly assemblyToCheck))
            {
                string hierarchyPath = $"{currentHierarchyPath}/{assemblyToCheck.GetName().Name}";
                GetAllDependentAssemblies(assemblyToCheck, hierarchyPath);
            }
        }
    }

    private bool TrackReference(string assemblyName, string currentHierachyPath)
    {
        if (references.TryGetValue(assemblyName, out TrackedReference existingReference))
        {
            existingReference.Add(currentHierachyPath);
            return true;
        }

        references.Add(assemblyName, new TrackedReference(assemblyName, currentHierachyPath));
        return false;
    }

    private static bool TryLoadAssembly(AssemblyName assemblyName, out Assembly assembly)
    {
        try
        {
            assembly = Assembly.Load(assemblyName);
            return true;
        }
        catch (Exception)
        {
            assembly = null;
            return false;
        }
    }
}
