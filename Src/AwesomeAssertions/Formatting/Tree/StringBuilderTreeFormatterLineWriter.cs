using System.Text;

namespace AwesomeAssertions.Formatting.Tree;

/// <summary>
/// Handler for formatting a tree line by line to a <see cref="StringBuilder"/>.
/// </summary>
internal sealed class StringBuilderTreeFormatterLineWriter : ITreeFormatterLineWriter
{
    private readonly StringBuilder sb;
    private int lineStartPos;

    public StringBuilderTreeFormatterLineWriter(StringBuilder sb)
    {
        this.sb = sb;
    }

    public void Append(string s)
        => sb.Append(s);

    public void Prepend(string s)
        => sb.Insert(lineStartPos, s);

    public void FlushLine()
    {
        sb.AppendLine();
        lineStartPos = sb.Length;
    }
}
