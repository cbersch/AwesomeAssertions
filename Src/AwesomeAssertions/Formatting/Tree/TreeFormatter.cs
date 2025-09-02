using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions.Common;

namespace AwesomeAssertions.Formatting.Tree;

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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="itemHandler">Handler for managing the parent-child hierarchy of type <typeparamref name="T"/></param>
    public TreeFormatter(TreeFormatterItemHandler<T> itemHandler)
    {
        this.itemHandler = itemHandler ?? throw new ArgumentNullException(nameof(itemHandler));
    }

    /// <summary>
    /// Format <paramref name="item"/> as single line according to its position in the tree.
    /// </summary>
    /// <param name="handler">A handler for processing each preformatted line</param>
    private void Format(T item, Stack<T> parents, ITreeFormatterLineWriter handler)
    {
        if (item is null)
        {
            return;
        }

        T current = item;
        bool isLast = true;
        foreach (T parent in parents)
        {
            IReadOnlyList<T> children = itemHandler.GetChildren(parent);
            Position posInParent = GetPositionInChildren(current, children);
            if (posInParent == Position.First || posInParent == Position.Middle)
            {
                if (isLast)
                {
                    handler.Prepend(SymbolVerticalRight);
                }
                else
                {
                    handler.Prepend(SymbolVertical);
                }
            }
            else if (posInParent == Position.Last && isLast)
            {
                handler.Prepend(SymbolUpRight);
            }
            else
            {
                handler.Prepend(SymbolEmpty);
            }

            isLast = false;
            current = parent;
        }

        handler.Append(itemHandler.GetDisplayName(item));
    }

    /// <summary>
    /// Format the full tree structure defined by <paramref name="root"/> as root.
    /// </summary>
    /// <returns>The formatted tree.</returns>
    public string FormatRecursive(T root, ITreeFormatterLineWriter handler)
    {
        Guard.ThrowIfArgumentIsNull(root);
        Guard.ThrowIfArgumentIsNull(handler);

        StringBuilder sb = new();
        FormatRecursive(root, new Stack<T>(), handler);
        return sb.ToString();
    }

    /// <summary>
    /// Format the full tree structure defined by <paramref name="root"/> as root.
    /// </summary>
    /// <param name="handler">A handler for processing each preformatted line.</param>
    public void FormatRecursive(T root, Stack<T> parents, ITreeFormatterLineWriter handler)
    {
        Format(root, parents, handler);
        handler.FlushLine();

        parents.Push(root);
        foreach (T child in itemHandler.GetChildren(root))
        {
            FormatRecursive(child, parents, handler);
        }

        parents.Pop();
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
