using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace AwesomeAssertions.Events;

/// <summary>
/// Records activity for a single event.
/// </summary>
[DebuggerNonUserCode]
internal sealed class EventRecorder : IEventRecording, IDisposable
{
    private readonly Func<DateTime> utcNow;
    private readonly BlockingCollection<RecordedEvent> raisedEvents = [];
    private readonly object lockable = new();
    private Action cleanup;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventRecorder"/> class.
    /// </summary>
    /// <param name="eventRaiser">The object events are recorded from</param>
    /// <param name="eventName">The name of the event that's recorded</param>
    /// <param name="utcNow">A delegate to get the current date and time in UTC format.</param>
    /// <param name="sequenceGenerator">Class used to generate a sequence in a thread-safe manner.</param>
    public EventRecorder(object eventRaiser, string eventName, Func<DateTime> utcNow,
        ThreadSafeSequenceGenerator sequenceGenerator)
    {
        this.utcNow = utcNow;
        EventObject = eventRaiser;
        EventName = eventName;
        this.sequenceGenerator = sequenceGenerator;
    }

    /// <summary>
    /// The object events are recorded from
    /// </summary>
    public object EventObject { get; private set; }

    /// <inheritdoc />
    public string EventName { get; }

    private readonly ThreadSafeSequenceGenerator sequenceGenerator;

    public Type EventHandlerType { get; private set; }

    [RequiresDynamicCode("Creating dynamic event handlers requires dynamic code generation")]
    public void Attach(WeakReference subject, EventInfo eventInfo)
    {
        EventHandlerType = eventInfo.EventHandlerType;

#pragma warning disable IL2072 // EventInfo.EventHandlerType cannot be statically annotated with DynamicallyAccessedMembers
        Delegate handler = EventHandlerFactory.GenerateHandler(eventInfo.EventHandlerType, this);
#pragma warning restore IL2072
        eventInfo.AddEventHandler(subject.Target, handler);

        cleanup = () =>
        {
            if (subject.Target is not null)
            {
                eventInfo.RemoveEventHandler(subject.Target, handler);
            }
        };
    }

    public void Dispose()
    {
        Action localCleanup = cleanup;
        if (localCleanup is not null)
        {
            localCleanup();
            cleanup = null;
            EventObject = null;
            raisedEvents.Dispose();
        }
    }

    /// <summary>
    /// Called by the auto-generated IL, to record information about a raised event.
    /// </summary>
    [UsedImplicitly]
    public void RecordEvent(params object[] parameters)
    {
        lock (lockable)
        {
            raisedEvents.Add(new RecordedEvent(utcNow(), sequenceGenerator.Increment(), parameters));
        }
    }

    /// <summary>
    /// Resets recorder to clear records of events raised so far.
    /// </summary>
    public void Reset()
    {
        lock (lockable)
        {
            while (raisedEvents.Count > 0)
            {
                raisedEvents.TryTake(out _);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<OccurredEvent> GetEnumerator()
    {
        foreach (RecordedEvent @event in raisedEvents.ToArray())
        {
            yield return new OccurredEvent
            {
                EventName = EventName,
                Parameters = @event.Parameters,
                TimestampUtc = @event.TimestampUtc,
                Sequence = @event.Sequence
            };
        }
    }
}
