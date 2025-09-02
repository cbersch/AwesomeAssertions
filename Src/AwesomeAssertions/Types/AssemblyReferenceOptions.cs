using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AwesomeAssertions.Types;

/// <summary>
/// Options that configure the behavior of the assembly reference assertions, such as matching rules or filtering
/// criteria.
/// </summary>
public sealed class AssemblyReferenceOptions
{
    /// <summary>
    /// Default options. Excludes system references.
    /// </summary>
    public static AssemblyReferenceOptions Defaults { get; } = new(Array.Empty<string>(), Array.Empty<string>(), includeSystemReferences: false);

    public IEnumerable<string> IncludeWildcards { get; } = Array.Empty<string>();

    public IEnumerable<string> ExcludeWildcards { get; } = Array.Empty<string>();

    /// <summary>
    /// Include system references if <see langword="true"/>.
    /// </summary>
    public bool IncludeSystemReferences { get; }

    private AssemblyReferenceOptions(IEnumerable<string> includeWildcards, IEnumerable<string> excludeWildcards, bool includeSystemReferences)
    {
        IncludeWildcards = includeWildcards;
        ExcludeWildcards = excludeWildcards;
        IncludeSystemReferences = includeSystemReferences;
    }

    public AssemblyReferenceOptions IncludingSystemReferences() =>
        new(IncludeWildcards, ExcludeWildcards, includeSystemReferences: true);

    public AssemblyReferenceOptions Including(params string[] wildcards) =>
        new(IncludeWildcards.Concat(wildcards).ToArray(), ExcludeWildcards, IncludeSystemReferences);

    public AssemblyReferenceOptions Excluding(params string[] wildcards) =>
        new(IncludeWildcards, ExcludeWildcards.Concat(wildcards).ToArray(), IncludeSystemReferences);

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("- ")
            .Append(IncludeSystemReferences ? "Including" : "Excluding")
            .AppendLine(" system references");

        if (!IncludeWildcards.Any())
        {
            builder.AppendLine("- Without includes by wildcards");
        }
        else
        {
            foreach (var wildcard in IncludeWildcards)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "- Including wildcard \"{0}\"", wildcard);
            }
        }

        if (!ExcludeWildcards.Any())
        {
            builder.AppendLine("- Without excludes by wildcards");
        }
        else
        {
            foreach (var wildcard in ExcludeWildcards)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "- Excluding wildcard \"{0}\"", wildcard);
            }
        }

        return builder.ToString();
    }
}
