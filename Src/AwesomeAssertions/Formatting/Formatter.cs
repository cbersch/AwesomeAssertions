using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using AwesomeAssertions.Common;
using AwesomeAssertions.Equivalency.Execution;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Xml;

namespace AwesomeAssertions.Formatting;

/// <summary>
/// Provides services for formatting an object being used in an assertion in a human-readable format.
/// </summary>
public static class Formatter
{
    #region Private Definitions

    private static readonly List<IValueFormatter> CustomFormatters = [];

    private static readonly List<IValueFormatter> DefaultFormatters = BuildDefaultFormatters();

    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "Reflection-based formatters only added when RuntimeFeature.IsDynamicCodeSupported.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Reflection-based formatters only added when RuntimeFeature.IsDynamicCodeSupported.")]
    private static List<IValueFormatter> BuildDefaultFormatters()
    {
        var formatters = new List<IValueFormatter>
        {
            new XmlReaderValueFormatter(),
            new XmlNodeFormatter(),
            new AggregateExceptionValueFormatter(),
            new XDocumentValueFormatter(),
            new XElementValueFormatter(),
            new XAttributeValueFormatter(),
            new PropertyInfoFormatter(),
            new MethodInfoFormatter(),
            new NullValueFormatter(),
            new GuidValueFormatter(),
            new DateTimeOffsetValueFormatter(),
#if NET6_0_OR_GREATER
            new DateOnlyValueFormatter(),
            new TimeOnlyValueFormatter(),
#endif
            new TimeSpanValueFormatter(),
            new Int32ValueFormatter(),
            new Int64ValueFormatter(),
            new DoubleValueFormatter(),
            new SingleValueFormatter(),
            new DecimalValueFormatter(),
            new ByteValueFormatter(),
            new UInt32ValueFormatter(),
            new UInt64ValueFormatter(),
            new Int16ValueFormatter(),
            new UInt16ValueFormatter(),
            new SByteValueFormatter(),
            new StringValueFormatter(),
            new TaskFormatter(),
            new ExpressionValueFormatter(),
            new ExceptionValueFormatter(),
            new MultidimensionalArrayFormatter(),
            new DictionaryValueFormatter(),
            new EnumerableValueFormatter(),
            new EnumValueFormatter(),
            new TypeValueFormatter(),
        };

#if NET5_0_OR_GREATER
        if (RuntimeFeature.IsDynamicCodeSupported)
#endif
        {
            formatters.Insert(2, new AttributeBasedFormatter());
            formatters.Add(new PredicateLambdaExpressionValueFormatter());
            formatters.Add(new DefaultValueFormatter());
        }
#if NET5_0_OR_GREATER
        else
        {
            formatters.Add(new ToStringValueFormatter());
        }
#endif

        return formatters;
    }

    /// <summary>
    /// Is used to detect recursive calls by <see cref="IValueFormatter"/> implementations.
    /// </summary>
    [ThreadStatic]
    private static bool isReentry;

    #endregion

    /// <summary>
    /// A list of objects responsible for formatting the objects represented by placeholders.
    /// </summary>
    public static IEnumerable<IValueFormatter> Formatters =>
        AssertionScope.Current.FormattingOptions.ScopedFormatters
            .Concat(CustomFormatters)
            .Concat(DefaultFormatters);

    /// <summary>
    /// Returns a human-readable representation of a particular object.
    /// </summary>
    /// <param name="value">The value for which to create a <see cref="string"/>.</param>
    /// <param name="options">
    /// Indicates whether the formatter should use line breaks when the specific <see cref="IValueFormatter"/> supports it.
    /// </param>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public static string ToString(object value, FormattingOptions options = null)
    {
        options ??= new FormattingOptions();

        try
        {
            if (isReentry)
            {
                throw new InvalidOperationException(
                    $"Use the {nameof(FormatChild)} delegate inside a {nameof(IValueFormatter)} to recursively format children");
            }

            isReentry = true;

            var graph = new ObjectGraph(value);

            var context = new FormattingContext
            {
                UseLineBreaks = options.UseLineBreaks,
                MaxItems = options.MaxItems
            };

            FormattedObjectGraph output = new(options.MaxLines);

            try
            {
                Format(value, output, context,
                    (path, childValue, childOutput)
                        => FormatChild(path, childValue, childOutput, context, options, graph));
            }
            catch (MaxLinesExceededException)
            {
            }

            return output.ToString();
        }
        finally
        {
            isReentry = false;
        }
    }

    private static void FormatChild(string path, object value, FormattedObjectGraph output, FormattingContext context,
        FormattingOptions options, ObjectGraph graph)
    {
        try
        {
            Guard.ThrowIfArgumentIsNullOrEmpty(path, nameof(path), "Formatting a child value requires a path");

            if (!graph.TryPush(path, value))
            {
                output.AddFragment("{Cyclic reference to type ");
                TypeValueFormatter.FormatType(value.GetType(), output.AddFragment);
                output.AddFragment(" detected}");
            }
            else if (graph.Depth > options.MaxDepth)
            {
                output.AddLine($"Maximum recursion depth of {options.MaxDepth} was reached. " +
                    $" Increase {nameof(FormattingOptions.MaxDepth)} on {nameof(AssertionScope)} or {nameof(AssertionConfiguration)} to get more details.");
            }
            else
            {
                using (output.WithIndentation())
                {
                    Format(value, output, context, (childPath, childValue, nestedOutput) =>
                        FormatChild(childPath, childValue, nestedOutput, context, options, graph));
                }
            }
        }
        finally
        {
            graph.Pop();
        }
    }

    private static void Format(object value, FormattedObjectGraph output, FormattingContext context, FormatChild formatChild)
    {
        IValueFormatter firstFormatterThatCanHandleValue = Formatters.First(f => f.CanHandle(value));
        firstFormatterThatCanHandleValue.Format(value, output, context, formatChild);
    }

    /// <summary>
    /// Removes a custom formatter that was previously added through <see cref="Formatter.AddFormatter"/>.
    /// </summary>
    /// <remarks>
    /// This method is not thread-safe and should not be invoked from within a unit test.
    /// See the <see href="https://awesomeassertions.org/extensibility/#thread-safety">docs</see> on how to safely use it.
    /// </remarks>
    public static void RemoveFormatter(IValueFormatter formatter)
    {
        CustomFormatters.Remove(formatter);
    }

    /// <summary>
    /// Ensures a custom formatter is included in the chain, just before the default formatter is executed.
    /// </summary>
    /// <remarks>
    /// This method is not thread-safe and should not be invoked from within a unit test.
    /// See the <see href="https://awesomeassertions.org/extensibility/#thread-safety">docs</see> on how to safely use it.
    /// </remarks>
    public static void AddFormatter(IValueFormatter formatter)
    {
        if (!CustomFormatters.Contains(formatter))
        {
            CustomFormatters.Insert(0, formatter);
        }
    }

    /// <summary>
    /// Tracks the objects that were formatted as well as the path in the object graph of
    /// that object.
    /// </summary>
    /// <remarks>
    /// Is used to detect the maximum recursion depth as well as cyclic references in the graph.
    /// </remarks>
    /// <summary>AOT-safe fallback: formats any value by calling ToString().</summary>
    private sealed class ToStringValueFormatter : IValueFormatter
    {
        public bool CanHandle(object value) => true;

        public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
        {
            if (context.UseLineBreaks)
            {
                formattedGraph.AddFragmentOnNewLine(value.ToString());
            }
            else
            {
                formattedGraph.AddFragment(value.ToString());
            }
        }
    }

    private sealed class ObjectGraph
    {
        private readonly CyclicReferenceDetector tracker;
        private readonly Stack<string> pathStack;

        public ObjectGraph(object rootObject)
        {
            tracker = new CyclicReferenceDetector();
            pathStack = new Stack<string>();
            TryPush("root", rootObject);
        }

        public bool TryPush(string path, object value)
        {
            pathStack.Push(path);

            string fullPath = GetFullPath();
            var reference = new ObjectReference(value, fullPath);
            return !tracker.IsCyclicReference(reference);
        }

        private string GetFullPath() => string.Join(".", pathStack.Reverse());

        public void Pop()
        {
            pathStack.Pop();
        }

        public int Depth => pathStack.Count - 1;

        public override string ToString()
        {
            return string.Join(".", pathStack.Reverse());
        }
    }
}
