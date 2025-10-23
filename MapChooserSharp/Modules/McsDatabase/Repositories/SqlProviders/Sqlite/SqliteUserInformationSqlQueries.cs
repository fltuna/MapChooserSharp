using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

internal sealed class SqliteUserInformationSqlQueries(string tableName) : IMcsUserInformationSqlQueries
{
    public string TableName { get; } = tableName;

    public string GetEnsureTableExistsSql() => @$"
        CREATE TABLE IF NOT EXISTS {TableName} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SteamId INTEGER NOT NULL UNIQUE,
            SessionTime INTEGER NOT NULL DEFAULT 0,
            LastLoggedInAt TEXT NOT NULL,
            UserSessionStartedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        )";
    
    public string GetUpsertUserInfoSql() => @$"
        INSERT INTO {TableName} (SteamId, SessionTime, LastLoggedInAt, UserSessionStartedAt) 
        VALUES (@SteamId, @SessionTime, @LastLoggedInAt, @UserSessionStartedAt)
        ON CONFLICT(SteamId) DO UPDATE SET 
        SessionTime = @SessionTime,
        LastLoggedInAt = @LastLoggedInAt,
        UserSessionStartedAt = @UserSessionStartedAt";
    
    public string GetIncrementSessionTimeSql() => 
        $"UPDATE {TableName} SET SessionTime = SessionTime + 1, LastLoggedInAt = @LastLoggedInAt WHERE SteamId = @SteamId";
    
    public string GetResetSessionTimeSql() => 
        $"UPDATE {TableName} SET SessionTime = 0 WHERE SteamId = @SteamId";
    
    public string GetResetSessionTimeWithStartTimeSql() => 
        $"UPDATE {TableName} SET SessionTime = 0, UserSessionStartedAt = @UserSessionStartedAt WHERE SteamId = @SteamId";
    
    public string GetUserInfoBySteamIdSql() => 
        $"SELECT * FROM {TableName} WHERE SteamId = @SteamId";
}

