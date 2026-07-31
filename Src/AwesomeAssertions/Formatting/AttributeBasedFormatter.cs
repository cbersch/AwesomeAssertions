using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AwesomeAssertions.Common;
using AwesomeAssertions.Configuration;

namespace AwesomeAssertions.Formatting;

/// <summary>
/// Specialized value formatter that looks for static methods in the caller's assembly marked with the
/// <see cref="ValueFormatterAttribute"/>.
/// </summary>
[RequiresDynamicCode("AttributeBasedFormatter scans assemblies at runtime and is not AOT-compatible.")]
[RequiresUnreferencedCode("AttributeBasedFormatter scans assemblies at runtime and is not trim-compatible.")]
public class AttributeBasedFormatter : IValueFormatter
{
    private Dictionary<Type, MethodInfo> formatters;
    private ValueFormatterDetectionMode detectionMode;

    /// <summary>
    /// Indicates whether the current <see cref="IValueFormatter"/> can handle the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value for which to create a <see cref="string"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the current <see cref="IValueFormatter"/> can handle the specified value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanHandle(object value)
    {
        return IsScanningEnabled && value is not null && GetFormatter(value) is not null;
    }

    private static bool IsScanningEnabled => AssertionConfiguration.Current.Formatting.ValueFormatterDetectionMode == ValueFormatterDetectionMode.Scan;

    /// <inheritdoc />
    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        MethodInfo method = GetFormatter(value);

        object[] parameters = [value, formattedGraph];

        method.Invoke(null, parameters);
    }

    private MethodInfo GetFormatter(object value)
    {
        Type valueType = value.GetType();

        do
        {
            if (Formatters.TryGetValue(valueType, out var formatter))
            {
                return formatter;
            }

            valueType = valueType.BaseType;
        }
        while (valueType is not null);

        return null;
    }

    private Dictionary<Type, MethodInfo> Formatters
    {
        get
        {
            HandleValueFormatterDetectionModeChanges();

            return formatters ??= FindCustomFormatters();
        }
    }

    private void HandleValueFormatterDetectionModeChanges()
    {
        ValueFormatterDetectionMode configuredDetectionMode =
            AssertionEngine.Configuration.Formatting.ValueFormatterDetectionMode;

        if (detectionMode != configuredDetectionMode)
        {
            detectionMode = configuredDetectionMode;
            formatters = null;
        }
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Intentional assembly scan guarded by class-level RequiresUnreferencedCode.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067", Justification = "Intentional assembly scan guarded by class-level RequiresUnreferencedCode.")]
    private static Dictionary<Type, MethodInfo> FindCustomFormatters()
    {
        var query =
            from type in TypeReflector.GetAllTypesFromAppDomain(Applicable)
            where type is not null
            from method in GetFormatterMethods(type)
            where method.IsStatic
            where method.ReturnType == typeof(void)
            where method.IsDecoratedWithOrInherit<ValueFormatterAttribute>()
            let methodParameters = method.GetParameters()
            where methodParameters.Length == 2
            select new { Type = methodParameters[0].ParameterType, Method = method }
            into formatter
            group formatter by formatter.Type
            into formatterGroup
            select formatterGroup.First();

        return query.ToDictionary(f => f.Type, f => f.Method);
    }

    private static MethodInfo[] GetFormatterMethods(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        Type type)
    {
        return type.GetMethods(BindingFlags.Static | BindingFlags.Public);
    }

    private static bool Applicable(Assembly assembly)
    {
        GlobalFormattingOptions options = AssertionEngine.Configuration.Formatting;
        ValueFormatterDetectionMode mode = options.ValueFormatterDetectionMode;

        return mode == ValueFormatterDetectionMode.Scan || (
            mode == ValueFormatterDetectionMode.Specific &&
            assembly.FullName.Split(',')[0].Equals(options.ValueFormatterAssembly, StringComparison.OrdinalIgnoreCase));
    }
}
