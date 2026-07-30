using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using AwesomeAssertions.Execution;

namespace AwesomeAssertions.Equivalency.Steps;

/// <summary>
/// Asserts the equivalency of two <see cref="XElement"/> instances.
/// </summary>
public class XElementEquivalencyStep : EquivalencyStep<XElement>
{
    /// <inheritdoc />
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "This internal step intentionally forwards to an API marked as non-trim-compatible.")]
    [UnconditionalSuppressMessage("AotAnalysis", "IL3050",
        Justification = "This internal step intentionally forwards to an API marked as non-AOT-compatible.")]
    protected override EquivalencyResult OnHandle(Comparands comparands,
        IEquivalencyValidationContext context,
        IValidateChildNodeEquivalency nestedValidator)
    {
        var subject = (XElement)comparands.Subject;
        var expectation = (XElement)comparands.Expectation;

        AssertionChain.GetOrCreate().For(context).ReuseOnce();

        subject.Should().BeEquivalentTo(expectation, context.Reason.FormattedMessage, context.Reason.Arguments);

        return EquivalencyResult.EquivalencyProven;
    }
}
