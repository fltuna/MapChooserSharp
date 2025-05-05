using Dapper;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Entities;
using MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MapChooserSharp.Modules.McsDatabase.Repositories;

public class McsMapInformationRepository
    : McsDatabaseRepositoryBase, IMcsMapInformationRepository
{
    
    private readonly IMcsSqlQueryProvider _sqlQueryProvider;
    
    private readonly IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi;

    private readonly string _connectionString;

    public McsMapInformationRepository(string connectionString, string providerName, IServiceProvider provider): base(provider)
    {
        _mcsInternalMapConfigProviderApi = provider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _sqlQueryProvider = CreateSqlProvider(providerName);
        _connectionString = connectionString;
        
        EnsureTableExists();
    }

    private void EnsureTableExists()
    {
        try
        {
            using var connection =  _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            connection.Execute(_sqlQueryProvider.MapInfoSqlQueries().GetEnsureTableExistsSql());
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Failed to create database table");
            throw;
        }
        Plugin.Logger.LogInformation("McsMapInformation table ensured");
    }

    public async Task UpsertMapCooldownAsync(string mapName, int cooldownValue)
    {
        Logger.LogInformation($"Updating cooldown information for map {mapName} to {cooldownValue}");
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(
                _sqlQueryProvider.MapInfoSqlQueries().GetUpsertMapInfoSql(), 
                new { MapName = mapName, CooldownRemains = cooldownValue }
            );
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Failed to upsert map info for {mapName}");
            throw;
        }
        Logger.LogInformation($"Updated cooldown information for map {mapName} to {cooldownValue}");
    }
    
    public async Task DecrementAllCooldownsAsync()
    {
        Logger.LogInformation("Decrementing cooldowns information for all maps");
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(_sqlQueryProvider.MapInfoSqlQueries().GetDecrementCooldownsSql());
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Failed to decrement cooldowns");
            throw;
        }
        Logger.LogInformation("Decremented cooldowns information for all maps");
    }
    
    public async Task EnsureAllMapInfoExistsAsync()
    {
        Plugin.Logger.LogInformation("Start creating missing map information");
        
        var mapConfigs = _mcsInternalMapConfigProviderApi.GetMapConfigs();
        int newMapsCount = 0;
        
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var allMapInfos = await connection.QueryAsync<McsMapInformation>(
                    _sqlQueryProvider.MapInfoSqlQueries().GetAllMapInfosSql(),
                    transaction: transaction
                );
                
                // Convert to hashset for faster searching
                var existingMapNames = allMapInfos.Select(m => m.MapName).ToHashSet();
                
                string insertSql = _sqlQueryProvider.MapInfoSqlQueries().GetInsertMapInfoSql();
                
                // Add maps that are in the configuration but not in the database
                foreach (var (mapName, _) in mapConfigs)
                {
                    if (existingMapNames.Contains(mapName))
                        continue;
                    
                    await connection.ExecuteAsync(
                        insertSql,
                        new { MapName = mapName, CooldownRemains = 0 },
                        transaction: transaction
                    );
                    
                    newMapsCount++;
                }
                
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Error creating new map information");
        }
        
        Plugin.Logger.LogInformation($"Created new map information for {newMapsCount} maps");
    }
    
    public async Task CollectAllCooldownsAsync()
    {
        Plugin.Logger.LogInformation("Start collecting map cooldown information");
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            var mapInfos = await connection.QueryAsync<McsMapInformation>(
                _sqlQueryProvider.MapInfoSqlQueries().GetAllMapInfosSql()
            );
            
            var mapConfigProvider = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
            var mapConfigs = mapConfigProvider.GetMapConfigs();

            int mapCount = 0;
            foreach (var (mapName, mapConfig) in mapConfigs)
            {
                var mapInfo = mapInfos.FirstOrDefault(m => m.MapName == mapName);
                
                if (mapInfo != null)
                {
                    mapConfig.MapCooldown.CurrentCooldown = mapInfo.CooldownRemains;
                }
                else
                {
                    mapConfig.MapCooldown.CurrentCooldown = 0;
                }

                mapCount++;
            }
            Plugin.Logger.LogInformation($"Successfully collected cooldowns from database for {mapCount} maps");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Failed to collect map cooldowns");
            throw;
        }
    }
}