namespace MapChooserSharp.API.Events;

/// <summary>
/// Event system for MapChooserSharp
/// </summary>
public interface IMcsEventSystem
{
    /// <summary>
    /// Event handler registration method for events with result
    /// </summary>
    /// <param name="handler">Callback that returns a result</param>
    /// <typeparam name="TEvent">Event type</typeparam>
    void RegisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler) 
        where TEvent : IMcsEventWithResult;
    
    /// <summary>
    /// Event handler registration method for events without result
    /// </summary>
    /// <param name="handler">Callback that doesn't return a result</param>
    /// <typeparam name="TEvent">Event type</typeparam>
    void RegisterEventHandler<TEvent>(Action<TEvent> handler) 
        where TEvent : IMcsEventNoResult;
    
    /// <summary>
    /// Event handler unregistration method for events with result
    /// </summary>
    /// <param name="handler">Callback that returns a result</param>
    /// <typeparam name="TEvent">Event type</typeparam>
    void UnregisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler)
        where TEvent : IMcsEventWithResult;
    
    /// <summary>
    /// Event handler unregistration method for events without result
    /// </summary>
    /// <param name="handler">Callback that doesn't return a result</param>
    /// <typeparam name="TEvent">Event type</typeparam>
    void UnregisterEventHandler<TEvent>(Action<TEvent> handler)
        where TEvent : IMcsEventNoResult;
}