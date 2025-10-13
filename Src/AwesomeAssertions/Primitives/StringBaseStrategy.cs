using AwesomeAssertions.Common;
using AwesomeAssertions.Execution;

namespace AwesomeAssertions.Primitives
{
    internal abstract class StringBaseStrategy
    {
        private protected static bool ValidateAgainstNulls(AssertionChain assertionChain, string subject, string expected, string expectedDescription)
        {
            if (subject is null && expected is null)
            {
                return false;
            }

            assertionChain
                .ForCondition(subject is not null && expected is not null)
                .FailWith(() => GetFailureDescription(expectedDescription, subject, expected));

            return assertionChain.Succeeded;
        }

        private static FailReason GetFailureDescription(string expectedDescription, string subject, string expected)
        {
            if (subject.IsLongOrMultiline() || expected.IsLongOrMultiline())
            {
                return new FailReason($$"""
                    {{expectedDescription}}

                      {0}

                    {reason}, but found

                      {1}.
                    """, expected, subject);
            }

            return new FailReason($"{expectedDescription}{{0}}{{reason}}, but found {{1}}.", expected, subject);
        }
    }
}
