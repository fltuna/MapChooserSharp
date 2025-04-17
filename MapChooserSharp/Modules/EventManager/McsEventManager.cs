using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination.MapNominatedEvent;
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

    
    
    
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    public void RegisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler) 
        where TEvent : IMcsEvent
    {
        Type eventType = typeof(TEvent);
        
        if (!_handlers.TryGetValue(eventType, out List<Delegate>? value))
        {
            value = new List<Delegate>();
            _handlers[eventType] = value;
        }

        value.Add(handler);
    }
    
    public void UnregisterEventHandler<TEvent>(Func<TEvent, McsEventResultWithCallback> handler)
        where TEvent : IMcsEvent
    {
        Type eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    public McsEventResult FireEvent<TEvent>(TEvent eventInstance) 
        where TEvent : IMcsEvent
    {
        Type eventType = typeof(TEvent);
        McsEventResult highestResult = McsEventResult.Continue;
        Action<McsEventResult>? finalCallback = null;
        
        if (_handlers.TryGetValue(eventType, out var handlers))
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
                        break;
                    }
                }
            }
            
            // Execute callback
            // If event is not cancelled by McsEventResult.Stop
            // Then finalCallback should be null and nothing executed.
            finalCallback?.Invoke(highestResult);
        }
        
        return highestResult;
    }
}
