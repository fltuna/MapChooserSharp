using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination;
using MapChooserSharp.API.Events.Nomination.MapNominationBeginEvent;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.Nomination;

public sealed class McsMapNominationController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), INominationApi
{
    public override string PluginModuleName => "McsMapNominationController";
    public override string ModuleChatPrefix => $" {ChatColors.Green}[Nomination]";

    private McsEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;

    protected override void OnInitialize()
    {
        Plugin.AddCommand("mcs_testevent", "", CommandTestEvent);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<McsEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        
        _mcsEventManager.RegisterEventHandler<McsMapNominationBeginEvent>(OnMapNominated);

    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("mcs_testevent", CommandTestEvent);
    }


    
    
    
    private void CommandTestEvent(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            info.ReplyToCommand("Usage: mcs_mapinfo <map name>");
            return;
        }

        var mapCfg = _mapConfigProvider.GetMapConfig(info.ArgByIndex(1));

        if (mapCfg == null)
        {
            info.ReplyToCommand("Map config not found");
            return;
        }

        
        var nominationBegin = new McsMapNominationBeginEvent(player, mapCfg);
        McsEventResult result = _mcsEventManager.FireEvent(nominationBegin);
        
        if (result > McsEventResult.Changed)
        {
            return;
        }
        
        
        info.ReplyToCommand("Nominated");
    }


    private McsEventResultWithCallback OnMapNominated(McsMapNominationBeginEvent @event)
    {
        if (@event.Player == null || @event.Player.PlayerPawn.Value == null || @event.Player.PlayerPawn.Value.Health < 80)
        {
            return McsEventResultWithCallback.Stop(result =>
            {
                @event.Player!.PrintToChat($"[Some Plugin Prefix] Your health is not enough to nominate!!! Cancelling nomination. Status: {result}");
            });
        }

        return McsEventResult.Continue;
    }
}