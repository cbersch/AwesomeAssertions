using System.Collections.Generic;

namespace FluentAssertions.Formatting.Tree;

/// <summary>
/// Formatter for visualizing a tree structure.
/// </summary>
/// <typeparam name="T">Type of object to work on.</typeparam>
internal sealed class TreeFormatter<T>
    where T : class
{
    private const string SymbolEmpty = "  ";
    private const string SymbolUpRight = "└─";
    private const string SymbolVerticalRight = "├─";
    private const string SymbolVertical = "│ ";

    private readonly TreeFormatterItemHandler<T> itemHandler;

    public TreeFormatter(TreeFormatterItemHandler<T> itemHandler) => this.itemHandler = itemHandler;

    /// <summary>
    /// Format <paramref name="item"/> as single line according to its position in the tree.
    /// </summary>
    /// <param name="item">The item to format</param>
    /// <param name="writer">A writer for processing each preformatted line</param>
    public void Format(T item, ITreeFormatterLineWriter writer)
    {
        if (item is null)
        {
            return;
        }

        T current = item;
        T parent;
        bool isLast = true;
        while ((parent = itemHandler.GetParent(current)) is not null)
        {
            IReadOnlyList<T> children = itemHandler.GetChildren(parent);
            Position posInParent = GetPositionInChildren(current, children);
            if (posInParent == Position.First || posInParent == Position.Middle)
            {
                if (isLast)
                {
                    writer.Prepend(SymbolVerticalRight);
                }
                else
                {
                    writer.Prepend(SymbolVertical);
                }
            }
            else if (posInParent == Position.Last && isLast)
            {
                writer.Prepend(SymbolUpRight);
            }
            else
            {
                writer.Prepend(SymbolEmpty);
            }

            isLast = false;
            current = parent;
        }

        writer.Append(itemHandler.GetDisplayName(item));
    }

    /// <summary>
    /// Format the full tree structure defined by <paramref name="root"/> as root.
    /// </summary>
    /// <param name="handler">A handler for processing each preformatted line.</param>
    public void FormatRecursive(T root, ITreeFormatterLineWriter handler)
    {
        Format(root, handler);
        handler.FlushLine();
        if (root is null)
        {
            return;
        }

        foreach (T child in itemHandler.GetChildren(root))
        {
            FormatRecursive(child, handler);
        }
    }

    private static Position GetPositionInChildren(T item, IReadOnlyList<T> children)
    {
        int pos = IndexOf(children, item);
        if (children.Count == 1 || pos == children.Count - 1)
        {
            return Position.Last;
        }

        if (pos == 0)
        {
            return Position.First;
        }

        return Position.Middle;
    }

    private static int IndexOf(IReadOnlyList<T> children, T item)
    {
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].Equals(item))
            {
                return i;
            }
        }

        return -1;
    }

    private enum Position
    {
        First,
        Middle,
        Last,
    }
}
