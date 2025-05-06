using Dapper;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Entities;
using MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MapChooserSharp.Modules.McsDatabase.Repositories;

public sealed class McsGroupInformationRepository
    : McsDatabaseRepositoryBase, IMcsGroupInformationRepository
{
    
    private readonly IMcsSqlQueryProvider _sqlQueryProvider;
    
    private readonly IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi;

    private readonly string _connectionString;

    public McsGroupInformationRepository(string connectionString, string providerName, IServiceProvider provider): base(provider)
    {
        _mcsInternalMapConfigProviderApi = provider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _sqlQueryProvider = CreateSqlProvider(providerName);
        _connectionString = connectionString;
        
        EnsureTableExists();
        CollectAllGroupCooldownsAsync().ConfigureAwait(false);
    }

    private void EnsureTableExists()
    {
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            connection.Execute(_sqlQueryProvider.GroupSqlQueries().GetEnsureTableExistsSql());
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Failed to create McsGroupInformation table");
            throw;
        }
        Plugin.Logger.LogInformation("McsGroupInformation table ensured");
    }

    /// <summary>
    /// Upserts a group's cooldown value
    /// </summary>
    /// <param name="groupName">Group name</param>
    /// <param name="cooldownValue">New cooldown value</param>
    public async Task UpsertGroupCooldownAsync(string groupName, int cooldownValue)
    {
        try
        {
            Plugin.Logger.LogInformation($"Updating cooldown information for group {groupName} to {cooldownValue}");
            
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(
                _sqlQueryProvider.GroupSqlQueries().GetUpsertGroupCooldownSql(),
                new { GroupName = groupName, CooldownValue = cooldownValue }
            );
            
            Plugin.Logger.LogInformation($"Successfully updated cooldown for group {groupName}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error updating group cooldown for {groupName}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Decrements all group cooldowns where the cooldown is greater than zero
    /// </summary>
    public async Task DecrementAllCooldownsAsync()
    {
        try
        {
            Plugin.Logger.LogInformation("Decrementing cooldowns information for all groups");
            
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(_sqlQueryProvider.GroupSqlQueries().GetDecrementCooldownsSql());
            
            Plugin.Logger.LogInformation("Decremented cooldowns information for all groups");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error decrementing group cooldowns: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Ensures all groups in the configuration have corresponding entries in the database
    /// </summary>
    public async Task EnsureAllGroupInfoExistsAsync()
    {
        Plugin.Logger.LogInformation("Start creating missing group information");
        
        var mapConfigProvider = _mcsInternalMapConfigProviderApi;
        var groupSettings = mapConfigProvider.GetGroupSettings();
        int newGroupsCount = 0;
        
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var allGroupInfos = await connection.QueryAsync<McsGroupInformation>(
                    _sqlQueryProvider.GroupSqlQueries().GetAllGroupInfosSql(),
                    transaction: transaction
                );
                
                // Convert to hashset for faster searching
                var existingGroupNames = allGroupInfos.Select(g => g.GroupName).ToHashSet();
                
                // Get insert SQL
                string insertSql = _sqlQueryProvider.GroupSqlQueries().GetInsertGroupInfoSql();
                
                // Add groups that are in the configuration but not in the database
                foreach (var (groupName, _) in groupSettings)
                {
                    if (existingGroupNames.Contains(groupName))
                        continue;
                    
                    // If group not found in database, create a new entry
                    await connection.ExecuteAsync(
                        insertSql,
                        new { GroupName = groupName, CooldownRemains = 0 },
                        transaction: transaction
                    );
                    
                    newGroupsCount++;
                }
                
                // Commit the transaction
                transaction.Commit();
            }
            catch (Exception)
            {
                // Rollback on error
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Error creating new group information");
            throw;
        }
        
        Plugin.Logger.LogInformation($"Created new group information for {newGroupsCount} groups");
    }
    /// <summary>
    /// Collects cooldown information for all groups and updates the in-memory configuration
    /// </summary>
    public async Task CollectAllGroupCooldownsAsync()
    {
        Plugin.Logger.LogInformation($"Start collecting all group cooldowns");
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            // Retrieve all group information from the database
            var groupInfos = await connection.QueryAsync<McsGroupInformation>(
                _sqlQueryProvider.GroupSqlQueries().GetAllGroupInfosSql()
            );
            
            // Get the group configuration provider
            var groupConfigs = _mcsInternalMapConfigProviderApi.GetGroupSettings();
            
            // Update in-memory group cooldowns from the database
            int groupsCount = 0;
            foreach (var (groupName, groupConfig) in groupConfigs)
            {
                var groupInfo = groupInfos.FirstOrDefault(g => g.GroupName == groupName);
                
                if (groupInfo != null)
                {
                    groupConfig.GroupCooldown.CurrentCooldown = groupInfo.CooldownRemains;
                    Plugin.Logger.LogDebug($"Updated in-memory cooldown for group {groupName} to {groupInfo.CooldownRemains}");
                }
                else
                {
                    groupConfig.GroupCooldown.CurrentCooldown = 0;
                    Plugin.Logger.LogDebug($"Group {groupName} not found in database, setting cooldown to 0");
                }
                groupsCount++;
            }
            
            Plugin.Logger.LogInformation($"Successfully collected cooldowns from database for {groupsCount} groups");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Error collecting group cooldowns");
            throw;
        }
    }
}