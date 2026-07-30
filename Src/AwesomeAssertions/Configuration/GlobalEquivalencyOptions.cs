using System;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using AwesomeAssertions.Common;
using AwesomeAssertions.Equivalency;
using JetBrains.Annotations;

namespace AwesomeAssertions.Configuration;

/// <summary>
/// Provides access to the defaults used by the structural equivalency assertions.
/// </summary>
public class GlobalEquivalencyOptions
{
    private EquivalencyOptions defaults = new();
    private EquivalencyPlan plan;

    /// <summary>
    /// Represents a mutable plan consisting of steps that are executed while asserting a (collection of) object(s)
    /// is structurally equivalent to another (collection of) object(s).
    /// </summary>
    /// <remarks>
    /// Members on this property are not thread-safe and should not be invoked from within a unit test.
    /// See the <see href="https://awesomeassertions.org/extensibility/#thread-safety">docs</see> on how to safely use it.
    /// Accessing this property is not supported in Native AOT.
    /// </remarks>
    public EquivalencyPlan Plan
    {
        [RequiresUnreferencedCode("GlobalEquivalencyOptions.Plan relies on reflection-based equivalency steps and is not trim-compatible.")]
        [RequiresDynamicCode("GlobalEquivalencyOptions.Plan relies on dynamic code and is not compatible with Native AOT.")]
        get
        {
#if NET5_0_OR_GREATER
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                throw new NotSupportedException("GlobalEquivalencyOptions.Plan is not supported when dynamic code is unavailable (for example, Native AOT). Use non-equivalency configuration APIs instead.");
            }
#endif

            return plan ??= new EquivalencyPlan();
        }
    }

    /// <summary>
    /// Allows configuring the defaults used during a structural equivalency assertion.
    /// </summary>
    /// <remarks>
    /// This method is not thread-safe and should not be invoked from within a unit test.
    /// See the <see href="https://awesomeassertions.org/extensibility/#thread-safety">docs</see> on how to safely use it.
    /// </remarks>
    /// <param name="configureOptions">
    /// An action that is used to configure the defaults.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public void Modify(Func<EquivalencyOptions, EquivalencyOptions> configureOptions)
    {
        Guard.ThrowIfArgumentIsNull(configureOptions);

        defaults = configureOptions(defaults);
    }

    /// <summary>
    /// Creates a clone of the default options and allows the caller to modify them.
    /// </summary>
    /// <remarks>
    /// Can be used by external packages like AwesomeAssertions.DataSets to create a copy of the default equivalency options.
    /// </remarks>
    [PublicAPI]
    public EquivalencyOptions<T> CloneDefaults<T>()
    {
        return new EquivalencyOptions<T>(defaults);
    }
}
