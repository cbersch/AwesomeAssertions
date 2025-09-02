namespace AwesomeAssertions.Formatting.Tree;

/// <summary>
/// Handler for formatting a tree line by line
/// </summary>
internal interface ITreeFormatterLineWriter
{
    /// <summary>
    /// Append <paramref name="s"/> to the current line.
    /// </summary>
    /// <param name="s">String to append to the current line.</param>
    void Append(string s);

    /// <summary>
    /// Prepend <paramref name="s"/> to the current line.
    /// </summary>
    /// <param name="s">String to prepend to the current line.</param>
    void Prepend(string s);

    /// <summary>
    /// Write the current line to the target.
    /// </summary>
    void FlushLine();
}
