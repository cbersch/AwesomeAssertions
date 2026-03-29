using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions.Execution;

namespace AwesomeAssertions.Equivalency.Steps;

/// <summary>
/// Asserts the equivalency of two objects by recursively comparing the members that are selected from the expectation.
/// </summary>
public class StructuralEqualityEquivalencyStep : IEquivalencyStep
{
    /// <inheritdoc />
    public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context,
        IValidateChildNodeEquivalency valueChildNodes)
    {
        if (!context.CurrentNode.IsRoot && !context.Options.IsRecursive)
        {
            return EquivalencyResult.ContinueWithNext;
        }

        var assertionChain = AssertionChain.GetOrCreate().For(context);

        if (comparands.Expectation is null)
        {
            assertionChain
                .BecauseOf(context.Reason)
                .FailWith(
                    "Expected {context:subject} to be <null>{reason}, but found {0}.",
                    comparands.Subject);
        }
        else if (comparands.Subject is null)
        {
            assertionChain
                .BecauseOf(context.Reason)
                .FailWith(
                    "Expected {context:object} to be {0}{reason}, but found {1}.",
                    comparands.Expectation,
                    comparands.Subject);
        }
        else
        {
            IMember[] selectedMembers = GetExpectationMembers(context.CurrentNode, comparands, context.Options).ToArray();

            if (context.CurrentNode.IsRoot && selectedMembers.Length == 0)
            {
                throw new InvalidOperationException(
                    "No members were found for comparison. " +
                    "Please specify some members to include in the comparison or choose a more meaningful assertion.");
            }

            List<IMember> matchingMembers = new(selectedMembers.Length);
            foreach (IMember selectedMember in selectedMembers)
            {
                IMember matchingMember = AssertMemberEquality(comparands, context, valueChildNodes, selectedMember, context.Options);
                if (matchingMember is not null)
                {
                    matchingMembers.Add(matchingMember);
                }
            }

            if (context.Options.ThrowOnUnexpectedMembers)
            {
                AssertNoUnexpectedMembers(matchingMembers, context.CurrentNode, comparands, context.Options);
            }
        }

        return EquivalencyResult.EquivalencyProven;
    }

    private static void AssertNoUnexpectedMembers(List<IMember> matchingMembers, INode currentNode, Comparands comparands, IEquivalencyOptions options)
    {
        foreach (var subjectMember in GetSubjectMembers(currentNode, comparands, options))
        {
            if (!matchingMembers.Exists(x => x.Subject.Name == subjectMember.Subject.Name))
            {
                AssertionChain.GetOrCreate()
                    .FailWith($"Subject has {subjectMember.Subject} which the expectation does not have");
            }
        }
    }

    private static IMember AssertMemberEquality(Comparands comparands, IEquivalencyValidationContext context,
        IValidateChildNodeEquivalency parent, IMember selectedMember, IEquivalencyOptions options)
    {
        var assertionChain = AssertionChain.GetOrCreate().For(context);

        IMember matchingMember = FindMatchFor(selectedMember, context.CurrentNode, comparands.Subject, options, assertionChain);
        if (matchingMember is not null)
        {
            var nestedComparands = new Comparands(
                matchingMember.GetValue(comparands.Subject),
                selectedMember.GetValue(comparands.Expectation),
                selectedMember.Type);

            // In case the matching process selected a different member on the subject,
            // adjust the current member so that assertion failures report the proper name.
            selectedMember.AdjustForRemappedSubject(matchingMember);

            parent.AssertEquivalencyOf(nestedComparands, context.AsNestedMember(selectedMember));
        }

        return matchingMember;
    }

    private static IMember FindMatchFor(IMember selectedMember, INode currentNode, object subject,
        IEquivalencyOptions config, AssertionChain assertionChain)
    {
        IEnumerable<IMember> query =
            from rule in config.MatchingRules
            let match = rule.Match(selectedMember, subject, currentNode, config, assertionChain)
            where match is not null
            select match;

        if (config.IgnoreNonBrowsableOnSubject)
        {
            query = query.Where(member => member.IsBrowsable);
        }

        return query.FirstOrDefault();
    }

    private static IEnumerable<IMember> GetExpectationMembers(INode currentNode, Comparands comparands,
        IEquivalencyOptions options)
    {
        IEnumerable<IMember> members = [];

        foreach (IMemberSelectionRule rule in options.SelectionRules)
        {
            members = rule.SelectMembers(currentNode, members,
                new MemberSelectionContext(comparands.CompileTimeType, comparands.RuntimeType, options));
        }

        return members;
    }

    private static IEnumerable<IMember> GetSubjectMembers(INode currentNode, Comparands comparands,
        IEquivalencyOptions options)
    {
        IEnumerable<IMember> members = [];

        foreach (IMemberSelectionRule rule in options.SelectionRules)
        {
            members = rule.SelectMembers(currentNode, members,
                new MemberSelectionContext(comparands.SubjectCompileTimeType, comparands.SubjectRuntimeType, options));
        }

        return members;
    }
}
