using System;
using System.Collections.Generic;

namespace AwesomeAssertions.Formatting.Tree;

/// <summary>
/// Handler for configurable parent-child relations to be visualized with <see cref="TreeFormatter{T}"/>.
/// </summary>
/// <typeparam name="T">Type of tree item to work on</typeparam>
internal sealed class TreeFormatterItemHandler<T>
    where T : class
{
    private readonly Func<T, IReadOnlyList<T>> getChildren;
    private readonly Func<T, string> getDisplayName;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="getChildren">Function to get the children of an item</param>
    /// <param name="getDisplayName">Function to get the displayed name of an item. If empty, the object's <see cref="object.ToString"/> method is used.</param>
    public TreeFormatterItemHandler(Func<T, IReadOnlyList<T>> getChildren, Func<T, string> getDisplayName)
    {
        this.getChildren = getChildren;
        this.getDisplayName = getDisplayName;
    }

    public IReadOnlyList<T> GetChildren(T item)
        => getChildren(item) ?? [];

    public string GetDisplayName(T item)
        => getDisplayName(item);
}
