using MapChooserSharp.Modules.McsDatabase.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories;
using MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.McsDatabase;

internal sealed class McsDatabaseProvider(IServiceProvider serviceProvider)
    : PluginModuleBase(serviceProvider), IMcsDatabaseProvider
{
    public override string PluginModuleName => "McsDatabaseProvider";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    
    private IMcsPluginConfigProvider _pluginConfigProvider = null!;
    private string _connectionString = null!;
    private string _providerName = null!;
    
    
    public IMcsMapInformationRepository MapInfoRepository { get; private set; } = null!;
    
    public McsGroupInformationRepository GroupInfoRepository { get; private set; } = null!;

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsDatabaseProvider>(this);
    }
    
    protected override void OnInitialize()
    {
        _pluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        ConfigureDatabase();
        
        MapInfoRepository = new McsMapInformationRepository(_connectionString, _providerName, ServiceProvider);
        GroupInfoRepository = new McsGroupInformationRepository(_connectionString, _providerName, ServiceProvider);
        
        MapInfoRepository.EnsureAllMapInfoExistsAsync().ConfigureAwait(false);
        GroupInfoRepository.EnsureAllGroupInfoExistsAsync().ConfigureAwait(false);
    }
    
    private void ConfigureDatabase()
    {
        var config = _pluginConfigProvider.PluginConfig;
        
        // Todo database type from config
        switch ("sqlite")
        {
            // case "mysql":
            //     _providerName = "mysql";
            //     _connectionString = $"Server={config.MySqlHost};Port={config.MySqlPort};Database={config.MySqlDatabase};User={config.MySqlUser};Password={config.MySqlPassword};";
            //     Plugin.Logger.LogInformation("Using MySQL database");
            //     break;
            //     
            // case "postgresql":
            //     _providerName = "postgresql";
            //     _connectionString = $"Host={config.PostgresHost};Port={config.PostgresPort};Database={config.PostgresDatabase};Username={config.PostgresUser};Password={config.PostgresPassword};";
            //     Plugin.Logger.LogInformation("Using PostgreSQL database");
            //     break;
                
            case "sqlite":
            default:
                _providerName = "sqlite";
                string dbPath = Path.Combine(Plugin.ModuleDirectory, "MapChooserSharp.db");
                _connectionString = $"Data Source={dbPath}";
                Plugin.Logger.LogInformation($"Using SQLite database at {dbPath}");
                break;
        }
    }
}