using System.Runtime.InteropServices;
using System.Security;
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
    private McsSupportedSqlType _providerType = McsSupportedSqlType.Sqlite;
    
    
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
        
        MapInfoRepository = new McsMapInformationRepository(_connectionString, _providerType, _pluginConfigProvider.PluginConfig.GeneralConfig.SqlConfig.MapSettingsSqlTableName, ServiceProvider);
        GroupInfoRepository = new McsGroupInformationRepository(_connectionString, _providerType, _pluginConfigProvider.PluginConfig.GeneralConfig.SqlConfig.GroupSettingsSqlTableName, ServiceProvider);
        
        MapInfoRepository.EnsureAllMapInfoExistsAsync().ConfigureAwait(false);
        GroupInfoRepository.EnsureAllGroupInfoExistsAsync().ConfigureAwait(false);
    }
    
    private void ConfigureDatabase()
    {
        var config = _pluginConfigProvider.PluginConfig;
        
        switch (config.GeneralConfig.SqlConfig.DataBaseType)
        {
            case McsSupportedSqlType.MySql:
                _providerType = McsSupportedSqlType.MySql;    
                string passwordMySql = ConvertSecureStringToString(config.GeneralConfig.SqlConfig.Password);

                try
                {
                    _connectionString = $"Server={config.GeneralConfig.SqlConfig.Host};" +
                                        $"Database={config.GeneralConfig.SqlConfig.DatabaseName};" +
                                        $"User Id={config.GeneralConfig.SqlConfig.UserName};" +
                                        $"Password={passwordMySql};" +
                                        $"Port={config.GeneralConfig.SqlConfig.Port};";
                    
                    Plugin.Logger.LogInformation($"Using MySQL database at host: {config.GeneralConfig.SqlConfig.Host}, Database Name: {config.GeneralConfig.SqlConfig.DatabaseName}");
                }
                finally
                {
                    // Clear password from memory ASAP
                    SecurelyEraseString(ref passwordMySql);
                }
                
                break;
                
            
            case McsSupportedSqlType.PostgreSql:
                _providerType = McsSupportedSqlType.PostgreSql;
                string passwordPostgreSql = ConvertSecureStringToString(config.GeneralConfig.SqlConfig.Password);
    
                try
                {
                    _connectionString = $"Host={config.GeneralConfig.SqlConfig.Host};" +
                                        $"Database={config.GeneralConfig.SqlConfig.DatabaseName};" +
                                        $"Username={config.GeneralConfig.SqlConfig.UserName};" +
                                        $"Password={passwordPostgreSql};" +
                                        $"Port={config.GeneralConfig.SqlConfig.Port};";
        
                    Plugin.Logger.LogInformation($"Using PostgreSQL database at {config.GeneralConfig.SqlConfig.Host}");
                }
                finally
                {
                    SecurelyEraseString(ref passwordPostgreSql);
                }
                
                break;
                
            
            case McsSupportedSqlType.Sqlite:
            default:
                _providerType = McsSupportedSqlType.Sqlite;
                string dbPath = Path.Combine(Plugin.ModuleDirectory, config.GeneralConfig.SqlConfig.DatabaseName);
                _connectionString = $"Data Source={dbPath}";
                Plugin.Logger.LogInformation($"Using SQLite database at {dbPath}");
                break;
        }
    }
    
    private string ConvertSecureStringToString(SecureString secureString)
    {
        IntPtr bstrPtr = Marshal.SecureStringToBSTR(secureString);
        try
        {
            return Marshal.PtrToStringBSTR(bstrPtr);
        }
        finally
        {
            Marshal.ZeroFreeBSTR(bstrPtr);
        }
    }
    
    private void SecurelyEraseString(ref string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        
        GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
    
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            int size = value.Length * sizeof(char);
        
            Marshal.Copy(new byte[size], 0, ptr, size);
        }
        finally
        {
            handle.Free();
        
            value = "";
        }
    }
}