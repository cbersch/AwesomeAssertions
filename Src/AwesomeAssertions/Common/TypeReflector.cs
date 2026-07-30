using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AwesomeAssertions.Common;

internal static class TypeReflector
{
    [RequiresUnreferencedCode("GetAllTypesFromAppDomain uses Assembly.GetExportedTypes() which requires metadata")]
    public static IEnumerable<Type> GetAllTypesFromAppDomain(Func<Assembly, bool> predicate)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !IsDynamic(a) && IsRelevant(a) && predicate(a))
            .SelectMany(GetExportedTypes).ToArray();
    }

    private static bool IsRelevant(Assembly ass)
    {
        string assemblyName = ass.GetName().Name;

        return
            assemblyName is not null &&
            !assemblyName.StartsWith("microsoft.", StringComparison.OrdinalIgnoreCase) &&
            !assemblyName.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) &&
            !assemblyName.StartsWith("jetbrains.", StringComparison.OrdinalIgnoreCase) &&
            !assemblyName.StartsWith("system", StringComparison.OrdinalIgnoreCase) &&
            !assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) &&
            !assemblyName.StartsWith("newtonsoft", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDynamic(Assembly assembly)
    {
        return assembly.GetType().FullName is "System.Reflection.Emit.AssemblyBuilder"
            or "System.Reflection.Emit.InternalAssemblyBuilder";
    }

    private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
    {
        try
        {
#pragma warning disable IL2026 // Assembly.GetExportedTypes requires unreferenced code; called from [RequiresUnreferencedCode] method
            return assembly.GetExportedTypes();
#pragma warning restore IL2026
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
        catch (FileLoadException)
        {
            return [];
        }
        catch (Exception)
        {
            return [];
        }
    }
}
