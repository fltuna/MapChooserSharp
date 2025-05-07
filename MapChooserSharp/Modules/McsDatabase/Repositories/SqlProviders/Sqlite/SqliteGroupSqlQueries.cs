using MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Interfaces;

namespace MapChooserSharp.Modules.McsDatabase.Repositories.SqlProviders.Sqlite;

internal sealed class SqliteGroupSqlQueries(string tableName) : IMcsGroupSqlQueries
{
    public string TableName { get; } = tableName;

    public string GetEnsureTableExistsSql() => @$"
        CREATE TABLE IF NOT EXISTS {TableName} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            GroupName TEXT NOT NULL UNIQUE,
            CooldownRemains INTEGER NOT NULL
        )";
    
    public string GetDecrementCooldownsSql() => 
        $"UPDATE {TableName} SET CooldownRemains = CooldownRemains - 1 WHERE CooldownRemains > 0";
    
    public string GetUpsertGroupCooldownSql() => @$"
        INSERT INTO {TableName} (GroupName, CooldownRemains) 
        VALUES (@GroupName, @CooldownValue)
        ON CONFLICT(GroupName) DO UPDATE SET 
        CooldownRemains = @CooldownValue";
    
    public string GetAllGroupInfosSql() => 
        $"SELECT * FROM {TableName}";
    
    public string GetInsertGroupInfoSql() => 
        $"INSERT INTO {TableName} (GroupName, CooldownRemains) VALUES (@GroupName, @CooldownRemains)";
}