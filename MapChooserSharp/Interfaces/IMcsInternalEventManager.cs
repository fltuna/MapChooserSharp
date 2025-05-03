using MapChooserSharp.API.Events;

namespace MapChooserSharp.Interfaces;

internal interface IMcsInternalEventManager: IMcsEventSystem
{
    internal McsEventResult FireEvent<TEvent>(TEvent eventInstance)
        where TEvent : IMcsEventWithResult;

    internal void FireEventNoResult<TEvent>(TEvent eventInstance)
        where TEvent : IMcsEventNoResult;
}