using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.Nomination;

public sealed class McsMapNominationController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsNominationApi
{
    public override string PluginModuleName => "McsMapNominationController";
    public override string ModuleChatPrefix => $" {ChatColors.Green}[Nomination]{ChatColors.Default}";

    private McsEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;

    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_nominate", "", CommandNominateMap);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<McsEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        
        _mcsEventManager.RegisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsEventManager.RegisterEventHandler<McsMapNominatedEvent>(OnMapNominated);

    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsEventManager.UnregisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
    }






    private void CommandNominateMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            info.ReplyToCommand("TODO_TRANSLATE| Usage: !nominate <MapName>");
            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapConfigProvider.GetMapConfigs();

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            info.ReplyToCommand($"TODO_TRANSLATE| No map(s) found with {mapName}");

            if (player == null)
                return;
            
            ShowNominationMenu(player);
            return;
        }

        if (matchedMaps.Count > 1)
        {
            info.ReplyToCommand($"TODO_TRANSLATE| {matchedMaps.Count} maps found with {mapName}");

            if (player == null)
            {
                info.ReplyToCommand("Please specify the identical name.");
                return;
            }
            
            ShowNominationMenu(player, matchedMaps);
            return;
        }
        
        NominateMap(player, matchedMaps.First().Value);
    }



    private void NominateMap(CCSPlayerController? player, IMapConfig mapConfig)
    {
        var nominationBegin = new McsNominationBeginEvent(player, mapConfig, ModuleChatPrefix);
        McsEventResult result = _mcsEventManager.FireEvent(nominationBegin);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Nomination begin event cancelled by a another plugin.");
            return;
        }

        string executorName = PlayerUtil.GetPlayerName(player);
        if (mapConfig.MapNameAlias != string.Empty)
        {
            Server.PrintToChatAll($"TODO_TRANSLATE| {executorName} nominated {mapConfig.MapNameAlias}");
        }
        else
        {
            Server.PrintToChatAll($"TODO_TRANSLATE| {executorName} nominated {mapConfig.MapName}");
        }
        
        var eventNominated = new McsMapNominatedEvent(player, mapConfig, executorName);
        _mcsEventManager.FireEventNoResult(eventNominated);
    }
    
    private void ShowNominationMenu(CCSPlayerController player, Dictionary<string, IMapConfig>? mapCfgArg = null)
    {
        // TODO() Implement later
        player.PrintToChat("TODO_MENU| Nomination menu");

        Dictionary<string, IMapConfig>? mapConfigs = mapCfgArg ?? _mapConfigProvider.GetMapConfigs();
    }
    
    
    

    private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
    {
        if (@event.Player == null || @event.Player.PlayerPawn.Value == null || @event.Player.PlayerPawn.Value.Health < 80)
        {
            return McsEventResultWithCallback.Stop(result =>
            {
                @event.Player!.PrintToChat($"[McsMapNominationBeginEvent Listener] Your health is not enough to nominate!!! Cancelling nomination. Status: {result}");
                @event.Player.PrintToChat($"{@event.ModulePrefix} You can use MCS module's prefix!");
            });
        }

        return McsEventResult.Continue;
    }

    private void OnMapNominated(McsMapNominatedEvent @event)
    {
        var player = @event.Player;

        string executorName = PlayerUtil.GetPlayerName(@event.Player);
        
        if (player == null)
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected {executorName} nominated {@event.MapConfig.MapName}");
        }
        else
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected {executorName} nominated {@event.MapConfig.MapName}");
        }
    }
}