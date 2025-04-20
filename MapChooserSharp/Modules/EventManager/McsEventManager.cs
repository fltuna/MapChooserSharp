using System.Reflection;
using MapChooserSharp.API.Events;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.EventManager;

public sealed class McsEventManager(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsEventSystem
{
    public override string PluginModuleName => "McsEventManager";
    public override string ModuleChatPrefix => "McsEventManager";


    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
    }

    
    
    
    private readonly Dictionary<Type, List<Delegate>> _withResultHandlers = new();
    private readonly Dictionary<Type, List<Delegate>> _noResultHandlers = new();
    
    /// <summary>
    /// Register a handler for events with result
    /// </summary>
    public void RegisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler) 
        where TEvent : IMcsEventWithResult
    {
        Type eventType = typeof(TEvent);
        
        if (!_withResultHandlers.TryGetValue(eventType, out List<Delegate>? value))
        {
            value = new List<Delegate>();
            _withResultHandlers[eventType] = value;
        }

        value.Add(handler);
    }
    
    /// <summary>
    /// Register a handler for events without result
    /// </summary>
    public void RegisterEventHandler<TEvent>(Action<TEvent> handler) 
        where TEvent : IMcsEventNoResult
    {
        Type eventType = typeof(TEvent);
        
        if (!_noResultHandlers.TryGetValue(eventType, out List<Delegate>? value))
        {
            value = new List<Delegate>();
            _noResultHandlers[eventType] = value;
        }

        value.Add(handler);
    }
    
    /// <summary>
    /// Unregister a handler for events with result
    /// </summary>
    public void UnregisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler)
        where TEvent : IMcsEventWithResult
    {
        Type eventType = typeof(TEvent);
        
        if (_withResultHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    /// <summary>
    /// Unregister a handler for events without result
    /// </summary>
    public void UnregisterEventHandler<TEvent>(Action<TEvent> handler)
        where TEvent : IMcsEventNoResult
    {
        Type eventType = typeof(TEvent);
        
        if (_noResultHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    /// <summary>
    /// Fire an event with result
    /// </summary>
    internal McsEventResult FireEvent<TEvent>(TEvent eventInstance) 
        where TEvent : IMcsEventWithResult
    {
        Type eventType = typeof(TEvent);
        McsEventResult highestResult = McsEventResult.Continue;
        Action<McsEventResult>? finalCallback = null;
        
        if (_withResultHandlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handlerObj in handlers)
            {
                if (handlerObj is Func<TEvent, McsEventResultWithCallback> typedHandler)
                {
                    McsEventResultWithCallback result = typedHandler(eventInstance);
                    
                    if (result.Result > highestResult)
                    {
                        highestResult = result.Result;
                        finalCallback = result.Callback;
                    }
                    
                    if (highestResult >= McsEventResult.Stop)
                    {
                        var method = typedHandler.Method;
                        var declaringType = method.DeclaringType;
                        var fullClassName = declaringType?.FullName ?? "Unknown";
                        var methodName = method.Name;
    
                        DebugLogger.LogDebug($"The event {eventType} iteration is stopped by {fullClassName}::{methodName}");
                        break;
                    }
                }
            }
            
            // Execute callback if present
            finalCallback?.Invoke(highestResult);
        }
        
        return highestResult;
    }
    
    /// <summary>
    /// Fire an event without result
    /// </summary>
    internal void FireEventNoResult<TEvent>(TEvent eventInstance) 
        where TEvent : IMcsEventNoResult
    {
        Type eventType = typeof(TEvent);
        
        if (_noResultHandlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handlerObj in handlers)
            {
                if (handlerObj is Action<TEvent> typedHandler)
                {
                    typedHandler(eventInstance);
                }
            }
        }
    }
}
