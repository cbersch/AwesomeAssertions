using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AwesomeAssertions.Common;

namespace AwesomeAssertions.Types;

/// <summary>
/// Analyzes assembly references of a given assembly and returns a structured representation.
/// </summary>
internal sealed class AssemblyReferenceAnalyzer
{
    private readonly AssemblyReferenceNameMatcher matcher;
    private readonly Func<AssemblyName, bool> isUnwanted;
    private readonly Dictionary<AssemblyName, AssemblyReference> knownReferences = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyReferenceAnalyzer"/> class.
    /// </summary>
    /// <param name="isUnwanted">A function that determines whether a given <see cref="AssemblyName"/> is considered unwanted. The function
    /// should return <see langword="true"/> for unwanted assemblies; otherwise, <see langword="false"/>.</param>
    /// <param name="options">The options that configure the behavior of the assembly reference analysis, such as matching rules or filtering
    /// criteria.</param>
    private AssemblyReferenceAnalyzer(Func<AssemblyName, bool> isUnwanted, AssemblyReferenceOptions options)
    {
        matcher = new(options);
        this.isUnwanted = isUnwanted;
    }

    public static AssemblyReference AnalyzeFromExpected(Assembly subject, string expected, AssemblyReferenceOptions options)
    {
        Regex expectedRegex = AssemblyReferenceNameMatcher.ConvertWildcardToRegex(expected);
        bool IsUnwanted(AssemblyName assemblyName) => !expectedRegex.IsMatch(assemblyName.Name);

        return new AssemblyReferenceAnalyzer(IsUnwanted, options).Execute(subject);
    }

    public static AssemblyReference AnalyzeFromExpected(Assembly subject, string[] expected, AssemblyReferenceOptions options)
    {
        Regex[] expectedRegex = expected.Select(AssemblyReferenceNameMatcher.ConvertWildcardToRegex).ToArray();
        bool IsUnwanted(AssemblyName assemblyName) => !expectedRegex.Any(x => x.IsMatch(assemblyName.Name));

        return new AssemblyReferenceAnalyzer(IsUnwanted, options).Execute(subject);
    }

    public static AssemblyReference AnalyzeFromUnexpected(Assembly subject, string unexpected, AssemblyReferenceOptions options)
    {
        Regex unexpectedRegex = AssemblyReferenceNameMatcher.ConvertWildcardToRegex(unexpected);
        bool IsUnwanted(AssemblyName assemblyName) => unexpectedRegex.IsMatch(assemblyName.Name);

        return new AssemblyReferenceAnalyzer(IsUnwanted, options).Execute(subject);
    }

    private AssemblyReference Execute(Assembly subject)
    {
        Guard.ThrowIfArgumentIsNull(subject);
        return ResolveAllDependentAssemblies(subject);
    }

    private AssemblyReference ResolveAllDependentAssemblies(Assembly assembly)
    {
        AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
        List<AssemblyReference> references = ResolveAllUnwantedAssemblies(referencedAssemblies);
        AssemblyName assemblyName = assembly.GetName();
        if (references.Count > 0 || isUnwanted(assemblyName))
        {
            return new AssemblyReference(assemblyName, references);
        }

        return null;
    }

    private List<AssemblyReference> ResolveAllUnwantedAssemblies(AssemblyName[] names)
    {
        var references = new List<AssemblyReference>();

        foreach (AssemblyName name in FilterMatchingNames(names).OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!knownReferences.TryGetValue(name, out AssemblyReference knownReference))
            {
                knownReference = ResolveReferences(name);
                knownReferences[name] = knownReference;
            }

            if (knownReference is not null)
            {
                references.Add(knownReference);
            }
        }

        return references;
    }

    private IEnumerable<AssemblyName> FilterMatchingNames(AssemblyName[] assemblyNames) =>
        assemblyNames.Where(assemblyName => matcher.IncludeForAnalysis(assemblyName.Name));

    private AssemblyReference ResolveReferences(AssemblyName name)
    {
        if (TryLoadAssembly(name, out Assembly assembly))
        {
            return ResolveAllDependentAssemblies(assembly);
        }

        if (isUnwanted(name))
        {
            return new AssemblyReference(name, []);
        }

        return null;
    }

    private static bool TryLoadAssembly(AssemblyName assemblyName, out Assembly assembly)
    {
        assembly = Assembly.Load(assemblyName);
        return true;
    }

    private sealed class AssemblyReferenceNameMatcher(AssemblyReferenceOptions options)
    {
        private readonly Regex[] including = [.. options.IncludeWildcards.Select(ConvertWildcardToRegex)];
        private readonly Regex[] excluding = [.. options.ExcludeWildcards.Select(ConvertWildcardToRegex)];
        private readonly bool includeSystemReferences = options.IncludeSystemReferences;

        public static Regex ConvertWildcardToRegex(string wildcardExpression)
        {
            return new Regex("^"
                + Regex.Escape(wildcardExpression)
                    .Replace("\\*", ".*", StringComparison.Ordinal)
                    .Replace("\\?", ".", StringComparison.Ordinal)
                + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public bool IncludeForAnalysis(string assemblyName)
        {
            if (IsSystemAssembly(assemblyName))
            {
                return includeSystemReferences;
            }

            return IsIncluded(assemblyName) && !IsExcluded(assemblyName);
        }

        private bool IsIncluded(string assemblyName) =>
            including.Length == 0 || including.Any(regex => regex.IsMatch(assemblyName));

        private bool IsExcluded(string assemblyName) =>
            excluding.Length != 0 && excluding.Any(regex => regex.IsMatch(assemblyName));

        private static bool IsSystemAssembly(string assemblyName) =>
            assemblyName == "System"
            || assemblyName == "mscorlib"
            || assemblyName == "netstandard"
            || assemblyName.StartsWith("System.", StringComparison.Ordinal);
    }
}
