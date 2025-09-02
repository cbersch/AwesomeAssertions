using System.Text;
using AwesomeAssertions.Formatting.Tree;
using AwesomeAssertions.Types;

namespace AwesomeAssertions.Formatting;

internal sealed class AssemblyReferenceValueFormatter : IValueFormatter
{
    private readonly TreeFormatter<AssemblyReference> treeFormatter = new(new TreeFormatterItemHandler<AssemblyReference>(
            reference => reference.References, reference => reference.Name.Name));

    public bool CanHandle(object value) => value is AssemblyReference;

    public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
    {
        StringBuilder sb = new();
        treeFormatter.FormatRecursive((AssemblyReference)value, new StringBuilderTreeFormatterLineWriter(sb));
        formattedGraph.AddFragment(sb.ToString());
    }
}
