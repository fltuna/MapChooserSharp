using Dapper;
using MapChooserSharp.Modules.McsDatabase.Entities;
using MapChooserSharp.Modules.McsDatabase.Repositories.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;
using Microsoft.Extensions.Logging;

namespace MapChooserSharp.Modules.McsDatabase.Repositories;

public sealed class McsUserInformationRepository
    : McsDatabaseRepositoryBase, IMcsUserInformationRepository
{
    private readonly IMcsSqlQueryProvider _sqlQueryProvider;
    private readonly string _connectionString;

    public McsUserInformationRepository(string connectionString, McsSupportedSqlType providerType, string tableName, IServiceProvider provider) 
        : base(provider, tableName)
    {
        _sqlQueryProvider = CreateSqlProvider(providerType);
        _connectionString = connectionString;
        
        EnsureTableExists();
    }

    private void EnsureTableExists()
    {
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            connection.Execute(_sqlQueryProvider.UserInfoSqlQueries().GetEnsureTableExistsSql());
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, "Failed to create McsUserInformation table");
            throw;
        }
        Plugin.Logger.LogInformation("McsUserInformation table ensured");
    }

    public async Task UpsertUserInformationAsync(long steamId, McsUserInformation userInformation)
    {
        try
        {
            Logger.LogInformation($"Upserting user information for SteamId {steamId}");
            
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(
                _sqlQueryProvider.UserInfoSqlQueries().GetUpsertUserInfoSql(),
                new 
                { 
                    SteamId = steamId, 
                    userInformation.SessionTime,
                    userInformation.LastLoggedInAt,
                    userInformation.UserSessionStartedAt
                }
            );
            
            Logger.LogInformation($"Successfully upserted user information for SteamId {steamId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error upserting user information for SteamId {steamId}: {ex.Message}");
            throw;
        }
    }

    public async Task IncrementUserSessionTimeAsync(long steamId)
    {
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            var now = DateTime.UtcNow;
            
            // First try to increment existing record
            var rowsAffected = await connection.ExecuteAsync(
                _sqlQueryProvider.UserInfoSqlQueries().GetIncrementSessionTimeSql(),
                new { SteamId = steamId, LastLoggedInAt = now }
            );
            
            // If no rows were affected, the user doesn't exist yet, so insert a new record
            if (rowsAffected == 0)
            {
                await connection.ExecuteAsync(
                    _sqlQueryProvider.UserInfoSqlQueries().GetUpsertUserInfoSql(),
                    new 
                    { 
                        SteamId = steamId, 
                        SessionTime = 1,
                        LastLoggedInAt = now,
                        UserSessionStartedAt = now
                    }
                );
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error incrementing session time for SteamId {steamId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Increments user session time with automatic reset check based on configured reset time.
    /// If the current time has passed the reset time since UserSessionStartedAt, the session is reset.
    /// </summary>
    /// <param name="steamId">Steam ID</param>
    /// <param name="resetTimeOfDay">Time of day when sessions should reset (e.g., 04:00 for 4 AM)</param>
    public async Task IncrementUserSessionTimeWithResetCheckAsync(long steamId, TimeSpan resetTimeOfDay)
    {
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            var now = DateTime.UtcNow;
            
            // Get existing user information
            var userInfo = await connection.QueryFirstOrDefaultAsync<McsUserInformation>(
                _sqlQueryProvider.UserInfoSqlQueries().GetUserInfoBySteamIdSql(),
                new { SteamId = steamId }
            );
            
            if (userInfo == null)
            {
                // User doesn't exist, create new record
                await connection.ExecuteAsync(
                    _sqlQueryProvider.UserInfoSqlQueries().GetUpsertUserInfoSql(),
                    new 
                    { 
                        SteamId = steamId, 
                        SessionTime = 1,
                        LastLoggedInAt = now,
                        UserSessionStartedAt = now
                    }
                );
                return;
            }
            
            // Check if we need to reset the session
            bool shouldReset = ShouldResetSession(userInfo.UserSessionStartedAt, now, resetTimeOfDay);
            
            if (shouldReset)
            {
                Logger.LogInformation(
                    $"Resetting session for SteamId {steamId}. Started at: {userInfo.UserSessionStartedAt:yyyy-MM-dd HH:mm:ss}, " +
                    $"Current time: {now:yyyy-MM-dd HH:mm:ss}, Reset time: {resetTimeOfDay}"
                );
                
                // Reset session time and update start time
                await connection.ExecuteAsync(
                    _sqlQueryProvider.UserInfoSqlQueries().GetResetSessionTimeWithStartTimeSql(),
                    new { SteamId = steamId, UserSessionStartedAt = now }
                );
                
                // Increment to 1 (since this is a new session)
                await connection.ExecuteAsync(
                    _sqlQueryProvider.UserInfoSqlQueries().GetIncrementSessionTimeSql(),
                    new { SteamId = steamId, LastLoggedInAt = now }
                );
            }
            else
            {
                // Just increment normally
                await connection.ExecuteAsync(
                    _sqlQueryProvider.UserInfoSqlQueries().GetIncrementSessionTimeSql(),
                    new { SteamId = steamId, LastLoggedInAt = now }
                );
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error incrementing session time with reset check for SteamId {steamId}: {ex.Message}");
            throw;
        }
    }

    public async Task ResetUserSessionTimeAsync(long steamId)
    {
        try
        {
            Logger.LogInformation($"Resetting session time for SteamId {steamId}");
            
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            await connection.ExecuteAsync(
                _sqlQueryProvider.UserInfoSqlQueries().GetResetSessionTimeSql(),
                new { SteamId = steamId }
            );
            
            Logger.LogInformation($"Successfully reset session time for SteamId {steamId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error resetting session time for SteamId {steamId}: {ex.Message}");
            throw;
        }
    }

    public async Task<McsUserInformation?> GetUserInformationAsync(long steamId)
    {
        try
        {
            using var connection = _sqlQueryProvider.CreateConnection(_connectionString);
            connection.Open();
            
            var userInfo = await connection.QueryFirstOrDefaultAsync<McsUserInformation>(
                _sqlQueryProvider.UserInfoSqlQueries().GetUserInfoBySteamIdSql(),
                new { SteamId = steamId }
            );
            
            return userInfo;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex, $"Error getting user information for SteamId {steamId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Determines if a session should be reset based on the reset time of day.
    /// Returns true if the reset time has been crossed since the session started.
    /// </summary>
    /// <param name="sessionStartedAt">When the session started</param>
    /// <param name="currentTime">Current time</param>
    /// <param name="resetTimeOfDay">Time of day when reset should occur</param>
    /// <returns>True if session should be reset</returns>
    private bool ShouldResetSession(DateTime sessionStartedAt, DateTime currentTime, TimeSpan resetTimeOfDay)
    {
        // Calculate the reset datetime for the session start date
        var resetDateTimeOnSessionStartDate = sessionStartedAt.Date + resetTimeOfDay;

        // If session started before reset time on that day, the next reset is on the same day
        // If session started after reset time on that day, the next reset is on the next day
        var nextResetTime = sessionStartedAt <= resetDateTimeOnSessionStartDate
            ? resetDateTimeOnSessionStartDate
            : resetDateTimeOnSessionStartDate.AddDays(1);
        
        // Check if current time has passed the next reset time
        return currentTime >= nextResetTime;
    }
}