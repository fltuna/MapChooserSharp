using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.VoteMenu.BuiltInHtml;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu;

public sealed class McsMapVoteMenuProvider(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsMapVoteMenuProvider
{
    public override string PluginModuleName => "McsMapVoteMenuProvider";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    private IMcsPluginConfigProvider _pluginConfigProvider = null!;
    
    private readonly Dictionary<int, McsSupportedMenuType> _playerVoteMenuTypes = new();
    private readonly Dictionary<McsSupportedMenuType, IMcsMapVoteUiFactory> _uiFactories = new();

    private const McsSupportedMenuType FallBackMenuType = McsSupportedMenuType.BuiltInHtml;
    
    public IMcsMapVoteUserInterface CreateNewVoteUi(CCSPlayerController player)
    {
        // Fallback if player's menu type is not set
        if (!_playerVoteMenuTypes.TryGetValue(player.Slot, out var playerMenuType))
            return _uiFactories[FallBackMenuType].Create(player);


        // Fallback if player's specified menu type is not avaiable in server
        if (!_pluginConfigProvider.PluginConfig.VoteConfig.AvailableMenuTypes.Contains(playerMenuType))
        {
            return _uiFactories[FallBackMenuType].Create(player);
        }

        // Fallback if player's specified menu type is not initialized or avaialbe in provider
        if (!_uiFactories.TryGetValue(playerMenuType, out var uiFactory))
        {
            return _uiFactories[FallBackMenuType].Create(player);
        }

        return uiFactory.Create(player);
    }
    
    
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsMapVoteMenuProvider>(this);
    }

    protected override void OnInitialize()
    {
        _pluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();

        Plugin.RegisterListener<Listeners.OnClientConnected>(OnClientConnected);

        if (hotReload)
        {
            foreach (CCSPlayerController controller in Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }))
            {
                OnClientConnected(controller.Slot);
            }
        }

        InitializeSupportedMenus();
    }

    private void OnClientConnected(int slot)
    {
        // TODO() save menu type to DB and restore.
        
        // fallback to server's settings if failed to obtain player data from DB 
        _playerVoteMenuTypes[slot] = _pluginConfigProvider.PluginConfig.VoteConfig.CurrentMenuType;
    }


    private void InitializeSupportedMenus()
    {
        foreach (McsSupportedMenuType type in _pluginConfigProvider.PluginConfig.VoteConfig.AvailableMenuTypes)
        {
            switch (type)
            {
                case McsSupportedMenuType.BuiltInHtml:
                    _uiFactories[type] = new McsBuiltInHtmlVoteUiFactory(ServiceProvider);
                    break;
            }
        }
    }
}