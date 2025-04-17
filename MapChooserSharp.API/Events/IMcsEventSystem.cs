namespace MapChooserSharp.API.Events;

/// <summary>
/// Event system for MapChooserSharp
/// </summary>
public interface IMcsEventSystem
{
    /// <summary>
    /// Event handler registration method
    /// </summary>
    /// <param name="handler">callback</param>
    /// <typeparam name="TEvent">EventType</typeparam>
    void RegisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler) 
        where TEvent : IMcsEvent;
    
    /// <summary>
    /// Event handler unregistration method
    /// </summary>
    /// <param name="handler">callback</param>
    /// <typeparam name="TEvent">EventType</typeparam>
    void UnregisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler)
        where TEvent : IMcsEvent;
}